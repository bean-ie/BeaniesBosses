using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria;
using Terraria.ID;
using Terraria.GameContent.ItemDropRules;
using Terraria.GameContent.Items;
using BeaniesBosses.Items;

namespace BeaniesBosses.NPCs
{
    internal class BeanieNPC : GlobalNPC
    {
        public override void ModifyGlobalLoot(GlobalLoot globalLoot)
        {
            LeadingConditionRule sharkBaitRule = new LeadingConditionRule(new InHellDropCondition());
            sharkBaitRule.OnSuccess(ItemDropRule.ByCondition(new PogarkDownedDropCondition(), ModContent.ItemType<SharkBait>(), 20));
            globalLoot.Add(sharkBaitRule);
        }
    }

    public class InHellDropCondition : IItemDropRuleCondition
    {
        public bool CanDrop(DropAttemptInfo info)
        {
            return info.player.ZoneUnderworldHeight;
        }

        public bool CanShowItemDropInUI()
        {
            return true;
        }

        public string GetConditionDescription()
        {
            return "Drops only in the underworld";
        }
    }

    public class PogarkDownedDropCondition : IItemDropRuleCondition
    {
        public bool CanDrop(DropAttemptInfo info)
        {
            return DownedBossSystem.downedPogark;
        }

        public bool CanShowItemDropInUI()
        {
            return true;
        }

        public string GetConditionDescription()
        {
            return "Drops only after you defeat Duchess Pogark";
        }
    }
}
