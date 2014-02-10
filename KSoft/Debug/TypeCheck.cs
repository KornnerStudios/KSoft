using System;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Debug
{
	public static class TypeCheck
	{
		/// <summary>Basically a beefed up argument type checker</summary>
		/// <typeparam name="TIn">Input type</typeparam>
		/// <typeparam name="TResult">Result type</typeparam>
		/// <param name="obj">Object to try and convert to <typeparamref name="TResult"/></param>
		/// <param name="result">On return; <code>obj as <typeparamref name="TResult"/></code></param>
		/// <exception cref="ArgumentNullException">obj == null</exception>
		/// <exception cref="ArgumentException">When <paramref name="obj"/> can't be converted to <typeparamref name="TResult"/></exception>
		/// <remarks>Ignores user conversions</remarks>
		[Contracts.Pure]
		public static void CastValue<TResult>(object obj, out TResult result)
			where TResult : struct
		{
			Contract.Requires<ArgumentNullException>(obj != null);

			try {
				result = (TResult)obj;
			} catch (InvalidCastException ice) {
				throw new ArgumentException(string.Format("obj was an unexpected type. Got '{0}' where I expected '{1}'", 
					obj.GetType().FullName, typeof(TResult).FullName), ice);
			}
		}

		/// <summary>Basically a beefed up argument type checker</summary>
		/// <typeparam name="TIn">Input type</typeparam>
		/// <typeparam name="TResult">Result type</typeparam>
		/// <param name="obj">Object to try and convert to <typeparamref name="TResult"/></param>
		/// <param name="result">On return; <code>obj as <typeparamref name="TResult"/></code></param>
		/// <exception cref="ArgumentNullException">obj == null</exception>
		/// <exception cref="ArgumentException">When <paramref name="obj"/> can't be converted to <typeparamref name="TResult"/></exception>
		/// <remarks>Ignores user conversions</remarks>
		[Contracts.Pure]
		public static void CastReference<TIn, TResult>(TIn obj, out TResult result)
			where TIn : class 
			where TResult : class
		{
			Contract.Requires<ArgumentNullException>(obj != null);

			result = obj as TResult;

			if (result == null)
				throw new ArgumentException(string.Format("obj was an unexpected type. Got '{0}' where I expected '{1}'", 
					typeof(TIn).FullName, typeof(TResult).FullName));
		}

		/// <summary>Basically a beefed up argument type checker</summary>
		/// <typeparam name="TIn">Input type</typeparam>
		/// <typeparam name="TResult">Result type</typeparam>
		/// <param name="obj">Object to try and convert to <typeparamref name="TResult"/></param>
		/// <param name="result">On return; null if <paramref name="obj"/> is null or <code>obj as <typeparamref name="TResult"/></code></param>
		/// <exception cref="ArgumentException">When <paramref name="obj"/> can't be converted to <typeparamref name="TResult"/></exception>
		/// <remarks>Ignores user conversions</remarks>
		[Contracts.Pure]
		public static void TryCastReference<TIn, TResult>(TIn obj, out TResult result)
			where TIn : class
			where TResult : class
		{
			if (obj != null)
			{
				result = obj as TResult;

				if (result == null)
					throw new ArgumentException(string.Format("obj was an unexpected type. Got '{0}' where I expected '{1}'", 
						typeof(TIn).FullName, typeof(TResult).FullName));
			}
			else
				result = null;
		}
	};
}