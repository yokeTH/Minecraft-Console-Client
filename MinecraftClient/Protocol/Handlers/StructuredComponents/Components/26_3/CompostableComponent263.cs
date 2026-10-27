using System.Collections.Generic;
using MinecraftClient.Inventory.ItemPalettes;
using MinecraftClient.Protocol.Handlers.StructuredComponents.Core;

namespace MinecraftClient.Protocol.Handlers.StructuredComponents.Components._26_3;

/// <summary>
/// 26.3+ compostable: ResolvableInt layers.
/// </summary>
public class CompostableComponent263(DataTypes dataTypes, ItemPalette itemPalette, SubComponentRegistry subComponentRegistry)
    : StructuredComponent(dataTypes, itemPalette, subComponentRegistry)
{
    public ResolvableIntData Layers { get; set; } = new(0, null);

    public override void Parse(Queue<byte> data)
    {
        Layers = StructuredComponentCodecHelpers.ReadResolvableInt(DataTypes, data);
    }

    public override Queue<byte> Serialize()
    {
        var data = new List<byte>();
        StructuredComponentCodecHelpers.WriteResolvableInt(DataTypes, data, Layers);
        return new Queue<byte>(data);
    }
}
