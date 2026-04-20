using System.Collections.Generic;
using System.Text;

namespace Code.MainSystem.Dialogue.Parser
{
    /// <summary>
    /// CSV 파일을 읽어 행렬 형태의 문자열 리스트로 반환하는 유틸리티 클래스
    /// </summary>
    public static class DialogueCSVReader
    {
        /// <summary>
        /// CSV 원문 텍스트를 행별로 파싱하여 반환
        /// </summary>
        public static List<string[]> Parse(string csvText)
        {
            List<string[]> lines = new List<string[]>();
            if (string.IsNullOrEmpty(csvText))
            {
                return lines;
            }

            List<string> currentRow = new List<string>();
            StringBuilder currentField = new StringBuilder();
            bool inQuotes = false;
            bool headerSkipped = false;

            for (int i = 0; i < csvText.Length; i++)
            {
                char c = csvText[i];

                if (c == '"')
                {
                    if (inQuotes && i + 1 < csvText.Length && csvText[i + 1] == '"')
                    {
                        currentField.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }

                    continue;
                }

                if (!inQuotes && c == ',')
                {
                    currentRow.Add(CleanField(currentField.ToString()));
                    currentField.Clear();
                    continue;
                }

                if (!inQuotes && (c == '\r' || c == '\n'))
                {
                    currentRow.Add(CleanField(currentField.ToString()));
                    currentField.Clear();

                    if (!headerSkipped && IsHeaderRow(currentRow))
                    {
                        headerSkipped = true;
                    }
                    else if (!IsEmptyRow(currentRow))
                    {
                        lines.Add(currentRow.ToArray());
                    }

                    currentRow.Clear();

                    if (c == '\r' && i + 1 < csvText.Length && csvText[i + 1] == '\n')
                    {
                        i++;
                    }

                    continue;
                }

                currentField.Append(c);
            }

            if (currentField.Length > 0 || currentRow.Count > 0)
            {
                currentRow.Add(CleanField(currentField.ToString()));

                if (!headerSkipped && IsHeaderRow(currentRow))
                {
                    headerSkipped = true;
                }
                else if (!IsEmptyRow(currentRow))
                {
                    lines.Add(currentRow.ToArray());
                }
            }

            return lines;
        }

        private static bool IsHeaderRow(List<string> row)
        {
            return row != null
                   && row.Count > 0
                   && !string.IsNullOrEmpty(row[0])
                   && row[0].Contains("NodeID");
        }

        private static bool IsEmptyRow(List<string> row)
        {
            if (row == null || row.Count == 0)
            {
                return true;
            }

            for (int i = 0; i < row.Count; i++)
            {
                if (!string.IsNullOrWhiteSpace(row[i]))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 필드 전후 공백 정리
        /// </summary>
        private static string CleanField(string field)
        {
            if (field == null)
            {
                return string.Empty;
            }

            return field.Trim();
        }
    }
}
