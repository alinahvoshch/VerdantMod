using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Verdant.Items;
using Verdant.Items.Verdant.Blocks.PestControl;

namespace Verdant.Tiles.Verdant.Decor.ThornFurniture;

public class DeadleafPlatform : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileLighted[Type] = true;
        Main.tileFrameImportant[Type] = true;
        Main.tileSolidTop[Type] = true;
        Main.tileSolid[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileTable[Type] = true;
        Main.tileLavaDeath[Type] = true;

        TileID.Sets.Platforms[Type] = true;
        TileID.Sets.DisableSmartCursor[Type] = true;

        TileObjectData.newTile.CoordinateHeights = [16];
        TileObjectData.newTile.CoordinateWidth = 16;
        TileObjectData.newTile.CoordinatePadding = 2;
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.StyleMultiplier = 27;
        TileObjectData.newTile.StyleWrapLimit = 27;
        TileObjectData.newTile.UsesCustomCanPlace = false;
        TileObjectData.newTile.LavaDeath = true;
        TileObjectData.addTile(Type);

        AddToArray(ref TileID.Sets.RoomNeeds.CountsAsDoor);
        AddMapEntry(new Color(114, 69, 39));

        DustType = DustID.t_BorealWood;
        AdjTiles = [TileID.Platforms];
    }

    public override void PostSetDefaults() => Main.tileNoSunLight[Type] = false;
    public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;

    public class DeadleafPlatformItem : ModItem
    {
        public override void SetDefaults() => QuickItem.SetBlock(this, 16, 12, ModContent.TileType<DeadleafPlatform>());
        public override void AddRecipes() => QuickItem.AddRecipe(this, -1, 2, (ModContent.ItemType<ThornBlock>(), 1));
    }
}