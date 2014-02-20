using System;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.IO
{
	public abstract partial class TagElementTextStream<TDoc, TCursor> : TagElementStream<TDoc, TCursor, string>
		where TDoc : class
		where TCursor : class
	{
		/// <summary>Element's qualified name, or null if <see cref="Cursor"/> is null</summary>
		public abstract string CursorName { get; }

		[Contracts.Pure]
		public override bool ValidateNameArg(string name) { return !string.IsNullOrEmpty(name); }
	};
}