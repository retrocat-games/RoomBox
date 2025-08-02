using System;
using UnityEngine;

namespace RetroCat.Modules.RoomBox
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class WorldSticker : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;

        private void Awake()
        {
            if (_spriteRenderer == null)
            {
                _spriteRenderer = GetComponent<SpriteRenderer>();
            }
        }

        public void Initialize(StickerData data, int sortingOrder)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            _spriteRenderer.sprite = data.Sprite;
            _spriteRenderer.sortingOrder = sortingOrder;
            gameObject.name = data.name;
        }
    }
}
