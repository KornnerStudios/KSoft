using System;
using System.Collections.Generic;
using System.Linq;
using Contracts = System.Diagnostics.Contracts;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Values
{
	/// <summary>Base interface for Group Tag collections</summary>
	[Contracts.ContractClass(typeof(GroupTagCollectionContract))]
	public abstract class GroupTagCollection
		: IReadOnlyList<GroupTagData>
		, IO.IEndianStreamable
	{
		/// <summary>Compare two <see cref="GroupTagData"/> objects by their <see cref="GroupTagData.Name"/> properties</summary>
		class ByNameComparer
			: System.Collections.IComparer
			, System.Collections.Generic.IComparer<GroupTagData>
		{
			public int Compare(object x, object y)
			{
				System.Diagnostics.Debug.Assert(x != null);
				System.Diagnostics.Debug.Assert(y != null);

				return Compare((GroupTagData)x, (GroupTagData)y);
			}
			public int Compare(GroupTagData x, GroupTagData y)
			{
				System.Diagnostics.Debug.Assert(x != null);
				System.Diagnostics.Debug.Assert(y != null);

				return string.CompareOrdinal(x.Name, y.Name);
			}
		};

		protected abstract GroupTagData[] BaseGroupTags { get; }

		/// <summary>Get the group tag which represents null</summary>
		public abstract GroupTagData NullGroupTag { get; }

		/// <summary>Guid for this group tag collection</summary>
		public KGuid Uuid { get; private set; } = KGuid.Empty;

		/// <summary>Does this instance represent an empty collection of the concrete GroupTagData type?</summary>
		/// <remarks>If the Uuid is not zero, but there are no GroupTags, this will return false</remarks>
		public bool IsEmpty { get => BaseGroupTags.Length == 0 && Uuid == KGuid.Empty; }

		protected GroupTagCollection(GroupTagData[] groupTags)
		{
			ValidateGroupTags(groupTags);
		}
		protected GroupTagCollection(GroupTagData[] groupTags, KGuid uuid) : this(groupTags)
		{
			Uuid = uuid;
		}

		static void ValidateGroupTags(GroupTagData[] groupTags)
		{
			ArgumentNullException.ThrowIfNull(groupTags);
			if (!Array.TrueForAll(groupTags, Predicates.IsNotNull))
			{
				throw new ArgumentException("Group tag collections must not contain null elements.", nameof(groupTags));
			}
		}

		#region Indexers
		/// <summary>Get the full name of a group tag based on its character code</summary>
		/// <remarks>If <paramref name="tag"/> is not found, "unknown" is returned</remarks>
		[Contracts.Pure]
		public string this[char[] tag] { get {
			Verify.GroupTags.ExactLength(tag, NullGroupTag.Tag.Length, nameof(tag));
			Contract.Ensures(!string.IsNullOrEmpty(Contract.Result<string>()));

			foreach (GroupTagData t in BaseGroupTags)
			{
				if (t.Test(tag))
					{
						return t.Name;
					}
				}

			return NullGroupTag.Name;
		} }
		#endregion

		#region Searching
		/// <summary>Finds the index of a <see cref="GroupTagData"/></summary>
		/// <param name="groupTag">The <see cref="GroupTagData"/>'s 'tag' to search for</param>
		/// <returns>Index of <paramref name="groupTag"/> or <b>-1</b> if not found</returns>
		[Contracts.Pure]
		public int FindGroupIndexByTag(char[] groupTag)
		{
			Verify.GroupTags.ExactLength(groupTag, NullGroupTag.Tag.Length, nameof(groupTag));

			return BaseGroupTags.FindIndex(gt => gt.Test(groupTag));
		}
		/// <summary>Finds the index of a <see cref="GroupTagData"/></summary>
		/// <param name="tagString">The <see cref="GroupTagData"/>'s 'tag' to search for</param>
		/// <returns>Index of <paramref name="tagString"/> or <b>-1</b> if not found</returns>
		[Contracts.Pure]
		public int FindGroupIndexByTag(string tagString)
		{
			Verify.GroupTags.ExactLength(tagString, NullGroupTag.Tag.Length, nameof(tagString));

			return BaseGroupTags.FindIndex(gt => gt.TagString == tagString);
		}

		/// <summary>Finds the index of a <see cref="GroupTagData"/></summary>
		/// <param name="groupName">The name of a <see cref="GroupTagData"/> to search for</param>
		/// <returns>Index of <paramref name="groupName"/> or <b>-1</b> if not found</returns>
		[Contracts.Pure]
		public int FindGroupIndex(string groupName)
		{
			ArgumentException.ThrowIfNullOrEmpty(groupName);

			return BaseGroupTags.FindIndex(gt => gt.Name == groupName);
		}

		/// <summary>Finds the index of a supplied <see cref="GroupTagData"/> object</summary>
		/// <param name="group"><see cref="GroupTagData"/> object whose index we want to find</param>
		/// <returns>Index of <paramref name="group"/> or <b>-1</b> if not found</returns>
		[Contracts.Pure]
		public int FindGroupIndex(GroupTagData group)
		{
			ArgumentNullException.ThrowIfNull(group);

			return BaseGroupTags.FindIndex(gt => object.ReferenceEquals(gt, group));
		}

		/// <summary>Finds a <see cref="GroupTagData"/> of this collection based on its group tag</summary>
		/// <param name="groupTag">The <see cref="GroupTagData"/>'s 'tag' to search for</param>
		/// <returns>Null if <paramref name="groupTag"/> isn't a part of this collection</returns>
		[Contracts.Pure]
		public GroupTagData FindGroup(char[] groupTag)
		{
			ArgumentNullException.ThrowIfNull(groupTag);

			int index = FindGroupIndexByTag(groupTag);
			if (index.IsNone())
			{
				return null;
			}

			return BaseGroupTags[index];
		}
		/// <summary>Finds a <see cref="GroupTagData"/> of this collection based on its group tag</summary>
		/// <param name="tagString">The <see cref="GroupTagData"/>'s 'tag' to search for</param>
		/// <returns>Null if <paramref name="tagString"/> isn't a part of this collection</returns>
		[Contracts.Pure]
		public GroupTagData FindGroupByTag(string tagString)
		{
			Verify.GroupTags.ExactLength(tagString, NullGroupTag.Tag.Length, nameof(tagString));

			int index = FindGroupIndexByTag(tagString);
			if (index.IsNone())
			{
				return null;
			}

			return BaseGroupTags[index];
		}

		/// <summary>Finds a <see cref="GroupTagData"/> of this collection based on its group tag</summary>
		/// <param name="groupName">The name of a <see cref="GroupTagData"/> to search for</param>
		/// <returns>Null if <paramref name="groupName"/> isn't a part of this collection</returns>
		[Contracts.Pure]
		public GroupTagData FindGroup(string groupName)
		{
			ArgumentException.ThrowIfNullOrEmpty(groupName);

			int index = FindGroupIndex(groupName);
			if (index.IsNone())
			{
				return null;
			}

			return BaseGroupTags[index];
		}
		#endregion

		/// <summary>Sort the collection by the names of the group tags</summary>
		public void Sort() => Array.Sort<GroupTagData>(BaseGroupTags, new ByNameComparer());

		/// <summary>Does the collection contain multiple elements which share the same group tag and\or name?</summary>
		/// <returns></returns>
		[Contracts.Pure]
		public bool ContainsDuplicates()
		{
			var all_tags =	from gt in BaseGroupTags select gt.TagString;
			var all_names = from gt in BaseGroupTags select gt.Name;

			return all_tags.ContainsDuplicates() || all_names.ContainsDuplicates();
		}

		#region IEndianStreamable Members
		/// <summary>Seek ahead (<see cref="Count"/> * length of the <see cref="GroupTagData"/>'s character code) bytes</summary>
		/// <param name="s"></param>
		public abstract void Read(IO.EndianReader s);
		/// <summary>Writes each group tag's character code</summary>
		/// <param name="s"></param>
		public void Write(IO.EndianWriter s)
		{
			foreach (GroupTagData g in BaseGroupTags)
			{
				System.Diagnostics.Debug.Assert(g != null);

				g.Write(s);
			}
		}
		#endregion

		#region IReadOnlyList<GroupTagData> Members
		public GroupTagData this[int index] { get => BaseGroupTags[index]; }
		public int Count { get => BaseGroupTags.Length; }
		#endregion

		#region IEnumerable<GroupTagData> Members
		public IEnumerator<GroupTagData> GetEnumerator() => (IEnumerator<GroupTagData>)BaseGroupTags.GetEnumerator();
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => BaseGroupTags.GetEnumerator();
		#endregion
	};
	[Contracts.ContractClassFor(typeof(GroupTagCollection))]
	abstract class GroupTagCollectionContract : GroupTagCollection
	{
		protected GroupTagCollectionContract() : base(null) => throw new NotImplementedException();

		protected override GroupTagData[] BaseGroupTags { get {
			Contract.Ensures(Contract.Result<GroupTagData[]>() != null);

			throw new NotImplementedException();
		} }

		public override GroupTagData NullGroupTag { get {
			Contract.Ensures(Contract.Result<GroupTagData>() != null);

			throw new NotImplementedException();
		} }
	};
}
