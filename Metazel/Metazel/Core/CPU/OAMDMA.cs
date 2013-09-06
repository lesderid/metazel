using System.Data;

namespace Metazel
{
	internal class OAMDMA : IMemoryProvider
	{
		private readonly PPURegisters _registers;
		private readonly MemoryMap _memory;

		public OAMDMA(NESPPU ppu, NESCPU cpu)
		{
			_registers = ppu.Registers;
			_memory = cpu.Memory;
		}


		public byte this[int address]
		{
			get { throw new ReadOnlyException(); }
			set
			{
				//Console.WriteLine("Address: ${0:X2}00", address);

				for (var i = value << 8; i < (value << 8) + 255; i++)
					_registers[4] = _memory[i];
			}
		}
	}
}