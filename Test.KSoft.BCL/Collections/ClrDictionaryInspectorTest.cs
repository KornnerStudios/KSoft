using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Collections.Test
{
	[TestClass]
	public class ClrDictionaryInspectorTest : BaseTestClass
	{
		// Forces every key that's compared through it into the same Dictionary bucket chain
		// by always returning the same (possibly negative) hash code, regardless of input.
		sealed class FixedHashComparer : IEqualityComparer<string>
		{
			readonly int mHashCode;

			public FixedHashComparer(int hashCode)
			{
				mHashCode = hashCode;
			}

			public bool Equals(string? x, string? y) => string.Equals(x, y, StringComparison.Ordinal);
			public int GetHashCode(string obj) => mHashCode;
		};

		static Dictionary<int, int> NewNonCollidingIntDictionary(int count)
		{
			// int.GetHashCode() returns itself, and the BCL sizes the bucket table larger
			// than count, so sequential small ints never collide into the same bucket.
			var dic = new Dictionary<int, int>(count);
			for (int x = 0; x < count; x++)
				dic.Add(x, x);

			return dic;
		}

		[TestMethod]
		public void BucketsInUse_NeverPopulatedDictionary_ReturnsEmpty()
		{
			var dic = new Dictionary<int, int>();
			var inspector = new ClrDictionaryInspector<int, int>(dic);

			Assert.AreEqual(0, inspector.Buckets.Count);
			Assert.AreEqual(0, inspector.BucketsInUse.Count());
			Assert.AreEqual(0, inspector.Count);

			// A never-populated dictionary has no internal _buckets/_entries arrays; this must
			// not leak the BCL's NullReferenceException and must simply report no collisions.
			Assert.AreEqual(0, inspector.EntryCollisions(0).Count());
		}

		[TestMethod]
		public void BucketsInUse_NonCollidingSequentialIntKeys_MatchesDictionaryCount()
		{
			var dic = NewNonCollidingIntDictionary(16);
			var inspector = new ClrDictionaryInspector<int, int>(dic);

			Assert.AreEqual(dic.Count, inspector.BucketsInUse.Count());
			Assert.IsTrue(inspector.BucketsInUse.Count() <= inspector.Buckets.Count);
		}

		[TestMethod]
		public void GetEntriesInBucket_SumAcrossAllBuckets_EqualsDictionaryCount()
		{
			var dic = NewNonCollidingIntDictionary(16);
			var inspector = new ClrDictionaryInspector<int, int>(dic);

			int total_entries = Enumerable.Range(0, inspector.Buckets.Count)
				.Sum(b => inspector.GetEntriesInBucket(b).Count());

			Assert.AreEqual(dic.Count, total_entries);
		}

		[TestMethod]
		public void GetEntriesInBucket_NonCollidingKey_ReturnsSingleEntryWithMatchingKeyAndHashCode()
		{
			var dic = NewNonCollidingIntDictionary(16);
			var inspector = new ClrDictionaryInspector<int, int>(dic);

			foreach (int key in dic.Keys)
			{
				var owning_bucket_chain = Enumerable.Range(0, inspector.Buckets.Count)
					.Select(b => inspector.GetEntriesInBucket(b).ToArray())
					.Single(chain => chain.Any(e => e.Key == key));

				Assert.HasCount(1, owning_bucket_chain);
				Assert.AreEqual(key, owning_bucket_chain[0].Key);
				Assert.AreEqual((uint)EqualityComparer<int>.Default.GetHashCode(key), owning_bucket_chain[0].HashCode);
			}
		}

		[TestMethod]
		public void EntryCollisions_NonCollidingKey_ReturnsEmpty()
		{
			var dic = NewNonCollidingIntDictionary(16);
			var inspector = new ClrDictionaryInspector<int, int>(dic);

			foreach (int key in dic.Keys)
				Assert.AreEqual(0, inspector.EntryCollisions(key).Count());
		}

		[TestMethod]
		public void EntryCollisions_ForcedCollisionViaCustomComparer_ReturnsOtherCollidingEntriesOnly()
		{
			var comparer = new FixedHashComparer(42);
			var dic = new Dictionary<string, string>(comparer) {
				["a"] = "a",
				["b"] = "b",
				["c"] = "c",
			};
			var inspector = new ClrDictionaryInspector<string, string>(dic);

			foreach (string key in dic.Keys)
			{
				var expected_others = new HashSet<string>(dic.Keys.Where(k => k != key));
				var actual_others = inspector.EntryCollisions(key).ToArray();

				Assert.HasCount(expected_others.Count, actual_others);
				Assert.IsTrue(expected_others.SetEquals(actual_others.Select(e => e.Key)));
			}
		}

		[TestMethod]
		public void EntryCollisions_NegativeHashCodeKeys_MatchUnmaskedStoredHashCode()
		{
			const int k_negative_hash_code = -12345;
			var comparer = new FixedHashComparer(k_negative_hash_code);
			var dic = new Dictionary<string, string>(comparer) {
				["x"] = "x",
				["y"] = "y",
			};
			var inspector = new ClrDictionaryInspector<string, string>(dic);
			var expected_hash_code = unchecked((uint)k_negative_hash_code);

			var x_collisions = inspector.EntryCollisions("x").ToArray();
			Assert.HasCount(1, x_collisions);
			Assert.AreEqual("y", x_collisions[0].Key);
			Assert.AreEqual(expected_hash_code, x_collisions[0].HashCode);

			var y_collisions = inspector.EntryCollisions("y").ToArray();
			Assert.HasCount(1, y_collisions);
			Assert.AreEqual("x", y_collisions[0].Key);
			Assert.AreEqual(expected_hash_code, y_collisions[0].HashCode);
		}

		[TestMethod]
		public void EntryCollisions_AfterRemoval_ExcludesRemovedEntryAndReflectsFreeSlotReuse()
		{
			var comparer = new FixedHashComparer(42);
			var dic = new Dictionary<string, string>(comparer) {
				["a"] = "a",
				["b"] = "b",
				["c"] = "c",
			};

			// Each inspector snapshot is version-locked to the dictionary at its own
			// construction time, so a fresh inspector is created after each mutation below.
			var before_removal = new ClrDictionaryInspector<string, string>(dic)
				.EntryCollisions("a").ToArray();
			Assert.HasCount(2, before_removal);
			Assert.IsTrue(new HashSet<string>(before_removal.Select(e => e.Key)).SetEquals(new[] { "b", "c" }));

			dic.Remove("b");

			var after_removal = new ClrDictionaryInspector<string, string>(dic)
				.EntryCollisions("a").ToArray();
			Assert.HasCount(1, after_removal);
			Assert.IsTrue(new HashSet<string>(after_removal.Select(e => e.Key)).SetEquals(new[] { "c" }));

			dic["d"] = "d"; // reuses "b"'s freed slot

			var after_readd = new ClrDictionaryInspector<string, string>(dic)
				.EntryCollisions("a").ToDictionary(e => e.Key, e => e.Value);
			Assert.IsTrue(new HashSet<string>(after_readd.Keys).SetEquals(new[] { "c", "d" }));
			Assert.AreEqual("d", after_readd["d"]);
		}

		[TestMethod]
		public void ConstructorNull_ThrowsArgumentNullException()
		{
			AssertThrowsArgumentNull("dic", () => _ = new ClrDictionaryInspector<int, int>(null!));
		}

		[TestMethod]
		public void DicEntryGetNextGuards_ThrowExpectedExceptions()
		{
			var last = new ClrDictionaryInspector<int, int>.DicEntry { NextEntryIndex = TypeExtensions.kNone };
			var inspector = new ClrDictionaryInspector<int, int>(new Dictionary<int, int>());

			AssertThrowsArgumentNull("inspector", () => _ = last.GetNext(null!));
			Assert.ThrowsExactly<InvalidOperationException>(() => _ = last.GetNext(inspector));
		}

		[TestMethod]
		public void GetEntriesInBucketGuards_ThrowArgumentOutOfRangeException()
		{
			var inspector = new ClrDictionaryInspector<int, int>(new Dictionary<int, int>());

			AssertThrowsArgumentOutOfRange("bucketIndex", () => inspector.GetEntriesInBucket(-1).ToArray());
			AssertThrowsArgumentOutOfRange("bucketIndex", () => inspector.GetEntriesInBucket(0).ToArray());
		}
	};
}
