﻿using System;
using System.Collections.Generic;

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
}