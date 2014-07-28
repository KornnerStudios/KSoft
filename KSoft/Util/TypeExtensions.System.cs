using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft
{
	partial class TypeExtensions
	{
		[Contracts.Pure]
		public static bool IsSigned(this TypeCode c)
		{
			switch (c)
			{
				case TypeCode.SByte:
				case TypeCode.Int16:
				case TypeCode.Int32:
				case TypeCode.Int64:
					return true;

				default: return false;
			}
		}

		public static string Format(this string format, params object[] args)
		{
			return string.Format(format, args);
		}
		public static string FormatWith(this string format, IFormatProvider provider, params object[] args)
		{
			Contract.Requires<ArgumentNullException>(provider != null);

			return string.Format(provider, format, args);
		}

		#region Array
		[Contracts.Pure]
		public static bool EqualsZero<T>(this T[] array)
			where T : struct, IEquatable<T>
		{
			Contract.Requires(array != null);

			var zero = new T();

			for (int x = 0; x < array.Length; x++)
				if (!array[x].Equals(zero))
					return false;

			return true;
		}
		[Contracts.Pure]
		public static bool EqualsDefault<T>(this T[] array)
			where T : class, IEquatable<T>, new()
		{
			Contract.Requires(array != null);

			var zero = new T();

			for (int x = 0; x < array.Length; x++)
				if (!array[x].Equals(zero))
					return false;

			return true;
		}
		[Contracts.Pure]
		public static bool EqualsArray<T>(this T[] lhs, T[] rhs, int lhsOffset = 0)
			where T : IEquatable<T>
		{
			Contract.Requires(lhs != null && rhs != null);

			if (lhs == rhs)
				return true;
			else if (rhs.Length < (lhs.Length-lhsOffset))
				return false;

			for (int x = lhsOffset; x < (lhs.Length-lhsOffset); x++)
				if (!rhs[x].Equals(lhs[x]))
					return false;

			return true;
		}

		public static bool TrueForAny<T>(this T[] array, Predicate<T> match)
		{
			Contract.Requires<ArgumentNullException>(array != null);
			Contract.Requires<ArgumentNullException>(match != null);

			for (int x = 0; x < array.Length; x++)
				if (match(array[x]))
					return true;

			return false;
		}
		#endregion

		#region Type
		/// <summary>Test whether the subject type implements the specified interface type</summary>
		/// <param name="subject">The type in question</param>
		/// <param name="genericType">The interface type which the subject may or may not implement</param>
		/// <returns></returns>
		[Contracts.Pure]
		public static bool ImplementsInterface(this Type subject, Type interfaceType)
		{
			Contract.Requires(subject != null);
			Contract.Requires(interfaceType != null);
			Contract.Requires(interfaceType.IsInterface);

			return interfaceType.IsAssignableFrom(subject);
		}

		/// <summary>Test whether the subject type derives from a generic type using CRTP</summary>
		/// <param name="subject">The type in question</param>
		/// <param name="genericType">The generic type which has a single type parameter, which will be populated with subject</param>
		/// <returns></returns>
		/// <remarks>See: http://en.wikipedia.org/wiki/Curiously_recurring_template_pattern </remarks>
		[Contracts.Pure]
		public static bool IsCuriouslyRecurringTemplatePattern(this Type subject, Type genericType)
		{
			Contract.Requires(subject != null);
			Contract.Requires(genericType != null);
			Contract.Requires(genericType.IsGenericType && genericType.IsGenericTypeDefinition);
			Contract.Requires(genericType.GetGenericArguments().Length == 1);

			var concrete_type = genericType.MakeGenericType(subject);

			return concrete_type.IsAssignableFrom(subject);
		}

		/// <summary>Force the .cctor of a type to run by invoking the getter of a static property</summary>
		/// <param name="subject">The type in question</param>
		/// <param name="staticPropertyName">Name of the static property to get</param>
		public static void ForceStaticCtorToRunViaProperty(this Type subject, string staticPropertyName)
		{
			Contract.Requires(subject != null);
			Contract.Requires(!string.IsNullOrEmpty(staticPropertyName));

			const BindingFlags k_static_property_binding_flags
				= BindingFlags.Public | BindingFlags.Static | BindingFlags.GetProperty | BindingFlags.DeclaredOnly;

			var static_props = subject.GetProperties(k_static_property_binding_flags);
			var static_prop = (
					from prop in static_props
					where prop.Name == staticPropertyName
					select prop
				).First();

			// This doesn't cause the static ctor to run it would seem.
			//Activator.CreateInstance(concrete_type);

			// However, effectively invoking ValueTypeComparer<>.Default does
			static_prop.GetValue(null);
		}
		#endregion

		#region Collections
		[Contracts.Pure]
		[System.Diagnostics.DebuggerStepThrough]
		public static bool IsNullOrEmpty<T>(this ICollection<T> coll)
		{
			return coll == null || coll.Count == 0;
		}

		[Contracts.Pure]
		[System.Diagnostics.DebuggerStepThrough]
		public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T> seq)
		{
			return seq ?? Enumerable.Empty<T>();
		}

		/// <summary>Query if 'seq' contains duplicate values</summary>
		/// <param name="seq">The sequence to act on</param>
		/// <returns>True if there are duplicates, false if all values are distinct</returns>
		/// <remarks>Based on this answer http://stackoverflow.com/a/4712539/444977 </remarks>
		[Contracts.Pure]
		[System.Diagnostics.DebuggerStepThrough]
		public static bool ContainsDuplicates<T>(this IEnumerable<T> seq)
		{
			// A list has *no* duplicates when All items can be Add-ed to a set.
			return !seq.All(new HashSet<T>().Add);
		}

		[Contracts.Pure]
		public static int FindIndex<T>(this IEnumerable<T> seq, Predicate<T> match)
		{
			Contract.Requires<ArgumentNullException>(seq != null);
			Contract.Requires<ArgumentNullException>(match != null);

			var found = seq
				.Select((v, i) => new KeyValuePair<T, int?>(v, i))
				.FirstOrDefault(kvp => match(kvp.Key));

			return found.Value ?? TypeExtensions.kNone;
		}

		[Contracts.Pure]
		public static int FindIndex<T>(this IReadOnlyList<T> list,
			int startIndex, int count, Predicate<T> match)
		{
			Contract.Requires<ArgumentNullException>(list != null);
			Contract.Requires<ArgumentOutOfRangeException>(startIndex < list.Count);
			Contract.Requires<ArgumentOutOfRangeException>(count >= 0 && startIndex <= list.Count-count);
			Contract.Requires<ArgumentNullException>(match != null);
			Contract.Ensures(Contract.Result<int>().IsNoneOrPositive());
			Contract.Ensures(Contract.Result<int>() < startIndex+count);

			int end_index = startIndex + count;
			for (int x = startIndex; x < end_index; x++)
			{
				if (match(list[x]))
					return x;
			}

			return TypeExtensions.kNone;
		}
		[Contracts.Pure]
		public static int FindIndex<T>(this IReadOnlyList<T> list,
			int startIndex, Predicate<T> match)
		{
			Contract.Requires<ArgumentNullException>(list != null);
			Contract.Ensures(Contract.Result<int>().IsNoneOrPositive());
			Contract.Ensures(Contract.Result<int>() < startIndex+list.Count);

			return FindIndex(list, startIndex, list.Count-startIndex, match);
		}
		[Contracts.Pure]
		public static int FindIndex<T>(this IReadOnlyList<T> list,
			Predicate<T> match)
		{
			Contract.Requires<ArgumentNullException>(list != null);
			Contract.Ensures(Contract.Result<int>().IsNoneOrPositive());
			Contract.Ensures(Contract.Result<int>() < list.Count);

			return FindIndex(list, 0, list.Count, match);
		}
		#endregion

		#region IO
		[Contracts.Pure]
		public static bool CanRead(this System.IO.FileAccess value)
		{
			return (value & System.IO.FileAccess.Read) == System.IO.FileAccess.Read;
		}
		[Contracts.Pure]
		public static bool CanWrite(this System.IO.FileAccess value)
		{
			return (value & System.IO.FileAccess.Write) == System.IO.FileAccess.Write;
		}

		[Contracts.Pure]
		public static long BytesRemaining(this System.IO.Stream s)
		{
			Contract.Requires(s != null);
			Contract.Requires<InvalidOperationException>(s.CanSeek);

			return s.Length - s.Position;
		}
		[Contracts.Pure]
		public static long BytesRemaining(this System.IO.Stream s, long endPosition)
		{
			Contract.Requires(s != null);
			Contract.Requires<InvalidOperationException>(s.CanSeek);
			Contract.Requires<ArgumentOutOfRangeException>(endPosition <= s.Length);

			return endPosition - s.Position;
		}
		[Contracts.Pure]
		public static bool HasPermissions(this System.IO.Stream s, System.IO.FileAccess permissions)
		{
			Contract.Requires(s != null);
			Contract.Requires<InvalidOperationException>(s.CanSeek);
			bool result = true;

			if (permissions.CanRead())
				result &= s.CanRead;
			if (permissions.CanWrite())
				result &= s.CanWrite;

			return result;
		}
		#endregion

		#region Event handlers
		public static void SafeNotify(this System.ComponentModel.PropertyChangedEventHandler handler,
			object sender, System.ComponentModel.PropertyChangedEventArgs args)
		{
			if (handler != null)
				handler(sender, args);
		}
		public static void SafeNotify(this System.ComponentModel.PropertyChangedEventHandler handler,
			object sender, System.ComponentModel.PropertyChangedEventArgs[] argsList, int startIndex = 0)
		{
			Contract.Requires(argsList != null);
			Contract.Requires(startIndex >= 0 && startIndex < argsList.Length);

			if (handler != null)
				foreach (var args in argsList)
					handler(sender, args);
		}
		public static void SafeNotify(this System.Collections.Specialized.NotifyCollectionChangedEventHandler handler,
			object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs args)
		{
			if (handler != null)
				handler(sender, args);
		}

		// Based on http://www.codeproject.com/KB/cs/EventSafeTrigger.aspx
		public static void SafeTrigger<TEventArgs>(this EventHandler<TEventArgs> eventToTrigger,
			object sender, TEventArgs eventArgs) where TEventArgs : EventArgs
		{
			if (eventToTrigger != null)
				eventToTrigger(sender, eventArgs);
		}

		public static TReturnType SafeTrigger<TEventArgs, TReturnType>
			(this EventHandler<TEventArgs> eventToTrigger, object sender,
			TEventArgs eventArgs, Func<TEventArgs, TReturnType> retrieveDataFunction)
			where TEventArgs : EventArgs
		{
			Contract.Requires(retrieveDataFunction != null);

			if (eventToTrigger != null)
			{
				eventToTrigger(sender, eventArgs);
				return retrieveDataFunction(eventArgs);
			}
			else
				return default(TReturnType);
		}
		#endregion

		#region GetCustomAttribute
		// Based on http://www.codeproject.com/Tips/72637/Get-CustomAttributes-the-easy-way.aspx

		/// <summary>Returns first custom attribute of type T in the inheritance chain</summary>
		[Contracts.Pure]
		public static T GetCustomAttribute<T>(this ICustomAttributeProvider provider, bool inherited = false)
			where T : Attribute
		{
			Contract.Requires(provider != null);

			return provider.GetCustomAttributes<T>(inherited).FirstOrDefault();
		}

		/// <summary>Returns all custom attributes of type T in the inheritance chain</summary>
		[Contracts.Pure]
		public static List<T> GetCustomAttributes<T>(this ICustomAttributeProvider provider, bool inherited = false)
			where T : Attribute
		{
			Contract.Requires(provider != null);

			return provider.GetCustomAttributes(typeof(T), inherited).Cast<T>().ToList();
		}
		#endregion

		#region OrderBy LINQ
		// Based on http://stackoverflow.com/questions/271398/what-are-your-favorite-extension-methods-for-c-codeplex-com-extensionoverflow/858681#858681

		/// <summary>Sorts elements in a sequence in ascending order</summary>
		/// <typeparam name="TSrc"></typeparam>
		/// <typeparam name="TKey"></typeparam>
		/// <param name="src">Sequence to sort</param>
		/// <param name="keySelector"></param>
		/// <param name="comparerFunc">Comparison delegate</param>
		/// <returns></returns>
		[Contracts.Pure]
		public static IOrderedEnumerable<TSrc> OrderBy<TSrc, TKey>(this IEnumerable<TSrc> src,
			Func<TSrc, TKey> keySelector, Func<TKey, TKey, int> comparerFunc)
		{
			Contract.Requires<ArgumentNullException>(src != null);
			Contract.Requires/*<ArgumentNullException>*/(keySelector != null);
			Contract.Requires/*<ArgumentNullException>*/(comparerFunc != null);
			Contract.Ensures(Contract.Result<IOrderedEnumerable<TSrc>>() != null);

			var comparer = Util.CreateComparer(comparerFunc);
			return src.OrderBy(keySelector, comparer);
		}
		/// <summary>Sorts elements in a sequence in ascending order</summary>
		/// <typeparam name="TSrc"></typeparam>
		/// <typeparam name="TKey"></typeparam>
		/// <param name="src">Sequence to sort</param>
		/// <param name="keySelector"></param>
		/// <param name="comparerFunc">Comparison delegate</param>
		/// <returns></returns>
		[Contracts.Pure]
		public static IOrderedEnumerable<TSrc> OrderByDescending<TSrc, TKey>(this IEnumerable<TSrc> src,
			Func<TSrc, TKey> keySelector, Func<TKey, TKey, int> comparerFunc)
		{
			Contract.Requires<ArgumentNullException>(src != null);
			Contract.Requires/*<ArgumentNullException>*/(keySelector != null);
			Contract.Requires/*<ArgumentNullException>*/(comparerFunc != null);
			Contract.Ensures(Contract.Result<IOrderedEnumerable<TSrc>>() != null);

			var comparer = Util.CreateComparer(comparerFunc);
			return src.OrderByDescending(keySelector, comparer);
		}
		#endregion
	};
}