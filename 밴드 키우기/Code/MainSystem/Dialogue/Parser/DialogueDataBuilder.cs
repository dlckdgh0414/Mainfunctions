using System;
using System.Collections.Generic;
using Member.LS.Code.Dialogue;
using Member.LS.Code.Dialogue.Character;
using UnityEngine;

namespace Code.MainSystem.Dialogue.Parser
{
    /// <summary>
    /// 파싱된 문자열 행렬을 실제 DialogueNode 리스트로 변환하는 클래스
    /// </summary>
    public static class DialogueDataBuilder
    {
        /// <summary>
        /// 원시 문자열 데이터를 DialogueNode 리스트로 빌드
        /// </summary>
        /// <param name="rows">파싱할 CSV의 행별 문자열 배열 데이터</param>
        /// <param name="sourceName">로그 표시용 CSV 소스 이름</param>
        public static List<DialogueNode> Build(List<string[]> rows, string sourceName = "CSV")
        {
            List<DialogueNode> nodes = new List<DialogueNode>();
            HashSet<string> nodeIDs = new HashSet<string>();
            Dictionary<string, int> nodeToRowIndex = new Dictionary<string, int>();

            // 1단계: 기본 데이터 파싱 및 ID 수집
            for (int rowIndex = 0; rowIndex < rows.Count; rowIndex++)
            {
                string[] row = rows[rowIndex];
                if (row.Length < 6) continue;

                DialogueNode node = new DialogueNode
                {
                    NodeID = row[0],
                    CharacterID = row[1],
                    DialogueDetail = row[5],
                    BackgroundID = row[4]
                };

                // Enum 파싱: CharacterEmotionType
                if (Enum.TryParse(row[2], out CharacterEmotionType emotion))
                {
                    node.CharacterEmotion = emotion;
                }
                else
                {
                    Debug.LogError($"[DialogueDataBuilder] Invalid Emotion '{row[2]}' at Node {node.NodeID}");
                }

                // Enum 파싱: NameTagPositionType
                if (Enum.TryParse(row[3], out NameTagPositionType nameTag))
                {
                    node.NameTagPosition = nameTag;
                }
                else
                {
                    Debug.LogError($"[DialogueDataBuilder] Invalid NameTag '{row[3]}' at Node {node.NodeID}");
                }

                // NextNodeID 파싱 (새로 추가된 컬럼)
                if (row.Length > 6)
                {
                    node.NextNodeID = string.IsNullOrWhiteSpace(row[6]) ? string.Empty : row[6].Trim();
                }

                // 선택지 파싱 (인덱스 밀림)
                if (row.Length > 7)
                {
                    node.Choices = ParseChoices(row[7]);
                }

                // 명령어 파싱 (인덱스 밀림)
                if (row.Length > 8)
                {
                    node.Commands = DialogueCommandFactory.ParseBatch(row[8]);
                }

                // 보이스 파싱 (인덱스 밀림)
                if (row.Length > 9)
                {
                    node.VoiceID = row[9];
                }

                nodes.Add(node);
                nodeIDs.Add(node.NodeID);

                if (!string.IsNullOrWhiteSpace(node.NodeID) && !nodeToRowIndex.ContainsKey(node.NodeID))
                {
                    // CSVReader는 헤더를 제외해서 반환하므로 +2가 원본 파일의 대략적 라인 번호
                    nodeToRowIndex[node.NodeID] = rowIndex + 2;
                }
            }

            // 2단계: 참조 무결성 검사 (NextNodeID 및 선택지의 NextNodeID가 존재하는지 확인)
            ValidateIntegrity(nodes, nodeIDs, nodeToRowIndex, sourceName);

            return nodes;
        }

        /// <summary>
        /// 선택지 문자열(NextNodeID:Text[:ConditionA&ConditionB]|...) 파싱
        /// </summary>
        /// <param name="raw">파싱할 원시 문자열</param>
        private static List<DialogueChoice> ParseChoices(string raw)
        {
            List<DialogueChoice> choices = new List<DialogueChoice>();
            if (string.IsNullOrWhiteSpace(raw)) return choices;

            List<string> tokens = DialogueChoiceTokenUtility.SplitChoiceTokens(raw);
            foreach (string token in tokens)
            {
                if (DialogueChoiceTokenUtility.TryParseChoiceToken(
                        token,
                        out string nextNodeID,
                        out string choiceText,
                        out string rawConditions,
                        out string subText,
                        out string lockedSubText))
                {
                    DialogueChoice choice = new DialogueChoice
                    {
                        NextNodeID = nextNodeID,
                        ChoiceText = choiceText,
                        SubText = subText,
                        LockedSubText = lockedSubText,
                        Commands = new List<IDialogueCommand>(),
                        Conditions = new List<IDialogueCondition>()
                    };

                    if (!string.IsNullOrWhiteSpace(rawConditions))
                    {
                        choice.Conditions = DialogueConditionFactory.ParseBatch(rawConditions);
                    }

                    choices.Add(choice);
                }
                else
                {
                    Debug.LogWarning($"[DialogueDataBuilder] Invalid choice token skipped: {token}");
                }
            }
            return choices;
        }

        /// <summary>
        /// 데이터의 참조 무결성을 검증
        /// </summary>
        /// <param name="nodes">검증할 노드 리스트</param>
        /// <param name="nodeIDs">유효한 노드 ID 집합</param>
        /// <param name="nodeToRowIndex">NodeID -> CSV 라인 번호 매핑</param>
        /// <param name="sourceName">로그 표시용 CSV 소스 이름</param>
        private static void ValidateIntegrity(
            List<DialogueNode> nodes,
            HashSet<string> nodeIDs,
            IReadOnlyDictionary<string, int> nodeToRowIndex,
            string sourceName)
        {
            foreach (DialogueNode node in nodes)
            {
                if (!string.IsNullOrEmpty(node.NextNodeID) && !nodeIDs.Contains(node.NextNodeID))
                {
                    int line = GetCsvLine(nodeToRowIndex, node.NodeID);
                    Debug.LogError(
                        $"[Validator] Reference Error in '{sourceName}' (line ~{line}): " +
                        $"Node '{node.NodeID}' NextNodeID points to non-existent Node '{node.NextNodeID}'.");
                }

                if (node.Choices == null) continue;

                foreach (DialogueChoice choice in node.Choices)
                {
                    if (!string.IsNullOrEmpty(choice.NextNodeID) && !nodeIDs.Contains(choice.NextNodeID))
                    {
                        int line = GetCsvLine(nodeToRowIndex, node.NodeID);
                        Debug.LogError(
                            $"[Validator] Reference Error in '{sourceName}' (line ~{line}): " +
                            $"Node '{node.NodeID}' Choice points to non-existent Node '{choice.NextNodeID}'.");
                    }
                }
            }
        }

        private static int GetCsvLine(IReadOnlyDictionary<string, int> nodeToRowIndex, string nodeId)
        {
            if (nodeToRowIndex != null && !string.IsNullOrWhiteSpace(nodeId) && nodeToRowIndex.TryGetValue(nodeId, out int line))
            {
                return line;
            }

            return -1;
        }
    }
}
