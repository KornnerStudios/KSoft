
namespace KSoft.IO
{
	public class EnumBitStreamerOptions
	{
		/// <summary>Returns false</summary>
		/// <remarks>Not compatible with underlying or stream types that are SByte or Byte</remarks>
		public virtual bool UseNoneSentinelEncoding => false;

		public virtual bool SignExtend => false;

		public virtual bool BitSwap => false;
		/// <remarks>Returning <b>true</b> when <see cref="BitSwap"/> is <b>false</b> will throw an exception</remarks>
		public virtual bool BitSwapGuardAgainstOneBit => false;

		#region Common option implementations
		public class ShouldUseNoneSentinelEncoding : EnumBitStreamerOptions
		{
			public override bool UseNoneSentinelEncoding => true;
		};

		public class ShouldBitSwap : EnumBitStreamerOptions
		{
			public override bool BitSwap => true;
		};
		public class ShouldBitSwapWithOneBitGuard : ShouldBitSwap
		{
			public override bool BitSwapGuardAgainstOneBit => true;
		};
		#endregion
	};
}