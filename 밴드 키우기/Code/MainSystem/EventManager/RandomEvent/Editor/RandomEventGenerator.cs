#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Code.MainSystem.EventManager.RandomEvent.Editor
{
    public static class RandomEventGenerator
    {
        private const string SavePath = "Assets\\_Modules\\RandomEvent";

        private struct EventTemplate
        {
            public string id;
            public RandomEventType type;
            public string message;
            public float weight;
            public List<RandomEventEffect> effects;

            public EventTemplate(string id, RandomEventType type, string message, float weight,
                params (RandomEventEffectType t, int amt)[] effs)
            {
                this.id = id;
                this.type = type;
                this.message = message;
                this.weight = weight;
                effects = new List<RandomEventEffect>();
                foreach (var e in effs)
                    effects.Add(new RandomEventEffect { type = e.t, amount = e.amt });
            }
        }

        [MenuItem("Tools/Generate/Random Events")]
        public static void Generate()
        {
            if (!Directory.Exists(SavePath))
                Directory.CreateDirectory(SavePath);

            var templates = GetTemplates();
            int created = 0;

            foreach (var t in templates)
            {
                string assetPath = $"{SavePath}/Event_{t.id}.asset";
                if (File.Exists(assetPath)) continue;

                var so = ScriptableObject.CreateInstance<RandomEventDataSO>();
                so.eventId = t.id;
                so.type    = t.type;
                so.message = t.message;
                so.weight  = t.weight;
                so.effects = t.effects;

                AssetDatabase.CreateAsset(so, assetPath);
                created++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[RandomEventGenerator] {created}개 생성 완료 (총 {templates.Count}개 중)");
        }

        private static List<EventTemplate> GetTemplates()
        {
            return new List<EventTemplate>
            {
                // ===== 긍정 이벤트 =====
                new("FanSnack", RandomEventType.Good,
                    "익명의 팬이 간식을 잔뜩 보내왔다!\n멤버들의 기분이 좋아진다.",
                    3f, (RandomEventEffectType.AddConditionAll, 1)),

                new("ViralPost", RandomEventType.Good,
                    "SNS에 올린 짤이 바이럴됐다!\n새로운 팬들이 유입된다.",
                    2f, (RandomEventEffectType.AddFans, 100)),

                new("StreetCasting", RandomEventType.Good,
                    "길거리에서 캐스팅 제안을 받았다.\n관심도가 올라간다.",
                    1.5f, (RandomEventEffectType.AddFans, 50)),

                new("Sponsorship", RandomEventType.Good,
                    "작은 브랜드에서 협찬 제안이 들어왔다!\n자금이 들어온다.",
                    2f, (RandomEventEffectType.AddFunds, 500)),

                new("RadioGuest", RandomEventType.Good,
                    "라디오 게스트로 출연했다.\n출연료와 함께 인지도도 올랐다.",
                    1.5f, (RandomEventEffectType.AddFunds, 200), (RandomEventEffectType.AddFans, 30)),

                new("FreePractice", RandomEventType.Good,
                    "단골 카페 사장님이 연습실을 무료로 빌려줬다.\n자금이 절약됐다.",
                    1.5f, (RandomEventEffectType.AddFunds, 300)),

                new("Inspiration", RandomEventType.Good,
                    "멤버 한 명이 영감을 받았다!\n경험치가 쌓인다.",
                    2f, (RandomEventEffectType.AddExp, 30)),

                new("PDNoticed", RandomEventType.Good,
                    "방송국 PD의 눈에 띄었다.\n좋은 기회가 될 것 같다.",
                    1f, (RandomEventEffectType.AddFans, 80), (RandomEventEffectType.AddExp, 20)),

                new("LotteryWin", RandomEventType.Good,
                    "리더가 복권에 당첨됐다!\n팀에 한턱 쐈다.",
                    0.3f, (RandomEventEffectType.AddFunds, 1500)),

                new("FanLetter", RandomEventType.Good,
                    "팬에게서 따뜻한 손편지가 도착했다.\n멤버들이 감동했다.",
                    2.5f, (RandomEventEffectType.AddConditionAll, 1)),

                new("BigDeal", RandomEventType.Good,
                    "대형 협찬 계약이 성사됐다!\n자금이 두둑해진다.",
                    0.2f, (RandomEventEffectType.AddFunds, 2000), (RandomEventEffectType.AddFans, 50)),

                new("MusicAward", RandomEventType.Good,
                    "지난달 곡이 작은 음악상을 받았다.\n인지도가 상승한다.",
                    0.5f, (RandomEventEffectType.AddFans, 150), (RandomEventEffectType.AddExp, 50)),

                new("HealthyMeal", RandomEventType.Good,
                    "이번 달 식단을 잘 챙겼다.\n전원 컨디션이 좋아진다.",
                    2f, (RandomEventEffectType.AddConditionAll, 1)),

                new("OldFanReturn", RandomEventType.Good,
                    "옛 팬들이 다시 돌아왔다!\n잊지 않고 있었다고 한다.",
                    1f, (RandomEventEffectType.AddFans, 60)),

                new("SurpriseBonus", RandomEventType.Good,
                    "예상치 못한 정산금이 들어왔다.\n자금이 늘었다.",
                    1.5f, (RandomEventEffectType.AddFunds, 400)),

                // ===== 부정 이벤트 =====
                new("CommonCold", RandomEventType.Bad,
                    "환절기 감기가 돌고 있다.\n멤버들의 컨디션이 나빠진다.",
                    3f, (RandomEventEffectType.AddConditionAll, -1)),

                new("BrokenGuitar", RandomEventType.Bad,
                    "기타 줄이 끊어지고 픽업이 고장났다.\n수리비가 빠져나간다.",
                    2f, (RandomEventEffectType.AddFunds, -300)),

                new("Hateful", RandomEventType.Bad,
                    "악플이 쏟아졌다.\n팬이 일부 떨어져 나갔다.",
                    2f, (RandomEventEffectType.AddFans, -50), (RandomEventEffectType.AddConditionAll, -1)),

                new("WaterLeak", RandomEventType.Bad,
                    "연습실 천장에 누수가 생겼다.\n수리비가 든다.",
                    1f, (RandomEventEffectType.AddFunds, -400)),

                new("ScheduleConflict", RandomEventType.Bad,
                    "매니저가 일정을 중복으로 잡았다.\n멤버들이 지쳤다.",
                    1.5f, (RandomEventEffectType.AddConditionAll, -1)),

                new("EquipmentTheft", RandomEventType.Bad,
                    "장비 일부를 도난당했다.\n새로 구입해야 한다.",
                    0.3f, (RandomEventEffectType.AddFunds, -800)),

                new("AntiFan", RandomEventType.Bad,
                    "안티팬이 생겼다.\n분위기가 어수선하다.",
                    1.5f, (RandomEventEffectType.AddFans, -30)),

                new("Overwork", RandomEventType.Bad,
                    "스케줄을 너무 빡빡하게 돌렸다.\n전원 번아웃 직전이다.",
                    1.5f, (RandomEventEffectType.AddConditionAll, -2)),

                new("RumorSpread", RandomEventType.Bad,
                    "근거 없는 루머가 퍼졌다.\n팬 일부가 동요한다.",
                    1f, (RandomEventEffectType.AddFans, -40)),

                new("InstrumentExpense", RandomEventType.Bad,
                    "악기 정기 점검 비용이 청구됐다.",
                    2f, (RandomEventEffectType.AddFunds, -200)),

                new("MemberArgue", RandomEventType.Bad,
                    "멤버끼리 사소한 다툼이 있었다.\n분위기가 어색하다.",
                    1.5f, (RandomEventEffectType.AddConditionAll, -1)),

                new("MissedDeadline", RandomEventType.Bad,
                    "곡 작업 마감을 놓쳤다.\n자신감이 떨어진다.",
                    1f, (RandomEventEffectType.AddExp, -20), (RandomEventEffectType.AddConditionAll, -1)),

                new("BigScandal", RandomEventType.Bad,
                    "큰 논란에 휘말렸다.\n팬덤이 흔들린다.",
                    0.2f, (RandomEventEffectType.AddFans, -200), (RandomEventEffectType.AddConditionAll, -2)),

                new("FoodPoisoning", RandomEventType.Bad,
                    "회식에서 먹은 음식이 잘못됐다.\n전원이 앓아누웠다.",
                    0.5f, (RandomEventEffectType.AddConditionAll, -2)),

                new("PracticeRoomNoise", RandomEventType.Bad,
                    "연습실 층간소음 항의가 들어왔다.\n합의금을 물어줬다.",
                    1.5f, (RandomEventEffectType.AddFunds, -250)),
            };
        }
    }
}
#endif