using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using KSoft.PropertyChanged.SourceGeneration.Diagnostics;
using KSoft.SourceGeneration.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace KSoft.PropertyChanged.SourceGeneration;

internal static class PropertyChangedEventArgsCacheGeneration
{
	public static void Initialize(IncrementalGeneratorInitializationContext context)
	{
		IncrementalValuesProvider<Candidate> candidates = context.SyntaxProvider.ForAttributeWithMetadataName(
			GeneratorContracts.EventArgsAttributeMetadataName,
			static (node, _) => node is PropertyDeclarationSyntax,
			static (attributeContext, _) =>
			{
				var syntax = (PropertyDeclarationSyntax)attributeContext.TargetNode;
				// Explicit-interface attributes report a qualified target name, so retain the concrete host and syntax identifier.
				var containingType = (INamedTypeSymbol)attributeContext.SemanticModel.GetDeclaredSymbol(
					(ClassDeclarationSyntax)syntax.Parent!)!;
				INamedTypeSymbol? explicitInterfaceType = syntax.ExplicitInterfaceSpecifier == null
					? null
					: attributeContext.SemanticModel.GetTypeInfo(
						syntax.ExplicitInterfaceSpecifier.Name).Type as INamedTypeSymbol;
				return new Candidate(
					(IPropertySymbol)attributeContext.TargetSymbol,
					syntax,
					containingType,
					explicitInterfaceType);
			});

		context.RegisterSourceOutput(
			// Cache candidates must be grouped before emission because ordinary and explicit members can share one field name.
			candidates.Collect(),
			static (sourceContext, candidates) => Execute(sourceContext, candidates));
	}

	private static void Execute(
		SourceProductionContext context,
		ImmutableArray<Candidate> candidates)
	{
		Dictionary<INamedTypeSymbol, Dictionary<string, List<CacheModel>>> candidatesByType =
			GroupCandidatesByCacheField(context, candidates);
		EmitCacheSources(context, candidatesByType);
	}

	private static Dictionary<INamedTypeSymbol, Dictionary<string, List<CacheModel>>>
		GroupCandidatesByCacheField(
			SourceProductionContext context,
			ImmutableArray<Candidate> candidates)
	{
		var candidatesByType = new Dictionary<INamedTypeSymbol, Dictionary<string, List<CacheModel>>>(
			SymbolEqualityComparer.Default);
		foreach (Candidate candidate in candidates)
		{
			if (!TryCreateModel(context, candidate, out CacheModel? model)) continue;

			CacheModel validModel = model!;
			if (!candidatesByType.TryGetValue(validModel.Type, out Dictionary<string, List<CacheModel>>? candidatesByField))
			{
				candidatesByField = new Dictionary<string, List<CacheModel>>(System.StringComparer.Ordinal);
				candidatesByType.Add(validModel.Type, candidatesByField);
			}
			if (!candidatesByField.TryGetValue(validModel.FieldName, out List<CacheModel>? fieldCandidates))
			{
				fieldCandidates = new List<CacheModel>();
				candidatesByField.Add(validModel.FieldName, fieldCandidates);
			}

			fieldCandidates.Add(validModel);
		}

		return candidatesByType;
	}

	private static void EmitCacheSources(
		SourceProductionContext context,
		Dictionary<INamedTypeSymbol, Dictionary<string, List<CacheModel>>> candidatesByType)
	{
		foreach (KeyValuePair<INamedTypeSymbol, Dictionary<string, List<CacheModel>>> typeEntry
			in candidatesByType.OrderBy(static entry =>
				entry.Key.ToDisplayString(GeneratedSourceUtilities.TypeDisplayFormat),
				System.StringComparer.Ordinal))
		{
			foreach (KeyValuePair<string, List<CacheModel>> fieldEntry in typeEntry.Value.OrderBy(
				static entry => entry.Key,
				System.StringComparer.Ordinal))
			{
				List<CacheModel> fieldCandidates = fieldEntry.Value;
				EmitCacheSource(context, fieldCandidates);
			}
		}
	}

	private static void EmitCacheSource(
		SourceProductionContext context,
		List<CacheModel> fieldCandidates)
	{
		fieldCandidates.Sort(CompareCacheModels);
		if (fieldCandidates.Count != 1)
		{
			ReportDuplicateCacheField(context, fieldCandidates);
			return;
		}

		CacheModel candidate = fieldCandidates[0];
		context.AddSource(
			candidate.HintName,
			SourceText.From(BuildSource(candidate), Encoding.UTF8));
	}

	private static int CompareCacheModels(CacheModel left, CacheModel right)
	{
		int comparison = string.CompareOrdinal(left.HintName, right.HintName);
		if (comparison != 0) return comparison;

		comparison = string.CompareOrdinal(
			left.Location.SourceTree?.FilePath,
			right.Location.SourceTree?.FilePath);
		return comparison != 0
			? comparison
			: left.Location.SourceSpan.Start.CompareTo(right.Location.SourceSpan.Start);
	}

	private static void ReportDuplicateCacheField(
		SourceProductionContext context,
		IEnumerable<CacheModel> fieldCandidates)
	{
		// Each attribute receives a direct diagnostic instead of silently selecting one generated field.
		foreach (CacheModel collisionCandidate in fieldCandidates)
		{
			context.ReportDiagnostic(Diagnostic.Create(
				DiagnosticDescriptors.GeneratedMemberCollision,
				collisionCandidate.Location,
				collisionCandidate.Type.ToDisplayString(),
				collisionCandidate.FieldName,
				"another generated cache uses this field name"));
		}
	}

	private static bool TryCreateModel(
		SourceProductionContext context,
		Candidate candidate,
		out CacheModel? result)
	{
		result = null;
		IPropertySymbol property = candidate.Property;
		INamedTypeSymbol type = candidate.ContainingType;
		Location location = candidate.Syntax.GetLocation();
		if (property.GetAttributes().Any(static attribute =>
			string.Equals(
				attribute.AttributeClass?.ToDisplayString(),
				GeneratorContracts.AttributeMetadataName,
				System.StringComparison.Ordinal)))
		{
			context.ReportDiagnostic(Diagnostic.Create(
				DiagnosticDescriptors.ConflictingPropertyAttributes,
				location,
				property.Name));
			return false;
		}

		bool isExplicitInterfaceProperty = candidate.ExplicitInterfaceType != null;
		IPropertySymbol? explicitInterfaceProperty = null;
		if (isExplicitInterfaceProperty
			&& !TryGetExplicitInterfaceProperty(candidate, out explicitInterfaceProperty))
		{
			context.ReportDiagnostic(Diagnostic.Create(
				DiagnosticDescriptors.UnsupportedTarget,
				location,
				candidate.PropertyName,
				"cache-only generation requires explicit-interface properties to implement exactly one interface property"));
			return false;
		}
		if (type.ContainingType != null
			|| type.Arity != 0
			|| type.TypeKind != TypeKind.Class
			|| candidate.Syntax.Parent is not ClassDeclarationSyntax typeDeclaration
			|| !typeDeclaration.Modifiers.Any(static modifier => modifier.IsKind(SyntaxKind.PartialKeyword))
			|| typeDeclaration.Modifiers.Any(static modifier => modifier.IsKind(SyntaxKind.FileKeyword))
			|| property.IsStatic
			|| property.IsIndexer
			|| candidate.Syntax.ExplicitInterfaceSpecifier != null
				&& !isExplicitInterfaceProperty)
		{
			context.ReportDiagnostic(Diagnostic.Create(
				DiagnosticDescriptors.UnsupportedTarget,
				location,
				candidate.PropertyName,
				"cache-only generation requires an instance property in a top-level, non-generic partial class"));
			return false;
		}

		string fieldName = GeneratedSourceUtilities.PropertyChangedEventArgsFieldName(candidate.PropertyName);
		if (type.GetMembers(fieldName).Length != 0)
		{
			context.ReportDiagnostic(Diagnostic.Create(
				DiagnosticDescriptors.GeneratedMemberCollision,
				location,
				type.ToDisplayString(),
				fieldName,
				"an existing member already uses the generated cache field name"));
			return false;
		}

		string hintName = GeneratedSourceUtilities.CreateHintName(
			type,
			GeneratedSourceUtilities.TypeDisplayFormat,
			$"EventArgs.{candidate.PropertyName}");
		result = new CacheModel(
			type,
			typeDeclaration,
			fieldName,
			PropertyNameExpression(candidate.PropertyName, explicitInterfaceProperty),
			hintName,
			location);
		return true;
	}

	private static bool TryGetExplicitInterfaceProperty(
		Candidate candidate,
		out IPropertySymbol? result)
	{
		result = null;
		INamedTypeSymbol interfaceType = candidate.ExplicitInterfaceType!;
		// A qualified explicit implementation may name a property inherited from another interface.
		IPropertySymbol[] properties = interfaceType.AllInterfaces
			.Append(interfaceType)
			.SelectMany(interfaceCandidate => interfaceCandidate.GetMembers(candidate.PropertyName))
			.OfType<IPropertySymbol>()
			.ToArray();
		if (properties.Length != 1) return false;

		result = properties[0];
		return true;
	}

	private static string PropertyNameExpression(
		string propertyName,
		IPropertySymbol? explicitInterfaceProperty)
	{
		if (explicitInterfaceProperty != null)
		{
			// The concrete class has no unqualified member for an explicit implementation.
			string interfaceType = explicitInterfaceProperty.ContainingType.ToDisplayString(
				GeneratedSourceUtilities.TypeDisplayFormat);
			string interfacePropertyName = GeneratedSourceUtilities.EscapeIdentifier(explicitInterfaceProperty.Name);
			return $"nameof({interfaceType}.{interfacePropertyName})";
		}

		return $"nameof({GeneratedSourceUtilities.EscapeIdentifier(propertyName)})";
	}

	private static string BuildSource(CacheModel model)
	{
		var writer = new SourceWriter();
		writer.WriteLine("// <auto-generated/>");
		writer.WriteLine();

		string namespaceName = GeneratedSourceUtilities.NamespaceName(model.Type.ContainingNamespace);
		if (namespaceName.Length != 0)
		{
			writer.WriteFileScopedNamespace(namespaceName);
			writer.WriteLine();
		}
		writer.WriteLine(
			$"{GeneratedSourceUtilities.ModifiersText(model.TypeDeclaration.Modifiers)}class {GeneratedSourceUtilities.EscapeIdentifier(model.Type.Name)}");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			writer.WriteLine(
				$"static readonly global::System.ComponentModel.PropertyChangedEventArgs {model.FieldName} =");
			using (writer.EnterBlock())
			{
				writer.WriteLine($"new({model.PropertyNameExpression});");
			}
		}

		return writer.ToString();
	}

	private sealed class Candidate
	{
		public Candidate(
			IPropertySymbol property,
			PropertyDeclarationSyntax syntax,
			INamedTypeSymbol containingType,
			INamedTypeSymbol? explicitInterfaceType)
		{
			Property = property;
			Syntax = syntax;
			ContainingType = containingType;
			ExplicitInterfaceType = explicitInterfaceType;
		}

		public IPropertySymbol Property { get; }
		public PropertyDeclarationSyntax Syntax { get; }
		public INamedTypeSymbol ContainingType { get; }
		public INamedTypeSymbol? ExplicitInterfaceType { get; }
		public string PropertyName => Syntax.Identifier.ValueText;
	}

	private sealed class CacheModel
	{
		public CacheModel(
			INamedTypeSymbol type,
			ClassDeclarationSyntax typeDeclaration,
			string fieldName,
			string propertyNameExpression,
			string hintName,
			Location location)
		{
			Type = type;
			TypeDeclaration = typeDeclaration;
			FieldName = fieldName;
			PropertyNameExpression = propertyNameExpression;
			HintName = hintName;
			Location = location;
		}

		public INamedTypeSymbol Type { get; }
		public ClassDeclarationSyntax TypeDeclaration { get; }
		public string FieldName { get; }
		public string PropertyNameExpression { get; }
		public string HintName { get; }
		public Location Location { get; }
	}
}
