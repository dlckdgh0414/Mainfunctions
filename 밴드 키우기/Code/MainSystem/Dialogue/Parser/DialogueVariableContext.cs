using System;
using System.Collections.Generic;

namespace Code.MainSystem.Dialogue.Parser
{
    /// <summary>
    /// 다이알로그 세션 단위 변수 값을 저장/조회하는 컨텍스트.
    /// </summary>
    public class DialogueVariableContext
    {
        private readonly Dictionary<string, object> VALUE_MAP = new Dictionary<string, object>(StringComparer.Ordinal);
        private readonly Dictionary<string, Func<object>> GETTER_MAP = new Dictionary<string, Func<object>>(StringComparer.Ordinal);

        /// <summary>
        /// 고정 값을 키에 바인딩.
        /// </summary>
        /// <param name="key">치환 키.</param>
        /// <param name="value">치환 값.</param>
        public void SetValue(string key, object value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return;
            }

            VALUE_MAP[key] = value;
            GETTER_MAP.Remove(key);
        }

        /// <summary>
        /// 동적 값을 반환하는 getter를 키에 바인딩.
        /// </summary>
        /// <param name="key">치환 키.</param>
        /// <param name="getter">치환 시점 값 반환 함수.</param>
        public void SetGetter(string key, Func<object> getter)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return;
            }

            GETTER_MAP[key] = getter;
            VALUE_MAP.Remove(key);
        }

        /// <summary>
        /// 등록된 키를 제거.
        /// </summary>
        /// <param name="key">제거 키.</param>
        public void Remove(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return;
            }

            VALUE_MAP.Remove(key);
            GETTER_MAP.Remove(key);
        }

        /// <summary>
        /// 키에 해당하는 값을 조회.
        /// </summary>
        /// <param name="key">조회 키.</param>
        /// <param name="value">조회 결과 값.</param>
        /// <returns>조회 성공 여부.</returns>
        public bool TryGetValue(string key, out object value)
        {
            value = null;

            if (string.IsNullOrWhiteSpace(key))
            {
                return false;
            }

            if (GETTER_MAP.TryGetValue(key, out Func<object> getter))
            {
                try
                {
                    value = getter != null ? getter.Invoke() : null;
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            if (VALUE_MAP.TryGetValue(key, out object storedValue))
            {
                value = storedValue;
                return true;
            }

            return false;
        }
    }
}
