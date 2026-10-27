using System.Collections.Generic;
using MinecraftClient.Inventory.ItemPalettes;
using MinecraftClient.Protocol.Handlers.StructuredComponents.Core;

namespace MinecraftClient.Protocol.Handlers.StructuredComponents.Components._26_3;

/// <summary>
/// 26.3+ sign_text_front / sign_text_back: 4 Components, Optional 4 filtered Components, VarInt DyeColor, Bool glowing.
/// </summary>
public class SignTextComponent263(DataTypes dataTypes, ItemPalette itemPalette, SubComponentRegistry subComponentRegistry)
    : StructuredComponent(dataTypes, itemPalette, subComponentRegistry)
{
    private const int LineCount = 4;

    public List<Dictionary<string, object>> Messages { get; set; } = [];
    public List<Dictionary<string, object>>? FilteredMessages { get; set; }
    public int Color { get; set; }
    public bool HasGlowingText { get; set; }

    public override void Parse(Queue<byte> data)
    {
        Messages = ReadLines(data);
        FilteredMessages = DataTypes.ReadNextBool(data) ? ReadLines(data) : null;
        Color = DataTypes.ReadNextVarInt(data);
        HasGlowingText = DataTypes.ReadNextBool(data);
    }

    public override Queue<byte> Serialize()
    {
        var data = new List<byte>();
        WriteLines(data, Messages);
        data.AddRange(DataTypes.GetBool(FilteredMessages is not null));
        if (FilteredMessages is not null)
            WriteLines(data, FilteredMessages);
        data.AddRange(DataTypes.GetVarInt(Color));
        data.AddRange(DataTypes.GetBool(HasGlowingText));
        return new Queue<byte>(data);
    }

    private List<Dictionary<string, object>> ReadLines(Queue<byte> data)
    {
        var lines = new List<Dictionary<string, object>>(LineCount);
        for (var i = 0; i < LineCount; i++)
            lines.Add(DataTypes.ReadNextNbt(data));
        return lines;
    }

    private void WriteLines(List<byte> data, List<Dictionary<string, object>> lines)
    {
        foreach (var line in lines)
            data.AddRange(DataTypes.GetNbt(line));
    }
}
