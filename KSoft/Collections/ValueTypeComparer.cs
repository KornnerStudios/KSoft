using System.Collections.Generic;

namespace KSoft.Collections
{
	/// <summary></summary>
	/// <typeparam name="T"></typeparam>
	/// <remarks>Based on .NET's own (internal) <b>ObjectComparer{T}</b></remarks>
	sealed class ValueTypeComparer<T> : Comparer<T>
		where T : struct, System.Collections.IComparer, IComparer<T>
	{
		const string kDefaultComparerFieldName = "defaultComparer";

		T mDummy;

		ValueTypeComparer()						{ mDummy = new T(); }

		static ValueTypeComparer()
		{
			var default_setter = Reflection.Util.GenerateStaticFieldSetter<Comparer<T>, Comparer<T>>(kDefaultComparerFieldName);

			default_setter(new ValueTypeComparer<T>());
		}
		// By forwarding Default we avoid having to require users to knowingly touch something explicitly of
		// this class (eg, the ctor) to get the static ctor to execute. If we didn't forward Default, the static
		// ctor wouldn't execute since there are no other static members nor can they call the ctor
		/// <summary>Forwards <see cref="Comparer{T}.Default"/></summary>
		public new static Comparer<T> Default	{ get { return Comparer<T>.Default; } }

		public override bool Equals(object obj)
		{
			var comparer = obj as ValueTypeComparer<T>;
			return comparer != null;
		}
		public override int GetHashCode()
		{
			return base.GetType().Name.GetHashCode();
		}

		public override int Compare(T x, T y)	{ return mDummy.Compare(x, y); }
	};

#if false // This is aspect programming. Bare Attributes can't be used like this.
	public sealed class InitializeValueTypeComparerAttribute : Attribute
	{
		const string kValueTypeComparer_SingletonPropertyName
			= "Default";

		public InitializeValueTypeComparerAttribute(Type targetType)
		{
			Contract.Requires(targetType != null);
			Contract.Requires(targetType.IsValueType &&
				targetType.ImplementsInterface(typeof(System.Collections.IComparer)) &&
				targetType.IsCuriouslyRecurringTemplatePattern(typeof(IComparer<>)),
				"targetType doesn't meet the type constraints of ValueTypeComparer"
			);

			var generic_type = typeof(ValueTypeComparer<>);
			// create ValueTypeComparer<targetType>
			var concrete_type = generic_type.MakeGenericType(targetType);

			concrete_type.ForceStaticCtorToRunViaProperty(kValueTypeComparer_SingletonPropertyName);
		}
	};
#endif
}