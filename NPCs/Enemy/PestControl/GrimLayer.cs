using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;
using Verdant.Systems.MinableNPCs;

namespace Verdant.NPCs.Enemy.PestControl;

public class GrimLayer : ModNPC, ITileNPC, ISolidNPC
{
    enum LayerState
    {
        Initialize,
        Move,
    }

    private Player Target => Main.player[NPC.target];
    private ref float Direction => ref NPC.ai[0];
    private ref float Timer => ref NPC.ai[1];

    private LayerState State 
    { 
        get => (LayerState)NPC.ai[2]; 
        set => NPC.ai[2] = (float)value; 
    }

    public override void SetDefaults()
    {
        NPC.width = 48;
        NPC.height = 48;
        NPC.scale = 3f;
        NPC.damage = 0;
        NPC.lifeMax = 15;
        NPC.noGravity = true;
        NPC.noTileCollide = false;
        NPC.value = Item.buyPrice(0, 0, 10);
        NPC.knockBackResist = 0f;
        NPC.aiStyle = -1;
        NPC.HitSound = SoundID.Dig;
        NPC.DeathSound = SoundID.NPCDeath4;
        SpawnModBiomes = [ModContent.GetInstance<Scenes.VerdantBiome>().Type];
    }

    public override bool CheckActive() => false;
    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) => bestiaryEntry.AddInfo(this, "");
    public override bool? CanBeHitByItem(Player player, Item item) => false;
    public override bool CanBeHitByNPC(NPC attacker) => false;
    public override bool? CanBeHitByProjectile(Projectile projectile) => false;

    public override void AI()
    {
        NPC.TargetClosest(true);
        CollideWithEntities();

        if (State == LayerState.Initialize)
        {
            Direction = Main.rand.Next(4);
            SwitchState(LayerState.Move);
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
                SwitchState(LayerState.Move);
            }

            if (NPC.collideY)
            {
                Direction = Main.rand.NextBool() ? 0 : 1;
                SwitchState(LayerState.Move);
            }
        }
    }

    private void CollideWithEntities()
    {
        foreach (var npc in Main.ActiveNPCs)
        {
            if (npc.whoAmI != NPC.whoAmI && npc.type == ModContent.NPCType<GrimLayer>() && npc.Hitbox.Intersects(NPC.Hitbox))
            {
                if (npc.velocity.X != 0)
                    npc.collideX = NPC.collideX = true;
                else if (npc.velocity.Y != 0)
                    npc.collideY = NPC.collideY = true;

                if (npc.collideX || npc.collideY)
                    npc.position -= npc.velocity;
            }
        }
    }

    private void SwitchState(LayerState move)
    {
        State = move;
        NPC.netUpdate = true;
        Timer = 0;
    }

    public override bool? CanFallThroughPlatforms() => true;

    public bool CanMineNPC(Point mousePosition) => true;

    public void MineNPC(Point mousePosition)
    {
        NPC.life--;
        NPC.checkDead();
        SoundEngine.PlaySound(NPC.HitSound);
    }
}