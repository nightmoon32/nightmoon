#region LICENSE

// Copyright 2014 - 2014 Support
// Soraka.cs is part of Support.
// Support is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// Support is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// You should have received a copy of the GNU General Public License
// along with Support. If not, see <http://www.gnu.org/licenses/>.

#endregion

#region

using LeagueSharp;
using LeagueSharp.Common;
using Support.Util;
using System;
using ActiveGapcloser = Support.Util.ActiveGapcloser;

#endregion

namespace Support.Plugins
{
    public class Soraka : PluginBase
    {
        public Soraka()
        {
            Q = new Spell(SpellSlot.Q, 950);
            W = new Spell(SpellSlot.W, 450);
            E = new Spell(SpellSlot.E, 925);
            R = new Spell(SpellSlot.R);

            Q.SetSkillshot(0.5f, 300, 1750, false, SkillshotType.SkillshotCircle);
            E.SetSkillshot(0.5f, 70f, 1750, false, SkillshotType.SkillshotCircle);
        }

        public override void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            var unit = gapcloser.Sender;

            if (ConfigValue<bool>("GapcloserQ") && unit.IsValidTarget(Q.Range) && Q.IsReady())
                Q.Cast(unit);

            if (ConfigValue<bool>("GapcloserE") && unit.IsValidTarget(E.Range) && E.IsReady())
                E.Cast(unit);
        }

        public override void OnPossibleToInterrupt(Obj_AI_Base target, InterruptableSpell spell)
        {
            if (!ConfigValue<bool>("InterruptE") ||
                spell.DangerLevel < InterruptableDangerLevel.High ||
                target.IsAlly)
                return;

            if (!target.IsValidTarget(E.Range))
                return;

            if (!E.IsReady())
                return;

            E.Cast(target);
        }

        public override void OnUpdate(EventArgs args)
        {
            if (ComboMode)
            {
                if (Q.CastCheck(Target, "ComboQ"))
                {
                    Q.Cast(Target);
                }

                if (E.CastCheck(Target, "ComboE"))
                {
                    E.Cast(Target);
                }
            }

            if (HarassMode)
            {
                if (Q.CastCheck(Target, "HarassQ"))
                {
                    Q.Cast(Target);
                }

                if (E.CastCheck(Target, "HarassE"))
                {
                    E.Cast(Target);
                }
            }

            AutoW();

            AutoR();
        }

        private void AutoW()
        {
            if (W.IsReady() && ConfigValue<bool>("AutoW"))
            {
                var ally = Helpers.AllyBelowHp(ConfigValue<Slider>("AutoWPercent").Value, W.Range);

                if (Player.IsRecalling() ||
                    ally.IsRecalling() ||
                    ObjectManager.Player.InFountain())
                    return;

                if (ally != null)
                {
                    W.CastOnUnit(ally);
                }
            }
        }

        private void AutoR()
        {
            if (R.IsReady() && ConfigValue<bool>("AutoR"))
            {
                var ally = Helpers.AllyBelowHp(ConfigValue<Slider>("AutoRPercent").Value, R.Range);

                if (ally != null || (Player.Health / Player.MaxHealth) * 100 < ConfigValue<Slider>("AutoRPercent").Value)
                {
                    R.Cast();
                }

            }
        }

        public override void ComboMenu(Menu config)
        {
            config.AddBool("ComboQ", "使用 Q", true);
            config.AddBool("ComboE", "使用 E", true);
        }

        public override void HarassMenu(Menu config)
        {
            config.AddBool("HarassQ", "使用 Q", true);
            config.AddBool("HarassE", "使用 E", true);
        }

        public override void MiscMenu(Menu config)
        {
            config.AddBool("GapcloserQ", "使用 Q 防止突进", true);
            config.AddBool("GapcloserE", "使用 E 防止突进", true);

            config.AddBool("AutoW", "Auto 使用 W", true);
            config.AddSlider("AutoWPercent", "W Percent", 50, 1, 100);

            config.AddBool("AutoR", "Auto 使用 R", true);
            config.AddSlider("AutoRPercent", "R Percent", 15, 1, 100);

            config.AddBool("InterruptE", "使用 E 打断技能", true);
        }
    }
}