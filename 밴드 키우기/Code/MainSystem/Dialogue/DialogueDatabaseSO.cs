using System;
using System.Collections.Generic;
using Member.LS.Code.Dialogue.Character;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Code.MainSystem.Dialogue
{
    /// <summary>
    /// ID 기반 리소스 조회를 지원하는 다이알로그 에셋 데이터베이스
    /// </summary>
    [CreateAssetMenu(fileName = "DialogueDatabase", menuName = "SO/Dialogue/Database", order = 21)]
    public class DialogueDatabaseSO : ScriptableObject
    {
        /// <summary>
        /// 캐릭터 정보 에셋 항목 구조체
        /// </summary>
        [Serializable]
        public struct CharacterEntry
        {
            /// <summary>
            /// 캐릭터 식별자
            /// </summary>
            public string Id;
            /// <summary>
            /// 실제 캐릭터 정보 에셋 참조
            /// </summary>
            public AssetReferenceT<CharacterInformationSO> CharacterReference;
        }

        /// <summary>
        /// 배경 스프라이트 에셋 항목 구조체
        /// </summary>
        [Serializable]
        public struct BackgroundEntry
        {
            /// <summary>
            /// 배경 식별자
            /// </summary>
            public string Id;
            /// <summary>
            /// 실제 배경 스프라이트 참조
            /// </summary>
            public AssetReferenceSprite BackgroundReference;
        }

        /// <summary>
        /// 등록된 캐릭터 목록
        /// </summary>
        [SerializeField] private List<CharacterEntry> characterEntries;

        /// <summary>
        /// 등록된 배경 목록
        /// </summary>
        [SerializeField] private List<BackgroundEntry> backgroundEntries;

        /// <summary>
        /// ID를 통해 캐릭터 정보 에셋 참조를 반환
        /// </summary>
        /// <param name="id">조회할 캐릭터 식별자</param>
        public AssetReferenceT<CharacterInformationSO> GetCharacter(string id)
        {
            TryGetCharacter(id, out AssetReferenceT<CharacterInformationSO> characterReference);
            return characterReference;
        }

        /// <summary>
        /// ID를 통해 캐릭터 정보 에셋 참조를 안전하게 조회
        /// </summary>
        /// <param name="id">조회할 캐릭터 식별자</param>
        /// <param name="characterReference">조회된 캐릭터 참조</param>
        /// <returns>조회 성공 여부</returns>
        public bool TryGetCharacter(string id, out AssetReferenceT<CharacterInformationSO> characterReference)
        {
            characterReference = null;

            if (string.IsNullOrWhiteSpace(id))
            {
                return false;
            }

            int index = characterEntries.FindIndex(entry => entry.Id == id);
            if (index < 0)
            {
                return false;
            }

            characterReference = characterEntries[index].CharacterReference;
            return characterReference != null && characterReference.RuntimeKeyIsValid();
        }

        /// <summary>
        /// ID를 통해 배경 스프라이트 에셋 참조를 반환
        /// </summary>
        /// <param name="id">조회할 배경 식별자</param>
        public AssetReferenceSprite GetBackground(string id)
        {
            TryGetBackground(id, out AssetReferenceSprite backgroundReference);
            return backgroundReference;
        }

        /// <summary>
        /// ID를 통해 배경 스프라이트 에셋 참조를 안전하게 조회
        /// </summary>
        /// <param name="id">조회할 배경 식별자</param>
        /// <param name="backgroundReference">조회된 배경 참조</param>
        /// <returns>조회 성공 여부</returns>
        public bool TryGetBackground(string id, out AssetReferenceSprite backgroundReference)
        {
            backgroundReference = null;

            if (string.IsNullOrWhiteSpace(id))
            {
                return false;
            }

            int index = backgroundEntries.FindIndex(entry => entry.Id == id);
            if (index < 0)
            {
                return false;
            }

            backgroundReference = backgroundEntries[index].BackgroundReference;
            return backgroundReference != null && backgroundReference.RuntimeKeyIsValid();
        }
    }
}
