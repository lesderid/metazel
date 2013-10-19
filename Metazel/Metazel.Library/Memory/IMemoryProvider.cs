namespace Metazel.Library
{
	public interface IMemoryProvider
	{
		byte this[int address] { get; set; }
	}
}