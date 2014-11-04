using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using xSLx_Orbwalker;
using Color = System.Drawing.Color;

namespace Ultimate_Carry_Prevolution.Plugin
{
	class Akali : Champion
	{
		private Obj_AI_Hero _killableTarget;
		private float _assignTime;

		public Akali()
		{
			SetSpells();
			LoadMenu();
		}

		private void SetSpells()
		{
			Q = new Spell(SpellSlot.Q, 600);

			W = new Spell(SpellSlot.W, 700);

			E = new Spell(SpellSlot.E, 325);

			R = new Spell(SpellSlot.R, 800);
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
					AddSpelltoMenu(comboMenu, "R", true);
					champMenu.AddSubMenu(comboMenu);
				}
				var harassMenu = new Menu("Harass", "Harass");
				{
					AddSpelltoMenu(harassMenu, "Q", true);
					AddSpelltoMenu(harassMenu, "E", true);
					champMenu.AddSubMenu(harassMenu);
				}
				var laneClearMenu = new Menu("LaneClear", "LaneClear");
				{
					AddSpelltoMenu(laneClearMenu, "Q", true);
					AddSpelltoMenu(laneClearMenu, "E", true);
					laneClearMenu.AddItem(new MenuItem("LaneClear_useE_minHit", "Use E if min. hit").SetValue(new Slider(2, 1, 6)));
					champMenu.AddSubMenu(laneClearMenu);
				}
				
				var miscMenu = new Menu("Misc", "Misc");
				{
					miscMenu.AddItem(new MenuItem("Misc_useW_enemyCount", "Use W if x Enemys Arround")).SetValue(new Slider(1, 1, 5));
					miscMenu.AddItem(new MenuItem("Misc_useW_Health", "Use W if health below").SetValue(new Slider(25)));
					champMenu.AddSubMenu(miscMenu);
				}
				var drawMenu = new Menu("Drawing", "Drawing");
				{
					drawMenu.AddItem(new MenuItem("Draw_Disabled", "Disable All").SetValue(false));
					drawMenu.AddItem(new MenuItem("Draw_Q", "Draw Q").SetValue(true));
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

		private float GetComboDamage(Obj_AI_Base target)
		{
			var rStacks = GetRStacks();
			var jumpCount = (rStacks - (int)(target.Distance(MyHero.Position) / R.Range));
			var comboDamage = 0d;
			if(Q.IsReady())
				comboDamage += MyHero.GetSpellDamage(target, SpellSlot.Q) + MyHero.CalcDamage(target, Damage.DamageType.Magical, (45 + 35 * Q.Level + 0.5 * MyHero.FlatMagicDamageMod));
			if(E.IsReady())
				comboDamage += MyHero.GetSpellDamage(target, SpellSlot.E);

			if(HasBuff(target, "AkaliMota"))
				comboDamage += MyHero.CalcDamage(target, Damage.DamageType.Magical, (45 + 35 * Q.Level + 0.5 * MyHero.FlatMagicDamageMod));

			comboDamage += MyHero.CalcDamage(target, Damage.DamageType.Magical, CalcPassiveDmg());

			if(rStacks > 0)
				comboDamage += jumpCount > 0 ? MyHero.GetSpellDamage(target, SpellSlot.R) * jumpCount : MyHero.GetSpellDamage(target, SpellSlot.R);
			// Items comes soon

			return (float)(comboDamage + ObjectManager.Player.GetAutoAttackDamage(target));
		}

		public override void OnDraw()
		{
			if(Menu.Item("Draw_Disabled").GetValue<bool>())
			{
				xSLxOrbwalker.DisableDrawing();
				return;
			}
			xSLxOrbwalker.EnableDrawing();

			if(Menu.Item("Draw_Q").GetValue<bool>())
				if(Q.Level > 0)
					Utility.DrawCircle(MyHero.Position, Q.Range, Q.IsReady() ? Color.Green : Color.Red);

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

		//public override void OnPassive()
		//{
		//	var possibleTarget = SimpleTs.GetTarget(R.Range * 2 + xSLxOrbwalker.GetAutoAttackRange(), SimpleTs.DamageType.Magical);

		//	if(_killableTarget.IsDead || Game.Time - _assignTime > 1.5)
		//		_killableTarget = null;

		//	if(_killableTarget == default(Obj_AI_Hero) && GetComboDamage(possibleTarget) > possibleTarget.Health)
		//	{
		//		_killableTarget = possibleTarget;
		//		_assignTime = Game.Time;
		//	}

		//	if(_killableTarget != null)
		//	{
		//		if(MyHero.Distance(_killableTarget) < R.Range * 2 + xSLxOrbwalker.GetAutoAttackRange() && MyHero.Distance(_killableTarget) > Q.Range)
		//			Cast_R(_killableTarget.Position);
		//		else if(MyHero.Distance(_killableTarget) < Q.Range)
		//			KillCombo(_killableTarget);
		//		else
		//			_killableTarget = null;
		//	}
		//}
		public override void OnCombo()
		{
			if (_killableTarget == null)
			{
			
				if(IsSpellActive("Q"))
					Cast_Q(true);
				if(IsSpellActive("E"))
					Cast_E(true);
				if(IsSpellActive("W"))
					Cast_W();
				if(!IsSpellActive("R"))
					return;
				var target = SimpleTs.GetTarget(R.Range, SimpleTs.DamageType.Magical);
				if((target.IsValidTarget(R.Range) && !xSLxOrbwalker.InAutoAttackRange(target)) || R.IsKillable(target))
					R.Cast(target, UsePackets());
			}
		}

		public override void OnHarass()
		{
			if(IsSpellActive("Q"))
				Cast_Q(true);
			if(IsSpellActive("E"))
				Cast_E(true);
		}

		public override void OnLaneClear()
		{
			if(IsSpellActive("Q"))
				Cast_Q(false);
			if(IsSpellActive("E"))
				Cast_E(false);
		}

		private void KillCombo(Obj_AI_Base target)
		{
			xSLxOrbwalker.SetAttack(!Q.IsReady() && !E.IsReady() && MyHero.Distance(target) < 800f);
			xSLxOrbwalker.ForcedTarget = target;

			if(Q.IsReady() && Q.InRange(target.Position) && !HasBuff(target, "AkaliMota"))
				Q.Cast(target, UsePackets());
			if(E.IsReady() && E.InRange(target.Position))
				E.Cast();
			if(W.IsReady() && W.InRange(target.Position) && !(HasBuff(target, "AkaliMota") && MyHero.Distance(target) > Orbwalking.GetRealAutoAttackRange(MyHero)))
				W.Cast(V2E(MyHero.Position, target.Position, MyHero.Distance(target) + W.Width * 2 - 20), UsePackets());
			if(R.IsReady() && R.InRange(target.Position))
				R.Cast(target, UsePackets());
		}

		private void Cast_Q(bool mode)
		{
			if(!Q.IsReady())
				return;
			if(mode)
			{
				var target = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Magical);
				if(!target.IsValidTarget(Q.Range))
					return;
				Q.Cast(target,UsePackets());
			}
			else
			{
				foreach(var minion in MinionManager.GetMinions(MyHero.Position, Q.Range).Where(minion => HasBuff(minion, "AkaliMota") && xSLxOrbwalker.InAutoAttackRange(minion)))
					xSLxOrbwalker.ForcedTarget = minion;

				foreach(var minion in MinionManager.GetMinions(MyHero.Position, Q.Range).Where(minion => HealthPrediction.GetHealthPrediction(minion,
						(int)(E.Delay + (minion.Distance(MyHero) / E.Speed)) * 1000) <
															 MyHero.GetSpellDamage(minion, SpellSlot.Q) &&
															 HealthPrediction.GetHealthPrediction(minion,
																 (int)(E.Delay + (minion.Distance(MyHero) / E.Speed)) * 1000) > 0 &&
															 xSLxOrbwalker.InAutoAttackRange(minion)))
					Q.Cast(minion);

				foreach(var minion in MinionManager.GetMinions(MyHero.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth).Where(minion => MyHero.Distance(minion) <= Q.Range))
					Q.Cast(minion);
			}
		}

		private void Cast_W()
		{
			if(Menu.Item("Misc_useW_enemyCount").GetValue<Slider>().Value > Utility.CountEnemysInRange(400) &&
				Menu.Item("Misc_useW_Health").GetValue<Slider>().Value < (int)(MyHero.Health / MyHero.MaxHealth * 100))
				return;
			W.Cast(MyHero.Position, UsePackets());
		}

		private void Cast_E(bool mode)
		{
			if(!E.IsReady())
				return;
			if(mode)
			{
				var target = SimpleTs.GetTarget(E.Range, SimpleTs.DamageType.Magical);
				if(target == null || !target.IsValidTarget(E.Range))
					return;
				if(HasBuff(target, "AkaliMota") && !E.IsReady() && xSLxOrbwalker.InAutoAttackRange(target))
					xSLxOrbwalker.ForcedTarget = target;
				else
					E.Cast();
			}
			else
			{
				if(MinionManager.GetMinions(MyHero.Position, E.Range).Count >= Menu.Item("LaneClear_useE_minHit").GetValue<Slider>().Value)
					E.Cast();
				foreach(var minion in MinionManager.GetMinions(MyHero.ServerPosition, Q.Range, MinionTypes.All,
					MinionTeam.Neutral, MinionOrderTypes.MaxHealth).Where(minion => MyHero.Distance(minion) <= E.Range))
					E.Cast();
			}
		}

		private void Cast_R(Vector3 position, bool mouseJump = false)
		{
			Obj_AI_Base[] target = { null };
			foreach(var minion in ObjectManager.Get<Obj_AI_Base>().Where(minion => minion.IsValidTarget(R.Range) && MyHero.Distance(position, true) > minion.Distance(position, true) && minion.Distance(position, true) < target[0].Distance(position, true)))
				if(mouseJump)
				{
					if(minion.Distance(position) < 200)
						target[0] = minion;
				}
				else
					target[0] = minion;
			if(target[0] == null || (!R.IsReady() || !R.InRange(target[0].Position) || target[0].IsMe))
				return;
			if(mouseJump && target[0].Distance(position) < 200)
				R.CastOnUnit(target[0], UsePackets());
			else if(MyHero.Distance(position, true) > target[0].Distance(position, true))
				R.CastOnUnit(target[0], UsePackets());
		}

		private double CalcPassiveDmg()
		{
			return (0.06 + 0.01 * (MyHero.FlatMagicDamageMod / 6)) * (MyHero.FlatPhysicalDamageMod + MyHero.BaseAttackDamage);
		}

		private int GetRStacks()
		{
			return (from buff in MyHero.Buffs
					where buff.Name == "AkaliShadowDance"
					select buff.Count).FirstOrDefault();
		}
	}
}
