using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria;
using Terraria.ID;
using Terraria.DataStructures;
using Microsoft.Xna.Framework;
using Terraria.Net;
using BeaniesBosses.NPCs.SharkBoss;
using Terraria.Chat;
using Terraria.Localization;
using BeaniesBosses.Items;

namespace BeaniesBosses
{
    internal class ModPlr : ModPlayer
    {
        public override void CatchFish(FishingAttempt attempt, ref int itemDrop, ref int npcSpawn, ref AdvancedPopupRequest sonar, ref Vector2 sonarPosition)
        {
            if (attempt.CanFishInLava && attempt.heightLevel == 4 && attempt.inLava && (!DownedBossSystem.downedPogark || attempt.playerFishingConditions.BaitItemType == ModContent.ItemType<SharkBait>()))
            {
                npcSpawn = ModContent.NPCType<SharkBoss>();
                sonar.Text = "Poggers";
                sonar.Color = Color.OrangeRed;
                sonar.DurationInFrames = 60;
                sonar.Velocity = Vector2.Zero;
                sonarPosition -= Vector2.UnitY * 25;
            }
        }
    }
}
