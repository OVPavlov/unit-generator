using System.Collections.Generic;
using System.Linq;

namespace Metric.Editor.Generator
{
	internal static class Filter
	{
		public static bool HasAny(in Fraction frac, BaseUnits units)
		{
			return (frac.BaseUnits & units) != 0;
		}
		public static bool HasOnly(in Fraction frac, BaseUnits units)
		{
			return (frac.BaseUnits | units) == units;
		}

		public static bool Fit(in Fraction frac, Vec vec)
		{
			if (vec == Vec.NoVectors & frac.VecSize > 1) return false;
			if (vec == Vec.OnlyVectors & frac.VecSize == 1) return false;
			return true;
		}

		public static List<Unit> ByBaseUnits(IEnumerable<Unit> list, BaseUnits baseUnits)
		{
			var res = new List<Unit>();
			foreach (var unit in list)
			{
				if (HasOnly(unit.Fraction, baseUnits))
				{
					res.Add(unit);
				}
			}
			return res;
		}

		public static List<Unit> ByTags(IEnumerable<Unit> list, Tag tags)
		{
			var res = new List<Unit>();
			foreach (var unit in list)
			{
				if ((unit.Tag & tags) != 0)
				{
					res.Add(unit);
				}
			}
			return res;
		}
		
		public static List<Unit> ByVec(IEnumerable<Unit> list, Vec vec)
		{
			return vec switch
			{
				Vec.NoVectors => list.Where(u => u.VecSize == 1).ToList(),
				Vec.OnlyVectors => list.Where(u => u.VecSize > 1).ToList(),
				_ => list as List<Unit> ?? list.ToList()
			};
		}

	}
}