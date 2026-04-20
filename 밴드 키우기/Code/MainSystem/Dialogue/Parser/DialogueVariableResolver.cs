using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using Code.Core;
using Code.MainSystem.NewMainScreen;
using Code.MainSystem.StatSystem.BaseStats;
using Code.SubSystem.BandFunds;
using UnityEngine;

namespace Code.MainSystem.Dialogue.Parser
{
    /// <summary>
    /// 다이얼로그 텍스트 내의 변수를 런타임 값으로 치환하는 정적 유틸리티 클래스
    /// </summary>
    public static class DialogueVariableResolver
    {
        private static readonly Dictionary<string, Func<object>> GLOBAL_VARIABLE_REGISTRY = new Dictionary<string, Func<object>>(StringComparer.Ordinal);
        private static readonly Regex VARIABLE_PATTERN = new Regex(@"\{([^}]+)\}", RegexOptions.Compiled);

        /// <summary>
        /// 외부 시스템에서 변수 값을 제공하는 함수 등록
        /// </summary>
        /// <param name="key">치환할 변수명 (중괄호 제외)</param>
        /// <param name="getter">해당 변수의 런타임 값을 반환하는 델리게이트</param>
        public static void Register(string key, Func<object> getter)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return;
            }

            GLOBAL_VARIABLE_REGISTRY[key] = getter;
        }

        /// <summary>
        /// 등록된 변수 제공 함수 해제
        /// </summary>
        /// <param name="key">해제할 변수명</param>
        public static void Unregister(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return;
            }

            GLOBAL_VARIABLE_REGISTRY.Remove(key);
        }

        /// <summary>
        /// 주어진 텍스트 내의 모든 변수 플레이스홀더를 등록된 값으로 치환하여 반환
        /// </summary>
        /// <param name="text">치환 전 원본 텍스트</param>
        /// <param name="variableContext">대화 세션 변수 컨텍스트</param>
        /// <returns>변수가 치환된 최종 텍스트</returns>
        public static string Resolve(string text, DialogueVariableContext variableContext = null)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }

            return VARIABLE_PATTERN.Replace(text, match => MatchEvaluator(match, variableContext));
        }

        private static string MatchEvaluator(Match match, DialogueVariableContext variableContext)
        {
            string fullKey = match.Groups[1].Value;
            string key = fullKey;
            string format = string.Empty;

            int colonIndex = fullKey.IndexOf(':');
            if (colonIndex >= 0)
            {
                key = fullKey.Substring(0, colonIndex);
                format = fullKey.Substring(colonIndex + 1);
            }

            if (TryResolveValue(key, variableContext, out object value))
            {
                if (value == null)
                {
                    return string.Empty;
                }

                if (!string.IsNullOrEmpty(format) && value is IFormattable formattableValue)
                {
                    try
                    {
                        return formattableValue.ToString(format, CultureInfo.InvariantCulture);
                    }
                    catch (FormatException)
                    {
                        Debug.LogWarning($"[DialogueVariableResolver] Invalid format '{format}' for key '{key}'");
                        return $"<color=red>Err({fullKey})</color>";
                    }
                }

                return value.ToString();
            }

            return $"<color=red>Err({fullKey})</color>";
        }

        /// <summary>
        /// 컨텍스트/전역 레지스트리 순서로 변수 값을 조회
        /// </summary>
        /// <param name="key">조회 키</param>
        /// <param name="variableContext">세션 컨텍스트</param>
        /// <param name="value">조회 값</param>
        /// <returns>조회 성공 여부</returns>
        private static bool TryResolveValue(string key, DialogueVariableContext variableContext, out object value)
        {
            value = null;

            if (variableContext != null && variableContext.TryGetValue(key, out object contextValue))
            {
                value = contextValue;
                return true;
            }

            if (GLOBAL_VARIABLE_REGISTRY.TryGetValue(key, out Func<object> getter))
            {
                try
                {
                    value = getter != null ? getter.Invoke() : null;
                    return true;
                }
                catch (Exception exception)
                {
                    Debug.LogWarning($"[DialogueVariableResolver] Getter exception on key '{key}': {exception.Message}");
                    return false;
                }
            }

            return false;
        }
    }

    public static class DialoguePlaceholderKeys
    {
        public const string OutingMemberId = "outing.member.id";
        public const string OutingMemberName = "outing.member.name";
        public const string OutingLocationId = "outing.location.id";
        public const string OutingLocationName = "outing.location.name";

        public const string OutingMemberTypeAlias = "outing.memberType";
        public const string OutingLocationTypeAlias = "outing.locationType";

        public const string StatMainName = "stat.main.name";
        public const string StatMainValue = "stat.main.value";
        public const string StatMainDelta = "stat.main.delta";
        public const string StatMainPreview = "stat.main.preview";

        public const string GoldValue = "gold.value";
        public const string GoldDelta = "gold.delta";
        public const string GoldPreview = "gold.preview";

        public const string StatFormat = "stat.{0}.{1}.{2}";
        public const string ConditionFormat = "condition.{0}.{1}";

        public const string ConditionTrendUp = "회복됨";
        public const string ConditionTrendDown = "나빠짐";
        public const string ConditionTrendStay = "유지됨";
    }

    /// <summary>
    /// Registers the canonical placeholder keys for dialogue variable contexts.
    /// </summary>
    public static class DialoguePlaceholderRegistrar
    {
        private static readonly StatType[] MAIN_STAT_PRIORITY =
        {
            StatType.Composition,
            StatType.InstrumentProficiency,
        };

        public static void RegisterOutingContext(
            DialogueVariableContext context,
            MemberType memberType,
            LocationType locationType)
        {
            if (context == null)
            {
                return;
            }

            RegisterCommon(context);

            string memberId = ToToken(memberType);
            string locationId = ToToken(locationType);

            context.SetValue(DialoguePlaceholderKeys.OutingMemberId, memberId);
            context.SetValue(DialoguePlaceholderKeys.OutingMemberName, GetMemberDisplayName(memberType));
            context.SetValue(DialoguePlaceholderKeys.OutingLocationId, locationId);
            context.SetValue(DialoguePlaceholderKeys.OutingLocationName, GetLocationDisplayName(locationType));

            context.SetValue(DialoguePlaceholderKeys.OutingMemberTypeAlias, memberType.ToString());
            context.SetValue(DialoguePlaceholderKeys.OutingLocationTypeAlias, locationType.ToString());
        }

        public static void RegisterCommon(DialogueVariableContext context)
        {
            if (context == null)
            {
                return;
            }

            RegisterAllStatPlaceholders(context);
            RegisterMainStatPlaceholders(context);
            RegisterGoldPlaceholders(context);
            RegisterConditionPlaceholders(context);
        }

        private static void RegisterAllStatPlaceholders(DialogueVariableContext context)
        {
            MemberType[] memberTypes = (MemberType[])Enum.GetValues(typeof(MemberType));
            StatType[] statTypes = (StatType[])Enum.GetValues(typeof(StatType));

            foreach (MemberType memberType in memberTypes)
            {
                foreach (StatType statType in statTypes)
                {
                    MemberType capturedMember = memberType;
                    StatType capturedStat = statType;

                    RegisterStatSet(context, ToToken(capturedMember), ToToken(capturedStat), capturedMember, capturedStat);

                    // Backward compatibility for existing authored keys.
                    RegisterStatSet(context, capturedMember.ToString(), capturedStat.ToString(), capturedMember, capturedStat);
                }
            }
        }

        private static void RegisterStatSet(
            DialogueVariableContext context,
            string memberToken,
            string statToken,
            MemberType memberType,
            StatType statType)
        {
            string keyDelta = string.Format(DialoguePlaceholderKeys.StatFormat, memberToken, statToken, "delta");

            context.SetGetter(keyDelta, () => DialogueSessionState.GetStatDelta(memberType, statType));
        }

        private static void RegisterMainStatPlaceholders(DialogueVariableContext context)
        {
            context.SetGetter(DialoguePlaceholderKeys.StatMainDelta, () => GetMainStatDelta());
        }

        private static void RegisterGoldPlaceholders(DialogueVariableContext context)
        {
            context.SetGetter(DialoguePlaceholderKeys.GoldValue, () => GetCurrentGoldValue());
            context.SetGetter(DialoguePlaceholderKeys.GoldDelta, () => DialogueSessionState.GoldDelta);
            context.SetGetter(DialoguePlaceholderKeys.GoldPreview, () => GetCurrentGoldValue() + DialogueSessionState.GoldDelta);
        }

        private static void RegisterConditionPlaceholders(DialogueVariableContext context)
        {
            MemberType[] memberTypes = (MemberType[])Enum.GetValues(typeof(MemberType));

            foreach (MemberType memberType in memberTypes)
            {
                MemberType capturedMember = memberType;
                string memberToken = ToToken(capturedMember);

                string keyValue = string.Format(DialoguePlaceholderKeys.ConditionFormat, memberToken, "value");
                string keyDelta = string.Format(DialoguePlaceholderKeys.ConditionFormat, memberToken, "delta");
                string keyPreview = string.Format(DialoguePlaceholderKeys.ConditionFormat, memberToken, "preview");
                string keyTrend = string.Format(DialoguePlaceholderKeys.ConditionFormat, memberToken, "trend");
                string keySummary = string.Format(DialoguePlaceholderKeys.ConditionFormat, memberToken, "summary");

                context.SetGetter(keyValue, () => GetCurrentConditionDisplay(capturedMember));
                context.SetGetter(keyDelta, () => DialogueSessionState.GetConditionDelta(capturedMember));
                context.SetGetter(keyPreview, () => GetPreviewConditionDisplay(capturedMember));
                context.SetGetter(keyTrend, () => GetConditionTrendText(capturedMember));
                context.SetGetter(keySummary, () => $"{GetPreviewConditionDisplay(capturedMember)}까지 {GetConditionTrendText(capturedMember)}");
            }
        }
        
        private static int GetMainStatDelta()
        {
            return TrySelectMainStat(out _, out _, out int delta) ? delta : 0;
        }
        
        private static bool TrySelectMainStat(out MemberType selectedMemberType, out StatType selectedStatType, out int selectedDelta)
        {
            selectedMemberType = default;
            selectedStatType = default;
            selectedDelta = 0;

            var deltas = DialogueSessionState.GetAllStatDeltas();
            if (deltas == null || deltas.Count == 0)
            {
                return false;
            }

            bool hasSelection = false;
            int bestAbsDelta = -1;
            int bestPriority = int.MaxValue;

            foreach (var pair in deltas)
            {
                int absDelta = Math.Abs(pair.Value);
                int priority = GetMainStatPriority(pair.Key.statType);

                if (!hasSelection
                    || absDelta > bestAbsDelta
                    || (absDelta == bestAbsDelta && priority < bestPriority)
                    || (absDelta == bestAbsDelta && priority == bestPriority && (int)pair.Key.statType < (int)selectedStatType)
                    || (absDelta == bestAbsDelta && priority == bestPriority && pair.Key.statType == selectedStatType && (int)pair.Key.memberType < (int)selectedMemberType))
                {
                    hasSelection = true;
                    selectedMemberType = pair.Key.memberType;
                    selectedStatType = pair.Key.statType;
                    selectedDelta = pair.Value;
                    bestAbsDelta = absDelta;
                    bestPriority = priority;
                }
            }

            return hasSelection;
        }

        private static int GetMainStatPriority(StatType statType)
        {
            for (int i = 0; i < MAIN_STAT_PRIORITY.Length; i++)
            {
                if (MAIN_STAT_PRIORITY[i] == statType)
                {
                    return i;
                }
            }

            return int.MaxValue;
        }
        
        private static int GetCurrentGoldValue()
        {
            return BandSupplyManager.Instance != null ? BandSupplyManager.Instance.BandFunds : 0;
        }

        private static string GetCurrentConditionDisplay(MemberType memberType)
        {
            MemberConditionMode mode = GetCurrentConditionMode(memberType);
            return GetConditionDisplayName(mode);
        }

        private static string GetPreviewConditionDisplay(MemberType memberType)
        {
            int currentValue = (int)GetCurrentConditionMode(memberType);
            int previewValue = currentValue + DialogueSessionState.GetConditionDelta(memberType);

            int minValue = (int)MemberConditionMode.VeryGood;
            int maxValue = (int)MemberConditionMode.VeryBad;
            previewValue = Mathf.Clamp(previewValue, minValue, maxValue);

            MemberConditionMode previewMode = (MemberConditionMode)previewValue;
            return GetConditionDisplayName(previewMode);
        }

        private static string GetConditionTrendText(MemberType memberType)
        {
            int currentValue = (int)GetCurrentConditionMode(memberType);
            int previewValue = currentValue + DialogueSessionState.GetConditionDelta(memberType);

            int minValue = (int)MemberConditionMode.VeryGood;
            int maxValue = (int)MemberConditionMode.VeryBad;
            previewValue = Mathf.Clamp(previewValue, minValue, maxValue);

            if (previewValue < currentValue)
            {
                return DialoguePlaceholderKeys.ConditionTrendUp;
            }

            if (previewValue > currentValue)
            {
                return DialoguePlaceholderKeys.ConditionTrendDown;
            }

            return DialoguePlaceholderKeys.ConditionTrendStay;
        }

        private static MemberConditionMode GetCurrentConditionMode(MemberType memberType)
        {
            if (MemberConditionManager.Instance == null)
            {
                return MemberConditionMode.Commonly;
            }

            return MemberConditionManager.Instance.GetCondition(memberType);
        }

        private static string GetConditionDisplayName(MemberConditionMode mode)
        {
            switch (mode)
            {
                case MemberConditionMode.VeryGood:
                    return "최상";
                case MemberConditionMode.Good:
                    return "좋음";
                case MemberConditionMode.Commonly:
                    return "보통";
                case MemberConditionMode.Bad:
                    return "나쁨";
                case MemberConditionMode.VeryBad:
                    return "최악";
                default:
                    return mode.ToString();
            }
        }

        private static string ToToken(MemberType memberType)
        {
            return memberType.ToString().ToLowerInvariant();
        }

        private static string ToToken(StatType statType)
        {
            return statType.ToString().ToLowerInvariant();
        }

        private static string ToToken(LocationType locationType)
        {
            return locationType.ToString().ToLowerInvariant();
        }

        private static string GetMemberDisplayName(MemberType memberType)
        {
            switch (memberType)
            {
                case MemberType.Guitar:
                    return "기타";
                case MemberType.Drums:
                    return "드럼";
                case MemberType.Bass:
                    return "베이스";
                case MemberType.Vocal:
                    return "보컬";
                case MemberType.Piano:
                    return "키보드";
                default:
                    return memberType.ToString();
            }
        }

        private static string GetLocationDisplayName(LocationType locationType)
        {
            switch (locationType)
            {
                case LocationType.Downtown:
                    return "번화가";
                case LocationType.Park:
                    return "공원";
                case LocationType.AcademyDistrict:
                    return "학원가";
                case LocationType.LiveHouse:
                    return "라이브하우스";
                case LocationType.MusicStore:
                    return "악기점";
                default:
                    return locationType.ToString();
            }
        }
    }
}
