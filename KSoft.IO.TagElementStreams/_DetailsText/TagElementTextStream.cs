using Contracts = System.Diagnostics.Contracts;

namespace KSoft.IO
{
	/// <remarks><typeparamref name="TCursor"/> needs to implement <see cref="Text.ITextLineInfo">LineInfo</see></remarks>
	public abstract partial class TagElementTextStream<TDoc, TCursor> : TagElementStream<TDoc, TCursor, string>
		where TDoc : class
		where TCursor : class
	{
		/// <summary>Element's qualified name, or null if <see cref="Cursor"/> is null</summary>
		public abstract string CursorName { get; }

		#region GuidFormatString
		string mGuidFormatString = Values.KGuid.kFormatHyphenated;
		/// <summary>
		/// The formatting string for read/writing Guid values.
		/// Hyphenated is the default format. Setting this to NULL reverts the format to default
		/// </summary>
		public string GuidFormatString
		{
			get { return mGuidFormatString; }
			set { mGuidFormatString = value ?? Values.KGuid.kFormatHyphenated; }
		}
		#endregion

		#region SingleFormatSpecifier
		string mSingleFormatSpecifier = Numbers.kSingleRoundTripFormatSpecifier;
		/// <summary>RoundTrip by default</summary>
		public string SingleFormatSpecifier
		{
			get { return mSingleFormatSpecifier; }
			set { mSingleFormatSpecifier = value; }
		}
		#endregion

		#region DoubleFormatSpecifier
		string mDoubleFormatSpecifier = Numbers.kDoubleRoundTripFormatSpecifier;
		/// <summary>RoundTrip by default</summary>
		public string DoubleFormatSpecifier
		{
			get { return mDoubleFormatSpecifier; }
			set { mDoubleFormatSpecifier = value; }
		}
		#endregion

		[Contracts.Pure]
		public override bool ValidateNameArg(string name) { return !string.IsNullOrEmpty(name); }

		protected TagElementTextStream()
		{
			mReadErrorState = new TextStreamReadErrorState(this);
		}

		public void UseDefaultFloatFormatSpecifiers()
		{
			SingleFormatSpecifier = Numbers.kFloatDefaultFormatSpecifier;
			DoubleFormatSpecifier = Numbers.kFloatDefaultFormatSpecifier;
		}
		public void UseRoundTripFloatFormatSpecifiers()
		{
			SingleFormatSpecifier = Numbers.kSingleRoundTripFormatSpecifier;
			DoubleFormatSpecifier = Numbers.kDoubleRoundTripFormatSpecifier;
		}
	};

	static partial class TagElementTextStreamUtils
	{
	};
}