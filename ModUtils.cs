using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.GameContent;
using Terraria.ID;
using Terraria;

namespace BeaniesBosses
{
    public static class ModUtils
    {
        public static void DrawTelegraphRing(float timerVar, float start, float end, float startScale, Vector2 ringCenter, Color ringColor, byte ringAlpha = 0)
        {
            if (timerVar > end) return;
            float duration = end - start;
            float progress = (timerVar - start) / duration;
            Main.instance.LoadProjectile(ProjectileID.PrincessWeapon);
            Texture2D ringTexture = TextureAssets.Projectile[ProjectileID.PrincessWeapon].Value;
            ringColor.A = ringAlpha;
            float colorMult = 1 - MathF.Pow(1 - progress, 2);
            colorMult *= 0.125f;
            for (float i = 0; i < 8; i++)
            {
                Main.EntitySpriteDraw(ringTexture, ringCenter - Main.screenPosition, null, ringColor * colorMult, i / 8f * MathHelper.PiOver2, ringTexture.Size() / 2, (1 - progress) * startScale, SpriteEffects.None, 0);
            }
        }
        public static void DrawTelegraphLine(float timerVar, float start, float end, float lineWidth, float lineLength, Color lineColor, Vector2 lineStartPos, float lineRotation)
        {
            float duration = end - start;
            float progress = (timerVar - start) / duration;
            lineWidth = (1 - progress) * lineWidth;
            lineStartPos += new Vector2(lineWidth / 2, 0).RotatedBy(lineRotation - MathHelper.PiOver2);
            Vector2 lineEnd = lineStartPos + lineRotation.ToRotationVector2() * lineLength;
            lineColor.A = 0;
            if (lineWidth > 0 && timerVar > start)
                Utils.DrawLine(Main.spriteBatch, lineStartPos, lineEnd, lineColor, Color.Transparent, lineWidth);
        }
    }
}
