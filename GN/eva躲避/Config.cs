// Copyright 2014 - 2014 Esk0r
// Config.cs is part of Evade.
// 
// Evade is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// Evade is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Evade. If not, see <http://www.gnu.org/licenses/>.

#region

using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

#endregion

namespace Evade
{
    internal static class Config
    {
        public const bool PrintSpellData = false;
        public const bool TestOnAllies = false;
        public const int SkillShotsExtraRadius = 9;
        public const int SkillShotsExtraRange = 20;
        public const int GridSize = 10;
        public const int ExtraEvadeDistance = 15;
        public const int DiagonalEvadePointsCount = 7;
        public const int DiagonalEvadePointsStep = 20;

        public const int CrossingTimeOffset = 250;

        public const int EvadingFirstTimeOffset = 250;
        public const int EvadingSecondTimeOffset = 0;

        public const int EvadingRouteChangeTimeOffset = 250;

        public const int EvadePointChangeInterval = 300;
        public static int LastEvadePointChangeT = 0;

        public static Menu Menu;

        public static void CreateMenu()
        {
            Menu = new Menu("����-Eva���", "Evade", true);

            //Create the evade spells submenus.
            var evadeSpells = new Menu("��ܼ���", "evadeSpells");
            foreach (var spell in EvadeSpellDatabase.Spells)
            {
                var subMenu = new Menu(spell.Name, spell.Name);

                subMenu.AddItem(
                    new MenuItem("DangerLevel" + spell.Name, "Σ�յȼ�").SetValue(
                        new Slider(spell.DangerLevel, 5, 1)));

                if (spell.IsTargetted && spell.ValidTargets.Contains(SpellValidTargets.AllyWards))
                {
                    subMenu.AddItem(new MenuItem("WardJump" + spell.Name, "����").SetValue(true));
                }

                subMenu.AddItem(new MenuItem("Enabled" + spell.Name, "����").SetValue(true));

                evadeSpells.AddSubMenu(subMenu);
            }
            Menu.AddSubMenu(evadeSpells);

            //Create the skillshots submenus.
            var skillShots = new Menu("��������", "Skillshots");

            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (hero.Team != ObjectManager.Player.Team || Config.TestOnAllies)
                {
                    foreach (var spell in SpellDatabase.Spells)
                    {
                        if (spell.ChampionName.ToLower() == hero.ChampionName.ToLower())
                        {
                            var subMenu = new Menu(spell.MenuItemName, spell.MenuItemName);

                            subMenu.AddItem(
                                new MenuItem("DangerLevel" + spell.MenuItemName, "Σ�յȼ�").SetValue(
                                    new Slider(spell.DangerValue, 5, 1)));

                            subMenu.AddItem(
                                new MenuItem("IsDangerous" + spell.MenuItemName, "Σ��").SetValue(
                                    spell.IsDangerous));

                            subMenu.AddItem(new MenuItem("Draw" + spell.MenuItemName, "��Ȧ").SetValue(true));
                            subMenu.AddItem(new MenuItem("Enabled" + spell.MenuItemName, "����").SetValue(true));

                            skillShots.AddSubMenu(subMenu);
                        }
                    }
                }
            }

            Menu.AddSubMenu(skillShots);

            var shielding = new Menu("����", "Shielding");

            foreach (var ally in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (ally.IsAlly && !ally.IsMe)
                {
                    shielding.AddItem(
                        new MenuItem("shield" + ally.ChampionName, "����" + ally.ChampionName).SetValue(true));
                }
            }
            Menu.AddSubMenu(shielding);

            var collision = new Menu("��ײ���", "Collision");
            collision.AddItem(new MenuItem("MinionCollision", "С��").SetValue(false));
            collision.AddItem(new MenuItem("HeroCollision", "Ӣ��").SetValue(false));
            collision.AddItem(new MenuItem("YasuoCollision", "������ǽ").SetValue(true));
            collision.AddItem(new MenuItem("EnableCollision", "����").SetValue(true));
            //TODO add mode.
            Menu.AddSubMenu(collision);

            var drawings = new Menu("��Χ��ʾ", "Drawings");
            drawings.AddItem(new MenuItem("EnabledColor", "���÷�����ɫ").SetValue(Color.White));
            drawings.AddItem(new MenuItem("DisabledColor", "���÷�����ɫ").SetValue(Color.Red));
            drawings.AddItem(new MenuItem("MissileColor", "������ɫ").SetValue(Color.LimeGreen));
            drawings.AddItem(new MenuItem("Border", "�߿���").SetValue(new Slider(1, 5, 1)));

            drawings.AddItem(new MenuItem("EnableDrawings", "����").SetValue(true));
            Menu.AddSubMenu(drawings);

            var misc = new Menu("����", "Misc");
            misc.AddItem(new MenuItem("DisableFow", "����ս����������").SetValue(false));
            misc.AddItem(new MenuItem("ShowEvadeStatus", "��ʾ���״��").SetValue(false));
            Menu.AddSubMenu(misc);

            Menu.AddItem(
                new MenuItem("Enabled", "����").SetValue(new KeyBind("K".ToCharArray()[0], KeyBindType.Toggle, true)));

            Menu.AddItem(
                new MenuItem("OnlyDangerous", "ֻ���Σ�ռ���").SetValue(new KeyBind(32, KeyBindType.Press)));
				

            Menu.AddToMainMenu();
        }
    }
}
