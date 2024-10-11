using System.Linq;
using UnityEngine;

namespace Metric.Editor.Generator
{
	[System.Serializable]
	public class Block
	{
		public enum Vec
		{
			OnlyVectors = 1,
			NoVectors = 2,
			All = 3,
		}

		[Header("Input Units")] 
		public BaseUnits filterByBase;
		public Tag filterByTag;
		public Vec vecFilter;

		[Header("Allow operation IF:")] 
		public bool introducesNewUnits = true;
		public BaseUnits resultInBase;
		public bool resultIsNonSI = false;
		public Vec resultVec;
		public int hasComplexityBelow = 1000;
		public int complexityBelowMaxInputUnitMinusOffset = -1000;

		internal void GenerateOperators(UnitStructGenerator gen)
		{
			var units = gen.Units.Values.ToList();
			units = Filter.ByTags(units, filterByTag);
			units = Filter.ByBaseUnits(units, filterByBase);

			if (vecFilter == Vec.NoVectors)
				units = units.Where(u => u.VecSize == 1).ToList();
			else if (vecFilter == Vec.OnlyVectors)
				units = units.Where(u => u.VecSize > 1).ToList();

			Debug.Log($"Filtered {units.Count} / {gen.Units.Count} units");
			
			gen.GenerateOperators(units, resultBase: resultInBase,
				resultVec: resultVec , noNewUnits: !introducesNewUnits, dropNonSi: !resultIsNonSI,
				maxComplexity: hasComplexityBelow, inputComplexityLimitOffset: complexityBelowMaxInputUnitMinusOffset);
		}
		
	}
}