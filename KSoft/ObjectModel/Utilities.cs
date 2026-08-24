using System;
using System.Collections.Generic;
using System.ComponentModel;
using Exprs = System.Linq.Expressions;

namespace KSoft.ObjectModel
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1724:Type names should not match namespaces", Justification = "Established public object-model utility API retained for source compatibility.")]
	public static class Util
	{
		// based on System.Windows.Data.Binding.IndexerName in PresentationFramework.dll
		public const string kIndexerPropertyName = "Item[]";
		public static readonly PropertyChangedEventArgs kIndexerPropertyChanged =
			new(kIndexerPropertyName);


		public static readonly System.Collections.Specialized.NotifyCollectionChangedEventArgs kNotifyCollectionReset =
			new(System.Collections.Specialized.NotifyCollectionChangedAction.Reset);

		static void ValidatePropertyExpression(Exprs.LambdaExpression propertyExpr)
		{
			ArgumentNullException.ThrowIfNull(propertyExpr);
			if (propertyExpr.Body is not Exprs.MemberExpression && propertyExpr.Body is not Exprs.UnaryExpression)
			{
				throw new ArgumentException("Expression must reference a member.", nameof(propertyExpr));
			}
		}

		public static PropertyChangedEventArgs CreatePropertyChangedEventArgs<T>(
			Exprs.Expression<Func<T, object>> propertyExpr)
		{
			ValidatePropertyExpression(propertyExpr);

			return new PropertyChangedEventArgs(
				Reflection.Util.PropertyFromExpr(propertyExpr).Name);
		}
		public static PropertyChangedEventArgs CreatePropertyChangedEventArgs<T, TProp>(
			Exprs.Expression<Func<T, TProp>> propertyExpr)
		{
			ValidatePropertyExpression(propertyExpr);

			return new PropertyChangedEventArgs(
				Reflection.Util.PropertyFromExpr(propertyExpr).Name);
		}

		private static readonly Dictionary<Type, Func<object, object>> gCollectionGetUnderlyingListFuncs = [];
		public static List<T>? GetUnderlyingItemsAsList<T>(System.Collections.ObjectModel.Collection<T>? coll, bool throwOnError = true)
		{
			if (coll == null)
			{
				return null;
			}

			var collType = coll.GetType();
			Func<object, object> getFunc;
			lock (gCollectionGetUnderlyingListFuncs)
			{
				if (!gCollectionGetUnderlyingListFuncs.TryGetValue(collType, out getFunc!))
				{
					getFunc = Reflection.Util.GenerateMemberGetter<object>(collType, "items");
					gCollectionGetUnderlyingListFuncs.Add(collType, getFunc);
				}
			}

			var items = getFunc(coll);
			var list = items as List<T>;

			if (list == null && throwOnError)
			{
				throw new InvalidOperationException(string.Format(KSoft.Util.InvariantCultureInfo,
					"Tried to get Collection's underling Items as a List<{0}> but it is a {1}",
					typeof(T), items?.GetType()));
			}

			return list;
		}
	};
}
