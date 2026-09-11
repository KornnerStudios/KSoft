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

		public override IEnumerable<ReservedMember> ReservedMembers(IReadOnlyList<PropertyModel> properties)
		{
			if (RequiresValueEqualityHelper(properties))
			{
				yield return new ReservedMember(
					GeneratorContracts.ValueEqualityMethodName,
					properties[0].Location);
			}
		}

		public override void WriteTypeMembers(SourceWriter writer, IReadOnlyList<PropertyModel> properties)
		{
			if (!RequiresValueEqualityHelper(properties)) return;

			WriteValueEqualityHelper(writer);
		}

		public override void WriteSetter(
			SourceWriter writer,
			PropertyModel model,
			AccessorDeclarationSyntax setter,
			string typeName,
			string propertyName,
			string storage)
		{
			string valueStorage = ValueStorage(model, storage);
			writer.WriteLine($"{GeneratedSourceUtilities.ModifiersText(setter.Modifiers)}set");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				WriteEqualityGuard(writer, model, typeName, valueStorage);
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
