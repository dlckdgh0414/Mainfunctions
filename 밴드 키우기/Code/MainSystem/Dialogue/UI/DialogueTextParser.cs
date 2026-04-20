using System.Collections.Generic;
using System.Text;

namespace Code.MainSystem.Dialogue.UI
{
    public enum TextEffectType { None, Shrink, Speed }

    public struct TextEffectData
    {
        public TextEffectType Type;
        public int StartIndex;
        public int EndIndex;
        public float Value;
    }

    public static class DialogueTextParser
    {
        private struct TagStartInfo
        {
            public TextEffectType Type;
            public float Value;
            public int StartIndex;
        }

        public static (string plainText, List<TextEffectData> effects) Parse(string rawText)
        {
            if (string.IsNullOrEmpty(rawText)) return (string.Empty, new List<TextEffectData>());

            List<TextEffectData> effects = new List<TextEffectData>();
            StringBuilder sb = new StringBuilder();
            Stack<TagStartInfo> stack = new Stack<TagStartInfo>();

            int i = 0;
            while (i < rawText.Length)
            {
                if (rawText[i] == '<')
                {
                    // 1. 닫는 태그 처리 (예: </shrink>)
                    if (i + 1 < rawText.Length && rawText[i + 1] == '/')
                    {
                        int endIdx = rawText.IndexOf('>', i);
                        if (endIdx != -1)
                        {
                            string tagName = rawText.Substring(i + 2, endIdx - (i + 2)).ToLower();
                            TextEffectType type = GetEffectType(tagName);

                            if (type != TextEffectType.None)
                            {
                                // 스택에서 가장 최근의 같은 타입 태그를 찾음
                                List<TagStartInfo> temp = new List<TagStartInfo>();
                                while (stack.Count > 0)
                                {
                                    TagStartInfo startInfo = stack.Pop();
                                    if (startInfo.Type == type)
                                    {
                                        effects.Add(new TextEffectData
                                        {
                                            Type = startInfo.Type,
                                            Value = startInfo.Value,
                                            StartIndex = startInfo.StartIndex,
                                            EndIndex = sb.Length
                                        });
                                        break;
                                    }
                                    else
                                    {
                                        temp.Add(startInfo);
                                    }
                                }
                                // 짝이 맞지 않는 다른 태그들은 다시 스택에 복구
                                for (int j = temp.Count - 1; j >= 0; j--) stack.Push(temp[j]);
                            }
                            
                            i = endIdx + 1;
                            continue;
                        }
                    }
                    // 2. 여는 태그 처리 (예: <shrink=0.5>)
                    else
                    {
                        int endIdx = rawText.IndexOf('>', i);
                        if (endIdx != -1)
                        {
                            string tagBody = rawText.Substring(i + 1, endIdx - (i + 1));
                            string[] parts = tagBody.Split('=');
                            if (parts.Length == 2)
                            {
                                string tagName = parts[0].ToLower();
                                TextEffectType type = GetEffectType(tagName);

                                if (type != TextEffectType.None && float.TryParse(parts[1], out float val))
                                {
                                    stack.Push(new TagStartInfo
                                    {
                                        Type = type,
                                        Value = val,
                                        StartIndex = sb.Length
                                    });
                                    i = endIdx + 1;
                                    continue;
                                }
                            }
                        }
                    }
                }

                // 태그가 아니거나 유효하지 않은 태그인 경우 일반 문자로 처리
                sb.Append(rawText[i]);
                i++;
            }

            return (sb.ToString(), effects);
        }

        private static TextEffectType GetEffectType(string tagName)
        {
            return tagName switch
            {
                "shrink" => TextEffectType.Shrink,
                "speed" => TextEffectType.Speed,
                _ => TextEffectType.None
            };
        }
    }
}
