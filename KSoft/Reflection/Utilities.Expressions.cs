using System;
using System.Collections.Generic;
using System.Linq;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;
using Exprs = System.Linq.Expressions;
using Expr = System.Linq.Expressions.Expression;
using Reflect = System.Reflection;

namespace KSoft.Reflection
{
	partial class Util
	{
		public const int kGenerateDynamicDelegateMaximumParameters = 16;

		// Reference:
		// http://www.codeproject.com/KB/cs/FastMethodInvoker.aspx
		// http://www.codeproject.com/KB/cs/AsyncMethodInvocation.aspx

		static Type GetDynamicDelegateActionType(int paramCount)
		{
			switch (paramCount)
			{
				case 0:  return typeof(Action);
				case 1:  return typeof(Action<>);
				case 2:  return typeof(Action<,>);
				case 3:  return typeof(Action<,,>);
				case 4:  return typeof(Action<,,,>);
				case 5:  return typeof(Action<,,,,>);
				case 6:  return typeof(Action<,,,,,>);
				case 7:  return typeof(Action<,,,,,,>);
				case 8:  return typeof(Action<,,,,,,,>);
				case 9:  return typeof(Action<,,,,,,,,>);
				case 10: return typeof(Action<,,,,,,,,,>);
				case 11: return typeof(Action<,,,,,,,,,,>);
				case 12: return typeof(Action<,,,,,,,,,,,>);
				case 13: return typeof(Action<,,,,,,,,,,,,>);
				case 14: return typeof(Action<,,,,,,,,,,,,,>);
				case 15: return typeof(Action<,,,,,,,,,,,,,,>);
				case 16: return typeof(Action<,,,,,,,,,,,,,,,>);

				default:
					throw new Debug.UnreachableException(paramCount.ToString());
			}
		}
		static Type GetDynamicDelegateFuncType(int paramCount)
		{
			switch (paramCount)
			{
				case 0:  return typeof(Func<>);
				case 1:  return typeof(Func<,>);
				case 2:  return typeof(Func<,,>);
				case 3:  return typeof(Func<,,,>);
				case 4:  return typeof(Func<,,,,>);
				case 5:  return typeof(Func<,,,,,>);
				case 6:  return typeof(Func<,,,,,,>);
				case 7:  return typeof(Func<,,,,,,,>);
				case 8:  return typeof(Func<,,,,,,,,>);
				case 9:  return typeof(Func<,,,,,,,,,>);
				case 10: return typeof(Func<,,,,,,,,,,>);
				case 11: return typeof(Func<,,,,,,,,,,,>);
				case 12: return typeof(Func<,,,,,,,,,,,,>);
				case 13: return typeof(Func<,,,,,,,,,,,,,>);
				case 14: return typeof(Func<,,,,,,,,,,,,,,>);
				case 15: return typeof(Func<,,,,,,,,,,,,,,,>);
				case 16: return typeof(Func<,,,,,,,,,,,,,,,,>);

				default:
					throw new Debug.UnreachableException(paramCount.ToString());
			}
		}
		static Type GetDynamicDelegateType(bool hasResult, int paramCount)
		{
			return hasResult 
				? GetDynamicDelegateFuncType(paramCount) 
				: GetDynamicDelegateActionType(paramCount);
		}
		static Type[] GetDynamicDelegateParamTypes(Type result, params Type[] parameters)
		{
			bool has_result = result != null;

			var types = parameters;
			if (has_result)
			{
				types = new Type[parameters.Length + 1];

				int i = 0;
				foreach (var param in parameters)
					types[i++] = param;
				types[i] = result;
			}

			return types;
		}
		public static Type GenerateDynamicDelegateType(Type result, params Type[] parameters)
		{
			Contract.Requires<ArgumentNullException>(parameters != null);
			Contract.Requires<ArgumentException>(parameters.Length <= kGenerateDynamicDelegateMaximumParameters);

			bool has_result = result != null || result != typeof(void);

			var del_type = GetDynamicDelegateType(has_result, parameters.Length);
			var del_params = GetDynamicDelegateParamTypes(result, parameters);

			return del_type.MakeGenericType(del_params);
		}
		public static Type GenerateDynamicDelegateType(Reflect.MethodInfo method)
		{
			return GenerateDynamicDelegateType(method.ReturnType, method.GetParameters().Select(p => p.ParameterType).ToArray());
		}

		public static TFunc GenerateObjectMethodProxy<T, TSig, TFunc>(string methodName, TSig signature)
		{
			Contract.Requires<ArgumentException>(!string.IsNullOrEmpty(methodName));
			Contract.Requires<ArgumentNullException>(signature != null);
			Contract.Requires<ArgumentException>(typeof(TSig).IsSubclassOf(typeof(Delegate)));
			Contract.Requires<ArgumentException>(typeof(TFunc).IsSubclassOf(typeof(Delegate)));
			Contract.Ensures(Contract.Result<TFunc>() != null);

			var type = typeof(T);
			var sig = signature as Delegate;
			var method_params = sig.Method.GetParameters().Select(p => p.ParameterType).ToArray();
			var method = type.GetMethod(methodName, method_params);

			var param_this =Expr.Parameter(type, kThisName);
			var @params =	sig.Method.GetParameters().Select(p => Expr.Parameter(p.ParameterType));
			var call =		Expr.Call(param_this, method, @params);

			var params_lamda = new System.Linq.Expressions.ParameterExpression[method_params.Length+1];
			{
				params_lamda[0] = param_this;
				int i = 1;
				foreach(var param in @params)
					params_lamda[i++] = param;
			}
			return Expr.Lambda<TFunc>(call, params_lamda).Compile();
		}
	};
}