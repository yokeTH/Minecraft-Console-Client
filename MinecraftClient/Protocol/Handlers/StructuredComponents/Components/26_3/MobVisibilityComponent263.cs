using System.Collections.Generic;
using MinecraftClient.Inventory.ItemPalettes;
using MinecraftClient.Protocol.Handlers.StructuredComponents.Core;

namespace MinecraftClient.Protocol.Handlers.StructuredComponents.Components._26_3;

/// <summary>
/// 26.3+ mob_visibility: HolderSet&lt;EntityType&gt; targeting entity types + Float visibility.
/// </summary>
public class MobVisibilityComponent263(DataTypes dataTypes, ItemPalette itemPalette, SubComponentRegistry subComponentRegistry)
    : StructuredComponent(dataTypes, itemPalette, subComponentRegistry)
{
    public HolderSetData TargetingEntityTypes { get; set; } = new(null, []);
    public float Visibility { get; set; }

    public override void Parse(Queue<byte> data)
    {
        TargetingEntityTypes = StructuredComponentCodecHelpers.ReadHolderSet(DataTypes, data);
        Visibility = DataTypes.ReadNextFloat(data);
    }

    public override Queue<byte> Serialize()
    {
        var data = new List<byte>();
        StructuredComponentCodecHelpers.WriteHolderSet(DataTypes, data, TargetingEntityTypes);
        data.AddRange(DataTypes.GetFloat(Visibility));
        return new Queue<byte>(data);
    }
}
