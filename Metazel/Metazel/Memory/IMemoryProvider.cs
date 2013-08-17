namespace Metazel
{
	internal interface IMemoryProvider
	{
		byte this[int address] { get; set; }
	}
}