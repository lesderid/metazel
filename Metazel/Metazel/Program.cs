namespace Metazel
{
	internal static class Program
	{
		private static void Main()
		{
			var cartridge = new NESCartridge("Super Mario Bros. (JU) [!].nes");

			var engine = new NESEngine();
			engine.Load(cartridge);
			engine.Run();
		}
	}
}