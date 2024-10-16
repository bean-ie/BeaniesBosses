using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria;
using Terraria.ModLoader.IO;
using System.IO;

namespace BeaniesBosses
{
    internal class DownedBossSystem : ModSystem
    {
        public static bool downedPogark = false;

        public override void OnWorldLoad()
        {
            downedPogark = false;
        }
        public override void OnWorldUnload()
        {
            downedPogark = false;
        }
        public override void SaveWorldData(TagCompound tag)
        {
            if (downedPogark)
            {
                tag["downedPogark"] = true;
            }
        }
        public override void LoadWorldData(TagCompound tag)
        {
            downedPogark = tag.ContainsKey("downedPogark");
        }
        public override void NetSend(BinaryWriter writer)
        {
            var flags = new BitsByte();
            flags[0] = downedPogark;
            writer.Write(flags);
        }
        public override void NetReceive(BinaryReader reader)
        {
            BitsByte flags = reader.ReadByte();
            downedPogark = flags[0];
        }
    }
}
