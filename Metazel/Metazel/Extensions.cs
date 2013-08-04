using System.Diagnostics.Contracts;

namespace Metazel
{
	internal static class Extensions
	{
		[Pure]
		public static byte SetBit(this byte input, int bit, bool value)
		{
			return (byte) (value ? input | 1 << bit : input & ~(1 << bit));
		}

		[Pure]
		public static bool GetBit(this byte input, int bit)
		{
			return (input | 1 << bit) == input;
		}

		[Pure]
		public static byte GetBits(this byte input, int start, int end)
		{
			return (byte) (input << 7 - end >> 7 - end + start);
		}
	}
}