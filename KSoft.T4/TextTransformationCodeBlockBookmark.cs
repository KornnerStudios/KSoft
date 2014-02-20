using System;

namespace KSoft.T4
{
	enum TextTransformationCodeBlockType
	{
		NoBrackets,
		Brackets,
	};
	struct TextTransformationCodeBlockBookmark : IDisposable
	{
		Microsoft.VisualStudio.TextTemplating.TextTransformation mFile;
		TextTransformationCodeBlockType mType;

		public TextTransformationCodeBlockBookmark(Microsoft.VisualStudio.TextTemplating.TextTransformation file,
			TextTransformationCodeBlockType type)
		{
			mFile = file;
			mType = type;
		}

		internal void Enter()
		{
			if (mType == TextTransformationCodeBlockType.Brackets)
				mFile.WriteLine("{");

			mFile.PushIndent("\t");
		}

		public void Dispose()
		{
			mFile.PopIndent();

			if (mType == TextTransformationCodeBlockType.Brackets)
				mFile.WriteLine("}");
		}
	};
}