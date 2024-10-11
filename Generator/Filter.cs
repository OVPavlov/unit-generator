using System.Collections.Generic;
using System.Linq;

namespace Metric.Editor.Generator
{
	internal static class Filter
	{
		internal static T GetUnits<T>(BaseUnits units) where T:ICollection<string>, new()
		{
			T list = new T();
			if (units.HasFlag(BaseUnits.s)) list.Add("s");
			if (units.HasFlag(BaseUnits.kg)) list.Add("kg");
			if (units.HasFlag(BaseUnits.m)) list.Add("m");
			if (units.HasFlag(BaseUnits.A)) list.Add("A");
			if (units.HasFlag(BaseUnits.K)) list.Add("K");
			if (units.HasFlag(BaseUnits.mol)) list.Add("mol");
			if (units.HasFlag(BaseUnits.cd)) list.Add("cd");
			if (units.HasFlag(BaseUnits.rad)) list.Add("rad");
			return list;
		}

		public static List<Unit> ByBaseUnits(IEnumerable<Unit> list, BaseUnits baseUnits, bool only = false, bool not = false)
		{
			var unitSet = GetUnits<HashSet<string>>(baseUnits);
			bool Predicate(KeyValuePair<string, int> kv) => not ^ unitSet.Contains(kv.Key);
			var res = new List<Unit>();
			foreach (var unit in list)
			{
				if (only ? unit.Fraction.Dict.All(Predicate) : unit.Fraction.Dict.Any(Predicate))
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

	}
}