using System;
using System.Collections.Generic;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft
{
	// Based on http://www.codeproject.com/KB/cs/EnumComparer.aspx

	public static class EnumComparer
	{
		public static EnumComparer<TEnum> For<TEnum>()
			where TEnum : struct, Enum
		{
			Contract.Ensures(Contract.Result<EnumComparer<TEnum>>() != null);

			return EnumComparer<TEnum>.Instance;
		}
	};

	/// <summary>
	/// A fast and efficient implementation of <see cref="IEqualityComparer{T}"/> for Enum types.
	/// Useful for dictionaries that use Enums as their keys.
	///
	/// Also implements <see cref="IComparer{T}"/>
	/// </summary>
	/// <example>
	/// <code>
	/// var dict = new Dictionary&lt;DayOfWeek, string&gt;(EnumComparer&lt;DayOfWeek&gt;.Instance);
	/// </code>
	/// </example>
	/// <typeparam name="TEnum">The type of the Enum.</typeparam>
	/// <remarks>
	/// BenchmarkDotNet evidence on .NET 9 showed the default BCL enum comparers are allocation-free for equality,
	/// hash-code, and ordering paths, and avoid the old expression-compilation first-touch cost. This type now exists as
	/// a KSoft compatibility shim; future cleanup should replace call sites with the BCL defaults when the combined
	/// comparer convenience shape is no longer needed.
	/// </remarks>
	public sealed class EnumComparer<TEnum> : IComparer<TEnum>, IEqualityComparer<TEnum>
		where TEnum : struct, Enum
	{
		// #VITA_SHIM: Default enum comparers avoid per-type expression compilation while preserving this KSoft API.
		static class EqualityComparerHolder
		{
			public static readonly EqualityComparer<TEnum> Instance = EqualityComparer<TEnum>.Default;
		}

		static class ComparerHolder
		{
			public static readonly Comparer<TEnum> Instance = Comparer<TEnum>.Default;
		}

		/// <summary>The singleton accessor.</summary>
		public static readonly EnumComparer<TEnum> Instance = new();

		/// <summary>Private constructor to prevent user instantiation.</summary>
		EnumComparer()
		{
		}

		#region IEqualityComparer<TEnum> Members
		/// <summary>Determines whether the specified objects are equal.</summary>
		/// <param name="x">The first object of type <typeparamref name="TEnum"/> to compare.</param>
		/// <param name="y">The second object of type <typeparamref name="TEnum"/> to compare.</param>
		/// <returns>
		/// true if the specified objects are equal; otherwise, false.
		/// </returns>
		public bool Equals(TEnum x, TEnum y)	{ return EqualityComparerHolder.Instance.Equals(x, y); }

		/// <summary>Returns a hash code for the specified object.</summary>
		/// <param name="obj">The <see cref="System.Object"/> for which a hash code is to be returned.</param>
		/// <returns>A hash code for the specified object.</returns>
		/// <exception cref="System.ArgumentNullException">
		/// The type of <paramref name="obj"/> is a reference type and <paramref name="obj"/> is null.
		/// </exception>
		public int GetHashCode(TEnum obj)		{ return EqualityComparerHolder.Instance.GetHashCode(obj); }
		#endregion

		#region IComparer<TEnum> Members
		public int Compare(TEnum x, TEnum y)
		{
			return ComparerHolder.Instance.Compare(x, y);
		}
		#endregion
	};
}
