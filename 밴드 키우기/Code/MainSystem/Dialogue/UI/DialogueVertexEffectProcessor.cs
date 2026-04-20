using System.Collections.Generic;
using Code.Core.Bus;
using Code.Core.Bus.GameEvents.DialogueEvents.Effects;
using TMPro;
using UnityEngine;

namespace Code.MainSystem.Dialogue.UI
{
    /// <summary>
    /// TMP 텍스트의 정점 데이터를 조작하여 Shrink 등의 연출 효과를 적용하는 클래스
    /// (Bus를 통해 정보를 수신하여 TextPresenter와의 의존성 제거)
    /// </summary>
    public class DialogueVertexEffectProcessor : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI dialogueText;
        private List<TextEffectData> _effects = new List<TextEffectData>();

        private void OnEnable()
        {
            Bus<TextEffectEvent>.OnEvent += OnTextEffect;
        }

        private void OnDisable()
        {
            Bus<TextEffectEvent>.OnEvent -= OnTextEffect;
        }

        private void OnTextEffect(TextEffectEvent evt)
        {
            _effects = evt.Effects;
            
            // 효과가 비어있으면 메쉬 초기화
            if (_effects == null || _effects.Count == 0)
            {
                if (dialogueText != null)
                {
                    dialogueText.ForceMeshUpdate();
                }
            }
        }

        private void LateUpdate()
        {
            if (_effects == null || _effects.Count == 0 || dialogueText == null) return;

            ApplyVertexEffects();
        }

        private void ApplyVertexEffects()
        {
            dialogueText.ForceMeshUpdate();
            TMP_TextInfo textInfo = dialogueText.textInfo;

            for (int i = 0; i < textInfo.characterCount; i++)
            {
                TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
                if (!charInfo.isVisible) continue;

                int stringIndex = charInfo.index;

                foreach (TextEffectData effect in _effects)
                {
                    if (stringIndex >= effect.StartIndex && stringIndex < effect.EndIndex)
                    {
                        if (effect.Type == TextEffectType.Shrink)
                        {
                            ApplyShrinkEffect(textInfo, i, stringIndex, effect);
                        }
                    }
                }
            }

            dialogueText.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);
        }

        private void ApplyShrinkEffect(TMP_TextInfo textInfo, int charVisibleIndex, int stringIndex, TextEffectData effect)
        {
            int materialIndex = textInfo.characterInfo[charVisibleIndex].materialReferenceIndex;
            int vertexIndex = textInfo.characterInfo[charVisibleIndex].vertexIndex;
            Vector3[] vertices = textInfo.meshInfo[materialIndex].vertices;

            // 하단 중앙점을 앵커로 설정 (베이스라인 정렬 유지)
            Vector3 anchorPoint = (vertices[vertexIndex + 0] + vertices[vertexIndex + 3]) / 2f;

            int length = effect.EndIndex - effect.StartIndex;
            float rangeProgress = length > 1 ? (float)(stringIndex - effect.StartIndex) / (length - 1) : 1f;
            float currentScale = Mathf.Lerp(1.0f, effect.Value, rangeProgress);

            for (int j = 0; j < 4; j++)
            {
                Vector3 origin = vertices[vertexIndex + j];
                vertices[vertexIndex + j] = anchorPoint + (origin - anchorPoint) * currentScale;
            }
        }
    }
}
