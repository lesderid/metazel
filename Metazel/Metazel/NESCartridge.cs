using System;
using System.IO;

namespace Metazel
{
	public class NESCartridge
	{
		public NESCartridge(string path)
		{
			var data = File.ReadAllBytes(path);
			var reader = new BinaryReader(new MemoryStream(data));

			var magic = new string(reader.ReadChars(4));
			if (magic != "NES\u001A")
				throw new InvalidDataException("The NES cartridge file contains invalid data.");

			Name = Path.GetFileName(path);

			var romBanks = reader.ReadByte();
			var vromBanks = reader.ReadByte();

			var byte6 = reader.ReadByte();
			var mirroring = byte6.GetBit(0);
			BatteryBackedRAM = byte6.GetBit(1); //Note: If set, first RAM bank is battery-backed (?).
			var trainer = byte6.GetBit(2);
			var fourScreenVRAM = byte6.GetBit(3);
			var romMapperLower = byte6.GetBits(4, 7);

			var byte7 = reader.ReadByte();
			VSSystem = byte7.GetBit(0);
			var byte7Reserved = byte7.GetBits(1, 3);
			var romMapperHigher = byte7.GetBits(4, 7);

			var ramBanks = reader.ReadByte();

			var byte9 = reader.ReadByte();
			var isPAL = byte9.GetBit(0);
			var byte9Reserved = byte9.GetBits(1, 7);

			var reservedBytes = reader.ReadBytes(6);

			ROMBanks = new byte[romBanks][];
			for (var i = 0; i < romBanks; i++)
				ROMBanks[i] = reader.ReadBytes(16384);

			VROMBanks = new byte[vromBanks][];
			for (var i = 0; i < vromBanks; i++)
				VROMBanks[i] = reader.ReadBytes(8192);

			if (trainer)
				Trainer = reader.ReadBytes(512);

			RAMBanks = new byte[ramBanks == 0 ? 1 : ramBanks][];
			for (var i = 0; i < RAMBanks.Length; i++)
				RAMBanks[i] = new byte[8192];

			//if (BatteryBackedRAM)
			//	; //Load RAM

			ROMMapper = (ROMMapper) (romMapperHigher << 4 | romMapperLower);

			VRAMLayout = fourScreenVRAM ? VRAMLayout.FourScreenLayout : (mirroring ? VRAMLayout.HorizontalLayout : VRAMLayout.VerticalLayout);

			Region = isPAL ? Region.PAL : Region.NTSC;
		}

		public void SaveRAM()
		{
			if (!BatteryBackedRAM)
				return;

			throw new NotImplementedException();
		}

		public string Name { get; private set; }

		public byte[][] ROMBanks { get; private set; }
		public byte[][] VROMBanks { get; private set; }

		public byte[][] RAMBanks { get; private set; }

		public byte[] Trainer { get; private set; }

		public Region Region { get; private set; }
		public VRAMLayout VRAMLayout { get; private set; }
		public ROMMapper ROMMapper { get; private set; }

		public bool VSSystem { get; private set; }

		public bool BatteryBackedRAM { get; private set; }
	}
}