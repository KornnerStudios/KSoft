
namespace KSoft.Collections
{
	/// <summary>Directions in which collection items may be sorted</summary>
	public enum SortDirection : byte
	{
		/// <summary>Sort data from least to greatest</summary>
		Ascending,
		/// <summary>Sort data from greatest to least</summary>
		Descending,

		[System.Reflection.Obfuscation(Exclude=true)]
		[System.Obsolete(EnumBitEncoderBase.kObsoleteMsg, true)] kNumberOf
	};

	[System.Flags]
	public enum TreeTraversalDirection : byte
	{
		PreOrder	= 1,	// Root, Left, Right
		InOrder		= 2,	// Left, Root, Right
		PostOrder	= 3,	// Left, Right, Root

		Left		= 1 << 2,
		Root		= 1 << 3,
		Right		= 1 << 4,

		kOrderMask	= (1 << 2) - 1,
		kDirMask	= (1 << 5) - 1 - kOrderMask,

		[System.Reflection.Obfuscation(Exclude=true)]
		[System.Obsolete(EnumBitEncoderBase.kObsoleteMsg, true)] kAll = kOrderMask | kDirMask
	};

	[System.Flags]
	public enum TreeTraversalOrders : byte
	{
		PreOrder = 1 << 0,
		InOrder = 1 << 1,
		PostOrder = 1 << 2,

		[System.Reflection.Obfuscation(Exclude=true)]
		kAll = PreOrder | InOrder | PostOrder
	};
}