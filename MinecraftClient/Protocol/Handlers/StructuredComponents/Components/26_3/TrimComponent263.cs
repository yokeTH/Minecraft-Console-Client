using System;
using System.Collections.Generic;
using MinecraftClient.Inventory.ItemPalettes;
using MinecraftClient.Protocol.Handlers.StructuredComponents.Components._1_21_5;
using MinecraftClient.Protocol.Handlers.StructuredComponents.Core;
using MinecraftClient.Protocol.Message;

namespace MinecraftClient.Protocol.Handlers.StructuredComponents.Components._26_3;

/// <summary>
/// 26.3+ trim: Holder&lt;TrimMaterial&gt; + Holder&lt;TrimPattern&gt;, where an inline TrimMaterial is
/// Identifier paletteId + Component description.
/// </summary>
public class TrimComponent263(DataTypes dataTypes, ItemPalette itemPalette, SubComponentRegistry subComponentRegistry)
    : StructuredComponent(dataTypes, itemPalette, subComponentRegistry)
{
    public int MaterialHolderValue { get; set; }
    public DirectTrimMaterial263? DirectMaterial { get; set; }
    public int PatternHolderValue { get; set; }
    public DirectTrimPattern1215? DirectPattern { get; set; }

    public override void Parse(Queue<byte> data)
    {
        MaterialHolderValue = DataTypes.ReadNextVarInt(data);
        if (MaterialHolderValue == 0)
        {
            var paletteId = DataTypes.ReadNextString(data);
            var descriptionNbt = DataTypes.ReadNextNbt(data);
            DirectMaterial = new DirectTrimMaterial263(paletteId, descriptionNbt, ChatParser.ParseText(descriptionNbt));
        }

        PatternHolderValue = DataTypes.ReadNextVarInt(data);
        if (PatternHolderValue == 0)
        {
            var assetId = DataTypes.ReadNextString(data);
            var descriptionNbt = DataTypes.ReadNextNbt(data);
            var description = ChatParser.ParseText(descriptionNbt);
            var decal = DataTypes.ReadNextBool(data);
            DirectPattern = new DirectTrimPattern1215(assetId, descriptionNbt, description, decal);
        }
    }

    public override Queue<byte> Serialize()
    {
        var data = new List<byte>();

        data.AddRange(DataTypes.GetVarInt(MaterialHolderValue));
        if (MaterialHolderValue == 0)
        {
            if (DirectMaterial is null)
                throw new ArgumentNullException(nameof(DirectMaterial), "Direct trim material payload is required when holder value is 0.");

            data.AddRange(DataTypes.GetString(DirectMaterial.PaletteId));
            data.AddRange(DataTypes.GetNbt(DirectMaterial.DescriptionNbt));
        }

        data.AddRange(DataTypes.GetVarInt(PatternHolderValue));
        if (PatternHolderValue == 0)
        {
            if (DirectPattern is null)
                throw new ArgumentNullException(nameof(DirectPattern), "Direct trim pattern payload is required when holder value is 0.");

            data.AddRange(DataTypes.GetString(DirectPattern.AssetId));
            data.AddRange(DataTypes.GetNbt(DirectPattern.DescriptionNbt));
            data.AddRange(DataTypes.GetBool(DirectPattern.Decal));
        }

        return new Queue<byte>(data);
    }
}

public sealed record DirectTrimMaterial263(
    string PaletteId,
    Dictionary<string, object> DescriptionNbt,
    string Description);
