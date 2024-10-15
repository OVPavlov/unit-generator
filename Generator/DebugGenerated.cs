using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Metric.Editor.Generator
{
	internal readonly struct DebugGenerated : System.IDisposable
	{
		[Flags]
		public enum Mode
		{
			None,
			Ops = 1 << 1,
			Units = 1 << 2,
			DisplayPretty = 1 << 3,
		};
		private readonly string _name;
		private readonly UnitStructGeneratorLvl0 _gen;
		private readonly HashSet<Op> _opsBefore;
		private readonly HashSet<Unit> _unitsBefore;
		private readonly Mode _mode;

		public DebugGenerated( UnitStructGeneratorLvl0 gen, string name, Mode mode)
		{
			_name = name;
			_gen = gen;
			_mode = mode;
			_opsBefore = mode.HasFlag(Mode.Ops) ? new HashSet<Op>(gen.Ops) : null;
			_unitsBefore = mode.HasFlag(Mode.Units) ? gen.Units.Values.ToHashSet() : null;
		}
		public void Dispose()
		{
			if ((_mode & (Mode.Ops | Mode.Units)) == 0) return;
	
			bool pretty = _mode.HasFlag(Mode.DisplayPretty);

			string PrintUnit(Unit unit)
			{
				return pretty ? unit.Fraction.GetFractionalNotation() : unit.Name;
			}
			var sb = new System.Text.StringBuilder(100);
			sb.Append($"{_name} ");
			
			if (_unitsBefore != null)
			{
				var unitsAfter =  _gen.Units.Values.ToHashSet();
				unitsAfter.ExceptWith(_unitsBefore);
				if (unitsAfter.Count > 0)
				{
					sb.AppendLine("Generated Units");
					foreach (var unit in unitsAfter)
					{
						sb.AppendLine(PrintUnit(unit));
					}
				}
			}

			if (_opsBefore != null)
			{
				var opsAfter = new HashSet<Op>(_gen.Ops);
				opsAfter.ExceptWith(_opsBefore);
				if (opsAfter.Count > 0)
				{
					sb.AppendLine("Generated Operations");
					foreach (var op in opsAfter)
					{
						sb.Append(PrintUnit(op.A));
						sb.Append(op.Multiply ? " * " : " / ");
						sb.Append(PrintUnit(op.B));
						sb.Append(" = ");
						sb.Append(PrintUnit(op.Res));
						sb.AppendLine();
					}
				}
			}

			Debug.Log(sb);
		}
	}
}