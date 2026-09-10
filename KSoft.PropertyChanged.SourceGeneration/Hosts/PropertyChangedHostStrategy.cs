using System.Collections.Generic;
using KSoft.SourceGeneration.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace KSoft.PropertyChanged.SourceGeneration;

public sealed partial class PropertyChangedGenerator
{
	private abstract class PropertyChangedHostStrategy
	{
		public virtual string? ReservedMemberName(IReadOnlyList<PropertyModel> properties) => null;

		public virtual void WriteTypeMembers(SourceWriter writer, IReadOnlyList<PropertyModel> properties)
		{
		}

		public abstract void WriteSetter(
			SourceWriter writer,
			PropertyModel model,
			AccessorDeclarationSyntax setter,
			string typeName,
			string propertyName,
			string storage);
	}
}
