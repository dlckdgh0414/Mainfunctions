using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Code.MainSystem.Dialogue.Parser.Editor
{
    /// <summary>
    /// CSV 파일을 선택하여 DialogueInformationSO에 임포트하거나 내보내는 에디터 유틸리티 클래스
    /// </summary>
    public static class DialogueCSVImporter
    {
        /// <summary>
        /// 파일 탐색기를 열어 CSV를 선택하고 데이터를 SO에 덮어씌움
        /// </summary>
        public static void ImportCSV(DialogueInformationSO targetSO)
        {
            if (targetSO == null)
            {
                Debug.LogError("[DialogueCSVImporter] Target SO is null!");
                return;
            }

            string path = EditorUtility.OpenFilePanel("CSV 다이알로그 임포트", "", "csv");
            if (string.IsNullOrEmpty(path)) return;

            ImportCSVFromPath(targetSO, path, true, true);
        }

        /// <summary>
        /// 지정된 CSV 파일 경로를 읽어 SO에 임포트.
        /// </summary>
        /// <param name="targetSO">대상 SO</param>
        /// <param name="path">CSV 절대 경로</param>
        /// <param name="showPopup">완료/실패 팝업 표시 여부</param>
        /// <param name="saveAssets">AssetDatabase 저장/새로고침 수행 여부</param>
        /// <returns>임포트 성공 여부</returns>
        public static bool ImportCSVFromPath(
            DialogueInformationSO targetSO,
            string path,
            bool showPopup = true,
            bool saveAssets = true,
            bool persistAutoCorrectionsToCsv = false)
        {
            if (targetSO == null)
            {
                Debug.LogError("[DialogueCSVImporter] Target SO is null!");
                return false;
            }

            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                string failMessage = $"CSV path is invalid: '{path}'";
                Debug.LogError($"[DialogueCSVImporter] {failMessage}");
                if (showPopup)
                {
                    EditorUtility.DisplayDialog("임포트 실패", failMessage, "확인");
                }
                return false;
            }

            try
            {
                string csvText = File.ReadAllText(path);

                if (persistAutoCorrectionsToCsv
                    && TryBuildNormalizedCsvText(csvText, out string normalizedCsvText, out int correctedCount)
                    && correctedCount > 0
                    && !string.Equals(normalizedCsvText, csvText, System.StringComparison.Ordinal))
                {
                    File.WriteAllText(path, normalizedCsvText, new UTF8Encoding(true));
                    Debug.LogWarning($"[DialogueCSVImporter] Persisted {correctedCount} auto-correction(s) to '{path}'.");
                    csvText = normalizedCsvText;
                }

                return ImportCSVFromText(targetSO, csvText, Path.GetFileName(path), showPopup, saveAssets);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[DialogueCSVImporter] Import failed: {e.Message}");
                if (showPopup)
                {
                    EditorUtility.DisplayDialog("임포트 실패", $"오류가 발생했습니다: {e.Message}", "확인");
                }
                return false;
            }
        }

        private static bool TryBuildNormalizedCsvText(string sourceCsvText, out string normalizedCsvText, out int correctedCount)
        {
            normalizedCsvText = sourceCsvText;
            correctedCount = 0;

            if (string.IsNullOrEmpty(sourceCsvText))
            {
                return false;
            }

            List<string[]> rows = DialogueCSVReader.Parse(sourceCsvText);
            DialogueCSVAutoCorrector.Report report = DialogueCSVAutoCorrector.NormalizeAndValidate(rows);

            if (report.INVALID_COMMANDS.Count > 0 || report.INVALID_CONDITIONS.Count > 0)
            {
                return false;
            }

            correctedCount = report.correctedCommandCount;
            if (correctedCount <= 0)
            {
                return true;
            }

            string header = GetHeaderLine(sourceCsvText);
            if (string.IsNullOrEmpty(header))
            {
                return false;
            }

            normalizedCsvText = BuildCsvText(header, rows);
            return true;
        }

        private static string GetHeaderLine(string csvText)
        {
            string[] lines = csvText.Split(new[] { "\r\n", "\r", "\n" }, System.StringSplitOptions.None);
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                return line;
            }

            return string.Empty;
        }

        private static string BuildCsvText(string header, List<string[]> rows)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(header);

            foreach (string[] row in rows)
            {
                builder.Append("\r\n");
                builder.Append(SerializeCsvRow(row));
            }

            return builder.ToString();
        }

        private static string SerializeCsvRow(string[] row)
        {
            if (row == null || row.Length == 0)
            {
                return string.Empty;
            }

            return string.Join(",", row.Select(EscapeCsvField));
        }

        private static string EscapeCsvField(string field)
        {
            if (field == null)
            {
                return string.Empty;
            }

            bool mustQuote = field.Contains(",") || field.Contains("\"") || field.Contains("\n") || field.Contains("\r");
            if (!mustQuote)
            {
                return field;
            }

            return $"\"{field.Replace("\"", "\"\"")}\"";
        }

        /// <summary>
        /// CSV 텍스트를 SO에 임포트.
        /// </summary>
        /// <param name="targetSO">대상 SO</param>
        /// <param name="csvText">CSV 원문 텍스트</param>
        /// <param name="sourceName">로그/메시지 표시용 소스 이름</param>
        /// <param name="showPopup">완료/실패 팝업 표시 여부</param>
        /// <param name="saveAssets">AssetDatabase 저장/새로고침 수행 여부</param>
        /// <returns>임포트 성공 여부</returns>
        public static bool ImportCSVFromText(DialogueInformationSO targetSO, string csvText, string sourceName = "CSV", bool showPopup = true, bool saveAssets = true)
        {
            if (targetSO == null)
            {
                Debug.LogError("[DialogueCSVImporter] Target SO is null!");
                return false;
            }

            try
            {
                // 1. CSV 파싱 (행렬 문자열 추출)
                List<string[]> rows = DialogueCSVReader.Parse(csvText);

                // 1.5. 커맨드/조건 규칙 자동 보정 및 사전 검증
                DialogueCSVAutoCorrector.Report report = DialogueCSVAutoCorrector.NormalizeAndValidate(rows);

                if (report.INVALID_COMMANDS.Count > 0 || report.INVALID_CONDITIONS.Count > 0)
                {
                    string invalidCommandSample = string.Join("\n", report.INVALID_COMMANDS.Take(5));
                    string invalidConditionSample = string.Join("\n", report.INVALID_CONDITIONS.Take(5));

                    string failMessage =
                        $"CSV rule validation failed.\n" +
                        $"Invalid Commands: {report.INVALID_COMMANDS.Count}\n" +
                        $"Invalid Conditions: {report.INVALID_CONDITIONS.Count}\n\n" +
                        (string.IsNullOrEmpty(invalidCommandSample) ? string.Empty : $"[Commands]\n{invalidCommandSample}\n\n") +
                        (string.IsNullOrEmpty(invalidConditionSample) ? string.Empty : $"[Conditions]\n{invalidConditionSample}");

                    Debug.LogError($"[DialogueCSVImporter] {sourceName} validation failed.\n{failMessage}");
                    if (showPopup)
                    {
                        EditorUtility.DisplayDialog("임포트 실패", failMessage, "확인");
                    }

                    return false;
                }

                if (report.correctedCommandCount > 0)
                {
                    Debug.LogWarning(
                        $"[DialogueCSVImporter] Auto-corrected {report.correctedCommandCount} command token(s) in '{sourceName}'.");
                }

                // 2. 데이터 빌드 (Node 리스트 생성 및 검증)
                List<DialogueNode> nodes = DialogueDataBuilder.Build(rows, sourceName);

                if (nodes == null || nodes.Count == 0)
                {
                    const string FAIL_MESSAGE = "No valid nodes found in CSV.";
                    Debug.LogWarning($"[DialogueCSVImporter] {FAIL_MESSAGE}");
                    if (showPopup)
                    {
                        EditorUtility.DisplayDialog("임포트 실패", FAIL_MESSAGE, "확인");
                    }
                    return false;
                }

                // 3. 자동 정렬 (Auto-Layout)
                DialogueNodeLayoutUtility.ApplyAutoLayout(nodes);

                // 4. 데이터 적용
                Undo.RecordObject(targetSO, "Import Dialogue from CSV");
                targetSO.SetNodes(nodes);
                targetSO.SetStartNode(nodes[0].NodeID); // 첫 번째 노드를 시작 노드로 자동 지정

                // 5. 저장 및 동기화
                EditorUtility.SetDirty(targetSO);
                if (saveAssets)
                {
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }

                Debug.Log($"[DialogueCSVImporter] Successfully imported {nodes.Count} nodes from '{sourceName}' into '{targetSO.name}'");
                if (showPopup)
                {
                    EditorUtility.DisplayDialog(
                        "임포트 완료",
                        $"{nodes.Count}개의 노드를 성공적으로 임포트했습니다.\n" +
                        $"(자동 정렬 적용됨)\n" +
                        (report.correctedCommandCount > 0
                            ? $"자동 교정 커맨드: {report.correctedCommandCount}"
                            : "자동 교정 없음"),
                        "확인");
                }
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[DialogueCSVImporter] Import failed: {e.Message}");
                if (showPopup)
                {
                    EditorUtility.DisplayDialog("임포트 실패", $"오류가 발생했습니다: {e.Message}", "확인");
                }
                return false;
            }
        }

        /// <summary>
        /// SO 데이터를 CSV로 변환하여 파일로 저장
        /// </summary>
        public static void ExportCSV(DialogueInformationSO targetSO)
        {
            if (targetSO == null)
            {
                Debug.LogError("[DialogueCSVImporter] Target SO is null!");
                return;
            }

            string path = EditorUtility.SaveFilePanel("CSV 다이알로그 엑스포트", "", targetSO.name, "csv");
            if (string.IsNullOrEmpty(path)) return;

            try
            {
                // 1. CSV 데이터 생성
                string csvText = DialogueCSVWriter.Write(targetSO);

                // 2. 파일 저장
                File.WriteAllText(path, csvText, System.Text.Encoding.UTF8);

                Debug.Log($"[DialogueCSVImporter] Successfully exported '{targetSO.name}' to '{path}'");
                EditorUtility.DisplayDialog("엑스포트 완료", $"성공적으로 엑스포트했습니다.", "확인");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[DialogueCSVImporter] Export failed: {e.Message}");
                EditorUtility.DisplayDialog("엑스포트 실패", $"오류가 발생했습니다: {e.Message}", "확인");
            }
        }

    }
}
