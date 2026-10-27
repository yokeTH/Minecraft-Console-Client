using MinecraftClient.Inventory.ItemPalettes;
using MinecraftClient.Protocol.Handlers.StructuredComponents.Components._1_21_2;
using MinecraftClient.Protocol.Handlers.StructuredComponents.Core;

namespace MinecraftClient.Protocol.Handlers.StructuredComponents.Components._26_3;

public class DeathProtectionComponent263(DataTypes dataTypes, ItemPalette itemPalette, SubComponentRegistry subComponentRegistry)
    : DeathProtectionComponent(dataTypes, itemPalette, subComponentRegistry)
{
    protected override bool TeleportHasDirectionalParticles => true;
}
