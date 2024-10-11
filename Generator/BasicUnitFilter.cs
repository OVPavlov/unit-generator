using System.Collections.Generic;

namespace Metric.Editor.Generator
{
	[System.Flags]
	public enum BaseUnits
	{
		s = 1 << 0,
		kg = 1 << 1,
		m = 1 << 2,
		A = 1 << 3,
		K = 1 << 4,
		mol = 1 << 5,
		cd = 1 << 6,
		rad = 1 << 7,
	}
	[System.Serializable]
	public struct BasicUnitFilter
	{
		public BaseUnits Units;
		public Tag Tags;
		
		internal void FilterAndAdd(UnitStructGenerator gen, List<Unit> units)
		{
			units = Filter.ByBaseUnits(units, Units);
			units = Filter.ByTags(units, Tags);
			gen.AddUnits(units);
		}
	}
	

	
}