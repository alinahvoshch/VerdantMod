using Terraria;
using Terraria.ModLoader;

namespace Verdant.Items.Verdant.Critter;

[Sacrifice(5)]
class CanopySnailItem : ModItem
{
    public override void SetDefaults() => QuickItem.SetCritter(this, 44, 32, ModContent.NPCType<NPCs.Passive.Snails.CanopySnail>(), 1, 15, Item.buyPrice(0, 0, 18, 0));
    public override bool CanUseItem(Player player) => QuickItem.CanCritterSpawnCheck();
}
