using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Reflex.Attributes;

namespace RetroCat.Modules.RoomBox
{
    [RequireComponent(typeof(Collider2D))]
    public class StickerSurface : MonoBehaviour
    {
        [SerializeField] private StickerId id;
        private readonly List<StickerInstance> _placed = new List<StickerInstance>();
        [Inject] private IWorldStickerFactory _worldStickerFactory;

        public bool CanPlaceSticker(StickerData data)
        {
            return data != null && data.BanSurfaces.Contains(id) == false;
        }

        public void PlaceSticker(StickerData data, Vector2 localPosition)
        {
            if (!CanPlaceSticker(data))
                return;

            int sortingOrder = _placed.Count > 0
                ? _placed.Max(p => p.gameObject.GetComponent<SpriteRenderer>().sortingOrder) + 1
                : 0;

            var worldPos = transform.TransformPoint(localPosition);
            var instance = _worldStickerFactory.Create(worldPos, data, sortingOrder, transform);

            _placed.Add(new StickerInstance { sticker = data, gameObject = instance.gameObject });
        }
    }
}