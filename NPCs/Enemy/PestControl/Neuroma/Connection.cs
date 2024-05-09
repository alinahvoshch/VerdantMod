using Microsoft.CodeAnalysis;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.NetModules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Verdant.NPCs.Enemy.PestControl.Neuroma;

public class Connection : ModNPC
{
    public class Anchor
    {
        public bool IsTile => TileLocation.HasValue;
        public Vector2 Position => IsTile ? TileLocation.Value.ToWorldCoordinates() : NPCAnchor.Center;

        public Point? TileLocation;
        public NPC NPCAnchor;

        public Anchor(Point tile) => TileLocation = tile;
        public Anchor(NPC npc) => NPCAnchor = npc;

        public bool IsInvalid() => IsTile ? !WorldGen.SolidTile(TileLocation.Value) : !NPCAnchor.active;
    }

    public float AnchorDistance => anchors.first.Position.Distance(anchors.second.Position);
    public float AnchorAngle => anchors.first.Position.AngleTo(anchors.second.Position) + MathHelper.PiOver2;

    internal (Anchor first, Anchor second) anchors = (null, null);

    public override void SetStaticDefaults()
    {
    }

    public override void SetDefaults()
    {
        NPC.width = 42;
        NPC.height = 42;
        NPC.damage = 50;
        NPC.defense = 30;
        NPC.lifeMax = 3000;
        NPC.noGravity = true;
        NPC.noTileCollide = true;
        NPC.value = Item.buyPrice(0, 0, 10);
        NPC.knockBackResist = 0f;
        NPC.aiStyle = -1;
        NPC.HitSound = SoundID.Critter;
        NPC.DeathSound = SoundID.NPCDeath4;
        SpawnModBiomes = [ModContent.GetInstance<Scenes.VerdantBiome>().Type];
    }

    public override bool CanHitPlayer(Player target, ref int cooldownSlot) => false;
    public override bool? CanBeHitByProjectile(Projectile projectile) => false;
    public override bool? CanBeHitByItem(Player player, Item item) => false;
    public override bool CanBeHitByNPC(NPC attacker) => false;
    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) => bestiaryEntry.AddInfo(this, "");
    public override bool CheckActive() => false;

    public override void AI()
    {
        if (anchors.first.IsInvalid() || anchors.second.IsInvalid())
        {
            NPC.life = 0;
            NPC.checkDead();
        }

        foreach (var proj in Main.ActiveProjectiles)
        {
            float _ = 0;
            bool col = Collision.CheckAABBvLineCollision(proj.position, proj.Size, anchors.first.Position, anchors.second.Position, 8, ref _);

            if (col && ProjectileLoader.CanHitNPC(proj, NPC) != false && HackCustomProjCollision(proj))
                JustMakeProjectileDoDamage.DamageNPC(proj, NPC.whoAmI);
        }
    }

    private bool HackCustomProjCollision(Projectile proj)
    {
        float max = AnchorDistance / 16f;

        for (int i = 0; i < max; ++i)
        {
            Vector2 pos = Vector2.Lerp(anchors.first.Position, anchors.second.Position, i / max);
            Rectangle rect = new((int)pos.X, (int)pos.Y, 16, 16);

            if (ProjectileLoader.Colliding(proj, proj.Hitbox, rect) is null or true)
                return true;
        }

        return false;
    }

    public override void FindFrame(int frameHeight) => NPC.frameCounter++;

    public override bool? CanCollideWithPlayerMeleeAttack(Player player, Item item, Rectangle meleeAttackHitbox)
    {
        float _ = 0;
        return Collision.CheckAABBvLineCollision(meleeAttackHitbox.Location.ToVector2(), meleeAttackHitbox.Size(), anchors.first.Position, anchors.second.Position, 8, ref _);
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        if (NPC.IsABestiaryIconDummy)
            return false;

        Texture2D tex = TextureAssets.Npc[Type].Value;
        float sin = MathF.Sin((float)NPC.frameCounter * 0.06f) * 0.2f;
        Vector2 scale = new(sin + 1, AnchorDistance / (tex.Height * 0.9f));

        sin = MathF.Sin(((float)NPC.frameCounter + MathHelper.Pi) * 0.03f) * 0.1f;
        scale *= new Vector2(sin + 1, 1f);

        spriteBatch.Draw(tex, anchors.second.Position - screenPos, null, drawColor, AnchorAngle, new Vector2(tex.Width / 2f, 0), scale * 0.9f, SpriteEffects.None, 0);
        return false;
    }
}
