using UnityEngine;
using Reflex.Core;

namespace RetroCat.Modules.RoomBox
{
    public class RoomBoxInstaller : MonoBehaviour, IInstaller
    {
        [SerializeField] private WorldSticker _worldStickerPrefab;
        [SerializeField] private StickerInventory _inventory;

        public void InstallBindings(ContainerBuilder containerBuilder)
        {
            containerBuilder.AddSingleton<IWorldStickerFactory>(new WorldStickerFactory(_worldStickerPrefab));
            containerBuilder.AddSingleton(_inventory);
        }
    }
}
