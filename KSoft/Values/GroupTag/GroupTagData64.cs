using System;
using System.Collections.Generic;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Values
{
	/// <summary>Group Tag identifier definition using 64-bit storage space</summary>
	public sealed class GroupTagData64 : GroupTagData
	{
		#region Null
		/// <summary>Represents a null value for <see cref="GroupTagData64"/> objects</summary>
		public static readonly GroupTagData64 Null = new GroupTagData64();
		GroupTagData64() : base(8)
		{
			Contract.Assume(mTag.Length == 8);

			mTag[0] = (char)0xFF;
			mTag[1] = (char)0xFF;
			mTag[2] = (char)0xFF;
			mTag[3] = (char)0xFF;
			mTag[4] = (char)0xFF;
			mTag[5] = (char)0xFF;
			mTag[6] = (char)0xFF;
			mTag[7] = (char)0xFF;
			mID = ulong.MaxValue;
			mTagAsString = new string(mTag);
		}
		#endregion
		public static readonly IEqualityComparer<GroupTagData> kEqualityComparer = Null;

		[Contracts.ContractInvariantMethod]
		void ObjectInvariant()
		{
			Contract.Invariant(mTag.Length == 8);
		}


		#region ID
		readonly ulong mID;
		/// <summary>The eight character code translated into a unsigned integer</summary>
		public ulong ID { get { return mID; } }
		#endregion

		#region Ctor
		/// <summary>Initialize a 64-bit group tag</summary>
		/// <param name="groupTag">Eight character code string</param>
		/// <param name="name">Name of this group tag</param>
		public GroupTagData64(string groupTag, string name) : base(groupTag, name, sizeof(ulong))
		{
			Contract.Requires(!string.IsNullOrEmpty(groupTag));
			Contract.Requires(!string.IsNullOrEmpty(name));
			Contract.Requires(groupTag.Length == sizeof(ulong));

			Contract.Assume(mTag.Length == sizeof(ulong));

			mID = ToULong(mTag);
		}
		/// <summary>Initialize a 64-bit group tag with a <see cref="Guid"/></summary>
		/// <param name="groupTag">Eight character code string</param>
		/// <param name="name">Name of this group tag</param>
		/// <param name="guid">Guid for this group tag</param>
		public GroupTagData64(string groupTag, string name, Guid guid) : base(groupTag, name, guid, sizeof(ulong))
		{
			Contract.Requires(!string.IsNullOrEmpty(groupTag));
			Contract.Requires(!string.IsNullOrEmpty(name));
			Contract.Requires(groupTag.Length == sizeof(ulong));

			Contract.Assume(mTag.Length == sizeof(ulong));

			mID = ToULong(mTag);
		}
		/// <summary>Specialized ctor for initialing from two <see cref="GroupTagData32"/> instances</summary>
		/// <param name="maj">First four-character code</param>
		/// <param name="min">Second four-character code</param>
		/// <param name="name">Name of this <see cref="GroupTagData32"/> pair</param>
		/// <remarks>Constructs a group tag in the form of '<paramref name="maj"/>' + '<paramref name="min"/>'</remarks>
		public GroupTagData64(GroupTagData32 maj, GroupTagData32 min, string name) : base(maj, min, name)
		{
			Contract.Requires(maj != null && maj != GroupTagData32.Null);
			Contract.Requires(min != null && min != GroupTagData32.Null);
			Contract.Requires(!string.IsNullOrEmpty(name));

			Contract.Assume(mTag.Length == sizeof(ulong));

			mID = ToULong(mTag);
		}
		/// <summary>Specialized ctor for initialing from two <see cref="GroupTagData32"/> instances along with a <see cref="Guid"/></summary>
		/// <param name="maj">First four-character code</param>
		/// <param name="min">Second four-character code</param>
		/// <param name="name">Name of this <see cref="GroupTagData32"/> pair</param>
		/// <param name="guid">Guid for this group tag</param>
		/// <remarks>Constructs a group tag in the form of '<paramref name="maj"/>' + '<paramref name="min"/>'</remarks>
		public GroupTagData64(GroupTagData32 maj, GroupTagData32 min, string name, Guid guid) : base(maj, min, name, guid)
		{
			Contract.Requires(maj != null && maj != GroupTagData32.Null);
			Contract.Requires(min != null && min != GroupTagData32.Null);
			Contract.Requires(!string.IsNullOrEmpty(name));

			Contract.Assume(mTag.Length == sizeof(ulong));

			mID = ToULong(mTag);
		}
		#endregion

		#region Overrides
		/// <summary>Compares two <see cref="GroupTagData64"/> objects testing their group tags for equality</summary>
		/// <param name="obj">other <see cref="GroupTagData64"/> object</param>
		/// <returns>true if both this object and <paramref name="obj"/> are equal</returns>
		public override bool Equals(object obj)
		{
			if(obj is GroupTagData64)
				return mID == (obj as GroupTagData64).mID;

			return false;
		}

		/// <summary>Returns the hash code for this instance</summary>
		/// <returns>This object's group tag</returns>
		public override int GetHashCode()
		{
			int hi = (int)(mID >> 32);
			int lo = (int)(mID & 0xFFFFFFFF);

			return hi ^ lo;
		}
		#endregion

		#region Operators
		/// <summary>Returns the group tag in integer form</summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static explicit operator ulong(GroupTagData64 value)
		{
			Contract.Requires(value != null);

			return value.mID;
		}
		#endregion

		/// <summary>
		/// Takes another eight character code and performs a check on it against this object's tag to see if they are completely equal</summary>
		/// <param name="other"></param>
		/// <returns>True if equal to this</returns>
		public override bool Test(char[] other)
		{
			return GroupTagData64.Test(mTag, other);
		}
		/// <summary>Is this <see cref="GroupTagData64"/> equal to the "null" equivalent value?</summary>
		public override bool IsNull	{ get {
			return object.ReferenceEquals(this, Null);
		} }

		#region IEquatable & IEqualityComparer Members
		/// <summary>Compares this to another <see cref="GroupTagData64"/> object testing their <see cref="ID"/> fields for equality</summary>
		/// <param name="other">other <see cref="GroupTagData64"/> object</param>
		/// <returns>true if both this object and <paramref name="obj"/> are equal</returns>
		public override bool Equals(GroupTagData obj)
		{
			if(obj is GroupTagData64)
				return mID == (obj as GroupTagData64).mID;

			return false;
		}
		#endregion

		#region IEndianStreamable Members
		/// <summary>Moves the stream ahead by the sizeof a eight character code (8 bytes)</summary>
		/// <param name="s"></param>
		/// <remarks>Doesn't actually read any data from the stream, only seeks forward</remarks>
		public override void Read(IO.EndianReader s)	{ s.Seek(sizeof(ulong), System.IO.SeekOrigin.Current); }
		/// <summary>Writes this tag group's eight character code</summary>
		/// <param name="s"></param>
		public override void Write(IO.EndianWriter s)	{ s.WriteTag64(mID); }
		#endregion


		#region Util
		/// <summary>Takes a eight character code and performs a qword byte swap on it, storing the result in a new four character code</summary>
		/// <param name="tag">value to be byte swapped</param>
		/// <returns>qword byte swapped eight character code</returns>
		public static char[] Swap(char[] tag)
		{
			Contract.Requires(tag != null);
			Contract.Requires<ArgumentOutOfRangeException>(tag.Length >= 8);

			Contract.Ensures(Contract.Result<char[]>() != null);
			Contract.Ensures(Contract.Result<char[]>().Length == 8);

			char[] swap = new char[8];
			swap[0] = tag[3];
			swap[1] = tag[2];
			swap[2] = tag[1];
			swap[3] = tag[0];

			swap[4 + 0] = tag[4 + 3];
			swap[4 + 1] = tag[4 + 2];
			swap[4 + 2] = tag[4 + 1];
			swap[4 + 3] = tag[4 + 0];

			return swap;
		}

		/// <summary>Takes two four character codes and performs a check on them to see if they are completely equal</summary>
		/// <param name="tag1"></param>
		/// <param name="tag2"></param>
		/// <returns>True if both are equal</returns>
		public static bool Test(char[] tag1, char[] tag2)
		{
			Contract.Requires(tag1 != null && tag2 != null);
			Contract.Requires<ArgumentOutOfRangeException>(tag1.Length >= 8);
			Contract.Requires<ArgumentOutOfRangeException>(tag2.Length >= 8);

			if (tag1[0] == tag2[0] &&
				tag1[1] == tag2[1] &&
				tag1[2] == tag2[2] &&
				tag1[3] == tag2[3])
				if (tag1[4 + 0] == tag2[4 + 0] &&
					tag1[4 + 1] == tag2[4 + 1] &&
					tag1[4 + 2] == tag2[4 + 2] &&
					tag1[4 + 3] == tag2[4 + 3])
					return true;

			return false;
		}

		#region ULong
		/// <summary>Takes a eight-cc and converts it into its (unsigned) integer value</summary>
		/// <param name="tag"></param>
		/// <returns></returns>
		/// <remarks>assumes <paramref name="tag"/> is in big-endian order, though in most cases order doesn't matter</remarks>
		public static ulong ToULong(char[] tag)
		{
			Contract.Requires(tag != null);
			Contract.Requires<ArgumentOutOfRangeException>(tag.Length >= 8);

			// high bits
			ulong value = (ulong)(
						((byte)tag[0] << 24) |
						((byte)tag[1] << 16) |
						((byte)tag[2] << 8) |
						((byte)tag[3])
					);
			// low bits
			value <<= 32;
			value = (ulong)(
						((byte)tag[4 + 0] << 24) |
						((byte)tag[4 + 1] << 16) |
						((byte)tag[4 + 2] << 8) |
						((byte)tag[4 + 3])
					);

			if (!System.BitConverter.IsLittleEndian)
				Bitwise.ByteSwap.Swap(ref value);

			return value;
		}

		/// <summary>Takes a eight-cc and converts it into its (unsigned) integer value</summary>
		/// <param name="tag"></param>
		/// <returns></returns>
		/// <remarks>assumes <paramref name="tag"/> is in big-endian order, though in most cases order doesn't matter</remarks>
		public static ulong ToULong(string tag)
		{
			Contract.Requires(!string.IsNullOrEmpty(tag));
			Contract.Requires<ArgumentOutOfRangeException>(tag.Length >= 8);

			// high bits
			ulong value = (ulong)(
						((byte)tag[0] << 24) |
						((byte)tag[1] << 16) |
						((byte)tag[2] << 8) |
						((byte)tag[3])
					);
			// low bits
			value <<= 32;
			value = (ulong)(
						((byte)tag[4 + 0] << 24) |
						((byte)tag[4 + 1] << 16) |
						((byte)tag[4 + 2] << 8) |
						((byte)tag[4 + 3])
					);

			if (!System.BitConverter.IsLittleEndian)
				Bitwise.ByteSwap.Swap(ref value);

			return value;
		}

		/// <summary>Takes a (unsigned) integer and converts it into its eight-cc value</summary>
		/// <param name="groupTag"></param>
		/// <param name="tag">optional result buffer</param>
		/// <param name="isBigEndian">endian order override</param>
		/// <returns>big-endian ordered eight-cc if <paramref name="isBigEndian"/> is true, little-endian if false</returns>
		public static char[] FromULong(ulong groupTag, char[] tag = null, bool isBigEndian = true)
		{
			Contract.Requires(tag == null || tag.Length >= 8);

			Contract.Ensures(Contract.Result<char[]>() != null);
			Contract.Ensures(Contract.Result<char[]>().Length >= 8);

			if (tag == null) tag = new char[8];

			if (isBigEndian)
			{
				// high bits
				tag[0] = (char)((groupTag & 0xFF000000) >> 24);
				tag[1] = (char)((groupTag & 0x00FF0000) >> 16);
				tag[2] = (char)((groupTag & 0x0000FF00) >>  8);
				tag[3] = (char) (groupTag & 0x000000FF)       ;
				// low bits
				groupTag >>= 32;
				tag[4+0] = (char)((groupTag & 0xFF000000) >> 24);
				tag[4+1] = (char)((groupTag & 0x00FF0000) >> 16);
				tag[4+2] = (char)((groupTag & 0x0000FF00) >>  8);
				tag[4+3] = (char) (groupTag & 0x000000FF)       ;
			}
			else
			{
				// high bits
				tag[3] = (char)((groupTag & 0xFF000000) >> 24);
				tag[2] = (char)((groupTag & 0x00FF0000) >> 16);
				tag[1] = (char)((groupTag & 0x0000FF00) >>  8);
				tag[0] = (char) (groupTag & 0x000000FF)       ;
				// low bits
				groupTag >>= 32;
				tag[4+3] = (char)((groupTag & 0xFF000000) >> 24);
				tag[4+2] = (char)((groupTag & 0x00FF0000) >> 16);
				tag[4+1] = (char)((groupTag & 0x0000FF00) >>  8);
				tag[4+0] = (char) (groupTag & 0x000000FF)       ;
			}

			return tag;
		}
		#endregion
		#endregion
	};
	/// <summary>Attribute for declaring a <see cref="GroupTagData64"/> on a <b>class</b> or <b>struct</b></summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple=false, Inherited=false)]
	public sealed class GroupTagData64Attribute : GroupTagDataAttribute
	{
		#region GroupTag
		/// <summary>Get the <see cref="GroupTagData64"/> this attribute defines</summary>
		public GroupTagData64 GroupTag				{ get; private set; }
		/// <summary>Get the <see cref="GroupTagData"/> this attribute defines</summary>
		public override GroupTagData GroupTagData	{ get { return GroupTag; } }
		#endregion

		/// <summary>Initialize a 64-bit group tag attribute</summary>
		/// <param name="groupTag">Eight character code string</param>
		/// <param name="name">Name of this group tag</param>
		/// <param name="guid"><see cref="Guid"/> for this group tag</param>
		public GroupTagData64Attribute(string groupTag, string name, string guid)
		{
			Contract.Requires(!string.IsNullOrEmpty(groupTag));
			Contract.Requires(!string.IsNullOrEmpty(name));
			Contract.Requires(groupTag.Length == sizeof(ulong));

			GroupTag = new GroupTagData64(groupTag, name, new Guid(guid));
		}
	};
}