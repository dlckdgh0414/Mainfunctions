using System.Collections.Generic;
using UnityEngine;

namespace Code.MainSystem.Song
{
    public enum MarketingQuality
    {
        None = 0,     // 안 만듦
        Standard = 1, // 적당히 만듦
        Premium = 2   // 제일 좋게 만듦
    }
    
    public class SongResultCalculator
    {
        private const float BASE_REVENUE = 0.45f; 
        private const float DIFFICULTY_SCALING = 1.15f; 
        
        public MusicReleaseResultData GetResultData(
            float lyrics, float melody, float harmony, float proficiency,
            int currentFans, int releaseCount, int gameYear, 
            float synergyScale, MarketingQuality thumbLevel, MarketingQuality pvLevel)
        {
            MusicReleaseResultData data = new MusicReleaseResultData();
            data.AverageStars = new List<float>();

            float totalStat = lyrics + melody + harmony + proficiency;
            float targetStat = 100f * Mathf.Pow(DIFFICULTY_SCALING, Mathf.Max(0, gameYear)) * (1 + releaseCount * 0.05f) * 2.5f;
            Debug.Log(totalStat);
            Debug.Log(targetStat);
            
            float sumOfStars = 0f;
            for (int i = 0; i < 4; i++)
            {
                // 계산기에는 결과의 다양성을 위해 랜덤 유지
                float rand = Random.Range(0.85f, 1.15f);
                int rand2 = Random.Range(-1, 2);
                float star = (totalStat / targetStat) * synergyScale * 5.0f * rand + rand2;
                star = Mathf.Clamp(star, 1.0f, 5.0f);
                data.AverageStars.Add(star);
                sumOfStars += star;
            }

            data.TotalScore = sumOfStars / 4.0f;

            // 1. 평점 영향력 강화 (2.8f -> 4.0f ~ 5.0f 정도로 상향)
            // 지수가 높을수록 5점일 때의 보너스가 기하급수적으로 늘어납니다.
            float ratingImpact = Mathf.Pow(data.TotalScore, 3.25f);

            float marketingBonus = GetThumbnailMultiplier(thumbLevel) * GetPVMultiplier(pvLevel);
            float variance = Random.Range(0.95f, 1.05f);

            // 2. 조회수 계산식에서 평점 비중 높이기
            // 기존: (currentFans * 0.25f) + (ratingImpact * 120f * ...)
            // 아래처럼 ratingImpact 뒤의 계수(120f -> 200f)를 높이면 평점의 영향이 더 커집니다.
            data.PlayCount = Mathf.RoundToInt(
                ((currentFans * 0.2f) + (ratingImpact * 1600f * (1 + releaseCount * 0.1f))) * marketingBonus * variance
             * 0.4f);

            // 3. 수익(EarnedMoney) 계산 (조회수가 평점 영향을 받으므로 자동 적용됨)
            data.EarnedMoney = Mathf.RoundToInt(data.PlayCount * BASE_REVENUE * 0.8f);

            // 4. 팬 유입량에도 평점 가중치 강화
            // (sumOfStars / 20.0f) 부분의 지수를 높여서 평점이 낮으면 팬이 거의 안 늘게 변경
            data.NewFans = Mathf.RoundToInt((data.PlayCount * 0.08f) * Mathf.Pow(data.TotalScore / 5.0f, 2.0f) * 0.4f);

            data.GetExp = Mathf.RoundToInt((totalStat * 0.15f) + (sumOfStars * 35f) * 0.8f);

            return data;
        }

        private float GetThumbnailMultiplier(MarketingQuality level) => level switch {
            MarketingQuality.Standard => 1.15f, MarketingQuality.Premium => 1.3f, _ => 1.0f
        };

        private float GetPVMultiplier(MarketingQuality level) => level switch {
            MarketingQuality.Standard => 1.35f, MarketingQuality.Premium => 1.75f, _ => 1.0f
        };
    }
}