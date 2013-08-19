using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metazel
{
	public class NESEngine
	{
		public readonly MemoryMap CPUMemoryMap = new MemoryMap();
		public readonly MemoryMap PPUMemoryMap = new MemoryMap();

		private byte[] _cpuRAM = new byte[0x800];

		private object _cpuRegisters;
		private object _ppuRegisters;

		public NESCartridge Cartridge { get; private set; }

		private byte[] _nametableA = new byte[0x400];
		private byte[] _nametableB = new byte[0x400];

		private byte[] _ppuPaletteData = new byte[0x20];

		private NESCPU _cpu;
		private NESPPU _ppu;

		public void Load(NESCartridge cartridge)
		{
			//TODO: Change memory map based on mapper type.

			Cartridge = cartridge;

			CPUMemoryMap.Clear();
			PPUMemoryMap.Clear();

			_cpuRAM = new byte[_cpuRAM.Length];

			_nametableA = new byte[_nametableA.Length];
			_nametableB = new byte[_nametableB.Length];

			_ppuPaletteData = new byte[_ppuPaletteData.Length];

			InitialiseCPUMemoryMap();
			InitialisePPUMemoryMap();
		}

		private void InitialisePPUMemoryMap()
		{
			if (Cartridge.VROMBanks.Length > 0)
			{
				PPUMemoryMap.Add(0, 0x1000, Cartridge.VROMBanks[0]);
				PPUMemoryMap.Add(0x1000, 0x1000, new IndexedByteArray(Cartridge.VROMBanks[0], 0x1000));
			}

			switch (Cartridge.VRAMLayout)
			{
				case VRAMLayout.VerticalLayout:
					PPUMemoryMap.Add(0x2000, _nametableA.Length, _nametableA);
					PPUMemoryMap.Add(0x2400, _nametableA.Length, _nametableA);
					PPUMemoryMap.Add(0x2800, _nametableB.Length, _nametableB);
					PPUMemoryMap.Add(0x2C00, _nametableB.Length, _nametableB);

					PPUMemoryMap.Add(0x3000, _nametableA.Length, _nametableA);
					PPUMemoryMap.Add(0x3400, _nametableA.Length, _nametableA);
					PPUMemoryMap.Add(0x3800, _nametableB.Length, _nametableB);
					PPUMemoryMap.Add(0x3C00, _nametableB.Length - 0x100, _nametableB);
					break;
				case VRAMLayout.HorizontalLayout:
					PPUMemoryMap.Add(0x2000, _nametableA.Length, _nametableA);
					PPUMemoryMap.Add(0x2400, _nametableB.Length, _nametableB);
					PPUMemoryMap.Add(0x2800, _nametableA.Length, _nametableA);
					PPUMemoryMap.Add(0x2C00, _nametableB.Length, _nametableB);

					PPUMemoryMap.Add(0x3000, _nametableA.Length, _nametableA);
					PPUMemoryMap.Add(0x3400, _nametableB.Length, _nametableB);
					PPUMemoryMap.Add(0x3800, _nametableA.Length, _nametableA);
					PPUMemoryMap.Add(0x3C00, _nametableB.Length - 0x100, _nametableB);
					break;
				default:
					throw new NotImplementedException();
			}

			PPUMemoryMap.Add(0x3F00, _ppuPaletteData.Length, _ppuPaletteData);

			PPUMemoryMap.Add(0x4000, 0x4000, new MemoryMirror(PPUMemoryMap, 0, 0x4000));
			PPUMemoryMap.Add(0x8000, 0x4000, new MemoryMirror(PPUMemoryMap, 0, 0x4000));
			PPUMemoryMap.Add(0xC000, 0x4000, new MemoryMirror(PPUMemoryMap, 0, 0x4000));
		}

		private void InitialiseCPUMemoryMap()
		{
			CPUMemoryMap.Add(0, _cpuRAM.Length, _cpuRAM);
			CPUMemoryMap.Add(_cpuRAM.Length, _cpuRAM.Length, _cpuRAM);
			CPUMemoryMap.Add(_cpuRAM.Length * 2, _cpuRAM.Length, _cpuRAM);
			CPUMemoryMap.Add(_cpuRAM.Length * 3, _cpuRAM.Length, _cpuRAM);

			for (var i = 0x2000; i < 0x4000; i += 8)
				CPUMemoryMap.Add(i, 8, _ppuRegisters);

			CPUMemoryMap.Add(0x4000, 24, _cpuRegisters);

			CPUMemoryMap.Add(0x4018, 0x1FE8, null); //Expansion ROM

			CPUMemoryMap.Add(0x6000, 0x2000, Cartridge.RAMBanks[0]);

			CPUMemoryMap.Add(0x8000, 0x4000, Cartridge.ROMBanks[0]);
			CPUMemoryMap.Add(0xC000, 0x4000, Cartridge.ROMBanks.Length == 1 ? Cartridge.ROMBanks[0] : Cartridge.ROMBanks[1]);

			CPUMemoryMap.PopulateTuplesList();
		}

		public void Run()
		{
			var path = Cartridge.Name + ".log";
			if (File.Exists(path))
				File.Delete(path);

			_cpu = new NESCPU(this);
			_ppu = new NESPPU(this);

			var i = 1;

			var previousTicks = Environment.TickCount;

			while (true)
			{
				switch (i)
				{
					case 1:
						_ppu.DoCycle();

						i = 2;
						break;
					case 2:
						_cpu.DoCycle();

						if (_cpu.TotalCycleCount % 1789772 == 0)
						{
							Console.WriteLine(Environment.TickCount - previousTicks);

							previousTicks = Environment.TickCount;
						}

						_ppu.DoCycle();

						i = 3;
						break;
					case 3:
						_ppu.DoCycle();

						i = 1;
						break;
				}
			}
		}
	}
}
