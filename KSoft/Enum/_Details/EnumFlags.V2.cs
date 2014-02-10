using System;
using System.Collections.Generic;
using System.Reflection;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;
using Expr = System.Linq.Expressions.Expression;
using ExprParam = System.Linq.Expressions.ParameterExpression;

namespace KSoft
{
	using EnumUtils = Reflection.EnumUtils;

	partial class EnumFlags<TEnum>
	{
		/// <summary>
		/// Implements the expressions by directly accessing the enum's internal integer field and doing bitwise 
		/// operations on that instead
		/// </summary>
		static class V2
		{
			public static readonly ModifyDelegate kAddFlags, kRemoveFlags;
			public static readonly ModifyByRefDelegate kAddFlagsByRef, kRemoveFlagsByRef;

			public static readonly ModifyCondDelegate kModifyFlags;
			public static readonly ModifyByRefCondDelegate kModifyFlagsByRef;

			public static readonly ReadDelegate kTestFlags;

			static V2()
			{
				kAddFlags = GenerateAddFlagsMethod();
				kAddFlagsByRef = GenerateAddFlagsMethodByRef();

				kRemoveFlags = GenerateRemoveFlagsMethod();
				kRemoveFlagsByRef = GenerateRemoveFlagsMethodByRef();

				kModifyFlags = GenerateModifyFlagsMethod();
				kModifyFlagsByRef = GenerateModifyFlagsMethodByRef();

				kTestFlags = GenerateTestFlagsMethod();
			}

			static Expr GenerateAddFlagsGuts(ExprParam paramV, ExprParam paramF)
			{
				var param_v_member=Expr.PropertyOrField(paramV, EnumUtils.kMemberName);	// value.value__
				var param_f_member=Expr.PropertyOrField(paramF, EnumUtils.kMemberName);	// flags.value__

				var or = Expr.Or(param_v_member, param_f_member);
				return Expr.Assign(param_v_member, or);
				//return Expr.OrAssign(param_v_member, param_f_member);					// value.value__ |= flags.value__
			}
			static Expr GenerateRemoveFlagsGuts(ExprParam paramV, ExprParam paramF)
			{
				var param_v_member=Expr.PropertyOrField(paramV, EnumUtils.kMemberName);	// value.value__
				var param_f_member=Expr.PropertyOrField(paramF, EnumUtils.kMemberName);	// flags.value__

				var f_complement = Expr.Not(param_f_member);							// ~flags.value__

				var and = Expr.And(param_v_member, f_complement);
				return Expr.Assign(param_v_member, and);
				//return Expr.AndAssign(param_v_member, f_complement);					// value.value__ &= ~flags.value__
			}
			static Expr GenerateModifyFlagsGuts(ExprParam paramV, ExprParam paramF, ExprParam paramCond)
			{
				return Expr.Condition(paramCond,
					GenerateAddFlagsGuts(paramV, paramF),
					GenerateRemoveFlagsGuts(paramV, paramF));
			}

			static ModifyDelegate GenerateAddFlagsMethod()
			{
				//////////////////////////////////////////////////////////////////////////
				// Define the generated method's parameters
				var param_v = GenerateParamValue(false);
				var param_f = GenerateParamFlags();

				//////////////////////////////////////////////////////////////////////////
				// return value | flags
				var ret = Expr.Convert(GenerateAddFlagsGuts(param_v, param_f), kEnumType);

				//////////////////////////////////////////////////////////////////////////
				// Generate a method based on the expression tree we've built
				var lambda = Expr.Lambda<ModifyDelegate>(ret, param_v, param_f);
				return lambda.Compile();
			}
			static ModifyByRefDelegate GenerateAddFlagsMethodByRef()
			{
				//////////////////////////////////////////////////////////////////////////
				// Define the generated method's parameters
				var param_v = GenerateParamValue(true);
				var param_f = GenerateParamFlags();

				//////////////////////////////////////////////////////////////////////////
				// value = value | flags
				var assign = GenerateAddFlagsGuts(param_v, param_f);

				//////////////////////////////////////////////////////////////////////////
				// Generate a method based on the expression tree we've built
				var lambda = Expr.Lambda<ModifyByRefDelegate>(assign, param_v, param_f);
				return lambda.Compile();
			}

			static ModifyDelegate GenerateRemoveFlagsMethod()
			{
				//////////////////////////////////////////////////////////////////////////
				// Define the generated method's parameters
				var param_v = GenerateParamValue(false);
				var param_f = GenerateParamFlags();

				//////////////////////////////////////////////////////////////////////////
				// return value & flags
				var ret = Expr.Convert(GenerateRemoveFlagsGuts(param_v, param_f), kEnumType);

				//////////////////////////////////////////////////////////////////////////
				// Generate a method based on the expression tree we've built
				var lambda = Expr.Lambda<ModifyDelegate>(ret, param_v, param_f);
				return lambda.Compile();
			}
			static ModifyByRefDelegate GenerateRemoveFlagsMethodByRef()
			{
				//////////////////////////////////////////////////////////////////////////
				// Define the generated method's parameters
				var param_v = GenerateParamValue(true);
				var param_f = GenerateParamFlags();

				//////////////////////////////////////////////////////////////////////////
				// value = value & flags
				var assign = GenerateRemoveFlagsGuts(param_v, param_f);

				//////////////////////////////////////////////////////////////////////////
				// Generate a method based on the expression tree we've built
				var lambda = Expr.Lambda<ModifyByRefDelegate>(assign, param_v, param_f);
				return lambda.Compile();
			}

			static ModifyCondDelegate GenerateModifyFlagsMethod()
			{
				//////////////////////////////////////////////////////////////////////////
				// Define the generated method's parameters
				var param_v = GenerateParamValue(false);
				var param_f = GenerateParamFlags();
				var param_c = GenerateParamAddOrRemove();

				//////////////////////////////////////////////////////////////////////////
				// return value & flags
				var ret = Expr.Convert(GenerateModifyFlagsGuts(param_v, param_f, param_c), kEnumType);

				//////////////////////////////////////////////////////////////////////////
				// Generate a method based on the expression tree we've built
				var lambda = Expr.Lambda<ModifyCondDelegate>(ret, param_c, param_v, param_f);
				return lambda.Compile();
			}
			static ModifyByRefCondDelegate GenerateModifyFlagsMethodByRef()
			{
				//////////////////////////////////////////////////////////////////////////
				// Define the generated method's parameters
				var param_v = GenerateParamValue(true);
				var param_f = GenerateParamFlags();
				var param_c = GenerateParamAddOrRemove();

				//////////////////////////////////////////////////////////////////////////
				// return value & flags
				var assign = GenerateModifyFlagsGuts(param_v, param_f, param_c);

				//////////////////////////////////////////////////////////////////////////
				// Generate a method based on the expression tree we've built
				var lambda = Expr.Lambda<ModifyByRefCondDelegate>(assign, param_c, param_v, param_f);
				return lambda.Compile();
			}

			static ReadDelegate GenerateTestFlagsMethod()
			{
				//////////////////////////////////////////////////////////////////////////
				// Define the generated method's parameters and return constructs
				var param_v = GenerateParamValue(false);
				var param_f = GenerateParamFlags();

				//////////////////////////////////////////////////////////////////////////
				// return (value & flags) == flags
				var param_v_member = Expr.PropertyOrField(param_v, EnumUtils.kMemberName);
				var param_f_member = Expr.PropertyOrField(param_f, EnumUtils.kMemberName);

				var and = Expr.And(param_v_member, param_f_member);
				var equ = Expr.Equal(and, param_f_member);

				//////////////////////////////////////////////////////////////////////////
				// Generate a method based on the expression tree we've built
				var lambda = Expr.Lambda<ReadDelegate>(equ, param_v, param_f);
				return lambda.Compile();
			}
		};
	};
}