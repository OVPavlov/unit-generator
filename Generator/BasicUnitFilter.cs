using System.Collections.Generic;
using UnityEngine;

namespace Metric.Editor.Generator
{
	[System.Flags]
	internal enum BaseUnits
	{
		s = 1 << 0,
		kg = 1 << 1,
		m = 1 << 2,
		A = 1 << 3,
		K = 1 << 4,
		mol = 1 << 5,
		cd = 1 << 6,
		rad = 1 << 7,
		
		
		SI = s|kg|m|A|K|mol|cd,
		Kinematic = s|kg|m,
		
		dimentionless = 1 << 31
	};
	
	[System.Serializable]
	internal struct BasicUnitFilter
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