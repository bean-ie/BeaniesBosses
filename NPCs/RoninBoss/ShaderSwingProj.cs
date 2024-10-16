using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using System;
using Microsoft.Xna.Framework;
using Terraria.GameContent;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Graphics;
using System.Collections.Generic;

namespace MasterMasterMode.Projectiles
{
    public class VertexSwordSlash : ModProjectile
    {
        public float swingRadiusMultiplier = 1;
        public bool followBoss = true;
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.DeathLaser;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 30;
            ProjectileID.Sets.TrailingMode[Type] = 2;
            ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 99999999;
        }
        public override void SetDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 30;
            Projectile.CloneDefaults(ProjectileID.DeathLaser);
            Projectile.Size = new(2);
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;//set this when you call projectile.newprojectile
            Projectile.penetrate = -1;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.extraUpdates = 0;
            Projectile.alpha = 0;
            Projectile.timeLeft = 400;
            Projectile.aiStyle = 0;
            Projectile.scale = 2;
            Projectile.tileCollide = false;
        }
        List<float> oldRotations = new();
        List<Vector2> oldPositions = new();
        List<Vector2> oldRotationCenters = new();
        public int swingTime;//ai0 is swing time already so I don't need this
        public int trailLength;
        public override void AI()
        {
            NPC bossNPC = Main.npc[(int)Projectile.localAI[2]];
            Projectile.localAI[0]++;
            float progressUnclamped = EaseOutExponential(Projectile.localAI[0] / Projectile.ai[0]) * Projectile.localAI[1];
            float rotOffset = Projectile.ai[1] * MathF.PI * 0.25f;
            float rotationOffset = MathF.PI * 1.5f + MathF.PI * 0.25f * Projectile.ai[1];
            Projectile.rotation = GetRotationAmount(1);
            if (MathF.Abs(progressUnclamped) <= 1)
            {
                Projectile.rotation = GetRotationAmount(progressUnclamped);
                Projectile.Center = (Projectile.rotation + rotationOffset).ToRotationVector2() * 50 * Projectile.scale + bossNPC.Center;//wait what if when done swinging it throws the sword lol
            }
            Projectile.rotation += rotOffset;

            {
                if (oldRotations.Count < Projectile.ai[0])
                    oldRotations.Add(Projectile.rotation);//the duration of the swing in frames
                for (int i = oldRotations.Count - 1; i >= 1; i--)
                {
                    oldRotations[i] = oldRotations[i - 1];//move every element down by 1
                }
                if (oldRotations.Count < Projectile.ai[0])
                    oldRotations[0] = Projectile.rotation;//update the first element

                if (oldPositions.Count < Projectile.ai[0])
                    oldPositions.Add(Projectile.Center);
                for (int i = oldPositions.Count - 1; i >= 1; i--)
                {
                    oldPositions[i] = oldPositions[i - 1];
                }
                if (oldPositions.Count < Projectile.ai[0])
                    oldPositions[0] = Projectile.Center;

                if (oldRotationCenters.Count < Projectile.ai[0])
                    oldRotationCenters.Add(bossNPC.Center);
                for (int i = oldRotationCenters.Count - 1; i >= 1; i--)
                {
                    oldRotationCenters[i] = oldRotationCenters[i - 1];
                }
                if (oldRotationCenters.Count < Projectile.ai[0])
                    oldRotationCenters[0] = bossNPC.Center;
            }
            //oldRotations.TrimExcess();
            //oldRotationCenters.TrimExcess();
            //oldPositions.TrimExcess();
            if (Projectile.localAI[0] > Projectile.ai[0] * 2)
                Projectile.Kill();
            Lighting.AddLight(Projectile.Center, Color.Red.ToVector3());
        }
        static float EaseInExponential(float progress)
        {
            float returnValue = MathF.Pow(progress, 2.3f);//MathF.Sin((progress * MathF.PI) / 2); //MathF.Sqrt(1 - MathF.Pow(progress - 1, 2));
            return returnValue;
        }
        static float EaseOutExponential(float progress) 
        {
            float returnValue = 1 - MathF.Pow(1 - progress, 2.3f);//MathF.Sin((progress * MathF.PI) / 2); //MathF.Sqrt(1 - MathF.Pow(progress - 1, 2));
            return returnValue;
        } 
        private float GetRotationAmount(float progressUnclamped)//rename to just progress
        {
            //float rotToMouse = (Main.MouseWorld - Main.player[Projectile.owner].Center).ToRotation();
            //progressUnclamped = 0.5f;
           
            float swingRadius = MathF.PI * 1.4f * swingRadiusMultiplier;//default value is pi * 1.2
            float idkWhatOffsetThisIs = swingRadius * 0.5f + MathF.PI * -0.1f * Projectile.localAI[1];//default value is pi * 0.4
            float returnVal = progressUnclamped * swingRadius * Projectile.ai[1]
                + Projectile.ai[2]//direction of the swing, as in, the global rotation offset so it points towards the cursor 
                - Projectile.ai[1] * idkWhatOffsetThisIs
                - Projectile.ai[1] * 0.2f//???????
                - (Projectile.ai[1] * 0.5f - 0.5f) * MathF.PI;//offset from direction. Add 180 degrees if the direction if left
            return returnVal;
        }

        public float WidthFunction(float progress)
        {
            return 36 * Projectile.scale;
        }
        public Color ColorFunction(float progress)
        {          
            return Color.White;
        }
        Effect shader;
        public void SetEffectParameters(Effect effect)
        {
            effect.Parameters["waveFunctionExponent"].SetValue(3);//float
            effect.Parameters["transparencyMultiplierX"].SetValue(30);//float
            effect.Parameters["transparencyMultiplierY"].SetValue(1);//float
            effect.Parameters["bottomOutlineBoundsPercent"].SetValue(0.95f);//float
            effect.Parameters["upperOutlineBoundsPercent"].SetValue(0f);//float
            effect.Parameters["slashProgress"].SetValue(Projectile.localAI[0] / Projectile.ai[0]);//float
            effect.Parameters["endFadePercentStartPosition"].SetValue(0.9f);//float
            effect.Parameters["endFadePercentStartTime"].SetValue(0.9f);//float
            effect.Parameters["outlineTransparencyMultiplier"].SetValue(2);//float
            effect.Parameters["startColor"].SetValue(new Color(128,100,255).ToVector4());//Vector4 (use color.ToVector4())
            effect.Parameters["middleColor"].SetValue(Color.Blue.ToVector4());//Vector4 (use color.ToVector4())
            effect.Parameters["endColor"].SetValue(new Color(128, 255, 255).ToVector4());//Vector4 (use color.ToVector4())
            effect.Parameters["WorldViewProjection"].SetValue(GetWorldViewProjectionMatrix());
        }
        public static Matrix GetWorldViewProjectionMatrix()
        {
            Matrix view = Matrix.CreateLookAt(Vector3.Zero, Vector3.UnitZ, Vector3.Up) * Matrix.CreateTranslation(Main.graphics.GraphicsDevice.Viewport.Width / 2, Main.graphics.GraphicsDevice.Viewport.Height / -2, 0) * Matrix.CreateRotationZ(MathHelper.Pi) * Matrix.CreateScale(Main.GameViewMatrix.Zoom.X, Main.GameViewMatrix.Zoom.Y, 1f);
            Matrix projection = Matrix.CreateOrthographic(Main.graphics.GraphicsDevice.Viewport.Width, Main.graphics.GraphicsDevice.Viewport.Height, 0, 1000);
            return view * projection;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearWrap, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            if (shader == null)
                shader = ModContent.Request<Effect>("BeaniesBosses/NPCs/RoninBoss/Vertex_SwordSlash", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
            SetEffectParameters(shader);
            shader.CurrentTechnique.Passes[0].Apply();
            VertexStrip vertexStrip = new();
            GetLerpedLists(0.2f, out List<float> lerpedRotations, out List<Vector2> lerpedPositions);
            vertexStrip.PrepareStrip(lerpedPositions.ToArray(), lerpedRotations.ToArray(), ColorFunction, WidthFunction, -Main.screenPosition, includeBacksides: true);
            vertexStrip.DrawTrail();
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            if (Projectile.localAI[0] > 0 + Projectile.ai[0])//add 1000000 because I'm testing
                return false;
            int itemSpriteToUse = ItemID.CobaltSword;
            Main.instance.LoadItem(itemSpriteToUse);
            Texture2D tex = TextureAssets.Item[itemSpriteToUse].Value;
            Player player = Main.player[Projectile.owner];
            Projectile.spriteDirection = (int)Projectile.ai[1];
            float rotationOffset = Projectile.ai[1] * -MathF.PI * 0.25f;
            SpriteEffects fx = Projectile.ai[1] == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            if (Projectile.localAI[1] < 0) 
            {
                //if (Projectile.localAI[0] > Projectile.ai[0])
                //    rotationOffset += Projectile.ai[1] * MathF.PI * 1.2f;
                rotationOffset +=  Projectile.ai[1] * MathF.PI * 0.5f;
                fx = fx == SpriteEffects.None ? fx = SpriteEffects.FlipHorizontally : SpriteEffects.None;
            }
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation + rotationOffset, tex.Size() / 2, Projectile.scale, fx); ;
            return false;
        }

        private void GetLerpedLists(float lerpIncrement, out List<float> lerpedRotations, out List<Vector2> lerpedPositions)
        {
            lerpedRotations = new();
            lerpedPositions = new();
            float startInterpolationIndex = Projectile.localAI[0] - Projectile.ai[0] + 2;
            
            if (startInterpolationIndex < 0)
                startInterpolationIndex = 0;

            for (int i = (int)startInterpolationIndex; i < oldRotationCenters.Count; i++)
            {
                int indexCorrection = i < oldPositions.Count && i != 0 ? 1 : 0;

                for (float j = 0; j < 1; j += lerpIncrement)
                {
                    Vector2 lerpedRotationCenter = Vector2.Lerp(oldRotationCenters[i - indexCorrection], oldRotationCenters[i], j);
                    float oldRotationFromVec2 = (oldPositions[i - indexCorrection] - oldRotationCenters[i - indexCorrection]).ToRotation();
                    float curRotationFromVec2 = (oldPositions[i] - oldRotationCenters[i]).ToRotation();
                    lerpedPositions.Add(Utils.AngleLerp(oldRotationFromVec2, curRotationFromVec2, j).ToRotationVector2() * 50 * Projectile.scale + (followBoss ? Main.npc[(int)Projectile.localAI[2]].Center : lerpedRotationCenter)); //+ Main.player[Projectile.owner].Center); //
                }
            }
            for (int i = (int)startInterpolationIndex; i < oldRotations.Count; i++)
            {
                int indexCorrection = i < oldPositions.Count && i != 0 ? 1 : 0;
                for (float j = 0; j < 1; j += lerpIncrement)
                {
                    lerpedRotations.Add(Utils.AngleLerp(oldRotations[i - indexCorrection], oldRotations[i], j));
                }
            }
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float collisionPoint = 86555;//will be changed if the thing collides with something
            bool collided = false;
            Main.instance.LoadProjectile(ProjectileID.LaserMachinegunLaser);
            GetLerpedLists(0.5f, out List<float> lerpedRotations, out List<Vector2> lerpedPositions);
            for (int i = 0; i < lerpedRotations.Count; i++)
            {
                if (collided)
                    break;
                float length = WidthFunction((float)i / lerpedRotations.Count);
                Vector2 lineStart = lerpedPositions[i] - (lerpedRotations[i] + MathHelper.PiOver2).ToRotationVector2() * length;
                Vector2 lineEnd = lerpedPositions[i] + (lerpedRotations[i] + MathHelper.PiOver2).ToRotationVector2() * length;
                collided = Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), lineStart, lineEnd, 4, ref collisionPoint);
            }
            return collided;
        }
    }
}
