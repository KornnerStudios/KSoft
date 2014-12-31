using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Collections.Test
{
	[TestClass]
	public class ClrDictionaryInspectorTest : BaseTestClass
	{
		[TestMethod]
		public void Collections_ClrDictionaryInspectorIntTest()
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
					Console.WriteLine("{0} had {1} collisions before it was added",
						x, collision_count);
			}
		}
	};
}