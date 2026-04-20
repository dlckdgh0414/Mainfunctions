using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Code.MainSystem.Dialogue.Parser
{
    /// <summary>
    /// 문자열로부터 다이알로그 연출 명령어 객체를 생성하는 팩토리 클래스 (OCP 준수)
    /// </summary>
    public static class DialogueCommandFactory
    {
        private static Dictionary<string, Type> _commandTypes;

        static DialogueCommandFactory()
        {
            InitializeCache();
        }

        /// <summary>
        /// 어셈블리 내의 모든 IDialogueCommand 구현체를 찾아 캐싱
        /// </summary>
        private static void InitializeCache()
        {
            _commandTypes = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
            
            Type interfaceType = typeof(IDialogueCommand);
            IEnumerable<Type> types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => interfaceType.IsAssignableFrom(p) && !p.IsInterface && !p.IsAbstract);

            foreach (Type type in types)
            {
                // 클래스 풀네임 등록 (예: ChangeStatCommand)
                _commandTypes[type.Name] = type;

                // "Command" 접미사를 뺀 이름도 등록 (예: ChangeStatCommand -> ChangeStat)
                if (type.Name.EndsWith("Command", StringComparison.OrdinalIgnoreCase))
                {
                    string shortName = type.Name.Substring(0, type.Name.Length - "Command".Length);
                    if (!_commandTypes.ContainsKey(shortName))
                    {
                        _commandTypes[shortName] = type;
                    }
                }
            }
        }

        /// <summary>
        /// 문자열(예: ShakeScreen:2.0)을 기반으로 명령어 객체 생성
        /// </summary>
        public static IDialogueCommand Create(string rawCommand)
        {
            if (string.IsNullOrEmpty(rawCommand)) return null;

            string[] parts = rawCommand.Split(':');
            string commandName = parts[0];
            string[] args = parts.Skip(1).ToArray();

            if (!_commandTypes.TryGetValue(commandName, out Type type))
            {
                Debug.LogError($"[DialogueCommandFactory] Unknown command: {commandName}");
                return null;
            }

            IDialogueCommand command = (IDialogueCommand)Activator.CreateInstance(type);

            // 리플렉션을 이용해 필드에 매개변수 주입 (단순 구현: 순서대로 주입)
            FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
            for (int i = 0; i < Math.Min(fields.Length, args.Length); i++)
            {
                try
                {
                    if (fields[i].FieldType.IsEnum)
                    {
                        object enumValue = Enum.Parse(fields[i].FieldType, args[i], true);
                        fields[i].SetValue(command, enumValue);
                    }
                    else
                    {
                        object value = Convert.ChangeType(args[i], fields[i].FieldType);
                        fields[i].SetValue(command, value);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"[DialogueCommandFactory] Parameter mismatch in {commandName}: {e.Message}");
                }
            }

            return command;
        }

        /// <summary>
        /// 파이프(|)로 구분된 명령어 텍스트 리스트를 파싱하여 반환
        /// </summary>
        public static List<IDialogueCommand> ParseBatch(string rawCommands)
        {
            if (string.IsNullOrWhiteSpace(rawCommands)) return new List<IDialogueCommand>();

            return rawCommands.Split('|')
                .Select(Create)
                .Where(c => c != null)
                .ToList();
        }

        public static bool IsRegistered(string commandName)
        {
            if (string.IsNullOrWhiteSpace(commandName))
            {
                return false;
            }

            return _commandTypes.ContainsKey(commandName);
        }
    }
}
