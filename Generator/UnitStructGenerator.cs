using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Metric.Editor.Generator
{
	internal class UnitStructGenerator : UnitStructGeneratorLvl1
	{
		
		private void GenerateStruct(StringBuilder sb, Unit unit)
		{
			if (!unit.Fraction.HasUnit) return;
			var type = unit.VecSize > 1 ? $"float{unit.VecSize}" : "float";
			var valName = unit.VecSize > 1 ? 'v' : 'f';
			var notation = unit.Fraction.GetFractionalNotation();

			if (!unit.Summary.Contains(notation))
			{
				unit.Summary += $" {notation}";
			}
            
			sb.AppendLine($"\t/// <summary> {unit.Summary} </summary>");
			if (unit.VisibleInEditor) sb.AppendLine("\t[System.Serializable]");
			sb.AppendLine($"\tpublic struct {unit.Name} : System.IFormattable");
			sb.AppendLine("\t{");
			sb.AppendLine($"\t\tpublic {type} {valName};");

			var scalarName = Units[unit.Fraction.ScalarID].Name;
			string[] constructors;
			System.Span<char> components = stackalloc char[4] { 'x', 'y', 'z', 'w' };
			if (unit.VecSize > 1)
			{
				for (int i = 0; i < unit.VecSize; i++)
				{
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

			var toStrForm = "ToString(format, formatProvider)";
			if (unit.VecSize == 1)
			{
				sb.AppendLine($"\t\tpublic override string ToString() => string.Format(\"{{0}} {notation}\", {valName});");
				sb.AppendLine(
					$"\t\tpublic string ToString(string format, System.IFormatProvider formatProvider) => string.Format(\"{{0}} {notation}\", {valName}.{toStrForm});");
			}
			else
			{
				string formatNums = "{0}";
				string componentsSimple = $"{valName}.x";
				string componentsFormat = $"{valName}.x.{toStrForm}";
				for (int i = 1; i < unit.VecSize; i++)
				{
					formatNums += $", {{{i}}}";
					componentsSimple += $", {valName}.{components[i]}";
					componentsFormat += $", {valName}.{components[i]}.{toStrForm}";
				}
				sb.AppendLine($"\t\tpublic override string ToString() => string.Format(\"({formatNums}) {notation}\", {componentsSimple});");
				sb.AppendLine($"\t\tpublic string ToString(string format, System.IFormatProvider formatProvider) => string.Format(\"({formatNums}) {notation}\", {componentsFormat});");
			}

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

		
		public void GenerateUntilHaveChanges(System.Action func)
		{
			for (int i = 0; i < 10; i++)
			{
				int opsBefore = Ops.Count;
				int unitsBefore = Units.Count;
				int mathOpsBefore = MathOps.Count;

				func();

				if (opsBefore == Ops.Count &
				    unitsBefore == Units.Count &
				    mathOpsBefore == MathOps.Count) break;
			}
		}
	}
}