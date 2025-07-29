using UnityEngine;
using UnityEngine.UI;

namespace RetroCat.Modules.RoomBox
{
    public class StickerInventoryView : MonoBehaviour
    {
        [SerializeField] private StickerInventory inventory;
        [SerializeField] private StickerPlacer placer;
        [SerializeField] private RectTransform content;
        [SerializeField] private Vector2 itemSize = new Vector2(100f, 100f);

        private void Start()
        {
            Load();
        }

        public void Load()
        {
            if (inventory == null || content == null)
                return;

            for (int i = 0; i < inventory.stickers.Count; i++)
            {
                var sticker = inventory.stickers[i];
                var item = new GameObject($"StickerItem_{i}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
                var rect = item.GetComponent<RectTransform>();
                rect.SetParent(content, false);
                rect.sizeDelta = itemSize;
                rect.anchorMin = new Vector2(0, 1);
                rect.anchorMax = new Vector2(0, 1);
                rect.pivot = new Vector2(0, 1);
                rect.anchoredPosition = new Vector2(i * itemSize.x, 0f);

                var image = item.GetComponent<Image>();
                image.sprite = sticker.Sprite;

                var button = item.GetComponent<Button>();
                button.targetGraphic = image;
                var data = sticker;
                button.onClick.AddListener(() =>
                {
                    if (placer != null)
                        placer.StartDrag(data);
                });
            }

            var size = content.sizeDelta;
            size.x = inventory.stickers.Count * itemSize.x;
            content.sizeDelta = size;
        }
    }
}
