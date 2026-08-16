using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Security.Cryptography;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Test
{
	[TestClass]
	public partial class UtilitiesTest : BaseTestClass
	{
		static void AssertThrowsArgumentNull(Action action, string paramName)
		{
			var exception = Assert.ThrowsExactly<ArgumentNullException>(action);

			Assert.AreEqual(paramName, exception.ParamName);
		}

		static void AssertThrowsArgumentOutOfRange(Action action, string paramName)
		{
			var exception = Assert.ThrowsExactly<ArgumentOutOfRangeException>(action);

			Assert.AreEqual(paramName, exception.ParamName);
		}

		static void AssertThrowsArgument(Action action, string paramName)
		{
			var exception = Assert.ThrowsExactly<ArgumentException>(action);

			Assert.AreEqual(paramName, exception.ParamName);
		}

		static void AssertThrowsInvalidOperation(Action action)
		{
			Assert.ThrowsExactly<InvalidOperationException>(action);
		}

		sealed class NonSeekableStream : MemoryStream
		{
			public NonSeekableStream(byte[] buffer) : base(buffer)
			{
			}

			public override bool CanSeek => false;
		}

		[TestMethod]
		public void Util_UnixTimeTest()
		{
			{
				long test = 0x4B71FD5B;
				var test_date = new System.DateTime(2010, 2, 10, 0, 27, 7, System.DateTimeKind.Utc);

				long value = Util.ConvertDateTimeToUnixTime(test_date);
				var value_date = Util.ConvertDateTimeFromUnixTime(test);

				Assert.AreEqual(test, value);
				Assert.AreEqual(test_date, value_date);
			}
			var now = System.DateTime.UtcNow;
			// We have to clamp it since datetime tracks TimeOfDay plus Milliseconds
			var now_clamped = new System.DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second, now.Kind);

			var time_t = Util.ConvertDateTimeToUnixTime(now);
			var converted = Util.ConvertDateTimeFromUnixTime(time_t);

			Assert.AreEqual(now_clamped, converted);
		}

		[TestMethod]
		public void Util_GenericReferenceEqualsTest()
		{
			string x = "x", y = "y";

			Assert.IsTrue(Util.GenericReferenceEquals(x, x));
			Assert.IsTrue(Util.GenericReferenceEquals(x, y) == Util.GenericReferenceEquals(y, x));

			y = null;
			Assert.IsFalse(Util.GenericReferenceEquals(x, y));

			x = null;
			Assert.IsTrue(Util.GenericReferenceEquals(x, y));
		}

		[TestMethod]
		public void Util_ThrowIfNullTest()
		{
			var value = new object();

			Assert.AreSame(value, Util.ThrowIfNull(value));

			value = null;
			var exception = Assert.ThrowsExactly<ArgumentNullException>(() => Util.ThrowIfNull(value));

			Assert.AreEqual(nameof(value), exception.ParamName);
		}

		[TestMethod]
		public void TypeExtensions_VirtualBufferLengthGuards_ThrowArgumentOutOfRangeException()
		{
			using var stream = new IO.EndianStream(new MemoryStream());

			AssertThrowsArgumentOutOfRange(() => stream.EnterVirtualBuffer(0), "bufferLength");
			AssertThrowsArgumentOutOfRange(() => stream.EnterVirtualBuffer(-1), "bufferLength");
			AssertThrowsArgumentOutOfRange(() => stream.EnterVirtualBufferWithBookmark(0), "bufferLength");
			AssertThrowsArgumentOutOfRange(() => stream.EnterVirtualBufferWithBookmark(-1), "bufferLength");
		}

		[TestMethod]
		public void TypeExtensions_StreamBookmarkGuards_ThrowExpectedExceptions()
		{
			using var stream = new IO.EndianStream(new MemoryStream());

			AssertThrowsArgumentNull(() => _ = TypeExtensions.NullOr<string, int>("value", null!), "func");
			AssertThrowsArgumentNull(() => _ = TypeExtensions.EnterStreamModeBookmark(null!, FileAccess.Read), "stream");
			AssertThrowsInvalidOperation(() => _ = TypeExtensions.EnterStreamModeBookmark(stream, FileAccess.Read));

			stream.StreamMode = FileAccess.Read;
			AssertThrowsArgument(() => _ = TypeExtensions.EnterStreamModeBookmark(stream, 0), "newMode");
			AssertThrowsArgumentNull(() => _ = new IO.IKSoftStreamOwnerBookmark(null!, null), "stream");
			AssertThrowsArgumentNull(() => _ = new IO.IKSoftStreamUserDataBookmark(null!, null), "stream");
			AssertThrowsArgumentNull(() => _ = new IO.IKSoftStreamModeBookmark(null!, FileAccess.Read), "stream");
			AssertThrowsArgument(() => _ = new IO.IKSoftStreamModeBookmark(stream, 0), "newMode");
		}

		[TestMethod]
		public void TypeExtensions_FromEncodingNull_ThrowsArgumentNullException()
		{
			AssertThrowsArgumentNull(() => _ = TypeExtensions.FromEncoding(null), "enc");
		}

		[TestMethod]
		public void Util_UnixTimeGuards_ThrowArgumentOutOfRangeException()
		{
			AssertThrowsArgumentOutOfRange(() => _ = Util.ConvertDateTimeFromUnixTime(-1), "time_t");
			AssertThrowsArgumentOutOfRange(() => _ = Util.ConvertDateTimeToUnixTime(Util.UnixTimeEpoch.AddTicks(-1)), "value");
		}

		[TestMethod]
		public void Util_CreateComparerGuard_ThrowsArgumentNullException()
		{
			AssertThrowsArgumentNull(() => _ = Util.CreateComparer<string>(null!), "comparer");
		}

		[TestMethod]
		public void Util_MinMaxChoiceReferenceGuards_ThrowArgumentNullException()
		{
			static int GetLength(string value) => value.Length;

			AssertThrowsArgumentNull(() => _ = Util.MinChoice<string>(null!, "rhs", GetLength), "lhs");
			AssertThrowsArgumentNull(() => _ = Util.MinChoice<string>("lhs", null!, GetLength), "rhs");
			AssertThrowsArgumentNull(() => _ = Util.MaxChoice<string>(null!, "rhs", GetLength), "lhs");
			AssertThrowsArgumentNull(() => _ = Util.MaxChoice<string>("lhs", null!, GetLength), "rhs");
		}

		[TestMethod]
		public void Util_GetRelativePathGuards_ThrowArgumentNullException()
		{
			AssertThrowsArgumentNull(() => _ = Util.GetRelativePath(null!, "C:\\"), "fromPath");
			AssertThrowsArgumentNull(() => _ = Util.GetRelativePath(string.Empty, "C:\\"), "fromPath");
			AssertThrowsArgumentNull(() => _ = Util.GetRelativePath("C:\\", null!), "toPath");
			AssertThrowsArgumentNull(() => _ = Util.GetRelativePath("C:\\", string.Empty), "toPath");
		}

		[TestMethod]
		public void TypeExtensions_SystemUtilityGuards_ThrowExpectedExceptions()
		{
			AssertThrowsArgumentNull(() => _ = "{0}".FormatWith(null!, 1), "provider");

			AssertThrowsArgumentNull(() => _ = ((int[])null!).TrueForAny(_ => true), "array");
			AssertThrowsArgumentNull(() => _ = new[] { 1 }.TrueForAny(null!), "match");

			IEnumerable<int> sequence = null!;
			IReadOnlyList<int> list = new[] { 1 };
			AssertThrowsArgumentNull(() => _ = sequence.FindIndex(_ => true), "seq");
			AssertThrowsArgumentNull(() => _ = ((IEnumerable<int>)list).FindIndex(null!), "match");
			AssertThrowsArgumentNull(() => _ = ((IReadOnlyList<int>)null!).FindIndex(0, 0, _ => true), "list");
			AssertThrowsArgumentOutOfRange(() => _ = list.FindIndex(1, 0, _ => true), "startIndex");
			AssertThrowsArgumentOutOfRange(() => _ = list.FindIndex(0, 2, _ => true), "count");
			AssertThrowsArgumentNull(() => _ = list.FindIndex(0, 1, null!), "match");
			AssertThrowsArgumentNull(() => _ = ((IReadOnlyList<int>)null!).FindIndex(0, _ => true), "list");
			AssertThrowsArgumentNull(() => _ = ((IReadOnlyList<int>)null!).FindIndex(_ => true), "list");

			AssertThrowsArgumentNull(() => ((ICollection<int>)null!).EnsureCount(1), "collection");
			AssertThrowsInvalidOperation(() =>
				((ICollection<int>)new ReadOnlyCollection<int>(new List<int>())).EnsureCount(1));
			AssertThrowsArgumentOutOfRange(() => new List<int>().EnsureCount(-1), "requiredCount");

			AssertThrowsArgumentNull(() =>
				_ = ((System.Reflection.ICustomAttributeProvider)null!).GetCustomAttribute<ObsoleteAttribute>(), "provider");
			AssertThrowsArgumentNull(() =>
				_ = ((System.Reflection.ICustomAttributeProvider)null!).GetCustomAttributes<ObsoleteAttribute>(), "provider");
			AssertThrowsArgumentNull(() => _ = ((IEnumerable<string>)null!).OrderBy(v => v, string.CompareOrdinal), "src");
			AssertThrowsArgumentNull(() =>
				_ = ((IEnumerable<string>)null!).OrderByDescending(v => v, string.CompareOrdinal), "src");
		}

		[TestMethod]
		public void TypeExtensions_StreamAndHashGuards_ThrowExpectedExceptions()
		{
			using var nonSeekableStream = new NonSeekableStream([1, 2, 3]);
			using var seekableStream = new MemoryStream([1, 2, 3]);
			using var sha256 = SHA256.Create();
			using var tiger = new Security.Cryptography.TigerHash();

			AssertThrowsInvalidOperation(() => _ = nonSeekableStream.BytesRemaining());
			AssertThrowsInvalidOperation(() => _ = nonSeekableStream.BytesRemaining(1));
			AssertThrowsArgumentOutOfRange(() => _ = seekableStream.BytesRemaining(4), "endPosition");
			AssertThrowsInvalidOperation(() => _ = nonSeekableStream.HasPermissions(FileAccess.Read));

			AssertThrowsArgumentNull(() => _ = sha256.ComputeHash((Stream)null!, 0, 1), "inputStream");
			AssertThrowsArgument(() => _ = sha256.ComputeHash(nonSeekableStream, 0, 1), "inputStream");
			AssertThrowsArgumentOutOfRange(() => _ = sha256.ComputeHash(seekableStream, -2, 1), "offset");
			AssertThrowsArgumentOutOfRange(() => _ = sha256.ComputeHash(seekableStream, 0, -1), "count");
			AssertThrowsArgumentOutOfRange(() => _ = sha256.ComputeHash(seekableStream, 1, 3), "count");

			AssertThrowsArgumentNull(() => _ = tiger.ComputeHash((Stream)null!, 0, 1), "inputStream");
			AssertThrowsArgument(() => _ = tiger.ComputeHash(nonSeekableStream, 0, 1), "inputStream");
			AssertThrowsArgumentOutOfRange(() => _ = tiger.ComputeHash(seekableStream, -2, 1), "offset");
			AssertThrowsArgumentOutOfRange(() => _ = tiger.ComputeHash(seekableStream, 0, -1), "count");
			AssertThrowsArgumentOutOfRange(() => _ = tiger.ComputeHash(seekableStream, 1, 3), "count");
		}
	};
}
