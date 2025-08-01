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
            _placed.Add(new StickerInstance { sticker = data, gameObject = obj });
        }
    }
}