using SubworldLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.GameContent.Generation;
using Terraria.IO;
using Terraria;
using Terraria.ModLoader;
using Terraria.WorldBuilding;
using Verdant.Tiles.Verdant.Basic.Blocks;
using Microsoft.Xna.Framework;
using Verdant.Tiles.Verdant.Decor;
using Verdant.Walls;
using Verdant.Tiles.Verdant.Decor.ThornFurniture;
using Verdant.Systems.PestControl;

namespace Verdant.World.PestControlSubworld;

[JITWhenModsEnabled("SubworldLibrary")]
[ExtendsFromMod("SubworldLibrary")]
internal class PestSubworld : Subworld
{
    public override int Width => 400;
    public override int Height => 340;

    public int OpenLeft => 51;
    public int OpenRight => Width - 51;
    public int OpenTop => 111;
    public int OpenBottom => Height - 100;

    public int OpenCenter => (OpenTop + OpenBottom) / 2;

    public override List<GenPass> Tasks => new() { new PassLegacy("Box", GenerateLand) };

    public void GenerateLand(GenerationProgress progress, GameConfiguration configuration)
    {
        Main.worldSurface = Main.maxTilesY - 42;
        Main.rockLayer = Main.maxTilesY;

        List<float> angles = [];

        for (int i = 0; i < 8; ++i)
            angles.Add(WorldGen.genRand.NextFloat(MathHelper.TwoPi) - MathHelper.Pi);

        for (int i = 0; i < Width; ++i)
            for (int j = 0; j < Height; ++j)
                WorldGen.PlaceTile(i, j, ModContent.TileType<ThornTile>(), true);

        DeformLand(Width / 2, Height / 2, true);
        DeformLand(Width / 2, Height / 2, false);

        for (int i = 0; i < 12; ++i)
        {
            int x = Width / 2 - (int)MathHelper.Lerp(-110, 110, i / 11f);
            int y = Height / 2;
            int dir = WorldGen.genRand.NextBool(2) ? -1 : 1;

            while (!WorldGen.SolidTile(x, y))
                y -= dir;

            Root(x, y + dir, WorldGen.genRand.Next(2, 5), dir);
        }

        for (int i = 0; i < 12; ++i)
        {
            int x = Width / 2;
            int y = Height / 2 - (int)MathHelper.Lerp(-80, 80, i / 11f);
            int dir = WorldGen.genRand.NextBool(2) ? -1 : 1;

            FindPlaceDeadleaf(x, y, dir);
        }
    }

    public static void FindPlaceDeadleaf(int x, int y, int dir)
    {
        while (!WorldGen.SolidTile(x, y))
            x += dir;

        x -= dir;

        PlaceDeadleaf(x, y, -dir);
    }

    private static void PlaceDeadleaf(int x, int y, int direction)
    {
        int length = WorldGen.genRand.Next(30, 60);
        int growthChance = 1;
        bool hammer = true;

        for (int i = 0; i < length; ++i)
        {
            if (Main.tile[x + (direction * i), y].HasTile)
                return;

            WorldGen.PlaceTile(x + (direction * i), y, i == 0 ? ModContent.TileType<ThornTile>() : ModContent.TileType<DeadleafPlatform>(), true);

            if (hammer && i != 0)
            {
                WorldGen.PoundPlatform(x + (direction * i), y);
                hammer = false;
            }

            if (i == 0 || (i < length - 1 && WorldGen.genRand.NextBool(growthChance)))
            {
                if (i == 0)
                    WorldGen.PlaceTile(x + (direction * i), y + 1, ModContent.TileType<ThornTile>(), true);

                y--;
                growthChance += 1;
                hammer = true;
            }
        }
    }

    private void DeformLand(int x, int y, bool kill)
    {
        const int AngleCount = 20;

        Dictionary<float, int> depthsByAngle = [];

        for (int i = 0; i < AngleCount; ++i)
            depthsByAngle.Add((i * MathHelper.TwoPi / (AngleCount - 1)) - MathHelper.Pi, kill ? WorldGen.genRand.Next(60, 90) : WorldGen.genRand.Next(90, 110));

        for (int i = 0; i < Width; ++i)
        {
            for (int j = 0; j < Height; ++j)
            {
                float angle = new Vector2(x, y).AngleTo(new Vector2(i, j));
                float minKey = depthsByAngle.Keys.MinBy(x => Math.Abs(x - angle));
                int firstDist = depthsByAngle[minKey];
                float secondMinKey = depthsByAngle.Keys.Where(x => x != minKey).MinBy(x => Math.Abs(x - angle));
                int secondDist = depthsByAngle[secondMinKey];

                int dist = (int)MathHelper.Lerp(firstDist, secondDist, DetermineFactor(minKey, secondMinKey, angle));

                if (kill && Vector2.DistanceSquared(new Vector2(i / 1.5f, j), new Vector2(x / 1.5f, y)) < dist * dist)
                    WorldGen.KillTile(i, j, false, false, true);
                else if (!kill && Vector2.DistanceSquared(new Vector2(i / 1.5f, j), new Vector2(x / 1.5f, y)) > dist * dist)
                    WorldGen.PlaceWall(i, j, ModContent.WallType<ThornWall>(), true);
            }
        }
    }

    private float DetermineFactor(float minKey, float secondMinKey, float angle)
    {
        if (minKey < secondMinKey)
            return (angle - minKey) / (secondMinKey - minKey);

        return (angle - secondMinKey) / (minKey - secondMinKey);
    }

    public static void Root(int x, int y, int width, int yMult)
    {
        Vector2 pos = new(x, y);
        Vector2 dir = new Vector2(0, 1).RotatedByRandom(MathHelper.PiOver2) * yMult;
        int time = 0;

        while (!WorldGen.SolidTile((int)pos.X, (int)pos.Y))
        {
            for (int i = 0; i < width; ++i)
            {
                var pPos = pos + dir.RotatedBy(MathHelper.PiOver2) * (i - width / 2);
                WorldGen.PlaceWall((int)pPos.X, (int)pPos.Y, ModContent.WallType<ThornWall>(), true);
            }

            pos += dir;
            time++;

            if (WorldGen.genRand.NextBool(40))
            {
                width--;

                if (width <= 0)
                    return;
            }

            if (WorldGen.genRand.NextBool(60))
                Root((int)pos.X, (int)pos.Y, width, yMult);

            if (time > WorldGen.genRand.Next(8, 18))
            {
                dir = new Vector2(0, 1).RotatedByRandom(MathHelper.PiOver2) * yMult;
                time = 0;
            }
        }
    }

    public override void OnEnter()
    {
        ModContent.GetInstance<PestSystem>().pestControlActive = true;
        ModContent.GetInstance<PestSystem>().pestControlProgress = 0;
    }

    public override void OnExit()
    {
        ModContent.GetInstance<PestSystem>().pestControlActive = false;
        ModContent.GetInstance<PestSystem>().pestControlProgress = 0;
    }

    public override void DrawMenu(GameTime gameTime)
    {
        base.DrawMenu(gameTime);

        if (!HardmodeApotheosis.blackoutTime.HasValue)
            return;

        HardmodeApotheosis.blackoutTime -= 0.4f;

        if (HardmodeApotheosis.blackoutTime <= 0)
            HardmodeApotheosis.blackoutTime = null;
    }
}