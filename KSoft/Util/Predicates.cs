using System;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft
{
	public static class Predicates
	{
		public static readonly Predicate<bool> IsFalse = b => !b;
		public static readonly Predicate<bool> IsTrue = b => b;

		public static readonly Predicate<string> IsNotNullOrEmpty = s => !string.IsNullOrEmpty(s);

		public static readonly Predicate<System.Collections.ICollection> HasItems = coll => coll.Count > 0;
		public static readonly Predicate<Collections.BitSet> HasBits = set => set.Cardinality > 0;

		//////////////////////////////////////////////////////////////////////////
		// The following are defined as functions to use type inference at the expense of implicit Predicate<> allocations

		[Contracts.Pure] public static bool True()				{ return true; }
		[Contracts.Pure] public static bool False()				{ return false; }
		[Contracts.Pure] public static bool True<T>(T dummy)	{ return true; }
		[Contracts.Pure] public static bool False<T>(T dummy)	{ return false; }

		[Contracts.Pure]
		public static bool IsNotNull<T>(T obj)
			where T : class
		{ return obj != null; }

		[Contracts.Pure] public static bool IsZero(int x)	{ return x == 0; }
		[Contracts.Pure] public static bool IsZero(uint x)	{ return x == 0; }

		[Contracts.Pure] public static bool IsNotZero(sbyte x)	{ return x != 0; }
		[Contracts.Pure] public static bool IsNotZero(byte x)	{ return x != 0; }
		[Contracts.Pure] public static bool IsNotZero(short x)	{ return x != 0; }
		[Contracts.Pure] public static bool IsNotZero(ushort x)	{ return x != 0; }
		[Contracts.Pure] public static bool IsNotZero(int x)	{ return x != 0; }
		[Contracts.Pure] public static bool IsNotZero(uint x)	{ return x != 0; }
		[Contracts.Pure] public static bool IsNotZero(long x)	{ return x != 0; }
		[Contracts.Pure] public static bool IsNotZero(ulong x)	{ return x != 0; }
		[Contracts.Pure] public static bool IsNotZero(float x)	{ return x != 0.0f; }

		/// <returns>x != -1</returns>
		[Contracts.Pure] public static bool IsNotNone(sbyte x)	{ return x != TypeExtensions.kNoneInt8; }
		/// <returns>x != -1</returns>
		[Contracts.Pure] public static bool IsNotNone(short x)	{ return x != TypeExtensions.kNoneInt16; }
		/// <returns>x != -1</returns>
		[Contracts.Pure] public static bool IsNotNone(int x)	{ return x != TypeExtensions.kNoneInt32; }
		/// <returns>x != -1</returns>
		[Contracts.Pure] public static bool IsNotNone(long x)	{ return x != TypeExtensions.kNoneInt64; }
	};
}