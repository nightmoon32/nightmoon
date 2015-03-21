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
    public class Amumu : PluginBase
    {

        private bool w使用 = false;
        public Amumu()
        {
            Q = new Spell(SpellSlot.Q, 1100);
            Q.SetSkillshot(Q.Instance.SData.SpellCastTime, Q.Instance.SData.LineWidth, Q.Instance.SData.MissileSpeed, true, SkillshotType.SkillshotLine);


            W = new Spell(SpellSlot.W, 300);
            E = new Spell(SpellSlot.E, 350);
            R = new Spell(SpellSlot.R, 550);

        }

        public override void OnUpdate(EventArgs args)
        {
            if (ComboMode)
            {

                

                var qPred = Q.GetPrediction(Target);

                if (Q.CastCheck(Target, "ComboQ"))
                {
                    Q.Cast(qPred.CastPosition);
                }

                if (W.IsReady() && !w使用 && Player.CountEnemiesInRange(R.Range) >= 1)
                {
                    W.Cast();
                    w使用 = true;
                }
                if (w使用 && Player.CountEnemiesInRange(R.Range) == 0)
                {
                    W.Cast();
                    w使用 = false;
                }

                if (E.CastCheck(Target, "ComboE"))
                {
                    E.Cast();
                }

                if (R.CastCheck(Target, "ComboR"))
                {
                    R.CastIfWillHit(Target, 2);
                }

            }
        }


        public override void ComboMenu(Menu config)
        {
            config.AddBool("ComboQ", "使用 Q", true);
            config.AddBool("ComboW", "使用 W", true);
            config.AddBool("ComboE", "使用 E", true);
            config.AddBool("ComboR", "使用 R", true);
        }
    }
}
