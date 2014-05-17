using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft
{
	// Based on http://www.codeproject.com/KB/cs/EnumComparer.aspx

	public static class EnumComparer
	{
		public static EnumComparer<TEnum> For<TEnum>()
			where TEnum : struct, IComparable, IConvertible, IFormattable
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
	/// <remarks>ATTN: This code is based on the following article: http://www.codeproject.com/KB/cs/EnumComparer.aspx</remarks>
	public sealed class EnumComparer<TEnum> : Reflection.EnumUtilBase<TEnum>, IComparer<TEnum>, IEqualityComparer<TEnum>
		where TEnum : struct, IComparable, IConvertible, IFormattable
	{
		const string kCompareMethodName = "CompareTo";

		static readonly Func<TEnum, TEnum, bool> kEqualsMethod;
		static readonly Func<TEnum, int> kGetHashCodeMethod;
		static readonly Func<TEnum, TEnum, int> kCompareMethod;

		/// <summary>The singleton accessor.</summary>
		public static readonly EnumComparer<TEnum> Instance;

		/// <summary>Initializes the <see cref="EnumComparer{TEnum}"/> class by generating the GetHashCode and Equals methods.</summary>
		static EnumComparer()
		{
			Reflection.EnumUtils.AssertTypeIsEnum(kEnumType);
			Reflection.EnumUtils.AssertUnderlyingTypeIsSupported(kEnumType, kUnderlyingType);

			kGetHashCodeMethod = GenerateGetHashCodeMethod();
			kEqualsMethod = GenerateEqualsMethod();
			kCompareMethod = GenerateCompareMethod();
			Instance = new EnumComparer<TEnum>();
		}

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
		public bool Equals(TEnum x, TEnum y)	{ return kEqualsMethod(x, y); }

		/// <summary>Returns a hash code for the specified object.</summary>
		/// <param name="obj">The <see cref="T:System.Object"/> for which a hash code is to be returned.</param>
		/// <returns>A hash code for the specified object.</returns>
		/// <exception cref="T:System.ArgumentNullException">
		/// The type of <paramref name="obj"/> is a reference type and <paramref name="obj"/> is null.
		/// </exception>
		public int GetHashCode(TEnum obj)		{ return kGetHashCodeMethod(obj); }

		/// <summary>Generates a comparison method similar to this:
		/// <code>
		/// bool Equals(TEnum x, TEnum y)
		/// {
		///     return x == y;
		/// }
		/// </code>
		/// </summary>
		/// <returns>The generated method.</returns>
		static Func<TEnum, TEnum, bool> GenerateEqualsMethod()
		{
			var xParam =			Expression.Parameter(kEnumType, "x");
			var yParam =			Expression.Parameter(kEnumType, "y");
			var equalExpression =	Expression.Equal(xParam, yParam);

			var lambda = Expression.Lambda<Func<TEnum, TEnum, bool>>(equalExpression, xParam, yParam);
			return lambda.Compile();
		}

		/// <summary>Generates a GetHashCode method similar to this:
		/// <code>
		/// int GetHashCode(TEnum obj)
		/// {
		///     return ((int)obj).GetHashCode();
		/// }
		/// </code>
		/// </summary>
		/// <returns>The generated method.</returns>
		static Func<TEnum, int> GenerateGetHashCodeMethod()
		{
			var objParam =				Expression.Parameter(kEnumType, "obj");
			var convertExpression =		Expression.Convert(objParam, kUnderlyingType);
			var getHashCodeMethod =		kUnderlyingType.GetMethod("GetHashCode");
			var getHashCodeExpression = Expression.Call(convertExpression, getHashCodeMethod);

			var lambda = Expression.Lambda<Func<TEnum, int>>(getHashCodeExpression, objParam);
			return lambda.Compile();
		}
		#endregion

		#region IComparer<TEnum> Members
		public int Compare(TEnum x, TEnum y)
		{
			return kCompareMethod(x, y);
		}

		/// <summary>Generates a comparison method similar to this:
		/// <code>
		/// int Compare(TEnum x, TEnum y)
		/// {
		///     return ( (int)x ).CompareTo( (int)y );
		/// }
		/// </code>
		/// Where 'int' is the underlying integer type.
		/// </summary>
		/// <returns>The generated method.</returns>
		static Func<TEnum, TEnum, int> GenerateCompareMethod()
		{
			var xParam =			Expression.Parameter(Reflection.EnumUtil<TEnum>.EnumType, "x");
			var yParam =			Expression.Parameter(Reflection.EnumUtil<TEnum>.EnumType, "y");
			var xAsInt =			Expression.Convert(xParam, kUnderlyingType);
			var yAsInt =			Expression.Convert(yParam, kUnderlyingType);
			var compareExpression = Expression.Call(xAsInt, kCompareMethodName, null, yAsInt);

			var lambda = Expression.Lambda<Func<TEnum, TEnum, int>>(compareExpression, xParam, yParam);
			return lambda.Compile();
		}
		#endregion
	};
}