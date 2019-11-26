
namespace KSoft.Security.Cryptography
{
	public abstract class JenkinsHashLookup
	{
		protected static void Fill(ref uint a, ref uint b, ref uint c, byte[] data, ref int i)
		{
			a += (uint)data[i++] |
				((uint)data[i++] << 8) |
				((uint)data[i++] << 16) |
				((uint)data[i++] << 24);
			b += (uint)data[i++] |
				((uint)data[i++] << 8) |
				((uint)data[i++] << 16) |
				((uint)data[i++] << 24);
			c += (uint)data[i++] |
				((uint)data[i++] << 8) |
				((uint)data[i++] << 16) |
				((uint)data[i++] << 24);
		}
		protected static void Fill(ref uint a, ref uint b, ref uint c, char[] data, ref int i)
		{
			a += (uint)data[i++] |
				((uint)data[i++] << 8) |
				((uint)data[i++] << 16) |
				((uint)data[i++] << 24);
			b += (uint)data[i++] |
				((uint)data[i++] << 8) |
				((uint)data[i++] << 16) |
				((uint)data[i++] << 24);
			c += (uint)data[i++] |
				((uint)data[i++] << 8) |
				((uint)data[i++] << 16) |
				((uint)data[i++] << 24);
		}
		protected static void Fill(ref uint a, ref uint b, ref uint c, string data, ref int i)
		{
			a += (uint)data[i++] |
				((uint)data[i++] << 8) |
				((uint)data[i++] << 16) |
				((uint)data[i++] << 24);
			b += (uint)data[i++] |
				((uint)data[i++] << 8) |
				((uint)data[i++] << 16) |
				((uint)data[i++] << 24);
			c += (uint)data[i++] |
				((uint)data[i++] << 8) |
				((uint)data[i++] << 16) |
				((uint)data[i++] << 24);
		}

		protected static void FinalFill(ref uint a, ref uint b, ref uint c, byte[] data, ref int i, int length)
		{
			if (i < length) a += data[i++];
			if (i < length) a += (uint)data[i++] << 8;
			if (i < length) a += (uint)data[i++] << 16;
			if (i < length) a += (uint)data[i++] << 24;

			if (i < length) b += (uint)data[i++];
			if (i < length) b += (uint)data[i++] << 8;
			if (i < length) b += (uint)data[i++] << 16;
			if (i < length) b += (uint)data[i++] << 24;

			if (i < length) c += (uint)data[i++] << 8;
			if (i < length) c += (uint)data[i++] << 16;
			if (i < length) c += (uint)data[i++] << 24;
		}
		protected static void FinalFill(ref uint a, ref uint b, ref uint c, char[] data, ref int i, int length)
		{
			if (i < length) a += data[i++];
			if (i < length) a += (uint)data[i++] << 8;
			if (i < length) a += (uint)data[i++] << 16;
			if (i < length) a += (uint)data[i++] << 24;

			if (i < length) b += (uint)data[i++];
			if (i < length) b += (uint)data[i++] << 8;
			if (i < length) b += (uint)data[i++] << 16;
			if (i < length) b += (uint)data[i++] << 24;

			if (i < length) c += (uint)data[i++] << 8;
			if (i < length) c += (uint)data[i++] << 16;
			if (i < length) c += (uint)data[i++] << 24;
		}
		protected static void FinalFill(ref uint a, ref uint b, ref uint c, string data, ref int i, int length)
		{
			if (i < length) a += data[i++];
			if (i < length) a += (uint)data[i++] << 8;
			if (i < length) a += (uint)data[i++] << 16;
			if (i < length) a += (uint)data[i++] << 24;

			if (i < length) b += (uint)data[i++];
			if (i < length) b += (uint)data[i++] << 8;
			if (i < length) b += (uint)data[i++] << 16;
			if (i < length) b += (uint)data[i++] << 24;

			if (i < length) c += (uint)data[i++] << 8;
			if (i < length) c += (uint)data[i++] << 16;
			if (i < length) c += (uint)data[i++] << 24;
		}
	};
}