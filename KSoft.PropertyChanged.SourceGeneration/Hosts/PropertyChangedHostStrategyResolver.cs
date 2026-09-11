using System.Collections.Generic;
using KSoft.PropertyChanged.SourceGeneration.Diagnostics;
using Microsoft.CodeAnalysis;

namespace KSoft.PropertyChanged.SourceGeneration;

public sealed partial class PropertyChangedGenerator
{
	private static bool TryResolveHostStrategy(
		SourceProductionContext context,
		Compilation compilation,
		INamedTypeSymbol containingType,
		IReadOnlyList<PropertyModel> properties,
		INamedTypeSymbol? hostAttribute,
		INamedTypeSymbol? propertyChangedEventArgs,
		INamedTypeSymbol? basicViewModel,
		INamedTypeSymbol? caliburnPropertyChangedBase,
		out PropertyChangedHostStrategy? strategy)
	{
		string targetName = properties[0].Property.Name;
		Location location = properties[0].Location;
		HostStrategyMatch cachedEventArgsMatch = CachedEventArgsHostStrategy.Match(
			compilation,
			containingType,
			hostAttribute,
			propertyChangedEventArgs,
			out strategy,
			out string? unsupportedReason);
		switch (cachedEventArgsMatch)
		{
			case HostStrategyMatch.Supported:
				return true;

			case HostStrategyMatch.InvalidContract:
				context.ReportDiagnostic(Diagnostic.Create(
					DiagnosticDescriptors.UnsupportedTarget,
					location,
					targetName,
					unsupportedReason));
				return false;
		}

		HostStrategyMatch basicViewModelMatch = BasicViewModelHostStrategy.Match(
			compilation,
			containingType,
			basicViewModel,
			propertyChangedEventArgs,
			properties,
			out unsupportedReason);
		switch (basicViewModelMatch)
		{
			case HostStrategyMatch.Supported:
				strategy = BasicViewModelHostStrategy.Instance;
				return true;

			case HostStrategyMatch.InvalidContract:
				context.ReportDiagnostic(Diagnostic.Create(
					DiagnosticDescriptors.UnsupportedTarget,
					location,
					targetName,
					unsupportedReason));
				strategy = null;
				return false;
		}

		HostStrategyMatch caliburnMatch = CaliburnMicroHostStrategy.Match(
			containingType,
			caliburnPropertyChangedBase,
			out unsupportedReason);
		switch (caliburnMatch)
		{
			case HostStrategyMatch.Supported:
				strategy = CaliburnMicroHostStrategy.Instance;
				return true;

			case HostStrategyMatch.InvalidContract:
				context.ReportDiagnostic(Diagnostic.Create(
					DiagnosticDescriptors.UnsupportedTarget,
					location,
					targetName,
					unsupportedReason));
				strategy = null;
				return false;

			default:
				context.ReportDiagnostic(Diagnostic.Create(
					DiagnosticDescriptors.InvalidPropertyChangedHost,
					location,
					containingType.ToDisplayString()));
				strategy = null;
				return false;
		}
	}

	private enum HostStrategyMatch
	{
		NotApplicable,
		Supported,
		InvalidContract,
	}
}
