using UnityEngine;

namespace RetroCat.Modules.RoomBox
{
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
            if (sticker.Prefab != null)
            {
                _draggingObject = Instantiate(sticker.Prefab);
            }
            else
            {
                _draggingObject = new GameObject(sticker.name + "_drag");
                var renderer = _draggingObject.AddComponent<SpriteRenderer>();
                renderer.sprite = sticker.Sprite;
            }
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