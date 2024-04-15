﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using Terraria;

namespace Verdant.Systems.ScreenText
{
    internal class ScreenTextManager
    {
        internal static string Speaker = string.Empty;
        internal static ScreenText CurrentText = null;
        internal static RenderTarget2D textTarget = null;
        internal static bool forceUpdate = false;

        internal static void Update()
        {
            if (CurrentText != null && (forceUpdate || (!Main.gameMenu && !Main.mapFullscreen)))
            {
                CurrentText.Update(Main.gameTimeCache);

                if (!CurrentText.active)
                    CurrentText = CurrentText.Next;
            }
        }

        internal static void Draw()
        {
            if (!Main.mapFullscreen)
                CurrentText?.Draw();
        }

        internal static void Render()
        {
            Main.spriteBatch.End();

            textTarget ??= new RenderTarget2D(Main.graphics.GraphicsDevice, Main.displayWidth.Max(), Main.displayHeight.Max(), false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);

            Main.graphics.GraphicsDevice.PresentationParameters.RenderTargetUsage = RenderTargetUsage.PreserveContents;

            var bindings = Main.graphics.GraphicsDevice.GetRenderTargets();
            Main.graphics.GraphicsDevice.SetRenderTarget(textTarget);
            Main.graphics.GraphicsDevice.Clear(Color.Transparent);
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.UIScaleMatrix);

            if (!Main.gameMenu)
                Draw();

            Main.spriteBatch.End();

            Main.graphics.GraphicsDevice.SetRenderTargets(bindings);
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, GetTextEffect(), Main.UIScaleMatrix);

            Main.spriteBatch.Draw(textTarget, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, 1 / Main.UIScale, SpriteEffects.None, 0);
            Main.spriteBatch.End();

            Main.graphics.GraphicsDevice.PresentationParameters.RenderTargetUsage = RenderTargetUsage.DiscardContents;
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.UIScaleMatrix);
        }

        internal static Effect GetTextEffect()
        {
            var effect = CurrentText is null || CurrentText.shader is null ? null : CurrentText.shader.Value;

            if (effect is not null)
            {
                effect.Parameters["timer"].SetValue(Main.GameUpdateCount * CurrentText.shaderParams.Timer);
                effect.Parameters["scale"].SetValue(CurrentText.shaderParams.Scale);
                effect.Parameters["scale2"].SetValue(CurrentText.shaderParams.Scale2);
            }

            return effect;
        }

        internal static void DrawAdditive()
        {
            if (!Main.mapFullscreen)
                CurrentText?.DrawAdditive();
        }
    }
}
