using System;
using System.Xml;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.IO
{
	partial class XmlElementStream
	{
		#region ReadElement impl
		protected override string GetInnerText(XmlElement n)
		{
			//return n.InnerText;

			var text_node = n.LastChild;
			if (text_node != null)
			{
				ReadErrorNode = text_node; // TODO: which is more informative, using the element (n) or text_node?
				// TextNode's actual text
				return text_node.Value;
			}

			return null;
		}
		#endregion

		#region ReadElement
		public override void ReadElementBegin(string name, out XmlElement oldCursor)
		{
			ValidateReadPermission();

			XmlElement n = Cursor[name];
			Contract.Assert(n != null, name);

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
			Contract.Assert(n != null, name);

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
			Contract.Assert(n != null, name);

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
			if (!string.IsNullOrEmpty(it))
				return it;

			return null;
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