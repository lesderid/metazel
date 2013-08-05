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

		private byte[] _nametableA = new byte[0x400];
		private byte[] _nametableB = new byte[0x400];

		private byte[] _ppuPaletteData = new byte[0x20];

		public void Load(NESCartridge cartridge)
		{
			//TODO: Change memory map based on mapper type.

			Cartridge = cartridge;

			_cpuMemoryMap.Clear();
			_ppuMemoryMap.Clear();

			_cpuRAM = new byte[_cpuRAM.Length];
			
			_nametableA = new byte[_nametableA.Length];
			_nametableB = new byte[_nametableB.Length];

			_ppuPaletteData = new byte[_ppuPaletteData.Length];

			InitialiseCPUMemoryMap();
			InitialisePPUMemoryMap();
		}

		private void InitialisePPUMemoryMap()
		{
			_ppuMemoryMap.Add(0, 0x1000, Cartridge.VROMBanks[0]);
			_ppuMemoryMap.Add(0x1000, 0x1000, new IndexedByteArray(Cartridge.VROMBanks[0], 0x1000));

			switch (Cartridge.VRAMLayout)
			{
				case VRAMLayout.VerticalLayout:
					_ppuMemoryMap.Add(0x2000, _nametableA.Length, _nametableA);
					_ppuMemoryMap.Add(0x2400, _nametableA.Length, _nametableA);
					_ppuMemoryMap.Add(0x2800, _nametableB.Length, _nametableB);
					_ppuMemoryMap.Add(0x2C00, _nametableB.Length, _nametableB);
		
					_ppuMemoryMap.Add(0x3000, _nametableA.Length, _nametableA);
					_ppuMemoryMap.Add(0x3400, _nametableA.Length, _nametableA);
					_ppuMemoryMap.Add(0x3800, _nametableB.Length, _nametableB);
					_ppuMemoryMap.Add(0x3C00, _nametableB.Length - 0x100, _nametableB);
					break;
				case VRAMLayout.HorizontalLayout:
					_ppuMemoryMap.Add(0x2000, _nametableA.Length, _nametableA);
					_ppuMemoryMap.Add(0x2400, _nametableB.Length, _nametableB);
					_ppuMemoryMap.Add(0x2800, _nametableA.Length, _nametableA);
					_ppuMemoryMap.Add(0x2C00, _nametableB.Length, _nametableB);

					_ppuMemoryMap.Add(0x3000, _nametableA.Length, _nametableA);
					_ppuMemoryMap.Add(0x3400, _nametableB.Length, _nametableB);
					_ppuMemoryMap.Add(0x3800, _nametableA.Length, _nametableA);
					_ppuMemoryMap.Add(0x3C00, _nametableB.Length - 0x100, _nametableB);
					break;
				default:
					throw new NotImplementedException();
			}

			_ppuMemoryMap.Add(0x3F00, _ppuPaletteData.Length, _ppuPaletteData);

			_ppuMemoryMap.Add(0x4000, 0x4000, new MemoryMirror(_ppuMemoryMap, 0, 0x4000));
			_ppuMemoryMap.Add(0x8000, 0x4000, new MemoryMirror(_ppuMemoryMap, 0, 0x4000));
			_ppuMemoryMap.Add(0xC000, 0x4000, new MemoryMirror(_ppuMemoryMap, 0, 0x4000));
		}

		private void InitialiseCPUMemoryMap()
		{
			_cpuMemoryMap.Add(0, _cpuRAM.Length, _cpuRAM);
			_cpuMemoryMap.Add(_cpuRAM.Length, _cpuRAM.Length, _cpuRAM);
			_cpuMemoryMap.Add(_cpuRAM.Length * 2, _cpuRAM.Length, _cpuRAM);
			_cpuMemoryMap.Add(_cpuRAM.Length * 3, _cpuRAM.Length, _cpuRAM);

			for (var i = 0x2000; i < 0x4000; i += 8)
				_cpuMemoryMap.Add(i, 8, _ppuRegisters);

			_cpuMemoryMap.Add(0x4000, 24, _cpuRegisters);

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
