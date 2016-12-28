using System;
using System.Collections.Generic;
using System.Xml;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.IO
{
	public sealed partial class XmlElementStream : TagElementTextStream<XmlDocument, XmlElement>
	{
		/// <summary>XmlNodes which we support explicit streaming on</summary>
		/// <param name="type"></param>
		/// <returns></returns>
		[Contracts.Pure]
		public static bool StreamSourceIsValid(XmlNodeType type)
		{
			switch (type)
			{
				case XmlNodeType.Element:
				case XmlNodeType.Attribute:
				case XmlNodeType.Text: // aka, Cursor
					return true;

				default: return false;
			}
		}

		#region Cursor
		public override string CursorName { get { return Cursor != null ? Cursor.Name : null; } }

		public override void InitializeAtRootElement()
		{
			Cursor = Document.DocumentElement;
		}
		#endregion

		#region Util
		public override bool AttributeExists(string name)
		{
			if (Cursor == null)
				return false;
			if (!ValidateNameArg(name))
				return false;

			XmlNode n = Cursor.Attributes[name];

			return n != null;
		}

		public override bool AttributesExist { get { return Cursor != null && Cursor.HasAttributes; } }

		public override IEnumerable<string> AttributeNames { get {
			if (AttributesExist)
				foreach (XmlAttribute attr in Cursor.Attributes)
					yield return attr.Name;
		} }

		public override bool ElementsExists(string name)
		{
			if (Cursor == null)
				return false;
			if (!ValidateNameArg(name))
				return false;

			XmlElement n = Cursor[name];

			return n != null && n.Value != string.Empty;
		}

		public override bool ElementsExist { get { return Cursor != null && Cursor.HasChildNodes; } }

		public override IEnumerable<XmlElement> Elements { get {
			if (ElementsExist)
				foreach (XmlNode n in Cursor)
					if (n is XmlElement)
						yield return (XmlElement)n;
		} }

		public override IEnumerable<XmlElement> ElementsByName(string localName)
		{
			if (ElementsExist)
				foreach (XmlNode n in Cursor.ChildNodes)
					if (n is XmlElement && n.Name == localName)
						yield return (XmlElement)n;

#if false // this returns ALL descendants, no just immediate children
			var elements = Cursor.GetElementsByTagName(localName);

			foreach(XmlNode n in elements)
				if(n is XmlElement) yield return (XmlElement)n;
#endif
		}

		/// <see cref="XmlElement.ChildNodes.Count"/>
		protected override int PredictElementCount(XmlElement cursor)
		{
			return cursor.ChildNodes.Count;
		}
		#endregion

		#region Constructor
		XmlElementStream()
		{
			CommentsEnabled = true;
		}

		/// <summary>Initialize an element stream from a stream with <see cref="owner"/> as the initial owner object</summary>
		/// <param name="sourceStream">Stream we're to load the XML from</param>
		/// <param name="permissions">Supported access permissions for this stream</param>
		/// <param name="owner">Initial owner object</param>
		public XmlElementStream(System.IO.Stream sourceStream,
			System.IO.FileAccess permissions = System.IO.FileAccess.ReadWrite, object owner = null)
		{
			Contract.Requires<ArgumentNullException>(sourceStream != null);
			Contract.Requires<ArgumentException>(sourceStream.HasPermissions(permissions));

			SetStreamName(sourceStream);

			Document = new Xml.XmlDocumentWithLocation();
			Document.Load(sourceStream);

			StreamMode = StreamPermissions = permissions;

			this.Owner = owner;
		}

		/// <summary>Initialize an element stream from the XML file <paramref name="filename"/> with <see cref="owner"/> as the initial owner object</summary>
		/// <param name="filename">Name of the XML file we're to load</param>
		/// <param name="permissions">Supported access permissions for this stream</param>
		/// <param name="owner">Initial owner object</param>
		public XmlElementStream(string filename,
			System.IO.FileAccess permissions = System.IO.FileAccess.ReadWrite, object owner = null)
		{
			Contract.Requires<ArgumentNullException>(filename != null);

			if (!System.IO.File.Exists(filename))
				throw new System.IO.FileNotFoundException("XmlElementStream: Load", filename);

			Document = new Xml.XmlDocumentWithLocation();
			Document.Load(this.StreamName = filename);

			StreamMode = StreamPermissions = permissions;

			this.Owner = owner;
		}

		/// <summary>
		/// Initialize an element stream from the XML nodes <paramref name="document"/>
		/// and <paramref name="cursor"/> with <paramref name="owner"/> as the initial owner object
		/// </summary>
		/// <param name="document"><paramref name="cursor"/>'s owner document</param>
		/// <param name="cursor">Starting element cursor</param>
		/// <param name="permissions">Supported access permissions for this stream</param>
		/// <param name="owner">Initial owner object</param>
		public XmlElementStream(XmlDocument document, XmlElement cursor,
			System.IO.FileAccess permissions = System.IO.FileAccess.ReadWrite, object owner = null)
		{
			Contract.Requires<ArgumentNullException>(document != null);
			Contract.Requires(object.ReferenceEquals(cursor.OwnerDocument, document));

			Document = document;
			Cursor = cursor;

			this.StreamName = string.Format("XmlDocument:{0}", document.Name);

			StreamMode = StreamPermissions = permissions;

			this.Owner = owner;
		}

		/// <summary>Initialize a new element stream with write permissions</summary>
		/// <param name="owner">Initial owner object</param>
		/// <param name="rootName">Name of the document element</param>
		/// <returns></returns>
		public static XmlElementStream CreateForWrite(string rootName, object owner = null)
		{
			Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(rootName));

			var root = new XmlDocument();
			root.AppendChild(root.CreateElement(rootName));

			XmlElementStream @this = new XmlElementStream();
			@this.Document = root;

			@this.StreamMode = @this.StreamPermissions = System.IO.FileAccess.Write;

			@this.Owner = owner;

			@this.InitializeAtRootElement();

			return @this;
		}
		#endregion

		public override bool SupportsComments { get { return true; } }
	};
}