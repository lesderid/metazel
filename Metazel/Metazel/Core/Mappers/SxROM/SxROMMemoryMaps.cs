using Metazel.Library;

namespace Metazel.NES
{
	public partial class NESEngine
	{
		private void InitialiseSxROMPPUMemoryMap()
		{
			if (Cartridge.VROMBanks.Length > 0)
			{
				PPUMemoryMap.Add(0, 0x1000, _sxROMMapper.CHRBank0Mapper);
				PPUMemoryMap.Add(0x1000, 0x1000, _sxROMMapper.CHRBank1Mapper);
			}
			else
			{
				_ppuRAM = new byte[0x2000];

				PPUMemoryMap.Add(0, 0x2000, _ppuRAM);
			}

			PPUMemoryMap.Add(0x2000, 0x400, _sxROMMapper.NameTable00Mapper);
			PPUMemoryMap.Add(0x2400, 0x400, _sxROMMapper.NameTable01Mapper);
			PPUMemoryMap.Add(0x2800, 0x400, _sxROMMapper.NameTable10Mapper);
			PPUMemoryMap.Add(0x2C00, 0x400, _sxROMMapper.NameTable11Mapper);

			PPUMemoryMap.Add(0x3000, 0xF00, new MemoryMirror(PPUMemoryMap, 0x2000, 0x1000));

			for (var i = 0x3F00; i < 0x4000; i += 0x20)
				PPUMemoryMap.Add(i, 0x20, PPU.PPUPaletteData);

			PPUMemoryMap.Add(0x4000, 0x4000, new MemoryMirror(PPUMemoryMap, 0, 0x4000));
			PPUMemoryMap.Add(0x8000, 0x4000, new MemoryMirror(PPUMemoryMap, 0, 0x4000));
			PPUMemoryMap.Add(0xC000, 0x4000, new MemoryMirror(PPUMemoryMap, 0, 0x4000));
		}

		private void InitialiseSxROMCPUMemoryMap()
		{
			var mapper = new SxROMMapper(this);

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

			CPUMemoryMap.Add(0x6000, 0x2000, mapper.RAMMapper);

			CPUMemoryMap.Add(0x8000, 0x4000, mapper.PRGBank0Mapper);
			CPUMemoryMap.Add(0xC000, 0x4000, mapper.PRGBank1Mapper);
		}
	}
}
