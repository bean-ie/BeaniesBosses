using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.Localization;
using Microsoft.Xna.Framework;
using Terraria.Audio;
using Microsoft.Xna.Framework.Graphics;
using Terraria.DataStructures;
using Terraria.Chat;
using Terraria.GameContent.Bestiary;

namespace BeaniesBosses.NPCs.SharkBoss
{
    [AutoloadBossHead]
    internal class SharkBoss : ModNPC
    {
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 4;
            NPCID.Sets.BossBestiaryPriority.Add(NPC.type);
        }
        public override void SetDefaults()
        {
            NPC.boss = true;
            NPC.noGravity = true;
            NPC.width = 42;
            NPC.height = 42;
            NPC.damage = 35;
            NPC.lifeMax = 4500;
            NPC.noTileCollide = true;
            NPC.npcSlots = 10f;
            NPC.knockBackResist = 0f;
            NPC.lavaImmune = true;
            NPC.defense = 15;
            NPC.aiStyle = -1;
            NPC.HitSound = SoundID.NPCHit18;
            NPC.timeLeft = NPC.activeTime * 30;
            Music = MusicLoader.GetMusicSlot(Mod, "Assets/Sounds/Music/SharkBossMusic");
            SceneEffectPriority = SceneEffectPriority.BossHigh;
        }
        /*public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)
        {
            NPC.lifeMax = (int)(NPC.lifeMax * balance * bossAdjustment * 0.8f);
        }*/
        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.TheUnderworld,
                new FlavorTextBestiaryInfoElement("The Duchess of Hell, Pogark, defends the lava lakes deep down in the underworld.")
            });
        }

        public override void OnSpawn(IEntitySource source)
        {
            NPC.Center += Vector2.UnitY * 50;
            if (Main.netMode == NetmodeID.SinglePlayer)
            {
                Main.NewText("Duchess Pogark has awoken!", 175, 75, 255);
            }
            else
            {
                ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral("Duchess Pogark has awoken!"), new Color(175, 75, 255));
            }
        }
        Vector2 directionToTarget, target;
        public override void AI()
        {
            Player player = Main.player[NPC.target];
            if (!AliveCheck(player))
            {
                NPC.EncourageDespawn(30);
                return;
            }

            switch (NPC.ai[3])
            {
                case 0: Initialize(); break;

                    //p1
                case 1: DashAttack(player, 2); break;
                case 2: ShootFireballs(player, 3); break;
                case 3: DashAttack(player, 4); break;
                case 4: JoeBiden(player, 1); break;

                    //p2
                case 5: P2Initialize(); break;
                case 6: DashAndFire(player, 7); break;
                case 7: CircleFireballs(player, 8); break;
                case 8: LavaRainP2(player, 9); break;
                case 9: TeleportFire(player, 6); break;
                default:
                    NPC.rotation += MathHelper.Pi / 180;
                    break;
            }

        }
        void Initialize()
        {
            NPC.direction = NPC.spriteDirection = NPC.Center.X > target.X ? 1 : -1;
            if (NPC.localAI[0] == 0)
            {
                NPC.velocity = -Vector2.UnitY;
                SoundEngine.PlaySound(new SoundStyle("BeaniesBosses/NPCs/SharkBoss/SharkSound") with { MaxInstances = 0 }, NPC.Center);
                NPC.localAI[0] = 1;
            } 
            if (NPC.ai[0] > 90)
            {
                SwitchAttack(1);
                NPC.ai[0] = 0;
                NPC.velocity = Vector2.Zero;
            }
            else NPC.ai[0]++;   
        }

        void DashAttack(Player player, int nextAttack)
        {
            if (Phase2Check()) return;
            if (NPC.ai[0] < 30)
            {
                NPC.velocity *= 0.9f;
                target = player.Center;
                directionToTarget = target - NPC.Center;
                directionToTarget.Normalize();
                NPC.direction = NPC.spriteDirection = NPC.Center.X > target.X ? 1 : -1;
                NPC.rotation = directionToTarget.ToRotation() + MathHelper.PiOver2 * (NPC.spriteDirection + 1);
            }
            else if (NPC.ai[0] == 30)
            {
                SoundEngine.PlaySound(SoundID.Unlock, NPC.Center);
                for (int i = 0; i < 5; i++)
                {
                    Dust.NewDust(NPC.Center, 0, 0, DustID.Flare, Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-2f, 2f), 110, Color.White, 1f);
                }
            }
            else if (NPC.ai[0] == 60)
            {
                string soundPath = "BeaniesBosses/NPCs/SharkBoss/Moon_" + Main.rand.Next(0, 2);
                SoundEngine.PlaySound(new SoundStyle(soundPath) with { MaxInstances = 0, Pitch = 1.3f }, NPC.Center);
                NPC.velocity = directionToTarget * 40;
            }
            else if (NPC.ai[0] >= 75)
            {
                NPC.velocity *= 0.7f;
            }
            if (NPC.ai[0] >= 85)
            {
                NPC.ai[0] = 0;
                NPC.ai[1]++;
                NPC.velocity = Vector2.Zero;
            }
            if (NPC.ai[1] >= 4)
            {
                //RandomPhase1Attack();
                SwitchAttack(nextAttack);
                return;
            }
            NPC.ai[0]++;
        }
        
        void ShootFireballs(Player player, int nextAttack)
        {
            if (Phase2Check()) return;
            Vector2 directionToPlayer = player.Center - NPC.Center;
            directionToPlayer.Normalize();
            target = player.Center + directionToPlayer * -350;
            NPC.rotation = (player.Center - NPC.Center).ToRotation() + MathHelper.PiOver2 * (NPC.spriteDirection + 1);
            NPC.direction = NPC.spriteDirection = NPC.Center.X > player.Center.X ? 1 : -1;
            directionToTarget = target - NPC.Center;
            NPC.velocity = directionToTarget * (Vector2.Distance(NPC.Center, target) > 5 ? 0.08f : 0.04f);
            if (NPC.localAI[0] <= 30)
            {
                NPC.localAI[0]++;
            }
            if (NPC.ai[1] < 6)
            {
                if (NPC.localAI[0] > 30)
                {
                Vector2 spawnOffset = new Vector2(50 * -NPC.spriteDirection, 14);
                if (NPC.ai[0] >= 60)
                {
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center + spawnOffset.RotatedBy(NPC.rotation), directionToPlayer * 15, ModContent.ProjectileType<SharkFire>(), 20, 2, player.whoAmI);
                    NPC.ai[0] = 0;
                    NPC.ai[1]++;
                    SoundEngine.PlaySound(SoundID.DD2_BetsyFireballShot, NPC.Center);
                }
                NPC.ai[0]++;    
                }
            }
            else SwitchAttack(nextAttack);
        }

        void JoeBiden(Player player, int nextAttack)
        {
            if (Phase2Check()) return;
            Vector2 originalPos = NPC.Center;
            if (NPC.localAI[0] == 0)
            {
                target = player.Center + new Vector2(400 * (NPC.Center.X > player.Center.X ? 1 : -1), 0);
                NPC.localAI[0] = 1;
            }
            if (NPC.localAI[0] == 1)
            {
                if (Vector2.Distance(target, NPC.Center) > 10)
                {
                    //directionToTarget = target - NPC.Center;
                    //Vector2 normalizedDir = directionToTarget;
                    //normalizedDir.Normalize();
                    //NPC.velocity = Vector2.Max((directionToTarget / 20), normalizedDir);
                    NPC.rotation = (player.Center - NPC.Center).ToRotation() + MathHelper.PiOver2 * (NPC.spriteDirection + 1);
                    NPC.direction = NPC.spriteDirection = NPC.Center.X > player.Center.X ? 1 : -1;

                    NPC.Center = Vector2.Lerp(originalPos, target, NPC.ai[0] / 180);
                    NPC.ai[0]++;
                }
                else
                {
                    NPC.localAI[0] = 2;
                    NPC.ai[0] = 0;
                }
            }
            if (NPC.localAI[0] == 2)
            {
                NPC.rotation = 0;
                Vector2 spawnOffset = new Vector2(50 * -NPC.spriteDirection, 14);
                if (NPC.ai[0] == 0)
                    NPC.direction = NPC.spriteDirection = NPC.Center.X > player.Center.X ? 1 : -1;
                else if (NPC.ai[0] == 60)
                {
                    for (int i = -3; i <= 3; i++)
                    {
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center + spawnOffset.RotatedBy(NPC.rotation), Vector2.UnitX.RotatedBy(MathHelper.ToRadians(15 * i)) * 20 * -NPC.spriteDirection, ModContent.ProjectileType<SharkFire>(), 25, 4, player.whoAmI);
                    }
                    SoundEngine.PlaySound(SoundID.DD2_BetsyFireballShot, NPC.Center);
                }
                else if (NPC.ai[0] == 90)
                    NPC.direction = NPC.spriteDirection = NPC.Center.X > player.Center.X ? 1 : -1;
                else if (NPC.ai[0] == 120)
                {
                    for (int i = -5; i <= 5; i += 2)
                    {
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center + spawnOffset.RotatedBy(NPC.rotation), Vector2.UnitX.RotatedBy(MathHelper.ToRadians(7.5f * i)) * 20 * -NPC.spriteDirection, ModContent.ProjectileType<SharkFire>(), 25, 4, player.whoAmI);
                    }
                    SoundEngine.PlaySound(SoundID.DD2_BetsyFireballShot, NPC.Center);
                }
                else if (NPC.ai[0] == 150) SwitchAttack(nextAttack);
                NPC.ai[0]++;
            }
        }

        void RandomPhase1Attack()
        {
            NPC.ai[3] = Main.rand.Next(1, 3);
            NPC.ai[0] = 0;
            NPC.ai[1] = 0;
            NPC.localAI[0] = 0;
        }

        bool Phase2Check()
        {
            if (NPC.life <= NPC.lifeMax / 2) { SwitchAttack(5); return true; } return false;
        }

        void P2Initialize()
        {
            NPC.velocity *= 0.9f;
            if (NPC.ai[0] == 0)
            {
                NPC.dontTakeDamage = true;
            }
            else if (NPC.ai[0] == 120)
            {
                SoundEngine.PlaySound(new SoundStyle("BeaniesBosses/NPCs/SharkBoss/SharkSound") with { MaxInstances = 0 }, NPC.Center);
                for (int i = 0; i < 20; i++)
                {
                    Dust.NewDust(NPC.Center, 0, 0, DustID.Flare, Main.rand.NextFloat(-5f, 5f), Main.rand.NextFloat(-5f, 5f), 110, Color.White, 2f);
                }
            }
            else if (NPC.ai[0] >= 240)
            {
                NPC.dontTakeDamage = false;
                SwitchAttack(6);
            }
            NPC.ai[0]++;
        }

        void DashAndFire(Player player, int nextAttack)
        {
            if (NPC.ai[0] < 25)
            {
                NPC.velocity *= 0.9f;
                target = player.Center;
                directionToTarget = target - NPC.Center;
                directionToTarget.Normalize();
                NPC.direction = NPC.spriteDirection = NPC.Center.X > target.X ? 1 : -1;
                NPC.rotation = directionToTarget.ToRotation() + MathHelper.PiOver2 * (NPC.spriteDirection + 1);
            }
            else if (NPC.ai[0] == 25)
            {
                SoundEngine.PlaySound(SoundID.Unlock, NPC.Center);
                for (int i = 0; i < 5; i++)
                {
                    Dust.NewDust(NPC.Center, 0, 0, DustID.Flare, Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-2f, 2f), 110, Color.White, 1f);
                }
            }
            else if (NPC.ai[0] == 50)
            {
                string soundPath = "BeaniesBosses/NPCs/SharkBoss/Moon_" + Main.rand.Next(0, 2);
                SoundEngine.PlaySound(new SoundStyle(soundPath) with { MaxInstances = 0, Pitch = 1.3f }, NPC.Center);
                NPC.velocity = directionToTarget * 50;
                Vector2 spawnOffset = new Vector2(50 * -NPC.spriteDirection, 14);
                for (int i = -2; i <= 2; i++)
                {
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center + spawnOffset.RotatedBy(NPC.rotation), directionToTarget.RotatedBy(MathHelper.ToRadians(15 * i)) * 15, ModContent.ProjectileType<SharkFire>(), 15, 4, player.whoAmI, 1);
                }
            }
            else if (NPC.ai[0] >= 60)
            {
                NPC.velocity *= 0.7f;
            }
            if (NPC.ai[0] >= 75)
            {
                NPC.ai[0] = 0;
                NPC.ai[1]++;
                NPC.velocity = Vector2.Zero;
            }
            if (NPC.ai[1] >= 4)
            {
                //RandomPhase1Attack();
                SwitchAttack(nextAttack);
                return;
            }
            NPC.ai[0]++;
        }

        int multiplier;
        Vector2 originalPosition;
        void CircleFireballs(Player player, int nextAttack)
        {
            NPC.rotation = (player.Center - NPC.Center).ToRotation() + MathHelper.PiOver2 * (NPC.spriteDirection + 1);
            NPC.direction = NPC.spriteDirection = NPC.Center.X > player.Center.X ? 1 : -1;
            if (NPC.localAI[0] == 0)
            {
                originalPosition = NPC.Center;
            }
            if (NPC.localAI[0] < 30)
            {
                multiplier = NPC.Center.X > player.Center.X ? 1 : -1;
                target = player.Center + Vector2.UnitX * 350 * multiplier;
                NPC.Center = Vector2.Lerp(originalPosition, target, NPC.localAI[0]/30);
                NPC.localAI[0]++;
            }
            else
            {
            Vector2 directionToPlayer = player.Center - NPC.Center;
            directionToPlayer.Normalize();
            NPC.Center = player.Center + Vector2.UnitX.RotatedBy(MathHelper.ToRadians(NPC.ai[2])) * 350 * multiplier;
            NPC.ai[2] += 2;
            if (NPC.localAI[1] < 60) NPC.localAI[1]++;
            else if (NPC.ai[1] < 20)
            {
                Vector2 spawnOffset = new Vector2(50 * -NPC.spriteDirection, 14);
                if (NPC.ai[0] >= 20)
                {
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center + spawnOffset.RotatedBy(NPC.rotation), directionToPlayer * 15, ModContent.ProjectileType<SharkFire>(), 20, 2, player.whoAmI, 1);
                    NPC.ai[0] = 0;
                    NPC.ai[1]++;
                    SoundEngine.PlaySound(SoundID.DD2_BetsyFireballShot, NPC.Center);
                }
                NPC.ai[0]++;
            }
            else SwitchAttack(nextAttack);
            }
        }

        void LavaRainP2(Player player, int nextAttack)
        {
            if (NPC.localAI[0] == 0)
            {
                originalPosition = NPC.Center;
                target = player.Center + Vector2.UnitX * 900 * NPC.spriteDirection + Vector2.UnitY * -300;
                NPC.ai[1] = 10;
            }
            if (NPC.localAI[0] < 30)
            {
                NPC.Center = Vector2.Lerp(originalPosition, target, NPC.localAI[0] / 30);
                NPC.localAI[0]++;
            }
            else
            {
                NPC.rotation = 0;
                NPC.velocity = Vector2.UnitX * 15 * -NPC.spriteDirection;
                if (NPC.ai[0] < 90)
                {
                    if (NPC.ai[1] >= 10)
                    {
                        Vector2 spawnOffset = new Vector2(50 * -NPC.spriteDirection, 14);
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center + spawnOffset.RotatedBy(NPC.rotation), Vector2.UnitY * 15, ModContent.ProjectileType<SharkFire>(), 25, 2, player.whoAmI, 1);
                        SoundEngine.PlaySound(SoundID.DD2_BetsyFireballShot, NPC.Center);
                        NPC.ai[1] = 0;
                    }
                    NPC.ai[0]++;
                    NPC.ai[1]++;
                }
                else SwitchAttack(nextAttack);
            }
        }

        Vector2 directionToShoot;
        void TeleportFire(Player player, int nextAttack)
        {
            NPC.velocity = Vector2.Zero;
            if (NPC.ai[0] == 30 && NPC.ai[1] <= 4)
            {
                for (int i = 0; i < 20; i++)
                {
                    Dust.NewDust(NPC.Center, 0, 0, DustID.Flare, Main.rand.NextFloat(-5f, 5f), Main.rand.NextFloat(-5f, 5f), 110, Color.White, 2f);
                }
                NPC.Center = player.Center + Vector2.UnitX.RotatedBy(MathHelper.ToRadians(120 * NPC.ai[2])) * 300;
                NPC.direction = NPC.spriteDirection = NPC.Center.X > player.Center.X ? 1 : -1;
                NPC.rotation = (player.Center - NPC.Center).ToRotation() + MathHelper.PiOver2 * (NPC.spriteDirection + 1);
                directionToShoot = player.Center - NPC.Center;
                directionToShoot.Normalize();
                for (int i = 0; i < 20; i++)
                {
                    Dust.NewDust(NPC.Center, 0, 0, DustID.Flare, Main.rand.NextFloat(-5f, 5f), Main.rand.NextFloat(-5f, 5f), 110, Color.White, 2f);
                }
            }
            else if (NPC.ai[0] == 60 && NPC.ai[1] < 3)
            {
                Vector2 spawnOffset = new Vector2(50 * -NPC.spriteDirection, 14);
                Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center + spawnOffset.RotatedBy(NPC.rotation), directionToShoot * 15, ModContent.ProjectileType<SharkFire>(), 20, 2, player.whoAmI, 1);
                SoundEngine.PlaySound(SoundID.DD2_BetsyFireballShot, NPC.Center);
                NPC.ai[0] = 0;
                NPC.ai[1]++;
                NPC.ai[2]++;
            }
            else if (NPC.ai[1] == 3) SwitchAttack(nextAttack);
            NPC.ai[0]++;
        }

        void SwitchAttack(int newAttack)
        {
            NPC.ai[3] = newAttack;
            NPC.ai[0] = 0;
            NPC.ai[1] = 0;
            NPC.ai[2] = 0;
            NPC.localAI[0] = 0;
            NPC.localAI[1] = 0;
        }
        

        bool AliveCheck(Player p)
        {
            if ((!p.active || p.dead || Vector2.Distance(NPC.Center, p.Center) > 5000f) || !p.ZoneUnderworldHeight)
            {
                NPC.TargetClosest();
                p = Main.player[NPC.target];
                if (!p.active || p.dead || Vector2.Distance(NPC.Center, p.Center) > 5000f || !p.ZoneUnderworldHeight)
                {
                    NPC.velocity.Y += 1f;
                    return false;
                }
            }

            if (NPC.timeLeft < 3600)
                NPC.timeLeft = 3600;

            return true;
        }
        public override void FindFrame(int frameHeight)
        {
            int startFrame = 0;
            int finalFrame = 3;

            int frameSpeed = 5;
            NPC.frameCounter += 0.5f;
            NPC.frameCounter += NPC.velocity.Length() / 10f;
            if (NPC.frameCounter > frameSpeed)
            {
                NPC.frameCounter = 0;
                NPC.frame.Y += frameHeight;

                if (NPC.frame.Y > finalFrame * frameHeight)
                {
                    NPC.frame.Y = startFrame * frameHeight;
                }
            }
        }
        Vector2 telegraphOffset = new Vector2(0, 14);
        public override Color? GetAlpha(Color drawColor)
        {
            return new Color(255, 200, 200);
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (NPC.ai[3] == 9)
            {
                ModUtils.DrawTelegraphLine(NPC.ai[0], 30, 60, 3, 1500, Color.OrangeRed, NPC.Center + telegraphOffset.RotatedBy(NPC.rotation), directionToShoot.ToRotation());
            }
            if (NPC.ai[3] == 4 && NPC.localAI[0] == 2)
            {
                for (int i = -3; i <= 3; i++)
                {
                    ModUtils.DrawTelegraphLine(NPC.ai[0], 30, 60, 3, 1500, Color.OrangeRed, NPC.Center + telegraphOffset.RotatedBy(NPC.rotation), MathHelper.ToRadians(i * 15) + MathHelper.PiOver2 * (NPC.spriteDirection + 1));
                }
                for (int i = -5; i <= 5; i+=2)
                {
                    ModUtils.DrawTelegraphLine(NPC.ai[0], 90, 120, 3, 1500, Color.OrangeRed, NPC.Center + telegraphOffset.RotatedBy(NPC.rotation), MathHelper.ToRadians(i * 7.5f) + MathHelper.PiOver2 * (NPC.spriteDirection + 1));
                }
            }
            //if (NPC.ai[3] == 1) ModUtils.DrawTelegraphLine(NPC.ai[0], 30, 60, 3, 1500, Color.OrangeRed, NPC.Center + telegraphOffset.RotatedBy(NPC.rotation), directionToTarget.ToRotation());
            return base.PreDraw(spriteBatch, screenPos, drawColor);
        }
        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life <= 0) SoundEngine.PlaySound(new SoundStyle("BeaniesBosses/NPCs/SharkBoss/SharkDeathSound") with { Pitch = 1.5f }, NPC.Center);
        }
        public override void OnKill()
        {
            NPC.SetEventFlagCleared(ref DownedBossSystem.downedPogark, -1);
        }
    }
}
