using UnityEngine;
using UnityEngine.UI;

namespace RetroCat.Modules.RoomBox
{
    public class StickerInventoryUI : MonoBehaviour
    {
        [SerializeField] private StickerInventory inventory;
        [SerializeField] private StickerPlacer placer;
        [SerializeField] private Transform content;
        [SerializeField] private GameObject itemPrefab;

        private void Start()
        {
            Refresh();
        }

        public void Refresh()
        {
            foreach (Transform child in content)
                Destroy(child.gameObject);

            foreach (var sticker in inventory.stickers)
            {
                var item = Instantiate(itemPrefab, content);
                var entry = item.GetComponent<StickerItemUI>();
                if (entry != null)
                    entry.Setup(this, sticker);
            }
        }

        public void UseSticker(StickerData data, StickerItemUI item)
        {
            if (placer != null)
                placer.StartDrag(data);
            inventory.RemoveSticker(data);
            Destroy(item.gameObject);
        }
    }
}
