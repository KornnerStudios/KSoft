using System;
using System.Linq;
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
			return paramCount switch
			{
				0 => typeof(Action),
				1 => typeof(Action<>),
				2 => typeof(Action<,>),
				3 => typeof(Action<,,>),
				4 => typeof(Action<,,,>),
				5 => typeof(Action<,,,,>),
				6 => typeof(Action<,,,,,>),
				7 => typeof(Action<,,,,,,>),
				8 => typeof(Action<,,,,,,,>),
				9 => typeof(Action<,,,,,,,,>),
				10 => typeof(Action<,,,,,,,,,>),
				11 => typeof(Action<,,,,,,,,,,>),
				12 => typeof(Action<,,,,,,,,,,,>),
				13 => typeof(Action<,,,,,,,,,,,,>),
				14 => typeof(Action<,,,,,,,,,,,,,>),
				15 => typeof(Action<,,,,,,,,,,,,,,>),
				16 => typeof(Action<,,,,,,,,,,,,,,,>),
				_ => throw new Debug.UnreachableException(paramCount.ToString(KSoft.Util.InvariantCultureInfo)),
			};
		}
		static Type GetDynamicDelegateFuncType(int paramCount)
		{
			return paramCount switch
			{
				0 => typeof(Func<>),
				1 => typeof(Func<,>),
				2 => typeof(Func<,,>),
				3 => typeof(Func<,,,>),
				4 => typeof(Func<,,,,>),
				5 => typeof(Func<,,,,,>),
				6 => typeof(Func<,,,,,,>),
				7 => typeof(Func<,,,,,,,>),
				8 => typeof(Func<,,,,,,,,>),
				9 => typeof(Func<,,,,,,,,,>),
				10 => typeof(Func<,,,,,,,,,,>),
				11 => typeof(Func<,,,,,,,,,,,>),
				12 => typeof(Func<,,,,,,,,,,,,>),
				13 => typeof(Func<,,,,,,,,,,,,,>),
				14 => typeof(Func<,,,,,,,,,,,,,,>),
				15 => typeof(Func<,,,,,,,,,,,,,,,>),
				16 => typeof(Func<,,,,,,,,,,,,,,,,>),
				_ => throw new Debug.UnreachableException(paramCount.ToString(KSoft.Util.InvariantCultureInfo)),
			};
		}
		static Type GetDynamicDelegateType(bool hasResult, int paramCount)
		{
			return hasResult
				? GetDynamicDelegateFuncType(paramCount)
				: GetDynamicDelegateActionType(paramCount);
		}
		static Type[] GetDynamicDelegateParamTypes(Type? result, params Type[] parameters)
		{
			bool has_result = result != null;

			var types = parameters;
			if (has_result)
			{
				types = new Type[parameters.Length + 1];

				int i = 0;
				foreach (var param in parameters)
				{
					types[i++] = param;
				}

				types[i] = result!;
			}

			return types;
		}
		public static Type GenerateDynamicDelegateType(Type? result, params Type[] parameters)
		{
			ArgumentNullException.ThrowIfNull(parameters);
			if (parameters.Length > kGenerateDynamicDelegateMaximumParameters)
				throw new ArgumentException(null, nameof(parameters));

			bool has_result = result != null || result != typeof(void);

			var del_type = GetDynamicDelegateType(has_result, parameters.Length);
			var del_params = GetDynamicDelegateParamTypes(result, parameters);

			return del_type.MakeGenericType(del_params);
		}
		public static Type GenerateDynamicDelegateType(Reflect.MethodInfo method)
		{
			return GenerateDynamicDelegateType(method.ReturnType, method.GetParameters().Select(p => p.ParameterType).ToArray());
		}

		public static TFunc GenerateObjectMethodProxy<T, TFunc, TSig>(
			string methodName,
			Reflect.BindingFlags bindingAttr = Reflect.BindingFlags.NonPublic | Reflect.BindingFlags.Instance)
			where TFunc : class
			where TSig : class
		{
			ArgumentException.ThrowIfNullOrEmpty(methodName);
			if (!typeof(TSig).IsSubclassOf(typeof(Delegate)))
				throw new ArgumentException(null, nameof(TSig));
			if (!typeof(TFunc).IsSubclassOf(typeof(Delegate)))
				throw new ArgumentException(null, nameof(TFunc));

			var type = typeof(T);
			var sig_method_info = typeof(TSig).GetMethod(kDelegateInvokeMethodName)!;
			var method_params = sig_method_info.GetParameters().Select(p => p.ParameterType).ToArray();
			var method = type.GetMethod(methodName, bindingAttr, null, method_params, null);

#pragma warning disable IDE0270 // Use coalesce expression
			if (method == null)
			{
				throw new InvalidOperationException(string.Format(KSoft.Util.InvariantCultureInfo,
					"Couldn't find a method in {0} named '{1}' ({2})",
					type, methodName, bindingAttr));
			}
#pragma warning restore IDE0270 // Use coalesce expression

			var param_this =Expr.Parameter(type, kThisName);
			// have to convert it to a collection, else a different set of Parameter objects will be created for Call and the Lambda
			var @params =	(from param_type in method_params
							select Expr.Parameter(param_type)).ToArray();
			var call =		Expr.Call(param_this, method, @params);

			var params_lamda = new System.Linq.Expressions.ParameterExpression[method_params.Length+1];
			{
				params_lamda[0] = param_this;
				int i = 1;
				foreach(var param in @params)
				{
					params_lamda[i++] = param;
				}
			}
			return Expr.Lambda<TFunc>(call, params_lamda).Compile();
		}

		#region GenerateConstructorFunc
		// Inspired by: http://rogeralsing.com/2008/02/28/linq-expressions-creating-objects/

		static TFunc GenerateConstructorFuncImpl<TFunc>(
			Type type,
			Reflect.BindingFlags bindingAttr)
			where TFunc : class
		{
			var func_type = typeof(TFunc);
			var func_method_info = func_type.GetMethod(kDelegateInvokeMethodName)!;
			#region func_method_info validation
			if (!func_method_info.ReturnType.IsAssignableFrom(type))
			{
				string msg = string.Format(KSoft.Util.InvariantCultureInfo,
					"Generation failed: {0} returns a {1} which isn't assignable from {2}",
					func_type, func_method_info.ReturnType, type);
				throw new InvalidOperationException(msg);
			}
			#endregion

			var func_params = func_method_info.GetParameters().Select(p => p.ParameterType).ToArray();
			var ctor = type.GetConstructor(bindingAttr, null, func_params, null);
			#region ctor validation
			if (ctor == null)
			{
				var param_names = new System.Text.StringBuilder();
				foreach (var param_type in func_params)
				{
					if (param_names.Length != 0)
					{
						param_names.Append(',');
					}

					param_names.Append(param_type.Name);
				}
				if (param_names.Length == 0)
				{
					param_names.Append("<no-parameters>");
				}

				string msg = string.Format(KSoft.Util.InvariantCultureInfo,
					"Generation failed: {0} has no ctor which matches the bindings '{1}' and takes the following parameter types: {2}",
					type, bindingAttr, param_names);

				throw new InvalidOperationException(msg);
			}
			#endregion

			// have to convert it to a collection, else a different set of Parameter objects will be created for New and the Lambda
			var call_params = (from param_type in func_params
							  select Expr.Parameter(param_type)).ToArray();

			var new_expr = Expr.New(ctor, call_params);
			var lambda = Expr.Lambda<TFunc>(new_expr, call_params);

			return lambda.Compile();
		}

		public static TFunc GenerateConstructorFunc<T, TFunc>(
			Type type,
			Reflect.BindingFlags bindingAttr = Reflect.BindingFlags.Public | Reflect.BindingFlags.Instance)
			where TFunc : class
		{
			ArgumentNullException.ThrowIfNull(type);
			if (!type.IsSubclassOf(typeof(T)))
				throw new ArgumentException(null, nameof(type));
			if (!typeof(TFunc).IsSubclassOf(typeof(Delegate)))
				throw new ArgumentException(null, nameof(TFunc));

			return GenerateConstructorFuncImpl<TFunc>(type, bindingAttr);
		}

		public static TFunc GenerateConstructorFunc<T, TFunc>(
			Reflect.BindingFlags bindingAttr = Reflect.BindingFlags.Public | Reflect.BindingFlags.Instance)
			where TFunc : class
		{
			if (!typeof(TFunc).IsSubclassOf(typeof(Delegate)))
				throw new ArgumentException(null, nameof(TFunc));

			var type = typeof(T);

			return GenerateConstructorFuncImpl<TFunc>(type, bindingAttr);
		}
		#endregion
	};
}
