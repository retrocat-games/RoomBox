using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RetroCat.Modules.RoomBox
{
    public class StickerItemUI : MonoBehaviour, IPointerDownHandler
    {
        [SerializeField] private Image icon;
        private StickerData _data;
        private StickerInventoryUI _inventoryUI;

        public void Setup(StickerInventoryUI ui, StickerData data)
        {
            _inventoryUI = ui;
            _data = data;
            if (icon != null)
                icon.sprite = data.Sprite;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _inventoryUI.UseSticker(_data, this);
        }
    }
}
