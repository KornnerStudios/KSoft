using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using KSoft.PropertyChanged.SourceGeneration.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace KSoft.PropertyChanged.SourceGeneration;

public sealed partial class PropertyChangedGenerator
{
	private static bool TryReadChangedHook(
		SourceProductionContext context,
		Candidate candidate,
		out ChangedHookMode changedHook)
	{
		changedHook = ChangedHookMode.None;
		foreach (KeyValuePair<string, TypedConstant> argument in candidate.Attribute.NamedArguments)
		{
			if (!string.Equals(
				argument.Key,
				GeneratorContracts.ChangedHookPropertyName,
				StringComparison.Ordinal)) continue;

			if (argument.Value.Value is int value
				&& value >= (int)ChangedHookMode.None
				&& value <= (int)ChangedHookMode.Parameterless)
			{
				changedHook = (ChangedHookMode)value;
				return true;
			}

			ReportUnsupported(
				context,
				candidate.Property,
				candidate.Location,
				$"ChangedHook value '{argument.Value.Value}' is undefined; use {nameof(ChangedHookMode.None)} or {nameof(ChangedHookMode.Parameterless)}");
			return false;
		}

		return true;
	}

	private static bool TryValidateChangedHooks(
		SourceProductionContext context,
		Compilation compilation,
		INamedTypeSymbol containingType,
		IReadOnlyList<PropertyModel> properties,
		PropertyChangedHostStrategy strategy)
	{
		PropertyModel[] hookProperties = properties
			.Where(static property => property.ChangedHook == ChangedHookMode.Parameterless)
			.ToArray();
		if (hookProperties.Length == 0) return true;

		if (strategy is not BasicViewModelHostStrategy)
		{
			foreach (PropertyModel property in hookProperties)
			{
				ReportUnsupported(
					context,
					property.Property,
					property.Location,
					$"ChangedHook {nameof(ChangedHookMode.Parameterless)} requires the resolved KSoft.ObjectModel.BasicViewModel provider; the selected provider is {ProviderName(strategy)}");
			}
			return false;
		}

		bool hasCollision = false;
		foreach (PropertyModel property in hookProperties)
		{
			string hookName = ParameterlessHookName(property);
			if (TryValidateParameterlessHook(
				compilation,
				containingType,
				hookName,
				out string? reason)) continue;

			context.ReportDiagnostic(Diagnostic.Create(
				DiagnosticDescriptors.GeneratedMemberCollision,
				property.Location,
				containingType.ToDisplayString(),
				hookName,
				$"property '{property.Property.Name}' requests ChangedHook {nameof(ChangedHookMode.Parameterless)}, but {reason}"));
			hasCollision = true;
		}

		return !hasCollision;
	}

	private static bool TryValidateParameterlessHook(
		Compilation compilation,
		INamedTypeSymbol containingType,
		string hookName,
		out string? reason)
	{
		ImmutableArray<ISymbol> localMembers = containingType.GetMembers(hookName);
		if (!localMembers.IsEmpty
			&& (localMembers.Length != 1
				|| !IsParameterlessHookImplementation(localMembers[0])))
		{
			reason =
				"the existing member must be removed or replaced with one implicit-private, parameterless partial implementation";
			return false;
		}

		for (INamedTypeSymbol? current = containingType.BaseType;
			current != null;
			current = current.BaseType)
		{
			if (!current.GetMembers(hookName)
				.Any(member => compilation.IsSymbolAccessibleWithin(member, containingType))) continue;

			reason =
				$"accessible inherited member '{hookName}' prevents the generated hook declaration; rename or remove that member";
			return false;
		}

		reason = null;
		return true;
	}

	private static bool IsParameterlessHookImplementation(ISymbol member)
	{
		if (member is not IMethodSymbol method
			|| method.MethodKind != MethodKind.Ordinary
			|| method.IsStatic
			|| method.IsAsync
			|| !method.ReturnsVoid
			|| method.Arity != 0
			|| method.Parameters.Length != 0
			|| method.DeclaringSyntaxReferences.Length != 1) return false;

		if (method.DeclaringSyntaxReferences[0].GetSyntax() is not MethodDeclarationSyntax syntax
			|| syntax.Body == null && syntax.ExpressionBody == null
			|| syntax.Modifiers.Count != 1
			|| !syntax.Modifiers[0].IsKind(SyntaxKind.PartialKeyword)) return false;

		return true;
	}

	private static string ParameterlessHookName(PropertyModel property) =>
		"On" + property.Property.Name + "Changed";

	private static string ProviderName(PropertyChangedHostStrategy strategy) =>
		strategy switch
		{
			CachedEventArgsHostStrategy => "CachedEventArgs",
			CaliburnMicroHostStrategy => "Caliburn.Micro.PropertyChangedBase",
			_ => strategy.GetType().Name,
		};

	private enum ChangedHookMode
	{
		None,
		Parameterless,
	}
}
