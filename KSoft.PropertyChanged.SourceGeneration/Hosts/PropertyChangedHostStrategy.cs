using System.Collections.Generic;
using System.Linq;
using KSoft.SourceGeneration.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace KSoft.PropertyChanged.SourceGeneration;

public sealed partial class PropertyChangedGenerator
{
	private abstract class PropertyChangedHostStrategy
	{
		public virtual IEnumerable<ReservedMember> ReservedMembers(IReadOnlyList<PropertyModel> properties)
		{
			yield break;
		}

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

		protected static bool RequiresValueEqualityHelper(IReadOnlyList<PropertyModel> properties) =>
			properties.Any(static property =>
				!property.AlwaysNotify
					&& property.Equality == EqualityMode.EquatableValue);

		protected static void WriteValueEqualityHelper(SourceWriter writer)
		{
			WriteGeneratedAttributes(writer);
			writer.WriteLine(
				$"private static bool {GeneratorContracts.ValueEqualityMethodName}<T>(ref T left, T right)");
			using (writer.EnterBlock())
			{
				writer.WriteLine("where T : struct, global::System.IEquatable<T>");
			}
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				writer.WriteLine("return left.Equals(right);");
			}
		}

		protected static string ValueStorage(PropertyModel model, string storage) =>
			model.BackingField == null
				? storage
				: $"this.{storage}";

		protected static void WriteEqualityGuard(
			SourceWriter writer,
			PropertyModel model,
			string typeName,
			string valueStorage)
		{
			if (model.AlwaysNotify) return;

			string equality = model.Equality == EqualityMode.EquatableValue
				? $"{GeneratorContracts.ValueEqualityMethodName}(ref {valueStorage}, value)"
				: $"global::System.Collections.Generic.EqualityComparer<{typeName}>.Default.Equals({valueStorage}, value)";
			writer.WriteLine($"if ({equality})");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				writer.WriteLine("return;");
			}
			writer.WriteLine();
		}
	}

	private sealed class ReservedMember
	{
		public ReservedMember(string name, Location location)
		{
			Name = name;
			Location = location;
		}

		public string Name { get; }
		public Location Location { get; }
	}
}
