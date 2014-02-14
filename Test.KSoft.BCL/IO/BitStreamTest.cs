using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.IO.Test
{
	[TestClass]
	public class BitStreamTest : BaseTestClass
	{
		[TestMethod]
		public void IO_BitStreamLogicTest()
		{
			var values = new KeyValuePair<uint, int>[] {
				new KeyValuePair<uint, int>(10, 7),
				new KeyValuePair<uint, int>(0xBEEFBEEF, 32),
				new KeyValuePair<uint, int>(12, 7),
				new KeyValuePair<uint, int>(0x13371337, 32),
				new KeyValuePair<uint, int>(123, 7),
				new KeyValuePair<uint, int>(0xDEADC0DE, 32),
				new KeyValuePair<uint, int>(0, 7),
				new KeyValuePair<uint, int>(111, 7),

				new KeyValuePair<uint, int>(1, 1),
				new KeyValuePair<uint, int>(2, 2),
				new KeyValuePair<uint, int>(7, 3),
				new KeyValuePair<uint, int>(14, 4),
				new KeyValuePair<uint, int>(21, 5),
				new KeyValuePair<uint, int>(42, 6),
				new KeyValuePair<uint, int>(14406, 15),
			};

			using (var ms = new MemoryStream())
			{
				using (var bs = new IO.BitStream(ms, FileAccess.Write))
				{
					bs.StreamMode = FileAccess.Write;
					foreach (var kv in values)
						bs.WriteWord(kv.Key, kv.Value);
				}
				Text.Util.ByteArrayToStream(ms.ToArray(), System.Console.Out);
				System.Console.WriteLine();

				ms.Position = 0;
				using (var bs_old = new BKSystem.IO.BitStream(ms))
				{
					foreach (var kv in values)
					{
						uint word;
						bs_old.Read(out word, 0, kv.Value);
						Assert.AreEqual(kv.Key, word);
					}
				}
			}

			using (var ms = new MemoryStream())
			{
				using (var bs_old = new BKSystem.IO.BitStream())
				{
					foreach (var kv in values)
						bs_old.Write(kv.Key, 0, kv.Value);
					bs_old.WriteTo(ms);
				}
				Text.Util.ByteArrayToStream(ms.ToArray(), System.Console.Out);
				System.Console.WriteLine();

				ms.Position = 0;
				using (var bs = new IO.BitStream(ms, FileAccess.Read))
				{
					bs.StreamMode = FileAccess.Read;
					foreach (var kv in values)
					{
						uint word;
						bs.ReadWord(out word, kv.Value);
						Assert.AreEqual(kv.Key, word);
					}
				}
			}

			using (var ms = new MemoryStream())
			{
				using (var bs = new IO.BitStream(ms, FileAccess.Write))
				{
					bs.StreamMode = FileAccess.Write;
					bs.Write((int)1337, 15);
					bs.Write(-21474836480L, 60);
					bs.Write(-1, 30);
					bs.Write(false);
					bs.Write((int)0xDEDEAD, 27);
				}
				Text.Util.ByteArrayToStream(ms.ToArray(), System.Console.Out);
				System.Console.WriteLine();

				ms.Position = 0;
//				using (var bs_old = new BKSystem.IO.BitStream(ms))
				using (var bs_old = new IO.BitStream(ms, FileAccess.Read))
				{
					bs_old.StreamMode = FileAccess.Read;
					int _int;
					long _long;
					bool _bool;

					//bs_old.Read(out _int, 0, 15);
					bs_old.Read(out _int, 15);
					Assert.AreEqual(1337, _int);

					bs_old.Read(out _long, 60, signExtend: true);
					Assert.AreEqual(-21474836480L, _long);
					//bs_old.Read(out _int, 0, 30);
					bs_old.Read(out _int, 30, signExtend: true);
					Assert.AreEqual(-1, _int);

					bs_old.Read(out _bool);
					Assert.AreEqual(false, _bool);

					//bs_old.Read(out _int, 0, 27);
					bs_old.Read(out _int, 27);
					Assert.AreEqual((int)0xDEDEAD, _int);
				}
			}
		}
	};
}