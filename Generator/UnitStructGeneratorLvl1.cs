using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Metric.Editor.Generator
{
	internal class UnitStructGeneratorLvl1 : UnitStructGeneratorLvl0
	{
		private void GenerateStruct(StringBuilder sb, Unit unit)
		{
			if (!unit.Fraction.HasUnit) return;
			var type = unit.VecSize > 1 ? $"float{unit.VecSize}" : "float";
			var valName = unit.VecSize > 1 ? 'v' : 'f';

			if (!unit.Summary.Contains(unit.Fraction.GetDescription()))
			{
				unit.Summary += $" {unit.Fraction.GetDescription()}";
			}
            
			sb.AppendLine($"\t/// <summary> {unit.Summary} </summary>");
			if (unit.VisibleInEditor) sb.AppendLine("\t[System.Serializable]");
			sb.AppendLine($"\tpublic struct {unit.Name}");
			sb.AppendLine("\t{");
			sb.AppendLine($"\t\tpublic {type} {valName};");

			var scalarName = Units[unit.Fraction.ScalarID].Name;
			string[] constructors;
			if (unit.VecSize > 1)
			{
				System.Span<char> components = stackalloc char[4] { 'x', 'y', 'z', 'w' };
     
				for (int i = 0; i < unit.VecSize; i++)
				{
					//sb.AppendLine($"\t\tpublic {scalarName} {components[i]} => new(v.{components[i]});");
					sb.AppendLine($"\t\tpublic {scalarName} {components[i]} {{ get => new(v.{components[i]}); set => v.{components[i]} = value.f; }}");
				}

				string constrSign = $"{scalarName} _x";
				string constrBody = "_x.f";
				for (int i = 1; i < unit.VecSize; i++)
				{
					constrSign += $", {scalarName} _{components[i]}";
					constrBody += $", _{components[i]}.f";
				}
				constructors = new[]
				{
					$"{unit.Name}({type} _x) {{ {valName} = _x;",
					$"{unit.Name}({scalarName} _x) {{ {valName} = _x.f;",
					$"{unit.Name}({constrSign}) {{ {valName} = new {type}({constrBody});"
				};
			}
			else
			{
				constructors = new[]
				{
					$"{unit.Name}({type} _x) {{ {valName} = _x;",
				};
			}
            
			if (unit.VisibleInEditor)
			{
				sb.AppendLine("#if UNITY_EDITOR");
				sb.AppendLine("\t\tpublic int EditorData;");
				foreach (var constructor in constructors)
					sb.AppendLine($"\t\tpublic {constructor} EditorData = 0; }}");
                
				sb.AppendLine("#else");
				foreach (var constructor in constructors)
					sb.AppendLine($"\t\tpublic {constructor} }}");
				sb.AppendLine("#endif");
			}
			else
			{
				foreach (var constructor in constructors)
					sb.AppendLine($"\t\tpublic {constructor} }}");
			}

			if (unit.AddFields.Count > 0) sb.AppendLine();
			foreach (var conversion in unit.AddFields)
			{
				sb.AppendLine($"\t\tpublic {conversion}");
			}

			if (unit.Ops.Count > 0) sb.AppendLine();
			foreach (var unitOp in unit.Ops)
			{
				unitOp.Add(sb);
			}

			sb.AppendLine();
            
			//        public static kg operator -(kg a) => new(-a.f);
			sb.AppendLine($"\t\tpublic static {unit.Name} operator -({unit.Name} a) => new(-a.{valName});");
			sb.AppendLine($"\t\tpublic static {unit.Name} operator +({unit.Name} a, {unit.Name} b) => new(a.{valName} + b.{valName});");
			sb.AppendLine($"\t\tpublic static {unit.Name} operator -({unit.Name} a, {unit.Name} b) => new(a.{valName} - b.{valName});");
			if (unit.VecSize == 1)
			{
				sb.AppendLine($"\t\tpublic static {unit.Name} operator +(float a, {unit.Name} b) => new(a + b.{valName});");
				sb.AppendLine($"\t\tpublic static {unit.Name} operator -(float a, {unit.Name} b) => new(a - b.{valName});");
				sb.AppendLine($"\t\tpublic static {unit.Name} operator +({unit.Name} a, float b) => new(a.{valName} + b);");
				sb.AppendLine($"\t\tpublic static {unit.Name} operator -({unit.Name} a, float b) => new(a.{valName} - b);");
                
				sb.AppendLine($"\t\tpublic static bool operator ==({unit.Name} a, {unit.Name} b) => a.{valName} == b.{valName};");
				sb.AppendLine($"\t\tpublic static bool operator !=({unit.Name} a, {unit.Name} b) => a.{valName} != b.{valName};");
				sb.AppendLine($"\t\tpublic static bool operator <({unit.Name} a, {unit.Name} b) => a.{valName} < b.{valName};");
				sb.AppendLine($"\t\tpublic static bool operator >({unit.Name} a, {unit.Name} b) => a.{valName} > b.{valName};");
				sb.AppendLine($"\t\tpublic static bool operator <=({unit.Name} a, {unit.Name} b) => a.{valName} <= b.{valName};");
				sb.AppendLine($"\t\tpublic static bool operator >=({unit.Name} a, {unit.Name} b) => a.{valName} >= b.{valName};");
			}
			sb.AppendLine($"\t\tpublic static explicit operator {unit.Name}({type} x) => new(x);");
			sb.AppendLine($"\t\tpublic static explicit operator {type}({unit.Name} x) => x.{valName};");

			sb.AppendLine("\t}");
		}

		public void GenerateFile(StringBuilder sb, string nameSpace, IEnumerable<Unit> units)
		{
			sb.AppendLine($"namespace {nameSpace}");
			sb.AppendLine("{");
			Dictionary<string, Unit> checkNames = new Dictionary<string, Unit>();
			foreach (var unit in units)
			{
				if (!checkNames.TryAdd(unit.Name, unit))
				{
					Debug.LogError(
						$"{unit.Name} struct already exist with id:{unit.Fraction.ID}, you are trying to add id: {checkNames[unit.Name].Fraction.ID}");
					continue;
				}
				GenerateStruct(sb, unit);
				sb.AppendLine();
			}
			sb.AppendLine("}");
		}
        
		public void GenerateMathFile(StringBuilder sb, string nameSpace, string className)
		{
			sb.AppendLine($"namespace {nameSpace}");
			sb.AppendLine("{");
			sb.AppendLine($"\tpublic static class {className}");
			sb.AppendLine("\t{");
			foreach (var op in MathOps)
			{
				sb.AppendLine($"\t\tpublic static {op}");
			}
			sb.AppendLine("\t}");
			sb.AppendLine("}");
		}


		public bool AddOperation(string a,char op, string b)
		{
			var uA = GetUnitByName(a);
			var uB = GetUnitByName(b);
			if (uA == null)
			{
				Debug.LogError($"can't find unit {a}");
				return false;
			}
			if (uB == null)
			{
				Debug.LogError($"can't find unit {b}");
				return false;
			}
			return AddOp(uA, op, uB, null);
		}

        
        
		public void DistributeOperations()
		{
			foreach (var op in Ops)
			{
				var host = op.A.Fraction.Complexity> op.B.Fraction.Complexity ? op.B : op.A;
				if (op.A.VecSize != op.B.VecSize)
				{
					host = op.A.VecSize < op.B.VecSize ? op.B : op.A;
				}
				if (!op.A.Fraction.HasUnit) host = op.B;
				if (!op.B.Fraction.HasUnit) host = op.A;
				host.Ops.Add(op);
			}
			Ops.Clear();
		}

		public void GenerateCustomOperators(System.Func<UnitStructGeneratorLvl0, Unit, Unit, Fraction, bool> drop, IEnumerable<Unit> units)
		{
			IList<Unit> list = units as IList<Unit> ?? units.ToList();
			foreach (var a in list)
			{ 
				foreach (var b in list)
				{
					AddOp(a, '/', b, drop);
					AddOp(a, '*', b, drop);
				}
			}
		}


	}
}