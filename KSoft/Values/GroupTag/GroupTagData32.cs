using System;
using System.Collections.Generic;

using TagWord = System.UInt32;

namespace KSoft.Values
{
	/// <summary>Group Tag identifier definition using 32-bit storage space</summary>
	public sealed class GroupTagData32 : GroupTagData
	{
		internal const int kExpectedTagLength = sizeof(TagWord);

		#region Null
		/// <summary>Represents a null value for <see cref="GroupTagData32"/> objects</summary>
		public static readonly GroupTagData32 Null = new();
		GroupTagData32() : base(kExpectedTagLength)
		{
			mID = TagWord.MaxValue;
		}
		#endregion
		public static readonly IEqualityComparer<GroupTagData> kEqualityComparer = Null;

		#region ID
		readonly TagWord mID;
		/// <summary>The four character code translated into a unsigned integer</summary>
		public TagWord ID { get { return mID; } }
		#endregion

		#region Ctor
		/// <summary>Initialize a 32-bit group tag</summary>
		/// <param name="groupTag">Four character code string</param>
		/// <param name="name">Name of this group tag</param>
		public GroupTagData32(string groupTag, string name) : base(groupTag, name, kExpectedTagLength)
		{
			System.Diagnostics.Debug.Assert(TagString.Length == kExpectedTagLength);

			mID = ToUInt(TagString.AsSpan());
		}
		/// <summary>Initialize a 32-bit group tag with a <see cref="Guid"/></summary>
		/// <param name="groupTag">Four character code string</param>
		/// <param name="name">Name of this group tag</param>
		/// <param name="uuid">Guid for this group tag</param>
		public GroupTagData32(string groupTag, string name, KGuid uuid) : base(groupTag, name, uuid, kExpectedTagLength)
		{
			System.Diagnostics.Debug.Assert(TagString.Length == kExpectedTagLength);

			mID = ToUInt(TagString.AsSpan());
		}
		#endregion


		#region Overrides
		/// <summary>Compares two <see cref="GroupTagData32"/> objects testing their group tags for equality</summary>
		/// <param name="obj">other <see cref="GroupTagData32"/> object</param>
		/// <returns>true if both this object and <paramref name="obj"/> are equal</returns>
		public override bool Equals(object? obj)
		{
			if (obj is GroupTagData32 g)
			{
				return mID == g.mID;
			}

			return false;
		}

		/// <summary>Returns the hash code for this instance</summary>
		/// <returns>This object's group tag</returns>
		public override int GetHashCode() { return (int)mID; }
		#endregion

		#region Operators
		/// <summary>Returns the group tag in integer form</summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public TagWord ToUInt32() => mID;
		public static explicit operator TagWord(GroupTagData32 value)
		{
			ArgumentNullException.ThrowIfNull(value);

			return value.ToUInt32();
		}
		#endregion

		/// <summary>
		/// Takes another four character code and performs a check on it against this object's tag to see if they are completely equal</summary>
		/// <param name="other"></param>
		/// <returns>True if equal to this</returns>
		public override bool Test(ReadOnlySpan<char> other)
		{
			ValidateTestArgument(other);

			return GroupTagData32.Test(TagString.AsSpan(), other);
		}
		/// <summary>Is this <see cref="GroupTagData32"/> equal to the "null" equivalent value?</summary>
		public override bool IsNull	=> object.ReferenceEquals(this, Null);

		#region IEquatable & IEqualityComparer Members
		/// <summary>Compares this to another <see cref="GroupTagData32"/> object testing their <see cref="ID"/> fields for equality
		/// </summary>
		/// <param name="other">other <see cref="GroupTagData32"/> object</param>
		/// <returns>true if both this object and <paramref name="obj"/> are equal</returns>
		public override bool Equals(GroupTagData? obj)
		{
			if (obj is GroupTagData32 g)
			{
				return mID == g.mID;
			}

			return false;
		}
		#endregion

		#region IEndianStreamable Members
		/// <summary>Moves the stream ahead by the sizeof a four character code (4 bytes)</summary>
		/// <param name="s"></param>
		/// <remarks>Doesn't actually read any data from the stream, only seeks forward</remarks>
		public override void Read(IO.EndianReader s)	=> s.Seek(sizeof(TagWord), System.IO.SeekOrigin.Current);
		/// <summary>Writes this tag group's four character code</summary>
		/// <param name="s"></param>
		public override void Write(IO.EndianWriter s)	=> s.WriteTag32(mID);
		#endregion


		#region Util

		/// <summary>Takes two four character codes and performs a check on them to see if they are completely equal</summary>
		/// <param name="tag1"></param>
		/// <param name="tag2"></param>
		/// <returns>True if equal</returns>
		public static bool Test(ReadOnlySpan<char> tag1, ReadOnlySpan<char> tag2)
		{
			ArgumentOutOfRangeException.ThrowIfLessThan(tag1.Length, kExpectedTagLength, nameof(tag1));
			ArgumentOutOfRangeException.ThrowIfLessThan(tag2.Length, kExpectedTagLength, nameof(tag2));

			return tag1[0] == tag2[0] &&
				tag1[1] == tag2[1] &&
				tag1[2] == tag2[2] &&
				tag1[3] == tag2[3];
		}
		#region UInt
		/// <summary>Takes a four-cc and converts it into its (unsigned) integer value</summary>
		/// <param name="tag"></param>
		/// <returns></returns>
		/// <remarks>assumes <paramref name="tag"/> is in big-endian order, though in most cases order doesn't matter</remarks>
		public static TagWord ToUInt(ReadOnlySpan<char> tag)
		{
			ArgumentOutOfRangeException.ThrowIfLessThan(tag.Length, kExpectedTagLength, nameof(tag));

			var value = (TagWord)(
					((byte)tag[0] << 24) |
					((byte)tag[1] << 16) |
					((byte)tag[2] << 8) |
					((byte)tag[3])
				);

			if (!System.BitConverter.IsLittleEndian)
			{
				value = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(value);
			}

			return value;
		}
		/// <summary>Takes a (unsigned) integer and writes its four-cc value to a destination</summary>
		/// <param name="groupTag"></param>
		/// <param name="tag">Destination buffer</param>
		/// <param name="isBigEndian">endian order override</param>
		/// <remarks>Writes a big-endian ordered four-cc if <paramref name="isBigEndian"/> is true, little-endian if false</remarks>
		public static void FromUInt(TagWord groupTag, Span<char> tag, bool isBigEndian = true)
		{
			ArgumentOutOfRangeException.ThrowIfLessThan(tag.Length, kExpectedTagLength, nameof(tag));

			if (isBigEndian)
			{
				tag[0] = (char)((groupTag & 0xFF000000) >> 24);
				tag[1] = (char)((groupTag & 0x00FF0000) >> 16);
				tag[2] = (char)((groupTag & 0x0000FF00) >>  8);
				tag[3] = (char) (groupTag & 0x000000FF)       ;
			}
			else
			{
				tag[3] = (char)((groupTag & 0xFF000000) >> 24);
				tag[2] = (char)((groupTag & 0x00FF0000) >> 16);
				tag[1] = (char)((groupTag & 0x0000FF00) >>  8);
				tag[0] = (char) (groupTag & 0x000000FF)       ;
			}
		}
		#endregion
		#endregion
	};

	/// <summary>
	/// Attribute for declaring a <see cref="GroupTagData32"/> on a <b>class</b> or <b>struct</b>
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple=false, Inherited=false)]
	public sealed class GroupTagData32Attribute : GroupTagDataAttribute
	{
		#region GroupTag
		/// <summary>Get the <see cref="GroupTagData32"/> this attribute defines</summary>
		public GroupTagData32 GroupTag				{ get; private set; }
		/// <summary>Get the <see cref="GroupTagData"/> this attribute defines</summary>
		public override GroupTagData GroupTagData	{ get { return GroupTag; } }
		#endregion

		/// <summary>Initialize a 32-bit group tag attribute</summary>
		/// <param name="groupTag">Four character code string</param>
		/// <param name="name">Name of this group tag</param>
		/// <param name="uuid"><see cref="Guid"/> for this group tag</param>
		public GroupTagData32Attribute(string groupTag, string name, string uuid)
		{
			Verify.GroupTags.NameAndExactLength(groupTag, name, GroupTagData32.kExpectedTagLength);

			GroupTag = new GroupTagData32(groupTag, name, new KGuid(uuid));
		}
	};
}
