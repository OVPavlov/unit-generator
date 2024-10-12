using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Metric.Editor.Generator
{
	[System.Serializable]
	internal class CustomUnitPermutation
	{
		public string[] Units;
		public ResultFilter resultFilter = ResultFilter.Default;

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
		
		public static void Permutate<T>(T[] array, System.Action<T[]> action)
		{
			int[] c = new int[array.Length];
			action(array);

			int i = 1;
			while (i < array.Length)
			{
				if (c[i] < i)
				{
					int swapIndex = (i % 2 == 0) ? 0 : c[i];
					(array[swapIndex], array[i]) = (array[i], array[swapIndex]);
					action(array);
					c[i]++;
					i = 1;
				}
				else
				{
					c[i] = 0;
					i++;
				}
			}
		}
		
		internal void Permutation(UnitStructGenerator gen)
		{
			using var _ = new BeforeAndAfter($"Permutation using [{string.Join(", ", Units)}]", gen);
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
			
			void Action((Unit, bool)[] permutation)
			{
				Unit previous = permutation[0].Item1;
				for (int i = 1; i < permutation.Length; i++)
				{
					var b = permutation[i].Item1;
					var multiply = permutation[i].Item2;

					var a = previous;

					if (a == null) a = UnitStructGeneratorLvl0.Float[1];
					if ((a.VecSize != b.VecSize) & (a.VecSize != 1) & (b.VecSize != 1)) continue;
					if (!a.Fraction.HasUnit & !b.Fraction.HasUnit) continue;

					var frac = new Fraction(a.Fraction, multiply, b.Fraction);
					if (resultFilter.Drop(gen, a, b, frac)) continue;
					
					Unit resUnit = gen.ToUnit(frac);
					gen.Ops.Add(new Op(resUnit, a, multiply, b));
					previous = resUnit;
				}
			}
			foreach (var equation in equations)
			{
				Permutate(equation, Action);
			}
		}
	}
}