using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;

namespace Metazel
{
	public class NESEngine
	{
		public readonly MemoryMap CPUMemoryMap = new MemoryMap();
		public readonly MemoryMap PPUMemoryMap = new MemoryMap();

		private byte[] _cpuRAM = new byte[0x800];
		private byte[] _ppuRAM;

		private object _cpuRegisters;

		public NESCartridge Cartridge { get; private set; }

		private byte[] _nametableA = new byte[0x400];
		private byte[] _nametableB = new byte[0x400];

		public NESCPU CPU;
		public NESPPU PPU;

		public readonly JoypadHandler Joypad1 = new JoypadHandler(1);
		public readonly JoypadHandler Joypad2 = new JoypadHandler(2);
		private OAMDMA _oamDMA;

		private int _frames = 0;

		public void Load(NESCartridge cartridge)
		{
			//TODO: Change memory map based on mapper type.

			Cartridge = cartridge;

			Debug.Assert(Cartridge.ROMMapper == ROMMapper.Mapper000);

			CPU = new NESCPU(this);
			PPU = new NESPPU(this);

			_oamDMA = new OAMDMA(PPU, CPU);

			CPUMemoryMap.Clear();
			PPUMemoryMap.Clear();

			_cpuRAM = new byte[_cpuRAM.Length];

			_nametableA = new byte[_nametableA.Length];
			_nametableB = new byte[_nametableB.Length];

			InitialiseCPUMemoryMap();
			InitialisePPUMemoryMap();

			NewFrame += (Action<Bitmap>) delegate { _frames++; };
		}

		private void InitialisePPUMemoryMap()
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
					PPUMemoryMap.Add(0x2000, _nametableA.Length, _nametableA);
					PPUMemoryMap.Add(0x2400, _nametableA.Length, _nametableA);
					PPUMemoryMap.Add(0x2800, _nametableB.Length, _nametableB);
					PPUMemoryMap.Add(0x2C00, _nametableB.Length, _nametableB);

					PPUMemoryMap.Add(0x3000, _nametableA.Length, _nametableA);
					PPUMemoryMap.Add(0x3400, _nametableA.Length, _nametableA);
					PPUMemoryMap.Add(0x3800, _nametableB.Length, _nametableB);
					PPUMemoryMap.Add(0x3C00, _nametableB.Length - 0x100, _nametableB);
					break;
				case NametableLayout.HorizontalLayout:
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

			for (var i = 0x3F00; i < 0x4000; i += PPU.PPUPaletteData.Length)
				PPUMemoryMap.Add(i, PPU.PPUPaletteData.Length, PPU.PPUPaletteData);

			PPUMemoryMap.Add(0x4000, 0x4000, new MemoryMirror(PPUMemoryMap, 0, 0x4000));
			PPUMemoryMap.Add(0x8000, 0x4000, new MemoryMirror(PPUMemoryMap, 0, 0x4000));
			PPUMemoryMap.Add(0xC000, 0x4000, new MemoryMirror(PPUMemoryMap, 0, 0x4000));

			PPUMemoryMap.PopulateTuplesList();
		}

		private void InitialiseCPUMemoryMap()
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

			CPUMemoryMap.PopulateTuplesList();
		}

		public void Run()
		{
			var path = Cartridge.Name + ".log";
			if (File.Exists(path))
				File.Delete(path);

			CPU.Initialize();

			var i = 1;

			var previousTicks = Environment.TickCount;

			while (true)
			{
				switch (i)
				{
					case 1:
						PPU.DoCycle();

						i = 2;
						break;
					case 2:
						PPU.DoCycle();

						CPU.DoCycle();

						var ticks = Environment.TickCount - previousTicks;
						if (CPU.TotalCycleCount % 1789772 == 0)
						{
							Console.WriteLine("{0} ({1})", ticks, _frames / (ticks / 1000f));

							previousTicks = Environment.TickCount;
							_frames = 0;
						}

						i = 3;
						break;
					case 3:
						PPU.DoCycle();

						i = 1;
						break;
				}
			}
		}

		public void SetNewFrame(Bitmap frame)
		{
			NewFrame(frame);
		}

		public event Action<Bitmap> NewFrame;
	}
}
