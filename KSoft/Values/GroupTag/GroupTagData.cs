using System;
using System.Collections;
using System.Collections.Generic;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Values
{
	/// <summary>Base interface for Group Tag identifier definitions</summary>
	[Contracts.ContractClass(typeof(GroupTagDataContract))]
//	[System.ComponentModel.TypeConverter(typeof(GroupTagDataConverter))]
	public abstract class GroupTagData : IO.IEndianStreamable, 
		IComparer<GroupTagData>, IComparable<GroupTagData>,
		IEquatable<GroupTagData>, IEqualityComparer<GroupTagData>,
		System.Collections.IComparer, IComparable
	{
		#region Name
		public const int kGroupNamePadLength = 64;

		readonly string mName;
		/// <summary>Full name of this group</summary>
		public string Name { get { return mName; } }

		/// <summary>Formats <see cref="Name"/> to a properly (left) aligned string (using blank-white-space)</summary>
		/// <returns><see cref="string.PadLeft"/> on <see cref="Name"/></returns>
		/// <remarks>Pad width is determined by <see cref="kGroupNamePadLength"/></remarks>
		public string NameToLeftPaddedString()	{ return mName.PadLeft(kGroupNamePadLength); }
		/// <summary> Formats <see cref="Name"/> to a properly (right) aligned string (using blank-white-space)</summary>
		/// <returns><see cref="string.PadRight"/> on <see cref="Name"/></returns>
		/// <remarks>Pad width is determined by <see cref="kGroupNamePadLength"/></remarks>
		public string NameToRightPaddedString()	{ return mName.PadRight(kGroupNamePadLength); }
		#endregion

		#region Tag
		readonly string mTagAsString;
		readonly char[] mTag;
		/// <summary>The character code of this group</summary>
		[System.ComponentModel.Browsable(false)]
		public char[] Tag { get { return mTag; } }
		/// <summary>Get the character code of this group as a string</summary>
		public string TagString { get { return mTagAsString; } }
		#endregion

		/// <summary>Extra data that can be tagged to this Group Tag</summary>
		[System.ComponentModel.Browsable(false)]
		public object UserData { get; private set; }

		#region Guid
		readonly KGuid mGuid = KGuid.Empty;
		/// <summary>Guid for this group tag</summary>
		public KGuid Guid	{ get { return mGuid; } }
		#endregion

#if false // ObjectInvariant moot, as all non-user properties are readonly
		[Contracts.ContractInvariantMethod]
		void ObjectInvariant()
		{
			Contract.Invariant(!string.IsNullOrEmpty(mName));
			Contract.Invariant(mTag != null);
			Contract.Invariant(!string.IsNullOrEmpty(mTagAsString));
		}
#endif

		#region Ctor
		/// <summary>Only for Null Constructors</summary>
		protected GroupTagData(int expectedLength)
		{
			Contract.Assume(expectedLength > 0);

			mName = "none";

			mTag = new char[expectedLength];
			for (int x = 0; x < mTag.Length; x++)
				mTag[x] = (char)0xFF;

			mTagAsString = new string(mTag);
		}
		/// <summary>Initialize a group tag from a character code and name</summary>
		/// <param name="groupTag">Character code string</param>
		/// <param name="name">Name of this group tag</param>
		/// <param name="expectedLength">Expected length of the <paramref name="groupTag"/></param>
		protected GroupTagData(string groupTag, string name, int expectedLength)
		{
#if false
			Contract.Requires(!string.IsNullOrEmpty(groupTag));
			Contract.Requires(!string.IsNullOrEmpty(name));
			Contract.Requires<ArgumentOutOfRangeException>(groupTag.Length == expectedLength);
#endif

			mName = name;
			mTagAsString = groupTag;
			mTag = groupTag.ToCharArray();
		}
		/// <summary>Initialize a group tag from a character code, name and <see cref="Guid"/></summary>
		/// <param name="groupTag">Character code string</param>
		/// <param name="name">Name of this group tag</param>
		/// <param name="guid">Guid for this group tag</param>
		/// <param name="expectedLength">Expected length of the <paramref name="groupTag"/></param>
		protected GroupTagData(string groupTag, string name, KGuid guid, int expectedLength) : this(groupTag, name, expectedLength)
		{
#if false
			Contract.Requires(!string.IsNullOrEmpty(groupTag));
			Contract.Requires(!string.IsNullOrEmpty(name));
			Contract.Requires<ArgumentOutOfRangeException>(groupTag.Length == expectedLength);
			Contract.Requires(guid != KGuid.Empty);
#endif

			mGuid = guid;
		}
		/// <summary>Specialized ctor for <see cref="GroupTagData64"/> built from two <see cref="GroupTagData32"/></summary>
		/// <param name="maj">First four-character code</param>
		/// <param name="min">Second four-character code</param>
		/// <param name="name">Name of this group tag</param>
		/// <remarks>Constructs a group tag in the form of '<paramref name="maj"/>' + '<paramref name="min"/>'</remarks>
		protected GroupTagData(GroupTagData32 maj, GroupTagData32 min, string name)
		{
#if false
 			Contract.Requires(maj != null && maj != GroupTagData32.Null);
			Contract.Requires(min != null && min != GroupTagData32.Null);
			Contract.Requires(!string.IsNullOrEmpty(name));
#endif

			mName = name;
			mTagAsString = string.Format("{0}{1}", maj.mTagAsString, min.mTagAsString);
			mTag = mTagAsString.ToCharArray();
		}
		/// <summary>Specialized ctor for <see cref="GroupTagData64"/> built from two <see cref="GroupTagData32"/></summary>
		/// <param name="maj">First four-character code</param>
		/// <param name="min">Second four-character code</param>
		/// <param name="name">Name of this group tag</param>
		/// <param name="guid">Guid for this group tag</param>
		/// <remarks>Constructs a group tag in the form of '<paramref name="maj"/>' + '<paramref name="min"/>'</remarks>
		protected GroupTagData(GroupTagData32 maj, GroupTagData32 min, string name, KGuid guid) : this(maj, min, name)
		{
#if false
			Contract.Requires(maj != null && maj != GroupTagData32.Null);
			Contract.Requires(min != null && min != GroupTagData32.Null);
			Contract.Requires(!string.IsNullOrEmpty(name));
			Contract.Requires(guid != KGuid.Empty);
#endif

			mGuid = guid;
		}
		#endregion


		#region Overrides
		public override string ToString()
		{
			return string.Format("['{0," + (this is GroupTagData32 ? "4" : "8") + "}'  {1}]", 
				TagString, Name);
		}
		#endregion

		#region Operators
		/// <summary>Returns the name of this group tag</summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static explicit operator string(GroupTagData value)
		{
			Contract.Requires(value != null);

			return value.mName;
		}

		/// <summary>Returns the group tag in char[] form</summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static explicit operator char[](GroupTagData value)
		{
			Contract.Requires(value != null);

			return value.mTag;
		}
		#endregion

		/// <summary>
		/// Takes another character code and performs a check on it against this
		/// object's tag to see if they are completely equal
		/// </summary>
		/// <param name="other"></param>
		/// <returns>True if equal to this</returns>
		public abstract bool Test(char[] other);
		/// <summary>Is this Group Tag equal to the "null" equivalent value?</summary>
		public abstract bool IsNull { get; }

		#region Comparing members (ID based)
		/// <summary>Compare this group tag's ID to another</summary>
		/// <param name="other"></param>
		/// <returns>this ID - <paramref name="other"/>'s ID</returns>
		public int CompareId(GroupTagData other)
		{
			Contract.Requires(other != null);

			return this.GetHashCode() - other.GetHashCode();
		}
		#endregion


		#region Compare Members (name based)
		/// <summary>Does a comparison based on the tag group's names</summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		/// <seealso cref="String.Compare(String, String, bool)"/>
		public int Compare(GroupTagData x, GroupTagData y)
		{
			Contract.Assume(x != null);
			Contract.Assume(y != null);

			return string.Compare(x.mName, y.mName, true);
		}

		/// <summary>Does a comparison based on the group tag's names</summary>
		/// <param name="other"></param>
		/// <returns></returns>
		/// <seealso cref="String.Compare(String, String, bool)"/>
		public int CompareTo(GroupTagData other)
		{
			Contract.Assume(other != null);

			return string.Compare(this.mName, other.mName, true);
		}

		/// <summary>Does a comparison based on the tag group's names</summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		/// <seealso cref="String.Compare(String, String, bool)"/>
		public int Compare(object x, object y)
		{
			Contract.Assume(x != null);
			Contract.Assume(y != null);

			return Compare((GroupTagData)x, (GroupTagData)y);
		}
		/// <summary>Does a comparison based on the tag group's names</summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		/// <seealso cref="String.Compare(String, String, bool)"/>
		public int CompareTo(object obj)
		{
			Contract.Assume(obj != null);

			return CompareTo((GroupTagData)obj);
		}
		#endregion

		#region IEquatable & IEqualityComparer Members
		/// <summary>Compares this to another <see cref="GroupTagData"/> object testing their "ID" fields for equality</summary>
		/// <param name="other"></param>
		/// <returns>true if both this object and <paramref name="obj"/> are equal</returns>
		public abstract bool Equals(GroupTagData other);
		/// <summary>Compares two <see cref="GroupTagData"/> objects testing their "ID" fields for equality</summary>
		/// <param name="x">left-hand value for comparison expression</param>
		/// <param name="y">right-hand value for comparison expression</param>
		/// <returns>true if both <paramref name="x"/> and <paramref name="y"/> are equal</returns>
		public bool Equals(GroupTagData x, GroupTagData y)
		{
			Contract.Assume(x != null);
			Contract.Assume(y != null);

			return x.Equals(y);
		}
		/// <summary>Returns the hash code for this instance</summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public int GetHashCode(GroupTagData obj)
		{
			Contract.Assume(obj != null);

			return obj.GetHashCode();
		}
		#endregion

		#region IEndianStreamable Members
		public abstract void Read(IO.EndianReader s);

		public abstract void Write(IO.EndianWriter s);
		#endregion
	};
	[Contracts.ContractClassFor(typeof(GroupTagData))]
	abstract class GroupTagDataContract : GroupTagData
	{
		GroupTagDataContract() : base(4) {}

		public override bool Test(char[] other)
		{
			Contract.Requires(other != null);
			Contract.Requires<ArgumentOutOfRangeException>(other.Length == Tag.Length);

			throw new NotImplementedException();
		}

		public override bool Equals(GroupTagData other)
		{
			Contract.Requires(other != null);

			throw new NotImplementedException();
		}
	};

	/// <summary>Base attribute for declaring a <see cref="GroupTagData"/> on a <b>class</b> or <b>struct</b></summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
	public abstract class GroupTagDataAttribute : Attribute
	{
		/// <summary>Get the <see cref="GroupTagData"/> this attribute defines</summary>
		public abstract GroupTagData GroupTagData { get; }
	};
}