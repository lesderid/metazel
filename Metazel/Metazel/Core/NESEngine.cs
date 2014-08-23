using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using Metazel.Library;

namespace Metazel.NES
{
	public partial class NESEngine
	{
		public readonly MemoryMap CPUMemoryMap = new MemoryMap();
		public readonly MemoryMap PPUMemoryMap = new MemoryMap();

		private byte[] _cpuRAM = new byte[0x800];
		private byte[] _ppuRAM;

		private object _cpuRegisters;

		public NESCartridge Cartridge { get; private set; }

		public byte[] NameTableA = new byte[0x400];
		public byte[] NameTableB = new byte[0x400];

		public NESCPU CPU;
		public NESPPU PPU;

		public readonly JoypadHandler Joypad1 = new JoypadHandler(1);
		public readonly JoypadHandler Joypad2 = new JoypadHandler(2);
		private OAMDMA _oamDMA;

		private int _frames = 0;

		public void Load(NESCartridge cartridge)
		{
			//TODO: Add better mapper implementation (script-based?).

			Cartridge = cartridge;

			CPU = new NESCPU(this);
			PPU = new NESPPU(this);

			_oamDMA = new OAMDMA(PPU, CPU);

			CPUMemoryMap.Clear();
			PPUMemoryMap.Clear();

			_cpuRAM = new byte[_cpuRAM.Length];

			NameTableA = new byte[NameTableA.Length];
			NameTableB = new byte[NameTableB.Length];

			switch (Cartridge.ROMMapper)
			{
				case ROMMapper.SxROM:
					InitialiseSxROMMapper();
					break;
			}

			InitialiseCPUMemoryMap();
			InitialisePPUMemoryMap();

			NewFrame += (Action<Bitmap>) delegate { _frames++; };
		}

		private void InitialisePPUMemoryMap()
		{
			switch (Cartridge.ROMMapper)
			{
				case ROMMapper.NROM:
					InitialiseNROMPPUMemoryMap();
					break;
				case ROMMapper.SxROM:
					InitialiseSxROMPPUMemoryMap();
					break;
			}

			PPUMemoryMap.PopulateTuplesList();
		}

		private void InitialiseCPUMemoryMap()
		{
			switch (Cartridge.ROMMapper)
			{
				case ROMMapper.NROM:
					InitialiseNROMCPUMemoryMap();
					break;
				case ROMMapper.SxROM:
					InitialiseSxROMCPUMemoryMap();
					break;
                default:
			        throw new NotImplementedException(string.Format("Mapper {0} not implemented!", Cartridge.ROMMapper));
			}

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
                PPU.DoCycle();

				switch (i)
				{
					case 1:
						i = 2;
						break;
					case 2:
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
