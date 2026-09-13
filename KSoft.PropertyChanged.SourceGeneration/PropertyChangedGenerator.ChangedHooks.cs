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

	private static bool TryReadChangedCallback(
		SourceProductionContext context,
		Candidate candidate,
		out string? changedCallback)
	{
		changedCallback = null;
		foreach (KeyValuePair<string, TypedConstant> argument in candidate.Attribute.NamedArguments)
		{
			if (!string.Equals(
				argument.Key,
				GeneratorContracts.ChangedCallbackPropertyName,
				StringComparison.Ordinal)) continue;

			changedCallback = argument.Value.Value as string;
			if (!string.IsNullOrWhiteSpace(changedCallback)) return true;

			ReportUnsupported(
				context,
				candidate.Property,
				candidate.Location,
				$"{GeneratorContracts.ChangedCallbackPropertyName} must name one existing parameterless callback method");
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
		PropertyModel[] callbackProperties = properties
			.Where(static property => property.ChangedCallback != null)
			.ToArray();
		if (hookProperties.Length == 0 && callbackProperties.Length == 0) return true;

		bool valid = true;
		if (hookProperties.Length != 0
			&& strategy is not BasicViewModelHostStrategy
			&& strategy is not CaliburnMicroHostStrategy)
		{
			foreach (PropertyModel property in hookProperties)
			{
				ReportUnsupported(
					context,
					property.Property,
					property.Location,
					$"ChangedHook {nameof(ChangedHookMode.Parameterless)} requires the resolved KSoft.ObjectModel.BasicViewModel or Caliburn.Micro.PropertyChangedBase provider; the selected provider is {ProviderName(strategy)}");
			}
			valid = false;
		}

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
			valid = false;
		}

		if (callbackProperties.Length != 0
			&& strategy is not CaliburnMicroHostStrategy)
		{
			foreach (PropertyModel property in callbackProperties)
			{
				ReportUnsupported(
					context,
					property.Property,
					property.Location,
					$"{GeneratorContracts.ChangedCallbackPropertyName} requires the resolved Caliburn.Micro.PropertyChangedBase provider; the selected provider is {ProviderName(strategy)}");
			}
			valid = false;
		}

		foreach (PropertyModel property in callbackProperties)
		{
			if (TryValidateChangedCallback(
				compilation,
				containingType,
				property.ChangedCallback!,
				out string? reason)) continue;

			ReportUnsupported(
				context,
				property.Property,
				property.Location,
				$"{GeneratorContracts.ChangedCallbackPropertyName} '{property.ChangedCallback}' {reason}");
			valid = false;
		}

		return valid;
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

	private static bool TryValidateChangedCallback(
		Compilation compilation,
		INamedTypeSymbol containingType,
		string callbackName,
		out string? reason)
	{
		var methods = new List<IMethodSymbol>();
		for (INamedTypeSymbol? current = containingType;
			current != null;
			current = current.BaseType)
		{
			methods.AddRange(current.GetMembers(callbackName)
				.OfType<IMethodSymbol>()
				.Where(method =>
					SymbolEqualityComparer.Default.Equals(current, containingType)
					|| compilation.IsSymbolAccessibleWithin(method, containingType)));
		}

		if (methods.Count != 1)
		{
			reason = "must resolve to exactly one accessible method";
			return false;
		}

		IMethodSymbol method = methods[0];
		if (method.MethodKind != MethodKind.Ordinary
			|| method.IsStatic
			|| method.IsAsync
			|| !method.ReturnsVoid
			|| method.Arity != 0
			|| method.Parameters.Length != 0
			|| method.PartialDefinitionPart != null
			|| method.PartialImplementationPart != null)
		{
			reason =
				"must name an instance, synchronous, non-generic, parameterless void method that is not partial";
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

	private static string? ChangedCallbackName(PropertyModel property) =>
		property.ChangedHook == ChangedHookMode.Parameterless
			? ParameterlessHookName(property)
			: property.ChangedCallback;

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
