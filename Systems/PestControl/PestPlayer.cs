using Microsoft.Xna.Framework;
using SubworldLibrary;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Verdant.World;
using Verdant.World.PestControlSubworld;

namespace Verdant.Systems.PestControl;

[JITWhenModsEnabled("SubworldLibrary")]
internal class PestPlayer : ModPlayer
{
    public bool inPestControl = false;

    public bool InPestControl => SubworldSystem.Current is PestSubworld;

    public override void ResetEffects()
    {
        inPestControl = false;

        if (ModContent.GetInstance<VerdantGenSystem>().apotheosisLocation is not null)
        {
            var apoth = ModContent.GetInstance<VerdantGenSystem>().apotheosisLocation.Value;
            var loc = new Vector2(apoth.X, apoth.Y).ToWorldCoordinates();

            if (ModContent.GetInstance<PestSystem>().pestControlActive && Player.DistanceSQ(loc) / (16 * 16) < 120 * 120)
            {
                Player.noBuilding = true;
                Player.AddBuff(BuffID.NoBuilding, 2);
                inPestControl = true;
            }
        }
    }
}