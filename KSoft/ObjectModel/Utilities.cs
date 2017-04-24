using System;
using System.ComponentModel;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;
using Exprs = System.Linq.Expressions;

namespace KSoft.ObjectModel
{
	public static class Util
	{
		// based on System.Windows.Data.Binding.IndexerName in PresentationFramework.dll
		public const string kIndexerPropertyName = "Item[]";
		public static readonly PropertyChangedEventArgs kIndexerPropertyChanged =
			new PropertyChangedEventArgs(kIndexerPropertyName);


		public static readonly System.Collections.Specialized.NotifyCollectionChangedEventArgs kNotifyCollectionReset =
			new System.Collections.Specialized.NotifyCollectionChangedEventArgs(
				System.Collections.Specialized.NotifyCollectionChangedAction.Reset);

		public static PropertyChangedEventArgs CreatePropertyChangedEventArgs<T>(
			Exprs.Expression<Func<T, object>> propertyExpr)
		{
			Contract.Requires(propertyExpr != null);
			Contract.Requires(propertyExpr.Body is Exprs.MemberExpression || propertyExpr.Body is Exprs.UnaryExpression);

			return new PropertyChangedEventArgs(
				Reflection.Util.PropertyFromExpr(propertyExpr).Name);
		}
		public static PropertyChangedEventArgs CreatePropertyChangedEventArgs<T, TProp>(
			Exprs.Expression<Func<T, TProp>> propertyExpr)
		{
			Contract.Requires(propertyExpr != null);
			Contract.Requires(propertyExpr.Body is Exprs.MemberExpression || propertyExpr.Body is Exprs.UnaryExpression);

			return new PropertyChangedEventArgs(
				Reflection.Util.PropertyFromExpr(propertyExpr).Name);
		}
	};
}