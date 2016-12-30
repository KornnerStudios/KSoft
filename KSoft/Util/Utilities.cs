using System;
using System.Collections.Generic;
using System.Linq;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;
using Expr = System.Linq.Expressions.Expression;
using Reflect = System.Reflection;

namespace KSoft
{
	public static class Kontracts
	{
		public const string kCategory = "Microsoft.Contracts";

		// Based on ContractsManual.pdf: 5.2.3, Delegating Checks to Other Methods
		//Usage: [System.Diagnostics.CodeAnalysis.SuppressMessage(Kontracts.kCategory, Kontracts.kIgnoreOverrideId, Justification=Kontracts.kIgnoreOverrideJust)]
		/// <summary>SuppressMessage warning id</summary>
		public const string kIgnoreOverrideId = "CC1055";
		/// <summary>SuppressMessage justification</summary>
		public const string kIgnoreOverrideJust = "Validation performed in base method";
	};

	public static partial class Util
	{
		// Based on http://blogs.msdn.com/b/jaredpar/archive/2011/03/18/debuggerdisplay-attribute-best-practices.aspx
		// MSDN example uses "nq" suffix, yet doesn't explain it. I knew SOMEONE had to have commented on this. If you check
		// the VS2010-era DebuggerDisplay docs you'll find a Community Addition reply with WTF "nq" is for (and the above link)
		/// <summary>DebuggerDisplay value to use to display the object's {DebuggerDisplay} value without any quotes</summary>
		public const string DebuggerDisplayPropNameSansQuotes = "{DebuggerDisplay,nq}";

		#region static EmptyArray
		private static object[] gEmptyArray;
		/// <summary>A global zero-length array of objects. Should only be used as input for functions that don't use 'params'</summary>
		public static object[] EmptyArray { get {
			if (gEmptyArray == null)
				gEmptyArray = new object[] { };

			return gEmptyArray;
		} }
		#endregion

		#region static GetNullException function ptr
		private static Func<Exception> gGetNullException;
		internal static Func<Exception> GetNullException { get {
			if (gGetNullException == null)
				gGetNullException = () => null;

			return gGetNullException;
		} }
		#endregion

		#region static pre-boxed boolean values
		private static object gFalseObject;
		/// <summary>false boolean pre-boxed to an object</summary>
		public static object FalseObject { get {
			if (gFalseObject == null)
				gFalseObject = (object)false;

			return gFalseObject;
		} }

		public static object gTrueObject;
		/// <summary>true boolean pre-boxed to an object</summary>
		public static object TrueObject { get {
			if (gTrueObject == null)
				gTrueObject = (object)true;

			return gTrueObject;
		} }
		#endregion

		#region IDisposable
		sealed class NullDisposableImpl
			: IDisposable
		{
			public void Dispose() { }
		};
		/// <summary>Object which can be disposed of without limit and is thread safe</summary>
		public static readonly IDisposable NullDisposable = new NullDisposableImpl();

		/// <summary>If <paramref name="obj"/> isn't already null, calls its Dispose and nulls the reference</summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="obj"></param>
		public static void DisposeAndNull<T>(ref T obj)
			where T : class, IDisposable
		{
			if (obj != null)
			{
				obj.Dispose();
				obj = null;
			}
		}
		#endregion

		public static void ClearAndNull<T>(ref T[] array)
		{
			array = null;
		}
		public static void ClearAndNull<T>(ref ICollection<T> coll)
		{
			if (coll != null)
			{
				coll.Clear();
				coll = null;
			}
		}

		#region Unix Time
		private static long kUnixTimeEpochInTicks;
		private static DateTime kUnixTimeEpoch;
		/// <summary>The UTC of the <b>time_t</b> C++ construct</summary>
		public static DateTime UnixTimeEpoch { get {
			if (kUnixTimeEpochInTicks == 0)
			{
				kUnixTimeEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
				kUnixTimeEpochInTicks = kUnixTimeEpoch.Ticks;
			}

			return kUnixTimeEpoch;
		} }

		/// <summary>Convert a <b>time_t</b> or <b>time64_t</b> value to a <see cref="System.DateTime"/></summary>
		/// <param name="time_t">The <b>time_t</b> numerical value</param>
		/// <returns></returns>
		[Contracts.Pure]
		public static DateTime ConvertDateTimeFromUnixTime(long time_t)
		{
			Contract.Requires<ArgumentOutOfRangeException>(time_t >= 0);

			return UnixTimeEpoch.AddSeconds(time_t);
		}

		/// <summary>Convert a <see cref="System.DateTime"/> to a <b>time64_t</b> value</summary>
		/// <param name="value"></param>
		/// <returns></returns>
		[Contracts.Pure]
		public static long ConvertDateTimeToUnixTime(DateTime value)
		{
			Contract.Requires<ArgumentOutOfRangeException>(value >= UnixTimeEpoch);

			long time_t = 0;

			// Subtract unix's epoch then rebase back into seconds
			time_t = (value.ToFileTimeUtc() - UnixTimeEpoch.ToFileTimeUtc()) / 10000000;

			return time_t;
		}
		#endregion

		#region Comparer Factory
		sealed class ComparerFactory<T>
			: IComparer<T>
		{
			Func<T, T, int> mComparer;

			ComparerFactory(Func<T, T, int> comparer)
			{
				mComparer = comparer;
			}

			#region IComparer<T> Members
			public int Compare(T x, T y)
			{
				return mComparer(x, y);
			}
			#endregion

			[Contracts.Pure]
			public static IComparer<T> Create(Func<T, T, int> comparer)
			{
				return new ComparerFactory<T>(comparer);
			}
		};
		[Contracts.Pure]
		public static IComparer<T> CreateComparer<T>(Func<T, T, int> comparer)
		{
			Contract.Requires<ArgumentNullException>(comparer != null);
			Contract.Ensures(Contract.Result<IComparer<T>>() != null);

			return ComparerFactory<T>.Create(comparer);
		}
		#endregion

		/// <summary>Comapres two equatable reference objects, taking same-instance and null cases into account</summary>
		/// <typeparam name="T">Reference object which is equatable</typeparam>
		/// <param name="x">First object to compare</param>
		/// <param name="y">Second object to compare</param>
		/// <returns></returns>
		[Contracts.Pure]
		public static bool GenericReferenceEquals<T>(T x, T y)
			where T : class, IEquatable<T>
		{
			// Handles 'x is same instance as y' and 'null == null'
			bool result = object.ReferenceEquals(x, y);

			// If they're not the same instance, or both null, either one of them is null or neither is.
			// If x isn't null, then either y is null or they're two objects in which case they can be equated
			if (!result && (object)x != null)
				return x.Equals(y);

			return result;
		}

		#region Min/Max Choice
		/// <summary>Returns the object with the smaller of two properties</summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="lhs"></param>
		/// <param name="rhs"></param>
		/// <param name="choiceProperty"></param>
		/// <returns></returns>
		[Contracts.Pure]
		public static T MinChoice<T>(T lhs, T rhs, Func<T, int> choiceProperty)
			where T : class
		{
			Contract.Requires<ArgumentNullException>(lhs != null);
			Contract.Requires<ArgumentNullException>(rhs != null);
			Contract.Requires(choiceProperty != null);

			return choiceProperty(lhs) < choiceProperty(rhs)
				? lhs
				: rhs;
		}
		/// <summary>Returns the value with the smaller of two properties</summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="lhs"></param>
		/// <param name="rhs"></param>
		/// <param name="choiceProperty"></param>
		/// <returns></returns>
		[Contracts.Pure]
		public static T MinChoiceValue<T>(T lhs, T rhs, Func<T, int> choiceProperty)
			where T : struct
		{
			Contract.Requires(choiceProperty != null);

			return choiceProperty(lhs) < choiceProperty(rhs)
				? lhs
				: rhs;
		}

		/// <summary>Returns the object with the larger of two properties</summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="lhs"></param>
		/// <param name="rhs"></param>
		/// <param name="choiceProperty"></param>
		/// <returns></returns>
		[Contracts.Pure]
		public static T MaxChoice<T>(T lhs, T rhs, Func<T, int> choiceProperty)
			where T : class
		{
			Contract.Requires<ArgumentNullException>(lhs != null);
			Contract.Requires<ArgumentNullException>(rhs != null);
			Contract.Requires(choiceProperty != null);

			return choiceProperty(lhs) > choiceProperty(rhs)
				? lhs
				: rhs;
		}
		/// <summary>Returns the value with the larger of two properties</summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="lhs"></param>
		/// <param name="rhs"></param>
		/// <param name="choiceProperty"></param>
		/// <returns></returns>
		[Contracts.Pure]
		public static T MaxChoiceValue<T>(T lhs, T rhs, Func<T, int> choiceProperty)
			where T : struct
		{
			Contract.Requires(choiceProperty != null);

			return choiceProperty(lhs) > choiceProperty(rhs)
				? lhs
				: rhs;
		}

		#endregion

		public static void ValueTypeInitializeComparer<T>()
			where T : struct, System.Collections.IComparer, IComparer<T>
		{
			// ReSharper disable once NotAccessedVariable
			var comparer = Collections.ValueTypeComparer<T>.Default;
			comparer = null;
		}
		public static void ValueTypeInitializeEqualityComparer<T>()
			where T : struct, IEqualityComparer<T>
		{
			// ReSharper disable once NotAccessedVariable
			var comparer = Collections.ValueTypeEqualityComparer<T>.Default;
			comparer = null;
		}
		public static void ValueTypeInitializeEquatableComparer<T>()
			where T : struct, IEquatable<T>
		{
			// ReSharper disable once NotAccessedVariable
			var comparer = Collections.ValueTypeEquatableComparer<T>.Default;
			comparer = null;
		}
	};
}