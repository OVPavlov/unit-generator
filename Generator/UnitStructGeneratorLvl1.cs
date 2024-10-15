using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Metric.Editor.Generator
{
	internal class UnitStructGeneratorLvl1 : UnitStructGeneratorLvl0
	{
		public bool AddOperation(string a,char op, string b)
		{
			var uA = GetUnitByName(a);
			var uB = GetUnitByName(b);
			if (uA == null)
			{
				Debug.LogError($"can't find unit {a}");
				return false;
			}
			if (uB == null)
			{
				Debug.LogError($"can't find unit {b}");
				return false;
			}
			return AddOp(uA, op, uB, null);
		}

        
        
		public void DistributeOperations()
		{
			foreach (var op in Ops)
			{
				var host = op.A.Fraction.Complexity> op.B.Fraction.Complexity ? op.B : op.A;
				if (op.A.VecSize != op.B.VecSize)
				{
					host = op.A.VecSize < op.B.VecSize ? op.B : op.A;
				}
				if (!op.A.Fraction.HasUnit) host = op.B;
				if (!op.B.Fraction.HasUnit) host = op.A;
				host.Ops.Add(op);
			}
			Ops.Clear();
		}

		public void GenerateCustomOperators(System.Func<UnitStructGeneratorLvl0, Unit, Unit, Fraction, bool> drop, IEnumerable<Unit> units)
		{
			IList<Unit> list = units as IList<Unit> ?? units.ToList();
			foreach (var a in list)
			{ 
				foreach (var b in list)
				{
					AddOp(a, '/', b, drop);
					AddOp(a, '*', b, drop);
				}
			}
		}


	}
}