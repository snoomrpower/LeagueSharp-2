using System;

namespace Ultimate_Carry_Prevolution
{
	class Program
	{
		// ReSharper disable once UnusedParameter.Local
		static void Main(string[] args)
		{
			Events.Game.OnGameStart += OnGameStart;
		}

		private static void OnGameStart(EventArgs args)
		{
			LoadUC();
		}

		private static void LoadUC()
		{
			// ReSharper disable once ObjectCreationAsStatement
			new Loader();
		}
	}
}
