namespace KSoft.Bitwise
{
	public interface IByteSwappable
	{
		int SizeOf { get; }

		short[] ByteSwapCodes { get; }
	};
}
