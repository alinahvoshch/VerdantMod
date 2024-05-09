using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;
using Verdant.Systems.MinableNPCs;

namespace Verdant.NPCs.Enemy.PestControl.RotsludgeGiant;

public class Rotsludge : ModNPC, ITileNPC
{
    public class RotsludgeClearLocations : ModSystem
    {
        public override void PreUpdateNPCs() => sludgeLocations.Clear();
    }

    enum SludgeState
    {
        Initialize,
        Move,
    }

    public const int Size = 30;
    
    private static ILHook lightHook = null;
    private static readonly HashSet<Point> sludgeLocations = [];

    private Player Target => Main.player[NPC.target];
    private ref float Direction => ref NPC.ai[0];
    private ref float Timer => ref NPC.ai[1];

    private SludgeState State
    {
        get => (SludgeState)NPC.ai[2];
        set => NPC.ai[2] = (float)value;
    }

    private ref float SineTimer => ref NPC.ai[3];

    readonly Sludge[,] sludges = new Sludge[Size, Size];

    private bool _drawBack = true;

    public override void Unload()
    {
        lightHook.Undo();
        lightHook = null;
    }

    public override void SetDefaults()
    {
        NPC.width = Size * 16;
        NPC.height = Size * 16;
        NPC.damage = 0;
        NPC.lifeMax = 15;
        NPC.noGravity = true;
        NPC.noTileCollide = false;
        NPC.value = Item.buyPrice(0, 0, 10);
        NPC.knockBackResist = 0f;
        NPC.aiStyle = -1;
        NPC.HitSound = SoundID.Dig;
        NPC.DeathSound = SoundID.NPCDeath4;
        NPC.hide = true;
        
        SpawnModBiomes = [ModContent.GetInstance<Scenes.VerdantBiome>().Type];
    }

    public override void DrawBehind(int index)
    {
        Main.instance.DrawCacheNPCsBehindNonSolidTiles.Add(index);
        Main.instance.DrawCacheNPCsOverPlayers.Add(index);
    }

    public override bool CheckActive() => false;
    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) => bestiaryEntry.AddInfo(this, "");
    public override bool? CanBeHitByItem(Player player, Item item) => false;
    public override bool CanBeHitByNPC(NPC attacker) => false;
    public override bool? CanBeHitByProjectile(Projectile projectile) => false;

    public override void AI()
    {
        NPC.TargetClosest(true);

        SineTimer++;

        if (State == SludgeState.Initialize)
        {
            InitSludge();
            State = SludgeState.Move;
        }
        else
        {
            float mult = 1f;

            Timer++;

            if (Timer < 120)
                mult = Timer / 120f;

            NPC.velocity = Direction switch
            {
                0 => new Vector2(6, 0),
                1 => new Vector2(-6, 0),
                2 => new Vector2(0, 6),
                _ => new Vector2(0, -6)
            } * mult * 0.5f;

            if (NPC.collideX)
            {
                Direction = Main.rand.NextBool() ? 2 : 3;
                SwitchState(SludgeState.Move);
            }

            if (NPC.collideY)
            {
                Direction = Main.rand.NextBool() ? 0 : 1;
                SwitchState(SludgeState.Move);
            }
        }

        CollideWithEntities();
        SetLocations();
        CheckKill();
    }

    private void CheckKill()
    {
        int count = 0;

        foreach (var item in sludges)
        {
            if (item.Alive)
                count++;
        }

        if (count < Size * Size * 0.25f)
        {
            NPC.ApplyInteraction(0);
            NPC.NPCLoot();
            NPC.active = false;

            for (int i = 0; i < Size; ++i)
            {
                for (int j = 0; j < Size; ++j)
                {
                    if (sludges[i, j].Alive)
                    {
                        var pos = NPC.position + new Vector2(i, j).ToWorldCoordinates(0, 0);
                        Projectile.NewProjectile(NPC.GetSource_Death(), pos, new Vector2(0, NPC.velocity.Y), ModContent.ProjectileType<FallingSludge>(), 5, 0, Main.myPlayer);
                    }
                }
            }
        }
    }

    private void SetLocations()
    {
        int x = (int)NPC.position.X / 16;
        int y = (int)NPC.position.Y / 16;

        for (int i = 0; i < Size; ++i)
            for (int j = 0; j < Size; ++j)
                sludgeLocations.Add(new Point(x + i, y + j));
    }

    private void InitSludge()
    {
        for (int i = 0; i < Size; ++i)
        {
            for (int j = 0; j < Size; ++j)
            {
                int frameX = 18;

                if (i == 0)
                    frameX = 0;
                else if (i == Size - 1)
                    frameX = 36;

                int frameY = 18;

                if (j == 0)
                    frameY = 0;
                else if (j == Size - 1)
                    frameY = 36;

                sludges[i, j] = new Sludge(new Point(frameX, frameY));
            }
        }
    }

    private void CollideWithEntities()
    {
        foreach (var npc in Main.ActiveNPCs)
        {
            if (npc.whoAmI != NPC.whoAmI && npc.Hitbox.Intersects(NPC.Hitbox) && GetSludgeIntersection(NPC.Hitbox))
            {
                if (npc.type == ModContent.NPCType<Rotsludge>())
                {
                    if (npc.velocity.X != 0)
                        npc.collideX = NPC.collideX = true;
                    else if (npc.velocity.Y != 0)
                        npc.collideY = NPC.collideY = true;

                    if (npc.collideX || npc.collideY)
                        npc.position -= npc.velocity;
                }
                else
                    npc.position -= npc.velocity / 3;
            }
        }

        foreach (var plr in Main.ActivePlayers)
        {
            if (plr.Hitbox.Intersects(NPC.Hitbox) && GetSludgeIntersection(plr.Hitbox))
            {
                plr.honey = true;
                plr.wet = true;
                plr.honeyWet = true;
                plr.wetCount = 20;
            }
        }
    }

    private bool GetSludgeIntersection(Rectangle hitbox)
    {
        Vector2 pos = hitbox.Location.ToVector2();

        if (pos.X < NPC.position.X || pos.X > NPC.position.X + NPC.width)
            return false;

        if (pos.Y < NPC.position.Y || pos.Y > NPC.position.Y + NPC.height)
            return false;

        pos -= NPC.position;
        pos /= 16;

        for (int i = (int)pos.X - 1; i < pos.X + hitbox.Width / 16 + 1; ++i)
        {
            for (int j = (int)pos.Y - 1; j < pos.Y + hitbox.Height / 16 + 1; ++j)
            {
                if (i < 0 || j < 0 || i >= Size || j >= Size)
                    continue;

                if (sludges[i, j].Alive)
                    return true;
            }
        }

        return false;
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        if (NPC.IsABestiaryIconDummy)
        {
            if (SineTimer == 0)
                InitSludge();

            SineTimer++;
        }

        for (int i = 0; i < Size; ++i)
        {
            for (int j = 0; j < Size; ++j)
            {
                sludges[i, j].Frame(i, j, sludges);

                var tex = TextureAssets.Npc[Type].Value;
                var frame = sludges[i, j].frame;
                var old = sludges[i, j].originalFrame;
                var pos = NPC.position + new Vector2(i, j).ToWorldCoordinates(0, 0) + SineMovement(i, j);

                if (_drawBack || NPC.IsABestiaryIconDummy)
                {
                    var col = NPC.IsABestiaryIconDummy ? Color.White : Lighting.GetColor(pos.ToTileCoordinates(), Color.Gray) * 0.6f;
                    spriteBatch.Draw(tex, pos - screenPos, new Rectangle(old.X, old.Y, 18, 18), col, 0f, Vector2.Zero, new Vector2(1f, 1.03f), SpriteEffects.None, 0);
                }

                if (!_drawBack || NPC.IsABestiaryIconDummy)
                {
                    if (sludges[i, j].life <= 0)
                        continue;

                    var col = NPC.IsABestiaryIconDummy ? Color.White : Lighting.GetColor(pos.ToTileCoordinates(), Color.White) * 0.6f;
                    spriteBatch.Draw(tex, pos - screenPos, new Rectangle(frame.X, frame.Y, 18, 18), col, 0f, Vector2.Zero, new Vector2(1f, 1.03f), SpriteEffects.None, 0);
                }
            }
        }

        if (!NPC.IsABestiaryIconDummy)
            _drawBack = !_drawBack;

        return false;
    }

    private Vector2 SineMovement(int i, int j)
    {
        float sine = MathF.Sin(SineWave(i, j)) * 8;
        return new(0, (int)sine);
    }

    private float SineWave(int i, int j)
    {
        return ((i + j) * MathHelper.PiOver2 + SineTimer) * 0.04f;
    }

    private void SwitchState(SludgeState move)
    {
        State = move;
        NPC.netUpdate = true;
        Timer = 0;
    }

    public override bool? CanFallThroughPlatforms() => true;

    public bool CanMineNPC(Point mousePosition)
    {
        Vector2 pos = mousePosition.ToVector2();

        if (pos.X < NPC.position.X || pos.X > NPC.position.X + NPC.width)
            return false;

        if (pos.Y < NPC.position.Y || pos.Y > NPC.position.Y + NPC.height)
            return false;

        pos -= NPC.position;

        return sludges[(int)pos.X / 16, (int)pos.Y / 16].life > 0;
    }

    public void MineNPC(Point mousePosition)
    {
        Vector2 pos = mousePosition.ToVector2() - NPC.position;
        sludges[(int)pos.X / 16, (int)pos.Y / 16].Hit((int)pos.X / 16, (int)pos.Y / 16, sludges);
        SoundEngine.PlaySound(NPC.HitSound);
    }
}