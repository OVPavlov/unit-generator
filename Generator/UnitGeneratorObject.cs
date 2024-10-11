using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Metric.Editor.Generator
{
	[CreateAssetMenu]
	public class UnitGeneratorObject : ScriptableObject
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

		[System.Serializable]
		public class CustomUnit
		{
			public Element[] Fraction;
			public int VecSize = 1;
			public string Name;
			public string Description;
			public string VarName;
			public bool VisibleInEditor;
			public Tag Tag;
			public string[] AddFields;

			[System.Serializable]
			public struct Element
			{
				public string unit;
				public int power;
			}

			internal Unit GetUnit()
			{
				var frac = new Fraction(VecSize);
				foreach (var element in Fraction)
				{
					frac.Add(element.unit, element.power);
				}

				return new Unit
				{
					VisibleInEditor = VisibleInEditor,
					Tag = Tag,
					Name = Name,
					Summary = Description,
					VarName = VarName,
					AddFields = new List<string>(AddFields),
					Fraction = frac
				};
			}
		}

		public string NameSpace = "Metric";
		public string MathClassName = "metric";
		public bool AddAnalysisIntoComments;

		[Space(16)] public BasicUnitFilter BasicUnitFilter;
		public Block[] GenerationBlocks;
		public CustomOperation[] CustomOperations;
		public CustomUnit[] CustomUnits;
		public CustomUnitPermutation[] CustomUnitPermutations;
		public bool generateOperationsForAllUnits;
		public Block[] GenerationBlocksFinal;
		[Space]
		public UnitEditorDescriptor[] UnitEditors = UnitEditorDescriptor.Defaults;
		
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


			BasicUnitFilter.FilterAndAdd(gen, SiBaseUnits());
			BasicUnitFilter.FilterAndAdd(gen, SiDerivedUnits());
			BasicUnitFilter.FilterAndAdd(gen, CoherentDerivedUnits());
			BasicUnitFilter.FilterAndAdd(gen, DerivedUnitsWithSpecialNames());
			BasicUnitFilter.FilterAndAdd(gen, NonSiUnits());
			BasicUnitFilter.FilterAndAdd(gen, Vectors());
			gen.AddUnits(CustomUnits.Select(u => u.GetUnit()).ToList());

			for (var i = 0; i < GenerationBlocks.Length; i++)
			{
				Debug.Log($"#####  Executing Block [{i}]  #####");
				GenerationBlocks[i].GenerateOperators(gen);
			}


			int opsBefore = gen.Ops.Count;
			int unitsBefore = gen.Units.Count;
			AddCustomOps(gen);
			Debug.Log(
				$"AddCustomOps(gen) :\t {opsBefore} => {gen.Ops.Count} (+{gen.Ops.Count - opsBefore})ops;  \t{unitsBefore} => {gen.Units.Count} (+{gen.Units.Count - unitsBefore})units");



			Debug.Log($"Generating  MathOps all");
			gen.GenerateUntilHaveChanges(() => gen.GenerateMathOps());


			if (generateOperationsForAllUnits)
			{
				Debug.Log($"Generating only operations");

				bool dropIfDoesntExist(Unit a, Unit b, Fraction frac)
				{
					if (frac.ID == null) return false;
					return !gen.Units.ContainsKey(frac.ID);
				}

				gen.GenerateCustomOperators(dropIfDoesntExist, Filter.ByBaseUnits(gen.Units.Values, ~BaseUnits.rad));
			}

			for (var i = 0; i < GenerationBlocksFinal.Length; i++)
			{
				Debug.Log($"#####  Executing Block [{i}]  #####");
				GenerationBlocksFinal[i].GenerateOperators(gen);
			}



			gen.DistributeOperations();
			return gen;
		}

		private void AddCustomOps(UnitStructGenerator gen)
		{
			foreach (var cOp in CustomOperations)
			{
				gen.AddOperation(cOp.A, cOp.Op == OpType.Multiply ? '*' : '/', cOp.B);
			}

			// Generate inverse of base units
			foreach (var unit in gen.GetUnits(Tag.Base | Tag.Coherent | Tag.Special | Tag.DerivedFromSpecial))
			{
				if (!unit.Fraction.IsSI) continue;
				var frac = new Fraction(1) { { unit.Fraction.Dict.First().Key, -1 } };
				if (gen.Units.ContainsKey(frac.ID)) continue;
				gen.AddUnit(new() { Tag = Tag.DerivedFromSpecial, Fraction = frac });
				gen.AddOp(null, '/', unit, null);
			}

			foreach (var unit in gen.Units.Values)
			{
				//if(!unit.Fraction.IsSI)continue;
				gen.AddOp(null, '*', unit, null);
			}

			foreach (var permutation in CustomUnitPermutations)
			{
				permutation.Permutation(gen);
			}
		}


		#region Units

		private static List<Unit> SiBaseUnits()
		{
			return new List<Unit>
			{
				new()
				{
					Tag = Tag.Base,
					Name = "s",
					Summary = "Second",
					VarName = "time",
					Fraction = new Fraction(1) { { "s", 1 } }
				},
				new()
				{
					Tag = Tag.Base,
					Name = "kg",
					Summary = "kilogram",
					VarName = "mass",
					AddFields = new List<string>
					{
						"static kg fromG(float grams) => new(grams * 1e-3f);",
						"static kg fromT(float tons) => new(tons * 1e+3f);"
					},
					Fraction = new Fraction(1) { { "kg", 1 } }
				},
				new()
				{
					Tag = Tag.Base,
					Name = "m",
					Summary = "meter",
					VarName = "length",
					AddFields = new List<string>
					{
						"static m fromMm(float millimeters) => new(millimeters * 1e-3f);",
						"static m fromKm(float kilometers) => new(kilometers * 1e+3f);"
					},
					Fraction = new Fraction(1) { { "m", 1 } }
				},
				new()
				{
					Tag = Tag.Base,
					Name = "A",
					Summary = "Ampere",
					VarName = "electric_current",
					Fraction = new Fraction(1) { { "A", 1 } }
				},
				new()
				{
					Tag = Tag.Base,
					Name = "K",
					Summary = "Kelvin",
					VarName = "thermodynamic_temperature",
					Fraction = new Fraction(1) { { "K", 1 } }
				},
				new()
				{
					Tag = Tag.Base,
					Name = "mol",
					Summary = "Mole",
					VarName = "amount_of_substance",
					Fraction = new Fraction(1) { { "mol", 1 } }
				},
				new()
				{
					Tag = Tag.Base,
					Name = "cd",
					Summary = "Candela",
					VarName = "luminous_intensity",
					Fraction = new Fraction(1) { { "cd", 1 } }
				}
			};
		}

		private static List<Unit> SiDerivedUnits()
		{
			return new List<Unit>
			{
				new()
				{
					Tag = Tag.Special,
					Name = "Hz",
					Summary = "Hertz: frequency",
					VarName = "frequency",
					Fraction = new Fraction(1) { { "s", -1 } }
				},
				new()
				{
					Tag = Tag.Special,
					Name = "N",
					Summary = "Newton: force",
					VarName = "force",
					Fraction = new Fraction(1) { { "kg", 1 }, { "m", 1 }, { "s", -2 } }
				},
				new()
				{
					Tag = Tag.Special,
					Name = "Pa",
					Summary = "Pascal: pressure, stress",
					VarName = "pressure",
					Fraction = new Fraction(1) { { "kg", 1 }, { "m", -1 }, { "s", -2 } }
				},
				new()
				{
					Tag = Tag.Special,
					Name = "J",
					Summary = "Joule: energy, work, amount of heat",
					VarName = "energy",
					Fraction = new Fraction(1) { { "kg", 1 }, { "m", 2 }, { "s", -2 } }
				},
				new()
				{
					Tag = Tag.Special,
					Name = "W",
					Summary = "Watt: power, radiant flux",
					VarName = "power",
					Fraction = new Fraction(1) { { "kg", 1 }, { "m", 2 }, { "s", -3 } }
				},
				new()
				{
					Tag = Tag.Special,
					Name = "C",
					Summary = "Coulomb: electric charge",
					VarName = "electricCharge",
					Fraction = new Fraction(1) { { "s", 1 }, { "A", 1 } }
				},
				new()
				{
					Tag = Tag.Special,
					Name = "V",
					Summary = "Volt: electric potential, voltage",
					VarName = "voltage",
					Fraction = new Fraction(1) { { "kg", 1 }, { "m", 2 }, { "s", -3 }, { "A", -1 } }
				},
				new()
				{
					Tag = Tag.Special,
					Name = "F",
					Summary = "Farad: capacitance",
					VarName = "capacitance",
					Fraction = new Fraction(1) { { "kg", -1 }, { "m", -2 }, { "s", 4 }, { "A", 2 } }
				},
				new()
				{
					Tag = Tag.Special,
					Name = "Ohm",
					Summary = "Ohm: resistance",
					VarName = "resistance",
					Fraction = new Fraction(1) { { "kg", 1 }, { "m", 2 }, { "s", -3 }, { "A", -2 } }
				},
				new()
				{
					Tag = Tag.Special,
					Name = "S",
					Summary = "Siemens: electrical conductance",
					VarName = "conductance",
					Fraction = new Fraction(1) { { "kg", -1 }, { "m", -2 }, { "s", 3 }, { "A", 2 } }
				},
				new()
				{
					Tag = Tag.Special,
					Name = "Wb",
					Summary = "Weber: magnetic flux",
					VarName = "magneticFlux",
					Fraction = new Fraction(1) { { "kg", 1 }, { "m", 2 }, { "s", -2 }, { "A", -1 } }
				},
				new()
				{
					Tag = Tag.Special,
					Name = "T",
					Summary = "Tesla: magnetic flux density",
					VarName = "magneticFluxDensity",
					Fraction = new Fraction(1) { { "kg", 1 }, { "s", -2 }, { "A", -1 } }
				},
				new()
				{
					Tag = Tag.Special,
					Name = "H",
					Summary = "Henry: inductance",
					VarName = "inductance",
					Fraction = new Fraction(1) { { "kg", 1 }, { "m", 2 }, { "s", -2 }, { "A", -2 } }
				},
				new()
				{
					Tag = Tag.Special,
					Name = "lm",
					Summary = "Lumen: luminous flux",
					VarName = "luminousFlux",
					Fraction = new Fraction(1) { { "cd", 1 }, { "rad", 2 } }
				},
				new()
				{
					Tag = Tag.Special,
					Name = "lx",
					Summary = "Lux: illuminance",
					VarName = "illuminance",
					Fraction = new Fraction(1) { { "cd", 1 }, { "rad", 2 }, { "m", -2 } }
				},
				new()
				{
					Tag = Tag.Special,
					Name = "kat",
					Summary = "Katal: catalytic activity",
					VarName = "catalyticActivity",
					Fraction = new Fraction(1) { { "mol", 1 }, { "s", -1 } }
				}
			};
		}

		private static List<Unit> CoherentDerivedUnits()
		{
			return new List<Unit>
			{
				new()
				{
					Tag = Tag.Coherent,
					Summary = "area: m²",
					VarName = "area",
					Fraction = new Fraction(1) { { "m", 2 } }
				},
				new()
				{
					Tag = Tag.Coherent,
					Summary = "volume: m³",
					VarName = "volume",
					Fraction = new Fraction(1) { { "m", 3 } }
				},
				new()
				{
					Tag = Tag.Coherent,
					Summary = "speed: m/s",
					VarName = "speed",
					Fraction = new Fraction(1) { { "m", 1 }, { "s", -1 } }
				},
				new()
				{
					Tag = Tag.Coherent,
					Summary = "acceleration: m/s²",
					VarName = "accel",
					Fraction = new Fraction(1) { { "m", 1 }, { "s", -2 } }
				},
				new()
				{
					Tag = Tag.Coherent,
					Summary = "Reciprocal metre",
					VarName = "wavenumber",
					Fraction = new Fraction(1) { { "m", -1 } }
				},
				new()
				{
					Tag = Tag.Coherent,
					Summary = "Kilogram per cubic metre",
					VarName = "density",
					Fraction = new Fraction(1) { { "kg", 1 }, { "m", -3 } }
				},
				new()
				{
					Tag = Tag.Coherent,
					Summary = "Kilogram per square metre",
					VarName = "surfaceDensity",
					Fraction = new Fraction(1) { { "kg", 1 }, { "m", -2 } }
				},
				new()
				{
					Tag = Tag.Coherent,
					Summary = "Cubic metre per kilogram",
					VarName = "specificVolume",
					Fraction = new Fraction(1) { { "m", 3 }, { "kg", -1 } }
				},
				new()
				{
					Tag = Tag.Coherent,
					Summary = "Ampere per square metre",
					VarName = "currentDensity",
					Fraction = new Fraction(1) { { "A", 1 }, { "m", -2 } }
				},
				new()
				{
					Tag = Tag.Coherent,
					Summary = "Ampere per metre",
					VarName = "magneticFieldStrength",
					Fraction = new Fraction(1) { { "A", 1 }, { "m", -1 } }
				},
				new()
				{
					Tag = Tag.Coherent,
					Summary = "Mole per cubic metre",
					VarName = "concentration",
					Fraction = new Fraction(1) { { "mol", 1 }, { "m", -3 } }
				},
				new()
				{
					Tag = Tag.Coherent,
					Summary = "Candela per square metre",
					VarName = "luminance",
					Fraction = new Fraction(1) { { "cd", 1 }, { "m", -2 } }
				}
			};
		}

		private static List<Unit> DerivedUnitsWithSpecialNames()
		{
			return new List<Unit>
			{
				new()
				{
					Tag = Tag.DerivedFromSpecial,
					Name = "Pas",
					Summary = "Pascal-second: dynamic viscosity",
					VarName = "dynamicViscosity",
					Fraction = new Fraction(1) { { "kg", 1 }, { "m", -1 }, { "s", -1 } }
				},
				new()
				{
					Tag = Tag.DerivedFromSpecial,
					Name = "Npm",
					Summary = "Newton per metre: surface tension",
					VarName = "surfaceTension",
					Fraction = new Fraction(1) { { "kg", 1 }, { "s", -2 } }
				},
				new()
				{
					Tag = Tag.DerivedFromSpecial,
					Name = "radps2",
					Summary = "Radian per second squared: angular acceleration",
					VarName = "angularAcceleration",
					Fraction = new Fraction(1) { { "s", -2 } }
				},
				new()
				{
					Tag = Tag.DerivedFromSpecial,
					Name = "Wpm2",
					Summary = "Watt per square metre: heat flux density, irradiance",
					VarName = "heatFluxDensity",
					Fraction = new Fraction(1) { { "kg", 1 }, { "s", -3 } }
				},
				new()
				{
					Tag = Tag.DerivedFromSpecial,
					Name = "JpK",
					Summary = "Joule per kelvin: entropy, heat capacity",
					VarName = "entropy",
					Fraction = new Fraction(1) { { "m", 2 }, { "kg", 1 }, { "s", -2 }, { "K", -1 } }
				},
				new()
				{
					Tag = Tag.DerivedFromSpecial,
					Name = "JpkgK",
					Summary = "Joule per kilogram-kelvin: specific heat capacity, specific entropy",
					VarName = "specificHeatCapacity",
					Fraction = new Fraction(1) { { "m", 2 }, { "s", -2 }, { "K", -1 } }
				},
				new()
				{
					Tag = Tag.DerivedFromSpecial,
					Summary = "Joule per kilogram: specific energy, Velocity Squared m²/s²",
					VarName = "specificEnergy",
					Fraction = new Fraction(1) { { "m", 2 }, { "s", -2 } }
				},
				new()
				{
					Tag = Tag.DerivedFromSpecial,
					Name = "WpmK",
					Summary = "Watt per metre-kelvin: thermal conductivity",
					VarName = "thermalConductivity",
					Fraction = new Fraction(1) { { "m", 1 }, { "kg", 1 }, { "s", -3 }, { "K", -1 } }
				},
				new()
				{
					Tag = Tag.DerivedFromSpecial,
					Name = "Vpm",
					Summary = "Volt per metre: electric field strength",
					VarName = "electricFieldStrength",
					Fraction = new Fraction(1) { { "m", 1 }, { "kg", 1 }, { "s", -3 }, { "A", -1 } }
				},
				new()
				{
					Tag = Tag.DerivedFromSpecial,
					Name = "Cpm3",
					Summary = "Coulomb per cubic metre: electric charge density",
					VarName = "electricChargeDensity",
					Fraction = new Fraction(1) { { "m", -3 }, { "s", 1 }, { "A", 1 } }
				},
				new()
				{
					Tag = Tag.DerivedFromSpecial,
					Name = "Cpm2",
					Summary =
						"Coulomb per square metre: surface charge density, electric flux density, electric displacement",
					VarName = "surfaceChargeDensity",
					Fraction = new Fraction(1) { { "m", -2 }, { "s", 1 }, { "A", 1 } }
				},
				new()
				{
					Tag = Tag.DerivedFromSpecial,
					Name = "Fpm",
					Summary = "Farad per metre: permittivity",
					VarName = "permittivity",
					Fraction = new Fraction(1) { { "m", -3 }, { "kg", -1 }, { "s", 4 }, { "A", 2 } }
				},
				new()
				{
					Tag = Tag.DerivedFromSpecial,
					Name = "Hpm",
					Summary = "Henry per metre: permeability",
					VarName = "permeability",
					Fraction = new Fraction(1) { { "m", 1 }, { "kg", 1 }, { "s", -2 }, { "A", -2 } }
				},
				new()
				{
					Tag = Tag.DerivedFromSpecial,
					Name = "Jpmol",
					Summary = "Joule per mole: molar energy",
					VarName = "molarEnergy",
					Fraction = new Fraction(1) { { "m", 2 }, { "kg", 1 }, { "s", -2 }, { "mol", -1 } }
				},
				new()
				{
					Tag = Tag.DerivedFromSpecial,
					Name = "JpmolK",
					Summary = "Joule per mole-kelvin: molar entropy, molar heat capacity",
					VarName = "molarEntropy",
					Fraction = new Fraction(1) { { "m", 2 }, { "kg", 1 }, { "s", -2 }, { "K", -1 }, { "mol", -1 } }
				},
				new()
				{
					Tag = Tag.DerivedFromSpecial,
					Name = "Cpkg",
					Summary = "Coulomb per kilogram: exposure (x- and γ-rays)",
					VarName = "exposure",
					Fraction = new Fraction(1) { { "kg", -1 }, { "s", 1 }, { "A", 1 } }
				},
				new()
				{
					Tag = Tag.DerivedFromSpecial,
					Name = "Gyps",
					Summary = "Gray per second: absorbed dose rate",
					VarName = "absorbedDoseRate",
					Fraction = new Fraction(1) { { "m", 2 }, { "s", -3 } }
				},
				new()
				{
					Tag = Tag.DerivedFromSpecial,
					Name = "katpm3",
					Summary = "Katal per cubic metre: catalytic activity concentration",
					VarName = "catalyticActivityConcentration",
					Fraction = new Fraction(1) { { "m", -3 }, { "s", -1 }, { "mol", 1 } }
				}
			};
		}

		private static List<Unit> Vectors()
		{
			return new List<Unit>
			{
				new()
				{
					Tag = Tag.Vector,
					Name = "len3",
					Summary = "length: meter",
					VarName = "len",
					Fraction = new Fraction(3) { { "m", 1 } },
					AddFields =
					{
						"m length => (m)math.length(v);",
						"m2 sqrLength => (m2)math.lengthsq(v);"
					}
				},
				new()
				{
					Tag = Tag.Vector,
					Name = "vel3",
					Summary = "speed: m/s",
					VarName = "speed",
					Fraction = new Fraction(3) { { "m", 1 }, { "s", -1 } },
					AddFields =
					{
						"mps speed => (mps)math.length(v);",
						"m2ps2 sqrSpeed => new (math.lengthsq(v));"
					}
				},
				new()
				{
					Tag = Tag.Vector,
					Name = "accel3",
					Summary = "acceleration: m/s²",
					VarName = "accel",
					Fraction = new Fraction(3) { { "m", 1 }, { "s", -2 } },
					AddFields =
					{
						"mps2 acceleration => (mps2)math.length(v);",
					}
				},
				new()
				{
					Tag = Tag.Vector,
					Name = "force3",
					Summary = "Newton: force",
					VarName = "force",
					Fraction = new Fraction(3) { { "kg", 1 }, { "m", 1 }, { "s", -2 } }
				},

				new()
				{
					Tag = Tag.Vector,
					Name = "len2",
					Summary = "length: meter",
					VarName = "len",
					Fraction = new Fraction(2) { { "m", 1 } },
					AddFields =
					{
						"m length => (m)math.length(v);",
						"m2 sqrLength => (m2)math.lengthsq(v);"
					}
				},
				new()
				{
					Tag = Tag.Vector,
					Name = "vel2",
					Summary = "speed: m/s",
					VarName = "speed",
					Fraction = new Fraction(2) { { "m", 1 }, { "s", -1 } },
					AddFields =
					{
						"mps speed => (mps)math.length(v);",
						"m2ps2 sqrSpeed => new (math.lengthsq(v));"
					}
				},
				new()
				{
					Tag = Tag.Vector,
					Name = "accel2",
					Summary = "acceleration: m/s²",
					VarName = "accel",
					Fraction = new Fraction(2) { { "m", 1 }, { "s", -2 } },
					AddFields =
					{
						"mps2 acceleration => (mps2)math.length(v);",
					}
				},
				new()
				{
					Tag = Tag.Vector,
					Name = "force2",
					Summary = "Newton: force",
					VarName = "force",
					Fraction = new Fraction(2) { { "kg", 1 }, { "m", 1 }, { "s", -2 } }
				},
			};
		}

		private static List<Unit> NonSiUnits()
		{
			return new List<Unit>
			{
				new()
				{
					Tag = Tag.Base,
					Name = "rad",
					Summary = "Radian: plane angle",
					VarName = "angle",
					Fraction = new Fraction(1) { { "rad", 1 } }
				},
				new()
				{
					Tag = Tag.Coherent,
					Name = "sr",
					Summary = "Steradian: solid angle",
					VarName = "solidAngle",
					Fraction = new Fraction(1) { { "rad", 2 } }
				},
			};
		}

		#endregion
	}
}
