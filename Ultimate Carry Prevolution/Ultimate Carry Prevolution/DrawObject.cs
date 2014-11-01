using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using LeagueSharp.Common;

namespace Ultimate_Carry_Prevolution
{
	class DrawObject
	{
		public string Name;
		public Menu InMenu ;

		public DrawObject (Menu menu,string name)
		{
			Name = name;
			InMenu = menu;
		}

		public bool GetState()
		{
			return Champion.Menu.Item(InMenu.Name + "_" + Name.Replace(" ","_")).GetValue<bool>();
		}
	}
}
