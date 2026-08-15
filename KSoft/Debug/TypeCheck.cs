using System;
using Contracts = System.Diagnostics.Contracts;

#nullable enable

namespace KSoft.Debug
{
	public static class TypeCheck
	{
		#region CastValue
		/// <summary>Basically a beefed up argument type checker</summary>
		/// <typeparam name="TResult">Result type</typeparam>
		/// <param name="value">Object to try and convert to <typeparamref name="TResult"/></param>
		/// <param name="result">On return; <code>obj as <typeparamref name="TResult"/></code></param>
		/// <exception cref="ArgumentNullException">value == null</exception>
		/// <exception cref="ArgumentException">
		/// When <paramref name="value"/> can't be converted to <typeparamref name="TResult"/>
		/// </exception>
		/// <remarks>Ignores user conversions</remarks>
		public static void CastValue<TResult>(object? value, out TResult result)
			where TResult : struct
		{
			ArgumentNullException.ThrowIfNull(value);

			try
			{
				result = (TResult)value;
			}
			catch (InvalidCastException ice)
			{
				throw new ArgumentException(string.Format(Util.InvariantCultureInfo,
					"obj was an unexpected type. Got '{0}' where I expected '{1}'",
					value.GetType().FullName, typeof(TResult).FullName), ice);
			}
		}
		/// <summary>Basically a beefed up argument type checker</summary>
		/// <typeparam name="TResult">Result type</typeparam>
		/// <param name="value">Object to try and convert to <typeparamref name="TResult"/></param>
		/// <returns><code>obj as <typeparamref name="TResult"/></code></returns>
		/// <exception cref="ArgumentNullException">value == null</exception>
		/// <exception cref="ArgumentException">
		/// When <paramref name="value"/> can't be converted to <typeparamref name="TResult"/>
		/// </exception>
		/// <remarks>Ignores user conversions</remarks>
		[Contracts.Pure]
		public static TResult CastValue<TResult>(object? value)
			where TResult : struct
		{
			CastValue(value, out TResult _obj);

			return _obj;
		}
		#endregion

		#region CastReference
		/// <summary>Basically a beefed up argument type checker</summary>
		/// <typeparam name="TIn">Input type</typeparam>
		/// <typeparam name="TResult">Result type</typeparam>
		/// <param name="value">Object to try and convert to <typeparamref name="TResult"/></param>
		/// <param name="result">On return; <code>obj as <typeparamref name="TResult"/></code></param>
		/// <exception cref="ArgumentNullException">value == null</exception>
		/// <exception cref="ArgumentException">
		/// When <paramref name="value"/> can't be converted to <typeparamref name="TResult"/>
		/// </exception>
		/// <remarks>Ignores user conversions</remarks>
		public static void CastReference<TIn, TResult>(TIn? value, out TResult result)
			where TIn : class
			where TResult : class
		{
			ArgumentNullException.ThrowIfNull(value);

			if (value is TResult typedValue)
			{
				result = typedValue;
				return;
			}

			throw new ArgumentException(string.Format(Util.InvariantCultureInfo,
				"obj was an unexpected type. Got '{0}' where I expected '{1}'",
				value.GetType().FullName, typeof(TResult).FullName));
		}
		/// <summary>Basically a beefed up argument type checker</summary>
		/// <typeparam name="TIn">Input type</typeparam>
		/// <typeparam name="TResult">Result type</typeparam>
		/// <param name="value">Object to try and convert to <typeparamref name="TResult"/></param>
		/// <returns><code>obj as <typeparamref name="TResult"/></code></returns>
		/// <exception cref="ArgumentNullException">value == null</exception>
		/// <exception cref="ArgumentException">
		/// When <paramref name="value"/> can't be converted to <typeparamref name="TResult"/>
		/// </exception>
		/// <remarks>Ignores user conversions</remarks>
		[Contracts.Pure]
		public static TResult CastReference<TIn, TResult>(TIn? value)
			where TIn : class
			where TResult : class
		{
			CastReference(value, out TResult _obj);

			return _obj;
		}
		/// <summary>Basically a beefed up argument type checker</summary>
		/// <typeparam name="TResult">Result type</typeparam>
		/// <param name="value">Object to try and convert to <typeparamref name="TResult"/></param>
		/// <returns><code>obj as <typeparamref name="TResult"/></code></returns>
		/// <exception cref="ArgumentNullException">value == null</exception>
		/// <exception cref="ArgumentException">When <paramref name="value"/> can't be converted to <typeparamref name="TResult"/></exception>
		/// <remarks>Ignores user conversions</remarks>
		[Contracts.Pure]
		public static TResult CastReference<TResult>(object? value)
			where TResult : class
		{
			return CastReference<object, TResult>(value);
		}
		#endregion

		/// <summary>Basically a beefed up argument type checker</summary>
		/// <typeparam name="TIn">Input type</typeparam>
		/// <typeparam name="TResult">Result type</typeparam>
		/// <param name="value">Object to try and convert to <typeparamref name="TResult"/></param>
		/// <param name="result">
		/// On return; null if <paramref name="value"/> is null or <code>obj as <typeparamref name="TResult"/></code>
		/// </param>
		/// <exception cref="ArgumentException">
		/// When <paramref name="value"/> can't be converted to <typeparamref name="TResult"/>
		/// </exception>
		/// <remarks>Ignores user conversions</remarks>
		public static void TryCastReference<TIn, TResult>(TIn? value, out TResult? result)
			where TIn : class
			where TResult : class
		{
			if (value == null)
			{
				result = null;
				return;
			}

			if (value is TResult typedValue)
			{
				result = typedValue;
				return;
			}

			throw new ArgumentException(string.Format(Util.InvariantCultureInfo,
				"obj was an unexpected type. Got '{0}' where I expected '{1}'",
				typeof(TIn).FullName, typeof(TResult).FullName));
		}
	};
}
