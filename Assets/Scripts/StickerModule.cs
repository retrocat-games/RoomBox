using System.Collections.Generic;
using UnityEngine;

namespace RoomBox
{
    public enum StickerOrientation
    {
        Horizontal,
        Vertical
    }

    public enum StickerSurfaceType
    {
        Floor,
        Wall,
        Table
    }

    [CreateAssetMenu(fileName = "StickerData", menuName = "RoomBox/Sticker")]
    public class StickerData : ScriptableObject
    {
        public Sprite sprite;
        public StickerOrientation orientation;
        public List<StickerSurfaceType> allowedSurfaces = new List<StickerSurfaceType>();
    }

    public class StickerInstance
    {
        public StickerData sticker;
        public GameObject gameObject;
    }

    [RequireComponent(typeof(Collider2D))]
    public class StickerSurface : MonoBehaviour
    {
        public StickerSurfaceType surfaceType;
        private readonly List<StickerInstance> _placed = new List<StickerInstance>();

        public bool CanPlaceSticker(StickerData data)
        {
            return data != null && data.allowedSurfaces.Contains(surfaceType);
        }

        public void PlaceSticker(StickerData data, Vector2 localPosition)
        {
            if (!CanPlaceSticker(data))
                return;

            var obj = new GameObject(data.name);
            obj.transform.SetParent(transform, false);
            obj.transform.localPosition = localPosition;
            var renderer = obj.AddComponent<SpriteRenderer>();
            renderer.sprite = data.sprite;
            _placed.Add(new StickerInstance { sticker = data, gameObject = obj });
        }
    }

    public class StickerInventory : MonoBehaviour
    {
        public List<StickerData> stickers = new List<StickerData>();

        public StickerData GetSticker(int index)
        {
            if (index < 0 || index >= stickers.Count)
                return null;
            return stickers[index];
        }
    }

    public class StickerPlacer : MonoBehaviour
    {
        public Camera mainCamera;
        private StickerData _draggingSticker;
        private GameObject _draggingObject;

        public void StartDrag(StickerData sticker)
        {
            if (sticker == null)
                return;
            _draggingSticker = sticker;
            _draggingObject = new GameObject(sticker.name + "_drag");
            var renderer = _draggingObject.AddComponent<SpriteRenderer>();
            renderer.sprite = sticker.sprite;
        }

        private void Update()
        {
            if (_draggingSticker == null || _draggingObject == null)
                return;

            var mousePos = Input.mousePosition;
            var world = mainCamera.ScreenToWorldPoint(mousePos);
            world.z = 0f;
            _draggingObject.transform.position = world;

            if (Input.GetMouseButtonUp(0))
            {
                var hit = Physics2D.Raycast(world, Vector2.zero);
                if (hit.collider != null)
                {
                    var surface = hit.collider.GetComponent<StickerSurface>();
                    if (surface != null && surface.CanPlaceSticker(_draggingSticker))
                    {
                        var local = surface.transform.InverseTransformPoint(world);
                        surface.PlaceSticker(_draggingSticker, local);
                    }
                }
                Destroy(_draggingObject);
                _draggingSticker = null;
                _draggingObject = null;
            }
        }
    }
}
