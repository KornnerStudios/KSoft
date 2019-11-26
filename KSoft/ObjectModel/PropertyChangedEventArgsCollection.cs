using System;
using System.Collections.Generic;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif
using Exprs = System.Linq.Expressions;

namespace KSoft.ObjectModel
{
	public class PropertyChangedEventArgsCollection : IEnumerable<System.ComponentModel.PropertyChangedEventArgs>
	{
		List<System.ComponentModel.PropertyChangedEventArgs> mEventArgs;

		public PropertyChangedEventArgsCollection()
		{
			mEventArgs = new List<System.ComponentModel.PropertyChangedEventArgs>();
		}
		PropertyChangedEventArgsCollection(IEnumerable<System.ComponentModel.PropertyChangedEventArgs> eventArgs)
		{
			mEventArgs = new List<System.ComponentModel.PropertyChangedEventArgs>(eventArgs);
		}

		public PropertyChangedEventArgsCollection CreateArgs<T, TProp>(
			out System.ComponentModel.PropertyChangedEventArgs eventArgs,
			Exprs.Expression<Func<T, TProp>> propertyExpr)
		{
			eventArgs = Util.CreatePropertyChangedEventArgs(propertyExpr);
			mEventArgs.Add(eventArgs);

			return this;
		}

		public PropertyChangedEventArgsCollection Branch()
		{
			Contract.Ensures(Contract.Result<PropertyChangedEventArgsCollection>() != this);

			return new PropertyChangedEventArgsCollection(mEventArgs);
		}

		public void NotifyPropertiesChanged(object sender, System.ComponentModel.PropertyChangedEventHandler handler)
		{
			if (handler != null)
				foreach (var args in mEventArgs)
					handler(sender, args);
		}

		#region IEnumerable<PropertyChangedEventArgs> Members
		public IEnumerator<System.ComponentModel.PropertyChangedEventArgs> GetEnumerator()
		{
			return mEventArgs.GetEnumerator();
		}
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return mEventArgs.GetEnumerator();
		}
		#endregion
	};
}