
using System;
using System.Collections.Generic;

namespace KSoft.Values
{
	/// <summary>Base interface for Group Tag identifier definitions</summary>
//	[System.ComponentModel.TypeConverter(typeof(GroupTagDataConverter))]
	public abstract class GroupTagData
		: IO.IEndianStreamable
		, IComparer<GroupTagData>, IComparable<GroupTagData>
		, IEquatable<GroupTagData>, IEqualityComparer<GroupTagData>
		, System.Collections.IComparer, IComparable
	{
		#region Name
		public const string kNullGroupName = TypeExtensions.kNoneDisplayString;
		public const int kGroupNamePadLength = 64;

		readonly string mName;
		/// <summary>Full name of this group</summary>
		public string Name { get => mName; }

		/// <summary>Formats <see cref="Name"/> to a properly (left) aligned string (using blank-white-space)</summary>
		/// <returns><see cref="string.PadLeft"/> on <see cref="Name"/></returns>
		/// <remarks>Pad width is determined by <see cref="kGroupNamePadLength"/></remarks>
		public string NameToLeftPaddedString()	=> mName.PadLeft(kGroupNamePadLength);
		/// <summary> Formats <see cref="Name"/> to a properly (right) aligned string (using blank-white-space)</summary>
		/// <returns><see cref="string.PadRight"/> on <see cref="Name"/></returns>
		/// <remarks>Pad width is determined by <see cref="kGroupNamePadLength"/></remarks>
		public string NameToRightPaddedString()	=> mName.PadRight(kGroupNamePadLength);
		#endregion

		#region Tag
		readonly string mTagAsString;
		readonly char[] mTag;
		/// <summary>The character code of this group</summary>
		[System.ComponentModel.Browsable(false)]
		public char[] Tag { get => mTag; }
		/// <summary>Get the character code of this group as a string</summary>
		public string TagString { get => mTagAsString; }
		#endregion

		/// <summary>Extra data that can be tagged to this Group Tag</summary>
		[System.ComponentModel.Browsable(false)]
		public object? UserData { get; private set; }

		/// <summary>Guid for this group tag</summary>
		public KGuid Uuid	{ get; private set; } = KGuid.Empty;

		#region Ctor
		/// <summary>Only for Null Constructors</summary>
		protected GroupTagData(int expectedLength)
		{
			System.Diagnostics.Debug.Assert(expectedLength > 0);

			mName = kNullGroupName;

			mTag = new char[expectedLength];
			for (int x = 0; x < mTag.Length; x++)
			{
				mTag[x] = (char)0xFF;
			}

			mTagAsString = new string(mTag);
		}
		/// <summary>Initialize a group tag from a character code and name</summary>
		/// <param name="groupTag">Character code string</param>
		/// <param name="name">Name of this group tag</param>
		/// <param name="expectedLength">Expected length of the <paramref name="groupTag"/></param>
		protected GroupTagData(string groupTag, string name, int expectedLength)
		{
			Verify.GroupTags.NameAndExactLength(groupTag, name, expectedLength);
			if (name == kNullGroupName)
			{
				throw new ArgumentException("Name reserved for null group tags", nameof(name));
			}

			mName = name;
			mTagAsString = groupTag;
			mTag = groupTag.ToCharArray();
		}
		/// <summary>Initialize a group tag from a character code, name and <see cref="Guid"/></summary>
		/// <param name="groupTag">Character code string</param>
		/// <param name="name">Name of this group tag</param>
		/// <param name="uuid">Guid for this group tag</param>
		/// <param name="expectedLength">Expected length of the <paramref name="groupTag"/></param>
		protected GroupTagData(string groupTag, string name, KGuid uuid, int expectedLength) : this(groupTag, name, expectedLength)
		{
			Uuid = uuid;
		}
		/// <summary>Specialized ctor for <see cref="GroupTagData64"/> built from two <see cref="GroupTagData32"/></summary>
		/// <param name="maj">First four-character code</param>
		/// <param name="min">Second four-character code</param>
		/// <param name="name">Name of this group tag</param>
		/// <remarks>Constructs a group tag in the form of '<paramref name="maj"/>' + '<paramref name="min"/>'</remarks>
		protected GroupTagData(GroupTagData32 maj, GroupTagData32 min, string name)
		{
			Verify.GroupTags.CompositePair(maj, min, name);
			if (name == kNullGroupName)
			{
				throw new ArgumentException("Name reserved for null group tags", nameof(name));
			}

			mName = name;
			mTagAsString = string.Format(Util.InvariantCultureInfo,
				"{0}{1}", maj.mTagAsString, min.mTagAsString);
			mTag = mTagAsString.ToCharArray();
		}
		/// <summary>Specialized ctor for <see cref="GroupTagData64"/> built from two <see cref="GroupTagData32"/></summary>
		/// <param name="maj">First four-character code</param>
		/// <param name="min">Second four-character code</param>
		/// <param name="name">Name of this group tag</param>
		/// <param name="uuid">Guid for this group tag</param>
		/// <remarks>Constructs a group tag in the form of '<paramref name="maj"/>' + '<paramref name="min"/>'</remarks>
		protected GroupTagData(GroupTagData32 maj, GroupTagData32 min, string name, KGuid uuid) : this(maj, min, name)
		{
			Uuid = uuid;
		}
		#endregion


		#region Overrides
		public override string ToString() => string.Format(Util.InvariantCultureInfo,
			"['{0," + (this is GroupTagData32 ? "4" : "8") + "}'  {1}]",
			TagString, Name);

		public override int GetHashCode() => Name.GetHashCode();

		public override bool Equals(object? obj)
		{
			if (obj is GroupTagData g)
			{
				return this.Equals(g);
			}

			return false;
		}
		#endregion

		#region Operators
		public static bool operator ==(GroupTagData? a, GroupTagData? b) => a!.Equals(b);
		public static bool operator !=(GroupTagData? a, GroupTagData? b) => !(a == b);

		/// <summary>Returns the name of this group tag</summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static explicit operator string(GroupTagData value)
		{
			ArgumentNullException.ThrowIfNull(value);

			return value.mName;
		}

		/// <summary>Returns the group tag in char[] form</summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static explicit operator char[](GroupTagData value)
		{
			ArgumentNullException.ThrowIfNull(value);

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
		/// <summary>Validate input for concrete <see cref="Test"/> implementations.</summary>
		/// <param name="other">Character code to compare against this group tag.</param>
		protected void ValidateTestArgument(char[] other)
		{
			ArgumentNullException.ThrowIfNull(other);
			ArgumentOutOfRangeException.ThrowIfNotEqual(other.Length, Tag.Length, nameof(other));
		}
		/// <summary>Is this Group Tag equal to the "null" equivalent value?</summary>
		public abstract bool IsNull { get; }

		#region Comparing members (ID based)
		/// <summary>Compare this group tag's ID to another</summary>
		/// <param name="other"></param>
		/// <returns>this ID - <paramref name="other"/>'s ID</returns>
		public int CompareId(GroupTagData other)
		{
			ArgumentNullException.ThrowIfNull(other);

			return this.GetHashCode() - other.GetHashCode();
		}
		#endregion


		#region Compare Members (name based)
		/// <summary>Does a comparison based on the tag group's names</summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		/// <seealso cref="String.Compare(String, String, bool)"/>
		public int Compare(GroupTagData? x, GroupTagData? y)
		{
			System.Diagnostics.Debug.Assert(x != null);
			System.Diagnostics.Debug.Assert(y != null);

			return string.Compare(x!.mName, y!.mName, StringComparison.OrdinalIgnoreCase);
		}

		/// <summary>Does a comparison based on the group tag's names</summary>
		/// <param name="other"></param>
		/// <returns></returns>
		/// <seealso cref="String.Compare(String, String, bool)"/>
		public int CompareTo(GroupTagData? other)
		{
			System.Diagnostics.Debug.Assert(other != null);

			return string.Compare(this.mName, other!.mName, StringComparison.OrdinalIgnoreCase);
		}

		/// <summary>Does a comparison based on the tag group's names</summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		/// <seealso cref="String.Compare(String, String, bool)"/>
		public int Compare(object? x, object? y)
		{
			System.Diagnostics.Debug.Assert(x != null);
			System.Diagnostics.Debug.Assert(y != null);

			return Compare((GroupTagData)x!, (GroupTagData)y!);
		}
		/// <summary>Does a comparison based on the tag group's names</summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		/// <seealso cref="String.Compare(String, String, bool)"/>
		public int CompareTo(object? obj)
		{
			System.Diagnostics.Debug.Assert(obj != null);

			return CompareTo((GroupTagData)obj!);
		}
		#endregion

		#region IEquatable & IEqualityComparer Members
		/// <summary>Compares this to another <see cref="GroupTagData"/> object testing their "ID" fields for equality</summary>
		/// <param name="other"></param>
		/// <returns>true if both this object and <paramref name="obj"/> are equal</returns>
		public abstract bool Equals(GroupTagData? other);
		/// <summary>Compares two <see cref="GroupTagData"/> objects testing their "ID" fields for equality</summary>
		/// <param name="x">left-hand value for comparison expression</param>
		/// <param name="y">right-hand value for comparison expression</param>
		/// <returns>true if both <paramref name="x"/> and <paramref name="y"/> are equal</returns>
		public bool Equals(GroupTagData? x, GroupTagData? y)
		{
			System.Diagnostics.Debug.Assert(x != null);
			System.Diagnostics.Debug.Assert(y != null);

			return x!.Equals(y);
		}
		/// <summary>Returns the hash code for this instance</summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public int GetHashCode(GroupTagData obj)
		{
			System.Diagnostics.Debug.Assert(obj != null);

			return obj.GetHashCode();
		}
		#endregion

		#region IEndianStreamable Members
		public abstract void Read(IO.EndianReader s);

		public abstract void Write(IO.EndianWriter s);
		#endregion
	};

	/// <summary>Base attribute for declaring a <see cref="GroupTagData"/> on a <b>class</b> or <b>struct</b></summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
	public abstract class GroupTagDataAttribute : Attribute
	{
		/// <summary>Get the <see cref="GroupTagData"/> this attribute defines</summary>
		public abstract GroupTagData GroupTagData { get; }
	};
}
