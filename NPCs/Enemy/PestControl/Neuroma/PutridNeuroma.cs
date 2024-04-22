using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;
using Verdant.Tiles;

namespace Verdant.NPCs.Enemy.PestControl.Neuroma;

public class PutridNeuroma : ModNPC
{
    private List<Connection> connections = null;

    public override void SetStaticDefaults()
    {
    }

    public override void SetDefaults()
    {
        NPC.width = 42;
        NPC.height = 42;
        NPC.damage = 60;
        NPC.defense = 30;
        NPC.lifeMax = 1;
        NPC.dontTakeDamage = true;
        NPC.noGravity = true;
        NPC.noTileCollide = true;
        NPC.value = Item.buyPrice(0, 0, 10);
        NPC.knockBackResist = 0f;
        NPC.aiStyle = -1;
        NPC.HitSound = SoundID.Splash;
        SpawnModBiomes = [ModContent.GetInstance<Scenes.VerdantBiome>().Type];
    }

    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) => bestiaryEntry.AddInfo(this, "");

    public override void AI()
    {
        if (connections is null)
        {
            connections = [];

            HashSet<Point> points = FindTiles();
            HashSet<int> otherNeuroma = FindNeuroma();

            for (int i = 0; i < Math.Min(3, points.Count); ++i)
                SpawnConnection(points);

            if (otherNeuroma.Count > 0)
            {
                int[] neuromaArr = [.. otherNeuroma];

                for (int i = 0; i < otherNeuroma.Count; ++i)
                {
                    int neuroma = Main.rand.Next(neuromaArr);
                    SpawnConnection(neuroma);
                    otherNeuroma.Remove(neuroma);
                }
            }
        }

        if (connections.All(x => !x.NPC.active || x.NPC.type != ModContent.NPCType<Connection>()))
        {
            NPC.life = 0;
            NPC.NPCLoot();
            NPC.checkDead();
        }
    }

    private HashSet<int> FindNeuroma()
    {
        HashSet<int> npc = [];

        foreach (var item in Main.ActiveNPCs)
        {
            if (item.type == Type && item.DistanceSQ(NPC.Center) < 900 * 900)
                npc.Add(item.whoAmI);
        }

        return npc;
    }

    private HashSet<Point> FindTiles()
    {
        HashSet<Point> points = [];

        for (int i = -30; i < 30; ++i)
        {
            for (int j = -30; j < 30; ++j)
            {
                int x = (int)NPC.Center.X / 16 + i;
                int y = (int)NPC.Center.Y / 16 + j;

                if (WorldGen.SolidTile(x, y) && TileHelper.HasOpenAdjacent(x, y))
                    points.Add(new(x, y));
            }
        }

        return points;
    }

    private void SpawnConnection(HashSet<Point> points)
    {
        Point tile = Main.rand.Next([..points]);

        NPC npc = NPC.NewNPCDirect(NPC.GetSource_FromAI(), NPC.Center, ModContent.NPCType<Connection>(), NPC.whoAmI);
        Connection connection = npc.ModNPC as Connection;
        connection.anchors = (new Connection.Anchor(tile), new Connection.Anchor(NPC));
        connections.Add(connection);
    }

    private void SpawnConnection(int otherNeuroma)
    {
        NPC npc = NPC.NewNPCDirect(NPC.GetSource_FromAI(), NPC.Center, ModContent.NPCType<Connection>(), NPC.whoAmI);

        if (npc.ModNPC is not Connection connection)
            return;

        connection.anchors = (new Connection.Anchor(Main.npc[otherNeuroma]), new Connection.Anchor(NPC));
        connections.Add(connection);
    }

    public override void FindFrame(int frameHeight) => NPC.frameCounter++;

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        Texture2D tex = TextureAssets.Npc[Type].Value;
        float sin = MathF.Sin((float)NPC.frameCounter * 0.06f) * 0.4f;
        Vector2 scale = new(sin + 1, -sin + 1);

        sin = MathF.Sin(((float)NPC.frameCounter + MathHelper.Pi) * 0.03f) * 0.1f;
        scale *= new Vector2(sin + 1, -sin + 1);

        spriteBatch.Draw(tex, NPC.Center - screenPos, null, drawColor, NPC.rotation, NPC.Size / 2f, scale * 0.9f, SpriteEffects.None, 0);
        return false;
    }
}
