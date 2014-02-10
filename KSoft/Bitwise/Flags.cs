using System;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Bitwise
{
	/// <summary>Static class for manipulating integer bit-vectors</summary>
	public static partial class Flags
	{
		#region Test
		#region 16-bit
		/// <summary>Returns true if <paramref name="flag"/> is active in <paramref name="value"/></summary>
		/// <param name="value">Value to test in</param>
		/// <param name="flag">Value to test for</param>
		/// <returns>(<paramref name="value"/> &amp; <paramref name="flag"/>) != 0</returns>
		[Contracts.Pure]
		public static bool Test(short value, short flag) { return (value & flag) == flag; }

		/// <summary>Returns true if <paramref name="flag"/> is active in <paramref name="value"/></summary>
		/// <param name="value">Value to test in</param>
		/// <param name="flag">Value to test for</param>
		/// <returns>(<paramref name="value"/> &amp; <paramref name="flag"/>) != 0</returns>
		[Contracts.Pure]
		public static bool Test(ushort value, ushort flag) { return (value & flag) == flag; }
		#endregion

		#region 32-bit
		/// <summary>Returns true if <paramref name="flag"/> is active in <paramref name="value"/></summary>
		/// <param name="value">Value to test in</param>
		/// <param name="flag">Value to test for</param>
		/// <returns>(<paramref name="value"/> &amp; <paramref name="flag"/>) != 0</returns>
		[Contracts.Pure]
		public static bool Test(int value, int flag) { return (value & flag) == flag; }

		/// <summary>Returns true if <paramref name="flag"/> is active in <paramref name="value"/></summary>
		/// <param name="value">Value to test in</param>
		/// <param name="flag">Value to test for</param>
		/// <returns>(<paramref name="value"/> &amp; <paramref name="flag"/>) != 0</returns>
		[Contracts.Pure]
		public static bool Test(uint value, uint flag) { return (value & flag) == flag; }

		/// <summary>Returns true if all the flags in <paramref name="flags"/> are active in <paramref name="value"/></summary>
		/// <param name="value">Value to test in</param>
		/// <param name="flags">Values to test for</param>
		/// <returns>Returns true if ALL the flag values in <paramref name="flags"/> are set in <paramref name="value"/></returns>
		[Contracts.Pure]
		public static bool Test(uint value, params uint[] flags)
		{
			Contract.Requires(flags != null);

			bool ret = false;
			foreach (uint i in flags)
				ret = ret & Test(value, i);
			return ret;
		}

		/// <summary>Returns true if any one of the flags in <paramref name="flags"/> are active in <paramref name="value"/></summary>
		/// <param name="value"></param>
		/// <param name="flags"></param>
		/// <returns>Returns true if any (one, some, or all) flag values in <paramref name="flags"/> are set in <paramref name="value"/></returns>
		[Contracts.Pure]
		public static bool TestAny(uint value, params uint[] flags)
		{
			Contract.Requires(flags != null);

			foreach (uint i in flags)
				if (Test(value, i)) return true;

			return false;
		}
		#endregion

		/// <summary>Returns true if <paramref name="flag"/> is active in <paramref name="value"/></summary>
		/// <param name="value">Value to test in</param>
		/// <param name="flag">Value to test for</param>
		/// <returns>(<paramref name="value"/> &amp; <paramref name="flag"/>) != 0</returns>
		[Contracts.Pure]
		public static bool Test(ulong value, ulong flag) { return (value & flag) == flag; }

		/// <summary>Returns true if all the flags in <paramref name="flags"/> are active in <paramref name="value"/></summary>
		/// <param name="value">Value to test in</param>
		/// <param name="flags">Values to test for</param>
		/// <returns>Returns true if ALL the flag values in <paramref name="flags"/> are set in <paramref name="value"/></returns>
		[Contracts.Pure]
		public static bool Test(ulong value, params ulong[] flags)
		{
			Contract.Requires(flags != null);

			bool ret = false;
			foreach (ulong i in flags)
				ret = ret & Test(value, i);
			return ret;
		}

		/// <summary>Returns true if any one of the flags in <paramref name="flags"/> are active in <paramref name="value"/></summary>
		/// <param name="value"></param>
		/// <param name="flags"></param>
		/// <returns>Returns true if any (one, some, or all) flag values in <paramref name="flags"/> are set in <paramref name="value"/></returns>
		[Contracts.Pure]
		public static bool TestAny(ulong value, params ulong[] flags)
		{
			Contract.Requires(flags != null);

			foreach (ulong i in flags)
				if (Test(value, i)) return true;

			return false;
		}
		#endregion
	};

	/// <summary>Encases a 32-bit bit-vector into a reference object, with bit-manipulator methods</summary>
	/// <remarks>
	/// By using a reference object to store the underlying value, we can declare properties in 
	/// classes which use this type and then do things like:
	/// <code>obj.Flags.Reset(0);</code>
	/// With [Flags] retaining the new value. If this were a value type object, the above code 
	/// wouldn't affect [obj]'s [Flags] property because [Flags] would be RETURNING a value, not a reference
	/// </remarks>
	public sealed class Flags32
	{
		uint mValue;

		/// <summary>Reset the internal value with another</summary>
		/// <param name="value"></param>
		public void Reset(uint value)				{ mValue = value; }
		/// <summary>Reset the internal value with an array of bits</summary>
		/// <param name="values">Array of bit values</param>
		public void Reset(params uint[] values)
		{
			Contract.Requires(values != null);

			foreach (uint f in values) Add(f);
		}

		/// <summary>Implicitly case an unsigned integer into a <see cref="Flags32"/> representation</summary>
		/// <param name="value"></param>
		/// <returns><paramref name="value"/> as a <see cref="Flags32"/> object</returns>
		public static implicit operator uint(Flags32 value)
		{
			Contract.Requires(value != null);

			return value.mValue;
		}

		/// <summary>Construct a flags value from a 32-bit bit-vector</summary>
		/// <param name="value"></param>
		public Flags32(uint value)					{ Reset(value); }
		/// <summary>Construct a flags value from an array of 32-bit bit-vectors</summary>
		/// <param name="values"></param>
		public Flags32(params uint[] values)
		{
			Contract.Requires(values != null);

			Reset(values);
		}

		/// <summary>Tests if this.Value has <paramref name="flag"/></summary>
		/// <param name="flag">flag to test</param>
		/// <returns>True if <paramref name="flag"/> is set</returns>
		[Contracts.Pure]
		public bool Test(uint flag)					{ return (mValue & flag) == flag; }

		#region Add
		/// <summary>Adds <paramref name="flags"/> from this.Value</summary>
		/// <param name="flags">flags to add</param>
		public void Add(uint flags)					{ mValue |= flags; }

		/// <summary>Adds <paramref name="flags"/> when <paramref name="cond"/> is true</summary>
		/// <param name="cond">condition to use to determine when to set flags</param>
		/// <param name="flags">flags to add</param>
		/// <returns><paramref name="cond"/>'s value</returns>
		public bool Add(bool cond, uint flags)		{ if (cond) Add(flags); return cond; }
		#endregion

		#region Remove
		/// <summary>Removes <paramref name="flags"/> from this.Value</summary>
		/// <param name="flags">flags to remove</param>
		public void Remove(uint flags)				{ mValue &= ~flags; }

		/// <summary>Removes <paramref name="flags"/> when <paramref name="cond"/> is true</summary>
		/// <param name="cond">condition to use to determine when to remove flags</param>
		/// <param name="flags">flags to remove</param>
		/// <returns><paramref name="cond"/>'s value</returns>
		public bool Remove(bool cond, uint flags)	{ if (cond) Remove(flags); return cond; }
		#endregion


		#region Overrides
		/// <summary>Tests whether or not <paramref name="obj"/> is equal to this</summary>
		/// <param name="obj">The other object</param>
		/// <returns>Returns true if <paramref name="obj"/> is a <see cref="Flags32"/> object and if it's value is the same as this</returns>
		public override bool Equals(object obj)	{ return obj is Flags32 && (obj as Flags32).mValue == mValue; }
		/// <summary><see cref="uint.GetHashCode"/></summary>
		/// <returns></returns>
		public override int GetHashCode()		{ return mValue.GetHashCode(); }
		/// <summary><see cref="uint.ToString"/></summary>
		/// <returns>Returns a 8 character hexadecimal value in a string</returns>
		public override string ToString()
		{
			return mValue.ToString("X8");
		}
		#endregion
	};
}