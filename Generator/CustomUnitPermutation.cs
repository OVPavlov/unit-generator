using System.Collections.Generic;
using System.Linq;

namespace Metric.Editor.Generator
{
	[System.Serializable]
	public struct CustomUnitPermutation
	{
		public string[] Units;

		public List<List<T>> GetAllCombinations<T>(T[] array, int minLength, int maxLength)
		{
			var result = new List<List<T>>();

			for (int len = minLength; len <= maxLength; len++)
			{
				result.AddRange(GetCombinations(new List<T>(), 0, len));
			}

			return result;

			IEnumerable<List<T>> GetCombinations(List<T> current, int start, int length)
			{
				if (length == 0)
				{
					yield return new List<T>(current);
					yield break;
				}

				for (int i = start; i <= array.Length - length; i++)
				{
					current.Add(array[i]);
					foreach (var combination in GetCombinations(current, i + 1, length - 1))
					{
						yield return combination;
					}

					current.RemoveAt(current.Count - 1);
				}
			}
		}

		public static IEnumerable<T[]> Permutate<T>(T[] array)
		{
			if (array.Length == 1)
				yield return array;
			else
			{
				for (int i = 0; i < array.Length; i++)
				{
					T[] remaining = array.Where((_, index) => index != i).ToArray();
					foreach (var permutation in Permutate(remaining))
					{
						yield return new T[] { array[i] }.Concat(permutation).ToArray();
					}
				}
			}
		}

		internal void Permutation(UnitStructGenerator gen)
		{
			var units = Units.Select(gen.GetUnitByName).ToArray();
			var comb = GetAllCombinations(units, units.Length, units.Length);
			List<(Unit, bool)[]> equations = new List<(Unit, bool)[]>();
			foreach (var c in comb)
			{
				int totalCombinations = 1 << c.Count; // 2^n combinations

				for (int i = 0; i < totalCombinations; i++)
				{
					int sum = 0;
					var eq = new (Unit, bool)[c.Count];
					for (int j = 0; j < c.Count; j++)
					{
						bool multiply = (i & (1 << j)) != 0;
						eq[j] = (c[j], multiply);
					}

					equations.Add(eq);
				}
			}

			foreach (var equation in equations)
			{
				var permutations = Permutate(equation);
				foreach (var permutation in permutations)
				{
					Unit previous = permutation[0].Item1;
					for (int i = 1; i < permutation.Length; i++)
					{
						var b = permutation[i].Item1;
						var multiply = permutation[i].Item2;

						gen.AddOp(previous, multiply ? '*' : '/', b, null);

						var frac = Fraction.PerformUnitAnalysis(previous.Fraction, multiply, b.Fraction);
						previous = gen.ToUnit(frac);
					}
				}

			}
		}
	}
}