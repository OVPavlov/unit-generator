using System;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace Metric.Editor.Generator
{
	[System.Serializable]
	public struct UnitEditorDescriptor
	{
		[System.Serializable]
		public struct SuffixDescriptor
		{
			public string unit, description;
			public double mul;

			public SuffixDescriptor(string unit, string description, double mul)
			{
				this.unit = unit;
				this.description = description;
				this.mul = mul;
			}
		}

		public string unit;
		public string suf;
		public string description;
		public SuffixDescriptor[] suffixes;


		public string Generate()
		{
			var list = suffixes.ToList();
			list.Add(new SuffixDescriptor()
			{
				unit = unit,
				description = description,
				mul = 1
			});
			list.Sort((a, b) => a.mul.CompareTo(b.mul));

			var labels = string.Join("\n", list.Select(p => $"\t\t\tnew(\"{p.unit}\", \"{p.description}\"),"));
			var powers = string.Join(", ", list.Select(p => p.mul.ToString(CultureInfo.InvariantCulture)));

			int basePrefID = list.FindIndex(p => Math.Abs(p.mul - 1) < 1e-3);

			return $@"
    [UnityEditor.CustomPropertyDrawer(typeof({unit}))]
    public class {unit}Drawer : UnitDrawer
    {{
        private const int DefaultIdx = {basePrefID};
        private static readonly GUIContent[] Postfixes =
        {{
{labels}
        }};
        private static readonly double[] Powers = {{ {powers} }};
        public override void OnGUI(Rect position, UnityEditor.SerializedProperty property, GUIContent label)
        {{
            DrawUnit(position, property, label, Powers, Postfixes, DefaultIdx);
        }}
    }}";
		}


		public static readonly UnitEditorDescriptor[] Defaults =
		{
			new()
			{
				unit = "rad", suf = "rad", description = "radian",
				suffixes = new SuffixDescriptor[]
				{
					new("′", "arcminute", Mathf.Rad2Deg / 60.0),
					new("\u00b0", "degree", Mathf.Rad2Deg),
					new("turn", "turn", 2.0 * Mathf.PI)
				}
			},
			new()
			{
				unit = "kg", suf = "kg", description = "kilogram",
				suffixes = new SuffixDescriptor[]
				{
					new("μg", "microgram", 1e-9),
					new("mg", "milligram", 1e-6),
					new("g", "gram", 1e-3),
					new("t", "tonne", 1e+3),
					new("kt", "kilotonne", 1e+6)
				}
			},
			new()
			{
				unit = "m", suf = "m", description = "metre",
				suffixes = new SuffixDescriptor[]
				{
					new("μm", "micrometre", 1e-6),
					new("mm", "millimetre", 1e-3),
					new("cm", "centimetre", 1e-2),
					new("km", "kilometre", 1e+3)
				}
			},
			new()
			{
				unit = "s", suf = "s", description = "second",
				suffixes = new SuffixDescriptor[]
				{
					new("ns", "nanosecond", 1e-9),
					new("μs", "microsecond", 1e-6),
					new("ms", "millisecond", 1e-3),
					new("min", "minute", 60),
					new("h", "hour", (60 * 60)),
					new("day", "day", (60 * 60 * 24))
				}
			},
			new()
			{
				unit = "mps", suf = "m \u2044 s", description = "",
				suffixes = new SuffixDescriptor[]
				{
					new("mm \u2044 s", "", 1e-3),
					new("km \u2044 s", "", 1e+3)
				}
			},
			new()
			{
				unit = "mps2", suf = "m \u2044 s²", description = "",
				suffixes = new SuffixDescriptor[]
				{
					new("mm \u2044 s²", "", 1e-3),
					new("km \u2044 s²", "", 1e+3)
				}
			},
			new()
			{
				unit = "m2", suf = "m²", description = "",
				suffixes = new SuffixDescriptor[]
				{
					new("mm²", "", 1e-6),
					new("km²", "", 1e+6)
				}
			},
		};
	}
}