
using Vector2f = SlimMath.Vector2;
using Vector3f = SlimMath.Vector3;
using Vector4f = SlimMath.Vector4;
using QuaternionF = SlimMath.Quaternion;
using Plane3f = SlimMath.Plane;

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
	};
}