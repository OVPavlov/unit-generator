using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Metric.Editor.Generator
{
	[CreateAssetMenu(menuName = "Unit Generator", order = 90, fileName = "UnitGenerator")]
	internal class UnitGeneratorObject : ScriptableObject
	{
		public enum OpType
		{
			Multiply,
			Divide
		}

		[System.Serializable]
		public struct CustomOperation
		{
			public string A;
			public OpType Op;
			public string B;
		}

		public string NameSpace = "Units";
		public string MathClassName = "MathU";
		public bool AddAnalysisIntoComments;

		[Space(16)] public BasicUnitFilter BasicUnitFilter;
		public GenerationBlock[] GenerationBlocks;
		public CustomOperation[] CustomOperations;
		public CustomUnit[] CustomUnits;
		public CustomUnitPermutation[] CustomUnitPermutations;
		public GenerationBlock[] GenerationBlocksFinal;
		[Space]
		public UnitEditorDescriptor[] UnitEditors = UnitEditorDescriptor.GetDefaults();
		
		[Space(32)] public bool GenerateButton;
		public bool DryRun, SaveInTestDir;

		public void OnValidate()
		{
			if (GenerateButton)
			{
				GenerateButton = false;
				GenerateAll();
			}
		}

		public void GenerateAll()
		{
			Op.AddAnalysisIntoComments = AddAnalysisIntoComments;
			var so = CreateInstance<UnitGeneratorObject>();
			string soPath = AssetDatabase.GetAssetPath(this);

			DestroyImmediate(so);

			string directory = Path.GetDirectoryName(soPath);
			string generatedDirectory = Path.Combine(directory, SaveInTestDir ? "___test" : "generated");
			string unitsDir = Path.Combine(generatedDirectory, "units");
			string editorDir = Path.Combine(generatedDirectory, "Editor");
			string nameSpace = SaveInTestDir ? "___units_test_run" : NameSpace;
			Directory.CreateDirectory(unitsDir);
			Directory.CreateDirectory(editorDir);

			string disableWarnings = @"// ReSharper disable InconsistentNaming
// ReSharper disable IdentifierTypo
// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable CompareOfFloatsByEqualityOperator
// ReSharper disable MemberCanBePrivate.Global
#pragma warning disable CS0660, CS0661
";



			var gen = GenerateUnits();
			var scalars = gen.GetUnits(u => u.VecSize == 1);
			var vectors = gen.GetUnits(u => u.VecSize > 1);


			void WriteFile(IEnumerable<Unit> units, string name)
			{
				var sb = new StringBuilder($"using Unity.Mathematics;\n{disableWarnings}");
				gen.GenerateFile(sb, nameSpace, units);
				if (!DryRun) File.WriteAllText(Path.Combine(unitsDir, $"{name}.cs"), sb.ToString());
			}

			void WriteMathFile(UnitStructGenerator gen, string name)
			{
				var sb = new StringBuilder(disableWarnings);
				gen.GenerateMathFile(sb, nameSpace, MathClassName);
				if (!DryRun) File.WriteAllText(Path.Combine(unitsDir, $"{name}.cs"), sb.ToString());
			}

			GenerateEditors(gen, editorDir);
			
			WriteFile(scalars.Where(u => u.Tag == Tag.Base), "base");
			WriteFile(scalars.Where(u => u.Tag == Tag.Special), "special");
			WriteFile(scalars.Where(u => u.Tag == Tag.Coherent), "coherent");
			WriteFile(scalars.Where(u => u.Tag == Tag.DerivedFromSpecial), "derived_from_special");
			WriteFile(scalars.Where(u => u.Tag == Tag.AutoDerived), "auto_derived");
			WriteFile(vectors, "vectors");

			WriteMathFile(gen, "metric");

			GenerateAssemblyDefinition(unitsDir, $"{nameSpace}Assembly");
			
			AssetDatabase.Refresh();
			Debug.Log("Unit structs generated successfully.");
		}

		private void GenerateEditors(UnitStructGenerator gen, string directory)
		{
			var sb = new StringBuilder("using UnityEngine;\n\nnamespace Units.Editor\n{");
			foreach (var editorDescriptor in UnitEditors)
			{
				var unit = gen.GetUnitByName(editorDescriptor.unit);
				if (unit == null)
				{
					Debug.LogError($"Error can't find {editorDescriptor.unit}");
					continue;
				}
				unit.VisibleInEditor = true;
				sb.AppendLine(editorDescriptor.Generate());
			}

			sb.AppendLine("}");
			if (!DryRun) File.WriteAllText(Path.Combine(directory, $"units_editor.cs"), sb.ToString());
		}

		private void GenerateAssemblyDefinition(string directory, string name)
		{
			string asmdefPath = Path.Combine(directory, $"_{name.ToLower()}.asmdef");

			string asmdefContent = $@"{{
    ""name"": ""{name}"",
    ""rootNamespace"": """",
    ""references"": [
        ""Unity.Mathematics""
    ],
    ""includePlatforms"": [],
    ""excludePlatforms"": [],
    ""allowUnsafeCode"": false,
    ""overrideReferences"": true,
    ""precompiledReferences"": [],
    ""autoReferenced"": true,
    ""defineConstraints"": [],
    ""versionDefines"": [],
    ""noEngineReferences"": true
}}";

			if (!DryRun) File.WriteAllText(asmdefPath, asmdefContent);
			if (!DryRun) AssetDatabase.Refresh();
		}

		private UnitStructGenerator GenerateUnits()
		{
			var gen = new UnitStructGenerator();


			BasicUnitFilter.FilterAndAdd(gen, UnitCollection.SiBaseUnits());
			BasicUnitFilter.FilterAndAdd(gen, UnitCollection.SiDerivedUnits());
			BasicUnitFilter.FilterAndAdd(gen, UnitCollection.CoherentDerivedUnits());
			BasicUnitFilter.FilterAndAdd(gen, UnitCollection.DerivedUnitsWithSpecialNames());
			BasicUnitFilter.FilterAndAdd(gen, UnitCollection.NonSiUnits());
			BasicUnitFilter.FilterAndAdd(gen, UnitCollection.Vectors());
			gen.AddUnits(CustomUnits.Select(u => u.GetUnit()).ToList());

			for (var i = 0; i < GenerationBlocks.Length; i++)
			{
				Debug.Log($"#####  Executing Block [{i}] '{GenerationBlocks[i].name}' #####");
				GenerationBlocks[i].GenerateOperators(gen);
			}

			
			AddCustomOps(gen);
			
			foreach (var permutation in CustomUnitPermutations)
			{
				permutation.Permutation(gen);
			}

			
			using (var _ = new BeforeAndAfter("Generating MathOps all", gen))
			{
				gen.GenerateUntilHaveChanges(() => gen.GenerateMathOps());
			}

			for (var i = 0; i < GenerationBlocksFinal.Length; i++)
			{
				Debug.Log($"#####  Executing  Final Block [{i}] '{GenerationBlocksFinal[i].name}'  #####");
				GenerationBlocksFinal[i].GenerateOperators(gen);
			}

			gen.DistributeOperations();
			return gen;
		}

		private void AddCustomOps(UnitStructGenerator gen)
		{
			using (var _ = new BeforeAndAfter("AddCustomOps", gen))
			{
				foreach (var cOp in CustomOperations)
				{
					gen.AddOperation(cOp.A, cOp.Op == OpType.Multiply ? '*' : '/', cOp.B);
				}

				// Generate inverse of base units
				foreach (var unit in gen.GetUnits(Tag.Base))
				{
					if (!Filter.HasOnly(unit.Fraction, BaseUnits.SI) || !unit.IsFundamental || unit.VecSize > 1) continue;
					var frac = new Fraction(1, unit.Fraction, (_, i) => -i);
					if (gen.Units.ContainsKey(frac.ID)) continue;
					gen.AddUnit(new() { Tag = Tag.DerivedFromSpecial, Fraction = frac });
					gen.AddOp(null, '/', unit, null);
				}
				
				foreach (var unit in gen.Units.Values.ToList())
				{
					gen.AddOp(null, '*', unit, null);
				}
			}
		}

		
	}
}
