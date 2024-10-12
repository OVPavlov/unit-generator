using System.Linq;
using UnityEngine;

namespace Metric.Editor.Generator
{
	internal enum Vec
	{
		OnlyVectors = 1,
		NoVectors = 2,
		All = 3,
	}
	[System.Serializable]
	internal class GenerationBlock
	{
		public string name;
		[Header("Input Units")] 
		public BaseUnits filterByBase;
		public Tag filterByTag;
		public Vec vecFilter;
		public ResultFilter resultFilter = ResultFilter.Default;

		internal void GenerateOperators(UnitStructGenerator gen)
		{
			var units = gen.Units.Values.ToList();
			units = Filter.ByTags(units, filterByTag);
			units = Filter.ByBaseUnits(units, filterByBase);
			units = Filter.ByVec(units, vecFilter);

			Debug.Log($"\tFiltered {units.Count} / {gen.Units.Count} units");
			using (var _ = new BeforeAndAfter("\tGenerating", gen, units))
			{
				gen.GenerateOperators(units, resultFilter);
			}
		}
		
	}
}