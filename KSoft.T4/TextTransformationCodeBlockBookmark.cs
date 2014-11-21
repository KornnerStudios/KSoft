using System;

namespace KSoft.T4
{
	enum TextTransformationCodeBlockType
	{
		NoBrackets,
		Brackets,
		/// <summary>Mainly for a class definition statement</summary>
		BracketsStatement,
	};
	struct TextTransformationCodeBlockBookmark
		: IDisposable
	{
		internal const string kIndent = "\t";

		readonly Microsoft.VisualStudio.TextTemplating.TextTransformation mFile;
		readonly TextTransformationCodeBlockType mType;
		readonly int mIndentCount;

		public TextTransformationCodeBlockBookmark(Microsoft.VisualStudio.TextTemplating.TextTransformation file,
			TextTransformationCodeBlockType type, int indentCount = 1)
		{
			mFile = file;
			mType = type;
			mIndentCount = indentCount;
		}

		internal void Enter()
		{
			if (mType == TextTransformationCodeBlockType.Brackets ||
				mType == TextTransformationCodeBlockType.BracketsStatement)
				mFile.WriteLine("{");

			for (int x = 0; x < mIndentCount; x++)
				mFile.PushIndent(kIndent);
		}

		public void Dispose()
		{
			for (int x = 0; x < mIndentCount; x++)
				mFile.PopIndent();

			if (mType == TextTransformationCodeBlockType.Brackets)
				mFile.WriteLine("}");
			else if (mType == TextTransformationCodeBlockType.BracketsStatement)
				mFile.WriteLine("};");
		}
	};
}