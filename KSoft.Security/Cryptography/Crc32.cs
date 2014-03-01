using System;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;
using HashAlgorithm = System.Security.Cryptography.HashAlgorithm;

namespace KSoft.Security.Cryptography
{
	public static partial class Crc32
	{
		public const int kCrcTableSize = 256;
		public const uint kDefaultPolynomial = 0xEDB88320; // 0x04C11DB7 nets the same results
		internal static readonly uint[] kDefaultTable = new Definition().CrcTable;
	};

	public class CrcHash32 : HashAlgorithm
	{
		#region Registeration
		public const string kAlgorithmName = "KSoft.Security.Cryptography.CrcHash32";

		public new static CrcHash32 Create(string algName)
		{
			return (CrcHash32)System.Security.Cryptography.CryptoConfig.CreateFromName(kAlgorithmName);
		}
		public new static CrcHash32 Create()
		{
			return Create(kAlgorithmName);
		}

		static CrcHash32()
		{
			System.Security.Cryptography.CryptoConfig.AddAlgorithm(typeof(CrcHash32), kAlgorithmName);
		}
		#endregion

		readonly Crc32.Definition mDefinition;
		byte[] mHashBytes;
		public uint Hash32 { get; private set; }

		public CrcHash32()
		{
			base.HashSizeValue = Bits.kInt32BitCount;

			mDefinition = new Crc32.Definition(crcTable: Crc32.kDefaultTable);
			mHashBytes = new byte[sizeof(uint)];
		}

		public override void Initialize()
		{
			Array.Clear(mHashBytes, 0, mHashBytes.Length);
			Hash32 = mDefinition.InitialValue;
		}

		/// <summary>Performs the hash algorithm on the data provided.</summary>
		/// <param name="array">The array containing the data.</param>
		/// <param name="startIndex">The position in the array to begin reading from.</param>
		/// <param name="count">How many bytes in the array to read.</param>
		protected override void HashCore(byte[] array, int startIndex, int count)
		{
			Hash32 = mDefinition.HashCore(Hash32, array, startIndex, count);
		}

		/// <summary>Performs any final activities required by the hash algorithm.</summary>
		/// <returns>The final hash value.</returns>
		protected override byte[] HashFinal()
		{
			Bitwise.ByteSwap.ReplaceBytes(mHashBytes, 0, Hash32 ^ mDefinition.XorOut);
			return mHashBytes;
		}
	};
}