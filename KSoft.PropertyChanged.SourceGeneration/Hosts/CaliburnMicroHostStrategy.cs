using System.Collections.Generic;
using System.Linq;
using KSoft.SourceGeneration.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace KSoft.PropertyChanged.SourceGeneration;

public sealed partial class PropertyChangedGenerator
{
	/// <summary>Emits setters that retain Caliburn's virtual string-based notification pipeline.</summary>
	/// <remarks>
	/// <para>
	/// This provider removes handwritten setter boilerplate, not framework notification allocations.
	/// Caliburn.Micro 4.0.210 creates <c>PropertyChangedEventArgs</c> for delivered notifications and uses a
	/// capturing closure/delegate for UI dispatch. See that version's <c>PropertyChangedBase.NotifyOfPropertyChange</c>.
	/// </para>
	/// <para>
	/// Caliburn 5 and 6 have not been evaluated here for allocation changes; do not extrapolate the 4.0.210
	/// findings to those versions. Evaluate compatible upstream APIs before considering a maintained fork.
	/// </para>
	/// <para>
	/// Any allocation-free replacement must preserve subscriber gating, <c>IsNotifying</c>, UI dispatch, and
	/// virtual interception, including existing overrides. Do not bypass that pipeline with direct
	/// <c>OnPropertyChanged</c> calls or fork Gemini solely for this optimization.
	/// </para>
	/// </remarks>
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
					foreach (string dependentProperty in model.DependentProperties)
					{
						writer.WriteLine(
							$"propertyChangedNotifier.NotifyOfPropertyChange({DependentPropertyName(dependentProperty)});");
					}
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
