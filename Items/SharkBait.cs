using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria;
using Terraria.ID;

namespace BeaniesBosses.Items
{
    internal class SharkBait : ModItem
    {
        public override void SetDefaults()
        {
            Item.CloneDefaults(ItemID.HellButterfly);
            Item.bait = 420;
            ItemID.Sets.IsLavaBait[Type] = true;
        }
    }
}