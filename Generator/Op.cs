using System;
using System.Text;
using UnityEngine;

namespace Metric.Editor.Generator
{
	internal readonly struct Op : IEquatable<Op>
	{
		public static bool AddAnalysisIntoComments;
		public readonly Unit A, B, Res;
		public readonly bool Multiply;

		public Op(Unit res, Unit a, bool multiply, Unit b)
		{
			A = a;
			B = b;
			Res = res;
			Multiply = multiply;
			if (multiply && a.Fraction.ID > b.Fraction.ID) // to maintain fixed order
			{
				A = b;
				B = a;
			}
		}

		public void Add(StringBuilder sb)
		{
			int resVecSize = Mathf.Max(A.VecSize, B.VecSize);
			bool basic = Res == null || !Res.Fraction.HasUnit;

			string constr = "new";
			string resName;
			if (basic)
			{
				resName = resVecSize > 1 ? $"float{resVecSize}" : "float";
				constr = resVecSize > 1 ? $"new float{resVecSize}" : "";
			}
			else
			{
				resName = Res.Name;
				constr = $"new {Res.Name}";
			}

			string aVal = A.Fraction.HasUnit ? (A.VecSize > 1 ? ".v" : ".f") : "";
			string bVal = B.Fraction.HasUnit ? (B.VecSize > 1 ? ".v" : ".f") : "";

			string comment = "";
			if (AddAnalysisIntoComments & !basic)
			{
				string aDesc = A.Fraction.GetFractionalNotation();
				string bDesc = B.Fraction.GetFractionalNotation();
				string resDesc = Res.Fraction.GetFractionalNotation();
				comment = $"// ({aDesc}){(Multiply ? '*' : '/')}({bDesc}) = {resDesc}";
			}

			string varA = A.VarName ?? "a";
			string varB = B.VarName ?? "b";
			if (varA == varB)
			{
				varA = "a";
				varB = "b";
			}

			char op = Multiply ? '*' : '/';

			string beginAndReturn = $"\t\tpublic static {resName} operator";
			
			
			//	public static len3 operator *(float2 v, len3 len) => new len3(v * len.v); 
			// public static len3 operator *(len3 len, float2 v) => new len3(v * len.v);
			string signature    = $"{A.Name} {varA}, {B.Name} {varB}";
			string invSignature = $"{B.Name} {varB}, {A.Name} {varA}";
			if (signature == "float2 v, len3 len" || signature == "len3 len, float2 v")
			{
				Debug.Log($"WRONG  {A.Name}  {B.Name}");
			}
			
			string body = $"{varA}{aVal} {op} {varB}{bVal}";
	
			
			sb.AppendLine(
				$"{beginAndReturn} {op}({signature}) => {constr}({body}); {comment}");

			if (Multiply & A != B)
			{
				sb.AppendLine(
					$"{beginAndReturn} {op}({invSignature}) => {constr}({body});");
			}
		}

		public bool Equals(Op other)
		{
			return A == other.A & B == other.B & Multiply == other.Multiply;
		}

		public override bool Equals(object obj)
		{
			return obj is Op other && Equals(other);
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(A, B, Multiply);
		}
	}
}