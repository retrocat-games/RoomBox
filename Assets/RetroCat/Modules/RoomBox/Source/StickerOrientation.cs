using System;

namespace RetroCat.Modules.RoomBox
{
    [Flags]
    public enum StickerOrientation : uint
    {
        Horizontal = 0x01,
        Vertical = 0x10
    }
}