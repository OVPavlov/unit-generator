using System;
using System.Collections.Generic;
using UnityEngine;

namespace Metric.Editor.Generator
{
	internal readonly struct BeforeAndAfter : System.IDisposable
	{
		private readonly string _name;
		private readonly UnitStructGeneratorLvl0 _gen;
		private readonly int _opsBefore, _unitsBefore, _usingUnitsCount;
		private readonly long timestamp;

		public BeforeAndAfter(string name, UnitStructGeneratorLvl0 gen, ICollection<Unit> units = null)
		{
			_name = name;
			_gen = gen;
			_opsBefore = gen.Ops.Count;
			_unitsBefore = gen.Units.Count;
			_usingUnitsCount = units?.Count ?? -1;
			timestamp = System.Diagnostics.Stopwatch.GetTimestamp();
		}
		public void Dispose()
		{
			var sb = new System.Text.StringBuilder();
			sb.Append($"{_name} ");
			
			if (_usingUnitsCount > -1)
			{
				sb.Append($"using {_usingUnitsCount} units");
			}
			sb.Append(": ");
			//<color=#fff4>colorfully</color>
			string cLow = "<color=#ffffff44>";
			string cEnd = "</color>";
			sb.Append($"\t{cLow}{_opsBefore}{cEnd} <b>+{_gen.Ops.Count - _opsBefore}</b> <i>ops</i>;");
			sb.Append($"\t{cLow}{_unitsBefore}{cEnd} <b>+{_gen.Units.Count - _unitsBefore}</b> <i>units</i>");
			var currentTimestamp = System.Diagnostics.Stopwatch.GetTimestamp();
			var passed = new TimeSpan(currentTimestamp) - new TimeSpan(timestamp);
			sb.Append($"\noperation took: {passed.TotalSeconds:0}s {passed.Milliseconds:0}ms");
			Debug.Log(sb);
		}
	}
}