using System;
using System.Collections.Generic;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;
using Exprs = System.Linq.Expressions;

namespace KSoft.IO
{
	partial class TagElementStream<TDoc, TCursor, TName>
	{
		public delegate void StreamAction<T, TContext>(TagElementStream<TDoc, TCursor, TName> s, TContext ctxt, ref T value);
		public delegate void StreamActionNoContext<T>(TagElementStream<TDoc, TCursor, TName> s, ref T value);

		#region Stream Cursor
		/// <summary>Stream the Value of <see cref="Cursor"/> to or from <paramref name="value"/></summary>
		/// <typeparam name="TEnum">Enumeration type</typeparam>
		/// <param name="value">Source or destination value</param>
		/// <param name="isFlags">Is <paramref name="value"/> a <see cref="FlagsAttribute"/> based Enum?</param>
		/// <seealso cref="ReadCursor{TEnum}(TName, ref TEnum)"/>
		/// <seealso cref="WriteCursor(TName, Enum, bool)"/>
		public void StreamCursorEnum<TEnum>(ref TEnum value, bool isFlags = false)
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
				 if (IsReading) ReadCursorEnum(ref value);
			else if (IsWriting) WriteCursorEnum(value, isFlags);
		}

		public void StreamCursor(ref Values.KGuid value)
		{
				 if (IsReading) ReadCursor(ref value);
			else if (IsWriting) WriteCursor(value);
		}

		/// <summary>Stream the Value of attribute <paramref name="Cursor"/> and process it from a string to an id</summary>
		/// <typeparam name="TContext">Resolving context</typeparam>
		/// <typeparam name="TIdentifer">Type representing an id</typeparam>
		/// <param name="id">Source or destination of the postprocessed value</param>
		/// <param name="ctxt">Resolving context</param>
		/// <param name="idResolver">string to id resolver</param>
		/// <param name="stringResolver">id to string resolver</param>
		public void StreamCursorIdAsString<TContext, TIdentifer>(ref TIdentifer id,
			TContext ctxt,
			Func<TContext, string, TIdentifer> idResolver,
			Func<TContext, TIdentifer, string> stringResolver)
		{
			Contract.Requires(idResolver != null && stringResolver != null);

			bool reading = IsReading;
			string str = reading 
				? null 
				: stringResolver(ctxt, id);

			StreamCursor(ref str);

			if (reading)
				id = idResolver(ctxt, str);
		}

		public void StreamCursor(ref DateTime timestamp)
		{
			if (IsReading)
			{
				long time64 = 0; ReadCursor(ref time64, NumeralBase.Hex);
				timestamp = Util.ConvertDateTimeFromUnixTime(time64);
			}
			else if (IsWriting)
			{
				long time64 = Util.ConvertDateTimeToUnixTime(timestamp);
				WriteCursor(time64, NumeralBase.Hex);
			}
		}
		#endregion

		#region Stream Element
		public void StreamElementBegin(TName name, out TCursor oldCursor)
		{
			Contract.Requires(StreamMode != 0, "StreamMode not set! This is an error while trying to actually stream");

			oldCursor = null;

				 if (IsReading) ReadElementBegin(name, out oldCursor);
			else if (IsWriting) WriteElementBegin(name, out oldCursor);

			Contract.Assert(oldCursor != null);
		}
		public void StreamElementEnd(ref TCursor oldCursor)
		{
				 if (IsReading) ReadElementEnd(ref oldCursor);
			else if (IsWriting) WriteElementEnd(ref oldCursor);
		}

		/// <summary>Stream the Value of element <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <typeparam name="TEnum">Enumeration type</typeparam>
		/// <param name="name">Element name</param>
		/// <param name="value">Source or destination value</param>
		/// <param name="isFlags">Is <paramref name="value"/> a <see cref="FlagsAttribute"/> based Enum?</param>
		/// <seealso cref="ReadElement{TEnum}(TName, ref TEnum)"/>
		/// <seealso cref="WriteElement(TName, Enum, bool)"/>
		public void StreamElementEnum<TEnum>(TName name, ref TEnum value, bool isFlags = false)
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			Contract.Requires(ValidateNameArg(name));

				 if (IsReading) ReadElementEnum(name, ref value);
			else if (IsWriting) WriteElementEnum(name, value, isFlags);
		}
		public void StreamElementEnum<T, TEnum>(TName name, T obj, Exprs.Expression<Func<T, TEnum>> propExpr, 
			bool isFlags = false)
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			Contract.Requires(ValidateNameArg(name));

			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (IsReading)
			{
				var value = default( TEnum );
				ReadElementEnum(name, ref value);
				property.SetValue(obj, value, null);
			}
			else if (IsWriting)
				WriteElementEnum(name, (TEnum)property.GetValue(obj, null), isFlags);
		}

		public void StreamElement(TName name, ref Values.KGuid value)
		{
			Contract.Requires(ValidateNameArg(name));

				 if (IsReading) ReadElement(name, ref value);
			else if (IsWriting) WriteElement(name, value);
		}

		/// <summary>Stream the Value of element <paramref name="name"/> and process it from a string to an id</summary>
		/// <typeparam name="TContext">Resolving context</typeparam>
		/// <typeparam name="TIdentifer">Type representing an id</typeparam>
		/// <param name="name">Attribute name</param>
		/// <param name="id">Source or destination of the postprocessed value</param>
		/// <param name="ctxt">Resolving context</param>
		/// <param name="idResolver">string to id resolver</param>
		/// <param name="stringResolver">id to string resolver</param>
		public void StreamElementIdAsString<TContext, TIdentifer>(TName name, ref TIdentifer id,
			TContext ctxt,
			Func<TContext, string, TIdentifer> idResolver,
			Func<TContext, TIdentifer, string> stringResolver)
		{
			Contract.Requires(ValidateNameArg(name));
			Contract.Requires(idResolver != null && stringResolver != null);

			bool reading = IsReading;
			string str = reading 
				? null 
				: stringResolver(ctxt, id);

			StreamElement(name, ref str);

			if (reading)
				id = idResolver(ctxt, str);
		}

		public void StreamElement(TName name, ref DateTime timestamp)
		{
			Contract.Requires(ValidateNameArg(name));

			if (IsReading)
			{
				long time64 = 0; ReadElement(name, ref time64, NumeralBase.Hex);
				timestamp = Util.ConvertDateTimeFromUnixTime(time64);
			}
			else if (IsWriting)
			{
				long time64 = Util.ConvertDateTimeToUnixTime(timestamp);
				WriteElement(name, time64, NumeralBase.Hex);
			}
		}
		#endregion

		#region StreamElementOpt
		/// <summary>Stream the Value of element <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <typeparam name="TEnum">Enumeration type</typeparam>
		/// <param name="name">Element name</param>
		/// <param name="value">Source or destination value</param>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>is</b> written</param>
		/// <returns>True if <paramref name="value"/> was read/written from/to stream</returns>
		/// <param name="isFlags">Is <paramref name="value"/> a <see cref="FlagsElement"/> based Enum?</param>
		/// <seealso cref="ReadElementOpt{TEnum}(TName, ref TEnum)"/>
		/// <seealso cref="WriteElementOptOnTrue{TEnum}(TName, Enum, Predicate{TEnum}, bool)"/>
		public bool StreamElementEnumOpt<TEnum>(TName name, ref TEnum value, Predicate<TEnum> predicate = null, bool isFlags = false)
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			Contract.Requires(ValidateNameArg(name));

			if (predicate == null)
				predicate = Predicates.True<TEnum>;

			bool executed = false;
				 if (IsReading) executed = ReadElementEnumOpt(name, ref value);
			else if (IsWriting) executed = WriteElementEnumOptOnTrue(name, value, predicate, isFlags);
			return executed;
		}
		public bool StreamElementEnumOpt<T, TEnum>(TName name, T obj, Exprs.Expression<Func<T, TEnum>> propExpr, 
			Predicate<TEnum> predicate = null, bool isFlags = false)
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			Contract.Requires(ValidateNameArg(name));

			if (predicate == null)
				predicate = Predicates.True<TEnum>;

			bool executed = false;
			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (IsReading)
			{
				var value = default( TEnum );
				executed = ReadElementEnumOpt(name, ref value);
				property.SetValue(obj, value, null);
			}
			else if (IsWriting)
				executed = WriteElementEnumOptOnTrue(name, (TEnum)property.GetValue(obj, null), predicate, isFlags);

			return executed;
		}

		public bool StreamElementOpt(TName name, ref Values.KGuid value, Predicate<Values.KGuid> predicate = null)
		{
			Contract.Requires(ValidateNameArg(name));

			if (predicate == null)
				predicate = Predicates.True<Values.KGuid>;

			bool executed = false;
				 if (IsReading) executed = ReadElementOpt(name, ref value);
			else if (IsWriting) executed = WriteElementOptOnTrue(name, value, predicate);
			return executed;
		}

		/// <summary>Stream the Value of element <paramref name="name"/> and process it from a string to an id</summary>
		/// <typeparam name="TContext">Resolving context</typeparam>
		/// <typeparam name="TIdentifer">Type representing an id</typeparam>
		/// <param name="name">Attribute name</param>
		/// <param name="id">Source or destination of the postprocessed value</param>
		/// <param name="ctxt">Resolving context</param>
		/// <param name="idResolver">string to id resolver</param>
		/// <param name="stringResolver">id to string resolver</param>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>is</b> written</param>
		/// <returns>True if <paramref name="value"/> was read/written from/to stream</returns>
		public bool StreamElementOptIdAsString<TContext, TIdentifer>(TName name, ref TIdentifer id,
			TContext ctxt,
			Func<TContext, string, TIdentifer> idResolver,
			Func<TContext, TIdentifer, string> stringResolver,
			Predicate<string> predicate = null)
		{
			Contract.Requires(ValidateNameArg(name));
			Contract.Requires(idResolver != null && stringResolver != null);

			bool reading = IsReading;
			string str = reading 
				? null 
				: stringResolver(ctxt, id);

			bool executed = StreamElementOpt(name, ref str, predicate);

			if (reading && executed)
				id = idResolver(ctxt, str);

			return executed;
		}

		public bool StreamElementOpt(TName name, ref DateTime timestamp, Predicate<DateTime> predicate = null)
		{
			Contract.Requires(ValidateNameArg(name));

			if (predicate == null)
				predicate = Predicates.True<DateTime>;

			bool executed = false;
			if (IsReading)
			{
				long time64 = 0;
				executed = ReadElementOpt(name, ref time64, NumeralBase.Hex);
				timestamp = Util.ConvertDateTimeFromUnixTime(time64);
			}
			else if (IsWriting)
			{
				executed = predicate(timestamp);
				if (executed)
				{
					long time64 = Util.ConvertDateTimeToUnixTime(timestamp);
					WriteElement(name, time64, NumeralBase.Hex);
				}
			}
			return executed;
		}
		#endregion

		#region Stream Attribute
		/// <summary>Stream the Value of attribute <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <typeparam name="TEnum">Enumeration type</typeparam>
		/// <param name="name">Attribute name</param>
		/// <param name="value">Source or destination value</param>
		/// <param name="isFlags">Is <paramref name="value"/> a <see cref="FlagsAttribute"/> based Enum?</param>
		/// <seealso cref="ReadElement{TEnum}(string, ref TEnum)"/>
		/// <seealso cref="WriteAttribute(string, Enum, bool)"/>
		public void StreamAttributeEnum<TEnum>(TName name, ref TEnum value, bool isFlags = false)
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			Contract.Requires(ValidateNameArg(name));

				 if (IsReading) ReadAttributeEnum(name, ref value);
			else if (IsWriting) WriteAttributeEnum(name, value, isFlags);
		}
		public void StreamAttributeEnum<T, TEnum>(TName name, T obj, Exprs.Expression<Func<T, TEnum>> propExpr, 
			bool isFlags = false)
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			Contract.Requires(ValidateNameArg(name));

			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (IsReading)
			{
				var value = default( TEnum );
				ReadAttributeEnum(name, ref value);
				property.SetValue(obj, value, null);
			}
			else if (IsWriting)
				WriteAttributeEnum(name, (TEnum)property.GetValue(obj, null), isFlags);
		}

		public void StreamAttribute(TName name, ref Values.KGuid value)
		{
			Contract.Requires(ValidateNameArg(name));

				 if (IsReading) ReadAttribute(name, ref value);
			else if (IsWriting) WriteAttribute(name, value);
		}

		public void StreamAttribute(TName name, ref DateTime timestamp)
		{
			Contract.Requires(ValidateNameArg(name));

			if (IsReading)
			{
				long time64=0; ReadAttribute(name, ref time64, NumeralBase.Hex);
				timestamp = Util.ConvertDateTimeFromUnixTime(time64);
			}
			else if (IsWriting)
			{
				long time64 = Util.ConvertDateTimeToUnixTime(timestamp);
				WriteAttribute(name, time64, NumeralBase.Hex);
			}
		}
		#endregion

		#region StreamAttributeOpt
		/// <summary>Stream the Value of attribute <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <typeparam name="TEnum">Enumeration type</typeparam>
		/// <param name="name">Attribute name</param>
		/// <param name="value">Source or destination value</param>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>is</b> written</param>
		/// <returns>True if <paramref name="value"/> was read/written from/to stream</returns>
		/// <param name="isFlags">Is <paramref name="value"/> a <see cref="FlagsAttribute"/> based Enum?</param>
		/// <seealso cref="ReadAttributeOpt{TEnum}(string, ref TEnum)"/>
		/// <seealso cref="WriteAttributeOptOnTrue{TEnum}(string, Enum, Predicate{TEnum}, bool)"/>
		public bool StreamAttributeEnumOpt<TEnum>(TName name, ref TEnum value, Predicate<TEnum> predicate = null, bool isFlags = false)
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			Contract.Requires(ValidateNameArg(name));

			if (predicate == null)
				predicate = Predicates.True<TEnum>;

			bool executed = false;
				 if (IsReading) executed = ReadAttributeEnumOpt(name, ref value);
			else if (IsWriting) executed = WriteAttributeEnumOptOnTrue(name, value, predicate, isFlags);
			return executed;
		}
		public bool StreamAttributeEnumOpt<T, TEnum>(TName name, T obj, Exprs.Expression<Func<T, TEnum>> propExpr, 
			Predicate<TEnum> predicate = null, bool isFlags = false)
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			Contract.Requires(ValidateNameArg(name));

			if (predicate == null)
				predicate = Predicates.True<TEnum>;

			bool executed = false;
			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (IsReading)
			{
				var value = default( TEnum );
				executed = ReadAttributeEnumOpt(name, ref value);
				property.SetValue(obj, value, null);
			}
			else if (IsWriting)
				executed = WriteAttributeEnumOptOnTrue(name, (TEnum)property.GetValue(obj, null) , predicate, isFlags);

			return executed;
		}

		public bool StreamAttributeOpt(TName name, ref Values.KGuid value, Predicate<Values.KGuid> predicate = null)
		{
			Contract.Requires(ValidateNameArg(name));

			if (predicate == null)
				predicate = Predicates.True<Values.KGuid>;

			bool executed = false;
				 if (IsReading) executed = ReadAttributeOpt(name, ref value);
			else if (IsWriting) executed = WriteAttributeOptOnTrue(name, value, predicate);
			return executed;
		}

		public bool StreamAttributeOpt(TName name, ref DateTime timestamp, Predicate<DateTime> predicate = null)
		{
			Contract.Requires(ValidateNameArg(name));

			if (predicate == null)
				predicate = Predicates.True<DateTime>;

			bool executed = false;
			if (IsReading)
			{
				long time64 = 0;
				executed = ReadAttributeOpt(name, ref time64, NumeralBase.Hex);
				timestamp = Util.ConvertDateTimeFromUnixTime(time64);
			}
			else if (IsWriting)
			{
				executed = predicate(timestamp);
				if (executed)
				{
					long time64 = Util.ConvertDateTimeToUnixTime(timestamp);
					WriteAttribute(name, time64, NumeralBase.Hex);
				}
			}
			return executed;
		}
		#endregion

		// TODO: T4
		// TODO: wrap idResolver calls in try/catch then throw anything via ThrowReadException
		#region StreamAttributeId
		/// <summary>Stream the Value of attribute <paramref name="name"/> and process it from a string to an id</summary>
		/// <typeparam name="TContext">Resolving context</typeparam>
		/// <typeparam name="TIdentifer">Type representing an id</typeparam>
		/// <param name="name">Attribute name</param>
		/// <param name="id">Source or destination of the postprocessed value</param>
		/// <param name="ctxt">Resolving context</param>
		/// <param name="idResolver">string to id resolver</param>
		/// <param name="integerResolver">id to string resolver</param>
		public void StreamAttributeIdAsInt32<TContext, TIdentifer>(TName name, ref TIdentifer id,
			TContext ctxt,
			Func<TContext, int, TIdentifer> idResolver,
			Func<TContext, TIdentifer, int> integerResolver)
		{
			Contract.Requires(ValidateNameArg(name));
			Contract.Requires(idResolver != null && integerResolver != null);

			bool reading = IsReading;
			var integer = reading 
				? TypeExtensions.kNone
				: integerResolver(ctxt, id);

			StreamAttribute(name, ref integer);

			if (reading)
				id = idResolver(ctxt, integer);
		}

		/// <summary>Stream the Value of attribute <paramref name="name"/> and process it from a string to an id</summary>
		/// <typeparam name="TContext">Resolving context</typeparam>
		/// <typeparam name="TIdentifer">Type representing an id</typeparam>
		/// <param name="name">Attribute name</param>
		/// <param name="id">Source or destination of the postprocessed value</param>
		/// <param name="ctxt">Resolving context</param>
		/// <param name="idResolver">string to id resolver</param>
		/// <param name="stringResolver">id to string resolver</param>
		public void StreamAttributeIdAsString<TContext, TIdentifer>(TName name, ref TIdentifer id,
			TContext ctxt,
			Func<TContext, string, TIdentifer> idResolver,
			Func<TContext, TIdentifer, string> stringResolver)
		{
			Contract.Requires(ValidateNameArg(name));
			Contract.Requires(idResolver != null && stringResolver != null);

			bool reading = IsReading;
			string str = reading 
				? null 
				: stringResolver(ctxt, id);

			StreamAttribute(name, ref str);

			if (reading)
				id = idResolver(ctxt, str);
		}
		#endregion

		// TODO: T4
		// TODO: wrap idResolver calls in try/catch then throw anything via ThrowReadException
		#region StreamAttributeOptId
		/// <summary>Stream the Value of attribute <paramref name="name"/> and process it from a integer to an id</summary>
		/// <typeparam name="TContext">Resolving context</typeparam>
		/// <typeparam name="TIdentifer">Type representing an id</typeparam>
		/// <param name="name">Attribute name</param>
		/// <param name="id">Source or destination of the postprocessed value</param>
		/// <param name="ctxt">Resolving context</param>
		/// <param name="idResolver">integer to id resolver</param>
		/// <param name="integerResolver">id to integer resolver</param>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>is</b> written</param>
		/// <returns>True if <paramref name="value"/> was read/written from/to stream</returns>
		public bool StreamAttributeOptIdAsInt32<TContext, TIdentifer>(TName name, ref TIdentifer id,
			TContext ctxt,
			Func<TContext, int, TIdentifer> idResolver,
			Func<TContext, TIdentifer, int> integerResolver,
			Predicate<int> predicate = null)
		{
			Contract.Requires(ValidateNameArg(name));
			Contract.Requires(idResolver != null && integerResolver != null);

			bool reading = IsReading;
			var integer = reading 
				? TypeExtensions.kNone
				: integerResolver(ctxt, id);

			bool executed = StreamAttributeOpt(name, ref integer, predicate);

			if (reading && executed)
				id = idResolver(ctxt, integer);

			return executed;
		}

		/// <summary>Stream the Value of attribute <paramref name="name"/> and process it from a string to an id</summary>
		/// <typeparam name="TContext">Resolving context</typeparam>
		/// <typeparam name="TIdentifer">Type representing an id</typeparam>
		/// <param name="name">Attribute name</param>
		/// <param name="id">Source or destination of the postprocessed value</param>
		/// <param name="ctxt">Resolving context</param>
		/// <param name="idResolver">string to id resolver</param>
		/// <param name="stringResolver">id to string resolver</param>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>is</b> written</param>
		/// <returns>True if <paramref name="value"/> was read/written from/to stream</returns>
		public bool StreamAttributeOptIdAsString<TContext, TIdentifer>(TName name, ref TIdentifer id,
			TContext ctxt,
			Func<TContext, string, TIdentifer> idResolver,
			Func<TContext, TIdentifer, string> stringResolver,
			Predicate<string> predicate = null)
		{
			Contract.Requires(ValidateNameArg(name));
			Contract.Requires(idResolver != null && stringResolver != null);

			bool reading = IsReading;
			string str = reading 
				? null 
				: stringResolver(ctxt, id);

			bool executed = StreamAttributeOpt(name, ref str, predicate);

			if (reading && executed)
				id = idResolver(ctxt, str);

			return executed;
		}
		#endregion

		#region StreamElements (ICollection)
		public void StreamElements<T, TContext>(TName elementName,
			ICollection<T> coll, TContext ctxt, StreamAction<T, TContext> action, Func<TContext, T> ctor)
		{
			Contract.Requires(ValidateNameArg(elementName));
			Contract.Requires<ArgumentNullException>(coll != null);
			Contract.Requires(action != null);
			Contract.Requires(ctor != null);

				 if (IsReading) ReadElements(elementName, coll, ctxt, action, ctor);
			else if (IsWriting) WriteElements(elementName, coll, ctxt, action);
		}
		public void StreamElements<T, TContext>(TName elementName,
			ICollection<T> coll, TContext ctxt, StreamAction<T, TContext> action)
			where T : new()
		{
			Contract.Requires(ValidateNameArg(elementName));
			Contract.Requires<ArgumentNullException>(coll != null);
			Contract.Requires(action != null);

				 if (IsReading) ReadElements(elementName, coll, ctxt, action);
			else if (IsWriting) WriteElements(elementName, coll, ctxt, action);
		}
		public void StreamElements<T, TContext>(TName elementName,
			ICollection<T> coll, TContext ctxt,
			StreamAction<T, TContext> read, StreamAction<T, TContext> write)
			where T : new()
		{
			Contract.Requires(ValidateNameArg(elementName));
			Contract.Requires<ArgumentNullException>(coll != null);
			Contract.Requires(read != null && write != null);

				 if (IsReading) ReadElements(elementName, coll, ctxt, read);
			else if (IsWriting) WriteElements(elementName, coll, ctxt, write);
		}

		public void StreamableElements<T, TContext>(TName elementName,
			ICollection<T> coll, TContext ctxt, Func<TContext, T> ctor,
			Predicate<T> shouldWritePredicate = null)
			where T : ITagElementStreamable<TName>
		{
			Contract.Requires(ValidateNameArg(elementName));
			Contract.Requires<ArgumentNullException>(coll != null);
			Contract.Requires(ctor != null);

				 if (IsReading) ReadStreamableElements(elementName, coll, ctxt, ctor);
			else if (IsWriting) WriteStreamableElements(elementName, coll, shouldWritePredicate);
		}
		public void StreamableElements<T>(TName elementName,
			ICollection<T> coll,
			Predicate<T> shouldWritePredicate = null)
			where T : ITagElementStreamable<TName>, new()
		{
			Contract.Requires(ValidateNameArg(elementName));
			Contract.Requires<ArgumentNullException>(coll != null);

			StreamableElements(elementName, coll, (object)null, (nil) => new T(), shouldWritePredicate);
		}
		#endregion

		#region StreamElements (IDictionary)
		public void StreamElements<TKey, TValue, TContext>(TName elementName,
			IDictionary<TKey, TValue> dic, TContext ctxt,
			StreamAction<TKey, TContext> streamKey,
			StreamAction<TValue, TContext> streamValue, Func<TContext, TValue> valueCtor)
		{
			Contract.Requires(ValidateNameArg(elementName));
			Contract.Requires<ArgumentNullException>(dic != null);
			Contract.Requires(streamKey != null);
			Contract.Requires(streamValue != null && valueCtor != null);

				 if (IsReading) ReadElements(elementName, dic, ctxt, streamKey, streamValue, valueCtor);
			else if (IsWriting) WriteElements(elementName, dic, ctxt, streamKey, streamValue);
		}
		public void StreamElements<TKey, TValue, TContext>(TName elementName,
			IDictionary<TKey, TValue> dic, TContext ctxt,
			StreamAction<TKey, TContext> streamKey,
			StreamAction<TValue, TContext> streamValue)
			where TValue : new()
		{
			Contract.Requires(ValidateNameArg(elementName));
			Contract.Requires<ArgumentNullException>(dic != null);
			Contract.Requires(streamKey != null);
			Contract.Requires(streamValue != null);

				 if (IsReading) ReadElements(elementName, dic, ctxt, streamKey, streamValue);
			else if (IsWriting) WriteElements(elementName, dic, ctxt, streamKey, streamValue);
		}

		public void StreamableElements<TKey, TValue, TContext>(TName elementName,
			IDictionary<TKey, TValue> dic, TContext ctxt,
			StreamAction<TKey, TContext> streamKey,
			Predicate<KeyValuePair<TKey, TValue>> shouldWritePredicate = null)
			where TValue : ITagElementStreamable<TName>, new()
		{
			Contract.Requires(ValidateNameArg(elementName));
			Contract.Requires<ArgumentNullException>(dic != null);

				 if (IsReading) ReadStreamableElements(elementName, dic, ctxt, streamKey);
			else if (IsWriting) WriteStreamableElements(elementName, dic, ctxt, streamKey, shouldWritePredicate);
		}
		public void StreamableElements<TKey, TValue>(TName elementName,
			IDictionary<TKey, TValue> dic,
			StreamAction<TKey, object> streamKey,
			Predicate<KeyValuePair<TKey, TValue>> shouldWritePredicate = null)
			where TValue : ITagElementStreamable<TName>, new()
		{
			Contract.Requires(ValidateNameArg(elementName));
			Contract.Requires<ArgumentNullException>(dic != null);

			StreamableElements(elementName, dic, (object)null, streamKey, shouldWritePredicate);
		}
		#endregion

		#region Stream Fixed Array
		public int StreamableFixedArray<T, TContext>(TName elementName, T[] array,
			TContext ctxt, Func<TContext, T> ctor)
			where T : ITagElementStreamable<TName>
		{
			Contract.Requires(ValidateNameArg(elementName));
			Contract.Requires<ArgumentNullException>(array != null);
			Contract.Requires(ctor != null);

			if (IsReading) return ReadFixedArray(elementName, array, ctxt, ctor);
			else if (IsWriting) WriteStreamableElements(elementName, array);

			return array.Length;
		}
		public int StreamableFixedArray<T>(TName elementName, T[] array)
			where T : ITagElementStreamable<TName>, new()
		{
			Contract.Requires(ValidateNameArg(elementName));
			Contract.Requires<ArgumentNullException>(array != null);

			return StreamableFixedArray(elementName, array, (object)null, (nil) => new T());
		}
		#endregion

		#region Stream Values
		public void StreamValue<T>(ref T value)
			where T : struct, ITagElementStreamable<TName>
		{
			value.Serialize(this);
		}
		public void StreamValue<T>(ref T value, Func<T> initializer)
			where T : struct, ITagElementStreamable<TName>
		{
			Contract.Requires(initializer != null);

			if (IsReading)
				value = initializer();

			value.Serialize(this);
		}
		#endregion

		#region Stream Objects
		public void StreamObject<T>(T obj)
			where T : class, ITagElementStreamable<TName>
		{
			Contract.Requires(obj != null);

			obj.Serialize(this);
		}
		public void StreamObject<T>(T obj, Func<T> initializer)
			where T : class, ITagElementStreamable<TName>
		{
			Contract.Requires(IsReading || obj != null);
			Contract.Requires(initializer != null);

			if (IsReading)
				obj = initializer();

			obj.Serialize(this);
		}
		#endregion

		#region Stream Version (int)
		public void StreamVersionViaCursor(int expectedVersion, string dataDescription)
		{
			Contract.Requires(!string.IsNullOrEmpty(dataDescription));

			var data = expectedVersion;
			StreamCursor(ref data);
			if (data != expectedVersion)
				throw new VersionMismatchException(dataDescription, expectedVersion, data);
		}
		public void StreamVersionViaElement(TName elementName, int expectedVersion, string dataDescription)
		{
			Contract.Requires(!string.IsNullOrEmpty(dataDescription));
			Contract.Requires(ValidateNameArg(elementName));

			var data = expectedVersion;
			StreamElement(elementName, ref data);
			if (data != expectedVersion)
				throw new VersionMismatchException(dataDescription, expectedVersion, data);
		}
		public void StreamVersionViaAttribute(TName attributeName, int expectedVersion, string dataDescription)
		{
			Contract.Requires(!string.IsNullOrEmpty(dataDescription));
			Contract.Requires(ValidateNameArg(attributeName));

			var data = expectedVersion;
			StreamAttribute(attributeName, ref data);
			if (data != expectedVersion)
				throw new VersionMismatchException(dataDescription, expectedVersion, data);
		}
		#endregion

		#region Stream Version (uint)
		public void StreamVersionViaCursor(uint expectedVersion, string dataDescription)
		{
			Contract.Requires(!string.IsNullOrEmpty(dataDescription));

			var data = expectedVersion;
			StreamCursor(ref data);
			if (data != expectedVersion)
				throw new VersionMismatchException(dataDescription, expectedVersion, data);
		}
		public void StreamVersionViaElement(TName elementName, uint expectedVersion, string dataDescription)
		{
			Contract.Requires(!string.IsNullOrEmpty(dataDescription));
			Contract.Requires(ValidateNameArg(elementName));

			var data = expectedVersion;
			StreamElement(elementName, ref data);
			if (data != expectedVersion)
				throw new VersionMismatchException(dataDescription, expectedVersion, data);
		}
		public void StreamVersionViaAttribute(TName attributeName, uint expectedVersion, string dataDescription)
		{
			Contract.Requires(!string.IsNullOrEmpty(dataDescription));
			Contract.Requires(ValidateNameArg(attributeName));

			var data = expectedVersion;
			StreamAttribute(attributeName, ref data);
			if (data != expectedVersion)
				throw new VersionMismatchException(dataDescription, expectedVersion, data);
		}
		#endregion

		#region Stream Signature (string)
		public void StreamSignatureViaCursor(string expectedSignature, string dataDescription)
		{
			Contract.Requires(!string.IsNullOrEmpty(dataDescription));

			var data = expectedSignature;
			StreamCursor(ref data);
			if (data != expectedSignature)
				throw new SignatureMismatchException(dataDescription, expectedSignature, data);
		}
		public void StreamSignatureViaElement(TName elementName, string expectedSignature, string dataDescription)
		{
			Contract.Requires(!string.IsNullOrEmpty(dataDescription));
			Contract.Requires(ValidateNameArg(elementName));

			var data = expectedSignature;
			StreamElement(elementName, ref data);
			if (data != expectedSignature)
				throw new SignatureMismatchException(dataDescription, expectedSignature, data);
		}
		public void StreamSignatureViaAttribute(TName attributeName, string expectedSignature, string dataDescription)
		{
			Contract.Requires(!string.IsNullOrEmpty(dataDescription));
			Contract.Requires(ValidateNameArg(attributeName));

			var data = expectedSignature;
			StreamAttribute(attributeName, ref data);
			if (data != expectedSignature)
				throw new SignatureMismatchException(dataDescription, expectedSignature, data);
		}
		#endregion

		public void StreamString(TName name, ref string value, bool toLower,
			TagElementNodeType type = TagElementNodeType.Attribute, bool intern = false)
		{
			Contract.Requires(type.RequiresName() == (name != null));

				 if (type == TagElementNodeType.Element)	StreamElement(name, ref value);
			else if (type == TagElementNodeType.Attribute)	StreamAttribute(name, ref value);
			else if (type == TagElementNodeType.Text)		StreamCursor(ref value);

			if (IsReading)
			{
				if (toLower) value = value.ToLowerInvariant();
				if (intern) value = string.Intern(value);
			}
		}
		public bool StreamStringOpt(TName name, ref string value, bool toLower,
			TagElementNodeType type = TagElementNodeType.Attribute, bool intern = false)
		{
			Contract.Requires(type.RequiresName() == (name != null));

			bool result = true;
				 if (type == TagElementNodeType.Element)	result = StreamElementOpt(name, ref value, Predicates.IsNotNullOrEmpty);
			else if (type == TagElementNodeType.Attribute)	result = StreamAttributeOpt(name, ref value, Predicates.IsNotNullOrEmpty);
			else if (type == TagElementNodeType.Text)		StreamCursor(ref value);

			if (IsReading)
			{
				if (toLower) value = value.ToLowerInvariant();
				if (intern) value = string.Intern(value);
			}

			return result;
		}

		public bool StreamElementNamedFlag(TName name, ref bool value)
		{
			if (IsReading) value = ElementsExists(name);
			else if (IsWriting && value) WriteElement(name);

			return value;
		}
	};
}