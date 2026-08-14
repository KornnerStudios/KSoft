using System;
using HashAlgorithm = System.Security.Cryptography.HashAlgorithm;

#nullable enable

namespace KSoft.Security.Cryptography
{
	// See also Karl Malbrain's compact CRC-32, with pre and post conditioning.
	// See "A compact CCITT crc16 and crc32 C implementation that balances processor cache usage against speed":
	// http://www.geocities.ws/malbrain/crc_c.html

	public static partial class Crc16
	{
		public const int kCrcTableSize = 256;
		public const ushort kDefaultPolynomial = 0x1021;
		internal static readonly ushort[] kDefaultTable = new Definition().CrcTable;
	};

	public sealed class CrcHash16
		: HashAlgorithm
	{
		#region Registeration
		public const string kAlgorithmName = "KSoft.Security.Cryptography.CrcHash16";

		public new static CrcHash16 Create(string algName)
		{
			ArgumentNullException.ThrowIfNull(algName);

			return (CrcHash16)(System.Security.Cryptography.CryptoConfig.CreateFromName(algName)
				?? throw new InvalidOperationException($"'{algName}' is not registered as a CRC-16 hash algorithm."));
		}
		public new static CrcHash16 Create()
		{
			return Create(kAlgorithmName);
		}

		static CrcHash16()
		{
			System.Security.Cryptography.CryptoConfig.AddAlgorithm(typeof(CrcHash16), kAlgorithmName);
		}
		#endregion

		readonly Crc16.Definition mDefinition;
		readonly byte[] mHashBytes;
		public ushort Hash16 { get; private set; }

		public CrcHash16()
			: this(new Crc16.Definition(crcTable: Crc16.kDefaultTable))
		{
		}

		public CrcHash16(Crc16.Definition definition)
		{
			ArgumentNullException.ThrowIfNull(definition);

			base.HashSizeValue = Bits.kInt16BitCount;

			mDefinition = definition;
			mHashBytes = new byte[sizeof(ushort)];
			Initialize();
		}

		public override void Initialize()
		{
			Array.Clear(mHashBytes, 0, mHashBytes.Length);
			Hash16 = mDefinition.InitialValue;

			Hash16 ^= mDefinition.XorIn;
		}

		/// <summary>Performs the hash algorithm on the data provided.</summary>
		/// <param name="array">The array containing the data.</param>
		/// <param name="startIndex">The position in the array to begin reading from.</param>
		/// <param name="count">How many bytes in the array to read.</param>
		protected override void HashCore(byte[] array, int startIndex, int count)
		{
			Hash16 = mDefinition.HashCore(Hash16, array, startIndex, count);
		}

		/// <summary>Performs any final activities required by the hash algorithm.</summary>
		/// <returns>The final hash value.</returns>
		protected override byte[] HashFinal()
		{
			Hash16 ^= mDefinition.XorOut;
			Bitwise.ByteSwap.ReplaceBytes(mHashBytes, 0, Hash16);
			return mHashBytes;
		}
	};
}
