using System;
using System.Collections.Generic;
using System.Drawing;
using LeagueSharp;
using LeagueSharp.Common;
using xSLx_Orbwalker;

namespace Ultimate_Carry_Prevolution.Plugin
{
    class Vayne : Champion
    {
        public Vayne()
        {
            SetSpells();
            LoadMenu();
        }

        private void SetSpells()
        {
            Q = new Spell(SpellSlot.Q);
            Q.SetSkillshot(0.3f, 250f, 1250f, false, SkillshotType.SkillshotCircle);
            
            E = new Spell(SpellSlot.E, 550f);
            E.SetTargetted(0.25f, 1600f);

            R = new Spell(SpellSlot.R);
        }

        private void LoadMenu()
        {
            var champMenu = new Menu("Vayne Plugin", "Vayne");
            {
                var comboMenu = new Menu("Combo", "Combo");
                {
                    AddSpelltoMenu(comboMenu, "Q", true);
                    AddSpelltoMenu(comboMenu, "E", true);
                    AddSpelltoMenu(comboMenu, "R", true);
                    champMenu.AddSubMenu(comboMenu);
                }

                var harassMenu = new Menu("Harass", "Harass");
                {
                    AddSpelltoMenu(harassMenu, "Q", true);
                    AddSpelltoMenu(harassMenu, "E", true);
                    AddManaManagertoMenu(harassMenu, 30);
                    champMenu.AddSubMenu(harassMenu);
                }

                var laneClearMenu = new Menu("LaneClear", "LaneClear");
                {
                    AddSpelltoMenu(laneClearMenu, "Q", true);
                    AddManaManagertoMenu(laneClearMenu, 0);
                    champMenu.AddSubMenu(laneClearMenu);
                }

                var miscMenu = new Menu("Misc", "Misc");
                {
                    miscMenu.AddItem(new MenuItem("Misc_useE_Gap_Closer", "Use E On Gap Closer").SetValue(true));
                    miscMenu.AddItem(new MenuItem("Misc_useE_Interrupt", "Use E To Interrupt").SetValue(true));
                    miscMenu.AddItem(new MenuItem("Misc_E_Next", "E Next Auto").SetValue(new KeyBind("E".ToCharArray()[0], KeyBindType.Toggle)));
                    miscMenu.AddItem(new MenuItem("Misc_Push_Distance", "E Push Dist").SetValue(new Slider(400, 400, 500)));
                    champMenu.AddSubMenu(miscMenu);
                }

                var drawMenu = new Menu("Drawing", "Drawing");
                {
                    drawMenu.AddItem(new MenuItem("Draw_Disabled", "Disable All").SetValue(false));
                    drawMenu.AddItem(new MenuItem("Draw_E", "Draw E").SetValue(true));

                    MenuItem drawComboDamageMenu = new MenuItem("Draw_ComboDamage", "Draw Combo Damage").SetValue(true);
                    drawMenu.AddItem(drawComboDamageMenu);
                    Utility.HpBarDamageIndicator.DamageToUnit = GetComboDamage;
                    Utility.HpBarDamageIndicator.Enabled = drawComboDamageMenu.GetValue<bool>();
                    drawComboDamageMenu.ValueChanged +=
                        delegate(object sender, OnValueChangeEventArgs eventArgs)
                        {
                            Utility.HpBarDamageIndicator.Enabled = eventArgs.GetNewValue<bool>();
                        };

                    champMenu.AddSubMenu(drawMenu);
                }
            }

            Menu.AddSubMenu(champMenu);
            Menu.AddToMainMenu();
        }

        public override void OnDraw()
        {
            if (Menu.Item("Draw_Disabled").GetValue<bool>())
            {
                xSLxOrbwalker.DisableDrawing();
                return;
            }
            xSLxOrbwalker.EnableDrawing();

            if (Menu.Item("Draw_E").GetValue<bool>())
                if (E.Level > 0)
                    Utility.DrawCircle(MyHero.Position, E.Range, E.IsReady() ? Color.Green : Color.Red);
        }

        private IEnumerable<SpellSlot> GetSpellCombo()
        {
            var spellCombo = new List<SpellSlot>();
            if (Q.IsReady())
                spellCombo.Add(SpellSlot.Q);
            if (W.IsReady())
                spellCombo.Add(SpellSlot.W);
            if (E.IsReady())
                spellCombo.Add(SpellSlot.E);
            return spellCombo;
        }

        private float GetComboDamage(Obj_AI_Base target)
        {
            double comboDamage = (float)ObjectManager.Player.GetComboDamage(target, GetSpellCombo());
            return (float)(comboDamage + ObjectManager.Player.GetAutoAttackDamage(target));
        }

        public override void OnCombo()
        {
            if (IsSpellActive("E"))
                Cast_E(true);
            if (IsSpellActive("R"))
                Cast_R();
        }

        public override void OnHarass()
        {
            if (IsSpellActive("E"))
                Cast_E(false);
        }

        public override void OnAfterAttack(Obj_AI_Base unit, Obj_AI_Base target)
        {
            if (!unit.IsMe)
                return;

            if(xSLxOrbwalker.CurrentMode == xSLxOrbwalker.Mode.Combo || xSLxOrbwalker.CurrentMode == xSLxOrbwalker.Mode.Harass || xSLxOrbwalker.CurrentMode == xSLxOrbwalker.Mode.LaneClear)
                if(IsSpellActive("Q") && Q.IsReady())
                    Q.Cast(Game.CursorPos);
        }

        public override void OnGapClose(ActiveGapcloser gapcloser)
        {
            if (!Menu.Item("Misc_useE_Gap_Closer").GetValue<bool>()) return;

            if (E.IsReady() && gapcloser.Sender.IsValidTarget(E.Range))
                E.Cast(gapcloser.Sender, UsePackets());
        }

        public override void OnPossibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
        {
            if (spell.DangerLevel < InterruptableDangerLevel.Medium || unit.IsAlly)
                return;

            if (Menu.Item("Misc_useE_Interrupt").GetValue<bool>() && unit.IsValidTarget(E.Range))
                E.Cast(unit, UsePackets());
        }

        private void E_Next_AA(Obj_AI_Hero target)
        {
            if (!E.IsReady() || !target.IsValidTarget(E.Range) || !Menu.Item("Misc_E_Next").GetValue<KeyBind>().Active)
                return;

            E.Cast(target, UsePackets());
            Menu.Item("Misc_E_Next").SetValue(new KeyBind("E".ToCharArray()[0], KeyBindType.Toggle));
        }

        private void Cast_E(bool mode)
        {
            var Push_Distance = Menu.Item("Misc_Push_Distance").GetValue<Slider>().Value;
            var Target = SimpleTs.GetTarget(E.Range, SimpleTs.DamageType.Physical);

            if (MyHero.Distance(Target) < 100)
                E.Cast(Target, UsePackets());

            var Target_Pred = E.GetPrediction(Target);
            var Target_Pred_Pos = V3E(Target_Pred.CastPosition, MyHero.ServerPosition, Push_Distance);

        }

        private void Cast_R()
        {
            var Target = SimpleTs.GetTarget(E.Range, SimpleTs.DamageType.Physical);
            var dmg = GetComboDamage(Target) + MyHero.GetAutoAttackDamage(Target) * 3;

            if (dmg > Target.Health)
                R.Cast();
        }

    }
}
