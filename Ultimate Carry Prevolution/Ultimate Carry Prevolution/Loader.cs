using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LeagueSharp;
using LeagueSharp.Common;

namespace Ultimate_Carry_Prevolution
{
	class Loader
	{
		public const double VersionNumber = 0.1;
		public static Champion Champion;

		public Loader()
		{
			Chat.WellCome();
			LoadChampionPlugin();
		}

		private void LoadChampionPlugin()
		{
			try
			{
				// ReSharper disable once AssignNullToNotNullAttribute
				var handle = Activator.CreateInstance(null, "Ultimate_Carry_Prevolution.Plugin." + ObjectManager.Player.ChampionName);
				Champion = (Champion)handle.Unwrap();
			}
			// ReSharper disable once EmptyGeneralCatchClause
			catch(Exception)
			{
			}
		}
	}
}
