using System.Collections.Generic;

namespace Metric.Editor.Generator
{
	[System.Serializable]
	internal class CustomUnit
	{
		public Element[] Fraction;
		public int VecSize = 1;
		public string Name;
		public string Description;
		public string VarName;
		public bool VisibleInEditor;
		public Tag Tag;
		public string[] AddFields;

		[System.Serializable]
		public struct Element
		{
			public string unit;
			public int power;
		}

		internal Unit GetUnit()
		{
			var frac = new Fraction(VecSize);
			foreach (var element in Fraction)
			{
				frac.Add(element.unit, element.power);
			}

			return new Unit
			{
				VisibleInEditor = VisibleInEditor,
				Tag = Tag,
				Name = Name,
				Summary = Description,
				VarName = VarName,
				AddFields = new List<string>(AddFields),
				Fraction = frac
			};
		}
	}
}