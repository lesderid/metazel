using System.Data;
using Metazel.Library;

namespace Metazel.NES
{
	internal class OAMDMA : IMemoryProvider
	{
		private readonly NESPPU _ppu;
		private readonly MemoryMap _memory;

		public OAMDMA(NESPPU ppu, NESCPU cpu)
		{
			_ppu = ppu;
			_memory = cpu.Memory;
		}

		public byte this[int address]
		{
			get { throw new ReadOnlyException(); }
			set
			{
				for (var i = value << 8; i < (value << 8) + 255; i++)
					_ppu.OAMData[(_ppu.Registers.OAMAddress + i - (value << 8)) % 256] = _memory[i];
			}
		}
	}
}