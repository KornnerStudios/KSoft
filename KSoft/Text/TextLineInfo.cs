﻿using System;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Text
{
	[Contracts.ContractClass(typeof(ITextLineInfoContract))]
	public interface ITextLineInfo
	{
		bool HasLineInfo { get; }

		int LineNumber { get; }
		int LinePosition { get; }
	};
	[Contracts.ContractClassFor(typeof(ITextLineInfo))]
	abstract class ITextLineInfoContract : ITextLineInfo
	{
		public bool HasLineInfo { get {
			throw new NotImplementedException();
		} }

		public int LineNumber { get {
			Contract.Ensures(Contract.Result<int>() >= 0);

			throw new NotImplementedException();
		} }
		public int LinePosition { get {
			Contract.Ensures(Contract.Result<int>() >= 0);

			throw new NotImplementedException();
		} }
	};

	public struct TextLineInfo : ITextLineInfo, IComparable<ITextLineInfo>, IEquatable<ITextLineInfo>
	{
		public static readonly TextLineInfo Empty = new TextLineInfo();

		int mLineNumber, mLinePosition;

		public bool HasLineInfo	{ get { return mLineNumber > 0; } }

		public int LineNumber	{ get { return mLineNumber; } }
		public int LinePosition	{ get { return mLinePosition; } }

		public TextLineInfo(int lineNumber, int linePosition)
		{
			Contract.Requires(lineNumber > 0);
			Contract.Requires(linePosition > 0);

			mLineNumber = lineNumber;
			mLinePosition = linePosition;
		}

		public bool IsEmpty { get {
			return LineNumber == 0 && LinePosition == 0;
		} }

		public int CompareTo(ITextLineInfo other)
		{
			if (LineNumber == other.LineNumber)
				return LinePosition - other.LinePosition;
			else
				return LineNumber - other.LineNumber;
		}

		public bool Equals(ITextLineInfo other)
		{
			return LineNumber == other.LineNumber && 
				LinePosition == other.LinePosition;
		}

		public override bool Equals(object obj)
		{
			if(obj != null && obj is ITextLineInfo)
			{
				var other = (ITextLineInfo)obj;

				return Equals(other);
			}

			return false;
		}

		public override int GetHashCode()
		{
			return LineNumber.GetHashCode() ^ LinePosition.GetHashCode();
		}
	};
}