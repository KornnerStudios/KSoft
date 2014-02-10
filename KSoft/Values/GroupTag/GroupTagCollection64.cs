using System;
using System.Collections.Generic;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Values
{
	/// <summary>Collection of 64-bit Group Tags</summary>
	public class GroupTag64Collection : GroupTagCollection
	{
		#region Elements
		/// <summary>This collection's group tag elements</summary>
		public GroupTagData64[] GroupTags { get; private set; }

		protected override GroupTagData[] BaseGroupTags { get { return (GroupTagData[])GroupTags; } }
		#endregion

		public override GroupTagData NullGroupTag { get { return GroupTagData64.Null; } }

		[Contracts.ContractInvariantMethod]
		void ObjectInvariant()	{ Contract.Invariant(GroupTags != null); }

		#region Ctor
		/// <summary>Create a collection based on an existing list of group tags</summary>
		/// <param name="groupTags">Group tags to populate this collection with</param>
		public GroupTag64Collection(GroupTagData64[] groupTags) : base(groupTags)
		{
			Contract.Requires(groupTags != null);

			GroupTags = new GroupTagData64[groupTags.Length];
			groupTags.CopyTo(GroupTags, 0);

			Contract.Assume(GroupTags != null);
		}
		/// <summary>Create a collection based on an existing list of group tags</summary>
		/// <param name="guid">Guid for this group tag collection</param>
		/// <param name="groupTags">Group tags to populate this collection with</param>
		public GroupTag64Collection(Guid guid, GroupTagData64[] groupTags) : base(groupTags, guid)
		{
			Contract.Requires(guid != Guid.Empty);
			Contract.Requires(groupTags != null);

			GroupTags = new GroupTagData64[groupTags.Length];
			groupTags.CopyTo(GroupTags, 0);

			Contract.Assume(GroupTags != null);
		}
		/// <summary>Create a collection using an explicit list of group tags</summary>
		/// <param name="sort">Should we sort the list?</param>
		/// <param name="groupTags">Group tags to populate this collection with</param>
		public GroupTag64Collection(bool sort, params GroupTagData64[] groupTags) : this(groupTags)
		{
			Contract.Requires(groupTags != null);

			if (sort)
				Sort();
		}
		/// <summary>Create a collection using an explicit list of group tags</summary>
		/// <param name="guid">Guid for this group tag collection</param>
		/// <param name="sort">Should we sort the list?</param>
		/// <param name="groupTags">Group tags to populate this collection with</param>
		public GroupTag64Collection(Guid guid, bool sort, params GroupTagData64[] groupTags) : this(guid, groupTags)
		{
			Contract.Requires(guid != Guid.Empty);
			Contract.Requires(groupTags != null);

			if (sort)
				Sort();
		}
		#endregion

		#region Searching
		/// <summary>Finds the index of a <see cref="GroupTagData64"/></summary>
		/// <param name="groupTag">The id of a group tag to search for</param>
		/// <returns>Index of <paramref name="group"/> or <b>-1</b> if not found</returns>
		public int FindGroupIndex(ulong groupTag)
		{
			int index = 0;
			foreach (GroupTagData64 g in GroupTags)
			{
				Contract.Assume(g != null);
				if (g.ID == groupTag) return index;
				index++;
			}

			return TypeExtensions.kNoneInt32;
		}

		/// <summary>Find a <see cref="GroupTagData64"/> object in this collection based on it's group tag</summary>
		/// <param name="tag">Group tag to find</param>
		/// <returns><see cref="GroupTagData64"/> object existing in this collection, or null if not found.</returns>
		public GroupTagData64 FindTagGroup(ulong tag)
		{
			foreach (GroupTagData64 g in GroupTags)
			{
				Contract.Assume(g != null);
				if (g.ID == tag)
					return g;
			}

			return null;
		}

		/// <summary>Determines if a group tag ID exists in this collection</summary>
		/// <param name="tag">Group tag id to find</param>
		/// <returns></returns>
		public bool Contains(ulong tag)
		{
			foreach (GroupTagData64 g in GroupTags)
			{
				Contract.Assume(g != null);
				if (g.ID == tag)
					return true;
			}

			return false;
		}
		#endregion

		#region IEndianStreamable Members
		/// <summary>Moves the stream ahead by the sizeof a eight character code (8 bytes) times the count of the <see cref="GroupTags"/></summary>
		/// <param name="s"></param>
		/// <remarks>Doesn't actually read any data from the stream, only seeks forward</remarks>
		public override void Read(IO.EndianReader s)
		{
			s.Seek(GroupTags.Length * sizeof(ulong), System.IO.SeekOrigin.Current);
		}
		#endregion
	};


	/// <summary>Attribute applied to classes which house a static <see cref="GroupTag64Collection"/> property</summary>
	/// <remarks>
	/// Allows for ease-of-use in other attributes where we'd need to index a <see cref="GroupTag64Collection"/> 
	/// collection for a specific <see cref="GroupTagData64"/> member
	/// 
	/// <see cref="GroupTagContainerAttribute.kDefaultName"/> is the default name value used for the "main" 
	/// collection lookup
	/// </remarks>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public class GroupTagContainer64Attribute : GroupTagContainerAttribute
	{
		/// <summary>Initialize the attribute with a type containing a <see cref="GroupTag64Collection"/></summary>
		/// <param name="container">A type which contains static <see cref="GroupTag64Collection"/> properties</param>
		public GroupTagContainer64Attribute(Type container) : base(container)
		{
			Contract.Requires(container != null);
		}
		/// <summary>Initialize the attribute with a type containing a <see cref="GroupTag64Collection"/></summary>
		/// <param name="container">A type which contains static <see cref="GroupTag64Collection"/> properties</param>
		/// <param name="collection_name">Explicit name for the "main" group collection property</param>
		protected GroupTagContainer64Attribute(Type container, string collection_name) : base(container, collection_name)
		{
			Contract.Requires(container != null);
			Contract.Requires(!string.IsNullOrEmpty(collection_name));
		}

		/// <summary>The "main" group of the class which this attribute was applied to</summary>
		public GroupTag64Collection Collection { get { return mTagCollection as GroupTag64Collection; } }
	};
}