using System.Collections.Generic;
using System.Linq;

namespace Metric.Editor.Generator
{
    internal class UnitStructGeneratorLvl0
    {
        public Dictionary<string, Unit> Units = new();
        protected Dictionary<string, Unit> _structNames = new();
        public HashSet<Op> Ops = new();
        public HashSet<string> MathOps = new();
        
        protected static readonly Unit[] Float =
        {
            null,
            new() { Name = "float", VarName = "f", Fraction = new Fraction(1) },
            new() { Name = "float2", VarName = "v", Fraction = new Fraction(2) },
            new() { Name = "float3", VarName = "v", Fraction = new Fraction(3) },
        };

        public UnitStructGeneratorLvl0()
        {
            for (int i = 1; i < 4; i++)
            {
                AddUnit(Float[i]);
            }
        }


        public bool AddOp(Unit a, char op, Unit b, System.Func<Unit, Unit, Fraction, bool> drop)
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

            var frac = Fraction.PerformUnitAnalysis(a.Fraction, multiply, b.Fraction);

            if (drop != null)
            {
                if (drop(a, b, frac)) return false;
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
                throw new System.Exception($"{unit.Name}({unit.Summary}) with id {unit.Fraction.ID} collides with {c.Name}({c.Summary})");
            unit.Init();
            if (_structNames.TryGetValue(unit.Name, out var c2)) 
                throw new System.Exception($"{unit.Fraction.ID}({unit.Summary}) with name {unit.Name} collides with {c2.Fraction.ID}({c2.Summary})");
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
        public List<Unit> GetUnits(bool excluding, params string[] unitNames)
        {
            var set = new HashSet<string>(unitNames.Select(s => "1" + s));
            
            var list = new List<Unit>();
            foreach (var unit in Units.Values)
            {
                if (set.Contains(unit.Fraction.ID) ^ excluding)
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
            foreach (var unit in Units.Values)
            {
                var allPowersMulOf2 = unit.Fraction.Dict.Values.All(p => (p != 0) & ((p & 1) == 0));
                if (allPowersMulOf2 && unit.Fraction.HasUnit)
                {
                    var d2 = new Dictionary<string, int>();
                    foreach (var u in unit.Fraction.Dict)
                    {
                        d2[u.Key] = u.Value / 2;
                    }

                    var frac = new Fraction(unit.VecSize,
                        unit.Fraction.Dict.ToDictionary(p => p.Key, p => p.Value / 2));
                    var result = ToUnit(frac);


                    var varName = unit.VarName ?? "x";
                    if(result == null) continue;
                    MathOps.Add($"{result.Name} sqrt({unit.Name} {varName}) => new {result.Name}(System.MathF.Sqrt({varName}.{unit.InField}));");
                }
            }
        }

    }
}