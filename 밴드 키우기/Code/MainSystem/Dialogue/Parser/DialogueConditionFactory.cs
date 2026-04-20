using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Code.MainSystem.Dialogue.Parser
{
    /// <summary>
    /// 문자열로부터 다이알로그 노출 조건 객체를 생성하는 팩토리 클래스 (OCP 준수)
    /// </summary>
    public static class DialogueConditionFactory
    {
        private static Dictionary<string, Type> _conditionTypes;

        static DialogueConditionFactory()
        {
            InitializeCache();
        }

        private static void InitializeCache()
        {
            _conditionTypes = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
            
            Type interfaceType = typeof(IDialogueCondition);
            IEnumerable<Type> types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => interfaceType.IsAssignableFrom(p) && !p.IsInterface && !p.IsAbstract);

            foreach (Type type in types)
            {
                _conditionTypes[type.Name] = type;

                if (type.Name.EndsWith("Condition", StringComparison.OrdinalIgnoreCase))
                {
                    string shortName = type.Name.Substring(0, type.Name.Length - "Condition".Length);
                    if (!_conditionTypes.ContainsKey(shortName))
                    {
                        _conditionTypes[shortName] = type;
                    }
                }
            }
        }

        public static IDialogueCondition Create(string rawCondition)
        {
            if (string.IsNullOrEmpty(rawCondition)) return null;

            string[] parts = rawCondition.Split(':');
            string conditionName = parts[0];
            string[] args = parts.Skip(1).ToArray();

            if (!_conditionTypes.TryGetValue(conditionName, out Type type))
            {
                Debug.LogError($"[DialogueConditionFactory] Unknown condition: {conditionName}");
                return null;
            }

            IDialogueCondition condition = (IDialogueCondition)Activator.CreateInstance(type);

            FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
            for (int i = 0; i < Math.Min(fields.Length, args.Length); i++)
            {
                try
                {
                    object value = Convert.ChangeType(args[i], fields[i].FieldType);
                    fields[i].SetValue(condition, value);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[DialogueConditionFactory] Parameter mismatch in {conditionName}: {e.Message}");
                }
            }

            return condition;
        }

        public static List<IDialogueCondition> ParseBatch(string rawConditions)
        {
            if (string.IsNullOrWhiteSpace(rawConditions)) return new List<IDialogueCondition>();

            return rawConditions.Split('&')
                .Select(Create)
                .Where(c => c != null)
                .ToList();
        }

        public static bool IsRegistered(string conditionName)
        {
            if (string.IsNullOrWhiteSpace(conditionName))
            {
                return false;
            }

            return _conditionTypes.ContainsKey(conditionName);
        }
    }
}
