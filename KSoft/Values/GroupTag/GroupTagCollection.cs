using System;
using System.Collections;
using System.Collections.Generic;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Values
{
	/// <summary>Base interface for Group Tag collections</summary>
	[Contracts.ContractClass(typeof(GroupTagCollectionContract))]
	public abstract class GroupTagCollection
		: IReadOnlyList<GroupTagData>
		, IO.IEndianStreamable
	{
		/// <summary>Compare two <see cref="GroupTagData"/> objects by their <see cref="GroupTagData.Name"/> properties</summary>
		class ByNameComparer : System.Collections.IComparer, System.Collections.Generic.IComparer<GroupTagData>
		{
			public int Compare(object x, object y)
			{
				Contract.Assume(x != null);
				Contract.Assume(y != null);

				return Compare((GroupTagData)x, (GroupTagData)y);
			}
			public int Compare(GroupTagData x, GroupTagData y)
			{
				Contract.Assume(x != null);
				Contract.Assume(y != null);

				return string.Compare(x.Name, y.Name);
			}
		};

		protected abstract GroupTagData[] BaseGroupTags { get; }

		/// <summary>Get the group tag which represents null</summary>
		public abstract GroupTagData NullGroupTag { get; }

		#region Guid
		protected readonly KGuid mGuid = KGuid.Empty;
		/// <summary>Guid for this group tag collection</summary>
		public KGuid Guid { get { return mGuid; } }
		#endregion

		protected GroupTagCollection(GroupTagData[] groups)
		{
			Contract.Requires(Array.TrueForAll(groups, Predicates.IsNotNull));
		}
		protected GroupTagCollection(GroupTagData[] groups, KGuid guid) : this(groups)
		{
			mGuid = guid;
		}

		#region Indexers
		/// <summary>Get the full name of a group tag based on its character code</summary>
		/// <remarks>If <paramref name="tag"/> is not found, "unknown" is returned</remarks>
		public string this[char[] tag]
		{
			get
			{
				Contract.Requires(tag != null);
				Contract.Ensures(!string.IsNullOrEmpty(Contract.Result<string>()));

				foreach (GroupTagData t in BaseGroupTags)
				{
					Contract.Assume(t != null);
					Contract.Assume(t.Tag.Length == tag.Length);
					if (t.Test(tag))
						return t.Name;
				}

				return "unknown";
			}
		}
		#endregion

		#region Searching
		/// <summary>Finds the index of a <see cref="GroupTagData"/></summary>
		/// <param name="groupTag">The <see cref="GroupTagData"/>'s 'tag' to search for</param>
		/// <returns>Index of <paramref name="groupTag"/> or <b>-1</b> if not found</returns>
		public int FindGroupIndex(char[] groupTag)
		{
			Contract.Requires(groupTag != null);

			int index = 0;
			foreach (GroupTagData g in BaseGroupTags)
			{
				Contract.Assume(g != null);
				Contract.Assume(g.Tag.Length == groupTag.Length);
				if (g.Test(groupTag))
					return index;
				index++;
			}

			return TypeExtensions.kNoneInt32;
		}

		/// <summary>Finds the index of a <see cref="GroupTagData"/></summary>
		/// <param name="groupTag">The name of a <see cref="GroupTagData"/> to search for</param>
		/// <returns>Index of <paramref name="groupTag"/> or <b>-1</b> if not found</returns>
		public int FindGroupIndex(string groupTag)
		{
			Contract.Requires(!string.IsNullOrEmpty(groupTag));

			int index = 0;
			foreach (GroupTagData g in BaseGroupTags)
			{
				Contract.Assume(g != null);
				if (g.Name == groupTag)
					return index;
				index++;
			}

			return TypeExtensions.kNoneInt32;
		}

		/// <summary>Finds the index of a supplied <see cref="GroupTagData"/> object</summary>
		/// <param name="group"><see cref="GroupTagData"/> object whose index we want to find</param>
		/// <returns>Index of <paramref name="group"/> or <b>-1</b> if not found</returns>
		public int FindGroupIndex(GroupTagData group)
		{
			Contract.Requires(group != null);

			int index = 0;
			foreach (GroupTagData g in BaseGroupTags)
			{
				Contract.Assume(g != null);
				if (object.ReferenceEquals(g, group))
					return index;
				index++;
			}

			return TypeExtensions.kNoneInt32;
		}

		/// <summary>Finds a <see cref="GroupTagData"/> of this collection based on its group tag</summary>
		/// <param name="group_tag">The <see cref="GroupTagData"/>'s 'tag' to search for</param>
		/// <returns>Null if <paramref name="group_tag"/> isn't a part of this collection</returns>
		public GroupTagData FindGroup(char[] group_tag)
		{
			Contract.Requires(group_tag != null);

			int index = FindGroupIndex(group_tag);
			if (index.IsNone())
				return null;

			return BaseGroupTags[index];
		}

		/// <summary>Finds a <see cref="GroupTagData"/> of this collection based on its group tag</summary>
		/// <param name="groupTag">The name of a <see cref="GroupTagData"/> to search for</param>
		/// <returns>Null if <paramref name="groupTag"/> isn't a part of this collection</returns>
		public GroupTagData FindGroup(string groupTag)
		{
			Contract.Requires(!string.IsNullOrEmpty(groupTag));

			int index = FindGroupIndex(groupTag);
			if (index.IsNone())
				return null;

			return BaseGroupTags[index];
		}
		#endregion

		/// <summary>Sort the collection by the names of the group tags</summary>
		public void Sort() { Array.Sort<GroupTagData>(BaseGroupTags, new ByNameComparer()); }

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
				Contract.Assume(g != null);

				g.Write(s);
			}
		}
		#endregion

		#region IReadOnlyList<GroupTagData> Members
		public GroupTagData this[int index]
		{
			get { return BaseGroupTags[index]; }
		}
		public int Count
		{
			get { return BaseGroupTags.Length; }
		}
		#endregion

		#region IEnumerable<GroupTagData> Members
		public IEnumerator<GroupTagData> GetEnumerator()
		{
			return (IEnumerator<GroupTagData>)BaseGroupTags.GetEnumerator();
		}
		IEnumerator IEnumerable.GetEnumerator()
		{
			return BaseGroupTags.GetEnumerator();
		}
		#endregion
	};
	[Contracts.ContractClassFor(typeof(GroupTagCollection))]
	abstract class GroupTagCollectionContract : GroupTagCollection
	{
		protected GroupTagCollectionContract() : base(null) { throw new NotImplementedException(); }

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