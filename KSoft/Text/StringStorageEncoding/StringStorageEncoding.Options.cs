using System;

namespace KSoft.Text
{
	partial class StringStorageEncoding
	{
		/// <summary>Additional options for constructing the <see cref="StringStorageEncoding"/></summary>
		[Flags]
		public enum Options : byte
		{
//			/// <summary>No additional encoding options</summary>
//			/// <remarks>Options will thus default to "false"</remarks>
//			None = 0,

			/// <summary>Encoding uses a byte order marker</summary>
			/// <seealso cref="System.Text.Encoding.GetPreamble()"/>
			/// <remarks>This may conflict with strings which store prefix data (eg, <see cref="Memory.Strings.StringStorageWidthType.Pascal"/>)</remarks>
			UseByteOrderMark = 1 << 0,
			/// <summary>Throw an exception when invalid bytes are handled</summary>
			/// <remarks>
			/// Ignored in <see cref="Memory.Strings.StringStorageWidthType.Ascii"/> cases.
			///
			/// For <see cref="Memory.Strings.StringStorageWidthType.UTF7"/> this disables optional characters.
			/// </remarks>
			ThrowOnInvalidBytes = 1 << 1,
			/// <summary>
			/// By default, we always flush the encoding's encoder\decoder. Use this to listen
			/// to the "flush" parameter instead
			/// </summary>
			DontAlwaysFlush = 1 << 2,

			/// <summary>All options are enabled</summary>
			kAll = UseByteOrderMark | ThrowOnInvalidBytes | DontAlwaysFlush
		};
	};
}