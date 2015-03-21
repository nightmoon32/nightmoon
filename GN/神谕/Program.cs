using System;
using System.IO;
using System.Linq;
using System.Net;
using LeagueSharp;
using LeagueSharp.Common;
using Oracle.Extensions;
using Oracle.Core.Skillshots;
using Oracle.Core.Targeted;
using SpellSlot = LeagueSharp.SpellSlot;

namespace Oracle
{
    //  _____             _     
    // |     |___ ___ ___| |___ 
    // |  |  |  _| .'|  _| | -_|
    // |_____|_| |__,|___|_|___|
    // Copyright © Kurisu Solutions 2015
    internal static class Program
    {
        internal enum LogType
        {
            Error = 0,
            Danger = 1,
            Info = 2,
            Damage = 3,
            Action = 4
        };

        public static Menu Origin;
        public static Obj_AI_Hero Attacker;
        public static Obj_AI_Hero AggroTarget;
        public static float IncomeDamage, MinionDamage;
        private static readonly Obj_AI_Hero Me = ObjectManager.Player;

        private static void Main(string[] args)
        {
            Console.WriteLine("Oracle is loading...");
            CustomEvents.Game.OnGameLoad += OnGameLoad;
        }

        public static bool Spell;
        public static bool Stealth;
        public static bool Danger;
        public static bool Dangercc;
        public static bool DangerUlt;
        public static string FileName;
        public static bool CanManamune;
        public static string ChampionName;
        public const string Revision = "226";

        private static void OnGameLoad(EventArgs args)
        {
            FileName = "Oracle - " + DateTime.Now.ToString("yy.MM.dd") + " " + DateTime.Now.ToString("h.mm.ss") + ".txt";
      
            ChampionName = Me.ChampionName;
            Game.OnUpdate += Game_OnGameUpdate;
            Game.PrintChat("<font color=\"#1FFF8F\">Oracle# r." + Revision + " -</font><font color=\"#FFFFCC\"> by Kurisu</font>");

            if (!Directory.Exists(Config.LeagueSharpDirectory + @"\Logs\Oracle"))
            {
                Directory.CreateDirectory(Config.LeagueSharpDirectory + @"\Logs\Oracle");
                Game.PrintChat(
                    "<font color=\"#FFFFCC\"><b>Thank you for choosing Oracle! :^)</b></font>");
                Game.PrintChat(
                    "<font color=\"#FFFFCC\"><b>Log files are generated in </b></font>" + Config.LeagueSharpDirectory + @"\Logs\Oracle\");
            }

            else
            {
                Game.PrintChat("<font color=\"#FFFFCC\">Do appreciate donations via Paypal </font>: xrobinsong@gmail.com");
            }

            try
            {
                var wc = new WebClient { Proxy = null };
                var gitrevision =
                    wc.DownloadString(
                        "https://raw.githubusercontent.com/xKurisu/KurisuSolutions/master/Oracle%20(Activator)/Oracle.txt");

                if (Revision != gitrevision)
                {
                    Game.PrintChat("<font color=\"#FFFFCC\"><b>Oracle is outdated, please Update!</b></font>");
                }
            }

            catch (Exception e)
            {
                Logger(LogType.Error, string.Format("Something went wrong with update checker! {0}", e.Message));
            }

            Origin = new Menu("花边-神谕", "oracle", true);
            净化装备.Initialize(Origin);
            防守装备.Initialize(Origin);
            召唤师技能.Initialize(Origin);
            进攻装备.Initialize(Origin);
            药瓶.Initialize(Origin);
            自动法术.Initialize(Origin);

            var config = new Menu("净化设置", "oracleconfig");
            var dangerMenu = new Menu("危险技能设置", "dangerconfig");

            foreach (var i in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.Team != Me.Team))
            {
                var menu = new Menu(i.ChampionName, i.ChampionName + "cccmenu");

                foreach (
                    var spell in
                        TargetSpellDatabase.Spells.Where(spell => spell.ChampionName == i.ChampionName.ToLower()))
                {
                    var danger = spell.Spellslot.ToString() == "R" ||
                                    spell.CcType != CcType.No && (spell.Type == SpellType.Skillshot || spell.Type == SpellType.Targeted);

                    menu.AddItem(new MenuItem(spell.Name + "ccc", spell.Name + " - " + spell.Spellslot)).SetValue(danger);
                }

                dangerMenu.AddSubMenu(menu);
            }

            config.AddItem(
                new MenuItem("usecombo", "连招 (启用)")
                    .SetValue(new KeyBind(32, KeyBindType.Press)));

            config.AddSubMenu(dangerMenu);

            var cskills = new Menu("净化", "cskills");
            foreach (var i in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.Team != Me.Team))
            {
                foreach (var debuff in GameBuff.CleanseBuffs.Where(t => t.ChampionName == i.ChampionName))
                    cskills.AddItem(new MenuItem("cure" + debuff.BuffName, debuff.ChampionName + " - " + debuff.Slot))
                        .SetValue(true);
            }

            config.AddSubMenu(cskills);

            var cleanseMenu = new Menu("净化debuff", "cdebufs");
            cleanseMenu.AddItem(new MenuItem("stun", "眩晕")).SetValue(true);
            cleanseMenu.AddItem(new MenuItem("charm", "魅惑")).SetValue(true);
            cleanseMenu.AddItem(new MenuItem("taunt", "嘲讽")).SetValue(true);
            cleanseMenu.AddItem(new MenuItem("fear", "恐惧")).SetValue(true);
            cleanseMenu.AddItem(new MenuItem("snare", "陷阱")).SetValue(true);
            cleanseMenu.AddItem(new MenuItem("silence", "沉默")).SetValue(true);
            cleanseMenu.AddItem(new MenuItem("suppression", "禁锢")).SetValue(true);
            cleanseMenu.AddItem(new MenuItem("polymorph", "变形")).SetValue(true);
            cleanseMenu.AddItem(new MenuItem("blind", "致盲")).SetValue(false);
            cleanseMenu.AddItem(new MenuItem("slow", "减速")).SetValue(false);
            cleanseMenu.AddItem(new MenuItem("poison", "中毒")).SetValue(false);
            config.AddSubMenu(cleanseMenu);

            var debugMenu = new Menu("调试", "debugmenu");
            debugMenu.AddItem(new MenuItem("dbool", "控制台调试")).SetValue(false);
            debugMenu.AddItem(new MenuItem("catchobject", "日志对象"))
                .SetValue(new KeyBind(89, KeyBindType.Press));
            config.AddSubMenu(debugMenu);

            Origin.AddSubMenu(config);
            Origin.AddToMainMenu();

            // Events
            GameObject.OnCreate += GameObject_OnCreate;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;

            Logger(LogType.Info, "Oracle Revision: " + Revision);
            Logger(LogType.Info, "Local Player: " + ChampionName);
            Logger(LogType.Info, "Local Version: " + Game.Version);
            Logger(LogType.Info, "Local Game Map: " + Game.MapId);
            Logger(LogType.Info, "Local Summoners: " + Me.Spellbook.GetSpell(SpellSlot.Summoner1).Name + " - " +
                                                       Me.Spellbook.GetSpell(SpellSlot.Summoner2).Name);

            foreach (var i in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (i.Team == Me.Team)
                {
                    Logger(LogType.Info, "Ally added: " + i.ChampionName);
                }

                if (i.Team != Me.Team)
                {
                    Logger(LogType.Info, "Enemy added: " + i.ChampionName);
                }
            }
        }

        public static void Logger(LogType type, string msg)
        {
            var prefix = "[" + DateTime.Now.ToString("T") + " " + type + "]: ";
            using (var file = new StreamWriter(Config.LeagueSharpDirectory + @"\Logs\Oracle\" + FileName, true))
            {
                file.WriteLine(prefix + msg);
                file.Close();
            }

            if (Origin.Item("dbool").GetValue<bool>())
                Console.WriteLine("Oracle: (" + type + ") " + msg);
        }


        private static GameObj _satchel, _miasma, _minefield, _crowstorm, _fizzbait, _caittrap;
        private static GameObj _chaosstorm, _glacialstorm, _lightstrike, _equinox, _tormentsoil;
        private static GameObj _depthcharge, _tremors, _acidtrail, _catalyst;

        private static void GameObject_OnCreate(GameObject obj, EventArgs args)
        {
            if (Origin.Item("catchobject").GetValue<KeyBind>().Active)
                Console.WriteLine(obj.Name);

            // red troy is always the enemy team no matter what side.
            if (obj.Name.Contains("Fizz_Ring_Red") && GetEnemy("Fizz").IsValid)
            {
                var dmg = (float)GetEnemy("Fizz").GetSpellDamage(Friendly(), SpellSlot.R);
                _fizzbait = new GameObj(obj.Name, obj, true, dmg, Environment.TickCount);
                Logger(LogType.Info, obj.Name + " detected/created (Fizz)");
            }

            else if (obj.Name.Contains("Acidtrail_buf_red") && GetEnemy("Singed").IsValid)
            {
                var dmg = (float)GetEnemy("Singed").GetSpellDamage(Friendly(), SpellSlot.Q);
                _acidtrail = new GameObj(obj.Name, obj, true, dmg, Environment.TickCount);
                Logger(LogType.Info, obj.Name + " detected/created (Poison)");
            }

            else if (obj.Name.Contains("Tremors_cas") && obj.IsEnemy && GetEnemy("Rammus").IsValid)
            {
                var dmg = (float) GetEnemy("Rammus").GetSpellDamage(Friendly(), SpellSlot.R);
                _tremors = new GameObj(obj.Name, obj, true, dmg, Environment.TickCount);
                Logger(LogType.Info, obj.Name + " detected/created (Tremors)");
            }

            else if (obj.Name.Contains("Crowstorm_red") && GetEnemy("Fiddlesticks").IsValid)
            {
                var dmg = (float) GetEnemy("Fiddlesticks").GetSpellDamage(Friendly(), SpellSlot.R);
                _crowstorm = new GameObj(obj.Name, obj, true, dmg, Environment.TickCount);
                Logger(LogType.Info, obj.Name + " detected/created (Crowstorm)");
            }
                
            else if (obj.Name.Contains("Nautilus_R_sequence_impact") && obj.IsEnemy && GetEnemy("Nautilus").IsValid)
            {              
                var dmg = (float) GetEnemy("Nautilus").GetSpellDamage(Friendly(), SpellSlot.R, 1);
                _depthcharge = new GameObj(obj.Name, obj, true, dmg, Environment.TickCount);
                Logger(LogType.Info, obj.Name + " detected/created (Depth Charge)");
            }

            else if (obj.Name.Contains("caitlyn_Base_yordleTrap_idle_red") && GetEnemy("Caitlyn").IsValid)
            {
                var dmg = (float) GetEnemy("Caitlyn").GetSpellDamage(Friendly(), SpellSlot.W);
                _caittrap = new GameObj(obj.Name, obj, true, dmg, Environment.TickCount);
                Logger(LogType.Info, obj.Name + " detected/created (Yordle Trap)");
            }

            else if (obj.Name.Contains("LuxLightstrike_tar_red") && GetEnemy("Lux").IsValid)
            {
                var dmg = (float) GetEnemy("Lux").GetSpellDamage(Friendly(), SpellSlot.E);
                _lightstrike = new GameObj(obj.Name, obj, true, dmg, Environment.TickCount);
                Logger(LogType.Info, obj.Name + " detected/created (Lightstrike)");
            }

            else if (obj.Name.Contains("Viktor_ChaosStorm_red") && GetEnemy("Viktor").IsValid)
            {
                var dmg = (float) GetEnemy("Viktor").GetSpellDamage(Friendly(), SpellSlot.R);
                _chaosstorm = new GameObj(obj.Name, obj, true, dmg, Environment.TickCount);
                Logger(LogType.Info, obj.Name + " detected/created (Chaos Storm)");
            }

            else if (obj.Name.Contains("Viktor_Catalyst_red") && GetEnemy("Viktor").IsValid)
            {
                _catalyst = new GameObj(obj.Name, obj, true, 0, Environment.TickCount);
                Logger(LogType.Info, obj.Name + " detected/created (Gravity Field)");
            }

            else if (obj.Name.Contains("cryo_storm_red") && GetEnemy("Anivia").IsValid)
            {
                var dmg = (float) GetEnemy("Anivia").GetSpellDamage(Friendly(), SpellSlot.R);
                _glacialstorm = new GameObj(obj.Name, obj, true, dmg, Environment.TickCount);
                Logger(LogType.Info, obj.Name + " detected/created (Glacialstorm)");
            }

            else if (obj.Name.Contains("ZiggsE_red") && GetEnemy("Ziggs").IsValid)
            {
                var dmg = (float) GetEnemy("Ziggs").GetSpellDamage(Friendly(), SpellSlot.E);
                _minefield = new GameObj(obj.Name, obj, true, dmg, Environment.TickCount);
                Logger(LogType.Info, obj.Name + " detected/created (Minefield)");
            }

            else if (obj.Name.Contains("ZiggsWRingRed") && GetEnemy("Ziggs").IsValid)
            {
                var dmg = (float) GetEnemy("Ziggs").GetSpellDamage(Friendly(), SpellSlot.W);
                _satchel = new GameObj(obj.Name, obj, true, dmg, Environment.TickCount);
                Logger(LogType.Info, obj.Name + " detected/created (Satchel)");
            }

            else if (obj.Name.Contains("CassMiasma_tar_red") && GetEnemy("Cassiopeia").IsValid)
            {
                var dmg = (float) GetEnemy("Cassiopeia").GetSpellDamage(Friendly(), SpellSlot.W);
                _miasma = new GameObj(obj.Name, obj, true, dmg, Environment.TickCount);
                Logger(LogType.Info, obj.Name + " detected/created (Miasma)");
            }

            else if (obj.Name.Contains("Soraka_Base_E_rune_RED") && GetEnemy("Soraka").IsValid)
            {
                var dmg = (float) GetEnemy("Soraka").GetSpellDamage(Friendly(), SpellSlot.E);
                _equinox = new GameObj(obj.Name, obj, true, dmg, Environment.TickCount);
                Logger(LogType.Info, obj.Name + " detected/created (Equinox)");
            }

            else if (obj.Name.Contains("Morgana_Base_W_Tar_red") && GetEnemy("Morgana").IsValid)
            {
                var dmg = (float) GetEnemy("Morgana").GetSpellDamage(Friendly(), SpellSlot.W);
                _tormentsoil = new GameObj(obj.Name, obj, true, dmg, Environment.TickCount);
                Logger(LogType.Info, obj.Name + " detected/created (Tormentsoil)");
            }
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            // prevent errors before spawning to the rift
            if (!Me.IsValidTarget(300, false))
            {
                return;
            }

            // Get dangerous buff update for zhonya (vladimir R) etc
            foreach (var buff in GameBuff.EvadeBuffs)
            {
                foreach (var aura in Friendly().Buffs)
                {
                    if (!aura.Name.ToLower().Contains(buff.SpellName) && aura.Name.ToLower() != buff.BuffName) 
                        continue;

                    Utility.DelayAction.Add(
                        buff.Delay, delegate
                        {
                            Attacker = GetEnemy(buff.ChampionName);
                            AggroTarget = Friendly();
                            IncomeDamage =
                                (float) GetEnemy(buff.ChampionName).GetSpellDamage(AggroTarget, buff.Slot);

                            // check if we still have buff and didn't walk out of it
                            if (aura.Name.ToLower().Contains(buff.SpellName) || aura.Name.ToLower() == buff.BuffName)
                            {
                                DangerUlt = Origin.Item(buff.SpellName + "ccc").GetValue<bool>();
                            }

                            Logger(LogType.Danger,
                                "(" + Attacker.SkinName + ") Dangerous buff on " + AggroTarget.SkinName + " should zhonyas!");
                        });
                }
            }
            
            // Get ground game object damage update
            if (_tremors.Included)
            {
                if (_tremors.Obj.IsValid && Friendly().Distance(_tremors.Obj.Position, true) <= 400 * 400)
                {
                    if (GetEnemy("Rammus").IsValid)
                    {
                        Attacker = GetEnemy("Rammus");
                        AggroTarget = Friendly();
                        IncomeDamage = _tremors.Damage;
                        Logger(LogType.Damage,
                            AggroTarget.SkinName + " is in Tremors (Ground Object) for: " + IncomeDamage);
                    }
                }
            }

            if (_acidtrail.Included)
            {
                if (_acidtrail.Obj.IsValid && Friendly().Distance(_acidtrail.Obj.Position, true) <= 150 * 150)
                {
                    if (GetEnemy("Singed").IsValid)
                    {
                        Attacker = GetEnemy("Singed");
                        AggroTarget = Friendly();
                        IncomeDamage = _acidtrail.Damage;

                        Logger(LogType.Damage,
                            AggroTarget.SkinName + " is in Poison Trail (Game Object) for: " + IncomeDamage);
                    }
                }
            }

            if (_catalyst.Included)
            {
                if (_catalyst.Obj.IsValid && Friendly().Distance(_catalyst.Obj.Position, true) <= 400*400)
                {
                    if (GetEnemy("Viktor").IsValid)
                    {
                        Attacker = GetEnemy("Viktor");
                        AggroTarget = Friendly();
                        Dangercc = true;

                        Logger(LogType.Danger,
                            AggroTarget.SkinName + " is in Gravity Field (Ground Object) for: 0");
                    }
                }
            }

            if (_glacialstorm.Included)
            {
                if (_glacialstorm.Obj.IsValid && Friendly().Distance(_glacialstorm.Obj.Position, true) <= 400 * 400)
                {
                    if (GetEnemy("Anivia").IsValid)
                    {
                        Attacker = GetEnemy("Anivia");
                        AggroTarget = Friendly();
                        IncomeDamage = _glacialstorm.Damage;
                        Dangercc = true;

                        Logger(LogType.Danger,
                            AggroTarget.SkinName + " is in Glacialstorm (Ground Object) for: " + IncomeDamage);
                    }
                }
            }

            if (_chaosstorm.Included)
            {
                if (_chaosstorm.Obj.IsValid && Friendly().Distance(_chaosstorm.Obj.Position, true) <= 400 * 400) 
                {
                    if (GetEnemy("Viktor").IsValid)
                    {
                        Attacker = GetEnemy("Viktor");
                        AggroTarget = Friendly();
                        IncomeDamage = _chaosstorm.Damage; 
                        Dangercc = true;

                        if (AggroTarget.NetworkId == Friendly().NetworkId &&
                            Origin.Item("viktorchaosstormccc").GetValue<bool>())
                        {
                            if (Friendly().CountHerosInRange(false) + 1 >= Friendly().CountHerosInRange(true) ||
                                IncomeDamage >= Friendly().Health)
                            {
                                Danger = true;
                                DangerUlt = true;
                            }
                        }

                        Logger(LogType.Danger,
                            AggroTarget.SkinName + " is in Chaostorm (Ground Object) for: " + IncomeDamage);
                    }
                }
            }

            if (_fizzbait.Included)
            {
                if (_fizzbait.Obj.IsValid && Friendly().Distance(_fizzbait.Obj.Position, true) <= 300*300)
                {
                    if (GetEnemy("Fizz").IsValid)
                    {
                        Attacker = GetEnemy("Fizz");
                        AggroTarget = Friendly();
                        IncomeDamage = _fizzbait.Damage;

                        if (Friendly().CountHerosInRange(false) + 1 >= Friendly().CountHerosInRange(true) ||
                            IncomeDamage >= Friendly().Health)
                        {
                            if (Environment.TickCount - _fizzbait.Start >= 900)
                            {
                                Danger = true;
                                DangerUlt = true;
                                Dangercc = true;
                            }
                        }
                        
                        Logger(LogType.Danger,
                            AggroTarget.SkinName + " is in fizz bait (Ground Object) for: " + IncomeDamage);
                    }
                }
            }

            if (_depthcharge.Included)
            {
                if (_depthcharge.Obj.IsValid && Friendly().Distance(_depthcharge.Obj.Position, true) <= 300 * 300)
                {
                    if (GetEnemy("Nautilus").IsValid)
                    {
                        Attacker = GetEnemy("Nautilus");
                        AggroTarget = Friendly();
                        IncomeDamage = _depthcharge.Damage;

                        if (Friendly().CountHerosInRange(false) + 1 >= Friendly().CountHerosInRange(true) ||
                            IncomeDamage >= Friendly().Health)
                        {
                            if (Friendly().HasBuff("nautilusgrandlinetarget", true))
                            {
                                Danger = true;
                                Dangercc = true;
                                DangerUlt = true;
                            }

                            else
                            {
                                Dangercc = true;
                            }
                        }

                        Logger(LogType.Danger,
                            "Nautilus depth charge is homing " + AggroTarget.SkinName + " for: " + IncomeDamage);
                    }

                }
            }

            if (_caittrap.Included)
            {
                if (_caittrap.Obj.IsValid && Friendly().Distance(_caittrap.Obj.Position, true) <= 150 * 150)
                {
                    if (GetEnemy("Caitlyn").IsValid)
                    {
                        Attacker = GetEnemy("Caitlyn");
                        AggroTarget = Friendly();
                        IncomeDamage = _caittrap.Damage;
                        Dangercc = true;

                        Logger(LogType.Danger,
                            AggroTarget.SkinName + " is in yordle trap (Ground Object) for: " + IncomeDamage);
                    }
                }
            }

            if (_crowstorm.Included)
            {
                // 575 Fear Range
                if (_crowstorm.Obj.IsValid && Friendly().Distance(_crowstorm.Obj.Position, true) <= 575 * 575)
                {
                    if (GetEnemy("Fiddlesticks").IsValid)
                    {
                        Attacker = GetEnemy("Fiddlesticks");
                        AggroTarget = Friendly();
                        IncomeDamage = _chaosstorm.Damage;

                        if (AggroTarget.NetworkId == Friendly().NetworkId &&
                            Origin.Item("crowstormccc").GetValue<bool>())
                        {
                            if (Friendly().CountHerosInRange(false) + 1 >= Friendly().CountHerosInRange(true) ||
                                IncomeDamage >= Friendly().Health)
                            {
                                Danger = true;
                                DangerUlt = true;
                            }
                        }

                        Logger(LogType.Danger,
                            AggroTarget.SkinName + " is in Crowstorm (Ground Object) for: " + IncomeDamage);
                    }
                }
            }

            if (_minefield.Included)
            {
                if (_minefield.Obj.IsValid && Friendly().Distance(_minefield.Obj.Position, true) <= 300 * 300)
                {
                    if (GetEnemy("Ziggs").IsValid)
                    {
                        Attacker = GetEnemy("Ziggs");
                        AggroTarget = Friendly();
                        IncomeDamage = _minefield.Damage;
                        Dangercc = true;

                        Logger(LogType.Danger,
                            AggroTarget.SkinName + " is in Minefield (Ground Object) for: " + IncomeDamage);
                    }
                }
            }

            if (_satchel.Included)
            {
                if (_satchel.Obj.IsValid && Friendly().Distance(_satchel.Obj.Position, true) <= 300 * 300)
                {
                    if (GetEnemy("Ziggs").IsValid)
                    {
                        Attacker = GetEnemy("Ziggs");
                        AggroTarget = Friendly();
                        IncomeDamage = _satchel.Damage;
                        Dangercc = true;

                        Logger(LogType.Danger,
                            AggroTarget.SkinName + " is in Satchel (Ground Object) for: " + IncomeDamage);
                    }
                }
            }

            if (_tormentsoil.Included)
            {
                if (_tormentsoil.Obj.IsValid && Friendly().Distance(_tormentsoil.Obj.Position, true) <= 300 * 300)
                {
                    if (GetEnemy("Morgana").IsValid)
                    {
                        Attacker = GetEnemy("Morgana");
                        AggroTarget = Friendly();
                        IncomeDamage = _tormentsoil.Damage;

                        Logger(LogType.Damage,
                            AggroTarget.SkinName + " is in Torment Soil (Ground Object) for: " + IncomeDamage);
                    }
                }
            }

            if (_miasma.Included)
            {
                if (_miasma.Obj.IsValid && Friendly().Distance(_miasma.Obj.Position, true) <= 300 * 300)
                {
                    if (GetEnemy("Cassiopeia").IsValid)
                    {
                        Attacker = GetEnemy("Cassiopeia");
                        AggroTarget = Friendly();
                        IncomeDamage = _satchel.Damage;
                        Dangercc = true;

                        Logger(LogType.Danger,
                            AggroTarget.SkinName + " is in Miasma (Ground Object) for: " + IncomeDamage);
                    }
                }
            }

            if (_lightstrike.Included)
            {
                if (_lightstrike.Obj.IsValid && Friendly().Distance(_lightstrike.Obj.Position, true) <= 300*300)
                {
                    if (GetEnemy("Lux").IsValid)
                    {
                        Attacker = GetEnemy("Lux");
                        AggroTarget = Friendly();
                        IncomeDamage = _lightstrike.Damage;
                        Dangercc = true;

                        Logger(LogType.Danger,
                            AggroTarget.SkinName + " is in Lightstrike (Ground Object) for: " + IncomeDamage);
                    }
                }
            }

            if (_equinox.Included)
            {
                if (_equinox.Obj.IsValid && Friendly().Distance(_equinox.Obj.Position, true) <= 300 * 300)
                {
                    if (GetEnemy("Soraka").IsValid)
                    {
                        Attacker = GetEnemy("Soraka");
                        AggroTarget = Friendly();
                        IncomeDamage = _equinox.Damage;
                        Dangercc = true;

                        Logger(LogType.Danger,
                            AggroTarget.SkinName + " is in Equinox (Ground Object) for: " + IncomeDamage);
                    }
                }
            }

            // reset income damage/danger safely
            if (IncomeDamage >= 1)
                Utility.DelayAction.Add(Game.Ping + 50, () => IncomeDamage = 0);
            if (MinionDamage >= 1)
                Utility.DelayAction.Add(Game.Ping + 50, () => MinionDamage = 0);

            if (Danger)
                Utility.DelayAction.Add(Game.Ping + 130, () => Danger = false);
            if (Dangercc)
                Utility.DelayAction.Add(Game.Ping + 130, () => Dangercc = false);
            if (DangerUlt)
                Utility.DelayAction.Add(Game.Ping + 130, () => DangerUlt = false);
            if (Spell)
                Utility.DelayAction.Add(Game.Ping + 130, () => Spell = false);
        }

        public static Obj_AI_Hero Friendly()
        {
            Obj_AI_Hero target = null;

            foreach (
                var unit in
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(x => x.IsAlly && x.IsValidTarget(900, false))
                        .OrderByDescending(xe => xe.Health / xe.MaxHealth * 100))
            {
                target = unit;
            }

            return target;
        }

        public static Obj_AI_Hero GetEnemy(string championname)
        {
            return
                ObjectManager.Get<Obj_AI_Hero>()
                    .First(enemy => enemy.Team != Me.Team && enemy.ChampionName == championname);
        }     

        public static bool IsValidState(this Obj_AI_Hero target)
        {
            return !target.HasBuffOfType(BuffType.SpellShield) && !target.HasBuffOfType(BuffType.SpellImmunity) &&
                   !target.HasBuffOfType(BuffType.Invulnerability);
        }

        public static int CountHerosInRange(this Obj_AI_Hero target, bool checkteam, float range = 1200f)
        {
            var objListTeam =
                ObjectManager.Get<Obj_AI_Hero>()
                    .Where(
                        x => x.IsValidTarget(range, false));

            return objListTeam.Count(hero => checkteam ? hero.Team != target.Team : hero.Team == target.Team);
        }

        public static float GetComboDamage(Obj_AI_Hero player, Obj_AI_Base target)
        {
            var ignite = player.GetSpellSlot("summonerdot");

            // todo: get damage for different spell states
            var qready = player.Spellbook.CanUseSpell(SpellSlot.Q) == SpellState.Ready;
            var wready = player.Spellbook.CanUseSpell(SpellSlot.W) == SpellState.Ready;
            var eready = player.Spellbook.CanUseSpell(SpellSlot.E) == SpellState.Ready;
            var rready = player.Spellbook.CanUseSpell(SpellSlot.R) == SpellState.Ready;
            var iready = player.Spellbook.CanUseSpell(ignite) == SpellState.Ready;

            var tmt = Items.HasItem(3077) && Items.CanUseItem(3077)
                ? Me.GetItemDamage(target, Damage.DamageItems.Tiamat)
                : 0;

            var hyd = Items.HasItem(3074) && Items.CanUseItem(3074)
                ? Me.GetItemDamage(target, Damage.DamageItems.Hydra)
                : 0;

            var bwc = Items.HasItem(3144) && Items.CanUseItem(3144)
                ? Me.GetItemDamage(target, Damage.DamageItems.Bilgewater)
                : 0;

            var brk = Items.HasItem(3153) && Items.CanUseItem(3153)
                ? Me.GetItemDamage(target, Damage.DamageItems.Botrk)
                : 0;

            var dfg = Items.HasItem(3128) && Items.CanUseItem(3128)
                ? Me.GetItemDamage(target, Damage.DamageItems.Dfg)
                : 0;

            var guise = Items.HasItem(3151)
                ? Me.GetItemDamage(target, Damage.DamageItems.LiandrysTorment)
                : 0;

            var torch = Items.HasItem(3188) && Items.CanUseItem(3188)
                ? Me.GetItemDamage(target, Damage.DamageItems.Dfg)
                : 0;

            var items = tmt + hyd + bwc + brk + dfg + torch + guise;
            var aa = player.GetAutoAttackDamage(target);

            var qq = qready ? player.GetSpellDamage(target, SpellSlot.Q) : 0;
            var ww = wready ? player.GetSpellDamage(target, SpellSlot.W) : 0;
            var ee = eready ? player.GetSpellDamage(target, SpellSlot.E) : 0;
            var rr = rready ? player.GetSpellDamage(target, SpellSlot.R) : 0;

            var ii = iready 
                ? player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite) 
                : 0;

            var damage = aa + qq + ww + ee + rr + ii + items;

            return (float) damage;
        }

        private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe)
            {
                foreach (var o in SkillshotDatabase.Spells.Where(x => x.SpellName == args.SData.Name))
                {
                    foreach (var i in Damage.Spells
                        .Where(d => d.Key == o.ChampionName)
                        .SelectMany(item => item.Value).Where(i => i.Slot == (SpellSlot)o.Slot))
                    {
                        if (i.DamageType == Damage.DamageType.Physical)
                            CanManamune = true;
                    }
                }

                foreach (var o in TargetSpellDatabase.Spells.Where(x => x.Name == args.SData.Name.ToLower()))
                {
                    foreach (var i in Damage.Spells.Where(d => d.Key == o.ChampionName)
                        .SelectMany(item => item.Value).Where(i => i.Slot == (SpellSlot)o.Spellslot))
                    {
                        if (i.DamageType == Damage.DamageType.Physical)
                            CanManamune = true;
                    }
                }

                if (Me.GetSpellSlot(args.SData.Name) == SpellSlot.Unknown &&
                   (Origin.Item("usecombo").GetValue<KeyBind>().Active || args.Target.Type == Me.Type))
                {
                    CanManamune = true;
                }

                else
                {
                    Utility.DelayAction.Add(400, () => CanManamune = false);
                }
            }

            Attacker = null;
            if (sender.Type == GameObjectType.obj_AI_Hero && sender.IsEnemy)
            {
                var heroSender = ObjectManager.Get<Obj_AI_Hero>().First(x => x.NetworkId == sender.NetworkId);
                if (heroSender.GetSpellSlot(args.SData.Name) == SpellSlot.Unknown && args.Target.Type == Me.Type)
                {
                    Danger = false;
                    Dangercc = false;
                    DangerUlt = false;
                    AggroTarget = ObjectManager.GetUnitByNetworkId<Obj_AI_Hero>(args.Target.NetworkId);

                    IncomeDamage = (float) heroSender.GetAutoAttackDamage(AggroTarget);
                    Logger(LogType.Damage,
                        heroSender.SkinName + " hit (AA) " + AggroTarget.SkinName + " for: " + IncomeDamage);
                }

                if (heroSender.ChampionName == "Jinx" && args.SData.Name.Contains("JinxQAttack") &&
                    args.Target.Type == Me.Type)
                {
                    Danger = false;
                    Dangercc = false;
                    DangerUlt = false;
                    AggroTarget = ObjectManager.GetUnitByNetworkId<Obj_AI_Hero>(args.Target.NetworkId);

                    IncomeDamage = (float) heroSender.GetAutoAttackDamage(AggroTarget);
                    Logger(LogType.Damage,
                        heroSender.SkinName + " hit (JinxQ) " + AggroTarget.SkinName + " for: " + IncomeDamage);
                }

                Attacker = heroSender;
                foreach (var o in TargetSpellDatabase.Spells.Where(x => x.Name == args.SData.Name.ToLower()))
                {

                    Stealth = o.Stealth;
                    if (o.Type == SpellType.Skillshot)
                    {
                        continue;
                    }

                    if (o.Type == SpellType.Self)
                    {
                        Utility.DelayAction.Add((int) (o.Delay), delegate
                        {
                            var vulnerableTarget =
                                ObjectManager.Get<Obj_AI_Hero>().OrderBy(x => x.Distance(heroSender.ServerPosition))
                                    .FirstOrDefault(x => x.IsAlly);

                            if (vulnerableTarget != null && vulnerableTarget.Distance(heroSender.ServerPosition, true) <= o.Range*o.Range)
                            {
                                AggroTarget = vulnerableTarget;
                                IncomeDamage = (float) heroSender.GetSpellDamage(AggroTarget, (SpellSlot) o.Spellslot);

                                if (o.Wait)
                                {
                                    return;
                                }

                                Spell = true;
                                Danger = Origin.Item(o.Name.ToLower() + "ccc").GetValue<bool>();
                                DangerUlt = Origin.Item(o.Name.ToLower() + "ccc").GetValue<bool>() &&
                                            o.Spellslot.ToString() == "R";
                                Dangercc = o.CcType != CcType.No && o.Type != SpellType.AutoAttack &&
                                           Origin.Item(o.Name.ToLower() + "ccc").GetValue<bool>();

                                Logger(LogType.Damage, "Danger (Self: " + o.Spellslot + "): " + Danger);
                                Logger(LogType.Damage,
                                    heroSender.SkinName + " hit (Self: " + o.Spellslot + ") " + AggroTarget.SkinName +
                                    " for: " + IncomeDamage);
                            }
                        });
                    }

                    if (o.Type == SpellType.Targeted && args.Target.Type == Me.Type)
                    {
                        Utility.DelayAction.Add((int) (o.Delay), delegate
                        {
                            AggroTarget = ObjectManager.GetUnitByNetworkId<Obj_AI_Hero>(args.Target.NetworkId);
                            IncomeDamage = (float) heroSender.GetSpellDamage(AggroTarget, (SpellSlot) o.Spellslot);

                            Logger(LogType.Damage, "Dangerous (Targetd: " + o.Spellslot + "): " + Danger);
                            Logger(LogType.Damage,
                                heroSender.SkinName + " hit (Targeted: " + o.Spellslot + ") " + AggroTarget.SkinName +
                                " for: " + IncomeDamage);

                            if (o.Wait)
                            {
                                return;
                            }

                            Spell = true;
                            Danger = o.Dangerous && Origin.Item(o.Name.ToLower() + "ccc").GetValue<bool>();
                            DangerUlt = o.Dangerous && Origin.Item(o.Name.ToLower() + "ccc").GetValue<bool>() &&
                                        o.Spellslot.ToString() == "R";
                            Dangercc = o.CcType != CcType.No && o.Type != SpellType.AutoAttack;
                        });
                    }
                }

                foreach (
                    var o in
                        SkillshotDatabase.Spells.Where(
                            x => x.SpellName == args.SData.Name || x.ExtraSpellNames.Contains(args.SData.Name)))
                {
                    var skillData =
                        new SkillshotData(o.ChampionName, o.SpellName, o.Slot, o.Type, o.Delay, o.Range,
                            o.Radius, o.MissileSpeed, o.AddHitbox, o.FixedRange, o.DangerValue);

                    var endPosition = args.Start.To2D() +
                                      o.Range*(args.End.To2D() - heroSender.ServerPosition.To2D()).Normalized();

                    var skillShot = new Skillshot(DetectionType.ProcessSpell, skillData, Environment.TickCount,
                        heroSender.ServerPosition.To2D(), endPosition, heroSender);

                    var castTime = (o.DontAddExtraDuration ? 0 : o.ExtraDuration) + o.Delay +
                                   (int) (1000*heroSender.Distance(Friendly().ServerPosition)/o.MissileSpeed) -
                                   (Environment.TickCount - skillShot.StartTick);

                    var vulnerableTarget =
                        ObjectManager.Get<Obj_AI_Hero>()
                            .FirstOrDefault(x => !skillShot.IsSafe(x.ServerPosition.To2D()) && x.IsAlly);

                    if (vulnerableTarget != null && vulnerableTarget.Distance(heroSender.ServerPosition, true) <= o.Range*o.Range)
                    {
                        Utility.DelayAction.Add(castTime - 400, delegate
                        {
                            AggroTarget = vulnerableTarget;
                            IncomeDamage =
                                (float) heroSender.GetSpellDamage(AggroTarget, (SpellSlot) skillShot.SkillshotData.Slot);

                            Spell = true;
                            Danger = o.IsDangerous && Origin.Item(o.SpellName.ToLower() + "ccc").GetValue<bool>();
                            DangerUlt = o.IsDangerous && Origin.Item(o.SpellName.ToLower() + "ccc").GetValue<bool>() &&
                                        o.Slot.ToString() == "R";

                            Logger(LogType.Damage, "Dangerous (Skillshot " + o.Slot + "): " + Danger);
                            Logger(LogType.Damage,
                                heroSender.SkinName + " may hit (SkillShot: " + o.Slot + ") " + AggroTarget.SkinName +
                                " for: " + IncomeDamage);

                        });
                    }
                }
            }

            if (sender.Type == GameObjectType.obj_AI_Minion && sender.IsEnemy)
            {
                if (args.Target.Type == Me.Type)
                {
                    Danger = false; Dangercc = false; DangerUlt = false;
                    AggroTarget = ObjectManager.GetUnitByNetworkId<Obj_AI_Hero>(args.Target.NetworkId);

                    MinionDamage =
                        (float)sender.CalcDamage(AggroTarget, Damage.DamageType.Physical,
                            sender.BaseAttackDamage + sender.FlatPhysicalDamageMod);
                }
            }

            if (sender.Type == GameObjectType.obj_AI_Turret && sender.IsEnemy)
            {
                if (args.Target.Type == Me.Type)
                {
                    Danger = false; Dangercc = false; DangerUlt = false;
                    if (sender.Distance(Friendly().ServerPosition, true) <= 900*900)
                    {
                        AggroTarget = ObjectManager.GetUnitByNetworkId<Obj_AI_Hero>(args.Target.NetworkId);

                        IncomeDamage =
                            (float)sender.CalcDamage(AggroTarget, Damage.DamageType.Physical,
                                sender.BaseAttackDamage + sender.FlatPhysicalDamageMod);

                        Logger(LogType.Damage,
                            sender.Name + " (Turret Attack) " + AggroTarget.SkinName + " for: " + IncomeDamage);
                    }
                }
            }
        }
    }
}