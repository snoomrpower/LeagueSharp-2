using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LeagueSharp;
using LeagueSharp.Common;
using LX_Orbwalker;

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
		public LXOrbwalker Orbwalker;

		public Champion()
		{
			LoadBasics();
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
			Menu.SubMenu("Packets").AddItem(new MenuItem("usePackets", "Use Packets").SetValue(true));
			//Orbwalker get changed to xSLxOrbwalker Soon
			var orbwalkerMenu = new Menu("LX Orbwalker", "LX_Orbwalker");
			LXOrbwalker.AddToMenu(orbwalkerMenu);
			Menu.AddSubMenu(orbwalkerMenu);

		}

	}
}
