#region LICENSE

// Copyright 2014 - 2014 SpellDetector
// TargetSpellDetector.cs is part of SpellDetector.
// SpellDetector is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// SpellDetector is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// You should have received a copy of the GNU General Public License
// along with SpellDetector. If not, see <http://www.gnu.org/licenses/>.

#endregion

#region

using System;
using LeagueSharp;
using LeagueSharp.Common;
using Oracle.Core.Helpers;

#endregion

namespace Oracle.Core.Targeted
{
    public class TargetSpellDetector
    {
        internal static bool InitComplete;

        public static int DetectionRange = 2000;

        public static SpellList<TargetSpell> ActiveTargeted = new SpellList<TargetSpell>();

        public delegate void OnTargetSpellDelegate(TargetSpell spell);

        public static event OnTargetSpellDelegate OnTargetSpell;

        static TargetSpellDetector()
        {
            if (!InitComplete)
            {
                InitComplete = true;
                Obj_AI_Base.OnProcessSpellCast += OnTargetSpellDetection;
                Game.OnUpdate += OnGameUpdate;
                Console.WriteLine("Target Spell Detector Init");
            }
        }

        private static void OnGameUpdate(EventArgs args)
        {
            try
            {
                if (OnTargetSpell == null)
                    return; // only if subscribers

                ActiveTargeted.RemoveAll(s => !s.IsActive);

                foreach (var spell in ActiveTargeted)
                {
                    OnTargetSpell(spell);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static void OnTargetSpellDetection(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            try
            {
                if (OnTargetSpell == null)
                    return; // only if subscribers

                if (sender.Position.Distance(ObjectManager.Player.Position) > DetectionRange)
                    return; // only detect spells in range

                if (!sender.IsValid<Obj_AI_Hero>())
                    return; // only hero

                if (!args.Target.IsValid<Obj_AI_Base>())
                    return; // only targeted

                if (args.SData.Name.ToLower().Contains("summoner") || args.SData.Name.ToLower().Contains("recall"))
                    return; // ignore summoners TODO: add summoners to database

                // TODO: add menu check

                var caster = (Obj_AI_Hero) sender;
                var target = (Obj_AI_Base) args.Target;
                var data = TargetSpellDatabase.GetByName(args.SData.Name);

                if (Orbwalking.IsAutoAttack(args.SData.Name))
                {
                    data = new TargetSpellData(
                        caster.ChampionName.ToLower(),
                        args.SData.Name.ToLower(),
                        SpellSlot.Unknown,
                        SpellType.AutoAttack,
                        CcType.No,
                        caster.AttackRange,
                        caster.AttackDelay,
                        caster.BasicAttack.MissileSpeed); // TODO: check melee
                }

                if (data == null)
                {
                    Program.Logger(Program.LogType.Error, "Target Spell not Found: " + args.SData.Name);
                    return;
                }

                ActiveTargeted.Add(new TargetSpell
                {
                    Caster = caster,
                    Target = target,
                    Spell = data,
                    StartTick = Environment.TickCount,
                    StartPosition = args.Start.To2D()
                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}