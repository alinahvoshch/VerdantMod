using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Verdant.Items.Verdant.Critter;

[Sacrifice(3)]
class LunarJellyItem : ModItem
{
    public override void SetStaticDefaults() => Main.RegisterItemAnimation(Type, new DrawAnimationVertical(6, 4));
    public override void SetDefaults() => QuickItem.SetCritter(this, 32, 26, ModContent.NPCType<NPCs.Passive.LunarJelly>(), ItemRarityID.Orange, 0);
    public override bool CanUseItem(Player player) => QuickItem.CanCritterSpawnCheck();
}
