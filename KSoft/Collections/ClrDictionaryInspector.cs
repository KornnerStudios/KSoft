#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using Reflect = System.Reflection;

namespace KSoft.Collections
{
	using DicEntryHashCodeType = uint;

	// I forget what even sparked the need for this class. Because I could?
	// It *does* end up exercising KSoft reflection and expression utils quite a bit, so there's that.
	public sealed class ClrDictionaryInspector<TKey, TValue>
		where TKey : notnull
	{
		#region Dictionary field names
		// post-netframework, the names have underscore prefixes
		const string kDicBucketsName = "_buckets";
		const string kDictGetBucketName = "GetBucket";
		const string kDicEntriesName = "_entries";
		const string kDicCountName = "_count";
		const string kDicVersionName = "_version";
		const string kDicFreeListName = "_freeList";
		const string kDicFreeCountName = "_freeCount";
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
			public DicEntryHashCodeType HashCode; // only the lower 31 bits of the actual hash code
			/// <summary>
			/// 0-based index of next entry in chain: -1 means end of chain
			/// also encodes whether this entry _itself_ is part of the free list by changing sign and subtracting 3,
			/// so -2 means end of free list, -3 means index 0 but on free list, -4 means index 1 but on free list, etc.
			/// </summary>
			public int NextEntryIndex;
			public TKey Key;
			public TValue Value;

//			public readonly bool IsFree { get => HashCode.IsNone(); }
			public readonly bool IsLast { get => NextEntryIndex.IsNone(); }

			public readonly DicEntry GetNext(ClrDictionaryInspector<TKey, TValue> inspector)
			{
				ArgumentNullException.ThrowIfNull(inspector);
				if (IsLast)
					throw new InvalidOperationException();

				return inspector.Entries[NextEntryIndex];
			}
		};

		#region Dictionary getters
		private delegate ref int DicGetBucketDelegate(DicEntryHashCodeType hashCode);
		private delegate ref int DicGetBucketDelegateWithThis(Dictionary<TKey, TValue> @this, DicEntryHashCodeType hashCode);

		static readonly Func<Dictionary<TKey, TValue>, int[]?> kGetDicBuckets;
		static readonly /*Func<Dictionary<TKey, TValue>, DicEntryHashCodeType>*/DicGetBucketDelegateWithThis kCallDictGetBucket;
		static readonly Func<Dictionary<TKey, TValue>, Array?> kGetDicEntries;
		static readonly Func<Dictionary<TKey, TValue>, int> kGetDicCount;
		static readonly Func<Dictionary<TKey, TValue>, int> kGetDicVersion;
		static readonly Func<Dictionary<TKey, TValue>, int> kGetDicFreeList;
		static readonly Func<Dictionary<TKey, TValue>, int> kGetDicFreeCount;
		#endregion
		#region Dictionary.Entry getters
		static readonly Func<object, DicEntryHashCodeType> kGetEntryHashCode;
		static readonly Func<object, int> kGetEntryNextEntryIndex;
		static readonly Func<object, TKey> kGetEntryKey;
		static readonly Func<object, TValue> kGetEntryValue;
		#endregion

		static ClrDictionaryInspector()
		{
			// implementations are totally different...
			if (Shell.Platform.IsMonoRuntime)
			{
				Debug.Trace.Collections.TraceDataSansId(System.Diagnostics.TraceEventType.Critical,
					nameof(ClrDictionaryInspector<TKey, TValue>) + " does not support Mono");
			}

			// "If a nested type is generic, this method returns its generic type definition. This is true even if the enclosing generic type is a closed constructed type."
			var dic_entry_type = typeof(Dictionary<TKey, TValue>)
				.GetNestedType(kEntryTypeName, Reflect.BindingFlags.NonPublic);
			if (dic_entry_type == null)
			{
				throw new InvalidOperationException(string.Format(Util.InvariantCultureInfo,
					"Dictionary entry type '{0}' could not be found.",
					kEntryTypeName));
			}
			dic_entry_type = dic_entry_type.MakeGenericType(typeof(TKey), typeof(TValue));

			#region Dictionary getters
			kGetDicBuckets =
				Reflection.Util.GenerateMemberGetter<Dictionary<TKey, TValue>, int[]?>(kDicBucketsName);
			kCallDictGetBucket =
				Reflection.Util.GenerateObjectMethodProxy<
					Dictionary<TKey, TValue>,
					DicGetBucketDelegateWithThis,
					DicGetBucketDelegate>(
						kDictGetBucketName);
			kGetDicEntries =
				Reflection.Util.GenerateMemberGetter<Dictionary<TKey, TValue>, Array?>(kDicEntriesName);
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
				Reflection.Util.GenerateMemberGetter<DicEntryHashCodeType>(dic_entry_type, kEntryHashCodeName);
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
		DicEntry[]? mEntries;

		public ClrDictionaryInspector(Dictionary<TKey, TValue> dic)
		{
			ArgumentNullException.ThrowIfNull(dic);

			mDic = dic;
			mExpectedVersion = Version;
		}

		void ThrowIfDictionaryWasModified()
		{
			if (Version != mExpectedVersion)
			{
				throw new InvalidOperationException("Tried to inspect a dictionary that has been modified since the inspector was created.");
			}
		}

		DicEntry[] InitializeEntries()
		{
			ThrowIfDictionaryWasModified();

			var entries = new DicEntry[Buckets.Count];
			mEntries = entries;
			var array = kGetDicEntries(mDic)!;

			for (int x = 0; x < array.Length; x++)
			{
				var entry = array.GetValue(x);

				entries[x] = new DicEntry()
				{
					HashCode = kGetEntryHashCode(entry!),
					NextEntryIndex = kGetEntryNextEntryIndex(entry!),
					Key = kGetEntryKey(entry!),
					Value = kGetEntryValue(entry!),
				};
			}

			return entries;
		}

		public IReadOnlyList<int> Buckets { get {
			var buckets = kGetDicBuckets(mDic);

			return buckets ?? [];
		} }
		public IReadOnlyList<DicEntry> Entries { get {
			var entries = mEntries;
			if (entries == null)
				entries = InitializeEntries();

			return entries;
		} }
		public int Count { get => kGetDicCount(mDic); }
		private int Version { get => kGetDicVersion(mDic); }
		public int FreeList { get => kGetDicFreeList(mDic); }
		public int FreeCount { get => kGetDicFreeCount(mDic); }

		public IEnumerable<int> BucketsInUse { get => Buckets.Where(b => b >= 0); }

		public IEnumerable<DicEntry> GetEntriesInBucket(int bucketIndex)
		{
			ArgumentOutOfRangeException.ThrowIfNegative(bucketIndex);
			ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(bucketIndex, Buckets.Count);

			for (int x = Buckets[bucketIndex]; x >= 0; x = Entries[x].NextEntryIndex)
			{
				yield return Entries[x];
			}
		}

		public IEnumerable<DicEntry> EntryCollisions(TKey key)
		{
			//int hash_code = mDic.Comparer.GetHashCode(key) & 0x7FFFFFFF;
			DicEntryHashCodeType hash_code = (DicEntryHashCodeType)mDic.Comparer.GetHashCode(key);
			int target_bucket = /*hash_code % Buckets.Count*/kCallDictGetBucket(mDic, hash_code);

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
