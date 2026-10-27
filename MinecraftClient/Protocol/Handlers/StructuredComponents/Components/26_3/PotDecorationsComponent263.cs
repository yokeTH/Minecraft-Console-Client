using System.Collections.Generic;
using MinecraftClient.Inventory;
using MinecraftClient.Inventory.ItemPalettes;
using MinecraftClient.Protocol.Handlers.StructuredComponents.Core;

namespace MinecraftClient.Protocol.Handlers.StructuredComponents.Components._26_3;

/// <summary>
/// 26.3+ pot decorations: four optional ItemStackTemplates in the order back, left, right, front.
/// </summary>
public class PotDecorationsComponent263(DataTypes dataTypes, ItemPalette itemPalette, SubComponentRegistry subComponentRegistry)
    : StructuredComponent(dataTypes, itemPalette, subComponentRegistry)
{
    public Item?[] Sides { get; set; } = new Item?[4];

    public override void Parse(Queue<byte> data)
    {
        for (var i = 0; i < Sides.Length; i++)
            Sides[i] = DataTypes.ReadNextBool(data) ? DataTypes.ReadNextItemStackTemplate(data, ItemPalette) : null;
    }

    public override Queue<byte> Serialize()
    {
        var data = new List<byte>();
        foreach (var side in Sides)
        {
            data.AddRange(DataTypes.GetBool(side is not null));
            if (side is not null)
                data.AddRange(DataTypes.GetItemStackTemplate(side, ItemPalette));
        }
        return new Queue<byte>(data);
    }
}
