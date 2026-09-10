using KSoft.PropertyChanged.SourceGeneration.Diagnostics;
using Microsoft.CodeAnalysis;

namespace KSoft.PropertyChanged.SourceGeneration;

public sealed partial class PropertyChangedGenerator
{
	private static bool TryResolveHostStrategy(
		SourceProductionContext context,
		INamedTypeSymbol containingType,
		string targetName,
		Location location,
		INamedTypeSymbol? basicViewModel,
		INamedTypeSymbol? caliburnPropertyChangedBase,
		out PropertyChangedHostStrategy? strategy)
	{
		if (basicViewModel != null
			&& SymbolEqualityComparer.Default.Equals(containingType.BaseType, basicViewModel))
		{
			strategy = BasicViewModelHostStrategy.Instance;
			return true;
		}

		HostStrategyMatch caliburnMatch = CaliburnMicroHostStrategy.Match(
			containingType,
			caliburnPropertyChangedBase,
			out string? unsupportedReason);
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
