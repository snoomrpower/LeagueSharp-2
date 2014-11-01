using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Linq;
using System.Text;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using xSLx_Orbwalker;

namespace Ultimate_Carry_Prevolution
{
	class Champion
	{
		public Obj_AI_Hero MyHero = ObjectManager.Player;
		public  IEnumerable<Obj_AI_Hero> AllHeros = ObjectManager.Get<Obj_AI_Hero>();
		public  IEnumerable<Obj_AI_Hero> AllHerosFriend = ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsAlly);
		public  IEnumerable<Obj_AI_Hero> AllHerosEnemy = ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsEnemy);
	
		public Spell Q;
		public Spell W;
		public Spell E;
		public Spell R;

		public static Menu Menu;

		private List<DrawObject> _drawObjectlist = new List<DrawObject>();
		public Champion()
		{
			LoadBasics();

			Game.OnGameUpdate += OnGameUpdate;
			Game.OnGameUpdate += OnGameUpdateModes;
			Drawing.OnDraw += OnDraw;
		}

		private void OnGameUpdateModes(EventArgs args)
		{

			switch (xSLxOrbwalker.CurrentMode)
			{
				case xSLxOrbwalker.Mode.Combo:
					OnCombo();
					break;
				case xSLxOrbwalker.Mode.Harass :
					OnHarass();
					break;
				case xSLxOrbwalker.Mode.LaneClear :
					OnLaneClear();
					break;
				case xSLxOrbwalker.Mode.LaneFreeze :
					OnLaneFreeze();
					break;
				case xSLxOrbwalker.Mode.Lasthit :
					OnLasthit();
					break;
				case xSLxOrbwalker.Mode.Flee :
					OnFlee();
					break;
				case xSLxOrbwalker.Mode.None:
					OnStandby();
					break;
			}
			OnPassive();
		}

		private void LoadBasics()
		{
			Menu = new Menu("UC-Prevolution", MyHero.ChampionName + "_UCP", true);
			
			//the Team
			Menu.AddSubMenu(new Menu("UC-Team", "Credits"));
			Menu.SubMenu("Credits").AddItem(new MenuItem("Lexxes", "Lexxes (Austria)"));
			Menu.SubMenu("Credits").AddItem(new MenuItem("xSalice", "xSalice (US)"));
			
			//PacketMenu
			Menu.AddSubMenu(new Menu("Packet Setting", "Packets"));
			Menu.SubMenu("Packets").AddItem(new MenuItem("usePackets", "Use Packets").SetValue(false));
			
			//Orbwalker get changed to xSLxOrbwalker Soon
			var orbwalkerMenu = new Menu("LX Orbwalker", "LX_Orbwalker");
			xSLxOrbwalker.AddToMenu(orbwalkerMenu);
			Menu.AddSubMenu(orbwalkerMenu);
		}

		public bool UsePackets()
		{
			return Menu.Item("usePackets").GetValue<bool>();
		}

		public Obj_AI_Hero Cast_BasicSkillshot_Enemy(Spell spell, SimpleTs.DamageType prio = SimpleTs.DamageType.True, float extrarange = 0)
		{
			if(!spell.IsReady())
				return null;
			var target = SimpleTs.GetTarget(spell.Range + extrarange, prio);
			if(target == null)
				return null;
			if(!target.IsValidTarget(spell.Range + extrarange) || spell.GetPrediction(target).Hitchance < HitChance.High)
				return null;
			spell.UpdateSourcePosition();
			spell.Cast(target, UsePackets());
			return target;
		}
	
		public void Cast_BasicSkillshot_AOE_Farm(Spell spell, int extrawidth = 0)
		{
			if(!spell.IsReady())
				return;
			var minions = MinionManager.GetMinions(MyHero.ServerPosition, spell.Type == SkillshotType.SkillshotLine ? spell.Range : spell.Range + ((spell.Width + extrawidth) / 2), MinionTypes.All, MinionTeam.NotAlly);
			if(minions.Count == 0)
				return;
			MinionManager.FarmLocation castPostion;

			switch (spell.Type)
			{
				case SkillshotType.SkillshotCircle:
					castPostion = MinionManager.GetBestCircularFarmLocation(minions.Select(minion => minion.ServerPosition.To2D()).ToList(), spell.Width + extrawidth, spell.Range);
					break;
				case SkillshotType.SkillshotLine:
					castPostion = MinionManager.GetBestLineFarmLocation(
						minions.Select(minion => minion.ServerPosition.To2D()).ToList(), spell.Width, spell.Range);
					break;
				default:
					return;
			}
			spell.UpdateSourcePosition();
			spell.Cast(castPostion.Position, UsePackets());
		}

		public static bool IsInsideEnemyTower(Vector3 position)
		{
			return ObjectManager.Get<Obj_AI_Turret>()
									.Any(tower => tower.IsEnemy && tower.Health > 0 && tower.Position.Distance(position) < 775);
		}

		public void AddSpelltoMenu(Menu menu, string name, bool state,string alternativename = "")
		{
			if (alternativename == "")
				alternativename = "Use " + menu;
			menu.AddItem(new MenuItem(menu.Name + "_" + name.Replace(" ", "_"), alternativename).SetValue(state));
		}

		public bool IsSpellActive(string name)
		{
			try
			{
				return Menu.Item(xSLxOrbwalker.CurrentMode + "_" + name.Replace(" ", "_")).GetValue<bool>();
			}
			catch
			{
				return false;
			}
		}

		public string GetSpellName(SpellSlot slot, Obj_AI_Hero unit = null)
		{
			return unit != null ? unit.Spellbook.GetSpell(slot).Name : MyHero.Spellbook.GetSpell(slot).Name;
		}

		public bool EnemysinRange(float range, int min = 1, Obj_AI_Hero unit = null)
		{
			if(unit == null)
				unit = MyHero;
			return min <= AllHerosEnemy.Count(hero => hero.Distance(unit) < range && hero.IsValidTarget());
		}
		public bool EnemysinRange(float range, int min, Vector3 pos)
		{
			return min <= AllHerosEnemy.Count(hero => hero.Position.Distance(pos) < range && hero.IsValidTarget() && !hero.IsDead);
		}

		public virtual void OnDraw(EventArgs args)
		{
			OnDraw();
		}
		public virtual void OnDraw()
		{
			// Virtual OnDraw
		}
		public virtual void OnStandby()
		{
			// Virtual OnStandby
		}

		public virtual void OnFlee()
		{
			// Virtual OnFlee
		}

		public virtual void OnLasthit()
		{
			// Virtual OnLasthit
		}

		public virtual void OnLaneFreeze()
		{
			// Virtual OnLaneFreeze
		}

		public virtual void OnLaneClear()
		{
			// Virtual OnLaneClear
		}

		public virtual void OnHarass()
		{
			// Virtual OnHarass
		}

		public virtual void OnCombo()
		{
			// Virtual OnCombo
		}

		public virtual void OnGameUpdate(EventArgs args)
		{
			// Virtual OnGameUpdate
		}
		public virtual void OnPassive()
		{
			// Virtual OnPassive
		}

	}
}
