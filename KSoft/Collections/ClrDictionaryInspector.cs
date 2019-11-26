using System;
using System.Collections.Generic;
using System.Linq;
using Contracts = System.Diagnostics.Contracts;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif
using Reflect = System.Reflection;

namespace KSoft.Collections
{
	public sealed class ClrDictionaryInspector<TKey, TValue>
	{
		#region Dictionary field names
		const string kDicBucketsName = "buckets";
		const string kDicEntriesName = "entries";
		const string kDicCountName = "count";
		const string kDicVersionName = "version";
		const string kDicFreeListName = "freeList";
		const string kDicFreeCountName = "freeCount";
		#endregion
		#region Dictionary.Entry field names
		const string kEntryTypeName = "Entry";
		const string kEntryHashCodeName = "hashCode";
		const string kEntryNextEntryIndexName = "next";
		const string kEntryKeyName = "key";
		const string kEntryValueName = "value";
		#endregion

		public struct DicEntry
		{
			public int HashCode; // only the lower 31 bits of the actual hash code
			public int NextEntryIndex;
			public TKey Key;
			public TValue Value;

			public bool IsFree { get { return HashCode.IsNone(); } }
			public bool IsLast { get { return NextEntryIndex.IsNone(); } }

			public DicEntry GetNext(ClrDictionaryInspector<TKey, TValue> inspector)
			{
				Contract.Requires<ArgumentNullException>(inspector != null);
				Contract.Requires<InvalidOperationException>(!IsLast);

				return inspector.Entries[NextEntryIndex];
			}
		};

		private static readonly IReadOnlyList<int> kEmptyBuckets = new int[0];

		#region Dictionary getters
		static readonly Func<Dictionary<TKey, TValue>, int[]> kGetDicBuckets;
		static readonly Func<Dictionary<TKey, TValue>, Array> kGetDicEntries;
		static readonly Func<Dictionary<TKey, TValue>, int> kGetDicCount;
		static readonly Func<Dictionary<TKey, TValue>, int> kGetDicVersion;
		static readonly Func<Dictionary<TKey, TValue>, int> kGetDicFreeList;
		static readonly Func<Dictionary<TKey, TValue>, int> kGetDicFreeCount;
		#endregion
		#region Dictionary.Entry getters
		static readonly Func<object, int> kGetEntryHashCode;
		static readonly Func<object, int> kGetEntryNextEntryIndex;
		static readonly Func<object, TKey> kGetEntryKey;
		static readonly Func<object, TValue> kGetEntryValue;
		#endregion

		static ClrDictionaryInspector()
		{
			// implementations are totally different...
			if (Shell.Platform.IsMonoRuntime)
				throw new PlatformNotSupportedException(
					typeof(ClrDictionaryInspector<TKey, TValue>).Name + " doesn't support Mono");

			// "If a nested type is generic, this method returns its generic type definition. This is true even if the enclosing generic type is a closed constructed type."
			var dic_entry_type = typeof(Dictionary<TKey, TValue>)
				.GetNestedType(kEntryTypeName, Reflect.BindingFlags.NonPublic)
				.MakeGenericType(typeof(TKey), typeof(TValue));
			Contract.Assert(dic_entry_type != null);

			#region Dictionary getters
			kGetDicBuckets =
				Reflection.Util.GenerateMemberGetter<Dictionary<TKey, TValue>, int[]>(kDicBucketsName);
			kGetDicEntries =
				Reflection.Util.GenerateMemberGetter<Dictionary<TKey, TValue>, Array>(kDicEntriesName);
			kGetDicCount =
				Reflection.Util.GenerateMemberGetter<Dictionary<TKey, TValue>, int>(kDicCountName);
			kGetDicVersion =
				Reflection.Util.GenerateMemberGetter<Dictionary<TKey, TValue>, int>(kDicVersionName);
			kGetDicFreeList =
				Reflection.Util.GenerateMemberGetter<Dictionary<TKey, TValue>, int>(kDicFreeListName);
			kGetDicFreeCount =
				Reflection.Util.GenerateMemberGetter<Dictionary<TKey, TValue>, int>(kDicFreeCountName);
			#endregion
			#region Dictionary.Entry getters
			kGetEntryHashCode =
				Reflection.Util.GenerateMemberGetter<int>(dic_entry_type, kEntryHashCodeName);
			kGetEntryNextEntryIndex =
				Reflection.Util.GenerateMemberGetter<int>(dic_entry_type, kEntryNextEntryIndexName);
			kGetEntryKey =
				Reflection.Util.GenerateMemberGetter<TKey>(dic_entry_type, kEntryKeyName);
			kGetEntryValue =
				Reflection.Util.GenerateMemberGetter<TValue>(dic_entry_type, kEntryValueName);
			#endregion
		}

		readonly Dictionary<TKey, TValue> mDic;
		readonly int mExpectedVersion;
		DicEntry[] mEntries;

		public ClrDictionaryInspector(Dictionary<TKey, TValue> dic)
		{
			Contract.Requires<ArgumentNullException>(dic != null);

			mDic = dic;
			mExpectedVersion = Version;
		}

		[Contracts.ContractInvariantMethod]
		void ObjectInvariant()
		{
			Contract.Invariant(Version == mExpectedVersion,
				"Tried to inspect a dictionary that has been modified since the inspector was created");
		}

		void InitializeEntries()
		{
			mEntries = new DicEntry[Buckets.Count];
			var array = kGetDicEntries(mDic);

			for (int x = 0; x < array.Length; x++)
			{
				var entry = array.GetValue(x);

				mEntries[x] = new DicEntry()
				{
					HashCode = kGetEntryHashCode(entry),
					NextEntryIndex = kGetEntryNextEntryIndex(entry),
					Key = kGetEntryKey(entry),
					Value = kGetEntryValue(entry),
				};
			}
		}

		public IReadOnlyList<int> Buckets { get {
			var buckets = kGetDicBuckets(mDic);

			return buckets ?? kEmptyBuckets;
		} }
		public IReadOnlyList<DicEntry> Entries { get {
			if (mEntries == null)
				InitializeEntries();

			return mEntries;
		} }
		public int Count { get { return kGetDicCount(mDic); } }
		private int Version { get { return kGetDicVersion(mDic); } }
		public int FreeList { get { return kGetDicFreeList(mDic); } }
		public int FreeCount { get { return kGetDicFreeCount(mDic); } }

		public IEnumerable<int> BucketsInUse { get {
			return Buckets.Where(b => b >= 0);
		} }
		public IEnumerable<DicEntry> GetEntriesInBucket(int bucketIndex)
		{
			Contract.Requires<ArgumentOutOfRangeException>(bucketIndex >= 0 && bucketIndex < Buckets.Count);

			for (int x = Buckets[bucketIndex]; x >= 0; x = Entries[x].NextEntryIndex)
				yield return Entries[x];
		}

		public IEnumerable<DicEntry> EntryCollisions(TKey key)
		{
			int hash_code = mDic.Comparer.GetHashCode(key) & 0x7FFFFFFF;
			int target_bucket = hash_code % Buckets.Count;

#if false // result as entry indices
			for (int x = Buckets[target_bucket]; x >= 0; x = Entries[x].NextEntryIndex)
			{
				if (Entries[x].HashCode == hash_code && mDic.Comparer.Equals(Entries[x].Key, key))
					yield break;

				yield return x;
			}
#endif

			return
				from e in GetEntriesInBucket(target_bucket)
				where e.HashCode == hash_code && !mDic.Comparer.Equals(e.Key, key)
				select e;
		}
	};
}