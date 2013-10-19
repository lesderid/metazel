using Metazel.Library;

namespace Metazel.NES
{
	public class PPUPaletteData : IMemoryProvider
	{
		private readonly byte[] _data = new byte[0x20];

		private readonly byte[] _lookup = new byte[]
		                                  {
			                                  0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07,
			                                  0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F,
			                                  0x00, 0x11, 0x12, 0x13, 0x04, 0x15, 0x16, 0x17,
			                                  0x08, 0x19, 0x1A, 0x1B, 0x0C, 0x1D, 0x1E, 0x1F
		                                  };

		#region IMemoryProvider Members

		public byte this[int address]
		{
			get { return _data[_lookup[address]]; }
			set { _data[_lookup[address]] = value; }
		}

		#endregion
	}
}