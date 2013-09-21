using System;
using System.Diagnostics;
using System.IO;

namespace Metazel
{
	public class PPURegisters : IMemoryProvider
	{
		private byte _ppuCtrl; //$2000
		private byte _ppuMask; //$2001
		private byte _ppuStatus; //$2002
		public byte OAMAddress; //$2003

		private readonly NESPPU _ppu;
		private readonly NESEngine _engine;

		public PPURegisters(NESPPU ppu, NESEngine engine)
		{
			_ppu = ppu;
			_engine = engine;
		}

		public int NameTableAddress
		{
			get
			{
				switch (_ppuCtrl.GetBits(0, 1))
				{
					case 0:
						return 0x2000;
					case 1:
						return 0x2400;
					case 2:
						return 0x2800;
					case 3:
						return 0x2C00;
				}

				return 0;
			}
		}
		public int AddressIncrementAmount
		{
			get { return _ppuCtrl.GetBit(2) ? 32 : 1; }
		}
		public int SpritePatternTableAddress
		{
			get { return _ppuCtrl.GetBit(3) ? 0x1000 : 0x0000; }
		}
		public int BackgroundPatternTableAddress
		{
			get { return _ppuCtrl.GetBit(4) ? 0x1000 : 0x0000; }
		}
		public bool LargeSprites
		{
			get { return _ppuCtrl.GetBit(5); }
		}
		public bool PPUSlave
		{
			get { return _ppuCtrl.GetBit(6); }
		}
		public bool VBlankNMI
		{
			get { return _ppuCtrl.GetBit(7); }
		}

		public bool Monochrome
		{
			get { return _ppuMask.GetBit(0); }
		}
		public bool BackgroundClipping
		{
			get { return _ppuMask.GetBit(1); }
		}
		public bool SpriteClipping
		{
			get { return _ppuMask.GetBit(2); }
		}
		public bool BackgroundVisible
		{
			get { return _ppuMask.GetBit(3); }
		}
		public bool SpritesVisible
		{
			get { return _ppuMask.GetBit(4); }
		}
		public bool RedIntensified
		{
			get { return _ppuMask.GetBit(5); }
		}
		public bool BlueIntensified
		{
			get { return _ppuMask.GetBit(6); }
		}
		public bool GreenIntensified
		{
			get { return _ppuMask.GetBit(7); }
		}

		public bool SpriteOverflow
		{
			get { return _ppuStatus.GetBit(5); }
			set { _ppuStatus = _ppuStatus.SetBit(5, value); }
		}
		public bool Sprite0Hit
		{
			get { return _ppuStatus.GetBit(6); }
			set { _ppuStatus = _ppuStatus.SetBit(6, value); }
		}
		public bool VBlank
		{
			get { return _ppuStatus.GetBit(7); }
			set { _ppuStatus = _ppuStatus.SetBit(7, value); }
		}

		public byte VerticalScroll { get; private set; }
		public byte HorizontalScroll { get; private set; }

		private bool _writeLatch;

		private ushort _ppuAddress;

		private byte _readBuffer = 0xE8; //TODO: Find out if this is the correct start buffer.

		#region IMemoryProvider Members

		public byte this[int address]
		{
			get
			{
				switch (address)
				{
					case 2:
						_writeLatch = false;

						var status = _ppuStatus;
						VBlank = false;
						return status;
					case 4:
						return _ppu.OAMData[OAMAddress];
					case 7:
						var previousValue = _readBuffer;
						_readBuffer = _ppu.Memory[_ppuAddress];
						_ppuAddress += (ushort) AddressIncrementAmount;
						return _ppuAddress >= 0x3F00 ? _readBuffer : previousValue;
					default:
						Console.WriteLine("Reading {0:X4} (?) ...", 0x2000 + address);

						File.WriteAllText(_engine.Cartridge.Name + ".log", _engine.CPU._stringBuilder.ToString());

						Environment.Exit(0);
						break;
				}

				return 0x00;
			}
			set
			{
				switch (address)
				{
					case 0:
						_ppuCtrl = value;
						break;
					case 1:
						_ppuMask = value;
						break;
					case 3:
						OAMAddress = value;
						break;
					case 4:
						_ppu.OAMData[OAMAddress] = value;
						OAMAddress++;
						break;
					case 5:
						if (_writeLatch)
							VerticalScroll = value;
						else
							HorizontalScroll = value;
							
						_writeLatch = !_writeLatch;
						break;
					case 6:
						if (_writeLatch)
							_ppuAddress |= value;
						else
							_ppuAddress = (ushort) (value << 8);

						_writeLatch = !_writeLatch;
						break;
					case 7:
						if (_ppuAddress == 0x3F10)
							_ppu.Memory[0x3F00] = value; //HACK: Fixes SMB background. Correct fix: implementing palette mirroring.
						else
							_ppu.Memory[_ppuAddress] = value;
						_ppuAddress += (ushort) AddressIncrementAmount;
						break;
					default:
						Console.WriteLine("Writing {0:X4} (?) ...", 0x2000 + address);

						File.WriteAllText(_engine.Cartridge.Name + ".log", _engine.CPU._stringBuilder.ToString());

						Environment.Exit(0);
						break;
				}
			}
		}

		#endregion
	}
}