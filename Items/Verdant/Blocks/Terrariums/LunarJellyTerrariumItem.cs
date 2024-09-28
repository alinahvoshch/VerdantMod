using Terraria.ID;
using Terraria.ModLoader;
using Verdant.Items.Verdant.Blocks.Plants;
using Verdant.Items.Verdant.Critter;
using Verdant.Items.Verdant.Materials;
using Verdant.Tiles.Verdant.Decor.Terrariums;

namespace Verdant.Items.Verdant.Blocks.Terrariums;

[Sacrifice(1)]
public class LunarJellyTerrariumItem : ModItem
{
    public override void SetDefaults() => QuickItem.SetBlock(this, 32, 48, ModContent.TileType<LunarJellyTerrarium>());
    public override void AddRecipes() => QuickItem.AddRecipe(this, -1, 1, (ItemID.Terrarium, 1), (ModContent.ItemType<LunarJellyItem>(), 1), 
        (ModContent.ItemType<StargazerPlantItem>(), 2), (ModContent.ItemType<LushLeaf>(), 5));
}
