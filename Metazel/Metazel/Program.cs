using System;
using System.Windows.Forms;

namespace Metazel.NES
{
	static class Program
	{
		static void Main()
		{
			Console.BufferHeight = 32766;

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new MainForm());
		}
	}
}
