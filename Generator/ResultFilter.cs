using UnityEngine;

namespace Metric.Editor.Generator
{
	[System.Serializable]
	internal struct ResultFilter
	{
		public static readonly ResultFilter Default = new ResultFilter()
		{
			eBase = (BaseUnits)~0,
			eVec = Vec.All,
			eHasComplexityBelow = 100,
			eComplexityBelowMaxInputUnitMinusOffset = -100,
			nBase = (BaseUnits)~0,
			nVec = Vec.All,
			nHasComplexityBelow = 100,
			nComplexityBelowMaxInputUnitMinusOffset = -100,
		};
		
		[Header("Allow if existing result unit:")] 
		public BaseUnits eBase;
		public Vec eVec;
		public int eHasComplexityBelow;
		public int eComplexityBelowMaxInputUnitMinusOffset;

		[Header("Allow if new result unit:")] 
		public BaseUnits nBase;
		public Vec nVec;
		public int nHasComplexityBelow;
		public int nComplexityBelowMaxInputUnitMinusOffset;

		public bool Drop(UnitStructGeneratorLvl0 gen, Unit a, Unit b, Fraction frac)
		{
			int inputComplexity = Mathf.Max(a.Fraction.Complexity, b.Fraction.Complexity);
		
			if (frac.ID != null && !gen.Units.ContainsKey(frac.ID)) // gonna create new one 
			{
				int limit = Mathf.Min(nHasComplexityBelow, inputComplexity - nComplexityBelowMaxInputUnitMinusOffset);
				if (frac.Complexity > limit) return true;
				if (!Filter.HasOnly(frac, nBase)) return true;
				if (!Filter.Fit(frac, nVec)) return true;
			}
			else //  Existing
			{
				int limit = Mathf.Min(eHasComplexityBelow, inputComplexity - eComplexityBelowMaxInputUnitMinusOffset);
				if (frac.Complexity > limit) return true;
				if (!Filter.HasOnly(frac, eBase)) return true;
				if (!Filter.Fit(frac, eVec)) return true;
			}

			return false;
		}
	}
}