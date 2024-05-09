using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Verdant.NPCs.Enemy.PestControl.RotsludgeGiant;

namespace Verdant.Tiles.Verdant.Basic.Blocks;

internal class SludgeTile : ModTile
{
    public override void SetStaticDefaults()
    {
        QuickTile.SetAll(this, 0, DustID.YellowStarfish, SoundID.DD2_SkeletonHurt, new Color(127, 108, 30), true, false);

        TileID.Sets.DrawsWalls[Type] = true;
        TileID.Sets.DrawTileInSolidLayer[Type] = true;
        TileID.Sets.Falling[Type] = true;
        TileID.Sets.FallingBlockProjectile[Type] = new(ModContent.ProjectileType<FallingSludge>(), 5);
        TileID.Sets.NotReallySolid[Type] = true;

        Main.tileBouncy[Type] = true;
        Main.tileBrick[Type] = true;
    }
}