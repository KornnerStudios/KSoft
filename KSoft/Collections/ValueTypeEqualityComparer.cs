using System;
using System.Collections.Generic;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Collections
{
	/// <summary></summary>
	/// <typeparam name="T"></typeparam>
	/// <remarks>Based on .NET's own (internal) <b>ObjectEqualityComparer{T}</b></remarks>
	sealed class ValueTypeEqualityComparer<T> : EqualityComparer<T>
		where T : struct, IEqualityComparer<T>
	{
		const string kDefaultComparerFieldName = "defaultComparer";

		T mDummy;

		ValueTypeEqualityComparer()				{ mDummy = new T(); }

		static ValueTypeEqualityComparer()
		{
			var default_setter = Reflection.Util.GenerateStaticFieldSetter<EqualityComparer<T>, EqualityComparer<T>>(kDefaultComparerFieldName);

			default_setter(new ValueTypeEqualityComparer<T>());
		}
		// By forwarding Default we avoid having to require users to knowingly touch something explicitly of
		// this class (eg, the ctor) to get the static ctor to execute. If we didn't forward Default, the static
		// ctor wouldn't execute since there are no other static members nor can they call the ctor
		/// <summary>Forwards <see cref="EqualityComparer{T}.Default"/></summary>
		public new static EqualityComparer<T> Default	{ get { return EqualityComparer<T>.Default; } }

		public override bool Equals(object obj)
		{
			var comparer = obj as ValueTypeEqualityComparer<T>;
			return comparer != null;
		}
		public override int GetHashCode()
		{
			return base.GetType().Name.GetHashCode();
		}

		public override bool Equals(T x, T y)	{ return mDummy.Equals(x, y); }
		public override int GetHashCode(T obj)	{ return mDummy.GetHashCode(obj); }
	};

	public sealed class InitializeValueTypeEqualityComparerAttribute : Attribute
	{
		const string kValueTypeEqualityComparer_SingletonPropertyName
			= "Default";

		public InitializeValueTypeEqualityComparerAttribute(Type targetType)
		{
			Contract.Requires(targetType != null);
			Contract.Requires(targetType.IsValueType &&
				targetType.IsCuriouslyRecurringTemplatePattern(typeof(IEqualityComparer<>)),
				"targetType doesn't meet the type constraints of ValueTypeEqualityComparer"
			);

			var generic_type = typeof(ValueTypeEqualityComparer<>);
			// create ValueTypeEqualityComparer<targetType>
			var concrete_type = generic_type.MakeGenericType(targetType);

			concrete_type.ForceStaticCtorToRunViaProperty(kValueTypeEqualityComparer_SingletonPropertyName);
		}
	};
}