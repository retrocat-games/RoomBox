using System.Collections.Generic;
using UnityEngine;
using UniRx;

namespace RetroCat.Modules.RoomBox
{
    public class StickerInventory : MonoBehaviour
    {
        [SerializeField] private List<StickerData> _initialStickers = new List<StickerData>();

        public ReactiveCollection<StickerData> Stickers { get; } = new ReactiveCollection<StickerData>();

        private void Awake()
        {
            foreach (var sticker in _initialStickers)
            {
                Stickers.Add(sticker);
            }
        }

        public StickerData GetSticker(int index)
        {
            if (index < 0 || index >= Stickers.Count)
                return null;
            return Stickers[index];
        }

        public void AddSticker(StickerData sticker)
        {
            if (sticker != null)
                Stickers.Add(sticker);
        }

        public void RemoveSticker(StickerData sticker)
        {
            if (sticker != null)
                Stickers.Remove(sticker);
        }
    }
}