using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Verdant.Tiles.Verdant.Basic.Blocks;
using Verdant.Tiles.Verdant.Basic.Plants;
using Verdant.World.PestControlSubworld;

namespace Verdant.Items;

public class ProbablyDelete : ModItem
{
    public override bool IsLoadingEnabled(Mod mod) =>
#if DEBUG
        true;
#else
        false;
#endif

    public override void SetStaticDefaults() => ItemID.Sets.DisableAutomaticPlaceableDrop[Type] = true;

    public override void SetDefaults()
	{
		Item.DamageType = DamageClass.Melee;
		Item.width = 40;
		Item.height = 40;
		Item.useTime = 2;
		Item.useAnimation = 2;
		Item.maxStack = 50;
		Item.useStyle = ItemUseStyleID.Swing;
		Item.knockBack = 6;
		Item.value = 10000;
		Item.rare = ItemRarityID.Green;
		Item.UseSound = SoundID.Item1;
		Item.autoReuse = false;
        Item.placeStyle = 0;
        //Item.shoot = ModContent.ProjectileType<HealPlants>();
        //Item.createWall = ModContent.WallType<BluescreenWall>();
        Item.createTile = ModContent.TileType<LilyPad>();
    }

    public override bool? UseItem(Player player)
    {
        Point mouse = Main.MouseWorld.ToTileCoordinates();

        Tile tile = Main.tile[mouse];
        tile.Slope = SlopeType.SlopeDownLeft;

        PestSubworld.FindPlaceDeadleaf(mouse.X, mouse.Y, Main.rand.NextBool() ? -1 : 1);
        SubworldLibrary.SubworldSystem.Enter<PestSubworld>();
        //for (int i = 0; i < Main.maxTilesX; ++i)
        //    for (int j = 0; j < Main.maxTilesY; ++j)
        //        Main.tile[i, j].ClearTile();

        //new PestSubworld().GenerateLand(new(), new(new()));

        //PestSubworld.Root(mouse.X, mouse.Y, 3);
        return true;
    }
}