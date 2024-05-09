using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
using Verdant.NPCs.Enemy.PestControl;
using Verdant.NPCs.Enemy.PestControl.Neuroma;
using Verdant.Systems.NPCCommon;

namespace Verdant.Systems.PestControl;

internal class PestSystem : ModSystem
{
    public bool pestControlActive = false;
    public float pestControlProgress = 0;
    public float pestControlMiscTimer = 0;

    public List<int> trackedEnemies = [];
    public float lastSpawnProgress = 0;

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        if (pestControlActive)
        {
            int index = layers.FindIndex(layer => layer is not null && layer.Name.Equals("Vanilla: Inventory"));
            LegacyGameInterfaceLayer uiInvasionProgress = new LegacyGameInterfaceLayer("Verdant: Pest Control UI",
                delegate
                {
                    PestInvasionUI.DrawEventUI(Main.spriteBatch);
                    return true;
                },
                InterfaceScaleType.UI);
            layers.Insert(index, uiInvasionProgress);
        }
    }

    public override void ModifySunLightColor(ref Color tileColor, ref Color backgroundColor)
    {
        pestControlMiscTimer++;

        if (pestControlActive)
            backgroundColor = tileColor = new Color(145, 137, 94).MultiplyRGB(Color.Lerp(Color.White, Color.Black, MathF.Sin(pestControlMiscTimer * 0.025f) * 0.1f + 0.5f));
    }

    public override void PostUpdateWorld()
    {
        if (pestControlActive)
        {
            if (pestControlProgress > 1)
            {
                pestControlActive = false;
                pestControlProgress = 0;

                foreach (int item in trackedEnemies)
                    Main.npc[item].active = false;

                trackedEnemies.Clear();
            }

            List<int> types = new()
            {
                ModContent.NPCType<ThornBeholder>(),
                ModContent.NPCType<DimCore>(),
                ModContent.NPCType<PutridNeuroma>()
            };

            trackedEnemies.RemoveAll(x => !Main.npc[x].active || !types.Contains(Main.npc[x].type));

            if (trackedEnemies.Count < 9 || lastSpawnProgress > pestControlProgress - 0.005f)
                pestControlProgress += 0.00001f;

            if ((int)(pestControlProgress * 10000) % 50 == 0 && (int)(pestControlProgress * 10000) != (int)(lastSpawnProgress * 10000))
            {
                var loc = Main.LocalPlayer.Center;
                int type = Main.rand.Next(types);
                var npcRef = ContentSamples.NpcsByNetId[type];
                var pos = npcRef.ModNPC is IPestSpawnNPC pest ? pest.GetSpawnLocation() : GetDefaultSpawnLocation(npcRef);
                pos = pos.ToWorldCoordinates().ToPoint();

                trackedEnemies.Add(NPC.NewNPC(Entity.GetSource_NaturalSpawn(), pos.X, pos.Y, type));
                lastSpawnProgress = pestControlProgress;
            }
        }
    }

    private static Point GetDefaultSpawnLocation(NPC npc)
    {
        while (true)
        {
            Point pos = new(Main.rand.Next(20, Main.maxTilesX - 20), Main.rand.Next(100, Main.maxTilesY - 100));

            if (!Collision.SolidCollision(pos.ToWorldCoordinates(), npc.width, npc.height))
                return pos;
        }
    }
}
