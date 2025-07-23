using System.Collections.Generic;
using UnityEngine;

namespace RetroCat.Modules.RoomBox
{
    public class StickerInventory : MonoBehaviour
    {
        public List<StickerData> stickers = new List<StickerData>();

        public StickerData GetSticker(int index)
        {
            if (index < 0 || index >= stickers.Count)
                return null;
            return stickers[index];
        }

        public void RemoveSticker(StickerData data)
        {
            stickers.Remove(data);
        }
    }
}