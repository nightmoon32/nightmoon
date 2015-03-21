#region

using System;
using System.Collections.Generic;
using LeagueSharp;
using LeagueSharp.Common;

#endregion

namespace Humanizer
{
    public class Program
    {
        public static Menu Menu;
        public static float LastMove;
        public static Obj_AI_Base Player = ObjectManager.Player;
        public static List<String> SpellList = new List<string> { "Q", "W", "E", "R" };
        public static List<float> LastCast = new List<float>();

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Menu = new Menu("花边-人性化", "Humanizer", true);

            var spells = Menu.AddSubMenu(new Menu("技能", "Spells"));

            for (var i = 0; i <= 3; i++)
            {
                LastCast.Add(0);
                var spell = SpellList[i];
                var menu = spells.AddSubMenu(new Menu(spell, spell));
                menu.AddItem(new MenuItem("Enabled" + i, "延迟 " + spell, true).SetValue(true));
                menu.AddItem(new MenuItem("Delay" + i, "施法延迟", true).SetValue(new Slider(80, 0, 400)));
            }

            var move = Menu.AddSubMenu(new Menu("动作", "Movement"));
            move.AddItem(new MenuItem("MovementEnabled", "启用").SetValue(true));
            move.AddItem(new MenuItem("MovementDelay", "移动延迟")).SetValue(new Slider(80, 0, 400));

            Menu.AddToMainMenu();

            Obj_AI_Base.OnIssueOrder += Obj_AI_Base_OnIssueOrder;
            Spellbook.OnCastSpell += Spellbook_OnCastSpell;
        }

        private static void Spellbook_OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (sender == null || !sender.Owner.IsMe || !args.Slot.IsMainSpell() ||Menu.Item("Enabled" + (int) args.Slot).GetValue<bool>())
            {
                return;
            }

            var delay = Menu.Item("Delay" + (int) args.Slot).GetValue<Slider>().Value;

            if (Environment.TickCount - LastCast[(int) args.Slot] < delay)
            {
                args.Process = false;
                return;
            }

            LastCast[(int) args.Slot] = Environment.TickCount;
        }

        private static void Obj_AI_Base_OnIssueOrder(Obj_AI_Base sender, GameObjectIssueOrderEventArgs args)
        {
            if (sender == null || !sender.IsValid || !sender.IsMe || args.Order != GameObjectOrder.MoveTo ||
                !Menu.Item("MovementEnabled").GetValue<bool>())
            {
                return;
            }

            if (Environment.TickCount - LastMove < Menu.Item("MovementDelay").GetValue<Slider>().Value)
            {
                args.Process = false;
                return;
            }

            LastMove = Environment.TickCount;
        }
    }
}