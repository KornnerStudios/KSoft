using System;
using System.Xml;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.IO
{
	partial class XmlElementStream
	{
		protected override void AppendElement(XmlElement e)
		{
			// if there is a node in scope, add the element after it and use it as the new scope
			if(Cursor != null)
				Cursor.AppendChild(e);
			else // if there is no XML node in scope, assume we're adding to the root
				Document.AppendChild(e);
		}

		protected override void NestElement(XmlElement e, out XmlElement oldCursor)
		{
			oldCursor = null;

			if (Cursor != null)
			{
				Cursor.AppendChild(e);

				oldCursor = Cursor;
				Cursor = e;
			}
			else // if there is no XML node in scope, assume we're adding to the root
			{
				Document.DocumentElement.AppendChild(e);
				Cursor = e;
			}
		}

		#region WriteElement impl
		protected override void WriteElement(XmlElement n, string value)
		{
			n.InnerText = value;

			//var text = m_root.CreateTextNode(value);
			//n.AppendChild(text);
		}
		#endregion

		#region WriteElement
		protected override XmlElement WriteElementAppend(string name)
		{
			ValidateWritePermission();

			XmlElement e = Document.CreateElement(name);
			AppendElement(e);

			return e;
		}

		protected override XmlElement WriteElementNest(string name, out XmlElement oldCursor)
		{
			ValidateWritePermission();

			XmlElement e = Document.CreateElement(name);
			NestElement(e, out oldCursor);

			return e;
		}
		#endregion

		#region WriteAttribute
		protected override void CursorWriteAttribute(string name, string value)
		{
			ValidateWritePermission();

			Cursor.SetAttribute(name, value);
		}
		#endregion

		protected override void WriteCommentImpl(string comment)
		{
			if (!string.IsNullOrEmpty(comment))
				Cursor.AppendChild(Document.CreateComment(comment));
		}
	};
}