using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using Metazel.Library;

namespace Metazel.NES
{
	public partial class NESEngine
	{
        public bool Running { get; private set; }

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

		private int _frames;

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
            Running = true;

		    var firstTickCountWrite = true; //HACK: Suppresses buggy tick display after pause and resume.

			var path = Cartridge.Name + ".log";
			if (File.Exists(path))
				File.Delete(path);

            if(!CPU.Initialised)
			    CPU.Initialise();

			var previousTicks = Environment.TickCount;

            while (Running)
			{
                PPU.DoCycle();
                PPU.DoCycle();
                PPU.DoCycle();

                CPU.DoCycle();

                if (CPU.TotalCycleCount % 1789772 == 0)
                {
                    var ticks = Environment.TickCount - previousTicks;

                    if (!firstTickCountWrite)
                        Console.WriteLine("{0} ({1})", ticks, _frames / (ticks / 1000f));
                    else
                        firstTickCountWrite = false;

                    previousTicks = Environment.TickCount;
                    _frames = 0;
                }
			}
		}

	    public void Stop()
	    {
            Running = false;
	    }

	    public void SetNewFrame(Bitmap frame)
		{
			NewFrame(frame);
		}

		public event Action<Bitmap> NewFrame;

	    public void Reset()
	    {
	        throw new NotImplementedException();
	    }
	}
}
