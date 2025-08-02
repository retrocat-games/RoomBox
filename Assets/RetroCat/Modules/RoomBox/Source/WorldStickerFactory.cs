using UnityEngine;

namespace RetroCat.Modules.RoomBox
{
    public interface IWorldStickerFactory
    {
        WorldSticker Create(Vector3 position, StickerData data, int sortingOrder, Transform parent = null);
    }

    public class WorldStickerFactory : IWorldStickerFactory
    {
        private readonly WorldSticker _prefab;

        public WorldStickerFactory(WorldSticker prefab)
        {
            _prefab = prefab;
        }

        public WorldSticker Create(Vector3 position, StickerData data, int sortingOrder, Transform parent = null)
        {
            var instance = Object.Instantiate(_prefab, position, Quaternion.identity, parent);
            instance.Initialize(data, sortingOrder);
            return instance;
        }
    }
}
