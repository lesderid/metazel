using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metazel
{
	public class NESEngine
	{
		private readonly MemoryMap _cpuMemoryMap = new MemoryMap();
		private readonly MemoryMap _ppuMemoryMap = new MemoryMap();

		private byte[] _cpuRAM = new byte[0x800];

		private object _cpuRegisters;
		private object _ppuRegisters;

		public NESCartridge Cartridge { get; private set; }

		public void Load(NESCartridge cartridge)
		{
			//TODO: Change memory map based on mapper type.

			Cartridge = cartridge;

			_cpuMemoryMap.Clear();
			_ppuMemoryMap.Clear();

			_cpuRAM = new byte[_cpuRAM.Length];

			InitialiseCPUMemoryMap();
		}

		private void InitialiseCPUMemoryMap()
		{
			_cpuMemoryMap.Add(0, _cpuRAM.Length, _cpuRAM);
			_cpuMemoryMap.Add(_cpuRAM.Length, _cpuRAM.Length, _cpuRAM);
			_cpuMemoryMap.Add(_cpuRAM.Length * 2, _cpuRAM.Length, _cpuRAM);
			_cpuMemoryMap.Add(_cpuRAM.Length * 3, _cpuRAM.Length, _cpuRAM);

			for (var i = 0x2000; i < 0x4000; i += 8)
				_cpuMemoryMap.Add(i, 8, _ppuRegisters);

			_cpuMemoryMap.Add(0x4000, 0x18, _cpuRegisters);

			_cpuMemoryMap.Add(0x4018, 0x1FE8, null); //Expansion ROM

			_cpuMemoryMap.Add(0x6000, 0x2000, Cartridge.RAMBanks[0]);

			_cpuMemoryMap.Add(0x8000, 0x4000, Cartridge.ROMBanks[0]);
			_cpuMemoryMap.Add(0xC000, 0x4000, Cartridge.ROMBanks[1]);
		}

		public void Run()
		{
			throw new NotImplementedException();
		}
	}
}
