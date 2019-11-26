using System.IO;

namespace KSoft.IO
{
	partial class EndianReader
	{
		/// <summary>Align the stream's position by a certain page boundry</summary>
		/// <param name="alignmentBit">log2 size of the alignment (ie, 1&lt;&lt;bit)</param>
		/// <returns>True if any alignment had to be performed, false if otherwise</returns>
		public bool AlignToBoundry(int alignmentBit)
		{
			int align_size = IntegerMath.PaddingRequired(alignmentBit, (uint)BaseStream.Position);

			if (align_size > 0)
			{
				BaseStream.Seek(align_size, SeekOrigin.Current);
				return true;
			}

			return false;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				// If we're not the owner, don't let BinaryReader dispose it
				if (!BaseStreamOwner)
					kSetBaseStream(this, null);
			}

			Owner = null;
			StreamName = null;
			mStringEncoding = null;
			mVAT = null;

			base.Dispose(disposing);
		}
	};

	partial class EndianWriter
	{
		/// <summary>Align the stream's position by a certain page boundry</summary>
		/// <param name="alignmentBit">log2 size of the alignment (ie, 1&lt;&lt;bit)</param>
		/// <returns>True if any alignment had to be performed, false if otherwise</returns>
		public bool AlignToBoundry(int alignmentBit)
		{
			int align_size = IntegerMath.PaddingRequired(alignmentBit, (uint)BaseStream.Position);

			if (align_size > 0)
			{
				while (align_size-- > 0)
					BaseStream.WriteByte(byte.MinValue);
				return true;
			}

			return false;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (!BaseStreamOwner)
					base.OutStream = Stream.Null;
			}

			Owner = null;
			StreamName = null;
			mStringEncoding = null;
			mVAT = null;

			base.Dispose(disposing);
		}
	};
}