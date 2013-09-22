namespace Metazel
{
	public partial class NESEngine
	{
		private SxROMMapper _sxROMMapper;

		private void InitialiseSxROMMapper()
		{
			_sxROMMapper = new SxROMMapper(this);
		}
	}

	internal class SxROMMapper
	{
		//TODO: Clean this up.

		public readonly SxROMCHRMapper CHRBank0Mapper;
		public readonly SxROMCHRMapper CHRBank1Mapper;
		public readonly SxROMNameTableMapper NameTable00Mapper;
		public readonly SxROMNameTableMapper NameTable01Mapper;
		public readonly SxROMNameTableMapper NameTable10Mapper;
		public readonly SxROMNameTableMapper NameTable11Mapper;
		public readonly SxROMPRGMapper PRGBank0Mapper;
		public readonly SxROMPRGMapper PRGBank1Mapper;
		public readonly SxROMRAMMapper RAMMapper;

		private readonly NESEngine _engine;

		public byte ControlRegister
		{
			get
			{
				return _controlRegister;
			}
			private set
			{
				_controlRegister = value;

				switch (Mirroring)
				{
					case 0:
						NameTable00Mapper._nameTable = _engine.NameTableA;
						NameTable01Mapper._nameTable = _engine.NameTableA;
						NameTable10Mapper._nameTable = _engine.NameTableA;
						NameTable11Mapper._nameTable = _engine.NameTableA;
						break;
					case 1:
						NameTable00Mapper._nameTable = _engine.NameTableB;
						NameTable01Mapper._nameTable = _engine.NameTableB;
						NameTable10Mapper._nameTable = _engine.NameTableB;
						NameTable11Mapper._nameTable = _engine.NameTableB;
						break;
					case 2:
						NameTable00Mapper._nameTable = _engine.NameTableA;
						NameTable01Mapper._nameTable = _engine.NameTableB;
						NameTable10Mapper._nameTable = _engine.NameTableA;
						NameTable11Mapper._nameTable = _engine.NameTableB;
						break;
					case 3:
						NameTable00Mapper._nameTable = _engine.NameTableA;
						NameTable01Mapper._nameTable = _engine.NameTableA;
						NameTable10Mapper._nameTable = _engine.NameTableB;
						NameTable11Mapper._nameTable = _engine.NameTableB;
						break;
				}
			}
		}
		private byte _controlRegister;

		public SxROMMapper(NESEngine engine)
		{
			_engine = engine;

			CHRBank0Mapper = new SxROMCHRMapper(this, false);
			CHRBank1Mapper = new SxROMCHRMapper(this, true);
			NameTable00Mapper = new SxROMNameTableMapper(this);
			NameTable01Mapper = new SxROMNameTableMapper(this);
			NameTable10Mapper = new SxROMNameTableMapper(this);
			NameTable11Mapper = new SxROMNameTableMapper(this);
			PRGBank0Mapper = new SxROMPRGMapper(this, false);
			PRGBank1Mapper = new SxROMPRGMapper(this, true);
			RAMMapper = new SxROMRAMMapper(this);

			ControlRegister = 0xC;
			PRGBank1Mapper._bank = _engine.Cartridge.ROMBanks.Length - 1;
		}

		public byte Mirroring
		{
			get { return (byte) ((byte) (_controlRegister << 6) >> 6); }
		}
		public byte PRGROMBankMode
		{
			get { return (byte) ((byte) (_controlRegister << 4) >> 6); }
		}
		public byte CHRROMBankMode
		{
			get { return (byte) (_controlRegister.GetBit(4) ? 1 : 0); }
		}

		#region Nested type: SxROMCHRMapper

		public class SxROMCHRMapper : IMemoryProvider
		{
			private readonly bool _isBank1;
			private readonly SxROMMapper _mapper;

			public int _bank;

			public SxROMCHRMapper(SxROMMapper mapper, bool isBank1)
			{
				_mapper = mapper;

				_isBank1 = isBank1;
			}

			#region IMemoryProvider Members

			public byte this[int address]
			{
				get
				{
					return _isBank1 && _mapper.CHRROMBankMode == 0
							   ? _mapper._engine.Cartridge.VROMBanks[_mapper.CHRBank0Mapper._bank][address + 0x1000]
							   : _mapper._engine.Cartridge.VROMBanks[_bank][address];
				}
				set
				{
					if (_isBank1 && _mapper.CHRROMBankMode == 0)
						_mapper._engine.Cartridge.VROMBanks[_mapper.CHRBank0Mapper._bank][address + 0x1000] = value;
					else
						_mapper._engine.Cartridge.VROMBanks[_bank][address] = value;
				}
			}

			#endregion
		}

		#endregion

		#region Nested type: SxROMNameTableMapper

		public class SxROMNameTableMapper : IMemoryProvider
		{
			private readonly SxROMMapper _mapper;

			public byte[] _nameTable;

			public SxROMNameTableMapper(SxROMMapper mapper)
			{
				_mapper = mapper;
			}

			#region IMemoryProvider Members

			public byte this[int address]
			{
				get { return _nameTable[address]; }
				set { _nameTable[address] = value; }
			}

			#endregion
		}

		#endregion

		#region Nested type: SxROMPRGMapper

		public class SxROMPRGMapper : IMemoryProvider
		{
			private readonly SxROMMapper _mapper;
			public int _bank;

			private byte _shiftRegister = 0x10;
			private int _writeNumber;

			private readonly bool _isBank1;

			public SxROMPRGMapper(SxROMMapper mapper, bool isBank1)
			{
				_mapper = mapper;

				_isBank1 = isBank1;
			}

			#region IMemoryProvider Members

			public byte this[int address]
			{
				get
				{
					return _isBank1 && _mapper.PRGROMBankMode < 2
							   ? _mapper._engine.Cartridge.ROMBanks[_mapper.CHRBank0Mapper._bank + 1][address]
							   : _mapper._engine.Cartridge.ROMBanks[_bank][address];
				}
				set
				{
					_writeNumber++;

					if (value.GetBit(7))
					{
						_shiftRegister = 0x10;
						_writeNumber = 0;

						_mapper.ControlRegister |= 0x0C;
					}
					else
					{
						if (_writeNumber < 5)
						{
							_shiftRegister >>= 1;
							_shiftRegister = _shiftRegister.SetBit(4, value.GetBit(0));
						}
						else
						{
							var registerValue = ((byte) (_shiftRegister >> 1)).SetBit(4, value.GetBit(0));

							if (!_isBank1)
							{
								if (address >= 0 && address < 0x2000)
									_mapper.ControlRegister = registerValue;
								else if (address >= 0x2000 && address < 0x4000)
									_mapper.CHRBank0Mapper._bank = _mapper.CHRROMBankMode == 0 ? value >> 1 : value;
							}
							else
							{
								if (address >= 0 && address < 0x2000)
								{
									if (_mapper.CHRROMBankMode != 0)
										_mapper.CHRBank1Mapper._bank = value;
								}
								else if (address >= 0x2000 && address < 0x4000)
								{
									switch (_mapper.PRGROMBankMode)
									{
										case 0:
										case 1:
											_bank = (registerValue & 0xF) * 2;
											break;
										case 2:
											_mapper.PRGBank1Mapper._bank = registerValue & 0xF;
											break;
										case 3:
											_mapper.PRGBank0Mapper._bank = registerValue & 0xF;
											break;
									}

									_mapper.RAMMapper.Disabled = registerValue.GetBit(4);
								}
							}

							_shiftRegister = 0x10;
							_writeNumber = 0;
						}
					}
				}
			}

			#endregion
		}

		#endregion

		#region Nested type: SxROMRAMMapper

		internal class SxROMRAMMapper : IMemoryProvider
		{
			private readonly SxROMMapper _mapper;

			public SxROMRAMMapper(SxROMMapper mapper)
			{
				_mapper = mapper;
			}

			public bool Disabled { private get; set; }

			#region IMemoryProvider Members

			public byte this[int address]
			{
				get { return (byte) (Disabled ? 0 : _mapper._engine.Cartridge.RAMBanks[0][address]); }
				set
				{
					if (Disabled)
						return;

					_mapper._engine.Cartridge.RAMBanks[0][address] = value;
				}
			}

			#endregion
		}

		#endregion
	}
}