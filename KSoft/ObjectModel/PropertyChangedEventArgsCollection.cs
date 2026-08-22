using System;
using System.Collections.Generic;
using Exprs = System.Linq.Expressions;
using ComponentModel = System.ComponentModel;


namespace KSoft.ObjectModel
{
	public class PropertyChangedEventArgsCollection : IEnumerable<ComponentModel.PropertyChangedEventArgs>
	{
		readonly List<ComponentModel.PropertyChangedEventArgs> mEventArgs;

		public PropertyChangedEventArgsCollection()
		{
			mEventArgs = [];
		}
		PropertyChangedEventArgsCollection(IEnumerable<ComponentModel.PropertyChangedEventArgs> eventArgs)
		{
			mEventArgs = new(eventArgs);
		}

		public PropertyChangedEventArgsCollection CreateArgs<T, TProp>(
			out ComponentModel.PropertyChangedEventArgs eventArgs,
			Exprs.Expression<Func<T, TProp>> propertyExpr)
		{
			eventArgs = Util.CreatePropertyChangedEventArgs(propertyExpr);
			mEventArgs.Add(eventArgs);

			return this;
		}

		public PropertyChangedEventArgsCollection Branch()
		{
			var branch = new PropertyChangedEventArgsCollection(mEventArgs);

			System.Diagnostics.Debug.Assert(!ReferenceEquals(branch, this));
			return branch;
		}

		public void NotifyPropertiesChanged(object? sender, ComponentModel.PropertyChangedEventHandler? handler)
		{
			if (handler != null)
			{
				foreach (var args in mEventArgs)
				{
					handler(sender, args);
				}
			}
		}

		#region IEnumerable<PropertyChangedEventArgs> Members
		public IEnumerator<ComponentModel.PropertyChangedEventArgs> GetEnumerator()
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