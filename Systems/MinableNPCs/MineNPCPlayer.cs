using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace Verdant.Systems.MinableNPCs;

internal class MineNPCPlayer : ModPlayer
{
    public NPC standingOnNPC = null;

    public override void Load()
    {
        On_Player.UpdateTouchingTiles += SolidNPCCollision;
    }

    private void SolidNPCCollision(On_Player.orig_UpdateTouchingTiles orig, Player self)
    {
        orig(self);

        bool standingVel = false;

        foreach (var npc in Main.ActiveNPCs)
        {
            if (self.Hitbox.Intersects(npc.Hitbox) && npc.ModNPC is ISolidNPC)
            {
                if (npc.velocity.Y != 0)
                    self.position.Y += npc.velocity.Y;

                if (self.Hitbox.Bottom < npc.Center.Y)
                {
                    self.fallStart = (int)(self.position.Y / 16f);
                    self.velocity.Y = npc.velocity.Y;
                    self.jump = 0;
                    self.sliding = true;
                    self.Bottom = new(self.Bottom.X, npc.position.Y + 2);
                    //self.position.X += npc.velocity.X;

                    self.GetModPlayer<MineNPCPlayer>().standingOnNPC = npc;
                }

                if (self.Hitbox.Top + self.velocity.Y > npc.Center.Y)
                {
                    self.jump = 0;
                    self.velocity.Y = self.gravity;
                }

                standingVel = true;
                break;
            }
        }

        if (!standingVel && self.GetModPlayer<MineNPCPlayer>().standingOnNPC is not null)
        {
            self.velocity += self.GetModPlayer<MineNPCPlayer>().standingOnNPC.velocity;
            self.GetModPlayer<MineNPCPlayer>().standingOnNPC = null;
        }
    }

    public override void PreUpdateMovement()
    {
        if (standingOnNPC is not null)
        {
            Player.position.X += standingOnNPC.velocity.X;
            //standingOnNPC = null;
        }
    }

    public override bool CanUseItem(Item item)
    {
        if (CanMineNPC(item, out NPC npc))
        {
            Player.itemAnimation = item.useAnimation;
            Player.itemTime = item.useTime;
            Player.SetDummyItemTime(item.useTime);

            (npc.ModNPC as ITileNPC).MineNPC(Main.MouseWorld.ToPoint());
            return false;
        }    
        return true;
    }

    private static bool CanMineNPC(Item item, out NPC foundNpc)
    {
        foundNpc = null;

        if (item.pick <= 0)
            return false;

        Point mousePos = Main.MouseWorld.ToPoint();

        foreach (var npc in Main.ActiveNPCs)
        {
            if (npc.Hitbox.Contains(mousePos) && npc.ModNPC is ITileNPC tileNpc && tileNpc.CanMineNPC(mousePos))
            {
                foundNpc = npc;
                return true;
            }
        }

        return false;
    }
}
