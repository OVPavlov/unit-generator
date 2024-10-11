using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Metric.Editor.Generator
{
	internal struct Fraction : IEnumerable<string>
	{
		private static readonly HashSet<string> BaseSiUnits = new() { "s", "m", "kg", "A", "K", "mol", "cd" };
		static readonly StringBuilder Num = new();
		static readonly StringBuilder Den = new();
		public Dictionary<string, int> Dict;
		public string ID;
		public readonly int VecSize;
		public int NumSize, DenSize;
		public int Complexity => NumSize + DenSize;
		public bool HasUnit => ID.Length > 1;
		public bool IsSI { get; private set; }
		public string ScalarID => $"1{ID.Substring(1)}";
		public Fraction ScalarFraction => new Fraction(1, Dict);
		

		public Fraction(int vecSize)
		{
			Dict = new Dictionary<string, int>();
			VecSize = vecSize;
			ID = vecSize.ToString();
			NumSize = DenSize = 0;
			IsSI = false;
		}

		public void Add(string unit, int power)
		{
			Dict.Add(unit, power);
			Init();
		}

		public Fraction(int vecSize, Dictionary<string, int> fraction)
		{
			Dict = fraction;
			VecSize = vecSize;
			ID = vecSize.ToString();
			NumSize = DenSize = 0;
			IsSI = false;
			Init();
		}

		private void Init()
		{
			if (Dict.Count == 0)
			{
				ID = VecSize.ToString();
				NumSize = DenSize = 0;
				return;
			}

			var fr = Dict.ToArray();
			System.Array.Sort(fr, (a, b) =>
				string.Compare(a.Key, b.Key, System.StringComparison.Ordinal) * 10 + a.Value.CompareTo(b.Value));

			IsSI = fr.Length > 0;
			Num.Clear();
			Den.Clear();
			foreach (var pair in fr)
			{
				if (pair.Value == 0) throw new System.Exception($"WTF!!!  {pair.Key}: {pair.Value}");

				var ts = pair.Value > 0 ? Num : Den;
				if (pair.Value > 0) NumSize += pair.Value;
				else DenSize += -pair.Value;

				if (ts.Length > 0) ts.Append('*');
				ts.Append(pair.Key);
				if (pair.Value > 1) ts.Append(pair.Value);
				else if (pair.Value < -1) ts.Append(-pair.Value);
				if (!BaseSiUnits.Contains(pair.Key)) IsSI = false;
			}

			string mum = Num.Length > 0 ? Num.ToString() : "1";
			ID = $"{VecSize}{mum}";
			if (Den.Length > 0) ID += $"/{Den}";

			Num.Clear();
			Den.Clear();
		}

		[Pure]
		public string GetName()
		{
			if (!HasUnit) return string.Empty;
			return ID.Substring(1).Replace("1/", "_1/").Replace('/', 'p').Replace("*", "");
		}

		[Pure]
		public string GetDescription()
		{
			if (!HasUnit) return string.Empty;
			return ID.Substring(1).Replace('2', '\u00b2').Replace('3', '\u00b3')
				.Replace('4', '\u00b4').Replace('5', '\u00b5').Replace('6', '\u00b6');
		}


		public static Fraction PerformUnitAnalysis(in Fraction a, bool multiply, in Fraction b)
		{
			var result = new Dictionary<string, int>(a.Dict);
			int powDir = multiply ? 1 : -1;
			foreach (var kvp in b.Dict)
			{
				if (result.ContainsKey(kvp.Key))
				{
					result[kvp.Key] += powDir * kvp.Value;
					if (result[kvp.Key] == 0)
					{
						result.Remove(kvp.Key);
					}
				}
				else
				{
					result[kvp.Key] = powDir * kvp.Value;
				}
			}

			return new Fraction(Mathf.Max(a.VecSize, b.VecSize), result);
		}

		public IEnumerator<string> GetEnumerator()
		{
			throw new System.NotImplementedException();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}