using System;
using System.Collections.Generic;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;
using Exprs = System.Linq.Expressions;

namespace KSoft.IO
{
	partial class TagElementStream<TDoc, TCursor, TName>
	{
		#region Stream Cursor
		/// <summary>Stream the Value of <see cref="Cursor"/> to or from <paramref name="value"/></summary>
		/// <param name="value">Source or destination value</param>
		/// <seealso cref="ReadCursor(string, ref string)"/>
		/// <seealso cref="WriteCursor(string, string)"/>
		public void StreamCursor(ref string value)
		{
				 if (IsReading) ReadCursor(ref value);
			else if (IsWriting) WriteCursor(value);
		}
		/// <summary>Stream the Value of <see cref="Cursor"/> to or from <paramref name="value"/></summary>
		/// <param name="value">Source or destination value</param>
		/// <seealso cref="ReadCursor(string, ref char)"/>
		/// <seealso cref="WriteCursor(string, char)"/>
		public void StreamCursor(ref char value)
		{
				 if (IsReading) ReadCursor(ref value);
			else if (IsWriting) WriteCursor(value);
		}
		/// <summary>Stream the Value of <see cref="Cursor"/> to or from <paramref name="value"/></summary>
		/// <param name="value">Source or destination value</param>
		/// <seealso cref="ReadCursor(string, ref bool)"/>
		/// <seealso cref="WriteCursor(string, bool)"/>
		public void StreamCursor(ref bool value)
		{
				 if (IsReading) ReadCursor(ref value);
			else if (IsWriting) WriteCursor(value);
		}
		/// <summary>Stream the Value of <see cref="Cursor"/> to or from <paramref name="value"/></summary>
		/// <param name="value">Source or destination value</param>
		/// <seealso cref="ReadCursor(string, ref float)"/>
		/// <seealso cref="WriteCursor(string, float)"/>
		public void StreamCursor(ref float value)
		{
				 if (IsReading) ReadCursor(ref value);
			else if (IsWriting) WriteCursor(value);
		}
		/// <summary>Stream the Value of <see cref="Cursor"/> to or from <paramref name="value"/></summary>
		/// <param name="value">Source or destination value</param>
		/// <seealso cref="ReadCursor(string, ref double)"/>
		/// <seealso cref="WriteCursor(string, double)"/>
		public void StreamCursor(ref double value)
		{
				 if (IsReading) ReadCursor(ref value);
			else if (IsWriting) WriteCursor(value);
		}

		/// <summary>Stream the Value of <see cref="Cursor"/> to or from <paramref name="value"/></summary>
		/// <param name="value">Source or destination value</param>
		/// <param name="numBase">numerical base to use</param>
		/// <seealso cref="ReadCursor(string, ref byte, NumeralBase)"/>
		/// <seealso cref="WriteCursor(string, byte, NumeralBase)"/>
		public void StreamCursor(ref byte value, NumeralBase numBase=kDefaultRadix)
		{
				 if (IsReading) ReadCursor(ref value, numBase);
			else if (IsWriting) WriteCursor(value, numBase);
		}
		/// <summary>Stream the Value of <see cref="Cursor"/> to or from <paramref name="value"/></summary>
		/// <param name="value">Source or destination value</param>
		/// <param name="numBase">numerical base to use</param>
		/// <seealso cref="ReadCursor(string, ref sbyte, NumeralBase)"/>
		/// <seealso cref="WriteCursor(string, sbyte, NumeralBase)"/>
		public void StreamCursor(ref sbyte value, NumeralBase numBase=kDefaultRadix)
		{
				 if (IsReading) ReadCursor(ref value, numBase);
			else if (IsWriting) WriteCursor(value, numBase);
		}
		/// <summary>Stream the Value of <see cref="Cursor"/> to or from <paramref name="value"/></summary>
		/// <param name="value">Source or destination value</param>
		/// <param name="numBase">numerical base to use</param>
		/// <seealso cref="ReadCursor(string, ref ushort, NumeralBase)"/>
		/// <seealso cref="WriteCursor(string, ushort, NumeralBase)"/>
		public void StreamCursor(ref ushort value, NumeralBase numBase=kDefaultRadix)
		{
				 if (IsReading) ReadCursor(ref value, numBase);
			else if (IsWriting) WriteCursor(value, numBase);
		}
		/// <summary>Stream the Value of <see cref="Cursor"/> to or from <paramref name="value"/></summary>
		/// <param name="value">Source or destination value</param>
		/// <param name="numBase">numerical base to use</param>
		/// <seealso cref="ReadCursor(string, ref short, NumeralBase)"/>
		/// <seealso cref="WriteCursor(string, short, NumeralBase)"/>
		public void StreamCursor(ref short value, NumeralBase numBase=kDefaultRadix)
		{
				 if (IsReading) ReadCursor(ref value, numBase);
			else if (IsWriting) WriteCursor(value, numBase);
		}
		/// <summary>Stream the Value of <see cref="Cursor"/> to or from <paramref name="value"/></summary>
		/// <param name="value">Source or destination value</param>
		/// <param name="numBase">numerical base to use</param>
		/// <seealso cref="ReadCursor(string, ref uint, NumeralBase)"/>
		/// <seealso cref="WriteCursor(string, uint, NumeralBase)"/>
		public void StreamCursor(ref uint value, NumeralBase numBase=kDefaultRadix)
		{
				 if (IsReading) ReadCursor(ref value, numBase);
			else if (IsWriting) WriteCursor(value, numBase);
		}
		/// <summary>Stream the Value of <see cref="Cursor"/> to or from <paramref name="value"/></summary>
		/// <param name="value">Source or destination value</param>
		/// <param name="numBase">numerical base to use</param>
		/// <seealso cref="ReadCursor(string, ref int, NumeralBase)"/>
		/// <seealso cref="WriteCursor(string, int, NumeralBase)"/>
		public void StreamCursor(ref int value, NumeralBase numBase=kDefaultRadix)
		{
				 if (IsReading) ReadCursor(ref value, numBase);
			else if (IsWriting) WriteCursor(value, numBase);
		}
		/// <summary>Stream the Value of <see cref="Cursor"/> to or from <paramref name="value"/></summary>
		/// <param name="value">Source or destination value</param>
		/// <param name="numBase">numerical base to use</param>
		/// <seealso cref="ReadCursor(string, ref ulong, NumeralBase)"/>
		/// <seealso cref="WriteCursor(string, ulong, NumeralBase)"/>
		public void StreamCursor(ref ulong value, NumeralBase numBase=kDefaultRadix)
		{
				 if (IsReading) ReadCursor(ref value, numBase);
			else if (IsWriting) WriteCursor(value, numBase);
		}
		/// <summary>Stream the Value of <see cref="Cursor"/> to or from <paramref name="value"/></summary>
		/// <param name="value">Source or destination value</param>
		/// <param name="numBase">numerical base to use</param>
		/// <seealso cref="ReadCursor(string, ref long, NumeralBase)"/>
		/// <seealso cref="WriteCursor(string, long, NumeralBase)"/>
		public void StreamCursor(ref long value, NumeralBase numBase=kDefaultRadix)
		{
				 if (IsReading) ReadCursor(ref value, numBase);
			else if (IsWriting) WriteCursor(value, numBase);
		}

		public void StreamCursor<T>( T obj, Exprs.Expression<Func<T, string >> propExpr  )
		{
			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (IsReading)
			{
				var value = default( string );
				ReadCursor( ref value );
				property.SetValue(obj, value, null);
			}
			else if (IsWriting)
				WriteCursor( (string)property.GetValue(obj, null) );
		}
		public void StreamCursor<T>( T obj, Exprs.Expression<Func<T, char >> propExpr  )
		{
			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (IsReading)
			{
				var value = default( char );
				ReadCursor( ref value );
				property.SetValue(obj, value, null);
			}
			else if (IsWriting)
				WriteCursor( (char)property.GetValue(obj, null) );
		}
		public void StreamCursor<T>( T obj, Exprs.Expression<Func<T, bool >> propExpr  )
		{
			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (IsReading)
			{
				var value = default( bool );
				ReadCursor( ref value );
				property.SetValue(obj, value, null);
			}
			else if (IsWriting)
				WriteCursor( (bool)property.GetValue(obj, null) );
		}
		public void StreamCursor<T>( T obj, Exprs.Expression<Func<T, float >> propExpr  )
		{
			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (IsReading)
			{
				var value = default( float );
				ReadCursor( ref value );
				property.SetValue(obj, value, null);
			}
			else if (IsWriting)
				WriteCursor( (float)property.GetValue(obj, null) );
		}
		public void StreamCursor<T>( T obj, Exprs.Expression<Func<T, double >> propExpr  )
		{
			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (IsReading)
			{
				var value = default( double );
				ReadCursor( ref value );
				property.SetValue(obj, value, null);
			}
			else if (IsWriting)
				WriteCursor( (double)property.GetValue(obj, null) );
		}

		public void StreamCursor<T>( T obj, Exprs.Expression<Func<T, byte >> propExpr  , NumeralBase numBase=kDefaultRadix)
		{
			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (IsReading)
			{
				var value = default( byte );
				ReadCursor( ref value , numBase);
				property.SetValue(obj, value, null);
			}
			else if (IsWriting)
				WriteCursor( (byte)property.GetValue(obj, null) , numBase);
		}
		public void StreamCursor<T>( T obj, Exprs.Expression<Func<T, sbyte >> propExpr  , NumeralBase numBase=kDefaultRadix)
		{
			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (IsReading)
			{
				var value = default( sbyte );
				ReadCursor( ref value , numBase);
				property.SetValue(obj, value, null);
			}
			else if (IsWriting)
				WriteCursor( (sbyte)property.GetValue(obj, null) , numBase);
		}
		public void StreamCursor<T>( T obj, Exprs.Expression<Func<T, ushort >> propExpr  , NumeralBase numBase=kDefaultRadix)
		{
			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (IsReading)
			{
				var value = default( ushort );
				ReadCursor( ref value , numBase);
				property.SetValue(obj, value, null);
			}
			else if (IsWriting)
				WriteCursor( (ushort)property.GetValue(obj, null) , numBase);
		}
		public void StreamCursor<T>( T obj, Exprs.Expression<Func<T, short >> propExpr  , NumeralBase numBase=kDefaultRadix)
		{
			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (IsReading)
			{
				var value = default( short );
				ReadCursor( ref value , numBase);
				property.SetValue(obj, value, null);
			}
			else if (IsWriting)
				WriteCursor( (short)property.GetValue(obj, null) , numBase);
		}
		public void StreamCursor<T>( T obj, Exprs.Expression<Func<T, uint >> propExpr  , NumeralBase numBase=kDefaultRadix)
		{
			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (IsReading)
			{
				var value = default( uint );
				ReadCursor( ref value , numBase);
				property.SetValue(obj, value, null);
			}
			else if (IsWriting)
				WriteCursor( (uint)property.GetValue(obj, null) , numBase);
		}
		public void StreamCursor<T>( T obj, Exprs.Expression<Func<T, int >> propExpr  , NumeralBase numBase=kDefaultRadix)
		{
			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (IsReading)
			{
				var value = default( int );
				ReadCursor( ref value , numBase);
				property.SetValue(obj, value, null);
			}
			else if (IsWriting)
				WriteCursor( (int)property.GetValue(obj, null) , numBase);
		}
		public void StreamCursor<T>( T obj, Exprs.Expression<Func<T, ulong >> propExpr  , NumeralBase numBase=kDefaultRadix)
		{
			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (IsReading)
			{
				var value = default( ulong );
				ReadCursor( ref value , numBase);
				property.SetValue(obj, value, null);
			}
			else if (IsWriting)
				WriteCursor( (ulong)property.GetValue(obj, null) , numBase);
		}
		public void StreamCursor<T>( T obj, Exprs.Expression<Func<T, long >> propExpr  , NumeralBase numBase=kDefaultRadix)
		{
			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (IsReading)
			{
				var value = default( long );
				ReadCursor( ref value , numBase);
				property.SetValue(obj, value, null);
			}
			else if (IsWriting)
				WriteCursor( (long)property.GetValue(obj, null) , numBase);
		}
		#endregion


		#region Stream Element
		/// <summary>Stream the Value of element <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <param name="name">Element name</param>
		/// <param name="value">Source or destination value</param>
		/// <seealso cref="ReadElement(string, ref string)"/>
		/// <seealso cref="WriteElement(string, string)"/>
		public void StreamElement(TName name, ref string value)
		{
			Contract.Requires(ValidateNameArg(name));

				 if (IsReading) ReadElement(name, ref value);
			else if (IsWriting) WriteElement(name, value);
		}
		/// <summary>Stream the Value of element <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <param name="name">Element name</param>
		/// <param name="value">Source or destination value</param>
		/// <seealso cref="ReadElement(string, ref char)"/>
		/// <seealso cref="WriteElement(string, char)"/>
		public void StreamElement(TName name, ref char value)
		{
			Contract.Requires(ValidateNameArg(name));

				 if (IsReading) ReadElement(name, ref value);
			else if (IsWriting) WriteElement(name, value);
		}
		/// <summary>Stream the Value of element <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <param name="name">Element name</param>
		/// <param name="value">Source or destination value</param>
		/// <seealso cref="ReadElement(string, ref bool)"/>
		/// <seealso cref="WriteElement(string, bool)"/>
		public void StreamElement(TName name, ref bool value)
		{
			Contract.Requires(ValidateNameArg(name));

				 if (IsReading) ReadElement(name, ref value);
			else if (IsWriting) WriteElement(name, value);
		}
		/// <summary>Stream the Value of element <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <param name="name">Element name</param>
		/// <param name="value">Source or destination value</param>
		/// <seealso cref="ReadElement(string, ref float)"/>
		/// <seealso cref="WriteElement(string, float)"/>
		public void StreamElement(TName name, ref float value)
		{
			Contract.Requires(ValidateNameArg(name));

				 if (IsReading) ReadElement(name, ref value);
			else if (IsWriting) WriteElement(name, value);
		}
		/// <summary>Stream the Value of element <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <param name="name">Element name</param>
		/// <param name="value">Source or destination value</param>
		/// <seealso cref="ReadElement(string, ref double)"/>
		/// <seealso cref="WriteElement(string, double)"/>
		public void StreamElement(TName name, ref double value)
		{
			Contract.Requires(ValidateNameArg(name));

				 if (IsReading) ReadElement(name, ref value);
			else if (IsWriting) WriteElement(name, value);
		}

		/// <summary>Stream the Value of element <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <param name="name">Element name</param>
		/// <param name="value">Source or destination value</param>
		/// <param name="numBase">numerical base to use</param>
		/// <seealso cref="ReadElement(string, ref byte, NumeralBase)"/>
		/// <seealso cref="WriteElement(string, byte, NumeralBase)"/>
		public void StreamElement(TName name, ref byte value, NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(ValidateNameArg(name));

				 if (IsReading) ReadElement(name, ref value, numBase);
			else if (IsWriting) WriteElement(name, value, numBase);
		}
		/// <summary>Stream the Value of element <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <param name="name">Element name</param>
		/// <param name="value">Source or destination value</param>
		/// <param name="numBase">numerical base to use</param>
		/// <seealso cref="ReadElement(string, ref sbyte, NumeralBase)"/>
		/// <seealso cref="WriteElement(string, sbyte, NumeralBase)"/>
		public void StreamElement(TName name, ref sbyte value, NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(ValidateNameArg(name));

				 if (IsReading) ReadElement(name, ref value, numBase);
			else if (IsWriting) WriteElement(name, value, numBase);
		}
		/// <summary>Stream the Value of element <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <param name="name">Element name</param>
		/// <param name="value">Source or destination value</param>
		/// <param name="numBase">numerical base to use</param>
		/// <seealso cref="ReadElement(string, ref ushort, NumeralBase)"/>
		/// <seealso cref="WriteElement(string, ushort, NumeralBase)"/>
		public void StreamElement(TName name, ref ushort value, NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(ValidateNameArg(name));

				 if (IsReading) ReadElement(name, ref value, numBase);
			else if (IsWriting) WriteElement(name, value, numBase);
		}
		/// <summary>Stream the Value of element <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <param name="name">Element name</param>
		/// <param name="value">Source or destination value</param>
		/// <param name="numBase">numerical base to use</param>
		/// <seealso cref="ReadElement(string, ref short, NumeralBase)"/>
		/// <seealso cref="WriteElement(string, short, NumeralBase)"/>
		public void StreamElement(TName name, ref short value, NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(ValidateNameArg(name));

				 if (IsReading) ReadElement(name, ref value, numBase);
			else if (IsWriting) WriteElement(name, value, numBase);
		}
		/// <summary>Stream the Value of element <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <param name="name">Element name</param>
		/// <param name="value">Source or destination value</param>
		/// <param name="numBase">numerical base to use</param>
		/// <seealso cref="ReadElement(string, ref uint, NumeralBase)"/>
		/// <seealso cref="WriteElement(string, uint, NumeralBase)"/>
		public void StreamElement(TName name, ref uint value, NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(ValidateNameArg(name));

				 if (IsReading) ReadElement(name, ref value, numBase);
			else if (IsWriting) WriteElement(name, value, numBase);
		}
		/// <summary>Stream the Value of element <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <param name="name">Element name</param>
		/// <param name="value">Source or destination value</param>
		/// <param name="numBase">numerical base to use</param>
		/// <seealso cref="ReadElement(string, ref int, NumeralBase)"/>
		/// <seealso cref="WriteElement(string, int, NumeralBase)"/>
		public void StreamElement(TName name, ref int value, NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(ValidateNameArg(name));

				 if (IsReading) ReadElement(name, ref value, numBase);
			else if (IsWriting) WriteElement(name, value, numBase);
		}
		/// <summary>Stream the Value of element <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <param name="name">Element name</param>
		/// <param name="value">Source or destination value</param>
		/// <param name="numBase">numerical base to use</param>
		/// <seealso cref="ReadElement(string, ref ulong, NumeralBase)"/>
		/// <seealso cref="WriteElement(string, ulong, NumeralBase)"/>
		public void StreamElement(TName name, ref ulong value, NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(ValidateNameArg(name));

				 if (IsReading) ReadElement(name, ref value, numBase);
			else if (IsWriting) WriteElement(name, value, numBase);
		}
		/// <summary>Stream the Value of element <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <param name="name">Element name</param>
		/// <param name="value">Source or destination value</param>
		/// <param name="numBase">numerical base to use</param>
		/// <seealso cref="ReadElement(string, ref long, NumeralBase)"/>
		/// <seealso cref="WriteElement(string, long, NumeralBase)"/>
		public void StreamElement(TName name, ref long value, NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(ValidateNameArg(name));

				 if (IsReading) ReadElement(name, ref value, numBase);
			else if (IsWriting) WriteElement(name, value, numBase);
		}

		public void StreamElement<T>(TName name, T obj, Exprs.Expression<Func<T, string >> propExpr  )
		{
			Contract.Requires(ValidateNameArg(name));

			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (IsReading)
			{
				var value = default( string );
				ReadElement(name, ref value );
				property.SetValue(obj, value, null);
			}
			else if (IsWriting)
				WriteElement(name, (string)property.GetValue(obj, null) );
		}
		public void StreamElement<T>(TName name, T obj, Exprs.Expression<Func<T, char >> propExpr  )
		{
			Contract.Requires(ValidateNameArg(name));

			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (IsReading)
			{
				var value = default( char );
				ReadElement(name, ref value );
				property.SetValue(obj, value, null);
			}
			else if (IsWriting)
				WriteElement(name, (char)property.GetValue(obj, null) );
		}
		public void StreamElement<T>(TName name, T obj, Exprs.Expression<Func<T, bool >> propExpr  )
		{
			Contract.Requires(ValidateNameArg(name));

			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (IsReading)
			{
				var value = default( bool );
				ReadElement(name, ref value );
				property.SetValue(obj, value, null);
			}
			else if (IsWriting)
				WriteElement(name, (bool)property.GetValue(obj, null) );
		}
		public void StreamElement<T>(TName name, T obj, Exprs.Expression<Func<T, float >> propExpr  )
		{
			Contract.Requires(ValidateNameArg(name));

			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (IsReading)
			{
				var value = default( float );
				ReadElement(name, ref value );
				property.SetValue(obj, value, null);
			}
			else if (IsWriting)
				WriteElement(name, (float)property.GetValue(obj, null) );
		}
		public void StreamElement<T>(TName name, T obj, Exprs.Expression<Func<T, double >> propExpr  )
		{
			Contract.Requires(ValidateNameArg(name));

			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (IsReading)
			{
				var value = default( double );
				ReadElement(name, ref value );
				property.SetValue(obj, value, null);
			}
			else if (IsWriting)
				WriteElement(name, (double)property.GetValue(obj, null) );
		}

		public void StreamElement<T>(TName name, T obj, Exprs.Expression<Func<T, byte >> propExpr  , NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(ValidateNameArg(name));

			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (IsReading)
			{
				var value = default( byte );
				ReadElement(name, ref value , numBase);
				property.SetValue(obj, value, null);
			}
			else if (IsWriting)
				WriteElement(name, (byte)property.GetValue(obj, null) , numBase);
		}
		public void StreamElement<T>(TName name, T obj, Exprs.Expression<Func<T, sbyte >> propExpr  , NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(ValidateNameArg(name));

			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (IsReading)
			{
				var value = default( sbyte );
				ReadElement(name, ref value , numBase);
				property.SetValue(obj, value, null);
			}
			else if (IsWriting)
				WriteElement(name, (sbyte)property.GetValue(obj, null) , numBase);
		}
		public void StreamElement<T>(TName name, T obj, Exprs.Expression<Func<T, ushort >> propExpr  , NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(ValidateNameArg(name));

			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (IsReading)
			{
				var value = default( ushort );
				ReadElement(name, ref value , numBase);
				property.SetValue(obj, value, null);
			}
			else if (IsWriting)
				WriteElement(name, (ushort)property.GetValue(obj, null) , numBase);
		}
		public void StreamElement<T>(TName name, T obj, Exprs.Expression<Func<T, short >> propExpr  , NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(ValidateNameArg(name));

			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (IsReading)
			{
				var value = default( short );
				ReadElement(name, ref value , numBase);
				property.SetValue(obj, value, null);
			}
			else if (IsWriting)
				WriteElement(name, (short)property.GetValue(obj, null) , numBase);
		}
		public void StreamElement<T>(TName name, T obj, Exprs.Expression<Func<T, uint >> propExpr  , NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(ValidateNameArg(name));

			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (IsReading)
			{
				var value = default( uint );
				ReadElement(name, ref value , numBase);
				property.SetValue(obj, value, null);
			}
			else if (IsWriting)
				WriteElement(name, (uint)property.GetValue(obj, null) , numBase);
		}
		public void StreamElement<T>(TName name, T obj, Exprs.Expression<Func<T, int >> propExpr  , NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(ValidateNameArg(name));

			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (IsReading)
			{
				var value = default( int );
				ReadElement(name, ref value , numBase);
				property.SetValue(obj, value, null);
			}
			else if (IsWriting)
				WriteElement(name, (int)property.GetValue(obj, null) , numBase);
		}
		public void StreamElement<T>(TName name, T obj, Exprs.Expression<Func<T, ulong >> propExpr  , NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(ValidateNameArg(name));

			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (IsReading)
			{
				var value = default( ulong );
				ReadElement(name, ref value , numBase);
				property.SetValue(obj, value, null);
			}
			else if (IsWriting)
				WriteElement(name, (ulong)property.GetValue(obj, null) , numBase);
		}
		public void StreamElement<T>(TName name, T obj, Exprs.Expression<Func<T, long >> propExpr  , NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(ValidateNameArg(name));

			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (IsReading)
			{
				var value = default( long );
				ReadElement(name, ref value , numBase);
				property.SetValue(obj, value, null);
			}
			else if (IsWriting)
				WriteElement(name, (long)property.GetValue(obj, null) , numBase);
		}
		#endregion


		#region StreamElementOpt
		/// <summary>Stream the Value of element <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <param name="name">Element name</param>
		/// <param name="value">Source or destination value</param>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>is</b> written</param>
		/// <returns>True if <paramref name="value"/> was read/written from/to stream</returns>
		/// <seealso cref="ReadElementOpt(TName, ref string)"/>
		/// <seealso cref="WriteElementOptOnTrue(TName, string, Predicate{string})"/>
		public bool StreamElementOpt(TName name, ref string value, Predicate<string> predicate = null)
		{
			Contract.Requires(ValidateNameArg(name));

			if (predicate == null)
				predicate = Predicates.True< string >;

			bool executed = false;
				 if (IsReading) executed = ReadElementOpt(name, ref value);
			else if (IsWriting) executed = WriteElementOptOnTrue(name, value, predicate);
			return executed;
		}
		/// <summary>Stream the Value of element <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <param name="name">Element name</param>
		/// <param name="value">Source or destination value</param>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>is</b> written</param>
		/// <returns>True if <paramref name="value"/> was read/written from/to stream</returns>
		/// <seealso cref="ReadElementOpt(TName, ref char)"/>
		/// <seealso cref="WriteElementOptOnTrue(TName, char, Predicate{char})"/>
		public bool StreamElementOpt(TName name, ref char value, Predicate<char> predicate = null)
		{
			Contract.Requires(ValidateNameArg(name));

			if (predicate == null)
				predicate = Predicates.True< char >;

			bool executed = false;
				 if (IsReading) executed = ReadElementOpt(name, ref value);
			else if (IsWriting) executed = WriteElementOptOnTrue(name, value, predicate);
			return executed;
		}
		/// <summary>Stream the Value of element <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <param name="name">Element name</param>
		/// <param name="value">Source or destination value</param>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>is</b> written</param>
		/// <returns>True if <paramref name="value"/> was read/written from/to stream</returns>
		/// <seealso cref="ReadElementOpt(TName, ref bool)"/>
		/// <seealso cref="WriteElementOptOnTrue(TName, bool, Predicate{bool})"/>
		public bool StreamElementOpt(TName name, ref bool value, Predicate<bool> predicate = null)
		{
			Contract.Requires(ValidateNameArg(name));

			if (predicate == null)
				predicate = Predicates.True< bool >;

			bool executed = false;
				 if (IsReading) executed = ReadElementOpt(name, ref value);
			else if (IsWriting) executed = WriteElementOptOnTrue(name, value, predicate);
			return executed;
		}
		/// <summary>Stream the Value of element <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <param name="name">Element name</param>
		/// <param name="value">Source or destination value</param>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>is</b> written</param>
		/// <returns>True if <paramref name="value"/> was read/written from/to stream</returns>
		/// <seealso cref="ReadElementOpt(TName, ref float)"/>
		/// <seealso cref="WriteElementOptOnTrue(TName, float, Predicate{float})"/>
		public bool StreamElementOpt(TName name, ref float value, Predicate<float> predicate = null)
		{
			Contract.Requires(ValidateNameArg(name));

			if (predicate == null)
				predicate = Predicates.True< float >;

			bool executed = false;
				 if (IsReading) executed = ReadElementOpt(name, ref value);
			else if (IsWriting) executed = WriteElementOptOnTrue(name, value, predicate);
			return executed;
		}
		/// <summary>Stream the Value of element <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <param name="name">Element name</param>
		/// <param name="value">Source or destination value</param>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>is</b> written</param>
		/// <returns>True if <paramref name="value"/> was read/written from/to stream</returns>
		/// <seealso cref="ReadElementOpt(TName, ref double)"/>
		/// <seealso cref="WriteElementOptOnTrue(TName, double, Predicate{double})"/>
		public bool StreamElementOpt(TName name, ref double value, Predicate<double> predicate = null)
		{
			Contract.Requires(ValidateNameArg(name));

			if (predicate == null)
				predicate = Predicates.True< double >;

			bool executed = false;
				 if (IsReading) executed = ReadElementOpt(name, ref value);
			else if (IsWriting) executed = WriteElementOptOnTrue(name, value, predicate);
			return executed;
		}

		/// <summary>Stream the Value of element <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <param name="name">Element name</param>
		/// <param name="value">Source or destination value</param>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>is</b> written</param>
		/// <param name="numBase">numerical base to use</param>
		/// <returns>True if <paramref name="value"/> was read/written from/to stream</returns>
		/// <seealso cref="ReadElementOpt(TName, ref byte, NumeralBase)"/>
		/// <seealso cref="WriteElementOptOnTrue(TName, byte, Predicate{byte}, NumeralBase)"/>
		public bool StreamElementOpt(TName name, ref byte value, Predicate<byte> predicate = null, NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(ValidateNameArg(name));

			if (predicate == null)
				predicate = Predicates.True< byte >;

			bool executed = false;
				 if (IsReading) executed = ReadElementOpt(name, ref value, numBase);
			else if (IsWriting) executed = WriteElementOptOnTrue(name, value, predicate, numBase);
			return executed;
		}
		/// <summary>Stream the Value of element <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <param name="name">Element name</param>
		/// <param name="value">Source or destination value</param>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>is</b> written</param>
		/// <param name="numBase">numerical base to use</param>
		/// <returns>True if <paramref name="value"/> was read/written from/to stream</returns>
		/// <seealso cref="ReadElementOpt(TName, ref sbyte, NumeralBase)"/>
		/// <seealso cref="WriteElementOptOnTrue(TName, sbyte, Predicate{sbyte}, NumeralBase)"/>
		public bool StreamElementOpt(TName name, ref sbyte value, Predicate<sbyte> predicate = null, NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(ValidateNameArg(name));

			if (predicate == null)
				predicate = Predicates.True< sbyte >;

			bool executed = false;
				 if (IsReading) executed = ReadElementOpt(name, ref value, numBase);
			else if (IsWriting) executed = WriteElementOptOnTrue(name, value, predicate, numBase);
			return executed;
		}
		/// <summary>Stream the Value of element <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <param name="name">Element name</param>
		/// <param name="value">Source or destination value</param>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>is</b> written</param>
		/// <param name="numBase">numerical base to use</param>
		/// <returns>True if <paramref name="value"/> was read/written from/to stream</returns>
		/// <seealso cref="ReadElementOpt(TName, ref ushort, NumeralBase)"/>
		/// <seealso cref="WriteElementOptOnTrue(TName, ushort, Predicate{ushort}, NumeralBase)"/>
		public bool StreamElementOpt(TName name, ref ushort value, Predicate<ushort> predicate = null, NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(ValidateNameArg(name));

			if (predicate == null)
				predicate = Predicates.True< ushort >;

			bool executed = false;
				 if (IsReading) executed = ReadElementOpt(name, ref value, numBase);
			else if (IsWriting) executed = WriteElementOptOnTrue(name, value, predicate, numBase);
			return executed;
		}
		/// <summary>Stream the Value of element <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <param name="name">Element name</param>
		/// <param name="value">Source or destination value</param>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>is</b> written</param>
		/// <param name="numBase">numerical base to use</param>
		/// <returns>True if <paramref name="value"/> was read/written from/to stream</returns>
		/// <seealso cref="ReadElementOpt(TName, ref short, NumeralBase)"/>
		/// <seealso cref="WriteElementOptOnTrue(TName, short, Predicate{short}, NumeralBase)"/>
		public bool StreamElementOpt(TName name, ref short value, Predicate<short> predicate = null, NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(ValidateNameArg(name));

			if (predicate == null)
				predicate = Predicates.True< short >;

			bool executed = false;
				 if (IsReading) executed = ReadElementOpt(name, ref value, numBase);
			else if (IsWriting) executed = WriteElementOptOnTrue(name, value, predicate, numBase);
			return executed;
		}
		/// <summary>Stream the Value of element <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <param name="name">Element name</param>
		/// <param name="value">Source or destination value</param>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>is</b> written</param>
		/// <param name="numBase">numerical base to use</param>
		/// <returns>True if <paramref name="value"/> was read/written from/to stream</returns>
		/// <seealso cref="ReadElementOpt(TName, ref uint, NumeralBase)"/>
		/// <seealso cref="WriteElementOptOnTrue(TName, uint, Predicate{uint}, NumeralBase)"/>
		public bool StreamElementOpt(TName name, ref uint value, Predicate<uint> predicate = null, NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(ValidateNameArg(name));

			if (predicate == null)
				predicate = Predicates.True< uint >;

			bool executed = false;
				 if (IsReading) executed = ReadElementOpt(name, ref value, numBase);
			else if (IsWriting) executed = WriteElementOptOnTrue(name, value, predicate, numBase);
			return executed;
		}
		/// <summary>Stream the Value of element <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <param name="name">Element name</param>
		/// <param name="value">Source or destination value</param>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>is</b> written</param>
		/// <param name="numBase">numerical base to use</param>
		/// <returns>True if <paramref name="value"/> was read/written from/to stream</returns>
		/// <seealso cref="ReadElementOpt(TName, ref int, NumeralBase)"/>
		/// <seealso cref="WriteElementOptOnTrue(TName, int, Predicate{int}, NumeralBase)"/>
		public bool StreamElementOpt(TName name, ref int value, Predicate<int> predicate = null, NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(ValidateNameArg(name));

			if (predicate == null)
				predicate = Predicates.True< int >;

			bool executed = false;
				 if (IsReading) executed = ReadElementOpt(name, ref value, numBase);
			else if (IsWriting) executed = WriteElementOptOnTrue(name, value, predicate, numBase);
			return executed;
		}
		/// <summary>Stream the Value of element <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <param name="name">Element name</param>
		/// <param name="value">Source or destination value</param>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>is</b> written</param>
		/// <param name="numBase">numerical base to use</param>
		/// <returns>True if <paramref name="value"/> was read/written from/to stream</returns>
		/// <seealso cref="ReadElementOpt(TName, ref ulong, NumeralBase)"/>
		/// <seealso cref="WriteElementOptOnTrue(TName, ulong, Predicate{ulong}, NumeralBase)"/>
		public bool StreamElementOpt(TName name, ref ulong value, Predicate<ulong> predicate = null, NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(ValidateNameArg(name));

			if (predicate == null)
				predicate = Predicates.True< ulong >;

			bool executed = false;
				 if (IsReading) executed = ReadElementOpt(name, ref value, numBase);
			else if (IsWriting) executed = WriteElementOptOnTrue(name, value, predicate, numBase);
			return executed;
		}
		/// <summary>Stream the Value of element <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <param name="name">Element name</param>
		/// <param name="value">Source or destination value</param>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>is</b> written</param>
		/// <param name="numBase">numerical base to use</param>
		/// <returns>True if <paramref name="value"/> was read/written from/to stream</returns>
		/// <seealso cref="ReadElementOpt(TName, ref long, NumeralBase)"/>
		/// <seealso cref="WriteElementOptOnTrue(TName, long, Predicate{long}, NumeralBase)"/>
		public bool StreamElementOpt(TName name, ref long value, Predicate<long> predicate = null, NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(ValidateNameArg(name));

			if (predicate == null)
				predicate = Predicates.True< long >;

			bool executed = false;
				 if (IsReading) executed = ReadElementOpt(name, ref value, numBase);
			else if (IsWriting) executed = WriteElementOptOnTrue(name, value, predicate, numBase);
			return executed;
		}

		public bool StreamElementOpt<T>(TName name, T obj, Exprs.Expression<Func<T, string >> propExpr , Predicate<string> predicate = null )
		{
			Contract.Requires(ValidateNameArg(name));

			if (predicate == null)
				predicate = x => true;

			bool executed = false;
			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (IsReading)
			{
				var value = default( string );
				executed = ReadElementOpt(name, ref value );
				property.SetValue(obj, value, null);
			}
			else if (IsWriting)
				executed = WriteElementOptOnTrue(name, (string)property.GetValue(obj, null) , predicate);

			return executed;
		}
		public bool StreamElementOpt<T>(TName name, T obj, Exprs.Expression<Func<T, char >> propExpr , Predicate<char> predicate = null )
		{
			Contract.Requires(ValidateNameArg(name));

			if (predicate == null)
				predicate = x => true;

			bool executed = false;
			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (IsReading)
			{
				var value = default( char );
				executed = ReadElementOpt(name, ref value );
				property.SetValue(obj, value, null);
			}
			else if (IsWriting)
				executed = WriteElementOptOnTrue(name, (char)property.GetValue(obj, null) , predicate);

			return executed;
		}
		public bool StreamElementOpt<T>(TName name, T obj, Exprs.Expression<Func<T, bool >> propExpr , Predicate<bool> predicate = null )
		{
			Contract.Requires(ValidateNameArg(name));

			if (predicate == null)
				predicate = x => true;

			bool executed = false;
			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (IsReading)
			{
				var value = default( bool );
				executed = ReadElementOpt(name, ref value );
				property.SetValue(obj, value, null);
			}
			else if (IsWriting)
				executed = WriteElementOptOnTrue(name, (bool)property.GetValue(obj, null) , predicate);

			return executed;
		}
		public bool StreamElementOpt<T>(TName name, T obj, Exprs.Expression<Func<T, float >> propExpr , Predicate<float> predicate = null )
		{
			Contract.Requires(ValidateNameArg(name));

			if (predicate == null)
				predicate = x => true;

			bool executed = false;
			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (IsReading)
			{
				var value = default( float );
				executed = ReadElementOpt(name, ref value );
				property.SetValue(obj, value, null);
			}
			else if (IsWriting)
				executed = WriteElementOptOnTrue(name, (float)property.GetValue(obj, null) , predicate);

			return executed;
		}
		public bool StreamElementOpt<T>(TName name, T obj, Exprs.Expression<Func<T, double >> propExpr , Predicate<double> predicate = null )
		{
			Contract.Requires(ValidateNameArg(name));

			if (predicate == null)
				predicate = x => true;

			bool executed = false;
			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (IsReading)
			{
				var value = default( double );
				executed = ReadElementOpt(name, ref value );
				property.SetValue(obj, value, null);
			}
			else if (IsWriting)
				executed = WriteElementOptOnTrue(name, (double)property.GetValue(obj, null) , predicate);

			return executed;
		}

		public bool StreamElementOpt<T>(TName name, T obj, Exprs.Expression<Func<T, byte >> propExpr , Predicate<byte> predicate = null , NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(ValidateNameArg(name));

			if (predicate == null)
				predicate = x => true;

			bool executed = false;
			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (IsReading)
			{
				var value = default( byte );
				executed = ReadElementOpt(name, ref value , numBase);
				property.SetValue(obj, value, null);
			}
			else if (IsWriting)
				executed = WriteElementOptOnTrue(name, (byte)property.GetValue(obj, null) , predicate, numBase);

			return executed;
		}
		public bool StreamElementOpt<T>(TName name, T obj, Exprs.Expression<Func<T, sbyte >> propExpr , Predicate<sbyte> predicate = null , NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(ValidateNameArg(name));

			if (predicate == null)
				predicate = x => true;

			bool executed = false;
			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (IsReading)
			{
				var value = default( sbyte );
				executed = ReadElementOpt(name, ref value , numBase);
				property.SetValue(obj, value, null);
			}
			else if (IsWriting)
				executed = WriteElementOptOnTrue(name, (sbyte)property.GetValue(obj, null) , predicate, numBase);

			return executed;
		}
		public bool StreamElementOpt<T>(TName name, T obj, Exprs.Expression<Func<T, ushort >> propExpr , Predicate<ushort> predicate = null , NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(ValidateNameArg(name));

			if (predicate == null)
				predicate = x => true;

			bool executed = false;
			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (IsReading)
			{
				var value = default( ushort );
				executed = ReadElementOpt(name, ref value , numBase);
				property.SetValue(obj, value, null);
			}
			else if (IsWriting)
				executed = WriteElementOptOnTrue(name, (ushort)property.GetValue(obj, null) , predicate, numBase);

			return executed;
		}
		public bool StreamElementOpt<T>(TName name, T obj, Exprs.Expression<Func<T, short >> propExpr , Predicate<short> predicate = null , NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(ValidateNameArg(name));

			if (predicate == null)
				predicate = x => true;

			bool executed = false;
			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (IsReading)
			{
				var value = default( short );
				executed = ReadElementOpt(name, ref value , numBase);
				property.SetValue(obj, value, null);
			}
			else if (IsWriting)
				executed = WriteElementOptOnTrue(name, (short)property.GetValue(obj, null) , predicate, numBase);

			return executed;
		}
		public bool StreamElementOpt<T>(TName name, T obj, Exprs.Expression<Func<T, uint >> propExpr , Predicate<uint> predicate = null , NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(ValidateNameArg(name));

			if (predicate == null)
				predicate = x => true;

			bool executed = false;
			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (IsReading)
			{
				var value = default( uint );
				executed = ReadElementOpt(name, ref value , numBase);
				property.SetValue(obj, value, null);
			}
			else if (IsWriting)
				executed = WriteElementOptOnTrue(name, (uint)property.GetValue(obj, null) , predicate, numBase);

			return executed;
		}
		public bool StreamElementOpt<T>(TName name, T obj, Exprs.Expression<Func<T, int >> propExpr , Predicate<int> predicate = null , NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(ValidateNameArg(name));

			if (predicate == null)
				predicate = x => true;

			bool executed = false;
			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (IsReading)
			{
				var value = default( int );
				executed = ReadElementOpt(name, ref value , numBase);
				property.SetValue(obj, value, null);
			}
			else if (IsWriting)
				executed = WriteElementOptOnTrue(name, (int)property.GetValue(obj, null) , predicate, numBase);

			return executed;
		}
		public bool StreamElementOpt<T>(TName name, T obj, Exprs.Expression<Func<T, ulong >> propExpr , Predicate<ulong> predicate = null , NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(ValidateNameArg(name));

			if (predicate == null)
				predicate = x => true;

			bool executed = false;
			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (IsReading)
			{
				var value = default( ulong );
				executed = ReadElementOpt(name, ref value , numBase);
				property.SetValue(obj, value, null);
			}
			else if (IsWriting)
				executed = WriteElementOptOnTrue(name, (ulong)property.GetValue(obj, null) , predicate, numBase);

			return executed;
		}
		public bool StreamElementOpt<T>(TName name, T obj, Exprs.Expression<Func<T, long >> propExpr , Predicate<long> predicate = null , NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(ValidateNameArg(name));

			if (predicate == null)
				predicate = x => true;

			bool executed = false;
			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (IsReading)
			{
				var value = default( long );
				executed = ReadElementOpt(name, ref value , numBase);
				property.SetValue(obj, value, null);
			}
			else if (IsWriting)
				executed = WriteElementOptOnTrue(name, (long)property.GetValue(obj, null) , predicate, numBase);

			return executed;
		}
		#endregion


		#region Stream Attribute
		/// <summary>Stream the Value of attribute <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <param name="name">Attribute name</param>
		/// <param name="value">Source or destination value</param>
		/// <seealso cref="ReadAttribute(TName, ref string)"/>
		/// <seealso cref="WriteAttribute(TName, string)"/>
		public void StreamAttribute(TName name, ref string value)
		{
			Contract.Requires(ValidateNameArg(name));

				 if (IsReading) ReadAttribute(name, ref value);
			else if (IsWriting) WriteAttribute(name, value);
		}
		/// <summary>Stream the Value of attribute <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <param name="name">Attribute name</param>
		/// <param name="value">Source or destination value</param>
		/// <seealso cref="ReadAttribute(TName, ref char)"/>
		/// <seealso cref="WriteAttribute(TName, char)"/>
		public void StreamAttribute(TName name, ref char value)
		{
			Contract.Requires(ValidateNameArg(name));

				 if (IsReading) ReadAttribute(name, ref value);
			else if (IsWriting) WriteAttribute(name, value);
		}
		/// <summary>Stream the Value of attribute <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <param name="name">Attribute name</param>
		/// <param name="value">Source or destination value</param>
		/// <seealso cref="ReadAttribute(TName, ref bool)"/>
		/// <seealso cref="WriteAttribute(TName, bool)"/>
		public void StreamAttribute(TName name, ref bool value)
		{
			Contract.Requires(ValidateNameArg(name));

				 if (IsReading) ReadAttribute(name, ref value);
			else if (IsWriting) WriteAttribute(name, value);
		}
		/// <summary>Stream the Value of attribute <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <param name="name">Attribute name</param>
		/// <param name="value">Source or destination value</param>
		/// <seealso cref="ReadAttribute(TName, ref float)"/>
		/// <seealso cref="WriteAttribute(TName, float)"/>
		public void StreamAttribute(TName name, ref float value)
		{
			Contract.Requires(ValidateNameArg(name));

				 if (IsReading) ReadAttribute(name, ref value);
			else if (IsWriting) WriteAttribute(name, value);
		}
		/// <summary>Stream the Value of attribute <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <param name="name">Attribute name</param>
		/// <param name="value">Source or destination value</param>
		/// <seealso cref="ReadAttribute(TName, ref double)"/>
		/// <seealso cref="WriteAttribute(TName, double)"/>
		public void StreamAttribute(TName name, ref double value)
		{
			Contract.Requires(ValidateNameArg(name));

				 if (IsReading) ReadAttribute(name, ref value);
			else if (IsWriting) WriteAttribute(name, value);
		}

		/// <summary>Stream the Value of attribute <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <param name="name">Attribute name</param>
		/// <param name="value">Source or destination value</param>
		/// <param name="numBase">numerical base to use</param>
		/// <seealso cref="ReadAttribute(TName, ref byte, NumeralBase)"/>
		/// <seealso cref="WriteAttribute(TName, byte, NumeralBase)"/>
		public void StreamAttribute(TName name, ref byte value, NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(ValidateNameArg(name));

				 if (IsReading) ReadAttribute(name, ref value, numBase);
			else if (IsWriting) WriteAttribute(name, value, numBase);
		}
		/// <summary>Stream the Value of attribute <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <param name="name">Attribute name</param>
		/// <param name="value">Source or destination value</param>
		/// <param name="numBase">numerical base to use</param>
		/// <seealso cref="ReadAttribute(TName, ref sbyte, NumeralBase)"/>
		/// <seealso cref="WriteAttribute(TName, sbyte, NumeralBase)"/>
		public void StreamAttribute(TName name, ref sbyte value, NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(ValidateNameArg(name));

				 if (IsReading) ReadAttribute(name, ref value, numBase);
			else if (IsWriting) WriteAttribute(name, value, numBase);
		}
		/// <summary>Stream the Value of attribute <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <param name="name">Attribute name</param>
		/// <param name="value">Source or destination value</param>
		/// <param name="numBase">numerical base to use</param>
		/// <seealso cref="ReadAttribute(TName, ref ushort, NumeralBase)"/>
		/// <seealso cref="WriteAttribute(TName, ushort, NumeralBase)"/>
		public void StreamAttribute(TName name, ref ushort value, NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(ValidateNameArg(name));

				 if (IsReading) ReadAttribute(name, ref value, numBase);
			else if (IsWriting) WriteAttribute(name, value, numBase);
		}
		/// <summary>Stream the Value of attribute <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <param name="name">Attribute name</param>
		/// <param name="value">Source or destination value</param>
		/// <param name="numBase">numerical base to use</param>
		/// <seealso cref="ReadAttribute(TName, ref short, NumeralBase)"/>
		/// <seealso cref="WriteAttribute(TName, short, NumeralBase)"/>
		public void StreamAttribute(TName name, ref short value, NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(ValidateNameArg(name));

				 if (IsReading) ReadAttribute(name, ref value, numBase);
			else if (IsWriting) WriteAttribute(name, value, numBase);
		}
		/// <summary>Stream the Value of attribute <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <param name="name">Attribute name</param>
		/// <param name="value">Source or destination value</param>
		/// <param name="numBase">numerical base to use</param>
		/// <seealso cref="ReadAttribute(TName, ref uint, NumeralBase)"/>
		/// <seealso cref="WriteAttribute(TName, uint, NumeralBase)"/>
		public void StreamAttribute(TName name, ref uint value, NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(ValidateNameArg(name));

				 if (IsReading) ReadAttribute(name, ref value, numBase);
			else if (IsWriting) WriteAttribute(name, value, numBase);
		}
		/// <summary>Stream the Value of attribute <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <param name="name">Attribute name</param>
		/// <param name="value">Source or destination value</param>
		/// <param name="numBase">numerical base to use</param>
		/// <seealso cref="ReadAttribute(TName, ref int, NumeralBase)"/>
		/// <seealso cref="WriteAttribute(TName, int, NumeralBase)"/>
		public void StreamAttribute(TName name, ref int value, NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(ValidateNameArg(name));

				 if (IsReading) ReadAttribute(name, ref value, numBase);
			else if (IsWriting) WriteAttribute(name, value, numBase);
		}
		/// <summary>Stream the Value of attribute <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <param name="name">Attribute name</param>
		/// <param name="value">Source or destination value</param>
		/// <param name="numBase">numerical base to use</param>
		/// <seealso cref="ReadAttribute(TName, ref ulong, NumeralBase)"/>
		/// <seealso cref="WriteAttribute(TName, ulong, NumeralBase)"/>
		public void StreamAttribute(TName name, ref ulong value, NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(ValidateNameArg(name));

				 if (IsReading) ReadAttribute(name, ref value, numBase);
			else if (IsWriting) WriteAttribute(name, value, numBase);
		}
		/// <summary>Stream the Value of attribute <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <param name="name">Attribute name</param>
		/// <param name="value">Source or destination value</param>
		/// <param name="numBase">numerical base to use</param>
		/// <seealso cref="ReadAttribute(TName, ref long, NumeralBase)"/>
		/// <seealso cref="WriteAttribute(TName, long, NumeralBase)"/>
		public void StreamAttribute(TName name, ref long value, NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(ValidateNameArg(name));

				 if (IsReading) ReadAttribute(name, ref value, numBase);
			else if (IsWriting) WriteAttribute(name, value, numBase);
		}

		public void StreamAttribute<T>(TName name, T obj, Exprs.Expression<Func<T, string >> propExpr  )
		{
			Contract.Requires(ValidateNameArg(name));

			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (IsReading)
			{
				var value = default( string );
				ReadAttribute(name, ref value );
				property.SetValue(obj, value, null);
			}
			else if (IsWriting)
				WriteAttribute(name, (string)property.GetValue(obj, null) );
		}
		public void StreamAttribute<T>(TName name, T obj, Exprs.Expression<Func<T, char >> propExpr  )
		{
			Contract.Requires(ValidateNameArg(name));

			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (IsReading)
			{
				var value = default( char );
				ReadAttribute(name, ref value );
				property.SetValue(obj, value, null);
			}
			else if (IsWriting)
				WriteAttribute(name, (char)property.GetValue(obj, null) );
		}
		public void StreamAttribute<T>(TName name, T obj, Exprs.Expression<Func<T, bool >> propExpr  )
		{
			Contract.Requires(ValidateNameArg(name));

			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (IsReading)
			{
				var value = default( bool );
				ReadAttribute(name, ref value );
				property.SetValue(obj, value, null);
			}
			else if (IsWriting)
				WriteAttribute(name, (bool)property.GetValue(obj, null) );
		}
		public void StreamAttribute<T>(TName name, T obj, Exprs.Expression<Func<T, float >> propExpr  )
		{
			Contract.Requires(ValidateNameArg(name));

			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (IsReading)
			{
				var value = default( float );
				ReadAttribute(name, ref value );
				property.SetValue(obj, value, null);
			}
			else if (IsWriting)
				WriteAttribute(name, (float)property.GetValue(obj, null) );
		}
		public void StreamAttribute<T>(TName name, T obj, Exprs.Expression<Func<T, double >> propExpr  )
		{
			Contract.Requires(ValidateNameArg(name));

			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (IsReading)
			{
				var value = default( double );
				ReadAttribute(name, ref value );
				property.SetValue(obj, value, null);
			}
			else if (IsWriting)
				WriteAttribute(name, (double)property.GetValue(obj, null) );
		}

		public void StreamAttribute<T>(TName name, T obj, Exprs.Expression<Func<T, byte >> propExpr  , NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(ValidateNameArg(name));

			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (IsReading)
			{
				var value = default( byte );
				ReadAttribute(name, ref value , numBase);
				property.SetValue(obj, value, null);
			}
			else if (IsWriting)
				WriteAttribute(name, (byte)property.GetValue(obj, null) , numBase);
		}
		public void StreamAttribute<T>(TName name, T obj, Exprs.Expression<Func<T, sbyte >> propExpr  , NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(ValidateNameArg(name));

			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (IsReading)
			{
				var value = default( sbyte );
				ReadAttribute(name, ref value , numBase);
				property.SetValue(obj, value, null);
			}
			else if (IsWriting)
				WriteAttribute(name, (sbyte)property.GetValue(obj, null) , numBase);
		}
		public void StreamAttribute<T>(TName name, T obj, Exprs.Expression<Func<T, ushort >> propExpr  , NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(ValidateNameArg(name));

			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (IsReading)
			{
				var value = default( ushort );
				ReadAttribute(name, ref value , numBase);
				property.SetValue(obj, value, null);
			}
			else if (IsWriting)
				WriteAttribute(name, (ushort)property.GetValue(obj, null) , numBase);
		}
		public void StreamAttribute<T>(TName name, T obj, Exprs.Expression<Func<T, short >> propExpr  , NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(ValidateNameArg(name));

			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (IsReading)
			{
				var value = default( short );
				ReadAttribute(name, ref value , numBase);
				property.SetValue(obj, value, null);
			}
			else if (IsWriting)
				WriteAttribute(name, (short)property.GetValue(obj, null) , numBase);
		}
		public void StreamAttribute<T>(TName name, T obj, Exprs.Expression<Func<T, uint >> propExpr  , NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(ValidateNameArg(name));

			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (IsReading)
			{
				var value = default( uint );
				ReadAttribute(name, ref value , numBase);
				property.SetValue(obj, value, null);
			}
			else if (IsWriting)
				WriteAttribute(name, (uint)property.GetValue(obj, null) , numBase);
		}
		public void StreamAttribute<T>(TName name, T obj, Exprs.Expression<Func<T, int >> propExpr  , NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(ValidateNameArg(name));

			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (IsReading)
			{
				var value = default( int );
				ReadAttribute(name, ref value , numBase);
				property.SetValue(obj, value, null);
			}
			else if (IsWriting)
				WriteAttribute(name, (int)property.GetValue(obj, null) , numBase);
		}
		public void StreamAttribute<T>(TName name, T obj, Exprs.Expression<Func<T, ulong >> propExpr  , NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(ValidateNameArg(name));

			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (IsReading)
			{
				var value = default( ulong );
				ReadAttribute(name, ref value , numBase);
				property.SetValue(obj, value, null);
			}
			else if (IsWriting)
				WriteAttribute(name, (ulong)property.GetValue(obj, null) , numBase);
		}
		public void StreamAttribute<T>(TName name, T obj, Exprs.Expression<Func<T, long >> propExpr  , NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(ValidateNameArg(name));

			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (IsReading)
			{
				var value = default( long );
				ReadAttribute(name, ref value , numBase);
				property.SetValue(obj, value, null);
			}
			else if (IsWriting)
				WriteAttribute(name, (long)property.GetValue(obj, null) , numBase);
		}
		#endregion


		#region StreamAttributeOpt
		/// <summary>Stream the Value of attribute <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <param name="name">Attribute name</param>
		/// <param name="value">Source or destination value</param>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>is</b> written</param>
		/// <returns>True if <paramref name="value"/> was read/written from/to stream</returns>
		/// <seealso cref="ReadAttributeOpt(TName, ref string)"/>
		/// <seealso cref="WriteAttributeOptOnTrue(TName, string, Predicate{string})"/>
		public bool StreamAttributeOpt(TName name, ref string value, Predicate<string> predicate = null)
		{
			Contract.Requires(ValidateNameArg(name));

			if (predicate == null)
				predicate = Predicates.True< string >;

			bool executed = false;
				 if (IsReading) executed = ReadAttributeOpt(name, ref value);
			else if (IsWriting) executed = WriteAttributeOptOnTrue(name, value, predicate);
			return executed;
		}
		/// <summary>Stream the Value of attribute <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <param name="name">Attribute name</param>
		/// <param name="value">Source or destination value</param>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>is</b> written</param>
		/// <returns>True if <paramref name="value"/> was read/written from/to stream</returns>
		/// <seealso cref="ReadAttributeOpt(TName, ref char)"/>
		/// <seealso cref="WriteAttributeOptOnTrue(TName, char, Predicate{char})"/>
		public bool StreamAttributeOpt(TName name, ref char value, Predicate<char> predicate = null)
		{
			Contract.Requires(ValidateNameArg(name));

			if (predicate == null)
				predicate = Predicates.True< char >;

			bool executed = false;
				 if (IsReading) executed = ReadAttributeOpt(name, ref value);
			else if (IsWriting) executed = WriteAttributeOptOnTrue(name, value, predicate);
			return executed;
		}
		/// <summary>Stream the Value of attribute <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <param name="name">Attribute name</param>
		/// <param name="value">Source or destination value</param>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>is</b> written</param>
		/// <returns>True if <paramref name="value"/> was read/written from/to stream</returns>
		/// <seealso cref="ReadAttributeOpt(TName, ref bool)"/>
		/// <seealso cref="WriteAttributeOptOnTrue(TName, bool, Predicate{bool})"/>
		public bool StreamAttributeOpt(TName name, ref bool value, Predicate<bool> predicate = null)
		{
			Contract.Requires(ValidateNameArg(name));

			if (predicate == null)
				predicate = Predicates.True< bool >;

			bool executed = false;
				 if (IsReading) executed = ReadAttributeOpt(name, ref value);
			else if (IsWriting) executed = WriteAttributeOptOnTrue(name, value, predicate);
			return executed;
		}
		/// <summary>Stream the Value of attribute <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <param name="name">Attribute name</param>
		/// <param name="value">Source or destination value</param>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>is</b> written</param>
		/// <returns>True if <paramref name="value"/> was read/written from/to stream</returns>
		/// <seealso cref="ReadAttributeOpt(TName, ref float)"/>
		/// <seealso cref="WriteAttributeOptOnTrue(TName, float, Predicate{float})"/>
		public bool StreamAttributeOpt(TName name, ref float value, Predicate<float> predicate = null)
		{
			Contract.Requires(ValidateNameArg(name));

			if (predicate == null)
				predicate = Predicates.True< float >;

			bool executed = false;
				 if (IsReading) executed = ReadAttributeOpt(name, ref value);
			else if (IsWriting) executed = WriteAttributeOptOnTrue(name, value, predicate);
			return executed;
		}
		/// <summary>Stream the Value of attribute <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <param name="name">Attribute name</param>
		/// <param name="value">Source or destination value</param>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>is</b> written</param>
		/// <returns>True if <paramref name="value"/> was read/written from/to stream</returns>
		/// <seealso cref="ReadAttributeOpt(TName, ref double)"/>
		/// <seealso cref="WriteAttributeOptOnTrue(TName, double, Predicate{double})"/>
		public bool StreamAttributeOpt(TName name, ref double value, Predicate<double> predicate = null)
		{
			Contract.Requires(ValidateNameArg(name));

			if (predicate == null)
				predicate = Predicates.True< double >;

			bool executed = false;
				 if (IsReading) executed = ReadAttributeOpt(name, ref value);
			else if (IsWriting) executed = WriteAttributeOptOnTrue(name, value, predicate);
			return executed;
		}

		/// <summary>Stream the Value of attribute <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <param name="name">Attribute name</param>
		/// <param name="value">Source or destination value</param>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>is</b> written</param>
		/// <param name="numBase">numerical base to use</param>
		/// <returns>True if <paramref name="value"/> was read/written from/to stream</returns>
		/// <seealso cref="ReadAttributeOpt(string, ref byte, NumeralBase)"/>
		/// <seealso cref="WriteAttributeOptOnTrue(string, byte, Predicate{byte}, NumeralBase)"/>
		public bool StreamAttributeOpt(TName name, ref byte value, Predicate<byte> predicate = null, NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(ValidateNameArg(name));

			if (predicate == null)
				predicate = Predicates.True< byte >;

			bool executed = false;
				 if (IsReading) executed = ReadAttributeOpt(name, ref value, numBase);
			else if (IsWriting) executed = WriteAttributeOptOnTrue(name, value, predicate, numBase);
			return executed;
		}
		/// <summary>Stream the Value of attribute <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <param name="name">Attribute name</param>
		/// <param name="value">Source or destination value</param>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>is</b> written</param>
		/// <param name="numBase">numerical base to use</param>
		/// <returns>True if <paramref name="value"/> was read/written from/to stream</returns>
		/// <seealso cref="ReadAttributeOpt(string, ref sbyte, NumeralBase)"/>
		/// <seealso cref="WriteAttributeOptOnTrue(string, sbyte, Predicate{sbyte}, NumeralBase)"/>
		public bool StreamAttributeOpt(TName name, ref sbyte value, Predicate<sbyte> predicate = null, NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(ValidateNameArg(name));

			if (predicate == null)
				predicate = Predicates.True< sbyte >;

			bool executed = false;
				 if (IsReading) executed = ReadAttributeOpt(name, ref value, numBase);
			else if (IsWriting) executed = WriteAttributeOptOnTrue(name, value, predicate, numBase);
			return executed;
		}
		/// <summary>Stream the Value of attribute <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <param name="name">Attribute name</param>
		/// <param name="value">Source or destination value</param>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>is</b> written</param>
		/// <param name="numBase">numerical base to use</param>
		/// <returns>True if <paramref name="value"/> was read/written from/to stream</returns>
		/// <seealso cref="ReadAttributeOpt(string, ref ushort, NumeralBase)"/>
		/// <seealso cref="WriteAttributeOptOnTrue(string, ushort, Predicate{ushort}, NumeralBase)"/>
		public bool StreamAttributeOpt(TName name, ref ushort value, Predicate<ushort> predicate = null, NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(ValidateNameArg(name));

			if (predicate == null)
				predicate = Predicates.True< ushort >;

			bool executed = false;
				 if (IsReading) executed = ReadAttributeOpt(name, ref value, numBase);
			else if (IsWriting) executed = WriteAttributeOptOnTrue(name, value, predicate, numBase);
			return executed;
		}
		/// <summary>Stream the Value of attribute <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <param name="name">Attribute name</param>
		/// <param name="value">Source or destination value</param>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>is</b> written</param>
		/// <param name="numBase">numerical base to use</param>
		/// <returns>True if <paramref name="value"/> was read/written from/to stream</returns>
		/// <seealso cref="ReadAttributeOpt(string, ref short, NumeralBase)"/>
		/// <seealso cref="WriteAttributeOptOnTrue(string, short, Predicate{short}, NumeralBase)"/>
		public bool StreamAttributeOpt(TName name, ref short value, Predicate<short> predicate = null, NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(ValidateNameArg(name));

			if (predicate == null)
				predicate = Predicates.True< short >;

			bool executed = false;
				 if (IsReading) executed = ReadAttributeOpt(name, ref value, numBase);
			else if (IsWriting) executed = WriteAttributeOptOnTrue(name, value, predicate, numBase);
			return executed;
		}
		/// <summary>Stream the Value of attribute <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <param name="name">Attribute name</param>
		/// <param name="value">Source or destination value</param>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>is</b> written</param>
		/// <param name="numBase">numerical base to use</param>
		/// <returns>True if <paramref name="value"/> was read/written from/to stream</returns>
		/// <seealso cref="ReadAttributeOpt(string, ref uint, NumeralBase)"/>
		/// <seealso cref="WriteAttributeOptOnTrue(string, uint, Predicate{uint}, NumeralBase)"/>
		public bool StreamAttributeOpt(TName name, ref uint value, Predicate<uint> predicate = null, NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(ValidateNameArg(name));

			if (predicate == null)
				predicate = Predicates.True< uint >;

			bool executed = false;
				 if (IsReading) executed = ReadAttributeOpt(name, ref value, numBase);
			else if (IsWriting) executed = WriteAttributeOptOnTrue(name, value, predicate, numBase);
			return executed;
		}
		/// <summary>Stream the Value of attribute <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <param name="name">Attribute name</param>
		/// <param name="value">Source or destination value</param>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>is</b> written</param>
		/// <param name="numBase">numerical base to use</param>
		/// <returns>True if <paramref name="value"/> was read/written from/to stream</returns>
		/// <seealso cref="ReadAttributeOpt(string, ref int, NumeralBase)"/>
		/// <seealso cref="WriteAttributeOptOnTrue(string, int, Predicate{int}, NumeralBase)"/>
		public bool StreamAttributeOpt(TName name, ref int value, Predicate<int> predicate = null, NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(ValidateNameArg(name));

			if (predicate == null)
				predicate = Predicates.True< int >;

			bool executed = false;
				 if (IsReading) executed = ReadAttributeOpt(name, ref value, numBase);
			else if (IsWriting) executed = WriteAttributeOptOnTrue(name, value, predicate, numBase);
			return executed;
		}
		/// <summary>Stream the Value of attribute <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <param name="name">Attribute name</param>
		/// <param name="value">Source or destination value</param>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>is</b> written</param>
		/// <param name="numBase">numerical base to use</param>
		/// <returns>True if <paramref name="value"/> was read/written from/to stream</returns>
		/// <seealso cref="ReadAttributeOpt(string, ref ulong, NumeralBase)"/>
		/// <seealso cref="WriteAttributeOptOnTrue(string, ulong, Predicate{ulong}, NumeralBase)"/>
		public bool StreamAttributeOpt(TName name, ref ulong value, Predicate<ulong> predicate = null, NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(ValidateNameArg(name));

			if (predicate == null)
				predicate = Predicates.True< ulong >;

			bool executed = false;
				 if (IsReading) executed = ReadAttributeOpt(name, ref value, numBase);
			else if (IsWriting) executed = WriteAttributeOptOnTrue(name, value, predicate, numBase);
			return executed;
		}
		/// <summary>Stream the Value of attribute <paramref name="name"/> to or from <paramref name="value"/></summary>
		/// <param name="name">Attribute name</param>
		/// <param name="value">Source or destination value</param>
		/// <param name="predicate">Predicate that defines the conditions for when <paramref name="value"/> <b>is</b> written</param>
		/// <param name="numBase">numerical base to use</param>
		/// <returns>True if <paramref name="value"/> was read/written from/to stream</returns>
		/// <seealso cref="ReadAttributeOpt(string, ref long, NumeralBase)"/>
		/// <seealso cref="WriteAttributeOptOnTrue(string, long, Predicate{long}, NumeralBase)"/>
		public bool StreamAttributeOpt(TName name, ref long value, Predicate<long> predicate = null, NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(ValidateNameArg(name));

			if (predicate == null)
				predicate = Predicates.True< long >;

			bool executed = false;
				 if (IsReading) executed = ReadAttributeOpt(name, ref value, numBase);
			else if (IsWriting) executed = WriteAttributeOptOnTrue(name, value, predicate, numBase);
			return executed;
		}

		public bool StreamAttributeOpt<T>(TName name, T obj, Exprs.Expression<Func<T, string >> propExpr , Predicate<string> predicate = null )
		{
			Contract.Requires(ValidateNameArg(name));

			if (predicate == null)
				predicate = x => true;

			bool executed = false;
			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (IsReading)
			{
				var value = default( string );
				executed = ReadAttributeOpt(name, ref value );
				property.SetValue(obj, value, null);
			}
			else if (IsWriting)
				executed = WriteAttributeOptOnTrue(name, (string)property.GetValue(obj, null) , predicate);

			return executed;
		}
		public bool StreamAttributeOpt<T>(TName name, T obj, Exprs.Expression<Func<T, char >> propExpr , Predicate<char> predicate = null )
		{
			Contract.Requires(ValidateNameArg(name));

			if (predicate == null)
				predicate = x => true;

			bool executed = false;
			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (IsReading)
			{
				var value = default( char );
				executed = ReadAttributeOpt(name, ref value );
				property.SetValue(obj, value, null);
			}
			else if (IsWriting)
				executed = WriteAttributeOptOnTrue(name, (char)property.GetValue(obj, null) , predicate);

			return executed;
		}
		public bool StreamAttributeOpt<T>(TName name, T obj, Exprs.Expression<Func<T, bool >> propExpr , Predicate<bool> predicate = null )
		{
			Contract.Requires(ValidateNameArg(name));

			if (predicate == null)
				predicate = x => true;

			bool executed = false;
			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (IsReading)
			{
				var value = default( bool );
				executed = ReadAttributeOpt(name, ref value );
				property.SetValue(obj, value, null);
			}
			else if (IsWriting)
				executed = WriteAttributeOptOnTrue(name, (bool)property.GetValue(obj, null) , predicate);

			return executed;
		}
		public bool StreamAttributeOpt<T>(TName name, T obj, Exprs.Expression<Func<T, float >> propExpr , Predicate<float> predicate = null )
		{
			Contract.Requires(ValidateNameArg(name));

			if (predicate == null)
				predicate = x => true;

			bool executed = false;
			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (IsReading)
			{
				var value = default( float );
				executed = ReadAttributeOpt(name, ref value );
				property.SetValue(obj, value, null);
			}
			else if (IsWriting)
				executed = WriteAttributeOptOnTrue(name, (float)property.GetValue(obj, null) , predicate);

			return executed;
		}
		public bool StreamAttributeOpt<T>(TName name, T obj, Exprs.Expression<Func<T, double >> propExpr , Predicate<double> predicate = null )
		{
			Contract.Requires(ValidateNameArg(name));

			if (predicate == null)
				predicate = x => true;

			bool executed = false;
			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (IsReading)
			{
				var value = default( double );
				executed = ReadAttributeOpt(name, ref value );
				property.SetValue(obj, value, null);
			}
			else if (IsWriting)
				executed = WriteAttributeOptOnTrue(name, (double)property.GetValue(obj, null) , predicate);

			return executed;
		}

		public bool StreamAttributeOpt<T>(TName name, T obj, Exprs.Expression<Func<T, byte >> propExpr , Predicate<byte> predicate = null , NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(ValidateNameArg(name));

			if (predicate == null)
				predicate = x => true;

			bool executed = false;
			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (IsReading)
			{
				var value = default( byte );
				executed = ReadAttributeOpt(name, ref value , numBase);
				property.SetValue(obj, value, null);
			}
			else if (IsWriting)
				executed = WriteAttributeOptOnTrue(name, (byte)property.GetValue(obj, null) , predicate, numBase);

			return executed;
		}
		public bool StreamAttributeOpt<T>(TName name, T obj, Exprs.Expression<Func<T, sbyte >> propExpr , Predicate<sbyte> predicate = null , NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(ValidateNameArg(name));

			if (predicate == null)
				predicate = x => true;

			bool executed = false;
			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (IsReading)
			{
				var value = default( sbyte );
				executed = ReadAttributeOpt(name, ref value , numBase);
				property.SetValue(obj, value, null);
			}
			else if (IsWriting)
				executed = WriteAttributeOptOnTrue(name, (sbyte)property.GetValue(obj, null) , predicate, numBase);

			return executed;
		}
		public bool StreamAttributeOpt<T>(TName name, T obj, Exprs.Expression<Func<T, ushort >> propExpr , Predicate<ushort> predicate = null , NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(ValidateNameArg(name));

			if (predicate == null)
				predicate = x => true;

			bool executed = false;
			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (IsReading)
			{
				var value = default( ushort );
				executed = ReadAttributeOpt(name, ref value , numBase);
				property.SetValue(obj, value, null);
			}
			else if (IsWriting)
				executed = WriteAttributeOptOnTrue(name, (ushort)property.GetValue(obj, null) , predicate, numBase);

			return executed;
		}
		public bool StreamAttributeOpt<T>(TName name, T obj, Exprs.Expression<Func<T, short >> propExpr , Predicate<short> predicate = null , NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(ValidateNameArg(name));

			if (predicate == null)
				predicate = x => true;

			bool executed = false;
			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (IsReading)
			{
				var value = default( short );
				executed = ReadAttributeOpt(name, ref value , numBase);
				property.SetValue(obj, value, null);
			}
			else if (IsWriting)
				executed = WriteAttributeOptOnTrue(name, (short)property.GetValue(obj, null) , predicate, numBase);

			return executed;
		}
		public bool StreamAttributeOpt<T>(TName name, T obj, Exprs.Expression<Func<T, uint >> propExpr , Predicate<uint> predicate = null , NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(ValidateNameArg(name));

			if (predicate == null)
				predicate = x => true;

			bool executed = false;
			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (IsReading)
			{
				var value = default( uint );
				executed = ReadAttributeOpt(name, ref value , numBase);
				property.SetValue(obj, value, null);
			}
			else if (IsWriting)
				executed = WriteAttributeOptOnTrue(name, (uint)property.GetValue(obj, null) , predicate, numBase);

			return executed;
		}
		public bool StreamAttributeOpt<T>(TName name, T obj, Exprs.Expression<Func<T, int >> propExpr , Predicate<int> predicate = null , NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(ValidateNameArg(name));

			if (predicate == null)
				predicate = x => true;

			bool executed = false;
			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (IsReading)
			{
				var value = default( int );
				executed = ReadAttributeOpt(name, ref value , numBase);
				property.SetValue(obj, value, null);
			}
			else if (IsWriting)
				executed = WriteAttributeOptOnTrue(name, (int)property.GetValue(obj, null) , predicate, numBase);

			return executed;
		}
		public bool StreamAttributeOpt<T>(TName name, T obj, Exprs.Expression<Func<T, ulong >> propExpr , Predicate<ulong> predicate = null , NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(ValidateNameArg(name));

			if (predicate == null)
				predicate = x => true;

			bool executed = false;
			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (IsReading)
			{
				var value = default( ulong );
				executed = ReadAttributeOpt(name, ref value , numBase);
				property.SetValue(obj, value, null);
			}
			else if (IsWriting)
				executed = WriteAttributeOptOnTrue(name, (ulong)property.GetValue(obj, null) , predicate, numBase);

			return executed;
		}
		public bool StreamAttributeOpt<T>(TName name, T obj, Exprs.Expression<Func<T, long >> propExpr , Predicate<long> predicate = null , NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(ValidateNameArg(name));

			if (predicate == null)
				predicate = x => true;

			bool executed = false;
			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (IsReading)
			{
				var value = default( long );
				executed = ReadAttributeOpt(name, ref value , numBase);
				property.SetValue(obj, value, null);
			}
			else if (IsWriting)
				executed = WriteAttributeOptOnTrue(name, (long)property.GetValue(obj, null) , predicate, numBase);

			return executed;
		}
		#endregion


		#region Stream Elements
		public void StreamElements(TName name, ICollection< string > coll)
		{
			Contract.Requires(ValidateNameArg(name));
			Contract.Requires<ArgumentNullException>(coll != null);

				 if (IsReading) ReadElements(name, coll);
			else if (IsWriting) WriteElements(name, coll);
		}
		public void StreamElements(TName name, ICollection< char > coll)
		{
			Contract.Requires(ValidateNameArg(name));
			Contract.Requires<ArgumentNullException>(coll != null);

				 if (IsReading) ReadElements(name, coll);
			else if (IsWriting) WriteElements(name, coll);
		}
		public void StreamElements(TName name, ICollection< bool > coll)
		{
			Contract.Requires(ValidateNameArg(name));
			Contract.Requires<ArgumentNullException>(coll != null);

				 if (IsReading) ReadElements(name, coll);
			else if (IsWriting) WriteElements(name, coll);
		}
		public void StreamElements(TName name, ICollection< float > coll)
		{
			Contract.Requires(ValidateNameArg(name));
			Contract.Requires<ArgumentNullException>(coll != null);

				 if (IsReading) ReadElements(name, coll);
			else if (IsWriting) WriteElements(name, coll);
		}
		public void StreamElements(TName name, ICollection< double > coll)
		{
			Contract.Requires(ValidateNameArg(name));
			Contract.Requires<ArgumentNullException>(coll != null);

				 if (IsReading) ReadElements(name, coll);
			else if (IsWriting) WriteElements(name, coll);
		}

		public void StreamElements(TName name, ICollection< byte > coll, NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(ValidateNameArg(name));
			Contract.Requires<ArgumentNullException>(coll != null);

				 if (IsReading) ReadElements(name, coll, numBase);
			else if (IsWriting) WriteElements(name, coll, numBase);
		}
		public void StreamElements(TName name, ICollection< sbyte > coll, NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(ValidateNameArg(name));
			Contract.Requires<ArgumentNullException>(coll != null);

				 if (IsReading) ReadElements(name, coll, numBase);
			else if (IsWriting) WriteElements(name, coll, numBase);
		}
		public void StreamElements(TName name, ICollection< ushort > coll, NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(ValidateNameArg(name));
			Contract.Requires<ArgumentNullException>(coll != null);

				 if (IsReading) ReadElements(name, coll, numBase);
			else if (IsWriting) WriteElements(name, coll, numBase);
		}
		public void StreamElements(TName name, ICollection< short > coll, NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(ValidateNameArg(name));
			Contract.Requires<ArgumentNullException>(coll != null);

				 if (IsReading) ReadElements(name, coll, numBase);
			else if (IsWriting) WriteElements(name, coll, numBase);
		}
		public void StreamElements(TName name, ICollection< uint > coll, NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(ValidateNameArg(name));
			Contract.Requires<ArgumentNullException>(coll != null);

				 if (IsReading) ReadElements(name, coll, numBase);
			else if (IsWriting) WriteElements(name, coll, numBase);
		}
		public void StreamElements(TName name, ICollection< int > coll, NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(ValidateNameArg(name));
			Contract.Requires<ArgumentNullException>(coll != null);

				 if (IsReading) ReadElements(name, coll, numBase);
			else if (IsWriting) WriteElements(name, coll, numBase);
		}
		public void StreamElements(TName name, ICollection< ulong > coll, NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(ValidateNameArg(name));
			Contract.Requires<ArgumentNullException>(coll != null);

				 if (IsReading) ReadElements(name, coll, numBase);
			else if (IsWriting) WriteElements(name, coll, numBase);
		}
		public void StreamElements(TName name, ICollection< long > coll, NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(ValidateNameArg(name));
			Contract.Requires<ArgumentNullException>(coll != null);

				 if (IsReading) ReadElements(name, coll, numBase);
			else if (IsWriting) WriteElements(name, coll, numBase);
		}
		#endregion


		#region Stream Fixed Array
		public int StreamFixedArray(TName name, string[] array)
		{
			Contract.Requires(ValidateNameArg(name));
			Contract.Requires<ArgumentNullException>(array != null);

				 if (IsReading) return ReadFixedArray(name, array);
			else if (IsWriting) WriteElements(name, array);

			return array.Length;
		}
		public int StreamFixedArray(TName name, char[] array)
		{
			Contract.Requires(ValidateNameArg(name));
			Contract.Requires<ArgumentNullException>(array != null);

				 if (IsReading) return ReadFixedArray(name, array);
			else if (IsWriting) WriteElements(name, array);

			return array.Length;
		}
		public int StreamFixedArray(TName name, bool[] array)
		{
			Contract.Requires(ValidateNameArg(name));
			Contract.Requires<ArgumentNullException>(array != null);

				 if (IsReading) return ReadFixedArray(name, array);
			else if (IsWriting) WriteElements(name, array);

			return array.Length;
		}
		public int StreamFixedArray(TName name, float[] array)
		{
			Contract.Requires(ValidateNameArg(name));
			Contract.Requires<ArgumentNullException>(array != null);

				 if (IsReading) return ReadFixedArray(name, array);
			else if (IsWriting) WriteElements(name, array);

			return array.Length;
		}
		public int StreamFixedArray(TName name, double[] array)
		{
			Contract.Requires(ValidateNameArg(name));
			Contract.Requires<ArgumentNullException>(array != null);

				 if (IsReading) return ReadFixedArray(name, array);
			else if (IsWriting) WriteElements(name, array);

			return array.Length;
		}

		public int StreamFixedArray(TName name, byte[] array, NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(ValidateNameArg(name));
			Contract.Requires<ArgumentNullException>(array != null);

				 if (IsReading) return ReadFixedArray(name, array, numBase);
			else if (IsWriting) WriteElements(name, array, numBase);

			return array.Length;
		}
		public int StreamFixedArray(TName name, sbyte[] array, NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(ValidateNameArg(name));
			Contract.Requires<ArgumentNullException>(array != null);

				 if (IsReading) return ReadFixedArray(name, array, numBase);
			else if (IsWriting) WriteElements(name, array, numBase);

			return array.Length;
		}
		public int StreamFixedArray(TName name, ushort[] array, NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(ValidateNameArg(name));
			Contract.Requires<ArgumentNullException>(array != null);

				 if (IsReading) return ReadFixedArray(name, array, numBase);
			else if (IsWriting) WriteElements(name, array, numBase);

			return array.Length;
		}
		public int StreamFixedArray(TName name, short[] array, NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(ValidateNameArg(name));
			Contract.Requires<ArgumentNullException>(array != null);

				 if (IsReading) return ReadFixedArray(name, array, numBase);
			else if (IsWriting) WriteElements(name, array, numBase);

			return array.Length;
		}
		public int StreamFixedArray(TName name, uint[] array, NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(ValidateNameArg(name));
			Contract.Requires<ArgumentNullException>(array != null);

				 if (IsReading) return ReadFixedArray(name, array, numBase);
			else if (IsWriting) WriteElements(name, array, numBase);

			return array.Length;
		}
		public int StreamFixedArray(TName name, int[] array, NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(ValidateNameArg(name));
			Contract.Requires<ArgumentNullException>(array != null);

				 if (IsReading) return ReadFixedArray(name, array, numBase);
			else if (IsWriting) WriteElements(name, array, numBase);

			return array.Length;
		}
		public int StreamFixedArray(TName name, ulong[] array, NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(ValidateNameArg(name));
			Contract.Requires<ArgumentNullException>(array != null);

				 if (IsReading) return ReadFixedArray(name, array, numBase);
			else if (IsWriting) WriteElements(name, array, numBase);

			return array.Length;
		}
		public int StreamFixedArray(TName name, long[] array, NumeralBase numBase=kDefaultRadix)
		{
			Contract.Requires(ValidateNameArg(name));
			Contract.Requires<ArgumentNullException>(array != null);

				 if (IsReading) return ReadFixedArray(name, array, numBase);
			else if (IsWriting) WriteElements(name, array, numBase);

			return array.Length;
		}
		#endregion
	};
}