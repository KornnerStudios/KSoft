﻿using System;
using System.Collections.Generic;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Values
{
	/// <summary>Group Tag identifier definition using 32-bit storage space</summary>
	public sealed class GroupTagData32 : GroupTagData
	{
		#region Null
		/// <summary>Represents a null value for <see cref="GroupTagData32"/> objects</summary>
		public static readonly GroupTagData32 Null = new GroupTagData32();
		GroupTagData32() : base(4)
		{
			Contract.Assume(mTag.Length == 4);

			mTag[0] = (char)0xFF;
			mTag[1] = (char)0xFF;
			mTag[2] = (char)0xFF;
			mTag[3] = (char)0xFF;
			mID = uint.MaxValue;
			mTagAsString = new string(mTag);
		}
		#endregion
		public static readonly IEqualityComparer<GroupTagData> kEqualityComparer = Null;

		[Contracts.ContractInvariantMethod]
		void ObjectInvariant()	{ Contract.Invariant(mTag.Length == 4); }


		#region ID
		readonly uint mID;
		/// <summary>The four character code translated into a unsigned integer</summary>
		public uint ID { get { return mID; } }
		#endregion

		#region Ctor
		/// <summary>Initialize a 32-bit group tag</summary>
		/// <param name="groupTag">Four character code string</param>
		/// <param name="name">Name of this group tag</param>
		public GroupTagData32(string groupTag, string name) : base(groupTag, name, sizeof(uint))
		{
			Contract.Requires(!string.IsNullOrEmpty(groupTag));
			Contract.Requires(!string.IsNullOrEmpty(name));
			Contract.Requires(groupTag.Length == sizeof(uint));

			Contract.Assume(mTag.Length == sizeof(uint));

			mID = ToUInt(mTag);
		}
		/// <summary>Initialize a 32-bit group tag with a <see cref="Guid"/></summary>
		/// <param name="groupTag">Four character code string</param>
		/// <param name="name">Name of this group tag</param>
		/// <param name="guid">Guid for this group tag</param>
		public GroupTagData32(string groupTag, string name, Guid guid) : base(groupTag, name, guid, sizeof(uint))
		{
			Contract.Requires(!string.IsNullOrEmpty(groupTag));
			Contract.Requires(!string.IsNullOrEmpty(name));
			Contract.Requires(groupTag.Length == sizeof(uint));

			Contract.Assume(mTag.Length == sizeof(uint));

			mID = ToUInt(mTag);
		}
		#endregion


		#region Overrides
		/// <summary>Compares two <see cref="GroupTagData32"/> objects testing their group tags for equality</summary>
		/// <param name="obj">other <see cref="GroupTagData32"/> object</param>
		/// <returns>true if both this object and <paramref name="obj"/> are equal</returns>
		public override bool Equals(object obj)
		{
			if(obj is GroupTagData32)
				return mID == (obj as GroupTagData32).mID;

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
		public static explicit operator uint(GroupTagData32 value)
		{
			Contract.Requires(value != null);

			return value.mID;
		}
		#endregion

		/// <summary>
		/// Takes another four character code and performs a check on it against this object's tag to see if they are completely equal</summary>
		/// <param name="other"></param>
		/// <returns>True if equal to this</returns>
		public override bool Test(char[] other)
		{
			return GroupTagData32.Test(mTag, other);
		}
		/// <summary>Is this <see cref="GroupTagData32"/> equal to the "null" equivalent value?</summary>
		public override bool IsNull	{ get { return object.ReferenceEquals(this, Null); } }

		#region IEquatable & IEqualityComparer Members
		/// <summary>Compares this to another <see cref="GroupTagData32"/> object testing their <see cref="ID"/> fields for equality
		/// </summary>
		/// <param name="other">other <see cref="GroupTagData32"/> object</param>
		/// <returns>true if both this object and <paramref name="obj"/> are equal</returns>
		public override bool Equals(GroupTagData obj)
		{
			if(obj is GroupTagData32)
				return mID == (obj as GroupTagData32).mID;

			return false;
		}
		#endregion

		#region IEndianStreamable Members
		/// <summary>Moves the stream ahead by the sizeof a four character code (4 bytes)</summary>
		/// <param name="s"></param>
		/// <remarks>Doesn't actually read any data from the stream, only seeks forward</remarks>
		public override void Read(IO.EndianReader s)	{ s.Seek(sizeof(uint), System.IO.SeekOrigin.Current); }
		/// <summary>Writes this tag group's four character code</summary>
		/// <param name="s"></param>
		public override void Write(IO.EndianWriter s)	{ s.WriteTag32(mID); }
		#endregion


		#region Util
		/// <summary>Takes a four character code and performs a dword byte swap on it, storing the result in a new four character code</summary>
		/// <param name="tag">value to be byte swapped</param>
		/// <returns>dword byte swapped four character code</returns>
		public static char[] Swap(char[] tag)
		{
			Contract.Requires(tag != null);
			Contract.Requires<ArgumentOutOfRangeException>(tag.Length >= 4);

			Contract.Ensures(Contract.Result<char[]>() != null);
			Contract.Ensures(Contract.Result<char[]>().Length == 4);

			char[] swap = new char[4];
			swap[0] = tag[3];
			swap[1] = tag[2];
			swap[2] = tag[1];
			swap[3] = tag[0];

			return swap;
		}

		/// <summary>Takes two four character codes and performs a check on them to see if they are completely equal</summary>
		/// <param name="tag1"></param>
		/// <param name="tag2"></param>
		/// <returns>True if both are equal</returns>
		public static bool Test(char[] tag1, char[] tag2)
		{
			Contract.Requires(tag1 != null && tag2 != null);
			Contract.Requires<ArgumentOutOfRangeException>(tag1.Length >= 4);
			Contract.Requires<ArgumentOutOfRangeException>(tag2.Length >= 4);

			if (tag1[0] == tag2[0] &&
				tag1[1] == tag2[1] &&
				tag1[2] == tag2[2] &&
				tag1[3] == tag2[3])
				return true;

			return false;
		}

		#region UInt
		/// <summary>Takes a four-cc and converts it into its (unsigned) integer value</summary>
		/// <param name="tag"></param>
		/// <returns></returns>
		/// <remarks>assumes <paramref name="tag"/> is in big-endian order, though in most cases order doesn't matter</remarks>
		public static uint ToUInt(char[] tag)
		{
			Contract.Requires(tag != null);
			Contract.Requires<ArgumentOutOfRangeException>(tag.Length >= 4);

			uint value = (uint)(
					((byte)tag[0] << 24) |
					((byte)tag[1] << 16) |
					((byte)tag[2] << 8) |
					((byte)tag[3])
						);

			if (!System.BitConverter.IsLittleEndian)
				Bitwise.ByteSwap.Swap(ref value);

			return value;
		}

		/// <summary>Takes a four-cc and converts it into its (unsigned) integer value</summary>
		/// <param name="tag"></param>
		/// <returns></returns>
		/// <remarks>assumes <paramref name="tag"/> is in big-endian order, though in most cases order doesn't matter</remarks>
		public static uint ToUInt(string tag)
		{
			Contract.Requires(!string.IsNullOrEmpty(tag));
			Contract.Requires<ArgumentOutOfRangeException>(tag.Length >= 4);

			uint value = (uint)(
					((byte)tag[0] << 24) |
					((byte)tag[1] << 16) |
					((byte)tag[2] << 8) |
					((byte)tag[3])
						);

			if (!System.BitConverter.IsLittleEndian)
				Bitwise.ByteSwap.Swap(ref value);

			return value;
		}

		/// <summary>Takes a (unsigned) integer and converts it into its four-cc value</summary>
		/// <param name="groupTag"></param>
		/// <param name="tag">optional result buffer</param>
		/// <param name="isBigEndian">endian order override</param>
		/// <returns>big-endian ordered four-cc if <paramref name="isBigEndian"/> is true, little-endian if false</returns>
		public static char[] FromUInt(uint groupTag, char[] tag = null, bool isBigEndian = true)
		{
			Contract.Requires(tag == null || tag.Length >= 4);

			Contract.Ensures(Contract.Result<char[]>() != null);
			Contract.Ensures(Contract.Result<char[]>().Length >= 4);

			if(tag == null)
				tag = new char[4];

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

			return tag;
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
		/// <param name="guid"><see cref="Guid"/> for this group tag</param>
		public GroupTagData32Attribute(string groupTag, string name, string guid)
		{
			Contract.Requires(!string.IsNullOrEmpty(groupTag));
			Contract.Requires(!string.IsNullOrEmpty(name));
			Contract.Requires(groupTag.Length == sizeof(uint));

			GroupTag = new GroupTagData32(groupTag, name, new Guid(guid));
		}
	};
}