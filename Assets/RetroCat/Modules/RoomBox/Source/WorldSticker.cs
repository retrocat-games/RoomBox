using System;
using System.Collections;
using UnityEngine;

namespace RetroCat.Modules.RoomBox
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class WorldSticker : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private float _revealDuration = 0.5f;

        private static readonly int RevealId = Shader.PropertyToID("_Reveal");
        private Coroutine _revealRoutine;

        private void Awake()
        {
            if (_spriteRenderer == null)
            {
                _spriteRenderer = GetComponent<SpriteRenderer>();
            }

            if (_spriteRenderer != null)
            {
                _spriteRenderer.material = Instantiate(_spriteRenderer.material);
                _spriteRenderer.material.SetFloat(RevealId, 0f);
            }
        }

        public void Initialize(StickerData data, int sortingOrder)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            _spriteRenderer.sprite = data.Sprite;
            _spriteRenderer.sortingOrder = sortingOrder;
            gameObject.name = data.name;

            PlayReveal();
        }

        private void OnDestroy()
        {
            if (_spriteRenderer != null)
            {
                Destroy(_spriteRenderer.material);
            }
        }

        private void PlayReveal()
        {
            if (_revealRoutine != null)
            {
                StopCoroutine(_revealRoutine);
            }
            _revealRoutine = StartCoroutine(RevealRoutine());
        }

        private IEnumerator RevealRoutine()
        {
            float t = 0f;
            while (t < _revealDuration)
            {
                t += Time.deltaTime;
                float value = Mathf.Clamp01(t / _revealDuration);
                _spriteRenderer.material.SetFloat(RevealId, value);
                yield return null;
            }

            _spriteRenderer.material.SetFloat(RevealId, 1f);
        }
    }
}
