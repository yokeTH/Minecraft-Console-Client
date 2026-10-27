using System.Collections.Generic;
using MinecraftClient.Inventory.ItemPalettes;
using MinecraftClient.Protocol.Handlers.StructuredComponents.Core;

namespace MinecraftClient.Protocol.Handlers.StructuredComponents.Components._26_3;

/// <summary>
/// 26.3+ cooking_fuel and brewing_fuel: ResolvableInt amount (burn time or uses) + ResolvableFloat speed multiplier.
/// </summary>
public class FuelComponent263(DataTypes dataTypes, ItemPalette itemPalette, SubComponentRegistry subComponentRegistry)
    : StructuredComponent(dataTypes, itemPalette, subComponentRegistry)
{
    public ResolvableIntData Amount { get; set; } = new(0, null);
    public ResolvableFloatData SpeedMultiplier { get; set; } = new(1, null);

    public override void Parse(Queue<byte> data)
    {
        Amount = StructuredComponentCodecHelpers.ReadResolvableInt(DataTypes, data);
        SpeedMultiplier = StructuredComponentCodecHelpers.ReadResolvableFloat(DataTypes, data);
    }

    public override Queue<byte> Serialize()
    {
        var data = new List<byte>();
        StructuredComponentCodecHelpers.WriteResolvableInt(DataTypes, data, Amount);
        StructuredComponentCodecHelpers.WriteResolvableFloat(DataTypes, data, SpeedMultiplier);
        return new Queue<byte>(data);
    }
}
