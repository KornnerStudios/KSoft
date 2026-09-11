using System.Collections.Generic;
using System.Linq;
using KSoft.SourceGeneration.Text;
using Microsoft.CodeAnalysis;
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

		public static HostStrategyMatch Match(
			Compilation compilation,
			INamedTypeSymbol containingType,
			INamedTypeSymbol? basicViewModel,
			INamedTypeSymbol? propertyChangedEventArgs,
			IReadOnlyList<PropertyModel> properties,
			out string? unsupportedReason)
		{
			unsupportedReason = null;
			if (basicViewModel == null
				|| !TryFindBaseType(containingType, basicViewModel, out INamedTypeSymbol? matchedBase))
			{
				return HostStrategyMatch.NotApplicable;
			}

			INamedTypeSymbol? equatableType = compilation.GetTypeByMetadataName(
				GeneratorContracts.EquatableMetadataName);
			INamedTypeSymbol enumType = compilation.GetSpecialType(SpecialType.System_Enum);
			foreach (PropertyModel property in properties)
			{
				string helperName = property.AlwaysNotify
					? "SetField"
					: HelperName(property.Equality);
				int parameterCount = property.AlwaysNotify ? 4 : 3;
				if (propertyChangedEventArgs == null
					|| !HasSupportedHelper(
						compilation,
						containingType,
						matchedBase!,
						propertyChangedEventArgs,
						helperName,
						parameterCount,
						equatableType,
						enumType))
				{
					unsupportedReason =
						$"KSoft.ObjectModel.BasicViewModel must expose an accessible protected bool {helperName}<T> helper with the generated call signature and {HelperConstraintDescription(helperName)} required by property '{property.Property.Name}'";
					return HostStrategyMatch.InvalidContract;
				}

				for (INamedTypeSymbol? current = containingType.BaseType;
					current != null
						&& !SymbolEqualityComparer.Default.Equals(current, matchedBase);
					current = current.BaseType)
				{
					if (!current.GetMembers(helperName)
						.Any(member => compilation.IsSymbolAccessibleWithin(member, containingType)))
					{
						continue;
					}

					unsupportedReason =
						$"intermediate base '{current.ToDisplayString()}' declares accessible member '{helperName}' and changes binding of the generated base.{helperName}<T>(...) call";
					return HostStrategyMatch.InvalidContract;
				}
			}

			PropertyModel? dependentProperty = properties.FirstOrDefault(
				static property => !property.DependentProperties.IsDefaultOrEmpty);
			if (dependentProperty != null
				&& !TryValidateOnPropertyChanged(
					compilation,
					containingType,
					matchedBase!,
					out unsupportedReason))
			{
				unsupportedReason +=
					$" required by dependent notifications on property '{dependentProperty.Property.Name}'";
				return HostStrategyMatch.InvalidContract;
			}

			return HostStrategyMatch.Supported;
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
			if (model.DependentProperties.IsDefaultOrEmpty)
			{
				writer.WriteLine($"{GeneratedSourceUtilities.ModifiersText(setter.Modifiers)}set => {helperCall};");
				return;
			}

			writer.WriteLine($"{GeneratedSourceUtilities.ModifiersText(setter.Modifiers)}set");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				writer.WriteLine($"if ({helperCall})");
				using (writer.EnterBlock(SourceWriterBlockType.Braces))
				{
					foreach (string dependentProperty in model.DependentProperties)
					{
						writer.WriteLine(
							$"OnPropertyChanged({DependentPropertyName(dependentProperty)});");
					}
				}
			}
		}

		private static bool TryFindBaseType(
			INamedTypeSymbol containingType,
			INamedTypeSymbol expectedBase,
			out INamedTypeSymbol? matchedBase)
		{
			for (INamedTypeSymbol? current = containingType.BaseType;
				current != null;
				current = current.BaseType)
			{
				if (!SymbolEqualityComparer.Default.Equals(current, expectedBase)) continue;

				matchedBase = current;
				return true;
			}

			matchedBase = null;
			return false;
		}

		private static bool HasSupportedHelper(
			Compilation compilation,
			INamedTypeSymbol containingType,
			INamedTypeSymbol basicViewModel,
			INamedTypeSymbol propertyChangedEventArgs,
			string helperName,
			int parameterCount,
			INamedTypeSymbol? equatableType,
			INamedTypeSymbol? enumType) =>
			basicViewModel.GetMembers(helperName)
				.OfType<IMethodSymbol>()
				.Any(method =>
					IsSupportedHelper(
						method,
						propertyChangedEventArgs,
						helperName,
						parameterCount,
						equatableType,
						enumType)
						&& compilation.IsSymbolAccessibleWithin(method, containingType));

		private static bool IsSupportedHelper(
			IMethodSymbol method,
			INamedTypeSymbol propertyChangedEventArgs,
			string helperName,
			int parameterCount,
			INamedTypeSymbol? equatableType,
			INamedTypeSymbol? enumType)
		{
			if (method.MethodKind != MethodKind.Ordinary
				|| method.IsStatic
				|| method.IsAbstract
				|| method.ReturnType.SpecialType != SpecialType.System_Boolean
				|| method.Arity != 1
				|| method.Parameters.Length != parameterCount)
			{
				return false;
			}

			ITypeParameterSymbol typeParameter = method.TypeParameters[0];
			if (method.Parameters[0].RefKind != RefKind.Ref
				|| !SymbolEqualityComparer.Default.Equals(method.Parameters[0].Type, typeParameter)
				|| method.Parameters[1].RefKind != RefKind.None
				|| !SymbolEqualityComparer.Default.Equals(method.Parameters[1].Type, typeParameter)
				|| method.Parameters[2].RefKind != RefKind.None
				|| !SymbolEqualityComparer.Default.Equals(
					method.Parameters[2].Type,
					propertyChangedEventArgs))
			{
				return false;
			}

			bool hasSupportedParameters = parameterCount == 3
				|| method.Parameters[3].RefKind == RefKind.None
					&& method.Parameters[3].Type.SpecialType == SpecialType.System_Boolean;
			return hasSupportedParameters
				&& HasExactHelperConstraints(
					method.TypeParameters[0],
					helperName,
					equatableType,
					enumType);
		}

		private static bool HasExactHelperConstraints(
			ITypeParameterSymbol typeParameter,
			string helperName,
			INamedTypeSymbol? equatableType,
			INamedTypeSymbol? enumType)
		{
			if (typeParameter.HasConstructorConstraint
				|| typeParameter.HasNotNullConstraint
				|| typeParameter.HasUnmanagedTypeConstraint
				|| typeParameter.AllowsRefLikeType)
			{
				return false;
			}

			switch (helperName)
			{
				case "SetField":
					return !typeParameter.HasReferenceTypeConstraint
						&& !typeParameter.HasValueTypeConstraint
						&& typeParameter.ConstraintTypes.IsEmpty;

				case "SetFieldVal":
					return equatableType != null
						&& !typeParameter.HasReferenceTypeConstraint
						&& typeParameter.HasValueTypeConstraint
						&& typeParameter.ConstraintTypes.Length == 1
						&& typeParameter.ConstraintTypes[0] is INamedTypeSymbol equatableConstraint
						&& SymbolEqualityComparer.Default.Equals(
							equatableConstraint.OriginalDefinition,
							equatableType)
						&& SymbolEqualityComparer.Default.Equals(
							equatableConstraint.TypeArguments[0],
							typeParameter);

				case "SetFieldEnum":
					return enumType != null
						&& !typeParameter.HasReferenceTypeConstraint
						&& typeParameter.HasValueTypeConstraint
						&& typeParameter.ConstraintTypes.Length == 1
						&& SymbolEqualityComparer.Default.Equals(
							typeParameter.ConstraintTypes[0],
							enumType);

				default:
					return false;
			}
		}

		private static string HelperConstraintDescription(string helperName) =>
			helperName switch
			{
				"SetField" => "no generic constraints",
				"SetFieldVal" => "exact constraints 'where T : struct, IEquatable<T>'",
				"SetFieldEnum" => "exact constraints 'where T : struct, System.Enum'",
				_ => "the exact required generic constraints",
			};

		private static bool TryValidateOnPropertyChanged(
			Compilation compilation,
			INamedTypeSymbol containingType,
			INamedTypeSymbol basicViewModel,
			out string? unsupportedReason)
		{
			IMethodSymbol[] baseMethods = basicViewModel.GetMembers("OnPropertyChanged")
				.OfType<IMethodSymbol>()
				.Where(method =>
					IsSupportedOnPropertyChanged(method)
						&& compilation.IsSymbolAccessibleWithin(method, containingType))
				.ToArray();
			if (baseMethods.Length != 1)
			{
				unsupportedReason =
					"KSoft.ObjectModel.BasicViewModel must expose exactly one accessible protected virtual ordinary void OnPropertyChanged(string) method";
				return false;
			}

			IMethodSymbol baseMethod = baseMethods[0];
			for (INamedTypeSymbol? current = containingType;
				current != null
					&& !SymbolEqualityComparer.Default.Equals(current, basicViewModel);
				current = current.BaseType)
			{
				ISymbol[] accessibleMembers = current.GetMembers("OnPropertyChanged")
					.Where(member => compilation.IsSymbolAccessibleWithin(member, containingType))
					.ToArray();
				if (accessibleMembers.Length == 0) continue;

				if (accessibleMembers.OfType<IMethodSymbol>().Any(
					method =>
						IsSupportedOnPropertyChanged(method)
							&& Overrides(method, baseMethod)))
				{
					unsupportedReason = null;
					return true;
				}

				unsupportedReason =
					$"type '{current.ToDisplayString()}' declares accessible member 'OnPropertyChanged' that changes unqualified dependent-notification lookup; it must be a genuine override of KSoft.ObjectModel.BasicViewModel.OnPropertyChanged(string)";
				return false;
			}

			unsupportedReason = null;
			return true;
		}

		private static bool IsSupportedOnPropertyChanged(IMethodSymbol method) =>
			method.MethodKind == MethodKind.Ordinary
				&& !method.IsStatic
				&& (method.IsVirtual || method.IsOverride)
				&& method.DeclaredAccessibility == Accessibility.Protected
				&& method.ReturnsVoid
				&& method.Arity == 0
				&& method.Parameters.Length == 1
				&& method.Parameters[0].RefKind == RefKind.None
				&& method.Parameters[0].Type.SpecialType == SpecialType.System_String;

		private static bool Overrides(IMethodSymbol method, IMethodSymbol expectedBase)
		{
			for (IMethodSymbol? overridden = method.OverriddenMethod;
				overridden != null;
				overridden = overridden.OverriddenMethod)
			{
				if (SymbolEqualityComparer.Default.Equals(overridden, expectedBase)) return true;
			}

			return false;
		}
	}
}
