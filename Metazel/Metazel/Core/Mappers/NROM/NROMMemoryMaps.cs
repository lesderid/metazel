using System;

namespace Metazel
{
	public partial class NESEngine 
	{
		public void InitialiseNROMPPUMemoryMap()
		{
			if (Cartridge.VROMBanks.Length > 0)
			{
				PPUMemoryMap.Add(0, 0x2000, Cartridge.VROMBanks[0]);
			}
			else
			{
				_ppuRAM = new byte[0x2000];

				PPUMemoryMap.Add(0, 0x2000, _ppuRAM);
			}

			switch (Cartridge.NametableLayout)
			{
				case NametableLayout.VerticalLayout:
					PPUMemoryMap.Add(0x2000, NameTableA.Length, NameTableA);
					PPUMemoryMap.Add(0x2400, NameTableA.Length, NameTableA);
					PPUMemoryMap.Add(0x2800, NameTableB.Length, NameTableB);
					PPUMemoryMap.Add(0x2C00, NameTableB.Length, NameTableB);

					PPUMemoryMap.Add(0x3000, NameTableA.Length, NameTableA);
					PPUMemoryMap.Add(0x3400, NameTableA.Length, NameTableA);
					PPUMemoryMap.Add(0x3800, NameTableB.Length, NameTableB);
					PPUMemoryMap.Add(0x3C00, NameTableB.Length - 0x100, NameTableB);
					break;
				case NametableLayout.HorizontalLayout:
					PPUMemoryMap.Add(0x2000, NameTableA.Length, NameTableA);
					PPUMemoryMap.Add(0x2400, NameTableB.Length, NameTableB);
					PPUMemoryMap.Add(0x2800, NameTableA.Length, NameTableA);
					PPUMemoryMap.Add(0x2C00, NameTableB.Length, NameTableB);

					PPUMemoryMap.Add(0x3000, NameTableA.Length, NameTableA);
					PPUMemoryMap.Add(0x3400, NameTableB.Length, NameTableB);
					PPUMemoryMap.Add(0x3800, NameTableA.Length, NameTableA);
					PPUMemoryMap.Add(0x3C00, NameTableB.Length - 0x100, NameTableB);
					break;
				default:
					throw new NotImplementedException();
			}

			for (var i = 0x3F00; i < 0x4000; i += 0x20)
				PPUMemoryMap.Add(i, 0x20, PPU.PPUPaletteData);

			PPUMemoryMap.Add(0x4000, 0x4000, new MemoryMirror(PPUMemoryMap, 0, 0x4000));
			PPUMemoryMap.Add(0x8000, 0x4000, new MemoryMirror(PPUMemoryMap, 0, 0x4000));
			PPUMemoryMap.Add(0xC000, 0x4000, new MemoryMirror(PPUMemoryMap, 0, 0x4000));
		}

		public void InitialiseNROMCPUMemoryMap()
		{
			CPUMemoryMap.Add(0, _cpuRAM.Length, _cpuRAM);
			CPUMemoryMap.Add(_cpuRAM.Length, _cpuRAM.Length, _cpuRAM);
			CPUMemoryMap.Add(_cpuRAM.Length * 2, _cpuRAM.Length, _cpuRAM);
			CPUMemoryMap.Add(_cpuRAM.Length * 3, _cpuRAM.Length, _cpuRAM);

			for (var i = 0x2000; i < 0x4000; i += 8)
				CPUMemoryMap.Add(i, 8, PPU.Registers);

			CPUMemoryMap.Add(0x4000, 20, _cpuRegisters);

			CPUMemoryMap.Add(0x4014, 1, _oamDMA);
			CPUMemoryMap.Add(0x4015, 1, _cpuRegisters);
			CPUMemoryMap.Add(0x4016, 1, Joypad1);
			CPUMemoryMap.Add(0x4017, 1, Joypad2);

			CPUMemoryMap.Add(0x4018, 0x1FE8, null); //Expansion ROM

			CPUMemoryMap.Add(0x6000, 0x2000, Cartridge.RAMBanks[0]);

			CPUMemoryMap.Add(0x8000, 0x4000, Cartridge.ROMBanks[0]);
			CPUMemoryMap.Add(0xC000, 0x4000, Cartridge.ROMBanks.Length == 1 ? Cartridge.ROMBanks[0] : Cartridge.ROMBanks[1]);
		}
	}
}
