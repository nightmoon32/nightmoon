using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace ezEvade
{

    internal class Evade
    {
        private static Obj_AI_Hero myHero { get { return ObjectManager.Player; } }

        public static SpellDetector spellDetector;
        private static SpellDrawer spellDrawer;
        private static EvadeTester evadeTester;
        private static EvadeSpell evadeSpell;

        private static SpellSlot lastSpellCast;

        public static float lastTickCount;

        public static bool isDodging = false;
        public static bool dodgeOnlyDangerous = false;

        public static EvadeHelper.PositionInfo lastPosInfo;
        public static EvadeCommand lastEvadeCommand = new EvadeCommand { isProcessed = true };

        public static Menu menu;      


        public Evade()
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
            Obj_AI_Hero.OnIssueOrder += Game_OnIssueOrder;
            Spellbook.OnCastSpell += Game_OnCastSpell;
            Game.OnUpdate += Game_OnGameUpdate;
            Game.OnSendPacket += Game_OnSendPacket;

            SpellDetector.OnCreateSpell += SpellDetector_OnCreateSpell;            
        }

        private void Game_OnGameLoad(EventArgs args)
        {
            Game.PrintChat("<font color=\"#00BFFF\">Ez韬查伩婕㈠寲 by Huabian</font>");

            menu = new Menu("花边-EZ躲避", "ezEvade", true);

            Menu mainMenu = new Menu("躲避设置", "Main");
            mainMenu.AddItem(new MenuItem("DodgeSkillShots", "躲避指向性技能").SetValue(new KeyBind('K', KeyBindType.Toggle, true)));
            mainMenu.AddItem(new MenuItem("UseEvadeSpells", "使用技能躲避").SetValue(true));
            mainMenu.AddItem(new MenuItem("DodgeDangerous", "只躲避危险技能").SetValue(false));
            mainMenu.AddItem(new MenuItem("DodgeFOWSpells", "躲避直线技能").SetValue(true));
            mainMenu.AddItem(new MenuItem("DodgeCircularSpells", "躲避圆形技能").SetValue(true));            
            menu.AddSubMenu(mainMenu);

            spellDetector = new SpellDetector(menu);
            evadeSpell = new EvadeSpell(menu);

            Menu keyMenu = new Menu("按键设置", "KeySettings");
            //keyMenu.AddItem(new MenuItem("DodgeSkillShotsKey", "Dodge SkillShots Key").SetValue(new KeyBind('K', KeyBindType.Press)));
            keyMenu.AddItem(new MenuItem("DodgeDangerousKeyEnabled", "只躲避危险技能").SetValue(false));
            keyMenu.AddItem(new MenuItem("DodgeDangerousKey", "只躲避危险技能 按键").SetValue(new KeyBind(32, KeyBindType.Press)));
            keyMenu.AddItem(new MenuItem("DodgeDangerousKey2", "只躲避危险技能 按键2").SetValue(new KeyBind('V', KeyBindType.Press)));
            menu.AddSubMenu(keyMenu);                       

            Menu miscMenu = new Menu("杂项设置", "MiscSettings");
            miscMenu.AddItem(new MenuItem("HigherPrecision", "增强躲避度").SetValue(true));
            miscMenu.AddItem(new MenuItem("RecalculatePosition", "重新计算路径").SetValue(true));
            //miscMenu.AddItem(new MenuItem("CalculateHeroPos", "Calculate Hero Position").SetValue(false));

            Menu fastEvadeMenu = new Menu("快速躲避", "FastEvade");
            fastEvadeMenu.AddItem(new MenuItem("FastEvadeActivationTime", "快速躲避时间").SetValue(new Slider(150, 0, 300)));
            fastEvadeMenu.AddItem(new MenuItem("RejectMinDistance", "碰撞距离缓冲区域").SetValue(new Slider(10, 0, 100)));

            miscMenu.AddSubMenu(fastEvadeMenu);

            Menu bufferMenu = new Menu("额外缓冲", "ExtraBuffers");
            bufferMenu.AddItem(new MenuItem("ExtraPingBuffer", "额外Ping缓冲").SetValue(new Slider(20, 0, 150)));
            bufferMenu.AddItem(new MenuItem("ExtraCPADistance", "额外碰撞距离").SetValue(new Slider(30, 0, 150)));
            bufferMenu.AddItem(new MenuItem("ExtraSpellRadius", "额外法术半径").SetValue(new Slider(0, 0, 100)));
            bufferMenu.AddItem(new MenuItem("ExtraEvadeDistance", "额外的回避距离").SetValue(new Slider(0, 0, 100)));
            bufferMenu.AddItem(new MenuItem("ExtraAvoidDistance", "额外的避免距离").SetValue(new Slider(0, 0, 100)));

            bufferMenu.AddItem(new MenuItem("MinComfortZone", "最小缓和").SetValue(new Slider(400, 0, 1000)));

            miscMenu.AddSubMenu(bufferMenu);
            menu.AddSubMenu(miscMenu);

            menu.AddToMainMenu();

            spellDrawer = new SpellDrawer(menu);
            //evadeTester = new EvadeTester(menu);
        }

        public static float GetTickCount()
        {
            return (float)DateTime.Now.TimeOfDay.TotalMilliseconds; //Game.ClockTime * 1000;
        }

        private void Game_OnSendPacket(GamePacketEventArgs args)
        {            
            // Check if the packet sent is a spell cast
            if (args.PacketData[0] == 228)
            {
                if (isDodging)
                {
                    foreach (KeyValuePair<String, SpellData> entry in SpellDetector.windupSpells)
                    {
                        SpellData spellData = entry.Value;

                        if (spellData.spellKey == lastSpellCast) //check if it's a spell that we should block
                        {
                            args.Process = false;
                            return;
                        }
                    }
                }
            }
        }

        private void Game_OnCastSpell(Spellbook hero, SpellbookCastSpellEventArgs args)
        {
            if (!hero.Owner.IsMe)
                return;

            lastSpellCast = args.Slot;
        }

        private void Game_OnIssueOrder(Obj_AI_Base hero, GameObjectIssueOrderEventArgs args)
        {
            if (!hero.IsMe)
                return;

            if (args.Order == GameObjectOrder.MoveTo)
            {

                //movement block code goes in here
                if (isDodging)
                {
                    args.Process = false; //Block the command
                }
                else
                {
                    var movePos = args.TargetPosition.To2D();
                    if (EvadeHelper.checkMovePath(movePos))
                    {
                        args.Process = false; //Block the command

                        var posInfo = EvadeHelper.GetBestPositionMovementBlock(movePos);
                        if (posInfo != null)
                        {
                            EvadeCommand.MoveTo(posInfo.position);
                        }
                        return;
                    }
                }
            }
            else //need more logic
            {
                if (isDodging)
                {
                    args.Process = false; //Block the command
                }
            }
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            if (myHero.IsDead)
                return;

            if (Evade.GetTickCount() - lastTickCount > 50) //Tick limiter
            {                
                DodgeSkillShots(); //walking
                //EvadeSpell.UseEvadeSpell(); //using spells
                lastTickCount = Evade.GetTickCount();
            }

            CheckDodgeOnlyDangerous();
        }

        private void DodgeSkillShots()
        {
            if (menu.SubMenu("Main").Item("DodgeSkillShots").GetValue<KeyBind>().Active == false)
            {
                return;
            }

            bool playerInDanger = false;
            foreach (KeyValuePair<int, Spell> entry in SpellDetector.spells)
            {
                Spell spell = entry.Value;
                                
                if (lastPosInfo.dodgeableSpells.Contains(spell.spellID) &&
                    EvadeHelper.inSkillShot(spell, myHero.ServerPosition.To2D(), myHero.BoundingRadius))
                {
                    playerInDanger = true;
                    break;
                }                
            }

            /*
            if (isDodging && playerInDanger == false) //serverpos test
            {
                myHero.IssueOrder(GameObjectOrder.HoldPosition, myHero, false);
            }*/

            isDodging = playerInDanger;         


            if (isDodging)
            {
                Vector2 lastBestPosition = lastPosInfo.position;                

                if (lastBestPosition.Distance(myHero.ServerPosition.To2D()) < 3) //a bit faulty
                {
                    //isDodging = false;
                }
                
                if (menu.SubMenu("MiscSettings").Item("RecalculatePosition").GetValue<bool>() && lastPosInfo != null)//recheck path
                {
                    var path = myHero.Path;
                    if (path.Length > 0)
                    {
                        var movePos = path[path.Length - 1].To2D();

                        if (movePos.Distance(lastPosInfo.position) < 5) //more strict checking
                        {
                            var posInfo = EvadeHelper.canHeroWalkToPos(movePos, myHero.MoveSpeed, 0);
                            if (EvadeHelper.isSamePosInfo(posInfo, lastPosInfo) && posInfo.posDangerCount > lastPosInfo.posDangerCount)
                            {
                                lastPosInfo = EvadeHelper.GetBestPosition();
                            }
                        }
                    }
                }

                EvadeCommand.MoveTo(lastBestPosition);
            }
            else //if not dodging
            {
                //Check if hero will walk into a skillshot
                var path = myHero.Path;
                if (path.Length > 0)
                {
                    var movePos = path[path.Length - 1].To2D();

                    if (EvadeHelper.checkMovePath(movePos))
                    {
                        var posInfo = EvadeHelper.GetBestPositionMovementBlock(movePos);
                        if (posInfo != null)
                        {
                            EvadeCommand.MoveTo(posInfo.position);
                        }
                        return;
                    }
                }
            }
        }      
               
        public static bool isDodgeDangerousEnabled()
        {
            if (menu.SubMenu("Main").Item("DodgeDangerous").GetValue<bool>() == true)
            {
                return true;
            }

            if (menu.SubMenu("KeySettings").Item("DodgeDangerousKeyEnabled").GetValue<bool>() == true)
            {
                if (menu.SubMenu("KeySettings").Item("DodgeDangerousKey").GetValue<KeyBind>().Active == true
                || menu.SubMenu("KeySettings").Item("DodgeDangerousKey2").GetValue<KeyBind>().Active == true)
                    return true;
            }

            return false;
        }

        public static void CheckDodgeOnlyDangerous() //Dodge only dangerous event
        {
            bool bDodgeOnlyDangerous = isDodgeDangerousEnabled();

            if (dodgeOnlyDangerous == false && bDodgeOnlyDangerous)
            {
                spellDetector.RemoveNonDangerousSpells();
                dodgeOnlyDangerous = true;
            }
            else
            {
                dodgeOnlyDangerous = bDodgeOnlyDangerous;
            }
        }

        private void SpellDetector_OnCreateSpell(Spell newSpell)
        {
            var posInfo = EvadeHelper.GetBestPosition();
            lastPosInfo = posInfo;

            //Game.PrintChat("SkillsDodged: " + lastPosInfo.dodgeableSpells.Count + " DangerLevel: " + lastPosInfo.posDangerLevel);
                        
            DodgeSkillShots(); //walking
            EvadeSpell.UseEvadeSpell(); //using spells

        }

        public static void CheckMovingIntoDanger(Vector2 movePos)
        {
            bool intersect = EvadeHelper.checkMovePath(movePos);
            if (intersect)
            {
                var posInfo = EvadeHelper.GetBestPositionMovementBlock(movePos);
                if (posInfo != null) //check if there is solution
                {
                    myHero.IssueOrder(GameObjectOrder.MoveTo, posInfo.position.To3D());
                }
            }
        }

    }
}
