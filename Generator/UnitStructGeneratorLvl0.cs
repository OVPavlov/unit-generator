using System.Collections.Generic;
using System.Linq;

namespace Metric.Editor.Generator
{
    internal class UnitStructGeneratorLvl0
    {
        public Dictionary<long, Unit> Units = new();
        protected Dictionary<string, Unit> _structNames = new();
        public HashSet<Op> Ops = new();
        public HashSet<string> MathOps = new();
        
        public static readonly Unit[] Float =
        {
            null,
            new() { Name = "float", VarName = "f", Fraction = new Fraction(1), Tag = Tag.Dimensionless},
            new() { Name = "float2", VarName = "v", Fraction = new Fraction(2), Tag = Tag.Dimensionless },
            new() { Name = "float3", VarName = "v", Fraction = new Fraction(3), Tag = Tag.Dimensionless },
        };

        public UnitStructGeneratorLvl0()
        {
            for (int i = 1; i < 4; i++)
            {
                AddUnit(Float[i]);
            }
        }


        public bool AddOp(Unit a, char op, Unit b, System.Func<UnitStructGeneratorLvl0, Unit, Unit, Fraction, bool> drop)
        {
            if (a == null) a = Float[1];
            if ((a.VecSize != b.VecSize) & (a.VecSize != 1) & (b.VecSize != 1)) return false;
            if (!a.Fraction.HasUnit & !b.Fraction.HasUnit) return false;
            

            var multiply = op switch
            {
                '/' => false,
                '*' => true,
                _ => throw new System.Exception($"Unknown operator {op}")
            };

            var frac = new Fraction(a.Fraction, multiply, b.Fraction);

            if (drop != null)
            {
                if (drop(this, a, b, frac)) return false;
            }
            
            Unit resUnit = ToUnit(frac);
            return Ops.Add(new Op(resUnit, a, multiply, b));
        }

        public Unit ToUnit(in Fraction fraction)
        {
            if (!fraction.HasUnit)
            {
                var flUnit = Float[fraction.VecSize];
                if (!Units.ContainsKey(flUnit.Fraction.ID)) AddUnit(flUnit);
                return flUnit; // it's float/float[n]
            }

            if (Units.TryGetValue(fraction.ID, out var resUnit)) return resUnit;
            
            resUnit = new Unit
            {
                Name = fraction.GetName(),
                Summary = fraction.GetDescription(),
                Fraction = fraction,
                Tag = Tag.AutoDerived,
            };
            AddUnit(resUnit);
            return resUnit;
        }

        public void AddUnit(Unit unit)
        {
            if (Units.TryGetValue(unit.Fraction.ID, out var c))
                throw new System.Exception($"{unit.Name}({unit.Summary}) with id {unit.Fraction.DebugID()} collides with {c.Name}({c.Summary})");
            unit.Init();
            if (_structNames.TryGetValue(unit.Name, out var c2)) 
                throw new System.Exception($"name {unit.Name} collision\n{unit.Fraction.DebugID()}({unit.Summary}) collides with \n{c2.Fraction.DebugID()}({c2.Summary})");
            Units.Add(unit.Fraction.ID, unit);
            _structNames.Add(unit.Name, unit);
        }
        
        public void AddUnits(IEnumerable<Unit> units)
        {
            foreach (var unit in units)
            {
                AddUnit(unit);
            }
        }

        public List<Unit> GetUnits(System.Func<Unit, bool> take)
        {
            var list = new List<Unit>();
            foreach (var unit in Units.Values)
            {
                if (take(unit))
                {
                    list.Add(unit);
                }
            }
            return list;
        }

        public List<Unit> GetUnits(Tag tags)
        {
            return Filter.ByTags(Units.Values, tags);
        }
        
        public Unit GetUnitByName(string name)
        {
            return _structNames.GetValueOrDefault(name);
        }


        public void GenerateMathOps()
        {
            foreach (var unit in Units.Values.ToList())
            {

                var allPowersMulOf2 = unit.Fraction.All((u, p) => (p & 1) == 0);
                if (allPowersMulOf2 && unit.Fraction.HasUnit)
                {
                    var frac  = new Fraction(unit.VecSize, unit.Fraction, (u, p) => p / 2);
                    var result = ToUnit(frac);
                    
                    var varName = unit.VarName ?? "x";
                    if(result == null) continue;
                    MathOps.Add($"{result.Name} sqrt({unit.Name} {varName}) => new {result.Name}(System.MathF.Sqrt({varName}.{unit.InField}));");
                }
            }
        }

    }
}