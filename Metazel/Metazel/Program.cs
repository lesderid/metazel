using System.Threading;

namespace Metazel
{
	internal static class Program
	{
		private static void Main()
		{
			var cartridge = new NESCartridge("official_only.nes");

			var thread = new Thread(() => { });
			thread.Priority = ThreadPriority.Highest;
			

			var engine = new NESEngine();
			engine.Load(cartridge);
			engine.Run();
		}
	}
}