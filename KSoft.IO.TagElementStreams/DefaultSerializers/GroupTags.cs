using System;
using System.Collections.Generic;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.IO
{
	public partial class TagElementStreamDefaultSerializer
	{
		public static void Serialize<TDoc, TCursor>(TagElementStream<TDoc, TCursor, string> s,
			ref Values.GroupTagData32 value)
			where TDoc : class
			where TCursor : class
		{
			bool reading = s.IsReading;

			var group_tag = reading
				? null
				: value.TagString;
			var name = reading
				? null
				: value.Name;
			var guid = reading
				? Values.KGuid.Empty
				: value.Guid;

			s.StreamAttribute("tag", ref group_tag);
			s.StreamAttributeOpt("guid", ref guid, Predicates.IsNotEmpty);
			s.StreamAttribute("name", ref name);

			if(reading)
			{
				value = new Values.GroupTagData32(group_tag, name, guid);
			}
		}

		static void StreamElements<TDoc, TCursor>(TagElementStream<TDoc, TCursor, string> s,
			ref Values.GroupTagData32[] tags)
			where TDoc : class
			where TCursor : class
		{
			const string k_element_name = "Group";

			if (s.IsReading)
			{
				var list = new List<Values.GroupTagData32>();

				foreach (var node in s.ElementsByName(k_element_name))
					using (s.EnterCursorBookmark(node))
					{
						Values.GroupTagData32 data = null;
						Serialize(s, ref data);

						list.Add(data);
					}

				tags = list.ToArray();
			}
			else if (s.IsWriting)
			{
				foreach (Values.GroupTagData32 data in tags)
					using (s.EnterCursorBookmark(k_element_name))
					{
						var temp = data; // can't pass a foreach value by ref
						Serialize(s, ref temp);
					}
			}
		}
		public static void Serialize<TDoc, TCursor>(TagElementStream<TDoc, TCursor, string> s,
			ref Values.GroupTag32Collection collection, string groupsElementName = "Groups")
			where TDoc : class
			where TCursor : class
		{
			Contract.Requires(!string.IsNullOrEmpty(groupsElementName));

			bool reading = s.IsReading;

			var guid = reading
				? Values.KGuid.Empty
				: collection.Guid;
			var tags = reading
				? null
				// HACK: GroupTags is exposed as a IReadOnlyList, but at the time of this writing is implemented
				// with a GroupTagData32[] as its backing field
				: (Values.GroupTagData32[])collection.GroupTags;

			s.StreamAttributeOpt("guid", ref guid, Predicates.IsNotEmpty);

			using (var bm = s.EnterCursorBookmarkOpt(groupsElementName, tags, Predicates.HasItems)) if (bm.IsNotNull)
				StreamElements(s, ref tags);

			if(reading)
			{
				bool sort = false;
				s.ReadAttributeOpt("sort", ref sort);

				collection = new Values.GroupTag32Collection(guid, sort, tags);
			}
		}
	};
}