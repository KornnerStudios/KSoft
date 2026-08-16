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

		sealed class EquatableDefault : IEquatable<EquatableDefault>
		{
			public int Value { get; set; }

			public bool Equals(EquatableDefault other) => other != null && Value == other.Value;
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
		public void TypeExtensions_VirtualBufferNullStreamGuards_ThrowArgumentNullException()
		{
			AssertThrowsArgumentNull(() => _ = TypeExtensions.EnterVirtualBuffer(null!, 1), "stream");
			AssertThrowsArgumentNull(() => _ = TypeExtensions.EnterVirtualBuffer(null!), "stream");
			AssertThrowsArgumentNull(() => _ = TypeExtensions.EnterVirtualBufferBookmark(null!), "stream");
			AssertThrowsArgumentNull(() => _ = TypeExtensions.EnterVirtualBufferWithBookmark(null!, 1), "stream");
			AssertThrowsArgumentNull(() => _ = new IO.IKSoftStreamWithVirtualBufferCleanup(null!), "stream");
			AssertThrowsArgumentNull(() => _ = new IO.IKSoftStreamWithVirtualBufferBookmark(null!), "stream");
			AssertThrowsArgumentNull(() => _ = new IO.IKSoftStreamWithVirtualBufferAndBookmark(null!, 1), "stream");
		}

		[TestMethod]
		public void TypeExtensions_VirtualBufferBookmarks_RestoreAndCleanUpState()
		{
			using var stream = new IO.EndianStream(new MemoryStream(new byte[8]));
			AssertThrowsArgument(() => _ = new IO.IKSoftStreamWithVirtualBufferCleanup(stream), "stream");

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
		public void LowLevel_UnmanagedGuards_ThrowArgumentNullException()
		{
			AssertThrowsArgumentNull(() =>
				_ = LowLevel.Util.Unmanaged.IntPtrToStructure(IntPtr.Zero, typeof(int)), "nativePtr");
			AssertThrowsArgumentNull(() =>
				_ = LowLevel.Util.Unmanaged.IntPtrToStructure(new IntPtr(1), null!), "t");
			AssertThrowsArgumentNull(() =>
				_ = LowLevel.Util.Unmanaged.IntPtrToStructure<int>(IntPtr.Zero), "nativePtr");
			AssertThrowsArgumentNull(() =>
				LowLevel.Util.Unmanaged.StructureToPtr(1, IntPtr.Zero), "nativePtr");
			AssertThrowsArgumentNull(() => _ = LowLevel.Util.Unmanaged.New(null!), "t");
			AssertThrowsArgumentNull(() => LowLevel.Util.Unmanaged.Delete(IntPtr.Zero), "nativePtr");
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

			AssertThrowsArgumentNull(() => manager.FromBuffer(null!), "buffer");
			AssertThrowsArgumentOutOfRange(() => manager.FromBuffer(new byte[sizeof(int)], -1), "startIndex");
			AssertThrowsArgumentOutOfRange(() => manager.FromBuffer(new byte[sizeof(int)], 1), "startIndex");
			AssertThrowsArgumentOutOfRange(() => manager.FromBuffer(new byte[sizeof(int) - 1]), "startIndex");
			AssertThrowsArgumentNull(() => manager.ToBuffer(null!), "buffer");
			AssertThrowsArgumentOutOfRange(() => manager.ToBuffer(new byte[sizeof(int)], -1), "startIndex");
			AssertThrowsArgumentOutOfRange(() => manager.ToBuffer(new byte[sizeof(int)], 1), "startIndex");
			AssertThrowsArgumentOutOfRange(() => manager.ToBuffer(new byte[sizeof(int) - 1]), "startIndex");
		}

		[TestMethod]
		public void Util_MinMaxChoiceReferenceGuards_ThrowArgumentNullException()
		{
			static int GetLength(string value) => value.Length;

			AssertThrowsArgumentNull(() => _ = Util.MinChoice<string>(null!, "rhs", GetLength), "lhs");
			AssertThrowsArgumentNull(() => _ = Util.MinChoice<string>("lhs", null!, GetLength), "rhs");
			AssertThrowsArgumentNull(() => _ = Util.MinChoice<string>("lhs", "rhs", null!), "choiceProperty");
			AssertThrowsArgumentNull(() => _ = Util.MaxChoice<string>(null!, "rhs", GetLength), "lhs");
			AssertThrowsArgumentNull(() => _ = Util.MaxChoice<string>("lhs", null!, GetLength), "rhs");
			AssertThrowsArgumentNull(() => _ = Util.MaxChoice<string>("lhs", "rhs", null!), "choiceProperty");
			AssertThrowsArgumentNull(() => _ = Util.MinChoiceValue(1, 2, null!), "choiceProperty");
			AssertThrowsArgumentNull(() => _ = Util.MaxChoiceValue(1, 2, null!), "choiceProperty");
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
			AssertThrowsArgumentOutOfRange(() => _ = "value".ToWideCharBuffer(-1), "maxBufferSize");
			AssertThrowsArgumentOutOfRange(() => _ = "value".ToAsciiCharBuffer(-1), "maxBufferSize");
			Assert.AreEqual(0, "value".ToWideCharBuffer(0).Length);
			Assert.AreEqual(0, "value".ToAsciiCharBuffer(0).Length);
			AssertThrowsArgumentNull(() => _ = TypeExtensions.TransformToString(null!), "list");
			AssertThrowsArgumentNull(() => _ = ((int[])null!).GetGenericEnumerator(), "array");
			AssertThrowsArgumentNull(() => _ = ((int[])null!).EqualsZero(), "array");
			AssertThrowsArgumentNull(() => _ = ((EquatableDefault[])null!).EqualsDefault(), "array");
			AssertThrowsArgumentNull(() => _ = new List<int>().ConvertAllArray<int, int>(null!), "converter");
			Assert.IsNull(TypeExtensions.ConvertAllArray<int, int>(null!, value => value));
			AssertThrowsArgumentNull(() => _ = TypeExtensions.EqualsArray<int>(null!, [1]), "lhs");
			AssertThrowsArgumentNull(() => _ = TypeExtensions.EqualsArray<int>([1], null!), "rhs");
			AssertThrowsArgumentOutOfRange(() => _ = new[] { 1 }.EqualsArray([1], -1), "lhsOffset");
			AssertThrowsArgumentOutOfRange(() => _ = new[] { 1 }.EqualsArray([1], 1), "lhsOffset");
			Assert.IsTrue(new[] { 0, 1, 2 }.EqualsArray([1, 2], 1));
			Assert.IsFalse(new[] { 0, 1, 2 }.EqualsArray([9, 2], 1));
			Assert.IsFalse(new[] { 0, 1, 2 }.EqualsArray([1], 1));
			AssertThrowsArgumentNull(() => _ = TypeExtensions.EqualsList<int>(null!, [1]), "lhs");
			AssertThrowsArgumentNull(() => _ = TypeExtensions.EqualsList<int>([1], null!), "rhs");
			AssertThrowsArgumentOutOfRange(() => _ = new List<int> { 1 }.EqualsList([1], -1), "lhsOffset");
			AssertThrowsArgumentOutOfRange(() => _ = new List<int> { 1 }.EqualsList([1], 1), "lhsOffset");
			Assert.IsTrue(new List<int> { 0, 1, 2 }.EqualsList([1, 2], 1));
			Assert.IsFalse(new List<int> { 0, 1, 2 }.EqualsList([9, 2], 1));
			Assert.IsFalse(new List<int> { 0, 1, 2 }.EqualsList([1], 1));

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
			AssertThrowsArgumentNull(() => _ = ((Type)null!).ImplementsInterface(typeof(IDisposable)), "subject");
			AssertThrowsArgumentNull(() => _ = typeof(MemoryStream).ImplementsInterface(null!), "interfaceType");
			AssertThrowsArgument(() => _ = typeof(MemoryStream).ImplementsInterface(typeof(MemoryStream)), "interfaceType");
			AssertThrowsArgumentNull(() =>
				_ = ((Type)null!).IsCuriouslyRecurringTemplatePattern(typeof(IComparable<>)), "subject");
			AssertThrowsArgumentNull(() =>
				_ = typeof(string).IsCuriouslyRecurringTemplatePattern(null!), "genericType");
			AssertThrowsArgument(() =>
				_ = typeof(string).IsCuriouslyRecurringTemplatePattern(typeof(string)), "genericType");
			AssertThrowsArgument(() =>
				_ = typeof(string).IsCuriouslyRecurringTemplatePattern(typeof(Dictionary<,>)), "genericType");
			AssertThrowsArgumentNull(() =>
				((Type)null!).ForceStaticCtorToRunViaProperty(nameof(Environment.TickCount)), "subject");
			AssertThrowsArgumentNull(() => typeof(Environment).ForceStaticCtorToRunViaProperty(null!), "staticPropertyName");
			AssertThrowsArgument(() => typeof(Environment).ForceStaticCtorToRunViaProperty(string.Empty), "staticPropertyName");
			AssertThrowsArgumentNull(() => _ = ((IEnumerable<string>)null!).OrderBy(v => v, string.CompareOrdinal), "src");
			AssertThrowsArgumentNull(() =>
				_ = TypeExtensions.OrderBy<string, string>(["value"], null!, string.CompareOrdinal), "keySelector");
			AssertThrowsArgumentNull(() =>
				_ = TypeExtensions.OrderBy<string, string>(["value"], v => v, null!), "comparerFunc");
			AssertThrowsArgumentNull(() =>
				_ = ((IEnumerable<string>)null!).OrderByDescending(v => v, string.CompareOrdinal), "src");
			AssertThrowsArgumentNull(() =>
				_ = TypeExtensions.OrderByDescending<string, string>(["value"], null!, string.CompareOrdinal),
				"keySelector");
			AssertThrowsArgumentNull(() =>
				_ = TypeExtensions.OrderByDescending<string, string>(["value"], v => v, null!), "comparerFunc");
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
			CollectionAssert.AreEqual(new[] { "value", "formatted 7" }, values);
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
			AssertThrowsArgumentNull(() =>
				traceSource.TraceDataSansId(System.Diagnostics.TraceEventType.Error, new object[] { 1 }), "source");
			AssertThrowsArgumentNull(() =>
				traceSource.TraceDataSansId(System.Diagnostics.TraceEventType.Error, (object)1), "source");
			AssertThrowsArgumentNull(() => _ = ((Stream)null!).BytesRemaining(), "s");
			AssertThrowsArgumentNull(() => _ = ((Stream)null!).BytesRemaining(0), "s");
			AssertThrowsInvalidOperation(() => _ = nonSeekableStream.BytesRemaining());
			AssertThrowsInvalidOperation(() => _ = nonSeekableStream.BytesRemaining(1));
			AssertThrowsArgumentOutOfRange(() => _ = seekableStream.BytesRemaining(4), "endPosition");
			AssertThrowsArgumentNull(() => _ = ((Stream)null!).HasPermissions(FileAccess.Read), "s");
			AssertThrowsInvalidOperation(() => _ = nonSeekableStream.HasPermissions(FileAccess.Read));
			AssertThrowsArgumentNull(() => _ = ((BinaryReader)null!).PeekByte(), "r");
			AssertThrowsArgumentOutOfRange(() => _ = (-1L).ToFilePositionHexString(), "filePos");

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

			AssertThrowsArgumentNull(() =>
				handler.SafeNotify(this, (System.ComponentModel.PropertyChangedEventArgs[])null!), "argsList");
			AssertThrowsArgumentOutOfRange(() => handler.SafeNotify(this, argsList, -1), "startIndex");
			AssertThrowsArgumentOutOfRange(() => handler.SafeNotify(this, argsList, argsList.Length + 1), "startIndex");
			handler.SafeNotify(this, argsList, 1);
			Assert.AreEqual(1, notifications);
			handler.SafeNotify(this, Array.Empty<System.ComponentModel.PropertyChangedEventArgs>());
			Assert.AreEqual(1, notifications);

			EventHandler<EventArgs> eventToTrigger = null!;
			AssertThrowsArgumentNull(() =>
				_ = eventToTrigger.SafeTrigger<EventArgs, int>(this, EventArgs.Empty, null!), "retrieveDataFunction");
		}

		[TestMethod]
		public void TypeExtensions_ObservableCollectionGuards_ThrowExpectedExceptions()
		{
			AssertThrowsArgumentNull(() => TypeExtensions.AddRange<int>(null!, [1]), "list");
			AssertThrowsArgumentNull(() => new ObservableCollection<int>().AddRange(null!), "collection");
			AssertThrowsArgumentNull(() => _ = TypeExtensions.BinarySearch<int>(null!, 1, null), "list");
			AssertThrowsArgumentNull(() => TypeExtensions.Sort<int>(null!), "list");
			AssertThrowsArgumentNull(() =>
				TypeExtensions.Sort<int>(null!, Comparer<int>.Default), "list");
			AssertThrowsArgumentNull(() =>
				TypeExtensions.Sort<int>(null!, (x, y) => x.CompareTo(y)), "list");
			AssertThrowsArgumentNull(() => TypeExtensions.Reverse<int>(null!), "list");

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
