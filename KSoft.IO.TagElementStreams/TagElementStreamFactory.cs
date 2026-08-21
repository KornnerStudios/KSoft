using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
namespace KSoft.IO
{
	public static class TagElementStreamFactory
	{
		#region Registration APIs
		/// <summary>Get the file extension for a given format, or null if it isn't supported</summary>
		/// <param name="format">Format to query the extension for. Supports type flags in value</param>
		/// <returns>The file extension (with initial dot) for that given format. Or null if it isn't support (eg, requested binary, but only supports text)</returns>
		public delegate string GetExtensionDelegate(TagElementStreamFormat format);
		public delegate dynamic OpenFromStreamDelegate(TagElementStreamFormat format, System.IO.Stream sourceStream,
			FileAccess permissions, object owner = null);

		[SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
		public sealed class RegisteredFormat
		{
			public string Name { get; private set; }
			public TagElementStreamFormat BaseFormat { get; private set; }

			internal GetExtensionDelegate GetExtension { get; private set; }
			internal OpenFromStreamDelegate Open { get; private set; }

			internal RegisteredFormat(string name, TagElementStreamFormat baseFormat)
			{
				Name = name;
				BaseFormat = baseFormat;

				GetExtension = null;
				Open = null;
			}

			public RegisteredFormat RegisterExtension(GetExtensionDelegate handler)
			{
				ArgumentNullException.ThrowIfNull(handler);

				GetExtension = handler;

				#region Register Text
				var extension_format = BaseFormat;
				string extension = GetExtension(extension_format);
				if (extension != null)
				{
					gRegisteredFileExtensions.Add(extension, extension_format);
				}
				#endregion
				#region Register Binary
				extension_format |= TagElementStreamFormat.Binary;
				// #TODO: not all binary formats are implemented yet, and will throw an exception
				try { extension = GetExtension(extension_format); }
				catch (NotImplementedException) { extension = null; }

				if (extension != null)
				{
					gRegisteredFileExtensions.Add(extension, extension_format);
				}
				#endregion

				return this;
			}
			public RegisteredFormat RegisterOpen(OpenFromStreamDelegate handler)
			{
				ArgumentNullException.ThrowIfNull(handler);

				Open = handler;

				return this;
			}
		};

		static readonly Dictionary<TagElementStreamFormat, RegisteredFormat> gRegisteredFormats;
		static readonly Dictionary<string, TagElementStreamFormat> gRegisteredFileExtensions;

		public static RegisteredFormat Register(TagElementStreamFormat baseFormat, string name = null)
		{
			ArgumentOutOfRangeException.ThrowIfEqual((int)baseFormat, (int)TagElementStreamFormat.Undefined, nameof(baseFormat));
			ArgumentOutOfRangeException.ThrowIfNotEqual(
				(int)baseFormat.GetTypeFlags(), (int)TagElementStreamFormat.Undefined, nameof(baseFormat));
			if ((baseFormat < TagElementStreamFormat.kCustomStart && baseFormat > TagElementStreamFormat.kCustomEnd) &&
				string.IsNullOrEmpty(name))
			{
				throw new ArgumentException("Custom formats require an explicit name", nameof(name));
			}

			if (string.IsNullOrEmpty(name))
			{
				name = baseFormat.ToString();
			}

			var registration = new RegisteredFormat(name, baseFormat);
			gRegisteredFormats.Add(baseFormat, registration);

			return registration;
		}

		static RegisteredFormat GetRegistration(TagElementStreamFormat format, string operation)
		{
			ArgumentException.ThrowIfNullOrEmpty(operation);

			var base_format = format.GetBaseFormat();

			if (!gRegisteredFormats.TryGetValue(base_format, out RegisteredFormat registration))
			{
				throw new ArgumentException(string.Format(Util.InvariantCultureInfo,
					"Format {0} ({1}) is not registered, can't {2}",
					base_format, format, operation));
			}

			return registration;
		}

		static void ThrowIfUnexpectedBaseFormat(TagElementStreamFormat format, TagElementStreamFormat expectedFormat)
		{
			var base_format = format.GetBaseFormat();
			if (base_format != expectedFormat)
			{
				throw new ArgumentException(string.Format(Util.InvariantCultureInfo,
					"Format base is {0}, expected {1}.",
					base_format,
					expectedFormat), nameof(format));
			}
		}
		#endregion

		#region Xml
		static string XmlGetExtension(TagElementStreamFormat format)
		{
			ThrowIfUnexpectedBaseFormat(format, TagElementStreamFormat.Xml);

			if (format.IsText())
			{
				return ".xml";
			}
			else if (format.IsBinary()) // haven't decided on a standard to use yet
			{
				throw new NotImplementedException("General binary XML files not yet implemented");
			}

			throw new Debug.UnreachableException(format.ToString());
		}
		static dynamic XmlOpenFromStream(TagElementStreamFormat format, System.IO.Stream sourceStream,
			FileAccess permissions, object owner)
		{
			ThrowIfUnexpectedBaseFormat(format, TagElementStreamFormat.Xml);

			if (format.IsText())
			{
				var stream = new XmlElementStream(sourceStream, permissions, owner);
				stream.InitializeAtRootElement();

				return stream;
			}
			else if (format.IsBinary()) // haven't decided on a standard to use yet
			{
				throw new NotImplementedException("General binary XML files not yet implemented");
			}

			throw new Debug.UnreachableException(format.ToString());
		}
		#endregion

		#region Json
		static string JsonGetExtension(TagElementStreamFormat format)
		{
			ThrowIfUnexpectedBaseFormat(format, TagElementStreamFormat.Json);

			if (format.IsText())
			{
				return ".json";
			}
			else if (format.IsBinary())
			{
				return ".bson";
			}

			throw new Debug.UnreachableException(format.ToString());
		}
		static dynamic JsonOpenFromStream(TagElementStreamFormat format, System.IO.Stream sourceStream,
			FileAccess permissions, object owner)
		{
			ThrowIfUnexpectedBaseFormat(format, TagElementStreamFormat.Json);

			if (format.IsText())
			{
				throw new NotImplementedException();
			}
			else if (format.IsBinary())
			{
				throw new NotImplementedException();
			}

			throw new Debug.UnreachableException(format.ToString());
		}
		#endregion

		#region Yaml
		static string YamlGetExtension(TagElementStreamFormat format)
		{
			ThrowIfUnexpectedBaseFormat(format, TagElementStreamFormat.Yaml);

			if (format.IsText())
			{
				return ".yaml";
			}
			else if (format.IsBinary()) // Yaml doesn't support binary formats
			{
				return null;
			}

			throw new Debug.UnreachableException(format.ToString());
		}
		static dynamic YamlOpenFromStream(TagElementStreamFormat format, System.IO.Stream sourceStream,
			FileAccess permissions, object owner)
		{
			ThrowIfUnexpectedBaseFormat(format, TagElementStreamFormat.Yaml);

			if (format.IsText())
			{
				throw new NotImplementedException();
			}
			else if (format.IsBinary())
			{
				throw new NotSupportedException("Yaml doesn't support binary streams");
			}

			throw new Debug.UnreachableException(format.ToString());
		}
		#endregion

		[SuppressMessage("Microsoft.Design", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
		static TagElementStreamFactory()
		{
			gRegisteredFormats = new Dictionary<TagElementStreamFormat, RegisteredFormat>();
			gRegisteredFileExtensions = new Dictionary<string, TagElementStreamFormat>();

			Register(TagElementStreamFormat.Xml)
				.RegisterExtension	(XmlGetExtension)
				.RegisterOpen		(XmlOpenFromStream);

			Register(TagElementStreamFormat.Json)
				.RegisterExtension	(JsonGetExtension)
				.RegisterOpen		(JsonOpenFromStream);

			Register(TagElementStreamFormat.Yaml)
				.RegisterExtension	(YamlGetExtension)
				.RegisterOpen		(YamlOpenFromStream);
		}

		public static dynamic Open(System.IO.Stream sourceStream, TagElementStreamFormat format,
			FileAccess permissions = FileAccess.ReadWrite, object owner = null)
		{
			ArgumentNullException.ThrowIfNull(sourceStream);
			if (!sourceStream.HasPermissions(permissions))
			{
				throw new ArgumentException("Stream does not have the requested permissions.", nameof(permissions));
			}

			var registration = GetRegistration(format, "open");

			return registration.Open(format, sourceStream, permissions, owner);
		}

		public static dynamic Open(string filename,
			FileAccess permissions = FileAccess.ReadWrite, object owner = null)
		{
			ArgumentException.ThrowIfNullOrEmpty(filename);

			string extension = Path.GetExtension(filename);
			if (string.IsNullOrEmpty(extension))
			{
				throw new ArgumentException(string.Format(Util.InvariantCultureInfo,
					"'{0}' doesn't have a valid file extension",
					filename));
			}

			if (!gRegisteredFileExtensions.TryGetValue(extension, out TagElementStreamFormat format))
			{
				throw new ArgumentException(string.Format(Util.InvariantCultureInfo,
					"No TagElementStream is registered to handle '{0}' files",
					extension));
			}

			// NOTE: could just use File.OpenRead instead. File isn't actually ever written to in this context
			using (var fs = File.Open(filename, FileMode.Open, permissions))
			{
				var stream = Open(fs, format, permissions, owner);

				return stream;
			}
		}
	};
}
