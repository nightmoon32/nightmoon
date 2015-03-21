

//xaxixeo *-*
using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Support.Evade;
using Support.Util;
using ActiveGapcloser = Support.Util.ActiveGapcloser;
using SpellData = LeagueSharp.SpellData;

namespace Support.Plugins
{
    public class Lucian : PluginBase
    {
        public Lucian()
        {
            Q = new Spell(SpellSlot.Q, 500);
            W = new Spell(SpellSlot.W, 1000);
            E = new Spell(SpellSlot.E, 445);
            R = new Spell(SpellSlot.R, 1400);

            Q.SetTargetted(0.5f, float.MaxValue);
            W.SetSkillshot(300, 80, 1600, true, SkillshotType.SkillshotLine);
            E.SetSkillshot(250, 1, float.MaxValue, false, SkillshotType.SkillshotLine);
            R.SetSkillshot(10, 110, 2800, true, SkillshotType.SkillshotLine);
        }

        public override void OnUpdate(EventArgs args)
        {
            if (ComboMode)
            {
                if (Q.CastCheck(Target, "ComboQ"))
                {
                    Q.Cast(Target);
                }
                if (W.CastCheck(Target, "ComboW"))
                {
                    W.Cast(Target);
                }

                if (R.CastCheck(Target, "ComboR") || R.IsKillable(Target))
                {
                    R.Cast(Target);
                }
            }

            if (HarassMode)
            {
                if (Q.CastCheck(Target, "HarassQ"))
                {
                    Q.Cast(Target);
                }
                if (W.CastCheck(Target, "HarassW"))
                {
                    W.Cast(Target);
                }
            }
        }

        public override void ComboMenu(Menu config)
        {
            config.AddBool("ComboQ", "使用 Q", true);
            config.AddBool("ComboW", "使用 W", true);
            config.AddBool("ComboR", "使用 R", true);
        }

        public override void HarassMenu(Menu config)
        {
            config.AddBool("HarassQ", "使用 Q", true);
            config.AddBool("HarassW", "使用 W", true);
        }
    }
}

