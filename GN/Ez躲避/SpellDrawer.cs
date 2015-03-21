using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Color = System.Drawing.Color;

using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace ezEvade
{
    internal class SpellDrawer
    {
        public static Menu menu;

        private static Obj_AI_Hero myHero { get { return ObjectManager.Player; } }


        public SpellDrawer(Menu mainMenu)
        {
            Drawing.OnDraw += Drawing_OnDraw;

            menu = mainMenu;
            Game_OnGameLoad();
        }

        private void Game_OnGameLoad()
        {
            //Game.PrintChat("SpellDrawer loaded");

            Menu drawMenu = new Menu("显示设置", "Draw");
            drawMenu.AddItem(new MenuItem("DrawSkillShots", "显示技能划线").SetValue(true));
            drawMenu.AddItem(new MenuItem("ShowStatus", "显示躲避状态").SetValue(true));
            drawMenu.AddItem(new MenuItem("DrawSpellPos", "显示技能位置").SetValue(false));

            Menu dangerMenu = new Menu("显示危险等级", "DangerLevelDrawings");
            Menu lowDangerMenu = new Menu("低", "LowDrawing");
            lowDangerMenu.AddItem(new MenuItem("LowWidth", "线宽度").SetValue(new Slider(3, 1, 15)));
            lowDangerMenu.AddItem(new MenuItem("LowColor", "颜色").SetValue(new Circle(true, Color.FromArgb(60, 255, 255, 255))));

            Menu normalDangerMenu = new Menu("普通", "NormalDrawing");
            normalDangerMenu.AddItem(new MenuItem("NormalWidth", "线宽度").SetValue(new Slider(3, 1, 15)));
            normalDangerMenu.AddItem(new MenuItem("NormalColor", "颜色").SetValue(new Circle(true, Color.FromArgb(140, 255, 255, 255))));

            Menu highDangerMenu = new Menu("高", "HighDrawing");
            highDangerMenu.AddItem(new MenuItem("HighWidth", "线宽度").SetValue(new Slider(4, 1, 15)));
            highDangerMenu.AddItem(new MenuItem("HighColor", "颜色").SetValue(new Circle(true, Color.FromArgb(255, 255, 255, 255))));

            Menu extremeDangerMenu = new Menu("很高", "ExtremeDrawing");
            extremeDangerMenu.AddItem(new MenuItem("ExtremeWidth", "线宽度").SetValue(new Slider(4, 1, 15)));
            extremeDangerMenu.AddItem(new MenuItem("ExtremeColor", "颜色").SetValue(new Circle(true, Color.FromArgb(255, 255, 255, 255))));

            /*
            Menu undodgeableDangerMenu = new Menu("Undodgeable", "Undodgeable");
            undodgeableDangerMenu.AddItem(new MenuItem("Width", "Line Width").SetValue(new Slider(6, 1, 15)));
            undodgeableDangerMenu.AddItem(new MenuItem("Color", "Color").SetValue(new Circle(true, Color.FromArgb(255, 255, 0, 0))));*/

            dangerMenu.AddSubMenu(lowDangerMenu);
            dangerMenu.AddSubMenu(normalDangerMenu);
            dangerMenu.AddSubMenu(highDangerMenu);
            dangerMenu.AddSubMenu(extremeDangerMenu);

            drawMenu.AddSubMenu(dangerMenu);
            
            menu.AddSubMenu(drawMenu);           
        }

        private void DrawLineRectangle(Vector2 start, Vector2 end, int radius, int width, Color color)
        {
            var dir = (end - start).Normalized();
            var pDir = dir.Perpendicular();

            var rightStartPos = start + pDir * radius;
            var leftStartPos = start - pDir * radius;
            var rightEndPos = end + pDir * radius;
            var leftEndPos = end - pDir * radius;

            var rStartPos = Drawing.WorldToScreen(new Vector3(rightStartPos.X, rightStartPos.Y, myHero.Position.Z));
            var lStartPos = Drawing.WorldToScreen(new Vector3(leftStartPos.X, leftStartPos.Y, myHero.Position.Z));
            var rEndPos = Drawing.WorldToScreen(new Vector3(rightEndPos.X, rightEndPos.Y, myHero.Position.Z));
            var lEndPos = Drawing.WorldToScreen(new Vector3(leftEndPos.X, leftEndPos.Y, myHero.Position.Z));

            Drawing.DrawLine(rStartPos, rEndPos, width, color);
            Drawing.DrawLine(lStartPos, lEndPos, width, color);
            Drawing.DrawLine(rStartPos, lStartPos, width, color);
            Drawing.DrawLine(lEndPos, rEndPos, width, color);
        }

        private void DrawEvadeStatus()
        {
            if (menu.SubMenu("Draw").Item("ShowStatus").GetValue<bool>())
            {
                var heroPos = Drawing.WorldToScreen(ObjectManager.Player.Position);

                if (menu.SubMenu("Main").Item("DodgeSkillShots").GetValue<KeyBind>().Active
                    && Evade.isDodgeDangerousEnabled())
                {
                    Drawing.DrawText(heroPos.X, heroPos.Y, Color.Red, "Evade: ON");
                }
                else if (menu.SubMenu("Main").Item("DodgeSkillShots").GetValue<KeyBind>().Active)
                {
                    Drawing.DrawText(heroPos.X, heroPos.Y, Color.White, "韬查伩锛氶枊");
                }
            }
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            if (menu.SubMenu("Draw").Item("DrawSkillShots").GetValue<bool>() == false)
            {
                return;
            }

            DrawEvadeStatus();

            foreach (KeyValuePair<int, Spell> entry in SpellDetector.drawSpells)
            {
                Spell spell = entry.Value;

                var dangerStr = EvadeHelper.GetSpellDangerString(spell);
                var spellDrawingConfig = Evade.menu.SubMenu("Draw").SubMenu("DangerLevelDrawings")
                    .SubMenu(dangerStr + "Drawing").Item(dangerStr + "Color").GetValue<Circle>();
                var spellDrawingWidth = Evade.menu.SubMenu("Draw").SubMenu("DangerLevelDrawings")
                    .SubMenu(dangerStr + "Drawing").Item(dangerStr + "Width").GetValue<Slider>().Value;                

                if (Evade.menu.SubMenu("Spells").SubMenu(spell.info.charName + spell.info.spellName + "Settings")
                    .Item(spell.info.spellName + "DrawSpell").GetValue<bool>()
                    && spellDrawingConfig.Active)
                {
                    if (spell.info.spellType == SpellType.Line)
                    {
                        Vector2 spellPos = SpellDetector.GetCurrentSpellPosition(spell);                                             
                        DrawLineRectangle(spellPos, spell.endPos, (int)EvadeHelper.GetSpellRadius(spell), spellDrawingWidth, spellDrawingConfig.Color);

                        if (menu.SubMenu("Draw").Item("DrawSpellPos").GetValue<bool>())
                        {
                            /*if (true)
                            {
                                var spellPos2 = spell.startPos + spell.direction * spell.info.projectileSpeed * (Evade.GetTickCount() - spell.startTime - spell.info.spellDelay) / 1000 + spell.direction * spell.info.projectileSpeed * ((float)Game.Ping / 1000);
                                Render.Circle.DrawCircle(new Vector3(spellPos2.X, spellPos2.Y, myHero.Position.Z), (int)EvadeHelper.GetSpellRadius(spell), Color.Red, 8);
                            }*/
                            
                            Render.Circle.DrawCircle(new Vector3(spellPos.X, spellPos.Y, myHero.Position.Z), (int)EvadeHelper.GetSpellRadius(spell), spellDrawingConfig.Color, spellDrawingWidth);
                        } 

                    }
                    else if (spell.info.spellType == SpellType.Circular)
                    {
                        Render.Circle.DrawCircle(new Vector3(spell.endPos.X, spell.endPos.Y, myHero.Position.Z), (int)EvadeHelper.GetSpellRadius(spell), spellDrawingConfig.Color, spellDrawingWidth);
                    }
                    else if (spell.info.spellType == SpellType.Cone)
                    {

                    }
                }
            }
        }
    }
}
