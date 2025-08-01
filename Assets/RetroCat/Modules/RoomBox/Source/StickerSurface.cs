using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RetroCat.Modules.RoomBox
{
    [RequireComponent(typeof(Collider2D))]
    public class StickerSurface : MonoBehaviour
    {
        [SerializeField] private StickerId id;
        private readonly List<StickerInstance> _placed = new List<StickerInstance>();

        public bool CanPlaceSticker(StickerData data)
        {
            return data != null && data.BanSurfaces.Contains(id) == false;
        }

        public void PlaceSticker(StickerData data, Vector2 localPosition)
        {
            if (!CanPlaceSticker(data))
                return;

            var obj = new GameObject(data.name);
            obj.transform.SetParent(transform, false);
            obj.transform.localPosition = localPosition;
            var renderer = obj.AddComponent<SpriteRenderer>();
            renderer.sprite = data.Sprite;

            // Ensure newly placed stickers appear on top of previously placed ones
            int sortingOrder = _placed.Count > 0
                ? _placed.Max(p => p.gameObject.GetComponent<SpriteRenderer>().sortingOrder) + 1
                : 0;
            renderer.sortingOrder = sortingOrder;

            _placed.Add(new StickerInstance { sticker = data, gameObject = obj });
        }
    }
}