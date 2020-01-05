
using Vector2f = System.Numerics.Vector2;
using Vector3f = System.Numerics.Vector3;
using Vector4f = System.Numerics.Vector4;
using QuaternionF = System.Numerics.Quaternion;
using Plane3f = System.Numerics.Plane;
using Matrix4x4 = System.Numerics.Matrix4x4;

namespace KSoft
{
	public static partial class TypeExtensionsMath
	{
		#region Read Real types
		public static void Read(this IO.EndianReader s, out Vector2f v)
		{
			v = new Vector2f(
				s.ReadSingle(), // I
				s.ReadSingle()  // J
				);
		}
		public static void Read(this IO.EndianReader s, out Vector3f v)
		{
			v = new Vector3f(
				s.ReadSingle(), // I
				s.ReadSingle(), // J
				s.ReadSingle()  // K
				);
		}
		public static void Read(this IO.EndianReader s, out Vector4f v)
		{
			v = new Vector4f(
				s.ReadSingle(), // I
				s.ReadSingle(), // J
				s.ReadSingle(), // K
				s.ReadSingle()  // W
				);
		}

		public static void Read(this IO.EndianReader s, out QuaternionF v)
		{
			v = new QuaternionF(
				s.ReadSingle(), // I
				s.ReadSingle(), // J
				s.ReadSingle(), // K
				s.ReadSingle()  // W
				);
		}

		public static void Read(this IO.EndianReader s, out Plane3f v)
		{
			v = new Plane3f(
				s.ReadSingle(), // X
				s.ReadSingle(), // Y
				s.ReadSingle(), // Z
				s.ReadSingle()  // D
				);
		}
		#endregion

		#region Write Real types
		public static void Write(this IO.EndianWriter s, Vector2f v)
		{
			s.Write(v.X);
			s.Write(v.Y);
		}
		public static void Write(this IO.EndianWriter s, Vector3f v)
		{
			s.Write(v.X);
			s.Write(v.Y);
			s.Write(v.Z);
		}
		public static void Write(this IO.EndianWriter s, Vector4f v)
		{
			s.Write(v.X);
			s.Write(v.Y);
			s.Write(v.Z);
			s.Write(v.W);
		}

		public static void Write(this IO.EndianWriter s, QuaternionF v)
		{
			s.Write(v.X);
			s.Write(v.Y);
			s.Write(v.Z);
			s.Write(v.W);
		}

		public static void Write(this IO.EndianWriter s, Plane3f v)
		{
			s.Write(v.Normal);
			s.Write(v.D);
		}
		#endregion

		#region Stream Real types
		public static IO.EndianStream StreamV(this IO.EndianStream s, ref Vector2f value)
		{
				 if (s.IsReading) s.Reader.Read(out value);
			else if (s.IsWriting) s.Writer.Write(value);

			return s;
		}
		public static IO.EndianStream StreamV(this IO.EndianStream s, ref Vector3f value)
		{
				 if (s.IsReading) s.Reader.Read(out value);
			else if (s.IsWriting) s.Writer.Write(value);

			return s;
		}
		public static IO.EndianStream StreamV(this IO.EndianStream s, ref Vector4f value)
		{
				 if (s.IsReading) s.Reader.Read(out value);
			else if (s.IsWriting) s.Writer.Write(value);

			return s;
		}

		public static IO.EndianStream Stream(this IO.EndianStream s, ref QuaternionF value)
		{
				 if (s.IsReading) s.Reader.Read(out value);
			else if (s.IsWriting) s.Writer.Write(value);

			return s;
		}

		public static IO.EndianStream Stream(this IO.EndianStream s, ref Plane3f value)
		{
				 if (s.IsReading) s.Reader.Read(out value);
			else if (s.IsWriting) s.Writer.Write(value);

			return s;
		}
		#endregion

		#region SlimMath-style Matrix helpers
		/// <summary>Get first row in the matrix; that is M11, M12, M13, and M14</summary>
		public static Vector4f GetRow1(this Matrix4x4 m)
		{
			return new Vector4f(m.M11, m.M12, m.M13, m.M14);
		}
		/// <summary>Set first row in the matrix; that is M11, M12, M13, and M14</summary>
		public static void SetRow1(this Matrix4x4 m, Vector4f v)
		{
			m.M11 = v.X; m.M12 = v.Y; m.M13 = v.Z; m.M14 = v.W;
		}

		/// <summary>Get first row in the matrix; that is M21, M22, M23, and M24</summary>
		public static Vector4f GetRow2(this Matrix4x4 m)
		{
			return new Vector4f(m.M21, m.M22, m.M23, m.M24);
		}
		/// <summary>Set first row in the matrix; that is M21, M22, M23, and M24</summary>
		public static void SetRow2(this Matrix4x4 m, Vector4f v)
		{
			m.M21 = v.X; m.M22 = v.Y; m.M23 = v.Z; m.M24 = v.W;
		}

		/// <summary>Get first row in the matrix; that is M31, M32, M33, and M34</summary>
		public static Vector4f GetRow3(this Matrix4x4 m)
		{
			return new Vector4f(m.M31, m.M32, m.M33, m.M34);
		}
		/// <summary>Set first row in the matrix; that is M31, M32, M33, and M34</summary>
		public static void SetRow3(this Matrix4x4 m, Vector4f v)
		{
			m.M31 = v.X; m.M32 = v.Y; m.M33 = v.Z; m.M34 = v.W;
		}

		/// <summary>Get first row in the matrix; that is M41, M42, M43, and M44</summary>
		public static Vector4f GetRow4(this Matrix4x4 m)
		{
			return new Vector4f(m.M41, m.M42, m.M43, m.M44);
		}
		/// <summary>Set first row in the matrix; that is M41, M42, M43, and M44</summary>
		public static void SetRow4(this Matrix4x4 m, Vector4f v)
		{
			m.M41 = v.X; m.M42 = v.Y; m.M43 = v.Z; m.M44 = v.W;
		}

		// In case I ever need them...
#if false
		/// <summary>
		/// Gets or sets the first column in the matrix; that is M11, M21, M31, and M41.
		/// </summary>
		public Vector4 Column1
		{
			get { return new Vector4(M11, M21, M31, M41); }
			set { M11 = value.X; M21 = value.Y; M31 = value.Z; M41 = value.W; }
		}

		/// <summary>
		/// Gets or sets the second column in the matrix; that is M12, M22, M32, and M42.
		/// </summary>
		public Vector4 Column2
		{
			get { return new Vector4(M12, M22, M32, M42); }
			set { M12 = value.X; M22 = value.Y; M32 = value.Z; M42 = value.W; }
		}

		/// <summary>
		/// Gets or sets the third column in the matrix; that is M13, M23, M33, and M43.
		/// </summary>
		public Vector4 Column3
		{
			get { return new Vector4(M13, M23, M33, M43); }
			set { M13 = value.X; M23 = value.Y; M33 = value.Z; M43 = value.W; }
		}

		/// <summary>
		/// Gets or sets the fourth column in the matrix; that is M14, M24, M34, and M44.
		/// </summary>
		public Vector4 Column4
		{
			get { return new Vector4(M14, M24, M34, M44); }
			set { M14 = value.X; M24 = value.Y; M34 = value.Z; M44 = value.W; }
		}

		/// <summary>
		/// Gets or sets the translation of the matrix; that is M41, M42, and M43.
		/// </summary>
		public Vector3 TranslationVector
		{
			get { return new Vector3(M41, M42, M43); }
			set { M41 = value.X; M42 = value.Y; M43 = value.Z; }
		}
#endif

		/// <summary>Get the scale of the matrix; that is M11, M22, and M33</summary>
		public static Vector3f GetScaleVector(this Matrix4x4 m)
		{
			return new Vector3f(m.M11, m.M22, m.M33);
		}
		/// <summary>Set the scale of the matrix; that is M11, M22, and M33</summary>
		public static void SetScaleVector(this Matrix4x4 m, Vector3f v)
		{
			m.M11 = v.X; m.M22 = v.Y; m.M33 = v.Z;
		}
		#endregion
	};
}