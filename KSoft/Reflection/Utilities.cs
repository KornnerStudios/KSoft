using System;
using System.Collections.Generic;
using System.Linq;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;
using Expr = System.Linq.Expressions.Expression;
using Reflect = System.Reflection;
using Interop = System.Runtime.InteropServices;

namespace KSoft.Reflection
{
	public static partial class Util
	{
		public static bool IsEnumType(object maybeType)
		{
			var type = maybeType as Type;
			if (type == null)
				return false;

			return type.IsEnum;
		}

		public static bool IsEnumTypeOrNull(object maybeType)
		{
			if (maybeType == null)
				return true;

			var type = maybeType as Type;
			if (type == null)
				return false;

			return type.IsEnum;
		}

		public static List<Reflect.FieldInfo> GetEnumFields(Type enumType)
		{
			Contract.Requires<ArgumentNullException>(enumType != null);
			Contract.Requires<ArgumentException>(enumType.IsEnum);

			return EnumUtils.GetEnumFields(enumType);
		}

		const string kDelegateInvokeMethodName = "Invoke";
		// http://www.codeproject.com/Tips/441743/A-look-at-marshalling-delegates-in-NET
		public static T GetDelegateForFunctionPointer<T>(IntPtr ptr, Interop.CallingConvention callConv)
			where T : class
		{
			Contract.Requires<ArgumentException>(typeof(T).IsSubclassOf(typeof(Delegate)));
			Contract.Requires<ArgumentNullException>(ptr != IntPtr.Zero);
			Contract.Requires<ArgumentException>(callConv != Interop.CallingConvention.ThisCall,
				"TODO: ThisCall's require a different implementation");

			Contract.Ensures(Contract.Result<T>() != null);

			var type = typeof(T);
			var method = type.GetMethod(kDelegateInvokeMethodName);
			var ret_type = method.ReturnType;
			var param_types = (from param in method.GetParameters()
							   select param.ParameterType)
							  .ToArray();

			var invoke = new Reflect.Emit.DynamicMethod(kDelegateInvokeMethodName, ret_type, param_types,
				typeof(Delegate));
			var il = invoke.GetILGenerator();

			// Generate IL for loading all the args by index
			// TODO: IL has Ldarg_0 to Ldarg_3...do these provide any tangible perf benefits?
			{
				int arg_index = 0;
				if (param_types.Length >= 1) {
					il.Emit(Reflect.Emit.OpCodes.Ldarg_0);
					arg_index++;
				}
				if (param_types.Length >= 2) {
					il.Emit(Reflect.Emit.OpCodes.Ldarg_1);
					arg_index++;
				}
				if (param_types.Length >= 3) {
					il.Emit(Reflect.Emit.OpCodes.Ldarg_2);
					arg_index++;
				}
				if (param_types.Length >= 4) {
					il.Emit(Reflect.Emit.OpCodes.Ldarg_3);
					arg_index++;
				}

				for (int x = arg_index; x < param_types.Length; x++)
					il.Emit(Reflect.Emit.OpCodes.Ldarg, x);
			}

			// Generate the IL for Calli's entry pointer (pushed to the stack)
			if (Environment.Is64BitProcess)
				il.Emit(Reflect.Emit.OpCodes.Ldc_I8, ptr.ToInt64());
			else
				il.Emit(Reflect.Emit.OpCodes.Ldc_I4, ptr.ToInt32());

			il.EmitCalli(Reflect.Emit.OpCodes.Calli, callConv, ret_type, param_types);
			il.Emit(Reflect.Emit.OpCodes.Ret);

			return invoke.CreateDelegate(type) as T;
		}
	};
}