
namespace KSoft.Memory.Strings
{
	/// <summary>
	/// Configuration properties for defining how a <see cref="StringMemoryPool"/> serializes strings and generates string reference data
	/// </summary>
	public class StringMemoryPoolSettings
	{
		/// <summary>String serializing definition for the <see cref="StringMemoryPool"/></summary>
		public StringStorage Storage { get; private set; }

		/// <summary>Byte order used to encode and decode the pool's string records, independently of the stream's numeric byte order.</summary>
		public Shell.EndianFormat ByteOrder { get; private set; }

		/// <summary>Start address for all string address references</summary>
		public Values.PtrHandle BaseAddress { get; private set; }

		#region AddressSize
		readonly Shell.ProcessorSize mAddressSize;
		/// <summary>Size of addresses used for referencing serialized strings</summary>
		public Shell.ProcessorSize AddressSize { get { return mAddressSize; } }
		#endregion

		/// <summary>Is an empty string entry automatically add to the pool</summary>
		public bool ImplicitNull { get; private set; }

		/// <summary>Are duplicate strings allowed in the pool?</summary>
		public bool AllowDuplicates { get; set; }


		#region Ctor
		/// <summary>Define a new <see cref="StringMemoryPool"/> configuration</summary>
		/// <param name="method">Text storage definition</param>
		/// <param name="byteOrder">Byte order used for string records.</param>
		/// <param name="implicitNull">Is a null string entry atomically added?</param>
		/// <param name="addressSize">Size of string address references</param>
		/// <remarks>Base address defaults to the null equivlent on <paramref name="addressSize"/> platforms</remarks>
		public StringMemoryPoolSettings(StringStorage method, Shell.EndianFormat byteOrder,
			bool implicitNull, Shell.ProcessorSize addressSize)
		{
			{ Storage = method; ByteOrder = byteOrder; AllowDuplicates = false; }

			mAddressSize = addressSize;
			ImplicitNull = implicitNull;

			BaseAddress = mAddressSize == Shell.ProcessorSize.x64 ?
				Values.PtrHandle.Null64 : Values.PtrHandle.Null32;
		}
		/// <summary>Define a new <see cref="StringMemoryPool"/> configuration</summary>
		/// <param name="method">Text storage definition</param>
		/// <param name="byteOrder">Byte order used for string records.</param>
		/// <param name="implicitNull">Is a null string entry atomically added?</param>
		/// <param name="baseAddress">Base address for string references</param>
		/// <remarks><see cref="AddressSize"/> is determined from <paramref name="baseAddress"/></remarks>
		public StringMemoryPoolSettings(StringStorage method, Shell.EndianFormat byteOrder,
			bool implicitNull, Values.PtrHandle baseAddress)
		{
			{ Storage = method; ByteOrder = byteOrder; AllowDuplicates = false; }

			mAddressSize = baseAddress.Is64bit ? Shell.ProcessorSize.x64 : Shell.ProcessorSize.x32;
			ImplicitNull = implicitNull;
			BaseAddress = baseAddress;
		}

		/// <summary>Define a new <see cref="StringMemoryPool"/> configuration</summary>
		/// <param name="method">Text storage definition</param>
		/// <param name="byteOrder">Byte order used for string records.</param>
		/// <param name="implicitNull">Is a null string entry atomically added?</param>
		/// <remarks><see cref="AddressSize"/> is determined from <see cref="Shell.Platform.Environment"/></remarks>
		public StringMemoryPoolSettings(StringStorage method, Shell.EndianFormat byteOrder, bool implicitNull)
			: this(method, byteOrder, implicitNull,
				Shell.Platform.Environment.ProcessorType.ProcessorSize)
		{
		}
		/// <summary>Define a new <see cref="StringMemoryPool"/> configuration</summary>
		/// <param name="method">Text storage definition</param>
		/// <param name="byteOrder">Byte order used for string records.</param>
		/// <param name="baseAddress">Base address for string references</param>
		/// <remarks>A null string entry <b>is</b> added by default</remarks>
		public StringMemoryPoolSettings(StringStorage method, Shell.EndianFormat byteOrder, Values.PtrHandle baseAddress)
			: this(method, byteOrder, true, baseAddress)
		{
		}
		/// <summary>Define a new <see cref="StringMemoryPool"/> configuration</summary>
		/// <param name="method">Text storage definition</param>
		/// <param name="byteOrder">Byte order used for string records.</param>
		/// <remarks>
		/// A null string entry <b>is</b> added by default.
		///
		/// <see cref="AddressSize"/> is determined from <see cref="Shell.Platform.Environment"/>
		/// </remarks>
		public StringMemoryPoolSettings(StringStorage method, Shell.EndianFormat byteOrder)
			: this(method, byteOrder, true,
				Shell.Platform.Environment.ProcessorType.ProcessorSize)
		{
		}
		#endregion
	};
}
