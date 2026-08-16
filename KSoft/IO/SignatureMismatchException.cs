using System;
using System.IO;

namespace KSoft.IO
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors")]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA2237:MarkISerializableTypesWithSerializable")]
	public partial class SignatureMismatchException : System.Exception
	{
		const string kFormat = "Invalid signature! @{0} Expected '{1}', got '{2}'";
		const string kDescFormat = "Invalid '{0}' signature! Expected '{1}', got '{2}'";

		static string FormatDescriptionMessage(string dataDescription, string expected, string found)
		{
			ArgumentException.ThrowIfNullOrEmpty(dataDescription);

			return string.Format(Util.InvariantCultureInfo, kDescFormat, dataDescription, expected, found);
		}

		static long GetSignaturePosition(Stream s, string expected)
		{
			ArgumentNullException.ThrowIfNull(s);

			return s.Position - expected.Length;
		}

		public SignatureMismatchException(string dataDescription, string expected, string found)
			: base(FormatDescriptionMessage(dataDescription, expected, found))
		{
		}

		SignatureMismatchException(long pos, string expected, string found) :
			base(string.Format(Util.InvariantCultureInfo, kFormat, pos.ToString("X8", Util.InvariantCultureInfo), expected, found))
		{
		}
		#region Stream ctors
		public SignatureMismatchException(Stream s, string expected, string found) :
			this(GetSignaturePosition(s, expected), expected, found)
		{
		}
		#endregion

		#region EndianReader utils
		public static void Assert(IO.EndianReader s, string expected, Memory.Strings.StringStorage storage)
		{
			ArgumentNullException.ThrowIfNull(s);
			ArgumentException.ThrowIfNullOrEmpty(expected);

			string signature = s.ReadString(storage, expected.Length);
			if (signature != expected)
			{
				throw new SignatureMismatchException(s.BaseStream, expected, signature);
			}
		}
		public static void Assert(IO.EndianReader s, string expected, Text.StringStorageEncoding encoding)
		{
			ArgumentNullException.ThrowIfNull(s);
			ArgumentException.ThrowIfNullOrEmpty(expected);
			ArgumentNullException.ThrowIfNull(encoding);

			string signature = s.ReadString(encoding, expected.Length);
			if (signature != expected)
			{
				throw new SignatureMismatchException(s.BaseStream, expected, signature);
			}
		}
		#endregion
	};
}
