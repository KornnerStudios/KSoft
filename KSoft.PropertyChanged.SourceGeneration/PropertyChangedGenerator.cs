using System;
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

/// <summary>Generates provider-specific partial properties and reusable event-args caches.</summary>
[Generator(LanguageNames.CSharp)]
public sealed partial class PropertyChangedGenerator : IIncrementalGenerator
{
	/// <inheritdoc />
	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		context.RegisterPostInitializationOutput(static postInitializationContext => postInitializationContext.AddSource(
			GeneratorContracts.ContractHintName, SourceText.From(GeneratorContracts.ContractSource, Encoding.UTF8)));

		// Attribute-driven discovery keeps unrelated syntax out of the incremental pipeline.
		IncrementalValuesProvider<Candidate> candidates = context.SyntaxProvider.ForAttributeWithMetadataName(
			GeneratorContracts.AttributeMetadataName,
			static (node, _) => node is PropertyDeclarationSyntax or IndexerDeclarationSyntax,
			static (attributeContext, _) => new Candidate(
				(IPropertySymbol)attributeContext.TargetSymbol,
				(BasePropertyDeclarationSyntax)attributeContext.TargetNode,
				attributeContext.Attributes[0]));

		context.RegisterSourceOutput(candidates.Collect().Combine(context.CompilationProvider),
			static (sourceContext, input) => Execute(sourceContext, input.Right, input.Left));

		PropertyChangedEventArgsCacheGeneration.Initialize(context);
	}

	private static void Execute(SourceProductionContext context, Compilation compilation, ImmutableArray<Candidate> candidates)
	{
		if (candidates.IsDefaultOrEmpty) return;

		INamedTypeSymbol? basicViewModel = compilation.GetTypeByMetadataName(GeneratorContracts.BasicViewModelMetadataName);
		INamedTypeSymbol? caliburnPropertyChangedBase = compilation.GetTypeByMetadataName(
			GeneratorContracts.CaliburnPropertyChangedBaseMetadataName);
		INamedTypeSymbol? hostAttribute = compilation.GetTypeByMetadataName(
			GeneratorContracts.HostAttributeMetadataName);
		INamedTypeSymbol? propertyChangedEventArgs = compilation.GetTypeByMetadataName(
			GeneratorContracts.PropertyChangedEventArgsMetadataName);
		INamedTypeSymbol? equatableType = compilation.GetTypeByMetadataName(
			GeneratorContracts.EquatableMetadataName);

		// One generated file per host keeps provider-specific members and properties together.
		var propertiesByType = new Dictionary<INamedTypeSymbol, List<PropertyModel>>(SymbolEqualityComparer.Default);
		foreach (Candidate candidate in candidates)
		{
			if (!TryCreateProperty(
				context,
				compilation,
				candidate,
				equatableType,
				out PropertyModel? property)) continue;
			PropertyModel validProperty = property!;
			if (!propertiesByType.TryGetValue(validProperty.ContainingType, out List<PropertyModel>? properties))
			{
				properties = new List<PropertyModel>();
				propertiesByType.Add(validProperty.ContainingType, properties);
			}
			properties.Add(validProperty);
		}

		var outputs = new List<GeneratedType>();
		foreach (KeyValuePair<INamedTypeSymbol, List<PropertyModel>> pair in propertiesByType)
		{
			pair.Value.Sort(static (left, right) => string.CompareOrdinal(left.Property.Name, right.Property.Name));
			if (!TryResolveHostStrategy(
				context,
				compilation,
				pair.Key,
				pair.Value,
				hostAttribute,
				propertyChangedEventArgs,
				basicViewModel,
				caliburnPropertyChangedBase,
				out PropertyChangedHostStrategy? strategy)) continue;

			PropertyChangedHostStrategy validStrategy = strategy!;
			if (validStrategy.UnsupportedDependentNotificationProvider is string providerName)
			{
				bool unsupportedDependents = false;
				foreach (PropertyModel property in pair.Value)
				{
					if (property.DependentProperties.IsDefaultOrEmpty) continue;

					context.ReportDiagnostic(Diagnostic.Create(
						DiagnosticDescriptors.UnsupportedDependentNotifications,
						property.Location,
						property.Property.Name,
						providerName));
					unsupportedDependents = true;
				}
				if (unsupportedDependents) continue;
			}

			bool hasCollision = false;
			foreach (ReservedMember reservedMember in validStrategy.ReservedMembers(pair.Value))
			{
				if (!HasAccessibleMember(pair.Key, reservedMember.Name, compilation)) continue;

				context.ReportDiagnostic(Diagnostic.Create(
					DiagnosticDescriptors.GeneratedMemberCollision,
					reservedMember.Location,
					pair.Key.ToDisplayString(),
					reservedMember.Name));
				hasCollision = true;
			}
			if (hasCollision) continue;

			outputs.Add(new GeneratedType(
				pair.Key,
				pair.Value,
				validStrategy,
				GeneratedSourceUtilities.CreateHintName(
					pair.Key,
					GeneratedSourceUtilities.TypeDisplayFormat,
					"Properties")));
		}

		outputs.Sort(static (left, right) => string.CompareOrdinal(left.HintName, right.HintName));
		var usedHints = new HashSet<string>(StringComparer.Ordinal);
		foreach (GeneratedType output in outputs)
		{
			if (!usedHints.Add(output.HintName))
			{
				context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.GeneratedMemberCollision, output.Properties[0].Location, output.Type.ToDisplayString(), output.HintName));
				continue;
			}
			context.AddSource(output.HintName, SourceText.From(BuildSource(output), Encoding.UTF8));
		}
	}

	private static bool HasAccessibleMember(
		INamedTypeSymbol containingType,
		string memberName,
		Compilation compilation)
	{
		for (INamedTypeSymbol? current = containingType;
			current != null;
			current = current.BaseType)
		{
			if (current.GetMembers(memberName)
				.Any(member => compilation.IsSymbolAccessibleWithin(member, containingType)))
			{
				return true;
			}
		}

		return false;
	}

	private static bool TryCreateProperty(
		SourceProductionContext context,
		Compilation compilation,
		Candidate candidate,
		INamedTypeSymbol? equatableType,
		out PropertyModel? result)
	{
		result = null;
		IPropertySymbol property = candidate.Property;
		INamedTypeSymbol containingType = property.ContainingType;
		Location location = candidate.Location;

		if (containingType.ContainingType != null || containingType.Arity != 0 || containingType.TypeKind != TypeKind.Class || candidate.Syntax.Parent is not ClassDeclarationSyntax)
		{
			ReportUnsupported(context, property, location, "only top-level, non-generic classes are supported");
			return false;
		}

		var containingDeclaration = (ClassDeclarationSyntax)candidate.Syntax.Parent;
		if (!containingDeclaration.Modifiers.Any(static modifier => modifier.IsKind(SyntaxKind.PartialKeyword)))
		{
			context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.NonPartialType, location, containingType.ToDisplayString()));
			return false;
		}
		if (containingDeclaration.Modifiers.Any(static modifier => modifier.IsKind(SyntaxKind.FileKeyword)))
		{
			ReportUnsupported(context, property, location, "file-local hosts cannot be extended from generated files");
			return false;
		}

		if (property.ExplicitInterfaceImplementations.Length != 0 || candidate.Syntax is PropertyDeclarationSyntax explicitProperty && explicitProperty.ExplicitInterfaceSpecifier != null)
		{
			ReportUnsupported(context, property, location, "explicit-interface properties are not supported");
			return false;
		}
		if (property.SetMethod?.IsInitOnly == true)
		{
			ReportUnsupported(context, property, location, "init-only properties are not supported");
			return false;
		}

		if (candidate.Syntax is not PropertyDeclarationSyntax syntax || !IsValidDefiningProperty(property, syntax))
		{
			context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.InvalidPartialProperty, location, property.Name));
			return false;
		}

		bool hasExplicitField = TryReadBackingField(candidate.Attribute, out bool backingFieldSpecified, out string? backingFieldName);
		if (backingFieldSpecified && !hasExplicitField)
		{
			context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.InvalidBackingField, location, backingFieldName ?? "<null>", property.Name));
			return false;
		}

		if (equatableType == null)
		{
			context.ReportDiagnostic(Diagnostic.Create(
				DiagnosticDescriptors.InvalidPropertyChangedHost,
				location,
				containingType.ToDisplayString()));
			return false;
		}

		EqualityMode equality = EqualityModeFor(property.Type, equatableType);
		bool alwaysNotify = ReadAlwaysNotify(candidate.Attribute);
		if (!TryReadDependentProperties(
			context,
			compilation,
			candidate,
			out ImmutableArray<string> dependentProperties))
		{
			return false;
		}

		IFieldSymbol? field = null;
		if (hasExplicitField && !TryGetBackingField(containingType, property, backingFieldName!, out field))
		{
			context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.InvalidBackingField, location, backingFieldName!, property.Name));
			return false;
		}

		result = new PropertyModel(
			property,
			syntax,
			field,
			equality,
			alwaysNotify,
			dependentProperties,
			location);
		return true;
	}

	private static bool IsValidDefiningProperty(IPropertySymbol property, PropertyDeclarationSyntax syntax)
	{
		if (!property.IsPartialDefinition || property.PartialImplementationPart != null || property.IsStatic || property.IsIndexer || property.ReturnsByRef || property.ReturnsByRefReadonly || property.GetMethod == null || property.SetMethod == null
			|| !syntax.Modifiers.Any(static modifier => modifier.IsKind(SyntaxKind.PartialKeyword)) || syntax.ExpressionBody != null || syntax.AccessorList == null || syntax.AccessorList.Accessors.Count != 2
			|| syntax.Modifiers.Any(static modifier => modifier.IsKind(SyntaxKind.AbstractKeyword) || modifier.IsKind(SyntaxKind.ExternKeyword) || modifier.IsKind(SyntaxKind.RequiredKeyword))) return false;

		bool hasGet = false;
		bool hasSet = false;
		foreach (AccessorDeclarationSyntax accessor in syntax.AccessorList.Accessors)
		{
			if (accessor.Body != null || accessor.ExpressionBody != null || accessor.SemicolonToken.IsMissing) return false;
			if (accessor.IsKind(SyntaxKind.GetAccessorDeclaration)) hasGet = true;
			else if (accessor.IsKind(SyntaxKind.SetAccessorDeclaration)) hasSet = true;
			else return false;
		}
		return hasGet && hasSet;
	}

	private static bool TryReadBackingField(AttributeData attribute, out bool specified, out string? fieldName)
	{
		specified = false;
		fieldName = null;
		foreach (KeyValuePair<string, TypedConstant> argument in attribute.NamedArguments)
		{
			if (!string.Equals(
				argument.Key,
				GeneratorContracts.BackingFieldPropertyName,
				StringComparison.Ordinal)) continue;
			specified = true;
			fieldName = argument.Value.Value as string;
			return !string.IsNullOrWhiteSpace(fieldName);
		}
		return false;
	}

	private static bool ReadAlwaysNotify(AttributeData attribute)
	{
		foreach (KeyValuePair<string, TypedConstant> argument in attribute.NamedArguments)
		{
			if (string.Equals(
				argument.Key,
				GeneratorContracts.AlwaysNotifyPropertyName,
				StringComparison.Ordinal))
			{
				return argument.Value.Value is true;
			}
		}

		return false;
	}

	private static bool TryReadDependentProperties(
		SourceProductionContext context,
		Compilation compilation,
		Candidate candidate,
		out ImmutableArray<string> dependentProperties)
	{
		dependentProperties = ImmutableArray<string>.Empty;
		foreach (KeyValuePair<string, TypedConstant> argument in candidate.Attribute.NamedArguments)
		{
			if (!string.Equals(
				argument.Key,
				GeneratorContracts.DependentPropertiesPropertyName,
				StringComparison.Ordinal)) continue;

			if (argument.Value.IsNull)
			{
				ReportInvalidDependentProperty(
					context,
					candidate,
					"<null>",
					"DependentProperties cannot be null");
				return false;
			}

			var builder = ImmutableArray.CreateBuilder<string>(argument.Value.Values.Length);
			var names = new HashSet<string>(StringComparer.Ordinal);
			bool valid = true;
			foreach (TypedConstant value in argument.Value.Values)
			{
				string? name = value.Value as string;
				if (string.IsNullOrWhiteSpace(name))
				{
					ReportInvalidDependentProperty(
						context,
						candidate,
						name ?? "<null>",
						"dependent property names must be non-empty");
					valid = false;
					continue;
				}
				string validName = name!;
				if (string.Equals(validName, candidate.Property.Name, StringComparison.Ordinal))
				{
					ReportInvalidDependentProperty(
						context,
						candidate,
						validName,
						"a generated property cannot depend on itself");
					valid = false;
					continue;
				}
				if (!names.Add(validName))
				{
					ReportInvalidDependentProperty(
						context,
						candidate,
						validName,
						"a dependent property cannot be listed more than once");
					valid = false;
					continue;
				}

				DependentPropertyResolution resolution = ResolveDependentProperty(
					compilation,
					candidate.Property.ContainingType,
					validName);
				if (resolution != DependentPropertyResolution.Instance)
				{
					string reason = resolution == DependentPropertyResolution.StaticOnly
						? "the name resolves only to a static property; name an instance property"
						: "the name must resolve to an instance property on the containing type, a base type, or an implemented interface";
					ReportInvalidDependentProperty(context, candidate, validName, reason);
					valid = false;
					continue;
				}

				builder.Add(validName);
			}

			if (!valid) return false;

			dependentProperties = builder.MoveToImmutable();
			return true;
		}

		return true;
	}

	private static DependentPropertyResolution ResolveDependentProperty(
		Compilation compilation,
		INamedTypeSymbol containingType,
		string name)
	{
		bool foundStatic = false;
		for (INamedTypeSymbol? current = containingType;
			current != null;
			current = current.BaseType)
		{
			foreach (IPropertySymbol property in current.GetMembers().OfType<IPropertySymbol>())
			{
				if (!MatchesDependentPropertyName(property, name)) continue;
				if (property.IsStatic)
				{
					foundStatic = true;
					continue;
				}
				if (compilation.IsSymbolAccessibleWithin(property, containingType)
					|| SymbolEqualityComparer.Default.Equals(current, containingType))
				{
					return DependentPropertyResolution.Instance;
				}
			}
		}

		foreach (INamedTypeSymbol interfaceType in containingType.AllInterfaces)
		{
			foreach (IPropertySymbol property in interfaceType.GetMembers().OfType<IPropertySymbol>())
			{
				if (!MatchesDependentPropertyName(property, name)) continue;
				if (!property.IsStatic) return DependentPropertyResolution.Instance;

				foundStatic = true;
			}
		}

		return foundStatic
			? DependentPropertyResolution.StaticOnly
			: DependentPropertyResolution.Missing;
	}

	private static bool MatchesDependentPropertyName(IPropertySymbol property, string name) =>
		string.Equals(property.Name, name, StringComparison.Ordinal)
			|| property.ExplicitInterfaceImplementations.Any(implementation =>
				string.Equals(implementation.Name, name, StringComparison.Ordinal));

	private static void ReportInvalidDependentProperty(
		SourceProductionContext context,
		Candidate candidate,
		string dependentProperty,
		string reason) =>
		context.ReportDiagnostic(Diagnostic.Create(
			DiagnosticDescriptors.InvalidDependentProperty,
			candidate.Location,
			candidate.Property.Name,
			dependentProperty,
			reason));

	private static EqualityMode EqualityModeFor(
		ITypeSymbol type,
		INamedTypeSymbol equatableType)
	{
		// These are the three legacy helper choices exercised by the pilot consumers.
		if (type.TypeKind == TypeKind.Enum)
		{
			return EqualityMode.Enum;
		}
		if (type.IsValueType
			&& !IsNullableValueType(type)
			&& ImplementsSelfEquatable(type, equatableType))
		{
			return EqualityMode.EquatableValue;
		}

		return EqualityMode.Default;
	}

	private static bool ImplementsSelfEquatable(ITypeSymbol type, INamedTypeSymbol equatableType)
	{
		foreach (INamedTypeSymbol interfaceType in type.AllInterfaces)
		{
			if (IsSelfEquatable(interfaceType, type, equatableType)) return true;
		}
		return false;
	}

	private static bool IsSelfEquatable(
		INamedTypeSymbol type,
		ITypeSymbol argument,
		INamedTypeSymbol equatableType) =>
		SymbolEqualityComparer.Default.Equals(type.OriginalDefinition, equatableType)
			&& SymbolEqualityComparer.Default.Equals(type.TypeArguments[0], argument);

	private static bool IsNullableValueType(ITypeSymbol type) => type is INamedTypeSymbol namedType && namedType.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T;

	private static bool TryGetBackingField(INamedTypeSymbol containingType, IPropertySymbol property, string fieldName, out IFieldSymbol? field)
	{
		ImmutableArray<ISymbol> members = containingType.GetMembers(fieldName);
		field = members.Length == 1 ? members[0] as IFieldSymbol : null;
		return field != null && !field.IsStatic && !field.IsReadOnly && !field.IsConst && SymbolEqualityComparer.Default.Equals(field.Type, property.Type);
	}

	private static void ReportUnsupported(SourceProductionContext context, IPropertySymbol property, Location location, string reason) =>
		context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.UnsupportedTarget, location, property.Name, reason));

	private static string BuildSource(GeneratedType output)
	{
		var writer = new SourceWriter();
		writer.WriteLine("// <auto-generated/>");
		writer.WriteLine();

		string namespaceName = GeneratedSourceUtilities.NamespaceName(output.Type.ContainingNamespace);
		if (namespaceName.Length != 0)
		{
			writer.WriteFileScopedNamespace(namespaceName);
			writer.WriteLine();
		}

		var declaration = (ClassDeclarationSyntax)output.Properties[0].Syntax.Parent!;
		writer.WriteLine(
			$"{GeneratedSourceUtilities.ModifiersText(declaration.Modifiers)}class {GeneratedSourceUtilities.EscapeIdentifier(output.Type.Name)}");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			output.Strategy.WriteTypeMembers(writer, output.Properties);

			foreach (PropertyModel property in output.Properties)
			{
				writer.WriteLine();
				WriteProperty(writer, property, output.Strategy);
			}
		}

		return writer.ToString();
	}

	private static void WriteProperty(
		SourceWriter writer,
		PropertyModel model,
		PropertyChangedHostStrategy strategy)
	{
		IPropertySymbol property = model.Property;
		string name = GeneratedSourceUtilities.EscapeIdentifier(property.Name);
		AccessorDeclarationSyntax getter = model.Syntax.AccessorList!.Accessors.Single(
			static accessor => accessor.IsKind(SyntaxKind.GetAccessorDeclaration));
		AccessorDeclarationSyntax setter = model.Syntax.AccessorList.Accessors.Single(
			static accessor => accessor.IsKind(SyntaxKind.SetAccessorDeclaration));
		// The defining partial property carries nullability; the generated implementation stays nullable-oblivious.
		string typeName = property.Type.ToDisplayString(GeneratedSourceUtilities.TypeDisplayFormat);

		WriteGeneratedAttributes(writer);
		writer.WriteLine(
			$"{GeneratedSourceUtilities.ModifiersText(model.Syntax.Modifiers)}{typeName} {name}");
		using (writer.EnterBlock(SourceWriterBlockType.Braces))
		{
			string storage = model.BackingField == null
				? "field"
				: GeneratedSourceUtilities.EscapeIdentifier(model.BackingField.Name);
			if (model.BackingField == null)
			{
				writer.WriteLine($"{GeneratedSourceUtilities.ModifiersText(getter.Modifiers)}get;");
			}
			else
			{
				writer.WriteLine($"{GeneratedSourceUtilities.ModifiersText(getter.Modifiers)}get => {storage};");
			}

			strategy.WriteSetter(writer, model, setter, typeName, name, storage);
		}
	}

	private static void WriteGeneratedAttributes(SourceWriter writer)
	{
		writer.WriteAttribute(
			$"global::System.CodeDom.Compiler.GeneratedCodeAttribute(\"{GeneratorContracts.GeneratorName}\", \"{GeneratorContracts.GeneratorVersion}\")");
		writer.WriteAttribute("global::System.Diagnostics.DebuggerNonUserCodeAttribute");
		writer.WriteAttribute("global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute");
	}

	private static string HelperName(EqualityMode equality) => equality switch
	{
		EqualityMode.EquatableValue => "SetFieldVal",
		EqualityMode.Enum => "SetFieldEnum",
		_ => "SetField",
	};

	private static string BasicViewModelCacheFieldName(string propertyName) => "s_" + propertyName;

	private sealed class Candidate
	{
		public Candidate(IPropertySymbol property, BasePropertyDeclarationSyntax syntax, AttributeData attribute) { Property = property; Syntax = syntax; Attribute = attribute; }
		public IPropertySymbol Property { get; }
		public BasePropertyDeclarationSyntax Syntax { get; }
		public AttributeData Attribute { get; }
		public Location Location => Syntax.GetLocation();
	}

	private sealed class PropertyModel
	{
		public PropertyModel(
			IPropertySymbol property,
			PropertyDeclarationSyntax syntax,
			IFieldSymbol? backingField,
			EqualityMode equality,
			bool alwaysNotify,
			ImmutableArray<string> dependentProperties,
			Location location)
		{
			Property = property;
			Syntax = syntax;
			BackingField = backingField;
			Equality = equality;
			AlwaysNotify = alwaysNotify;
			DependentProperties = dependentProperties;
			Location = location;
		}
		public IPropertySymbol Property { get; }
		public INamedTypeSymbol ContainingType => Property.ContainingType;
		public PropertyDeclarationSyntax Syntax { get; }
		public IFieldSymbol? BackingField { get; }
		public EqualityMode Equality { get; }
		public bool AlwaysNotify { get; }
		public ImmutableArray<string> DependentProperties { get; }
		public Location Location { get; }
	}

	private sealed class GeneratedType
	{
		public GeneratedType(INamedTypeSymbol type, List<PropertyModel> properties, PropertyChangedHostStrategy strategy, string hintName) { Type = type; Properties = properties; Strategy = strategy; HintName = hintName; }
		public INamedTypeSymbol Type { get; }
		public List<PropertyModel> Properties { get; }
		public PropertyChangedHostStrategy Strategy { get; }
		public string HintName { get; }
	}

	private enum EqualityMode { Default, EquatableValue, Enum }

	private enum DependentPropertyResolution { Missing, StaticOnly, Instance }
}
