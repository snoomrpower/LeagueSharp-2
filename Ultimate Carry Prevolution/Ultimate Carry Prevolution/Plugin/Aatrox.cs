using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp.Common;

namespace Ultimate_Carry_Prevolution.Plugin
{
	class Aatrox : Champion
	{
		public Aatrox()
		{
			LoadMenu();
		}

		private void LoadMenu()
		{
			var champMenu = new Menu("Aatrox Plugin", "Aatrox");
			{
				var comboMenu = new Menu("Combo", "Combo");
				comboMenu.AddItem(new MenuItem("useQ", "Use Q").SetValue(true));
				champMenu.AddSubMenu(comboMenu);
			}

			Menu.AddSubMenu(champMenu);
			Menu.AddToMainMenu();

		}
	}
}
