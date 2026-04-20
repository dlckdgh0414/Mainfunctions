using System;
using System.Collections.Generic;
using System.Text;

namespace Code.MainSystem.Dialogue.Parser
{
    /// <summary>
    /// Choices 컬럼 토큰 파싱/직렬화를 담당하는 유틸리티
    /// </summary>
    public static class DialogueChoiceTokenUtility
    {
        private const string META_KEY_SUB = "sub";
        private const string META_KEY_LOCK = "lock";

        /// <summary>
        /// Choices 컬럼 문자열을 선택지 토큰 리스트로 분리
        /// </summary>
        /// <param name="rawChoices">원본 Choices 문자열</param>
        public static List<string> SplitChoiceTokens(string rawChoices)
        {
            List<string> tokens = new List<string>();
            if (string.IsNullOrWhiteSpace(rawChoices))
            {
                return tokens;
            }

            StringBuilder tokenBuilder = new StringBuilder();
            int braceDepth = 0;
            int parenDepth = 0;
            bool escaped = false;

            for (int i = 0; i < rawChoices.Length; i++)
            {
                char current = rawChoices[i];

                if (escaped)
                {
                    tokenBuilder.Append(current);
                    escaped = false;
                    continue;
                }

                if (current == '\\')
                {
                    tokenBuilder.Append(current);
                    escaped = true;
                    continue;
                }

                if (current == '{')
                {
                    braceDepth++;
                    tokenBuilder.Append(current);
                    continue;
                }

                if (current == '}' && braceDepth > 0)
                {
                    braceDepth--;
                    tokenBuilder.Append(current);
                    continue;
                }

                if (current == '(')
                {
                    parenDepth++;
                    tokenBuilder.Append(current);
                    continue;
                }

                if (current == ')' && parenDepth > 0)
                {
                    parenDepth--;
                    tokenBuilder.Append(current);
                    continue;
                }

                if (current == '|' && braceDepth == 0 && parenDepth == 0)
                {
                    string token = tokenBuilder.ToString().Trim();
                    if (!string.IsNullOrWhiteSpace(token))
                    {
                        tokens.Add(token);
                    }
                    tokenBuilder.Clear();
                    continue;
                }

                tokenBuilder.Append(current);
            }

            string lastToken = tokenBuilder.ToString().Trim();
            if (!string.IsNullOrWhiteSpace(lastToken))
            {
                tokens.Add(lastToken);
            }

            return tokens;
        }

        /// <summary>
        /// 선택지 토큰 하나를 파싱
        /// </summary>
        /// <param name="token">선택지 토큰 문자열</param>
        /// <param name="nextNodeID">다음 노드 ID</param>
        /// <param name="choiceText">선택지 메인 텍스트</param>
        /// <param name="rawConditions">조건 원문 문자열</param>
        /// <param name="subText">보조 설명 텍스트</param>
        /// <param name="lockedSubText">잠금 보조 설명 텍스트</param>
        public static bool TryParseChoiceToken(
            string token,
            out string nextNodeID,
            out string choiceText,
            out string rawConditions,
            out string subText,
            out string lockedSubText)
        {
            nextNodeID = string.Empty;
            choiceText = string.Empty;
            rawConditions = string.Empty;
            subText = string.Empty;
            lockedSubText = string.Empty;

            if (string.IsNullOrWhiteSpace(token))
            {
                return false;
            }

            string workingToken = token.Trim();
            if (!TryExtractTrailingMeta(ref workingToken, out string parsedSubText, out string parsedLockedSubText))
            {
                return false;
            }

            int firstColonIndex = workingToken.IndexOf(':');
            if (firstColonIndex <= 0)
            {
                return false;
            }

            nextNodeID = workingToken.Substring(0, firstColonIndex).Trim();
            if (string.IsNullOrWhiteSpace(nextNodeID))
            {
                return false;
            }

            string remainder = workingToken.Substring(firstColonIndex + 1);
            int splitIndex = FindConditionSplitIndex(remainder);
            if (splitIndex < 0)
            {
                choiceText = UnescapeText(remainder.Trim());
            }
            else
            {
                choiceText = UnescapeText(remainder.Substring(0, splitIndex).Trim());
                rawConditions = remainder.Substring(splitIndex + 1).Trim();
            }

            if (string.IsNullOrWhiteSpace(choiceText))
            {
                return false;
            }

            subText = UnescapeText(parsedSubText.Trim());
            lockedSubText = UnescapeText(parsedLockedSubText.Trim());
            return true;
        }

        /// <summary>
        /// 선택지 데이터를 토큰 문자열로 직렬화
        /// </summary>
        /// <param name="choice">직렬화 대상 선택지 데이터</param>
        /// <param name="conditions">조건 직렬화 문자열</param>
        public static string BuildChoiceToken(DialogueChoice choice, string conditions)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(choice.NextNodeID);
            builder.Append(':');
            builder.Append(EscapeText(choice.ChoiceText));

            if (!string.IsNullOrWhiteSpace(conditions))
            {
                builder.Append(':');
                builder.Append(conditions);
            }

            if (!string.IsNullOrWhiteSpace(choice.SubText))
            {
                builder.Append('(');
                builder.Append(META_KEY_SUB);
                builder.Append('=');
                builder.Append(EscapeText(choice.SubText));
                builder.Append(')');
            }

            if (!string.IsNullOrWhiteSpace(choice.LockedSubText))
            {
                builder.Append('(');
                builder.Append(META_KEY_LOCK);
                builder.Append('=');
                builder.Append(EscapeText(choice.LockedSubText));
                builder.Append(')');
            }

            return builder.ToString();
        }

        /// <summary>
        /// 텍스트 예약 문자를 이스케이프
        /// </summary>
        /// <param name="value">이스케이프 대상 문자열</param>
        public static string EscapeText(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            StringBuilder builder = new StringBuilder(value.Length);
            for (int i = 0; i < value.Length; i++)
            {
                char current = value[i];
                if (current == '\\' || current == '|' || current == '(' || current == ')')
                {
                    builder.Append('\\');
                }

                builder.Append(current);
            }

            return builder.ToString();
        }

        /// <summary>
        /// 텍스트 이스케이프 시퀀스를 해제
        /// </summary>
        /// <param name="value">이스케이프 해제 대상 문자열</param>
        public static string UnescapeText(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            StringBuilder builder = new StringBuilder(value.Length);
            bool escaped = false;

            for (int i = 0; i < value.Length; i++)
            {
                char current = value[i];
                if (escaped)
                {
                    if (current == '\\' || current == '|' || current == '(' || current == ')')
                    {
                        builder.Append(current);
                    }
                    else
                    {
                        builder.Append('\\');
                        builder.Append(current);
                    }

                    escaped = false;
                    continue;
                }

                if (current == '\\')
                {
                    escaped = true;
                    continue;
                }

                builder.Append(current);
            }

            if (escaped)
            {
                builder.Append('\\');
            }

            return builder.ToString();
        }

        private static bool TryExtractTrailingMeta(ref string tokenWithoutMeta, out string subText, out string lockedSubText)
        {
            subText = string.Empty;
            lockedSubText = string.Empty;

            string working = tokenWithoutMeta.TrimEnd();

            while (TryReadLastMetaBlock(working, out int startIndex, out string key, out string value))
            {
                if (string.Equals(key, META_KEY_SUB, StringComparison.OrdinalIgnoreCase))
                {
                    if (!string.IsNullOrEmpty(subText))
                    {
                        return false;
                    }

                    subText = value;
                }
                else if (string.Equals(key, META_KEY_LOCK, StringComparison.OrdinalIgnoreCase))
                {
                    if (!string.IsNullOrEmpty(lockedSubText))
                    {
                        return false;
                    }

                    lockedSubText = value;
                }

                working = working.Substring(0, startIndex).TrimEnd();
            }

            tokenWithoutMeta = working;
            return true;
        }

        private static bool TryReadLastMetaBlock(string source, out int startIndex, out string key, out string value)
        {
            startIndex = -1;
            key = string.Empty;
            value = string.Empty;

            if (string.IsNullOrWhiteSpace(source))
            {
                return false;
            }

            int endIndex = source.Length - 1;
            if (source[endIndex] != ')')
            {
                return false;
            }

            if (IsEscapedCharacter(source, endIndex))
            {
                return false;
            }

            int openIndex = FindMatchingOpenParenthesis(source, endIndex);
            if (openIndex < 0)
            {
                return false;
            }

            string inner = source.Substring(openIndex + 1, endIndex - openIndex - 1);
            int splitIndex = FindKeyValueSplitIndex(inner);
            if (splitIndex <= 0)
            {
                return false;
            }

            key = inner.Substring(0, splitIndex).Trim();
            value = inner.Substring(splitIndex + 1);
            startIndex = openIndex;

            if (!string.Equals(key, META_KEY_SUB, StringComparison.OrdinalIgnoreCase)
                && !string.Equals(key, META_KEY_LOCK, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return !string.IsNullOrWhiteSpace(key);
        }

        private static int FindMatchingOpenParenthesis(string text, int closeIndex)
        {
            for (int i = closeIndex - 1; i >= 0; i--)
            {
                char current = text[i];

                if (current == '(' && !IsEscapedCharacter(text, i))
                {
                    return i;
                }
            }

            return -1;
        }

        private static bool IsEscapedCharacter(string text, int index)
        {
            int backslashCount = 0;
            int cursor = index - 1;

            while (cursor >= 0 && text[cursor] == '\\')
            {
                backslashCount++;
                cursor--;
            }

            return backslashCount % 2 == 1;
        }

        private static int FindKeyValueSplitIndex(string text)
        {
            bool escaped = false;
            for (int i = 0; i < text.Length; i++)
            {
                char current = text[i];
                if (escaped)
                {
                    escaped = false;
                    continue;
                }

                if (current == '\\')
                {
                    escaped = true;
                    continue;
                }

                if (current == '=')
                {
                    return i;
                }
            }

            return -1;
        }

        private static int FindConditionSplitIndex(string remainder)
        {
            int braceDepth = 0;
            bool escaped = false;

            for (int i = remainder.Length - 1; i >= 0; i--)
            {
                char current = remainder[i];

                if (escaped)
                {
                    escaped = false;
                    continue;
                }

                if (current == '\\')
                {
                    escaped = true;
                    continue;
                }

                if (current == '}')
                {
                    braceDepth++;
                    continue;
                }

                if (current == '{')
                {
                    braceDepth--;
                    continue;
                }

                if (current != ':' || braceDepth > 0 || i + 1 >= remainder.Length)
                {
                    continue;
                }

                string possibleCondition = remainder.Substring(i + 1).Trim();
                if (IsConditionExpression(possibleCondition))
                {
                    return i;
                }
            }

            return -1;
        }

        private static bool IsConditionExpression(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
            {
                return false;
            }

            string[] terms = expression.Split('&');
            for (int i = 0; i < terms.Length; i++)
            {
                string trimmedTerm = terms[i].Trim();
                if (string.IsNullOrEmpty(trimmedTerm))
                {
                    return false;
                }

                int delimiterIndex = trimmedTerm.IndexOf(':');
                string conditionName = delimiterIndex >= 0
                    ? trimmedTerm.Substring(0, delimiterIndex)
                    : trimmedTerm;

                if (string.IsNullOrWhiteSpace(conditionName))
                {
                    return false;
                }

                char firstChar = conditionName[0];
                if (!char.IsLetter(firstChar) && firstChar != '_')
                {
                    return false;
                }
            }

            return true;
        }
    }
}
