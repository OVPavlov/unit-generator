using System.Collections.Generic;
using System.Linq;

namespace Metric.Editor.Generator
{
	internal class UnitStructGenerator : UnitStructGeneratorLvl1
	{
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