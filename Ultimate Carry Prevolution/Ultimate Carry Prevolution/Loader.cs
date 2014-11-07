using System;
using LeagueSharp;

namespace Ultimate_Carry_Prevolution
{
	class Loader
	{
		public const double VersionNumber = 0.2;

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
				var handle = Activator.CreateInstance(null, "Plugin." + ObjectManager.Player.ChampionName);
			}
			// ReSharper disable once EmptyGeneralCatchClause
			catch(Exception)
			{
			}
		}
	}
}
