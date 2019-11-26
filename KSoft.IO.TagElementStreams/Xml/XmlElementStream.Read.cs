using System.Xml;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.IO
{
	partial class XmlElementStream
	{
		#region ReadElement impl
		protected override string GetInnerText(XmlElement n)
		{
			var text_node = GetInnerTextNode(n);
			if (text_node != null)
			{
				ReadErrorNode = text_node; // TODO: which is more informative, using the element (n) or text_node?
				// TextNode's actual text
				return text_node.Value;
			}

			return null;
		}

		private XmlNode GetInnerTextNode(XmlElement n)
		{
			if (!n.HasChildNodes)
				return null;

			var text_node = n.LastChild;
			if (text_node.NodeType == XmlNodeType.Text)
				return text_node;

			text_node = n.FirstChild;
			if (text_node.NodeType == XmlNodeType.Text)
				return text_node;

			foreach (XmlNode node in n.ChildNodes)
			{
				if (node.NodeType == XmlNodeType.Text)
					return node;
			}

			return null;
		}
		#endregion

		#region ReadElement
		public override void ReadElementBegin(string name, out XmlElement oldCursor)
		{
			ValidateReadPermission();

			XmlElement n = Cursor[name];
			if (n == null)
				ThrowReadException(new System.Collections.Generic.KeyNotFoundException(
					"Element doesn't exist: " + name));

			oldCursor = Cursor;
			// update the error state with the node we're about to read from
			ReadErrorNode = n;
			Cursor = n;
		}
		public override void ReadElementEnd(ref XmlElement oldCursor)
		{
			RestoreCursor(ref oldCursor);
		}

		protected override XmlElement GetElement(string name)
		{
			ValidateReadPermission();

			XmlElement n = Cursor[name];
			if (n == null)
				ThrowReadException(new System.Collections.Generic.KeyNotFoundException(
					"Element doesn't exist: " + name));

			// update the error state with the node we're about to read from
			ReadErrorNode = n;
			return n;
		}
		#endregion

		#region ReadAttribute
		protected override string ReadAttribute(string name)
		{
			ValidateReadPermission();

			XmlNode n = Cursor.Attributes[name];
			if (n == null)
				ThrowReadException(new System.Collections.Generic.KeyNotFoundException(
					"Attribute doesn't exist: " + name));

			Contract.Assume(n != null);
			// update the error state with the node we're about to read from
			ReadErrorNode = n;
			return n.Value;
		}
		#endregion

		#region ReadElementOpt
		/// <summary>Streams out the InnerText of element <paramref name="name"/></summary>
		/// <param name="name">Element name</param>
		/// <returns></returns>
		protected override string ReadElementOpt(string name)
		{
			ValidateReadPermission();

			XmlElement n = Cursor[name];
			if (n == null)
				return null;

			// element exists, update the error state with the node we're about to read from
			ReadErrorNode = n;

			// NOTE: GetInnerText will probably overwrite ReadErrorNode anyway
			string it = GetInnerText(n);

			return !string.IsNullOrEmpty(it)
				? it
				: null;
		}
		#endregion

		#region ReadAttributeOpt
		/// <summary>Streams out the attribute data of <paramref name="name"/></summary>
		/// <param name="name">Attribute name</param>
		/// <returns></returns>
		protected override string ReadAttributeOpt(string name)
		{
			ValidateReadPermission();

			XmlNode n = Cursor.Attributes[name];
			if (n == null)
				return null;

			// attribute exists, update the error state with the node we're about to read from
			ReadErrorNode = n;

			return n.Value;
		}
		#endregion
	};
}