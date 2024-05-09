using Terraria.ID;
using Terraria.ModLoader;
using Verdant.Tiles.Verdant.Basic.Blocks;

namespace Verdant.NPCs.Enemy.PestControl.RotsludgeGiant;

internal class FallingSludge : ModProjectile
{
    public override void SetDefaults()
    {
        Projectile.CloneDefaults(ProjectileID.SandBallFalling);
        Projectile.Size = new(8);

        ProjectileID.Sets.FallingBlockTileItem[Type] = new ProjectileID.Sets.FallingBlockTileItemInfo(ModContent.TileType<SludgeTile>());
    }
}
