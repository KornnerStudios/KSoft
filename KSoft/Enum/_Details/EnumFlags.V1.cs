using Expr = System.Linq.Expressions.Expression;
using ExprParam = System.Linq.Expressions.ParameterExpression;

namespace KSoft
{
	partial class EnumFlags<TEnum>
	{
		/// <summary>
		/// Implements the expressions by casting the enum parameters to their underlying integer types before
		/// doing a bitwise operation on them
		/// </summary>
		static class V1
		{
			public static readonly ModifyDelegate kAddFlags, kRemoveFlags;
			public static readonly ModifyByRefDelegate kAddFlagsByRef, kRemoveFlagsByRef;

			public static readonly ModifyCondDelegate kModifyFlags;
			public static readonly ModifyByRefCondDelegate kModifyFlagsByRef;

			public static readonly ReadDelegate kTestFlags;

			static V1()
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
				var v_as_int = Expr.Convert(paramV, kUnderlyingType);					// integer v = (integer)value
				var f_as_int = Expr.Convert(paramF, kUnderlyingType);					// integer f = (integer)flags

				return Expr.Convert(Expr.Or(v_as_int, f_as_int), kEnumType);			// (TEnum)(v | f)
			}
			static Expr GenerateRemoveFlagsGuts(ExprParam paramV, ExprParam paramF)
			{
				var v_as_int = Expr.Convert(paramV, kUnderlyingType);					// integer v = (integer)value
				var f_as_int = Expr.Convert(paramF, kUnderlyingType);					// integer f = (integer)flags

				var f_complement = Expr.Not(f_as_int);									// ~f

				return Expr.Convert(Expr.And(v_as_int, f_complement), kEnumType);		// (TEnum)(v & ~f)
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
				var ret = GenerateAddFlagsGuts(param_v, param_f);

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
				var assign = Expr.Assign(param_v, GenerateAddFlagsGuts(param_v, param_f));

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
				var ret = GenerateRemoveFlagsGuts(param_v, param_f);

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
				var assign = Expr.Assign(param_v, GenerateRemoveFlagsGuts(param_v, param_f));

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
				var ret = GenerateModifyFlagsGuts(param_v, param_f, param_c);

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
				var assign = Expr.Assign(param_v, GenerateModifyFlagsGuts(param_v, param_f, param_c));

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
				var v_as_int = Expr.Convert(param_v, kUnderlyingType);
				var f_as_int = Expr.Convert(param_f, kUnderlyingType);

				var and = Expr.Convert(Expr.And(v_as_int, f_as_int), kEnumType);
				var equ = Expr.Equal(and, param_f);

				//////////////////////////////////////////////////////////////////////////
				// Generate a method based on the expression tree we've built
				var lambda = Expr.Lambda<ReadDelegate>(equ, param_v, param_f);
				return lambda.Compile();
			}
		};
	};
}