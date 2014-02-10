
namespace KSoft.IO
{
	public class EnumBitStreamerOptions
	{
		/// <summary>Returns false</summary>
		/// <remarks>Not compatible with underlying or stream types that are SByte or Byte</remarks>
		public virtual bool UseNoneSentinelEncoding { get {
			return false;
		} }

		public virtual bool SignExtend { get {
			return false;
		} }

		public virtual bool BitSwap { get {
			return false;
		} }
		/// <remarks>Returning <b>true</b> when <see cref="BitSwap"/> is <b>false</b> will throw an exception</remarks>
		public virtual bool BitSwapGuardAgainstOneBit { get {
			return false;
		} }

		#region Common option implementations
		public class ShouldUseNoneSentinelEncoding : EnumBitStreamerOptions
		{
			public override bool UseNoneSentinelEncoding { get {
				return true;
			} }
		};

		public class ShouldBitSwap : EnumBitStreamerOptions
		{
			public override bool BitSwap { get {
				return true;
			} }
		};
		public class ShouldBitSwapWithOneBitGuard : ShouldBitSwap
		{
			public override bool BitSwapGuardAgainstOneBit { get {
				return true;
			} }
		};
		#endregion
	};
}