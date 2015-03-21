using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using OC = Oracle.Program;

namespace Oracle.Extensions
{
    internal static class 进攻
    {
        private static Menu _mainMenu, _menuConfig;
        private static readonly Obj_AI_Hero Me = ObjectManager.Player;

        public static void Initialize(Menu root)
        {
            Game.OnUpdate += Game_OnGameUpdate;

            _mainMenu = new Menu("进攻", "omenu");
            _menuConfig = new Menu("进攻对象", "oconfig");

            foreach (var x in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsEnemy))
                _menuConfig.AddItem(new MenuItem("ouseOn" + x.SkinName, "使用给 " + x.SkinName)).SetValue(true);
            _mainMenu.AddSubMenu(_menuConfig);

            CreateMenuItem("魔宗利刃", "Muramana", 90, 30, true);
            CreateMenuItem("提亚马特/九头蛇", "Hydra", 90, 30);
            CreateMenuItem("科技枪", "Hextech", 90, 30);
            CreateMenuItem("幽梦", "Youmuus", 90, 30);
            CreateMenuItem("弯刀", "Cutlass", 90, 30);
            CreateMenuItem("破败", "Botrk", 70, 70);
            CreateMenuItem("冰霜女王", "Frostclaim", 100, 30);
            CreateMenuItem("神圣之剑", "Divine", 90, 30);
            CreateMenuItem("守护者的号角", "Guardians", 90, 30);
            CreateMenuItem("冰霜战锤", "Entropy", 90, 30);

            root.AddSubMenu(_mainMenu);
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (!Me.IsValidTarget(300, false))
            {
                return;
            }

            if (_mainMenu.Item("useMuramana").GetValue<bool>())
            {
                if (OC.CanManamune)
                {
                    if (_mainMenu.Item("muraMode").GetValue<StringList>().SelectedIndex != 1 ||
                        OC.Origin.Item("usecombo").GetValue<KeyBind>().Active)
                    {
                        var manamune = Me.GetSpellSlot("Muramana");
                        if (manamune != SpellSlot.Unknown && !Me.HasBuff("Muramana"))
                        {
                            if (Me.Mana/Me.MaxMana*100 > _mainMenu.Item("useMuramanaMana").GetValue<Slider>().Value)
                                Me.Spellbook.CastSpell(manamune);

                            Utility.DelayAction.Add(400, () => OC.CanManamune = false);
                        }
                    }
                }

                if (!OC.CanManamune && !OC.Origin.Item("usecombo").GetValue<KeyBind>().Active)
                {
                    var manamune = Me.GetSpellSlot("Muramana");
                    if (manamune != SpellSlot.Unknown && Me.HasBuff("Muramana"))
                    {
                        Me.Spellbook.CastSpell(manamune);
                    }
                }
            }

            if (OC.Origin.Item("usecombo").GetValue<KeyBind>().Active)
            {
                UseItem("Entropy", 3184, 450f, true);
                UseItem("Guardians", 2051, 450f);
                UseItem("Entropy", 3184, 450f, true);
                UseItem("Frostclaim", 3092, 850f, true);
                UseItem("Youmuus", 3142, 650f);
                UseItem("Hydra", 3077, 250f);
                UseItem("Hydra", 3074, 250f);
                UseItem("Hextech", 3146, 700f, true);
                UseItem("Cutlass", 3144, 450f, true);
                UseItem("Botrk", 3153, 450f, true);
                UseItem("Divine", 3131, 650f);
            }
            
        }

        private static void UseItem(string name, int itemId, float range, bool targeted = false)
        {
            if (!Items.HasItem(itemId) || !Items.CanUseItem(itemId))
                return;

            if (!_mainMenu.Item("use" + name).GetValue<bool>())
                return;

            Obj_AI_Hero target = null;

            // Get current target near mouse cursor.
            foreach (
                var targ in
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(hero => hero.IsValidTarget(2000))
                        .OrderByDescending(hero => hero.Distance(Game.CursorPos)))
            {
                target = targ;
            }

            if (target.IsValidTarget(range))
            {
                var eHealthPercent = (int)((target.Health / target.MaxHealth) * 100);
                var aHealthPercent = (int)((Me.Health / target.MaxHealth) * 100);

                if (eHealthPercent <= _mainMenu.Item("use" + name + "Pct").GetValue<Slider>().Value &&
                    _mainMenu.Item("ouseOn" + target.SkinName).GetValue<bool>())
                {
                    if (targeted && itemId == 3092)
                    {
                        var pi = new PredictionInput
                        {
                            Aoe = true,
                            Collision = false,
                            Delay = 0.0f,
                            From = Me.Position,
                            Radius = 250f,
                            Range = 850f,
                            Speed = 1500f,
                            Unit = target,
                            Type = SkillshotType.SkillshotCircle
                        };

                        var po = Prediction.GetPrediction(pi);
                        if (po.Hitchance >= HitChance.Medium)
                        {
                            Items.UseItem(itemId, po.CastPosition);
                            OC.Logger(OC.LogType.Action,
                                "Used " + name + " near " + po.CastPosition.CountEnemiesInRange(300) + " enemies!");
                        }

                    }

                    else if (targeted)
                    {
                        Items.UseItem(itemId, target);
                        OC.Logger(Program.LogType.Action, "Used " + name + " (Targeted Enemy HP) on " + target.SkinName);
                    }

                    else
                    {
                        Items.UseItem(itemId);
                        OC.Logger(Program.LogType.Action, "Used " + name + " (Self Enemy HP) on " + target.SkinName);
                    }
                }

                else if (aHealthPercent <= _mainMenu.Item("use" + name + "Me").GetValue<Slider>().Value &&
                         _mainMenu.Item("ouseOn" + target.SkinName).GetValue<bool>())
                {
                    if (targeted)
                        Items.UseItem(itemId, target);
                    else
                        Items.UseItem(itemId);

                    OC.Logger(Program.LogType.Action, "Used " + name + " (Low My HP) on " + target.SkinName);
                }
            }
        }

        private static void CreateMenuItem(string displayname, string name, int evalue, int avalue, bool usemana = false)
        {
            var menuName = new Menu(displayname, name.ToLower());

            menuName.AddItem(new MenuItem("use" + name, "使用 " + displayname)).SetValue(true);
            menuName.AddItem(new MenuItem("use" + name + "Pct", "敌人血量 %")).SetValue(new Slider(evalue));

            if (!usemana)
                menuName.AddItem(new MenuItem("use" + name + "Me", "自己血量 %")).SetValue(new Slider(avalue));

            if (usemana)
                menuName.AddItem(new MenuItem("use" + name + "Mana", "最小法力 %使用")).SetValue(new Slider(35));


            if (name == "Muramana")
                menuName.AddItem(
                    new MenuItem("muraMode", " 模式: ").SetValue(new StringList(new[] {"总是", "连招"}, 1)));

            _mainMenu.AddSubMenu(menuName);
        }
    }
}