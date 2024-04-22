using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Graphics.Capture;
using Terraria.ModLoader;
using Verdant.World.PestControlSubworld;

namespace Verdant.Scenes;

internal class PestControlBiome : ModBiome
{
    public override ModWaterStyle WaterStyle => ModContent.Find<ModWaterStyle>("Verdant/VerdantWaterStyle");
    public override ModSurfaceBackgroundStyle SurfaceBackgroundStyle => ModContent.Find<ModSurfaceBackgroundStyle>("Verdant/PestControlBg");
    public override CaptureBiome.TileColorStyle TileColorStyle => CaptureBiome.TileColorStyle.Mushroom;
    public override SceneEffectPriority Priority => SceneEffectPriority.BossHigh;

    public override int Music => -1;// GetMusic();

    private int GetMusic()
    {
        if (Main.raining)
            return MusicLoader.GetMusicSlot(Mod, "Sounds/Music/PetalsFall");
        return MusicLoader.GetMusicSlot(Mod, "Sounds/Music/VibrantHorizon");
    }

    public override string BestiaryIcon => base.BestiaryIcon;
    public override string BackgroundPath => MapBackground;
    public override Color? BackgroundColor => new Color(145, 137, 94);
    public override string MapBackground => "Verdant/Backgrounds/PestControl/PestMap";

    public override bool IsBiomeActive(Player player) => SubworldLibrary.SubworldSystem.IsActive<PestSubworld>();
}