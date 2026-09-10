using System.Collections.Generic;
using System.Linq;
using KSoft.SourceGeneration.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace KSoft.PropertyChanged.SourceGeneration;

public sealed partial class PropertyChangedGenerator
{
	private sealed class CaliburnMicroHostStrategy
		: PropertyChangedHostStrategy
	{
		public static readonly CaliburnMicroHostStrategy Instance = new();

		private CaliburnMicroHostStrategy()
		{
		}

		public static HostStrategyMatch Match(
			INamedTypeSymbol containingType,
			INamedTypeSymbol? caliburnPropertyChangedBase,
			out string? unsupportedReason)
		{
			unsupportedReason = null;
			if (caliburnPropertyChangedBase == null
				|| !InheritsFrom(containingType, caliburnPropertyChangedBase))
			{
				return HostStrategyMatch.NotApplicable;
			}

			if (HasSupportedContract(caliburnPropertyChangedBase))
			{
				return HostStrategyMatch.Supported;
			}

			unsupportedReason =
				"Caliburn.Micro.PropertyChangedBase must expose virtual IsNotifying and NotifyOfPropertyChange(string) members";
			return HostStrategyMatch.InvalidContract;
		}

		public override string? ReservedMemberName(IReadOnlyList<PropertyModel> properties) =>
			RequiresValueEqualityHelper(properties)
				? GeneratorContracts.CaliburnValueEqualityMethodName
				: null;

		public override void WriteTypeMembers(SourceWriter writer, IReadOnlyList<PropertyModel> properties)
		{
			if (!RequiresValueEqualityHelper(properties)) return;

			WriteGeneratedAttributes(writer);
			writer.WriteLine(
				$"private static bool {GeneratorContracts.CaliburnValueEqualityMethodName}<T>(ref T left, T right)");
			using (writer.EnterBlock())
			{
				writer.WriteLine("where T : struct, global::System.IEquatable<T>");
			}
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				writer.WriteLine("return left.Equals(right);");
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
			string valueStorage = model.BackingField == null
				? storage
				: $"this.{storage}";
			writer.WriteLine($"{GeneratedSourceUtilities.ModifiersText(setter.Modifiers)}set");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				if (!model.AlwaysNotify)
				{
					string equality = model.Equality == EqualityMode.EquatableValue
						? $"{GeneratorContracts.CaliburnValueEqualityMethodName}(ref {valueStorage}, value)"
						: $"global::System.Collections.Generic.EqualityComparer<{typeName}>.Default.Equals({valueStorage}, value)";
					writer.WriteLine($"if ({equality})");
					using (writer.EnterBlock(SourceWriterBlockType.Braces))
					{
						writer.WriteLine("return;");
					}
				}

				writer.WriteLine($"{valueStorage} = value;");
				writer.WriteLine(
					"global::Caliburn.Micro.PropertyChangedBase propertyChangedNotifier = this;");
				writer.WriteLine("if (propertyChangedNotifier.IsNotifying)");
				using (writer.EnterBlock(SourceWriterBlockType.Braces))
				{
					writer.WriteLine(
						$"propertyChangedNotifier.NotifyOfPropertyChange(nameof({propertyName}));");
				}
			}
		}

		private static bool RequiresValueEqualityHelper(IReadOnlyList<PropertyModel> properties) =>
			properties.Any(static property =>
				!property.AlwaysNotify
					&& property.Equality == EqualityMode.EquatableValue);

		private static bool InheritsFrom(INamedTypeSymbol type, INamedTypeSymbol expectedBase)
		{
			for (INamedTypeSymbol? baseType = type.BaseType; baseType != null; baseType = baseType.BaseType)
			{
				if (SymbolEqualityComparer.Default.Equals(baseType, expectedBase)) return true;
			}

			return false;
		}

		private static bool HasSupportedContract(INamedTypeSymbol caliburnPropertyChangedBase)
		{
			bool hasIsNotifying = caliburnPropertyChangedBase.GetMembers("IsNotifying")
				.OfType<IPropertySymbol>()
				.Any(static property =>
					!property.IsStatic
						&& property.DeclaredAccessibility == Accessibility.Public
						&& property.Type.SpecialType == SpecialType.System_Boolean
						&& property.GetMethod is { IsVirtual: true, DeclaredAccessibility: Accessibility.Public });
			if (!hasIsNotifying) return false;

			return caliburnPropertyChangedBase.GetMembers("NotifyOfPropertyChange")
				.OfType<IMethodSymbol>()
				.Any(static method =>
					!method.IsStatic
						&& method.IsVirtual
						&& method.MethodKind == MethodKind.Ordinary
						&& method.DeclaredAccessibility == Accessibility.Public
						&& method.ReturnsVoid
						&& method.Arity == 0
						&& method.Parameters.Length == 1
						&& method.Parameters[0].RefKind == RefKind.None
						&& method.Parameters[0].Type.SpecialType == SpecialType.System_String);
		}
	}
}
