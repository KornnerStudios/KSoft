using System;
using Debug = System.Diagnostics.Debug;
using Reflect = System.Reflection;
using TextTemplating = Microsoft.VisualStudio.TextTemplating;

namespace KSoft.T4.Values
{
	public enum BittableHandleKind
	{
		/// <summary>Handle is otherwise opaque</summary>
		Undefined,
		/// <summary>Handle represents a value where NONE (-1) is the 'invalid' sentinel</summary>
		Noneable,
		/// <summary>Handle represents a value where NULL (0) is the 'invalid' sentinel</summary>
		Nullable,
		/// <summary>Handle is a wrapper for a void* value</summary>
		IntPtr,
	};

	public sealed class BittableHandleGenerator
	{
		#region Constants
		const string kNoneValueCodeName = "KSoft.TypeExtensions.kNone";
		const string kNoneFieldName = "None";

		const string kNullFieldName = "Null";
		#endregion

		readonly TextTemplating.TextTransformation mFile;
		readonly string mUnderlyingTypeName;
		readonly string mStructName;

		public string BackingFieldName { get; set; }

		/// <summary>Is this handle only internally accessed, or is it public (default value)?</summary>
		public bool IsInternal { get; set; }
		/// <summary>Should the type decl include a 'partial' specification? (default is true)</summary>
		public bool IsPartial { get; set; }

		public BittableHandleKind Kind { get; set; }

		public bool SupportsIComparable { get; set; }
		public bool SupportsIEquatable { get; set; }

		public string CtorFromValueTypeName { get; set; }
		public string CtorFromValueParamName { get; set; }

		#region Ctors
		BittableHandleGenerator()
		{
			BackingFieldName = "mValue";

			IsPartial = true;

			Kind = BittableHandleKind.Undefined;

			SupportsIComparable = true;
			SupportsIEquatable = true;

			CtorFromValueTypeName = null;
			CtorFromValueParamName = "value";
		}
		public BittableHandleGenerator(TextTemplating.TextTransformation ttFile, 
			string underlyingTypeName, string structName)
			: this()
		{
			mFile = ttFile;
			mUnderlyingTypeName = underlyingTypeName;
			mStructName = structName;
		}
		public BittableHandleGenerator(TextTemplating.TextTransformation ttFile, 
			NumberCodeDefinition underlyingType, string structName)
			: this(ttFile, underlyingType.Code.ToString(), structName)
		{
		}

		public static BittableHandleGenerator ForIntPtr(TextTemplating.TextTransformation ttFile,
			string structName)
		{
			return new BittableHandleGenerator(ttFile, "IntPtr", structName)
			{
				Kind = BittableHandleKind.IntPtr,

				SupportsIComparable = false,

				CtorFromValueTypeName = "IntPtr",
				CtorFromValueParamName = "pointer",
			};
		}
		#endregion

		#region Generate type declaration
		void WriteDeclAttributes()
		{
			// TODO
		}
		void WriteDeclHeader()
		{
			string visibility = IsInternal
				? "internal"
				: "public";

			string partial = IsPartial
				? "partial "
				: "";

			mFile.WriteLine("{0} {1} struct {2}", visibility, partial, mStructName);
		}
		#region GenerateDeclInheritance
		// Curiously recurring template pattern
		string BuildDeclInheritanceTextCRTP(string genericClassName)
		{
			return string.Format("{0}<{1}>", genericClassName, mStructName);
		}
		int WriteDeclInheritanceEntry(int position, bool entryExists, string entryText)
		{
			if (entryExists)
			{
				string prefix = position > 0
					? ","
					: ":";

				mFile.WriteLine("{0} {1}", prefix, entryText);
				position++;
			}

			return position;
		}
		void WriteDeclInheritance()
		{
			int entry_pos = 0;
			// indent the inheritance entries +1 more than the type decl (ie, public struct...)
			using (mFile.EnterCodeBlock())
			{
				entry_pos = WriteDeclInheritanceEntry(entry_pos, SupportsIComparable,
					BuildDeclInheritanceTextCRTP("IComparable"));

				entry_pos = WriteDeclInheritanceEntry(entry_pos, SupportsIEquatable,
					BuildDeclInheritanceTextCRTP("IEquatable"));
			}
		}
		#endregion
		public void GenerateDecl()
		{
			// indent the decl by 1. it is assumed the decl is nested in a single namespace declaration (like this codegen class)
			using (mFile.EnterCodeBlock())
			{
				WriteDeclAttributes();
				WriteDeclHeader();
				WriteDeclInheritance();
			}
		}
		#endregion

		public void GenerateBackingFieldDecl()
		{
			mFile.WriteLine("{0} {1};",
				mUnderlyingTypeName, BackingFieldName);

			mFile.NewLine();
		}

		// Use non-default CtorFromValue... inputs when the 'value' is of a different bit size than the underlying type
		// eg, underlying type is Int16, but we construct from Int32 inputs
		public void GenerateCtorFromValueMethod()
		{
			if (CtorFromValueTypeName == null)
				CtorFromValueTypeName = mUnderlyingTypeName;

			mFile.WriteLine("public {0}({1} {2})",
				mStructName, CtorFromValueTypeName, CtorFromValueParamName);

			using (mFile.EnterCodeBlock(TextTransformationCodeBlockType.Brackets))
			{
				mFile.WriteLine("{0} = ({1}){2};",
					BackingFieldName, mUnderlyingTypeName, CtorFromValueParamName);
			}
			mFile.NewLine();
		}

		#region Generate cast to and from methods
		void WriteCastToUnderlyingTypeMethod()
		{
			const string k_param_name = "handle";

			mFile.WriteLine("public static implicit operator {0}({1} {2})",
				mUnderlyingTypeName, mStructName, k_param_name);

			using (mFile.EnterCodeBlock(TextTransformationCodeBlockType.Brackets))
			{
				mFile.WriteLine("return {0}.{1}",
					k_param_name, BackingFieldName);
			}
		}
		void WriteCastFromUnderlyingTypeMethod()
		{
			const string k_param_name = "value";

			mFile.WriteLine("public static implicit operator {0}({1} {2})",
				mStructName, mUnderlyingTypeName, k_param_name);

			using (mFile.EnterCodeBlock(TextTransformationCodeBlockType.Brackets))
			{
				mFile.WriteLine("return new {0}({1});",
					mStructName, k_param_name);
			}
		}
		public void GenerateStockCastMethods()
		{
			WriteCastToUnderlyingTypeMethod();
			WriteCastFromUnderlyingTypeMethod();
			mFile.NewLine();
		}
		#endregion

		#region Generate equality methods
		void WriteEqualityOperatorMethod(string operatorSymbol)
		{
			Debug.Assert(SupportsIEquatable,
				"Trying to generate handle without IEquatable support, but with equality operators (this is non-optimal)",
				"in {0}",
				mStructName);

			mFile.WriteLine("public static bool operator {0}({1} x, {1} y)",
				operatorSymbol, mStructName);

			using (mFile.EnterCodeBlock(TextTransformationCodeBlockType.Brackets))
			{
				mFile.WriteLine("return x.Equals(y) {0} true",
					operatorSymbol);
			}
		}
		public void GenerateEqualityOperatorMethods()
		{
			WriteEqualityOperatorMethod("==");
			WriteEqualityOperatorMethod("!=");
			mFile.NewLine();
		}
		#endregion

		#region Generate NONE utilities
		void WriteNoneField(string fieldName = kNoneFieldName)
		{
			mFile.WriteLine("public static readonly {0} {1} = new {0}({2});",
				mStructName, fieldName, kNoneValueCodeName);
		}
		void WriteNoneableProperties()
		{
			mFile.WriteLine("public bool IsNone { get { return {0} == {1}; } }",
				BackingFieldName, kNoneValueCodeName);

			mFile.WriteLine("public bool IsNotNone { get { return {0} != {1}; } }",
				BackingFieldName, kNoneValueCodeName);
		}
		public void GenereateNoneableMembers(string noneFieldName = kNoneFieldName)
		{
			if (Kind == BittableHandleKind.Noneable)
			{
				WriteNoneField(noneFieldName);
				mFile.NewLine();

				WriteNoneableProperties();
				mFile.NewLine();
			}
		}
		#endregion

		#region Generate NULL utilities
		void WriteNullField(string fieldName = kNullFieldName)
		{
			mFile.WriteLine("public static readonly {0} {1} = new {0}( 0 );",
				mStructName, fieldName);
		}
		void WriteNullableProperties()
		{
			mFile.WriteLine("public bool IsNull { get { return {0} == 0; } }",
				BackingFieldName);

			mFile.WriteLine("public bool IsNotNull { get { return {0} != 0; } }",
				BackingFieldName);
		}
		public void GenereateNullableMembers(string nullFieldName = kNullFieldName)
		{
			if (Kind == BittableHandleKind.Nullable)
			{
				WriteNullField(nullFieldName);
				mFile.NewLine();

				WriteNullableProperties();
				mFile.NewLine();
			}
		}
		#endregion

		#region Generate object overrides
		void WriteGetHashCodeOverride()
		{
			mFile.WriteLine("public override int GetHashCode()");

			using (mFile.EnterCodeBlock(TextTransformationCodeBlockType.Brackets))
			{
				mFile.WriteLine("{0}.GetHashCode();",
					BackingFieldName);
			}
		}
		void WriteEqualsOverride()
		{
			mFile.WriteLine("public override bool Equals(object obj)");

			using (mFile.EnterCodeBlock(TextTransformationCodeBlockType.Brackets))
			{
				mFile.WriteLine("if (!(obj is {0}))",
					mStructName);
				using (mFile.EnterCodeBlock(TextTransformationCodeBlockType.Brackets))
				{
					mFile.WriteLine("return false;");
				}

				mFile.WriteLine("return Equals( ({0})obj );",
					mStructName);
			}
		}
		public void GenerateObjectMethodOverrides()
		{
			WriteGetHashCodeOverride();
			mFile.NewLine();

			WriteEqualsOverride();
			mFile.NewLine();
		}
		#endregion

		#region Generate Interface impls
		#region IComparable<struct> Members
		void GenerateIComparableImpl()
		{
			const string k_interface_name = "IComparable";

			mFile.WriteLine("#region {0}<{1}> Members",
				k_interface_name, mStructName);
			{
				const string k_return_type = "int";

				mFile.WriteLine("public {0} {1}({2} other)",
					k_return_type, "ComapreTo", mStructName);

				using (mFile.EnterCodeBlock(TextTransformationCodeBlockType.Brackets))
				{
					mFile.WriteLine("return ({0})({1} - other.{1});",
						k_return_type, BackingFieldName);
				}
			}
			mFile.WriteLine("#endregion");
		}
		#endregion

		#region IEquatable<struct> Members
		void GenerateIEquatableImpl()
		{
			const string k_interface_name = "IEquatable";

			mFile.WriteLine("#region {0}<{1}> Members",
				k_interface_name, mStructName);
			{
				const string k_return_type = "bool";

				mFile.WriteLine("public {0} {1}({2} other)",
					k_return_type, "Equals", mStructName);

				using (mFile.EnterCodeBlock(TextTransformationCodeBlockType.Brackets))
				{
					mFile.WriteLine("return {0} == other.{0};",
						BackingFieldName);
				}
			}
			mFile.WriteLine("#endregion");
		}
		#endregion

		public void GenerateSupportedInterfaceImplementations()
		{
			if (SupportsIComparable)
			{
				GenerateIComparableImpl();
				mFile.NewLine();
			}

			if (SupportsIEquatable)
			{
				GenerateIEquatableImpl();
				mFile.NewLine();
			}
		}
		#endregion

		public void GenerateDefaultBody()
		{
			using (mFile.EnterCodeBlock(TextTransformationCodeBlockType.BracketsStatement))
			{
				switch (Kind)
				{
				case BittableHandleKind.Noneable:
					WriteNoneField();
					break;
				case BittableHandleKind.Nullable:
					WriteNullField();
					break;
				}
				if (Kind != BittableHandleKind.Undefined)
					mFile.NewLine();

				GenerateBackingFieldDecl();

				GenerateCtorFromValueMethod();

				GenerateStockCastMethods();
				GenerateEqualityOperatorMethods();

				switch (Kind)
				{
				case BittableHandleKind.Noneable:
					WriteNoneableProperties();
					break;
				case BittableHandleKind.Nullable:
					WriteNullableProperties();
					break;
				}
				if (Kind != BittableHandleKind.Undefined)
					mFile.NewLine();

				GenerateObjectMethodOverrides();

				GenerateSupportedInterfaceImplementations();
			}
		}

		public void GenerateDefinition()
		{
			GenerateDecl();
			GenerateDefaultBody();
		}
	};
}