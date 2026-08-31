using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Test
{
	[TestClass]
	public partial class UtilitiesTest : BaseTestClass
	{

		sealed class NonSeekableStream : MemoryStream
		{
			public NonSeekableStream(byte[] buffer) : base(buffer)
			{
			}

			public override bool CanSeek => false;
		}

		sealed class EquatableDefault : IEquatable<EquatableDefault>
		{
			public int Value { get; set; }

			public bool Equals(EquatableDefault? other) => other != null && Value == other.Value;
			public override bool Equals(object? obj) => obj is EquatableDefault other && Equals(other);
			public override int GetHashCode() => Value.GetHashCode();
		}

		[TestMethod]
		public void UnixTimeTest()
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
		public void GenericReferenceEqualsTest()
		{
			string? x = "x", y = "y";

			Assert.IsTrue(Util.GenericReferenceEquals(x, x));
			Assert.IsTrue(Util.GenericReferenceEquals(x, y) == Util.GenericReferenceEquals(y, x));

			y = null;
			Assert.IsFalse(Util.GenericReferenceEquals(x, y));

			x = null;
			Assert.IsTrue(Util.GenericReferenceEquals(x, y));
		}

		[TestMethod]
		public void NullPreservingHelpers_RetainExistingBehavior()
		{
			var nullExceptionFactory = Util.GetNullException;
			Assert.IsNull(nullExceptionFactory());
			Assert.AreSame(nullExceptionFactory, Util.GetNullException);

			Assert.AreSame(Util.FalseObject, Util.FalseObject);
			Assert.AreEqual(false, Util.FalseObject);
			Assert.AreSame(Util.TrueObject, Util.TrueObject);
			Assert.AreEqual(true, Util.TrueObject);

			int comparerCalls = 0;
			var comparer = Util.CreateComparer<string>((lhs, rhs) =>
			{
				comparerCalls++;
				Assert.IsNull(lhs);
				Assert.IsNull(rhs);
				return 0;
			});
			Assert.AreEqual(0, comparer.Compare(null, null));
			Assert.AreEqual(1, comparerCalls);

			IDisposable? disposable = new MemoryStream();
			Util.DisposeAndNull(ref disposable);
			Assert.IsNull(disposable);

			string[]? array = [];
			Util.ClearAndNull(ref array);
			Assert.IsNull(array);

			var collectionInstance = new List<int> { 1 };
			ICollection<int>? collection = collectionInstance;
			Util.ClearAndNull(ref collection);
			Assert.IsNull(collection);
			Assert.AreEqual(0, collectionInstance.Count);

			string[]? nullArray = null;
			Assert.IsNull(Util.Trim(nullArray));
		}


		[TestMethod]
		public void ThrowIfNullTest()
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

			AssertThrowsArgumentOutOfRange("bufferLength", () => stream.EnterVirtualBuffer(0));
			AssertThrowsArgumentOutOfRange("bufferLength", () => stream.EnterVirtualBuffer(-1));
			AssertThrowsArgumentOutOfRange("bufferLength", () => stream.EnterVirtualBufferWithBookmark(0));
			AssertThrowsArgumentOutOfRange("bufferLength", () => stream.EnterVirtualBufferWithBookmark(-1));
		}

		[TestMethod]
		public void TypeExtensions_VirtualBufferNullStreamGuards_ThrowArgumentNullException()
		{
			AssertThrowsArgumentNull("stream", () => _ = TypeExtensions.EnterVirtualBuffer(null!, 1));
			AssertThrowsArgumentNull("stream", () => _ = TypeExtensions.EnterVirtualBuffer(null!));
			AssertThrowsArgumentNull("stream", () => _ = TypeExtensions.EnterVirtualBufferBookmark(null!));
			AssertThrowsArgumentNull("stream", () => _ = TypeExtensions.EnterVirtualBufferWithBookmark(null!, 1));
			AssertThrowsArgumentNull("stream", () => _ = new IO.IKSoftStreamWithVirtualBufferCleanup(null!));
			AssertThrowsArgumentNull("stream", () => _ = new IO.IKSoftStreamWithVirtualBufferBookmark(null!));
			AssertThrowsArgumentNull("stream", () => _ = new IO.IKSoftStreamWithVirtualBufferAndBookmark(null!, 1));
		}

		[TestMethod]
		public void TypeExtensions_VirtualBufferBookmarks_RestoreAndCleanUpState()
		{
			using var stream = new IO.EndianStream(new MemoryStream(new byte[8]));
			AssertThrowsArgument("stream", () => _ = new IO.IKSoftStreamWithVirtualBufferCleanup(stream));

			using (stream.EnterVirtualBuffer(3))
			{
				Assert.AreEqual(0, stream.VirtualBufferStart);
				Assert.AreEqual(3, stream.VirtualBufferLength);
				Assert.AreEqual(0, stream.BaseStream.Position);
			}
			Assert.AreEqual(3, stream.BaseStream.Position);
			Assert.AreEqual(0, stream.VirtualBufferStart);
			Assert.AreEqual(0, stream.VirtualBufferLength);

			stream.VirtualBufferStart = 1;
			stream.VirtualBufferLength = 2;
			using (stream.EnterVirtualBufferBookmark())
			{
				stream.VirtualBufferStart = 4;
				stream.VirtualBufferLength = 5;
			}
			Assert.AreEqual(1, stream.VirtualBufferStart);
			Assert.AreEqual(2, stream.VirtualBufferLength);
		}

		[TestMethod]
		public void TypeExtensions_StreamBookmarkGuards_ThrowExpectedExceptions()
		{
			using var stream = new IO.EndianStream(new MemoryStream());

			AssertThrowsArgumentNull("func", () => _ = TypeExtensions.NullOr<string, int>("value", null!));
			AssertThrowsArgumentNull("stream", () => _ = TypeExtensions.EnterStreamModeBookmark(null!, FileAccess.Read));
			Assert.ThrowsExactly<InvalidOperationException>(() => _ = TypeExtensions.EnterStreamModeBookmark(stream, FileAccess.Read));

			stream.StreamMode = FileAccess.Read;
			AssertThrowsArgument("newMode", () => _ = TypeExtensions.EnterStreamModeBookmark(stream, 0));
			AssertThrowsArgumentNull("stream", () => _ = new IO.IKSoftStreamOwnerBookmark(null!, null));
			AssertThrowsArgumentNull("stream", () => _ = new IO.IKSoftStreamUserDataBookmark(null!, null));
			AssertThrowsArgumentNull("stream", () => _ = new IO.IKSoftStreamModeBookmark(null!, FileAccess.Read));
			AssertThrowsArgument("newMode", () => _ = new IO.IKSoftStreamModeBookmark(stream, 0));
		}

		[TestMethod]
		public void TypeExtensions_FromEncodingNull_ThrowsArgumentNullException()
		{
			AssertThrowsArgumentNull("enc", () => _ = TypeExtensions.FromEncoding(null!));
		}

		[TestMethod]
		public void UnixTimeGuards_ThrowArgumentOutOfRangeException()
		{
			AssertThrowsArgumentOutOfRange("time_t", () => _ = Util.ConvertDateTimeFromUnixTime(-1));
			AssertThrowsArgumentOutOfRange("value", () => _ = Util.ConvertDateTimeToUnixTime(Util.UnixTimeEpoch.AddTicks(-1)));
		}

		[TestMethod]
		public void CreateComparerGuard_ThrowsArgumentNullException()
		{
			AssertThrowsArgumentNull("comparer", () => _ = Util.CreateComparer<string>(null!));
		}

		[TestMethod]
		public void LowLevel_UnmanagedGuards_ThrowArgumentNullException()
		{
			AssertThrowsArgumentNull("nativePtr",
				() =>
				_ = LowLevel.Util.Unmanaged.IntPtrToStructure<int>(IntPtr.Zero));
			AssertThrowsArgumentNull("t",
				() =>
				_ = LowLevel.Util.Unmanaged.IntPtrToStructure(new IntPtr(1), null!));
			AssertThrowsArgumentNull("nativePtr",
				() =>
				_ = LowLevel.Util.Unmanaged.IntPtrToStructure<int>(IntPtr.Zero));
			AssertThrowsArgumentNull("nativePtr",
				() =>
				LowLevel.Util.Unmanaged.StructureToPtr(1, IntPtr.Zero));
			AssertThrowsArgumentNull("t", () => _ = LowLevel.Util.Unmanaged.New(null!));
			AssertThrowsArgumentNull("nativePtr", () => LowLevel.Util.Unmanaged.Delete(IntPtr.Zero));
		}

		[TestMethod]
		public void LowLevel_StructBitManagerBufferRange_HandlesExactFitAndOffset()
		{
			byte[] exactBuffer = BitConverter.GetBytes(0x01020304);
			using var manager = new LowLevel.Util.StructBitManager<int>();
			manager.FromBuffer(exactBuffer);

			Assert.AreEqual(0x01020304, manager.ToValue());

			byte[] offsetBuffer = new byte[sizeof(int) + 4];
			manager.ToBuffer(offsetBuffer, 2);
			CollectionAssert.AreEqual(exactBuffer, offsetBuffer[2..(2 + sizeof(int))]);
		}

		[TestMethod]
		public void LowLevel_StructBitManagerInvalidBufferRange_ThrowsExpectedExceptions()
		{
			using var manager = new LowLevel.Util.StructBitManager<int>();

			AssertThrowsArgumentNull("buffer", () => manager.FromBuffer(null!));
			AssertThrowsArgumentOutOfRange("startIndex", () => manager.FromBuffer(new byte[sizeof(int)], -1));
			AssertThrowsArgumentOutOfRange("startIndex", () => manager.FromBuffer(new byte[sizeof(int)], 1));
			AssertThrowsArgumentOutOfRange("startIndex", () => manager.FromBuffer(new byte[sizeof(int) - 1]));
			AssertThrowsArgumentNull("buffer", () => manager.ToBuffer(null!));
			AssertThrowsArgumentOutOfRange("startIndex", () => manager.ToBuffer(new byte[sizeof(int)], -1));
			AssertThrowsArgumentOutOfRange("startIndex", () => manager.ToBuffer(new byte[sizeof(int)], 1));
			AssertThrowsArgumentOutOfRange("startIndex", () => manager.ToBuffer(new byte[sizeof(int) - 1]));
		}

		[TestMethod]
		public void MinMaxChoiceReferenceGuards_ThrowArgumentNullException()
		{
			static int GetLength(string value) => value.Length;

			AssertThrowsArgumentNull("lhs", () => _ = Util.MinChoice<string>(null!, "rhs", GetLength));
			AssertThrowsArgumentNull("rhs", () => _ = Util.MinChoice<string>("lhs", null!, GetLength));
			AssertThrowsArgumentNull("choiceProperty", () => _ = Util.MinChoice<string>("lhs", "rhs", null!));
			AssertThrowsArgumentNull("lhs", () => _ = Util.MaxChoice<string>(null!, "rhs", GetLength));
			AssertThrowsArgumentNull("rhs", () => _ = Util.MaxChoice<string>("lhs", null!, GetLength));
			AssertThrowsArgumentNull("choiceProperty", () => _ = Util.MaxChoice<string>("lhs", "rhs", null!));
			AssertThrowsArgumentNull("choiceProperty", () => _ = Util.MinChoiceValue(1, 2, null!));
			AssertThrowsArgumentNull("choiceProperty", () => _ = Util.MaxChoiceValue(1, 2, null!));
		}

		[TestMethod]
		public void GetRelativePathGuards_ThrowArgumentNullException()
		{
			AssertThrowsArgumentNull("fromPath", () => _ = Util.GetRelativePath(null!, "C:\\"));
			AssertThrowsArgumentNull("fromPath", () => _ = Util.GetRelativePath(string.Empty, "C:\\"));
			AssertThrowsArgumentNull("toPath", () => _ = Util.GetRelativePath("C:\\", null!));
			AssertThrowsArgumentNull("toPath", () => _ = Util.GetRelativePath("C:\\", string.Empty));
		}

		[TestMethod]
		public void PathHelpers_PreserveNullEmptyAndNormalizeSeparators()
		{
			string nullPath = null!;
			string separator = Path.DirectorySeparatorChar.ToString();
			string altSeparator = Path.AltDirectorySeparatorChar.ToString();

			Assert.IsNull(Util.AppendDirectorySeparatorChar(nullPath));
			Assert.AreEqual(string.Empty, Util.AppendDirectorySeparatorChar(string.Empty));
			Assert.AreEqual("file.txt", Util.AppendDirectorySeparatorChar("file.txt"));
			Assert.AreEqual("folder" + separator, Util.AppendDirectorySeparatorChar("folder"));

			Assert.IsNull(Util.PrependDirectorySeparatorChar(nullPath));
			Assert.AreEqual(string.Empty, Util.PrependDirectorySeparatorChar(string.Empty));
			Assert.AreEqual(separator + "folder", Util.PrependDirectorySeparatorChar("folder"));
			Assert.AreEqual(separator + "folder", Util.PrependDirectorySeparatorChar(separator + "folder"));

			Assert.IsNull(Util.RemoveTrailingDirectorySeparatorChar(nullPath));
			Assert.AreEqual(string.Empty, Util.RemoveTrailingDirectorySeparatorChar(string.Empty));
			Assert.AreEqual("folder", Util.RemoveTrailingDirectorySeparatorChar("folder" + separator));

			Assert.IsNull(Util.ReplaceDirectorySeparatorWithAltChar(nullPath));
			Assert.AreEqual(string.Empty, Util.ReplaceDirectorySeparatorWithAltChar(string.Empty));
			Assert.AreEqual("a" + altSeparator + "b", Util.ReplaceDirectorySeparatorWithAltChar("a" + separator + "b"));

			Assert.IsNull(Util.ReplaceAltDirectorySeparatorWithNormalChar(nullPath));
			Assert.AreEqual(string.Empty, Util.ReplaceAltDirectorySeparatorWithNormalChar(string.Empty));
			Assert.AreEqual("a" + separator + "b",
				Util.ReplaceAltDirectorySeparatorWithNormalChar("a" + altSeparator + "b"));
		}

		[TestMethod]
		public void TypeExtensions_ProcessorSizeHelpers_ReturnDocumentedRanges()
		{
			Assert.AreEqual(-1, TypeExtensions.GetBitCount(Shell.ProcessorSize.AnyCPU));
			Assert.AreEqual(Bits.kInt32BitCount, TypeExtensions.GetBitCount(Shell.ProcessorSize.x32));
			Assert.AreEqual(Bits.kInt64BitCount, TypeExtensions.GetBitCount(Shell.ProcessorSize.x64));

			Assert.AreEqual(-1, TypeExtensions.GetByteCount(Shell.ProcessorSize.AnyCPU));
			Assert.AreEqual(sizeof(int), TypeExtensions.GetByteCount(Shell.ProcessorSize.x32));
			Assert.AreEqual(sizeof(long), TypeExtensions.GetByteCount(Shell.ProcessorSize.x64));

			Assert.IsTrue(TypeExtensions.GetBitCount(Shell.ProcessorWordSize.x8) > 0);
			Assert.IsTrue(TypeExtensions.GetBitCount(Shell.ProcessorWordSize.x64) > 0);
			Assert.IsTrue(TypeExtensions.GetByteCount(Shell.ProcessorWordSize.x8) > 0);
			Assert.IsTrue(TypeExtensions.GetByteCount(Shell.ProcessorWordSize.x64) > 0);
		}

		[TestMethod]
		public void TypeExtensions_SystemUtilityGuards_ThrowExpectedExceptions()
		{
			AssertThrowsArgumentNull("provider", () => _ = "{0}".FormatWith(null!, 1));
			AssertThrowsArgumentOutOfRange("maxBufferSize", () => _ = "value".ToWideCharBuffer(-1));
			AssertThrowsArgumentOutOfRange("maxBufferSize", () => _ = "value".ToAsciiCharBuffer(-1));
			Assert.AreEqual(0, "value".ToWideCharBuffer(0).Length);
			Assert.AreEqual(0, "value".ToAsciiCharBuffer(0).Length);
			AssertThrowsArgumentNull("list", () => _ = TypeExtensions.TransformToString(null!));
			AssertThrowsArgumentNull("array", () => _ = ((int[])null!).GetGenericEnumerator());
			AssertThrowsArgumentNull("array", () => _ = ((int[])null!).EqualsZero());
			AssertThrowsArgumentNull("array", () => _ = ((EquatableDefault[])null!).EqualsDefault());
			AssertThrowsArgumentNull("converter", () => _ = new List<int>().ConvertAllArray<int, int>(null!));
			Assert.IsNull(TypeExtensions.ConvertAllArray<int, int>(null!, value => value));
			AssertThrowsArgumentNull("lhs", () => _ = TypeExtensions.EqualsArray<int>(null!, [1]));
			AssertThrowsArgumentNull("rhs", () => _ = TypeExtensions.EqualsArray<int>([1], null!));
			AssertThrowsArgumentOutOfRange("lhsOffset", () => _ = new[] { 1 }.EqualsArray([1], -1));
			AssertThrowsArgumentOutOfRange("lhsOffset", () => _ = new[] { 1 }.EqualsArray([1], 1));
			Assert.IsTrue(new[] { 0, 1, 2 }.EqualsArray([1, 2], 1));
			Assert.IsFalse(new[] { 0, 1, 2 }.EqualsArray([9, 2], 1));
			Assert.IsFalse(new[] { 0, 1, 2 }.EqualsArray([1], 1));
			AssertThrowsArgumentNull("lhs", () => _ = TypeExtensions.EqualsList<int>(null!, [1]));
			AssertThrowsArgumentNull("rhs", () => _ = TypeExtensions.EqualsList<int>([1], null!));
			AssertThrowsArgumentOutOfRange("lhsOffset", () => _ = new List<int> { 1 }.EqualsList([1], -1));
			AssertThrowsArgumentOutOfRange("lhsOffset", () => _ = new List<int> { 1 }.EqualsList([1], 1));
			Assert.IsTrue(new List<int> { 0, 1, 2 }.EqualsList([1, 2], 1));
			Assert.IsFalse(new List<int> { 0, 1, 2 }.EqualsList([9, 2], 1));
			Assert.IsFalse(new List<int> { 0, 1, 2 }.EqualsList([1], 1));

			AssertThrowsArgumentNull("array", () => _ = ((int[])null!).TrueForAny(_ => true));
			AssertThrowsArgumentNull("match", () => _ = new[] { 1 }.TrueForAny(null!));

			IEnumerable<int> sequence = null!;
			IReadOnlyList<int> list = new[] { 1 };
			AssertThrowsArgumentNull("seq", () => _ = sequence.FindIndex(_ => true));
			AssertThrowsArgumentNull("match", () => _ = ((IEnumerable<int>)list).FindIndex(null!));
			AssertThrowsArgumentNull("list", () => _ = ((IReadOnlyList<int>)null!).FindIndex(0, 0, _ => true));
			AssertThrowsArgumentOutOfRange("startIndex", () => _ = list.FindIndex(1, 0, _ => true));
			AssertThrowsArgumentOutOfRange("count", () => _ = list.FindIndex(0, 2, _ => true));
			AssertThrowsArgumentNull("match", () => _ = list.FindIndex(0, 1, null!));
			AssertThrowsArgumentNull("list", () => _ = ((IReadOnlyList<int>)null!).FindIndex(0, _ => true));
			AssertThrowsArgumentNull("list", () => _ = ((IReadOnlyList<int>)null!).FindIndex(_ => true));

			AssertThrowsArgumentNull("collection", () => ((ICollection<int>)null!).EnsureCount(1));
			Assert.ThrowsExactly<InvalidOperationException>(() =>
				((ICollection<int>)new ReadOnlyCollection<int>(new List<int>())).EnsureCount(1));
			AssertThrowsArgumentOutOfRange("requiredCount", () => new List<int>().EnsureCount(-1));

			AssertThrowsArgumentNull("provider",
				() =>
				_ = ((System.Reflection.ICustomAttributeProvider)null!).GetCustomAttribute<ObsoleteAttribute>());
			AssertThrowsArgumentNull("provider",
				() =>
				_ = ((System.Reflection.ICustomAttributeProvider)null!).GetCustomAttributes<ObsoleteAttribute>());
			AssertThrowsArgumentNull("subject", () => _ = ((Type)null!).ImplementsInterface(typeof(IDisposable)));
			AssertThrowsArgumentNull("interfaceType", () => _ = typeof(MemoryStream).ImplementsInterface(null!));
			AssertThrowsArgument("interfaceType", () => _ = typeof(MemoryStream).ImplementsInterface(typeof(MemoryStream)));
			AssertThrowsArgumentNull("subject",
				() =>
				_ = ((Type)null!).IsCuriouslyRecurringTemplatePattern(typeof(IComparable<>)));
			AssertThrowsArgumentNull("genericType",
				() =>
				_ = typeof(string).IsCuriouslyRecurringTemplatePattern(null!));
			AssertThrowsArgument("genericType",
				() =>
				_ = typeof(string).IsCuriouslyRecurringTemplatePattern(typeof(string)));
			AssertThrowsArgument("genericType",
				() =>
				_ = typeof(string).IsCuriouslyRecurringTemplatePattern(typeof(Dictionary<,>)));
			AssertThrowsArgumentNull("subject",
				() =>
				((Type)null!).ForceStaticCtorToRunViaProperty(nameof(Environment.TickCount)));
			AssertThrowsArgumentNull("staticPropertyName", () => typeof(Environment).ForceStaticCtorToRunViaProperty(null!));
			AssertThrowsArgument("staticPropertyName", () => typeof(Environment).ForceStaticCtorToRunViaProperty(string.Empty));
			AssertThrowsArgumentNull("src", () => _ = ((IEnumerable<string>)null!).OrderBy(v => v, string.CompareOrdinal));
			AssertThrowsArgumentNull("keySelector",
				() =>
				_ = TypeExtensions.OrderBy<string, string>(["value"], null!, string.CompareOrdinal));
			AssertThrowsArgumentNull("comparerFunc",
				() =>
				_ = TypeExtensions.OrderBy<string, string>(["value"], v => v, null!));
			AssertThrowsArgumentNull("src",
				() =>
				_ = ((IEnumerable<string>)null!).OrderByDescending(v => v, string.CompareOrdinal));
			AssertThrowsArgumentNull("keySelector",
				() =>
				_ = TypeExtensions.OrderByDescending<string, string>(["value"], null!, string.CompareOrdinal));
			AssertThrowsArgumentNull("comparerFunc",
				() =>
				_ = TypeExtensions.OrderByDescending<string, string>(["value"], v => v, null!));
		}

		[TestMethod]
		public void TypeExtensions_SystemPostconditionBehavior_ReturnsExpectedValues()
		{
			string nullString = null!;
			Assert.AreEqual(0, nullString.GetDeterministicHashCode());
			Assert.AreEqual(0, string.Empty.GetDeterministicHashCode());

			CollectionAssert.AreEqual(new[] { 'a', 'b', '\0' }, "abc".ToWideCharBuffer(3));
			CollectionAssert.AreEqual(new byte[] { (byte)'a', (byte)'b', 0 }, "abc".ToAsciiCharBuffer(3));

			var values = new List<string>();
			Assert.AreEqual("value", values.AddFormat("value"));
			Assert.AreEqual("formatted 7", values.AddFormat("formatted {0}", 7));
			Assert.AreEqual("formatted 7.5", values.AddFormatWith(System.Globalization.CultureInfo.InvariantCulture, "formatted {0}", 7.5));
			CollectionAssert.AreEqual(new[] { "value", "formatted 7", "formatted 7.5" }, values);
			Assert.IsNull(((ICollection<string>)new ReadOnlyCollection<string>(values)).AddFormat("ignored"));

			Assert.AreEqual(string.Empty, TypeExtensions.ArrayToConcatString(null));
			Assert.AreEqual("1|2", new[] { 1, 2 }.ArrayToConcatString("|"));

			IReadOnlyList<int> list = new[] { 10, 20, 30 };
			Assert.AreEqual(2, list.FindIndex(1, 2, value => value == 30));
			Assert.AreEqual(TypeExtensions.kNone, list.FindIndex(value => value == 99));

			Assert.AreEqual(string.Empty, TypeExtensions.ToConcatString<int>(null));
			Assert.AreEqual("1|2", new[] { 1, 2 }.ToConcatString("|"));
			Assert.AreEqual("1|0", new[] { true, false }.ToConcatBinaryString("|"));
			Assert.AreEqual("true|false", new[] { true, false }.ToConcatLowerString("|"));
			Assert.AreEqual("1.5|2.5", new[] { 1.5f, 2.5f }.ToConcatStringInvariant("|"));
			Assert.AreEqual("1.5|2.5", new[] { 1.5, 2.5 }.ToConcatStringInvariant("|"));

			var intComparer = Comparer<int>.Default;
			CollectionAssert.AreEqual(new[] { "a", "bb" },
				TypeExtensions.OrderBy(["bb", "a"], value => value.Length, intComparer.Compare).ToArray());
			CollectionAssert.AreEqual(new[] { "bb", "a" },
				TypeExtensions.OrderByDescending(["bb", "a"], value => value.Length, intComparer.Compare).ToArray());
		}

		[TestMethod]
		public void TypeExtensions_StreamAndHashGuards_ThrowExpectedExceptions()
		{
			using var nonSeekableStream = new NonSeekableStream([1, 2, 3]);
			using var seekableStream = new MemoryStream([1, 2, 3]);
			using var sha256 = SHA256.Create();
			using var tiger = new Security.Cryptography.TigerHash();

			System.Diagnostics.TraceSource traceSource = null!;
			AssertThrowsArgumentNull("source",
				() =>
				traceSource.TraceDataSansId(System.Diagnostics.TraceEventType.Error, new object[] { 1 }));
			AssertThrowsArgumentNull("source",
				() =>
				traceSource.TraceDataSansId(System.Diagnostics.TraceEventType.Error, (object)1));
			AssertThrowsArgumentNull("s", () => _ = ((Stream)null!).BytesRemaining());
			AssertThrowsArgumentNull("s", () => _ = ((Stream)null!).BytesRemaining(0));
			Assert.ThrowsExactly<InvalidOperationException>(() => _ = nonSeekableStream.BytesRemaining());
			Assert.ThrowsExactly<InvalidOperationException>(() => _ = nonSeekableStream.BytesRemaining(1));
			AssertThrowsArgumentOutOfRange("endPosition", () => _ = seekableStream.BytesRemaining(4));
			AssertThrowsArgumentNull("s", () => _ = ((Stream)null!).HasPermissions(FileAccess.Read));
			Assert.ThrowsExactly<InvalidOperationException>(() => _ = nonSeekableStream.HasPermissions(FileAccess.Read));
			AssertThrowsArgumentNull("r", () => _ = ((BinaryReader)null!).PeekByte());
			AssertThrowsArgumentOutOfRange("filePos", () => _ = (-1L).ToFilePositionHexString());

			AssertThrowsArgumentNull("inputStream", () => _ = sha256.ComputeHash((Stream)null!, 0, 1));
			AssertThrowsArgument("inputStream", () => _ = sha256.ComputeHash(nonSeekableStream, 0, 1));
			AssertThrowsArgumentOutOfRange("offset", () => _ = sha256.ComputeHash(seekableStream, -2, 1));
			AssertThrowsArgumentOutOfRange("count", () => _ = sha256.ComputeHash(seekableStream, 0, -1));
			AssertThrowsArgumentOutOfRange("count", () => _ = sha256.ComputeHash(seekableStream, 1, 3));

			AssertThrowsArgumentNull("inputStream", () => _ = tiger.ComputeHash((Stream)null!, 0, 1));
			AssertThrowsArgument("inputStream", () => _ = tiger.ComputeHash(nonSeekableStream, 0, 1));
			AssertThrowsArgumentOutOfRange("offset", () => _ = tiger.ComputeHash(seekableStream, -2, 1));
			AssertThrowsArgumentOutOfRange("count", () => _ = tiger.ComputeHash(seekableStream, 0, -1));
			AssertThrowsArgumentOutOfRange("count", () => _ = tiger.ComputeHash(seekableStream, 1, 3));
		}

		[TestMethod]
		public void TypeExtensions_EventHandlerGuards_ThrowExpectedExceptions()
		{
			var argsList = new[]
			{
				new System.ComponentModel.PropertyChangedEventArgs("One"),
				new System.ComponentModel.PropertyChangedEventArgs("Two"),
			};
			int notifications = 0;
			System.ComponentModel.PropertyChangedEventHandler handler = (_, _) => notifications++;

			AssertThrowsArgumentNull("argsList",
				() =>
				handler.SafeNotify(this, (System.ComponentModel.PropertyChangedEventArgs[])null!));
			AssertThrowsArgumentOutOfRange("startIndex", () => handler.SafeNotify(this, argsList, -1));
			AssertThrowsArgumentOutOfRange("startIndex", () => handler.SafeNotify(this, argsList, argsList.Length + 1));
			handler.SafeNotify(this, argsList, 1);
			Assert.AreEqual(1, notifications);
			handler.SafeNotify(this, Array.Empty<System.ComponentModel.PropertyChangedEventArgs>());
			Assert.AreEqual(1, notifications);

			EventHandler<EventArgs> eventToTrigger = null!;
			AssertThrowsArgumentNull("retrieveDataFunction",
				() =>
				_ = eventToTrigger.SafeTrigger<EventArgs, int>(this, EventArgs.Empty, null!));
		}

		[TestMethod]
		public void TypeExtensions_ObservableCollectionGuards_ThrowExpectedExceptions()
		{
			AssertThrowsArgumentNull("list", () => TypeExtensions.AddRange<int>(null!, [1]));
			AssertThrowsArgumentNull("collection", () => new ObservableCollection<int>().AddRange(null!));
			AssertThrowsArgumentNull("list", () => _ = TypeExtensions.BinarySearch<int>(null!, 1, null!));
			AssertThrowsArgumentNull("list", () => TypeExtensions.Sort<int>(null!));
			AssertThrowsArgumentNull("list",
				() =>
				TypeExtensions.Sort<int>(null!, Comparer<int>.Default));
			AssertThrowsArgumentNull("list",
				() =>
				TypeExtensions.Sort<int>(null!, (x, y) => x.CompareTo(y)));
			AssertThrowsArgumentNull("list", () => TypeExtensions.Reverse<int>(null!));

			var collection = new ObservableCollection<int> { 3, 1 };
			collection.AddRange([2]);
			CollectionAssert.AreEqual(new List<int> { 3, 1, 2 }, new List<int>(collection));

			collection.Sort();
			CollectionAssert.AreEqual(new List<int> { 1, 2, 3 }, new List<int>(collection));
			Assert.AreEqual(1, collection.BinarySearch(2, Comparer<int>.Default));

			collection.Sort((x, y) => y.CompareTo(x));
			CollectionAssert.AreEqual(new List<int> { 3, 2, 1 }, new List<int>(collection));
			collection.Reverse();
			CollectionAssert.AreEqual(new List<int> { 1, 2, 3 }, new List<int>(collection));
		}
	};
}
