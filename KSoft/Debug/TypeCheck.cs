using System;
using Contracts = System.Diagnostics.Contracts;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

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
		/// <exception cref="ArgumentException">When <paramref name="value"/> can't be converted to <typeparamref name="TResult"/></exception>
		/// <remarks>Ignores user conversions</remarks>
		public static void CastValue<TResult>(object value, out TResult result)
			where TResult : struct
		{
			Contract.Requires<ArgumentNullException>(value != null);

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
		/// <exception cref="ArgumentException">When <paramref name="value"/> can't be converted to <typeparamref name="TResult"/></exception>
		/// <remarks>Ignores user conversions</remarks>
		[Contracts.Pure]
		public static TResult CastValue<TResult>(object value)
			where TResult : struct
		{
			Contract.Requires<ArgumentNullException>(value != null);

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
		/// <exception cref="ArgumentException">When <paramref name="value"/> can't be converted to <typeparamref name="TResult"/></exception>
		/// <remarks>Ignores user conversions</remarks>
		public static void CastReference<TIn, TResult>(TIn value, out TResult result)
			where TIn : class
			where TResult : class
		{
			Contract.Requires<ArgumentNullException>(value != null);

			result = value as TResult;

			if (result == null)
			{
				throw new ArgumentException(string.Format(Util.InvariantCultureInfo,
					"obj was an unexpected type. Got '{0}' where I expected '{1}'",
					value.GetType().FullName, typeof(TResult).FullName));
			}
		}
		/// <summary>Basically a beefed up argument type checker</summary>
		/// <typeparam name="TIn">Input type</typeparam>
		/// <typeparam name="TResult">Result type</typeparam>
		/// <param name="value">Object to try and convert to <typeparamref name="TResult"/></param>
		/// <returns><code>obj as <typeparamref name="TResult"/></code></returns>
		/// <exception cref="ArgumentNullException">value == null</exception>
		/// <exception cref="ArgumentException">When <paramref name="value"/> can't be converted to <typeparamref name="TResult"/></exception>
		/// <remarks>Ignores user conversions</remarks>
		[Contracts.Pure]
		public static TResult CastReference<TIn, TResult>(TIn value)
			where TIn : class
			where TResult : class
		{
			Contract.Requires<ArgumentNullException>(value != null);

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
		public static TResult CastReference<TResult>(object value)
			where TResult : class
		{
			return CastReference<object, TResult>(value);
		}
		#endregion

		/// <summary>Basically a beefed up argument type checker</summary>
		/// <typeparam name="TIn">Input type</typeparam>
		/// <typeparam name="TResult">Result type</typeparam>
		/// <param name="value">Object to try and convert to <typeparamref name="TResult"/></param>
		/// <param name="result">On return; null if <paramref name="value"/> is null or <code>obj as <typeparamref name="TResult"/></code></param>
		/// <exception cref="ArgumentException">When <paramref name="value"/> can't be converted to <typeparamref name="TResult"/></exception>
		/// <remarks>Ignores user conversions</remarks>
		public static void TryCastReference<TIn, TResult>(TIn value, out TResult result)
			where TIn : class
			where TResult : class
		{
			if (value != null)
			{
				result = value as TResult;

				if (result == null)
				{
					throw new ArgumentException(string.Format(Util.InvariantCultureInfo,
						"obj was an unexpected type. Got '{0}' where I expected '{1}'",
						typeof(TIn).FullName, typeof(TResult).FullName));
				}
			}
			else
			{
				result = null;
			}
		}
	};
}
