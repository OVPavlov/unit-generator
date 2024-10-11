using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Metric.Editor.Generator
{
	internal class UnitStructGenerator : UnitStructGeneratorLvl1
	{
		public void GenerateOperators(IEnumerable<Unit> units = null, bool noNewUnits = false, bool dropNonSi = false,
		 BaseUnits resultBase = (BaseUnits)(~0), Block.Vec resultVec = Block.Vec.All,
			int maxComplexity = 1000, int inputComplexityLimitOffset = -1000)
		{
			var unitSet = Filter.GetUnits<HashSet<string>>(resultBase);
			bool Predicate(KeyValuePair<string, int> kv) => unitSet.Contains(kv.Key);
			
			bool drop(Unit a, Unit b, Fraction frac)
			{
				int inputComplexity = Mathf.Max(a.Fraction.Complexity, b.Fraction.Complexity);
				int limit = Mathf.Min(maxComplexity, inputComplexity - inputComplexityLimitOffset);
				if (frac.Complexity > limit) return true;
				if (frac.ID != null && !Units.ContainsKey(frac.ID)) // gonna create new one 
				{
					
					if (noNewUnits) return true;
					if (dropNonSi && !frac.IsSI) return true;
					if (resultVec == Block.Vec.NoVectors & frac.VecSize > 1) return true;
					if (resultVec == Block.Vec.OnlyVectors & frac.VecSize == 1) return true;
					if (!frac.Dict.All(Predicate)) return true;
				}
				return false;
			}

			if (units == null) units = Units.Values;
			ICollection<Unit> enumerable = units as ICollection<Unit> ?? units.ToList();
			GenerateCustomOperators(drop, enumerable);
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