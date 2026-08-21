namespace KSoft.WPF
{
	public interface IBitVectorUserInterfaceData
	{
		int NumberOfBits { get; }
		string GetDisplayName(int bitIndex);
		string GetDescription(int bitIndex);
		bool IsVisible(int bitIndex);
	};
}
