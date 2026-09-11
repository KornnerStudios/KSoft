using System.Collections.Generic;
using KSoft.SourceGeneration.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace KSoft.PropertyChanged.SourceGeneration;

public sealed partial class PropertyChangedGenerator
{
	private sealed class BasicViewModelHostStrategy
		: PropertyChangedHostStrategy
	{
		public static readonly BasicViewModelHostStrategy Instance = new();

		private BasicViewModelHostStrategy()
		{
		}

		public override IEnumerable<ReservedMember> ReservedMembers(IReadOnlyList<PropertyModel> properties)
		{
			yield return new ReservedMember(GeneratorContracts.CacheTypeName, properties[0].Location);
		}

		public override void WriteTypeMembers(SourceWriter writer, IReadOnlyList<PropertyModel> properties)
		{
			WriteGeneratedAttributes(writer);
			writer.WriteLine($"private static class {GeneratorContracts.CacheTypeName}");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				foreach (PropertyModel property in properties)
				{
					string propertyName = GeneratedSourceUtilities.EscapeIdentifier(property.Property.Name);
					writer.WriteLine(
						$"internal static readonly global::System.ComponentModel.PropertyChangedEventArgs {BasicViewModelCacheFieldName(property.Property.Name)} =");
					using (writer.EnterBlock())
					{
						writer.WriteLine($"new(nameof({propertyName}));");
					}
				}
			}
		}

		public override void WriteSetter(
			SourceWriter writer,
			PropertyModel model,
			AccessorDeclarationSyntax setter,
			string typeName,
			string propertyName,
			string storage)
		{
			string eventArgs =
				$"{GeneratorContracts.CacheTypeName}.{BasicViewModelCacheFieldName(model.Property.Name)}";
			string helperCall = model.AlwaysNotify
				? $"base.SetField<{typeName}>(ref {storage}, value, {eventArgs}, true)"
				: $"base.{HelperName(model.Equality)}<{typeName}>(ref {storage}, value, {eventArgs})";
			writer.WriteLine($"{GeneratedSourceUtilities.ModifiersText(setter.Modifiers)}set => {helperCall};");
		}
	}
}
