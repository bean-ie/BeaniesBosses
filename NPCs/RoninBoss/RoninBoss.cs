using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using MasterMasterMode.Projectiles;
using Microsoft.Xna.Framework.Graphics;

namespace BeaniesBosses.NPCs.RoninBoss
{
    [AutoloadBossHead]
    internal class RoninBoss : ModNPC
    {
        public override void SetDefaults()
        {
            NPC.width = 34;
            NPC.height = 48;
            NPC.aiStyle = -1;
            NPC.boss = true;
            NPC.lifeMax = 15000;
            NPC.damage = 30;
            NPC.npcSlots = 20f;
            NPC.knockBackResist = 0f;
            NPC.noGravity = true;
            NPC.lavaImmune = true;
            NPC.timeLeft = NPC.activeTime * 30;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.defense = 20;
            NPC.noTileCollide = true;
        }
        public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)
        {
            NPC.lifeMax = (int)(NPC.lifeMax * balance * bossAdjustment * 0.8f);
        }
        List<int> attackQueue = new List<int>();
        int maxAttacks = 2;
        public override void AI()
        {
            Player player = Main.player[NPC.target];
            if (!AliveCheck(player))
            {
                NPC.EncourageDespawn(30);
                return;
            }

            switch (NPC.ai[3]) {
                case 0:
                    Initialize();
                    break;
                case 1: HorizontalDashSlash(player); break;
                case 2: PushSwordSlash(player); break;
            }
        }

        public void Initialize()
        {
            Main.NewText("You massacred my brothers... It's about time you pay.");
            SwitchAttack();
        }

        public void SwitchAttack()
        {
            NPC.ai[0] = 0;
            NPC.ai[1] = 0;
            NPC.ai[2] = 0;
            NPC.localAI[0] = 0;
            NPC.localAI[1] = 0;
            if (attackQueue.Count == 0)
            {
                int nextAttack = Main.rand.Next(0, maxAttacks);
                while (nextAttack == NPC.ai[3]) nextAttack = Main.rand.Next(0, maxAttacks);
                NPC.ai[3] = nextAttack + 1;
            } else
            {
                NPC.ai[3] = attackQueue[0];
                attackQueue.RemoveAt(0);
            }
        }

        Vector2 offset = Vector2.Zero;
        Vector2 target;

        int direction;
        public void PushSwordSlash(Player player)
        {
            offset = Vector2.One.RotatedBy(NPC.Center.DirectionTo(player.Center).ToRotation()) * 100;
            target = player.Center + offset;
            if (NPC.ai[0] == 0)
            {
                direction = MathF.Sign(Main.rand.NextFloat(-1, 1));
                NPC.Center = player.Center + new Vector2(300 * direction, -300);
            }
            if (NPC.ai[0] < 30)
            {
                NPC.velocity = Vector2.Zero;
            }
            else if (NPC.ai[0] == 30)
            {
                NPC.velocity = NPC.Center.DirectionTo(target) * 35;
                SwordSlash(25, 15, MathHelper.ToRadians(direction == 1 ? 45 : 315), -1, 0.001f, false);
            }
            else if (NPC.ai[0] > 45)
            {
                NPC.velocity *= 0.7f;
            }
            if (NPC.ai[0] == 60)
            {
                NPC.ai[0] = -1;
            }
            NPC.ai[0]++;
        }

        public void HorizontalDashSlash(Player player)
        {
            offset.X = 300 * NPC.spriteDirection;
            target = player.Center + offset;
            Vector2 directionToTarget = target - NPC.Center;
            if (NPC.ai[0] < 30)
            {
                NPC.velocity = directionToTarget * (Vector2.Distance(NPC.Center, target) > 5 ? 0.08f : 0.04f);
                NPC.spriteDirection = player.Center.X < NPC.Center.X ? 1 : -1;
            }
            else if (NPC.ai[0] == 30)
            {
                NPC.velocity = Vector2.Zero;
            }
            else if (NPC.ai[0] == 50)
            {
                SwordSlash(25, 15, MathHelper.PiOver2 * (NPC.spriteDirection + 1), NPC.Center.Y > player.Center.Y ? 1 : -1);
                NPC.velocity.X = 35 * -NPC.spriteDirection;
                NPC.velocity.Y = 0;
            }
            else if (NPC.ai[0] >= 70)
            {
                NPC.velocity *= 0.8f;
            }
            if (NPC.ai[0] >= 90)
            {
                NPC.ai[0] = 0;
            }
            NPC.ai[0]++;
        }

        public void SwordSlash(int damage, int duration, float direction, int orientation, float swingRadiusMultiplier = 1, bool trailFollowBoss = true)
        {
            Projectile proj = Projectile.NewProjectileDirect(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<VertexSwordSlash>(), damage, 2, NPC.target, duration, NPC.spriteDirection, direction);
            proj.localAI[1] = -orientation;
            proj.localAI[2] = NPC.whoAmI;
            VertexSwordSlash modProj = proj.ModProjectile as VertexSwordSlash;
            modProj.swingRadiusMultiplier = swingRadiusMultiplier;
            modProj.followBoss = trailFollowBoss;
        }
        bool AliveCheck(Player p)
        {
            if ((!p.active || p.dead || Vector2.Distance(NPC.Center, p.Center) > 5000f))
            {
                NPC.TargetClosest();
                p = Main.player[NPC.target];
                if (!p.active || p.dead || Vector2.Distance(NPC.Center, p.Center) > 5000f)
                {
                    NPC.velocity.Y += 1f;
                    return false;
                }
            }

            if (NPC.timeLeft < 3600)
                NPC.timeLeft = 3600;
            return true;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            // ModUtils.DrawTelegraphLine(NPC.ai[0], 30, 50, 500, 700, Color.Blue, NPC.Center, MathHelper.PiOver2 * (NPC.spriteDirection + 1));
            ModUtils.DrawTelegraphRing(NPC.ai[0], 0, 50, 2.5f, NPC.Center, Color.Blue);
            return true;
        }
    }
}
