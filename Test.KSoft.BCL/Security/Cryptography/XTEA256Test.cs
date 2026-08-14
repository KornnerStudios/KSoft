using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Security.Cryptography.Test
{
	[TestClass]
	public sealed class XTEA256Test : BaseTestClass
	{
		[TestMethod]
		public void Encypt_EncryptPath_ThrowsNotImplementedException()
		{
			var xtea = new XTEA256();
			var buffer = new byte[8];

			Assert.ThrowsExactly<NotImplementedException>(() =>
				xtea.Encypt(buffer, 0, buffer.Length));
		}
	}
}
