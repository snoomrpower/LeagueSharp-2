using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace Ultimate_Carry_Prevolution.Plugin
{
	class Amumu : Champion
	{
		public Amumu()
		{
			SetSpells();
			LoadMenu();
		}

		private void SetSpells()
		{
			Q = new Spell(SpellSlot.Q, 1080);
			Q.SetSkillshot((float)0.25, 90, 2000, true, SkillshotType.SkillshotLine);

			W = new Spell(SpellSlot.W, 150);
			W.SetSkillshot((float)0.15, 300, float.MaxValue, false, SkillshotType.SkillshotCircle);

			E = new Spell(SpellSlot.E, 175);
			E.SetSkillshot((float)0.225, 350, float.MaxValue, false, SkillshotType.SkillshotCircle);

			R = new Spell(SpellSlot.R, 275);
			R.SetSkillshot((float)0.30, 550, float.MaxValue, false, SkillshotType.SkillshotCircle); 
		}

		private void LoadMenu()
		{
			var champMenu = new Menu("Amumu Plugin", "Amumu");
			{
				var comboMenu = new Menu("Combo", "Combo");
				{
					AddSpelltoMenu(comboMenu, "W", true);
					AddSpelltoMenu(comboMenu, "E", true);
					comboMenu.AddItem(new MenuItem("Combo_useR_onAmount", "R on Enemys in Range").SetValue(new Slider(2, 5, 0)));
					comboMenu.AddItem(new MenuItem("Combo_useR_onEnemyHealth", "R on Enemy %Health <").SetValue(new Slider(60, 100, 0)));
					champMenu.AddSubMenu(comboMenu);
				}
				var harassMenu = new Menu("Harass", "Harass");
				{
					AddSpelltoMenu(harassMenu, "Q", true);
					AddSpelltoMenu(harassMenu, "Q_Danger", true, "Q inside Dangerzone");
					AddSpelltoMenu(harassMenu, "E", true);
					champMenu.AddSubMenu(harassMenu);
				}
				var laneClearMenu = new Menu("LaneClear", "LaneClear");
				{
					AddSpelltoMenu(laneClearMenu, "Q", true);
					AddSpelltoMenu(laneClearMenu, "E", true);
					champMenu.AddSubMenu(laneClearMenu);
				}
				var fleeMenu = new Menu("Flee", "Flee");
				{
					AddSpelltoMenu(fleeMenu, "Q", true, "Use Q to Mouse");
					AddSpelltoMenu(fleeMenu, "E", true, "Use E to slow Enemy");
					champMenu.AddSubMenu(fleeMenu);
				}
				var miscMenu = new Menu("Misc", "Misc");
				{
					miscMenu.AddItem(new MenuItem("Misc_useW_Autoswitch", "Switch W Automatic").SetValue(true));
					miscMenu.AddItem(new MenuItem("Misc_useW_Autoswitch_health", "Use E to slow Enemy").SetValue(new Slider(60, 100, 0)));
					miscMenu.AddItem(new MenuItem("Misc_useW_Autoswitch_priorityhealth", "Heal Priority farming").SetValue(true));
					champMenu.AddSubMenu(miscMenu);
				}
				var drawMenu = new Menu("Drawing", "Drawing");
				{
					drawMenu.AddItem(new MenuItem("Draw_Disabled", "Disable All").SetValue(false));
					drawMenu.AddItem(new MenuItem("Draw_Q", "Draw Q").SetValue(true));
					drawMenu.AddItem(new MenuItem("Draw_E", "Draw E").SetValue(true));
					drawMenu.AddItem(new MenuItem("Draw_R", "Draw R").SetValue(true));

					var drawComboDamageMenu = new MenuItem("Draw_ComboDamage", "Draw Combo Damage").SetValue(true);
					drawMenu.AddItem(drawComboDamageMenu);
					Utility.HpBarDamageIndicator.DamageToUnit = GetComboDamage;
					Utility.HpBarDamageIndicator.Enabled = drawComboDamageMenu.GetValue<bool>();
					drawComboDamageMenu.ValueChanged += delegate(object sender, OnValueChangeEventArgs eventArgs)
					{
						Utility.HpBarDamageIndicator.Enabled = eventArgs.GetNewValue<bool>();
					};

					champMenu.AddSubMenu(drawMenu);
				}
			}

			Menu.AddSubMenu(champMenu);
			Menu.AddToMainMenu();

		}
	}
}
