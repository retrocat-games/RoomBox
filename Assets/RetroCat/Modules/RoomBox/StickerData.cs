using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace RetroCat.Modules.RoomBox
{
    [CreateAssetMenu(fileName = "StickerData", menuName = "RoomBox/Sticker")]
    public class StickerData : ScriptableObject
    {
        [SerializeField] private Sprite _sprite;
        [SerializeField] private StickerOrientation _orientation;
        [SerializeField] private List<StickerId> _banSurfaces = new List<StickerId>();
        
        public Sprite Sprite => _sprite;
        public StickerOrientation Orientation => _orientation;
        public IReadOnlyList<StickerId> BanSurfaces => _banSurfaces;
    }
}