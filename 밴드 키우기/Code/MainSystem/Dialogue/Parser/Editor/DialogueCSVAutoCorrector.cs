using System;
using System.Collections.Generic;
using System.Linq;

namespace Code.MainSystem.Dialogue.Parser.Editor
{
    public static class DialogueCSVAutoCorrector
    {
        private const int CHOICES_COLUMN_INDEX = 7;
        private const int COMMANDS_COLUMN_INDEX = 8;
        private static readonly Dictionary<string, string> MEMBER_ALIAS_MAP = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "Drum", "Drums" },
            { "Keyboard", "Piano" }
        };
        private static readonly Dictionary<string, string> STAT_ALIAS_MAP = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "작곡감", "Composition" },
            { "가사", "Lyrics" },
            { "멜로디", "Melody" }
        };

        public sealed class Report
        {
            public int correctedCommandCount;
            public readonly List<string> INVALID_COMMANDS = new List<string>();
            public readonly List<string> INVALID_CONDITIONS = new List<string>();
        }

        public static Report NormalizeAndValidate(List<string[]> rows)
        {
            Report report = new Report();
            if (rows == null)
            {
                return report;
            }

            for (int rowIndex = 0; rowIndex < rows.Count; rowIndex++)
            {
                string[] row = rows[rowIndex];
                NormalizeCommands(row, rowIndex, report);
                ValidateChoiceConditions(row, rowIndex, report);
            }

            return report;
        }

        private static void NormalizeCommands(string[] row, int rowIndex, Report report)
        {
            if (row == null || row.Length <= COMMANDS_COLUMN_INDEX)
            {
                return;
            }

            string rawCommands = row[COMMANDS_COLUMN_INDEX];
            if (string.IsNullOrWhiteSpace(rawCommands))
            {
                return;
            }

            string[] tokens = rawCommands.Split('|');
            bool changed = false;

            for (int i = 0; i < tokens.Length; i++)
            {
                string normalized = NormalizeCommandToken(tokens[i], out bool tokenChanged);
                if (tokenChanged)
                {
                    changed = true;
                    report.correctedCommandCount++;
                }

                tokens[i] = normalized;

                string commandName = GetHeadName(normalized);
                if (!string.IsNullOrEmpty(commandName) && !DialogueCommandFactory.IsRegistered(commandName))
                {
                    report.INVALID_COMMANDS.Add($"Line {rowIndex + 2}: {normalized}");
                }
            }

            if (changed)
            {
                row[COMMANDS_COLUMN_INDEX] = string.Join("|", tokens);
            }
        }

        private static string NormalizeCommandToken(string token, out bool changed)
        {
            changed = false;
            if (string.IsNullOrWhiteSpace(token))
            {
                return token;
            }

            string[] parts = token.Split(':');
            if (parts.Length == 0)
            {
                return token;
            }

            string commandName = parts[0].Trim();

            if (!commandName.Equals("ChangeStat", StringComparison.OrdinalIgnoreCase) || parts.Length < 4)
            {
                if (commandName.Equals("ChangeCondition", StringComparison.OrdinalIgnoreCase) && parts.Length >= 3)
                {
                    string mappedMember = MapMember(parts[1].Trim());
                    if (!string.Equals(mappedMember, parts[1], StringComparison.Ordinal))
                    {
                        changed = true;
                        return $"ChangeCondition:{mappedMember}:{string.Join(":", parts.Skip(2))}";
                    }
                }

                return token;
            }

            string member = MapMember(parts[1].Trim());
            string stat = parts[2].Trim();
            string value = string.Join(":", parts.Skip(3));

            if (member.Equals("PLAYER", StringComparison.OrdinalIgnoreCase)
                && stat.Equals("Gold", StringComparison.OrdinalIgnoreCase))
            {
                changed = true;
                return $"ChangeGold:{value}";
            }

            if (stat.Equals("컨디션", StringComparison.OrdinalIgnoreCase))
            {
                changed = true;
                return $"ChangeCondition:{member}:{value}";
            }

            string mappedStat = MapStat(stat);
            string normalizedToken = $"ChangeStat:{member}:{mappedStat}:{value}";

            if (!string.Equals(normalizedToken, token, StringComparison.Ordinal))
            {
                changed = true;
            }

            return normalizedToken;
        }

        private static void ValidateChoiceConditions(string[] row, int rowIndex, Report report)
        {
            if (row == null || row.Length <= CHOICES_COLUMN_INDEX)
            {
                return;
            }

            string rawChoices = row[CHOICES_COLUMN_INDEX];
            if (string.IsNullOrWhiteSpace(rawChoices))
            {
                return;
            }

            List<string> choiceTokens = DialogueChoiceTokenUtility.SplitChoiceTokens(rawChoices);
            foreach (string choiceToken in choiceTokens)
            {
                if (!DialogueChoiceTokenUtility.TryParseChoiceToken(
                        choiceToken,
                        out _,
                        out _,
                        out string rawConditions,
                        out _,
                        out _))
                {
                    report.INVALID_CONDITIONS.Add($"Line {rowIndex + 2}: Invalid choice token '{choiceToken}'");
                    continue;
                }

                foreach (string conditionToken in rawConditions.Split('&'))
                {
                    string conditionName = GetHeadName(conditionToken.Trim());
                    if (string.IsNullOrEmpty(conditionName))
                    {
                        continue;
                    }

                    if (!DialogueConditionFactory.IsRegistered(conditionName))
                    {
                        report.INVALID_CONDITIONS.Add($"Line {rowIndex + 2}: {conditionToken.Trim()}");
                    }
                }
            }
        }

        private static string MapMember(string member)
        {
            return TryMapAlias(member, MEMBER_ALIAS_MAP);
        }

        private static string MapStat(string stat)
        {
            return TryMapAlias(stat, STAT_ALIAS_MAP);
        }

        private static string TryMapAlias(string token, Dictionary<string, string> aliasMap)
        {
            if (string.IsNullOrWhiteSpace(token) || aliasMap == null)
            {
                return token;
            }

            return aliasMap.TryGetValue(token, out string mappedValue) ? mappedValue : token;
        }

        private static string GetHeadName(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return string.Empty;
            }

            int index = token.IndexOf(':');
            return index >= 0 ? token.Substring(0, index).Trim() : token.Trim();
        }
    }
}
