using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Verdant.NPCs.Passive;

namespace Verdant.Tiles.Verdant.Decor.Terrariums;

public class LunarJellyTerrarium : ModTile
{
    private static Asset<Texture2D> glowTex;

    public override void SetStaticDefaults()
    {
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileLavaDeath[Type] = true;
        Main.tileTable[Type] = true;
        Main.tileSolidTop[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
        TileObjectData.newTile.Width = 3;
        TileObjectData.newTile.Height = 4;
        TileObjectData.newTile.CoordinateHeights = [16, 16, 16, 16];
        TileObjectData.newTile.Origin = new Point16(1, 2);
        TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.Table | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
        TileObjectData.addTile(Type);

        DustType = DustID.Glass;

        glowTex = ModContent.Request<Texture2D>(Texture + "_Glow");
        AddMapEntry(new Color(22, 51, 81));
    }

    public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;

    public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
    {
        Tile tile = Main.tile[i, j];

        if (tile.TileFrameX == 0 && tile.TileFrameY == 36)
        {
            Main.instance.LoadNPC(ModContent.NPCType<LunarJelly>());
            Texture2D tex = TextureAssets.Npc[ModContent.NPCType<LunarJelly>()].Value;
            float sineOffset = (i * MathHelper.PiOver4 * 1.5f) + (j * MathHelper.PiOver4 / 2f) + Main.GameUpdateCount;
            float x = MathF.Floor(MathF.Sin(sineOffset * 0.01f) * 4);
            Vector2 off = new(-6 + x, MathF.Floor(MathF.Sin(sineOffset * 0.02f) * 10) + 16);
            double frame = (int)(Main.GameUpdateCount * 0.08f) % 4;
            spriteBatch.Draw(tex, TileHelper.TileCustomPosition(i, j, off), new Rectangle(0, (int)(28 * frame), 36, 26), LunarJelly.GetColor(new Point(i, j), 1f));
        }
        return true;
    }

    public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
    {
        Tile t = Framing.GetTileSafely(i, j);
        Vector2 pos = TileHelper.TileCustomPosition(i, j);

        for (int k = 0; k < 2; ++k)
        {
            float sine = MathF.Pow(MathF.Sin(Main.GameUpdateCount * 0.02f + (i + j) * 3) + MathHelper.PiOver2 * k, 2);
            Color light = Lighting.GetColor(i, j);
            Color color = Color.Lerp(Color.LightBlue, light, sine);
            spriteBatch.Draw(glowTex.Value, pos, new Rectangle(t.TileFrameX + (k * 54), t.TileFrameY, 16, 16), color, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
        }
    }
}