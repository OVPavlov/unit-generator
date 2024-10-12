using System.Collections.Generic;

namespace Metric.Editor.Generator
{
	internal static class UnitCollection
	{
		
		public static List<Unit> SiBaseUnits()
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

		public static List<Unit> SiDerivedUnits()
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

		public static List<Unit> CoherentDerivedUnits()
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

		public static List<Unit> DerivedUnitsWithSpecialNames()
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

		public static List<Unit> Vectors()
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

		public static List<Unit> NonSiUnits()
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

	}
}