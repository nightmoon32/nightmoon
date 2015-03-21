using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using LeagueSharp;

namespace Oracle
{
    internal class GameBuff
    {
        public string ChampionName { get; set; }
        public string BuffName { get; set; }
        public SpellSlot Slot { get; set; }
        public string SpellName { get; set; }
        public int Delay { get; set; }

        public static readonly List<GameBuff> EvadeBuffs = new List<GameBuff>();
        public static readonly List<GameBuff> CleanseBuffs = new List<GameBuff>();


        static GameBuff()
        {
            CleanseBuffs.Add(new GameBuff
            {
                ChampionName = "Braum",
                BuffName = "braummark",
                SpellName = "braumq",
                Slot = SpellSlot.Q,
                Delay = 0
            });

            CleanseBuffs.Add(new GameBuff
            {
                ChampionName = "Zed",
                BuffName = "zedulttargetmark",
                SpellName = "zedult",
                Slot = SpellSlot.R,
                Delay = 1800
            });

            CleanseBuffs.Add(new GameBuff
            {
                ChampionName = "Fizz",
                BuffName = "fizzmarinerdoombomb",
                SpellName = "fizzmarinerdoom",
                Slot = SpellSlot.R,
                Delay = 0
            });

            CleanseBuffs.Add(new GameBuff
            {
                ChampionName = "Leblanc",
                BuffName = "leblancsoulshackle",
                SpellName = "leblancsoulshackle",
                Slot = SpellSlot.E,
                Delay = 500
            });

            CleanseBuffs.Add(new GameBuff
            {
                ChampionName = "LeeSin",
                BuffName = "blindmonkqonechaos",
                SpellName = "blindmonkqone",
                Slot = SpellSlot.Q,
                Delay = 0
            });

            CleanseBuffs.Add(new GameBuff
            {
                ChampionName = "Leblanc",
                BuffName = "leblancsoulshacklem",
                SpellName = "leblancsoulshacklem",
                Slot = SpellSlot.R,
                Delay = 500
            });

            CleanseBuffs.Add(new GameBuff
            {
                ChampionName = "Nasus",
                BuffName = "NasusW",
                SpellName = "NasusW",
                Slot = SpellSlot.W,
                Delay = 0
            });

            CleanseBuffs.Add(new GameBuff
            {
                ChampionName = "Mordekaiser",
                BuffName = "mordekaiserchildrenofthegrave",
                SpellName = "mordekaiserchildrenofthegrave",
                Slot = SpellSlot.R,
                Delay = 0
            });

            CleanseBuffs.Add(new GameBuff
            {
                ChampionName = "Poppy",
                BuffName = "poppydiplomaticimmunity",
                SpellName = "poppydiplomaticimmunity",
                Slot = SpellSlot.R,
                Delay = 0
            });

            CleanseBuffs.Add(new GameBuff
            {
                ChampionName = "Skarner",
                BuffName = "skarnerimpale",
                SpellName = "skarnerimpale",
                Slot = SpellSlot.R,
                Delay = 0
            });

            CleanseBuffs.Add(new GameBuff
            {
                ChampionName = "Urgot",
                BuffName = "urgotswap2",
                SpellName = "urgotswap2",
                Slot = SpellSlot.R,
                Delay = 0
            });

            CleanseBuffs.Add(new GameBuff
            {
                ChampionName = "Vladimir",
                BuffName = "vladimirhemoplague",
                SpellName = "vladimirhemoplague",
                Slot = SpellSlot.R,
                Delay = 2000
            });

            CleanseBuffs.Add(new GameBuff
            {
                ChampionName = "Morgana",
                BuffName = "soulshackles",
                SpellName = "soulshackles",
                Slot = SpellSlot.R,
                Delay = 1000
            });

            EvadeBuffs.Add(new GameBuff
            {
                ChampionName = "Karthus",
                BuffName = "fallenonetarget",
                SpellName = "fallenone",
                Slot = SpellSlot.R,
                Delay = 2500
            });

            EvadeBuffs.Add(new GameBuff
            {
                ChampionName = "Morgana",
                BuffName = "soulshackles",
                SpellName = "soulshackles",
                Slot = SpellSlot.R,
                Delay = 2500
            });

            EvadeBuffs.Add(new GameBuff
            {
                ChampionName = "Vladimir",
                BuffName = "vladimirhemoplague",
                SpellName = "vladimirhemoplague",
                Slot = SpellSlot.R,
                Delay = 4500
            });

            EvadeBuffs.Add(new GameBuff
            {
                ChampionName = "Zed",
                BuffName = "zedulttargetmark",
                SpellName = "zedult",
                Slot = SpellSlot.R,
                Delay = 2800
            });

            EvadeBuffs.Add(new GameBuff
            {
                ChampionName = "Caitlyn",
                BuffName = "caitlynaceinthehole",
                SpellName = "caitlynaceinthehole",
                Slot = SpellSlot.R,
                Delay = 1000
            });
        }
    }
}
