using Terraria.ID;
using Terraria.ModLoader;
using Verdant.Items.Verdant.Materials;
using Verdant.Tiles.Verdant.Basic.Plants;

namespace Verdant.Items.Verdant.Blocks.Plants;

public class StargazerPlantItem : ModItem
{
    public override void SetDefaults() => QuickItem.SetBlock(this, 32, 34, ModContent.TileType<StargazerPlant>(), rarity: ItemRarityID.Blue);
    public override void AddRecipes() => QuickItem.AddRecipe(this, TileID.LivingLoom, 1, (ItemID.FallenStar, 2), (ModContent.ItemType<LushLeaf>(), 12));
}
