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

/// <summary>Generates cached-args BasicViewModel partial-property implementations.</summary>
[Generator(LanguageNames.CSharp)]
public sealed class PropertyChangedGenerator : IIncrementalGenerator
{
	private static readonly SymbolDisplayFormat sTypeDisplayFormat = new(
		globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Included,
		typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
		genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
		miscellaneousOptions: SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers);

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
		INamedTypeSymbol? equatableType = compilation.GetTypeByMetadataName(
			typeof(IEquatable<>).FullName!);

		// One generated file per host lets all of its properties share a single event-args cache.
		var propertiesByType = new Dictionary<INamedTypeSymbol, List<PropertyModel>>(SymbolEqualityComparer.Default);
		foreach (Candidate candidate in candidates)
		{
			if (!TryCreateProperty(
				context,
				candidate,
				basicViewModel,
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
			if (pair.Key.GetMembers(GeneratorContracts.CacheTypeName).Length != 0)
			{
				context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.GeneratedMemberCollision, pair.Value[0].Location, pair.Key.ToDisplayString(), GeneratorContracts.CacheTypeName));
				continue;
			}
			outputs.Add(new GeneratedType(
				pair.Key,
				pair.Value,
				GeneratedSourceUtilities.CreateHintName(pair.Key, sTypeDisplayFormat, "Properties")));
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

	private static bool TryCreateProperty(
		SourceProductionContext context,
		Candidate candidate,
		INamedTypeSymbol? basicViewModel,
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

		if (basicViewModel == null
			|| equatableType == null
			|| !SymbolEqualityComparer.Default.Equals(containingType.BaseType, basicViewModel))
		{
			context.ReportDiagnostic(Diagnostic.Create(
				DiagnosticDescriptors.InvalidBasicViewModelHost,
				location,
				containingType.ToDisplayString()));
			return false;
		}

		EqualityMode equality = EqualityModeFor(property.Type, equatableType);
		bool alwaysNotify = ReadAlwaysNotify(candidate.Attribute);

		IFieldSymbol? field = null;
		if (hasExplicitField && !TryGetBackingField(containingType, property, backingFieldName!, out field))
		{
			context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.InvalidBackingField, location, backingFieldName!, property.Name));
			return false;
		}

		result = new PropertyModel(property, syntax, field, equality, alwaysNotify, location);
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
			WriteGeneratedAttributes(writer);
			writer.WriteLine($"private static class {GeneratorContracts.CacheTypeName}");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				foreach (PropertyModel property in output.Properties)
				{
					string propertyName = GeneratedSourceUtilities.EscapeIdentifier(property.Property.Name);
					writer.WriteLine(
						$"internal static readonly global::System.ComponentModel.PropertyChangedEventArgs {EventArgsFieldName(property.Property.Name)} =");
					using (writer.EnterBlock())
					{
						writer.WriteLine($"new(nameof({propertyName}));");
					}
				}
			}

			foreach (PropertyModel property in output.Properties)
			{
				writer.WriteLine();
				WriteProperty(writer, property);
			}
		}

		return writer.ToString();
	}

	private static void WriteProperty(SourceWriter writer, PropertyModel model)
	{
		IPropertySymbol property = model.Property;
		string name = GeneratedSourceUtilities.EscapeIdentifier(property.Name);
		AccessorDeclarationSyntax getter = model.Syntax.AccessorList!.Accessors.Single(
			static accessor => accessor.IsKind(SyntaxKind.GetAccessorDeclaration));
		AccessorDeclarationSyntax setter = model.Syntax.AccessorList.Accessors.Single(
			static accessor => accessor.IsKind(SyntaxKind.SetAccessorDeclaration));
		// The defining partial property carries nullability; the generated implementation stays nullable-oblivious.
		string typeName = property.Type.ToDisplayString(sTypeDisplayFormat);

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

			string eventArgs = $"{GeneratorContracts.CacheTypeName}.{EventArgsFieldName(property.Name)}";
			string helperCall = model.AlwaysNotify
				? $"base.SetField<{typeName}>(ref {storage}, value, {eventArgs}, true)"
				: $"base.{HelperName(model.Equality)}<{typeName}>(ref {storage}, value, {eventArgs})";
			writer.WriteLine($"{GeneratedSourceUtilities.ModifiersText(setter.Modifiers)}set => {helperCall};");
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

	private static string EventArgsFieldName(string propertyName) => "s_" + propertyName;

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
		public PropertyModel(IPropertySymbol property, PropertyDeclarationSyntax syntax, IFieldSymbol? backingField, EqualityMode equality, bool alwaysNotify, Location location) { Property = property; Syntax = syntax; BackingField = backingField; Equality = equality; AlwaysNotify = alwaysNotify; Location = location; }
		public IPropertySymbol Property { get; }
		public INamedTypeSymbol ContainingType => Property.ContainingType;
		public PropertyDeclarationSyntax Syntax { get; }
		public IFieldSymbol? BackingField { get; }
		public EqualityMode Equality { get; }
		public bool AlwaysNotify { get; }
		public Location Location { get; }
	}

	private sealed class GeneratedType
	{
		public GeneratedType(INamedTypeSymbol type, List<PropertyModel> properties, string hintName) { Type = type; Properties = properties; HintName = hintName; }
		public INamedTypeSymbol Type { get; }
		public List<PropertyModel> Properties { get; }
		public string HintName { get; }
	}

	private enum EqualityMode { Default, EquatableValue, Enum }
}
