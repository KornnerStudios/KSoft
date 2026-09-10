using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace KSoft.PropertyChanged.SourceGeneration;

internal static class GeneratedSourceUtilities
{
	public static string ModifiersText(SyntaxTokenList modifiers) =>
		modifiers.Count == 0
			? string.Empty
			: string.Join(" ", modifiers.Select(static modifier => modifier.Text)) + " ";

	public static string NamespaceName(INamespaceSymbol namespaceSymbol)
	{
		if (namespaceSymbol.IsGlobalNamespace)
		{
			return string.Empty;
		}

		var parts = new Stack<string>();
		for (INamespaceSymbol? current = namespaceSymbol;
			current != null && !current.IsGlobalNamespace;
			current = current.ContainingNamespace)
		{
			parts.Push(EscapeIdentifier(current.Name));
		}

		return string.Join(".", parts);
	}

	public static string EscapeIdentifier(string identifier) =>
		SyntaxFacts.GetKeywordKind(identifier) != SyntaxKind.None
			|| SyntaxFacts.GetContextualKeywordKind(identifier) != SyntaxKind.None
				? "@" + identifier
				: identifier;

	public static string CreateHintName(
		INamedTypeSymbol type,
		SymbolDisplayFormat displayFormat,
		string domain)
	{
		string typeIdentity = type.ToDisplayString(displayFormat);
		string identity = $"{typeIdentity}.{domain}";
		var safeName = new StringBuilder(identity.Length);
		foreach (char character in identity)
		{
			safeName.Append(char.IsLetterOrDigit(character) ? character : '_');
		}
		var safeDomain = new StringBuilder(domain.Length);
		foreach (char character in domain)
		{
			safeDomain.Append(char.IsLetterOrDigit(character) ? character : '_');
		}

		return string.Format(
			CultureInfo.InvariantCulture,
			"KSoft.PropertyChanged.{0}.{1}.{2:X8}.g.cs",
			safeDomain,
			safeName,
			StableHash(identity));
	}

	private static uint StableHash(string value)
	{
		unchecked
		{
			uint hash = 2166136261;
			foreach (char character in value)
			{
				hash = (hash ^ character) * 16777619;
			}

			return hash;
		}
	}
}
