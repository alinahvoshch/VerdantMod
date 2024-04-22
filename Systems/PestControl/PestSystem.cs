using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;
using Verdant.NPCs.Enemy.PestControl;

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
            return;
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
                ModContent.NPCType<DimCore>()
            };

            trackedEnemies.RemoveAll(x => !Main.npc[x].active || !types.Contains(Main.npc[x].type));

            if (trackedEnemies.Count < 9 || lastSpawnProgress > pestControlProgress - 0.005f)
                pestControlProgress += 0.00001f;

            if ((int)(pestControlProgress * 10000) % 50 == 0 && (int)(pestControlProgress * 10000) != (int)(lastSpawnProgress * 10000))
            {
                var loc = Main.LocalPlayer.Center;
                var pos = loc + new Vector2(0, -1000).RotatedByRandom(MathHelper.PiOver2);

                trackedEnemies.Add(NPC.NewNPC(Entity.GetSource_NaturalSpawn(), (int)pos.X, (int)pos.Y, Main.rand.Next(types)));
                lastSpawnProgress = pestControlProgress;
            }
        }
    }
}
