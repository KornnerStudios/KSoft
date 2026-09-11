using System;
using System.Collections.Generic;
using System.Linq;
using KSoft.SourceGeneration.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace KSoft.PropertyChanged.SourceGeneration;

public sealed partial class PropertyChangedGenerator
{
	private sealed class CachedEventArgsHostStrategy
		: PropertyChangedHostStrategy
	{
		private const int CachedEventArgsProviderValue = 0;

		private readonly string mNotificationMethodName;

		private CachedEventArgsHostStrategy(string notificationMethodName)
		{
			mNotificationMethodName = notificationMethodName;
		}

		public static HostStrategyMatch Match(
			Compilation compilation,
			INamedTypeSymbol containingType,
			INamedTypeSymbol? hostAttribute,
			INamedTypeSymbol? propertyChangedEventArgs,
			out PropertyChangedHostStrategy? strategy,
			out string? unsupportedReason)
		{
			strategy = null;
			unsupportedReason = null;
			if (hostAttribute == null
				|| !TryFindNearestMarker(containingType, hostAttribute, out AttributeData? marker))
			{
				return HostStrategyMatch.NotApplicable;
			}

			if (!TryReadProvider(marker!, out int provider)
				|| provider != CachedEventArgsProviderValue)
			{
				unsupportedReason = "the explicit host marker specifies an unsupported notification provider";
				return HostStrategyMatch.InvalidContract;
			}

			if (!TryReadNotificationMethod(marker!, out string? notificationMethodName))
			{
				unsupportedReason = "the cached-event-args provider requires a non-empty NotificationMethod";
				return HostStrategyMatch.InvalidContract;
			}

			if (propertyChangedEventArgs == null
				|| !TryFindNotificationMethod(
					compilation,
					containingType,
					notificationMethodName!,
					propertyChangedEventArgs,
					out IMethodSymbol? notificationMethod))
			{
				unsupportedReason =
					$"'{notificationMethodName}' must identify exactly one accessible instance method returning void and accepting one by-value System.ComponentModel.PropertyChangedEventArgs";
				return HostStrategyMatch.InvalidContract;
			}

			strategy = new CachedEventArgsHostStrategy(notificationMethod!.Name);
			return HostStrategyMatch.Supported;
		}

		public override IEnumerable<ReservedMember> ReservedMembers(IReadOnlyList<PropertyModel> properties)
		{
			if (RequiresValueEqualityHelper(properties))
			{
				yield return new ReservedMember(
					GeneratorContracts.ValueEqualityMethodName,
					properties[0].Location);
			}

			foreach (PropertyModel property in properties)
			{
				yield return new ReservedMember(
					GeneratedSourceUtilities.PropertyChangedEventArgsFieldName(property.Property.Name),
					property.Location);
			}
		}

		public override void WriteTypeMembers(SourceWriter writer, IReadOnlyList<PropertyModel> properties)
		{
			bool wroteMember = false;
			if (RequiresValueEqualityHelper(properties))
			{
				WriteValueEqualityHelper(writer);
				wroteMember = true;
			}

			foreach (PropertyModel property in properties)
			{
				if (wroteMember) writer.WriteLine();

				string propertyName = GeneratedSourceUtilities.EscapeIdentifier(property.Property.Name);
				string fieldName = GeneratedSourceUtilities.PropertyChangedEventArgsFieldName(property.Property.Name);
				writer.WriteLine(
					$"private static readonly global::System.ComponentModel.PropertyChangedEventArgs {fieldName} =");
				using (writer.EnterBlock())
				{
					writer.WriteLine($"new(nameof({propertyName}));");
				}
				wroteMember = true;
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
			string valueStorage = ValueStorage(model, storage);
			string fieldName = GeneratedSourceUtilities.PropertyChangedEventArgsFieldName(model.Property.Name);
			writer.WriteLine($"{GeneratedSourceUtilities.ModifiersText(setter.Modifiers)}set");
			using (writer.EnterBlock(SourceWriterBlockType.Braces))
			{
				WriteEqualityGuard(writer, model, typeName, valueStorage);
				writer.WriteLine($"{valueStorage} = value;");
				writer.WriteLine(
					$"this.{GeneratedSourceUtilities.EscapeIdentifier(mNotificationMethodName)}({fieldName});");
			}
		}

		private static bool TryFindNearestMarker(
			INamedTypeSymbol containingType,
			INamedTypeSymbol hostAttribute,
			out AttributeData? marker)
		{
			for (INamedTypeSymbol? current = containingType;
				current != null;
				current = current.BaseType)
			{
				AttributeData[] markers = current.GetAttributes()
					.Where(attribute => SymbolEqualityComparer.Default.Equals(
						attribute.AttributeClass,
						hostAttribute))
					.ToArray();
				if (markers.Length == 0) continue;

				marker = markers[0];
				return true;
			}

			marker = null;
			return false;
		}

		private static bool TryReadProvider(AttributeData marker, out int provider)
		{
			provider = -1;
			if (marker.ConstructorArguments.Length != 1
				|| marker.ConstructorArguments[0].Value is not int value)
			{
				return false;
			}

			provider = value;
			return true;
		}

		private static bool TryReadNotificationMethod(
			AttributeData marker,
			out string? notificationMethodName)
		{
			notificationMethodName = null;
			foreach (KeyValuePair<string, TypedConstant> argument in marker.NamedArguments)
			{
				if (!string.Equals(
					argument.Key,
					GeneratorContracts.NotificationMethodPropertyName,
					StringComparison.Ordinal)) continue;

				notificationMethodName = argument.Value.Value as string;
				return !string.IsNullOrWhiteSpace(notificationMethodName);
			}

			return false;
		}

		private static bool TryFindNotificationMethod(
			Compilation compilation,
			INamedTypeSymbol containingType,
			string notificationMethodName,
			INamedTypeSymbol propertyChangedEventArgs,
			out IMethodSymbol? notificationMethod)
		{
			var candidates = new List<IMethodSymbol>();
			for (INamedTypeSymbol? current = containingType;
				current != null;
				current = current.BaseType)
			{
				candidates.AddRange(current.GetMembers(notificationMethodName)
					.OfType<IMethodSymbol>()
					.Where(method =>
						IsNotificationMethod(method, propertyChangedEventArgs)
							&& compilation.IsSymbolAccessibleWithin(method, containingType)));
			}

			IMethodSymbol[] effectiveCandidates = candidates
				.Where(candidate => !candidates.Any(other => Overrides(other, candidate)))
				.ToArray();
			if (effectiveCandidates.Length != 1)
			{
				notificationMethod = null;
				return false;
			}

			notificationMethod = effectiveCandidates[0];
			for (INamedTypeSymbol? current = containingType;
				current != null
					&& !SymbolEqualityComparer.Default.Equals(
						current,
						notificationMethod.ContainingType);
				current = current.BaseType)
			{
				if (current.GetMembers(notificationMethodName)
					.Any(member => compilation.IsSymbolAccessibleWithin(member, containingType)))
				{
					notificationMethod = null;
					return false;
				}
			}

			return true;
		}

		private static bool IsNotificationMethod(
			IMethodSymbol method,
			INamedTypeSymbol propertyChangedEventArgs) =>
			method.MethodKind == MethodKind.Ordinary
				&& !method.IsStatic
				&& method.ReturnsVoid
				&& method.Arity == 0
				&& method.Parameters.Length == 1
				&& method.Parameters[0].RefKind == RefKind.None
				&& SymbolEqualityComparer.Default.Equals(
					method.Parameters[0].Type,
					propertyChangedEventArgs);

		private static bool Overrides(IMethodSymbol method, IMethodSymbol candidate)
		{
			for (IMethodSymbol? overridden = method.OverriddenMethod;
				overridden != null;
				overridden = overridden.OverriddenMethod)
			{
				if (SymbolEqualityComparer.Default.Equals(overridden, candidate)) return true;
			}

			return false;
		}
	}
}
