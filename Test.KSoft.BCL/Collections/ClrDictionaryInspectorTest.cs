using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Collections.Test
{
	[TestClass]
	public class ClrDictionaryInspectorTest : BaseTestClass
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

		[TestMethod]
		public void IntTest()
		{
			const int k_initial_capacity = 16; // will result in a dictionary initially sized to 17 (prime) buckets

			var dic = new Dictionary<int, int>(k_initial_capacity);

			for (int x = 0; x < k_initial_capacity; x++)
			{
				Console.Write("{0}={1}, ", x, x.GetHashCode());
				dic.Add(x, x);
			}
			Console.WriteLine();

			var dic_inspector = new ClrDictionaryInspector<int, int>(dic);
			Console.WriteLine("Buckets: {0} out of {1} in use",
				dic_inspector.BucketsInUse.Count(), dic_inspector.Buckets.Count);
			for (int x = 0; x < k_initial_capacity; x++)
			{
				var collisions = dic_inspector.EntryCollisions(x);
				int collision_count = collisions.Count();
				if (collision_count > 0)
				{
					Console.WriteLine("{0} had {1} collisions before it was added",
						x, collision_count);
				}
			}
		}

		[TestMethod]
		public void ConstructorNull_ThrowsArgumentNullException()
		{
			AssertThrowsArgumentNull(() => _ = new ClrDictionaryInspector<int, int>(null!), "dic");
		}

		[TestMethod]
		public void DicEntryGetNextGuards_ThrowExpectedExceptions()
		{
			var last = new ClrDictionaryInspector<int, int>.DicEntry { NextEntryIndex = TypeExtensions.kNone };
			var inspector = new ClrDictionaryInspector<int, int>(new Dictionary<int, int>());

			AssertThrowsArgumentNull(() => _ = last.GetNext(null!), "inspector");
			Assert.ThrowsExactly<InvalidOperationException>(() => _ = last.GetNext(inspector));
		}

		[TestMethod]
		public void GetEntriesInBucketGuards_ThrowArgumentOutOfRangeException()
		{
			var inspector = new ClrDictionaryInspector<int, int>(new Dictionary<int, int>());

			AssertThrowsArgumentOutOfRange(() => inspector.GetEntriesInBucket(-1).ToArray(), "bucketIndex");
			AssertThrowsArgumentOutOfRange(() => inspector.GetEntriesInBucket(0).ToArray(), "bucketIndex");
		}
	};
}
