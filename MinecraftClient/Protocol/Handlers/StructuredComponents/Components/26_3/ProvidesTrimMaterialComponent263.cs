using System.Collections.Generic;
using MinecraftClient.Inventory.ItemPalettes;
using MinecraftClient.Protocol.Handlers.StructuredComponents.Core;

namespace MinecraftClient.Protocol.Handlers.StructuredComponents.Components._26_3;

public class ProvidesTrimMaterialComponent263(DataTypes dataTypes, ItemPalette itemPalette, SubComponentRegistry subComponentRegistry)
    : StructuredComponent(dataTypes, itemPalette, subComponentRegistry)
{
    public int HolderId { get; set; }

    public override void Parse(Queue<byte> data)
    {
        HolderId = DataTypes.ReadNextVarInt(data);
        if (HolderId != 0)
            return;

        DataTypes.ReadNextString(data); // Identifier paletteId
        DataTypes.ReadNextNbt(data); // ComponentSerialization.STREAM_CODEC
    }

    public override Queue<byte> Serialize()
    {
        var bytes = new List<byte>();
        bytes.AddRange(DataTypes.GetVarInt(HolderId));
        return new Queue<byte>(bytes);
    }
}
