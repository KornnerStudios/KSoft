using System;
using System.Diagnostics.CodeAnalysis;
using DebuggerStepThrough = System.Diagnostics.DebuggerStepThroughAttribute;

#nullable enable

namespace KSoft
{
	public static class Predicates
	{
		#region IsFalse/True
		private static Predicate<bool>? gIsFalse;
		public static Predicate<bool> IsFalse { get {
			return gIsFalse ??= b => !b;
		} }

		private static Predicate<bool>? gIsTrue;
		public static Predicate<bool> IsTrue { get {
			return gIsTrue ??= b => b;
		} }
		#endregion

		#region IsNotNullOrEmpty
		private static Predicate<string?>? gIsNotNullOrEmpty;
		public static Predicate<string?> IsNotNullOrEmpty { get {
			return gIsNotNullOrEmpty ??= s => !string.IsNullOrEmpty(s);
		} }
		#endregion

		#region HasItems/Bits
		private static Predicate<System.Collections.ICollection?>? gHasItems;
		public static Predicate<System.Collections.ICollection?> HasItems { get {
			return gHasItems ??= coll => coll != null && coll.Count > 0;
		} }

		private static Predicate<Collections.IReadOnlyBitSet?>? gHasBits;
		public static Predicate<Collections.IReadOnlyBitSet?> HasBits { get {
			return gHasBits ??= set => set != null && set.Cardinality > 0;
		} }
		#endregion

		//////////////////////////////////////////////////////////////////////////
		// The following are defined as functions to use type inference at the expense of implicit Predicate<> allocations

		[DebuggerStepThrough] public static bool True()				{ return true; }
		[DebuggerStepThrough] public static bool False()				{ return false; }
		//[SuppressMessage("Microsoft.Design", "CA1801:ReviewUnusedParameters")]
		[SuppressMessage("Microsoft.Design", "IDE0060:ReviewUnusedParameters")]
		[DebuggerStepThrough] public static bool True<T>(T dummy)		{ return true; }
		//[SuppressMessage("Microsoft.Design", "CA1801:ReviewUnusedParameters")]
		[SuppressMessage("Microsoft.Design", "IDE0060:ReviewUnusedParameters")]
		[DebuggerStepThrough] public static bool False<T>(T dummy)	{ return false; }

		[DebuggerStepThrough]
		public static bool IsNotNull<T>(T? theObj)
			where T : class
		{ return theObj != null; }

		[DebuggerStepThrough] public static bool IsNotEmpty(System.Guid uuid)		{ return uuid != System.Guid.Empty; }
		[DebuggerStepThrough] public static bool IsNotEmpty(Values.KGuid uuid)	{ return uuid != Values.KGuid.Empty; }

		[DebuggerStepThrough] public static bool IsZero(int x)	{ return x == 0; }
		[DebuggerStepThrough] public static bool IsZero(uint x)	{ return x == 0; }

		[DebuggerStepThrough] public static bool IsNotZero(sbyte x)	{ return x != 0; }
		[DebuggerStepThrough] public static bool IsNotZero(byte x)	{ return x != 0; }
		[DebuggerStepThrough] public static bool IsNotZero(short x)	{ return x != 0; }
		[DebuggerStepThrough] public static bool IsNotZero(ushort x)	{ return x != 0; }
		[DebuggerStepThrough] public static bool IsNotZero(int x)		{ return x != 0; }
		[DebuggerStepThrough] public static bool IsNotZero(uint x)	{ return x != 0; }
		[DebuggerStepThrough] public static bool IsNotZero(long x)	{ return x != 0; }
		[DebuggerStepThrough] public static bool IsNotZero(ulong x)	{ return x != 0; }
		[DebuggerStepThrough] public static bool IsNotZero(float x)	{ return x != 0.0f; }

		/// <returns>x != -1</returns>
		[DebuggerStepThrough] public static bool IsNotNone(sbyte x)	{ return x != TypeExtensions.kNoneInt8; }
		/// <returns>x != -1</returns>
		[DebuggerStepThrough] public static bool IsNotNone(short x)	{ return x != TypeExtensions.kNoneInt16; }
		/// <returns>x != -1</returns>
		[DebuggerStepThrough] public static bool IsNotNone(int x)		{ return x != TypeExtensions.kNoneInt32; }
		/// <returns>x != -1</returns>
		[DebuggerStepThrough] public static bool IsNotNone(long x)	{ return x != TypeExtensions.kNoneInt64; }
	};
}
