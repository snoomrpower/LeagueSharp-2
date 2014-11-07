using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using xSLx_Orbwalker;

namespace Ultimate_Carry_Prevolution.Plugin
{
	class Ashe:Champion
	{
		private bool QActive;
		public Ashe()
		{
			SetSpells();
			LoadMenu();
		}

		private void SetSpells()
		{
			Q = new Spell(SpellSlot.Q);

			W = new Spell(SpellSlot.W, 1200);
			W.SetSkillshot(250, (float)(50 * Math.PI / 180), 900, true, SkillshotType.SkillshotCone);
			
			E = new Spell(SpellSlot.E, 2500);
			E.SetSkillshot(350, 300, 1400, false, SkillshotType.SkillshotLine);

			R = new Spell(SpellSlot.R, 20000);
			R.SetSkillshot(250, 130f, 1600, false, SkillshotType.SkillshotLine);
		}

		private void LoadMenu()
		{
			var champMenu = new Menu("Ahri Plugin", "Ahri");
			{
				var comboMenu = new Menu("Combo", "Combo");
				{
					AddSpelltoMenu(comboMenu, "Q", true);
					AddSpelltoMenu(comboMenu, "W", true);
					AddSpelltoMenu(comboMenu, "E", true);
					comboMenu.AddItem(new MenuItem("Combo_useR_onKillHelp", "Use R to Safe Kill").SetValue(true));
					comboMenu.AddItem(new MenuItem("Combo_useR_onKillHelp", "Use R KS if Out of Range").SetValue(true));
					champMenu.AddSubMenu(comboMenu);
				}
				var harassMenu = new Menu("Harass", "Harass");
				{
					AddSpelltoMenu(harassMenu, "Q", true);
					//AddSpelltoMenu(harassMenu, "E", true);
					//AddManaManagertoMenu(harassMenu, 30);
					//champMenu.AddSubMenu(harassMenu);
				}
				var laneClearMenu = new Menu("LaneClear", "LaneClear");
				{
					//AddSpelltoMenu(laneClearMenu, "Q", true);
					//AddSpelltoMenu(laneClearMenu, "W", true);
					//AddManaManagertoMenu(laneClearMenu, 20);
					//champMenu.AddSubMenu(laneClearMenu);
				}
				var miscMenu = new Menu("Misc", "Misc");
				{
					miscMenu.AddItem(new MenuItem("Misc_useR_Interrupt", "Use R interrupt").SetValue(true));
					miscMenu.AddItem(new MenuItem("Misc_useE_Flash", "Use E on Enemy Flash").SetValue(true));
					champMenu.AddSubMenu(miscMenu);
				}
				var drawMenu = new Menu("Drawing", "Drawing");
				{
					drawMenu.AddItem(new MenuItem("Draw_Disabled", "Disable All").SetValue(false));
					drawMenu.AddItem(new MenuItem("Draw_W", "Draw W").SetValue(true));
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
		IEnumerable<SpellSlot> GetSpellCombo()
		{
			var spellCombo = new List<SpellSlot>();
			if(W.IsReady())
				spellCombo.Add(SpellSlot.W);
			if(R.IsReady())
				spellCombo.Add(SpellSlot.R);
			return spellCombo;
		}

		private float GetComboDamage(Obj_AI_Base target)
		{
			double comboDamage = (float)MyHero.GetComboDamage(target, GetSpellCombo());
			return (float)(comboDamage + MyHero.GetAutoAttackDamage(target) * (R.IsReady() ? 3 : 1));
		}

		public override void OnDraw()
		{
			if(Menu.Item("Draw_Disabled").GetValue<bool>())
			{
				xSLxOrbwalker.DisableDrawing();
				return;
			}
			xSLxOrbwalker.EnableDrawing();

			if(Menu.Item("Draw_W").GetValue<bool>())
				if(W.Level > 0)
					Utility.DrawCircle(MyHero.Position, W.Range - 2, W.IsReady() ? Color.Green : Color.Red);

			if(Menu.Item("Draw_E").GetValue<bool>())
				if(E.Level > 0)
					Utility.DrawCircle(MyHero.Position, E.Range, E.IsReady() ? Color.Green : Color.Red);

			if(Menu.Item("Draw_R").GetValue<bool>())
				if(R.Level > 0)
					Utility.DrawCircle(MyHero.Position, R.Range, R.IsReady() ? Color.Green : Color.Red);

		}
	
		public override void OnPassive()
		{
			SetERange();
			if(xSLxOrbwalker.CurrentMode != xSLxOrbwalker.Mode.Combo && xSLxOrbwalker.CurrentMode != xSLxOrbwalker.Mode.Harass)
			{
				if(QActive)
				{
					Q.Cast();
					QActive = false;
				}
			}
			if(xSLxOrbwalker.CurrentMode != xSLxOrbwalker.Mode.Harass)
				return;
			if(xSLxOrbwalker.GetPossibleTarget().IsMinion  && QActive)
			{
				Q.Cast();
				QActive = false;
			}
		}

		public override void OnPossibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
		{
			if (R.IsReady() && Menu.Item("Misc_useR_Interrupt").GetValue<bool>() && unit.IsValidTarget(R.Range))
				R.Cast(unit, UsePackets());
		}

		public override void OnProcessSpell(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs spell)
		{
			if(!Menu.Item("Misc_useE_Flash").GetValue<bool>() || unit.Team == MyHero.Team)
				return;
			if(spell.SData.Name.ToLower() == "summonerflash" && spell.End.Distance(MyHero.Position) <= E.Range + E.Width / 2)
				E.Cast(spell.End, UsePackets());
		}

		public override void OnCombo()
		{
			
		}

		private void SetERange()
		{
			var range = 1750 + (E.Level*750);
			E.Range = range;
		}
		
		public override void OnSendPacket(GamePacketEventArgs args)
		{
			if(!IsSpellActive("Q") || !ManaManagerAllowCast())
				return;
			if(args.PacketData[0] != Packet.C2S.Move.Header ||
				Packet.C2S.Move.Decoded(args.PacketData).SourceNetworkId != MyHero.NetworkId ||
				Packet.C2S.Move.Decoded(args.PacketData).MoveType != 3)
				return;
			if(!QActive)
				QActive = MyHero.Buffs.Any(buff => buff.Name == "FrostShot");
			var heroFound = AllHerosEnemy.Any(hero => hero.NetworkId == Packet.C2S.Move.Decoded(args.PacketData).TargetNetworkId);
			if(heroFound)
			{
				if(!QActive)
					Q.Cast();
				QActive = true;
			}
			else
			{
				if(QActive)
					Q.Cast();
				QActive = false;
			}
		}
	}
}
