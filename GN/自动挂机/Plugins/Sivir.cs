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
    public class Sivir : PluginBase
    {
        public Sivir()
        {
            Q = new Spell(SpellSlot.Q, 1250);
            Q.SetSkillshot(0.25f, 90f, 1350f, false, SkillshotType.SkillshotLine);

            W = new Spell(SpellSlot.W, 593);
        }


        public override void OnAfterAttack(AttackableUnit unit, AttackableUnit target) 
        {

            var t = target as Obj_AI_Hero;
            if (t != null && unit.IsMe){
                if (W.IsReady())
                {
                    W.Cast();
                    
                }
                if (R.IsReady())
                {
                    R.Cast();
                }
            }

        }

        public override void OnUpdate(EventArgs args)
        {

            if (Q.IsReady())
            {
                foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(h => h.IsValidTarget(Q.Range)))
                {
                    Q.CastIfHitchanceEquals(enemy, HitChance.Immobile);
                }
            }

            if (ComboMode)
            {
                if (Q.CastCheck(Target, "ComboQ"))
                {
                    Q.Cast(Target);
                }
                if(R.IsReady() && Player.CountEnemiesInRange(600) > 2){
                    R.Cast();
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
