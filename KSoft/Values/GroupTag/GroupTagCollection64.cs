﻿using System;
using System.Collections.Generic;
using System.Linq;
using Contracts = System.Diagnostics.Contracts;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

using TagWord = System.UInt64;

namespace KSoft.Values
{
	using GroupTagDatum = GroupTagData64;

	/// <summary>Collection of 64-bit Group Tags</summary>
	public sealed class GroupTag64Collection : GroupTagCollection
	{
		#region Elements
		readonly GroupTagDatum[] mGroupTags;
		/// <summary>This collection's group tag elements</summary>
		public IReadOnlyList<GroupTagDatum> GroupTags { get { return mGroupTags; } }

		protected override GroupTagData[] BaseGroupTags { get { return (GroupTagData[])mGroupTags; } }
		#endregion

		public override GroupTagData NullGroupTag { get { return GroupTagDatum.Null; } }

		#region Ctor
		/// <summary>Create a collection based on an existing list of group tags</summary>
		/// <param name="groupTags">Group tags to populate this collection with</param>
		public GroupTag64Collection(params GroupTagDatum[] groupTags) : this(KGuid.Empty, groupTags)
		{
			Contract.Requires<ArgumentNullException>(groupTags != null);
		}
		/// <summary>Create a collection based on an existing list of group tags</summary>
		/// <param name="guid">Guid for this group tag collection</param>
		/// <param name="groupTags">Group tags to populate this collection with</param>
		public GroupTag64Collection(KGuid guid, params GroupTagDatum[] groupTags) : base(groupTags, guid)
		{
			Contract.Requires<ArgumentNullException>(groupTags != null);

			mGroupTags = new GroupTagDatum[groupTags.Length];
			groupTags.CopyTo(mGroupTags, 0);
		}
		/// <summary>Create a collection using an explicit list of group tags</summary>
		/// <param name="sort">Should we sort the list?</param>
		/// <param name="groupTags">Group tags to populate this collection with</param>
		public GroupTag64Collection(bool sort, params GroupTagDatum[] groupTags) : this(groupTags)
		{
			Contract.Requires<ArgumentNullException>(groupTags != null);

			if (sort)
				Sort();
		}
		/// <summary>Create a collection using an explicit list of group tags</summary>
		/// <param name="guid">Guid for this group tag collection</param>
		/// <param name="sort">Should we sort the list?</param>
		/// <param name="groupTags">Group tags to populate this collection with</param>
		public GroupTag64Collection(KGuid guid, bool sort, params GroupTagDatum[] groupTags) : this(guid, groupTags)
		{
			Contract.Requires<ArgumentNullException>(groupTags != null);

			if (sort)
				Sort();
		}
		#endregion

		#region Searching
		/// <summary>Finds the index of a <see cref="GroupTagData64"/></summary>
		/// <param name="groupTag">The id of a group tag to search for</param>
		/// <returns>Index of <paramref name="group"/> or <b>-1</b> if not found</returns>
		[Contracts.Pure]
		public int FindGroupIndexByTag(TagWord groupTag)
		{
			return GroupTags.FindIndex(gt => gt.ID == groupTag);
		}

		/// <summary>Find a <see cref="GroupTagData64"/> object in this collection based on it's group tag</summary>
		/// <param name="tag">Group tag to find</param>
		/// <returns><see cref="GroupTagData64"/> object existing in this collection, or null if not found.</returns>
		[Contracts.Pure]
		public GroupTagDatum FindGroupByTag(TagWord tag)
		{
			var matching_tags = from gt in GroupTags
								where gt.ID == tag
								select gt;

			return matching_tags.FirstOrDefault();
		}

		/// <summary>Determines if a group tag ID exists in this collection</summary>
		/// <param name="tag">Group tag id to find</param>
		/// <returns></returns>
		[Contracts.Pure]
		public bool Contains(TagWord tag)
		{
			return GroupTags.Any(gt => gt.ID == tag);
		}
		#endregion

		#region IEndianStreamable Members
		/// <summary>Moves the stream ahead by the sizeof a eight character code (8 bytes) times the count of the <see cref="GroupTags"/></summary>
		/// <param name="s"></param>
		/// <remarks>Doesn't actually read any data from the stream, only seeks forward</remarks>
		public override void Read(IO.EndianReader s)
		{
			s.Seek(GroupTags.Count * sizeof(TagWord), System.IO.SeekOrigin.Current);
		}
		#endregion

		/// <summary>Get a new instance of an empty collection</summary>
		public static GroupTag64Collection Empty { get {
			return new GroupTag64Collection();
		} }
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
		/// <param name="collectionName">Explicit name for the "main" group collection property</param>
		protected GroupTagContainer64Attribute(Type container, string collectionName) : base(container, collectionName)
		{
			Contract.Requires(container != null);
			Contract.Requires(!string.IsNullOrEmpty(collectionName));
		}

		/// <summary>The "main" group of the class which this attribute was applied to</summary>
		public GroupTag64Collection Collection { get { return mTagCollection as GroupTag64Collection; } }
	};
}