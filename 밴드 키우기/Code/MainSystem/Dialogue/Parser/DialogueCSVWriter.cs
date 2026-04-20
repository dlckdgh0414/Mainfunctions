using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Code.MainSystem.Dialogue.Parser
{
    /// <summary>
    /// DialogueInformationSO 데이터를 CSV 문자열로 변환하는 유틸리티 클래스
    /// </summary>
    public static class DialogueCSVWriter
    {
        private const string HEADER = "NodeID,CharacterID,CharacterEmotion,NameTagPosition,BackgroundID,DialogueDetail,NextNodeID,Choices,Commands,VoiceID";

        public static string Write(DialogueInformationSO dialogueSO)
        {
            if (dialogueSO == null) return string.Empty;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine(HEADER);

            foreach (DialogueNode node in dialogueSO.DialogueNodes)
            {
                sb.AppendLine(FormatNode(node));
            }

            return sb.ToString();
        }

        private static string FormatNode(DialogueNode node)
        {
            string[] fields = new string[10];
            fields[0] = EscapeCSV(node.NodeID);
            fields[1] = EscapeCSV(node.CharacterID);
            fields[2] = node.CharacterEmotion.ToString();
            fields[3] = node.NameTagPosition.ToString();
            fields[4] = EscapeCSV(node.BackgroundID);
            fields[5] = EscapeCSV(node.DialogueDetail);
            fields[6] = EscapeCSV(node.NextNodeID);
            fields[7] = FormatChoices(node.Choices);
            fields[8] = FormatCommands(node.Commands);
            fields[9] = EscapeCSV(node.VoiceID);

            return string.Join(",", fields);
        }

        private static string FormatChoices(List<DialogueChoice> choices)
        {
            if (choices == null || choices.Count == 0) return string.Empty;

            List<string> choiceStrings = new List<string>();
            foreach (DialogueChoice choice in choices)
            {
                string conditions = FormatConditions(choice.Conditions);
                string choiceToken = DialogueChoiceTokenUtility.BuildChoiceToken(choice, conditions);
                choiceStrings.Add(choiceToken);
            }

            return EscapeCSV(string.Join("|", choiceStrings));
        }

        private static string FormatCommands(List<IDialogueCommand> commands)
        {
            if (commands == null || commands.Count == 0) return string.Empty;

            List<string> cmdStrings = new List<string>();
            foreach (IDialogueCommand cmd in commands)
            {
                if (cmd == null) continue;
                
                Type type = cmd.GetType();
                string cmdName = type.Name;
                if (cmdName.EndsWith("Command", StringComparison.OrdinalIgnoreCase))
                {
                    cmdName = cmdName.Substring(0, cmdName.Length - "Command".Length);
                }

                FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
                List<string> args = new List<string>();
                args.Add(cmdName);
                
                foreach (FieldInfo field in fields)
                {
                    object val = field.GetValue(cmd);
                    args.Add(val?.ToString() ?? string.Empty);
                }

                cmdStrings.Add(string.Join(":", args));
            }

            return EscapeCSV(string.Join("|", cmdStrings));
        }

        private static string FormatConditions(List<IDialogueCondition> conditions)
        {
            if (conditions == null || conditions.Count == 0) return string.Empty;

            List<string> condStrings = new List<string>();
            foreach (IDialogueCondition cond in conditions)
            {
                if (cond == null) continue;

                Type type = cond.GetType();
                string condName = type.Name;
                
                FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
                List<string> args = new List<string>();
                args.Add(condName);

                foreach (FieldInfo field in fields)
                {
                    object val = field.GetValue(cond);
                    args.Add(val?.ToString() ?? string.Empty);
                }

                condStrings.Add(string.Join(":", args));
            }

            return string.Join("&", condStrings);
        }

        private static string EscapeCSV(string field)
        {
            if (string.IsNullOrEmpty(field)) return string.Empty;

            bool containsComma = field.Contains(",");
            bool containsQuote = field.Contains("\"");
            bool containsNewline = field.Contains("\n") || field.Contains("\r");

            if (containsComma || containsQuote || containsNewline)
            {
                field = field.Replace("\"", "\"\"");
                return $"\"{field}\"";
            }

            return field;
        }
    }
}
