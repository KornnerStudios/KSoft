﻿using System;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.IO
{
	partial class TagElementTextStream<TDoc, TCursor>
	{
		#region ReadElement impl
		protected override void ReadElement(TCursor n, ref string value)
		{
			value = GetInnerText(n);
		}
		protected override void ReadElement(TCursor n, ref char value)
		{
			ParseString(GetInnerText(n), ref value, kThrowExcept);
		}
		protected override void ReadElement(TCursor n, ref bool value)
		{
			ParseString(GetInnerText(n), ref value, kThrowExcept);
		}
		protected override void ReadElement(TCursor n, ref float value)
		{
			ParseString(GetInnerText(n), ref value, kThrowExcept);
		}
		protected override void ReadElement(TCursor n, ref double value)
		{
			ParseString(GetInnerText(n), ref value, kThrowExcept);
		}

		protected override void ReadElement(TCursor n, ref byte value, NumeralBase fromBase)
		{
			Numbers.ParseString(GetInnerText(n), ref value, kThrowExcept, fromBase);
		}
		protected override void ReadElement(TCursor n, ref sbyte value, NumeralBase fromBase)
		{
			Numbers.ParseString(GetInnerText(n), ref value, kThrowExcept, fromBase);
		}
		protected override void ReadElement(TCursor n, ref ushort value, NumeralBase fromBase)
		{
			Numbers.ParseString(GetInnerText(n), ref value, kThrowExcept, fromBase);
		}
		protected override void ReadElement(TCursor n, ref short value, NumeralBase fromBase)
		{
			Numbers.ParseString(GetInnerText(n), ref value, kThrowExcept, fromBase);
		}
		protected override void ReadElement(TCursor n, ref uint value, NumeralBase fromBase)
		{
			Numbers.ParseString(GetInnerText(n), ref value, kThrowExcept, fromBase);
		}
		protected override void ReadElement(TCursor n, ref int value, NumeralBase fromBase)
		{
			Numbers.ParseString(GetInnerText(n), ref value, kThrowExcept, fromBase);
		}
		protected override void ReadElement(TCursor n, ref ulong value, NumeralBase fromBase)
		{
			Numbers.ParseString(GetInnerText(n), ref value, kThrowExcept, fromBase);
		}
		protected override void ReadElement(TCursor n, ref long value, NumeralBase fromBase)
		{
			Numbers.ParseString(GetInnerText(n), ref value, kThrowExcept, fromBase);
		}
		#endregion


		#region ReadAttribute
		public override void ReadAttribute(string name, ref string value)
		{
			value = ReadAttribute(name);
		}
		public override void ReadAttribute(string name, ref char value)
		{
			ParseString(ReadAttribute(name), ref value, kThrowExcept);
		}
		public override void ReadAttribute(string name, ref bool value)
		{
			ParseString(ReadAttribute(name), ref value, kThrowExcept);
		}
		public override void ReadAttribute(string name, ref float value)
		{
			ParseString(ReadAttribute(name), ref value, kThrowExcept);
		}
		public override void ReadAttribute(string name, ref double value)
		{
			ParseString(ReadAttribute(name), ref value, kThrowExcept);
		}

		public override void ReadAttribute(string name, ref byte value, NumeralBase fromBase=kDefaultRadix)
		{
			Numbers.ParseString(ReadAttribute(name), ref value, kThrowExcept, fromBase);
		}
		public override void ReadAttribute(string name, ref sbyte value, NumeralBase fromBase=kDefaultRadix)
		{
			Numbers.ParseString(ReadAttribute(name), ref value, kThrowExcept, fromBase);
		}
		public override void ReadAttribute(string name, ref ushort value, NumeralBase fromBase=kDefaultRadix)
		{
			Numbers.ParseString(ReadAttribute(name), ref value, kThrowExcept, fromBase);
		}
		public override void ReadAttribute(string name, ref short value, NumeralBase fromBase=kDefaultRadix)
		{
			Numbers.ParseString(ReadAttribute(name), ref value, kThrowExcept, fromBase);
		}
		public override void ReadAttribute(string name, ref uint value, NumeralBase fromBase=kDefaultRadix)
		{
			Numbers.ParseString(ReadAttribute(name), ref value, kThrowExcept, fromBase);
		}
		public override void ReadAttribute(string name, ref int value, NumeralBase fromBase=kDefaultRadix)
		{
			Numbers.ParseString(ReadAttribute(name), ref value, kThrowExcept, fromBase);
		}
		public override void ReadAttribute(string name, ref ulong value, NumeralBase fromBase=kDefaultRadix)
		{
			Numbers.ParseString(ReadAttribute(name), ref value, kThrowExcept, fromBase);
		}
		public override void ReadAttribute(string name, ref long value, NumeralBase fromBase=kDefaultRadix)
		{
			Numbers.ParseString(ReadAttribute(name), ref value, kThrowExcept, fromBase);
		}
		#endregion


		#region ReadElementOpt
		public override bool ReadElementOpt(string name, ref string value)
		{
			return (value = ReadElementOpt(name)) != null;
		}
		public override bool ReadElementOpt(string name, ref char value)
		{
			return ParseString(ReadElementOpt(name), ref value, kNoExcept);
		}
		public override bool ReadElementOpt(string name, ref bool value)
		{
			return ParseString(ReadElementOpt(name), ref value, kNoExcept);
		}
		public override bool ReadElementOpt(string name, ref float value)
		{
			return ParseString(ReadElementOpt(name), ref value, kNoExcept);
		}
		public override bool ReadElementOpt(string name, ref double value)
		{
			return ParseString(ReadElementOpt(name), ref value, kNoExcept);
		}

		public override bool ReadElementOpt(string name, ref byte value, NumeralBase fromBase=kDefaultRadix)
		{
			return Numbers.ParseString(ReadElementOpt(name), ref value, kNoExcept, fromBase);
		}
		public override bool ReadElementOpt(string name, ref sbyte value, NumeralBase fromBase=kDefaultRadix)
		{
			return Numbers.ParseString(ReadElementOpt(name), ref value, kNoExcept, fromBase);
		}
		public override bool ReadElementOpt(string name, ref ushort value, NumeralBase fromBase=kDefaultRadix)
		{
			return Numbers.ParseString(ReadElementOpt(name), ref value, kNoExcept, fromBase);
		}
		public override bool ReadElementOpt(string name, ref short value, NumeralBase fromBase=kDefaultRadix)
		{
			return Numbers.ParseString(ReadElementOpt(name), ref value, kNoExcept, fromBase);
		}
		public override bool ReadElementOpt(string name, ref uint value, NumeralBase fromBase=kDefaultRadix)
		{
			return Numbers.ParseString(ReadElementOpt(name), ref value, kNoExcept, fromBase);
		}
		public override bool ReadElementOpt(string name, ref int value, NumeralBase fromBase=kDefaultRadix)
		{
			return Numbers.ParseString(ReadElementOpt(name), ref value, kNoExcept, fromBase);
		}
		public override bool ReadElementOpt(string name, ref ulong value, NumeralBase fromBase=kDefaultRadix)
		{
			return Numbers.ParseString(ReadElementOpt(name), ref value, kNoExcept, fromBase);
		}
		public override bool ReadElementOpt(string name, ref long value, NumeralBase fromBase=kDefaultRadix)
		{
			return Numbers.ParseString(ReadElementOpt(name), ref value, kNoExcept, fromBase);
		}
		#endregion


		#region ReadAttributeOpt
		public override bool ReadAttributeOpt(string name, ref string value)
		{
			return (value = ReadAttributeOpt(name)) != null;
		}
		public override bool ReadAttributeOpt(string name, ref char value)
		{
			return ParseString(ReadAttributeOpt(name), ref value, kNoExcept);
		}
		public override bool ReadAttributeOpt(string name, ref bool value)
		{
			return ParseString(ReadAttributeOpt(name), ref value, kNoExcept);
		}
		public override bool ReadAttributeOpt(string name, ref float value)
		{
			return ParseString(ReadAttributeOpt(name), ref value, kNoExcept);
		}
		public override bool ReadAttributeOpt(string name, ref double value)
		{
			return ParseString(ReadAttributeOpt(name), ref value, kNoExcept);
		}

		public override bool ReadAttributeOpt(string name, ref byte value, NumeralBase fromBase=kDefaultRadix)
		{
			return Numbers.ParseString(ReadAttributeOpt(name), ref value, kNoExcept, fromBase);
		}
		public override bool ReadAttributeOpt(string name, ref sbyte value, NumeralBase fromBase=kDefaultRadix)
		{
			return Numbers.ParseString(ReadAttributeOpt(name), ref value, kNoExcept, fromBase);
		}
		public override bool ReadAttributeOpt(string name, ref ushort value, NumeralBase fromBase=kDefaultRadix)
		{
			return Numbers.ParseString(ReadAttributeOpt(name), ref value, kNoExcept, fromBase);
		}
		public override bool ReadAttributeOpt(string name, ref short value, NumeralBase fromBase=kDefaultRadix)
		{
			return Numbers.ParseString(ReadAttributeOpt(name), ref value, kNoExcept, fromBase);
		}
		public override bool ReadAttributeOpt(string name, ref uint value, NumeralBase fromBase=kDefaultRadix)
		{
			return Numbers.ParseString(ReadAttributeOpt(name), ref value, kNoExcept, fromBase);
		}
		public override bool ReadAttributeOpt(string name, ref int value, NumeralBase fromBase=kDefaultRadix)
		{
			return Numbers.ParseString(ReadAttributeOpt(name), ref value, kNoExcept, fromBase);
		}
		public override bool ReadAttributeOpt(string name, ref ulong value, NumeralBase fromBase=kDefaultRadix)
		{
			return Numbers.ParseString(ReadAttributeOpt(name), ref value, kNoExcept, fromBase);
		}
		public override bool ReadAttributeOpt(string name, ref long value, NumeralBase fromBase=kDefaultRadix)
		{
			return Numbers.ParseString(ReadAttributeOpt(name), ref value, kNoExcept, fromBase);
		}
		#endregion
	};
}