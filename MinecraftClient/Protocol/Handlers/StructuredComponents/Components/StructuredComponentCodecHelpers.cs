using System.Collections.Generic;

namespace MinecraftClient.Protocol.Handlers.StructuredComponents.Components;

internal static class StructuredComponentCodecHelpers
{
    public static HolderSetData ReadHolderSet(DataTypes dataTypes, Queue<byte> data)
    {
        var sizeOrTag = dataTypes.ReadNextVarInt(data);
        if (sizeOrTag == 0)
            return new HolderSetData(dataTypes.ReadNextString(data), []);

        var count = sizeOrTag - 1;
        var holderIds = new List<int>(count);
        for (var i = 0; i < count; i++)
            holderIds.Add(dataTypes.ReadNextVarInt(data));

        return new HolderSetData(null, holderIds);
    }

    public static void WriteHolderSet(DataTypes dataTypes, List<byte> bytes, HolderSetData holderSet)
    {
        if (holderSet.Tag is not null)
        {
            bytes.AddRange(DataTypes.GetVarInt(0));
            bytes.AddRange(dataTypes.GetString(holderSet.Tag));
            return;
        }

        bytes.AddRange(DataTypes.GetVarInt(holderSet.HolderIds.Count + 1));
        foreach (var holderId in holderSet.HolderIds)
            bytes.AddRange(DataTypes.GetVarInt(holderId));
    }

    public static SoundEventHolderData ReadSoundEventHolder(DataTypes dataTypes, Queue<byte> data)
    {
        var holderId = dataTypes.ReadNextVarInt(data);
        if (holderId != 0)
            return new SoundEventHolderData(holderId, null, false, 0);

        var soundLocation = dataTypes.ReadNextString(data);
        var hasFixedRange = dataTypes.ReadNextBool(data);
        var fixedRange = hasFixedRange ? dataTypes.ReadNextFloat(data) : 0;
        return new SoundEventHolderData(holderId, soundLocation, hasFixedRange, fixedRange);
    }

    public static void WriteSoundEventHolder(DataTypes dataTypes, List<byte> bytes, SoundEventHolderData soundEvent)
    {
        bytes.AddRange(DataTypes.GetVarInt(soundEvent.HolderId));
        if (soundEvent.HolderId != 0)
            return;

        bytes.AddRange(dataTypes.GetString(soundEvent.SoundLocation ?? ""));
        bytes.AddRange(dataTypes.GetBool(soundEvent.HasFixedRange));
        if (soundEvent.HasFixedRange)
            bytes.AddRange(dataTypes.GetFloat(soundEvent.FixedRange));
    }

    public static SoundEventHolderData? ReadOptionalSoundEventHolder(DataTypes dataTypes, Queue<byte> data)
    {
        return dataTypes.ReadNextBool(data) ? ReadSoundEventHolder(dataTypes, data) : null;
    }

    public static void WriteOptionalSoundEventHolder(DataTypes dataTypes, List<byte> bytes, SoundEventHolderData? soundEvent)
    {
        bytes.AddRange(dataTypes.GetBool(soundEvent is not null));
        if (soundEvent is not null)
            WriteSoundEventHolder(dataTypes, bytes, soundEvent);
    }

    // 26.3+ ResolvableInt: Either<Int constant, ResourceKey<context_int_provider>>
    public static ResolvableIntData ReadResolvableInt(DataTypes dataTypes, Queue<byte> data)
    {
        return dataTypes.ReadNextBool(data)
            ? new ResolvableIntData(dataTypes.ReadNextInt(data), null)
            : new ResolvableIntData(null, dataTypes.ReadNextString(data));
    }

    public static void WriteResolvableInt(DataTypes dataTypes, List<byte> bytes, ResolvableIntData value)
    {
        bytes.AddRange(dataTypes.GetBool(value.Constant is not null));
        bytes.AddRange(value.Constant is int constant ? DataTypes.GetInt(constant) : dataTypes.GetString(value.Reference ?? string.Empty));
    }

    // 26.3+ ResolvableFloat: Either<Float constant, ResourceKey<context_float_provider>>
    public static ResolvableFloatData ReadResolvableFloat(DataTypes dataTypes, Queue<byte> data)
    {
        return dataTypes.ReadNextBool(data)
            ? new ResolvableFloatData(dataTypes.ReadNextFloat(data), null)
            : new ResolvableFloatData(null, dataTypes.ReadNextString(data));
    }

    public static void WriteResolvableFloat(DataTypes dataTypes, List<byte> bytes, ResolvableFloatData value)
    {
        bytes.AddRange(dataTypes.GetBool(value.Constant is not null));
        bytes.AddRange(value.Constant is float constant ? dataTypes.GetFloat(constant) : dataTypes.GetString(value.Reference ?? string.Empty));
    }
}

public sealed record HolderSetData(string? Tag, List<int> HolderIds);

public sealed record SoundEventHolderData(int HolderId, string? SoundLocation, bool HasFixedRange, float FixedRange);

public sealed record ResolvableIntData(int? Constant, string? Reference);

public sealed record ResolvableFloatData(float? Constant, string? Reference);
