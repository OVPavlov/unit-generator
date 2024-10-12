using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Metric.Editor.Generator
{
	internal struct Fraction : IEnumerable<Fraction> // IEnumerable<Fraction> here to support { ... } initializer through Add func
 	{
	    private static readonly string[] superscripts = { "⁰", "¹", "²", "³", "⁴", "⁵", "⁶", "⁷", "⁸", "⁹" };
	    private static readonly string superscriptMunus = "⁻";
	    private static readonly StringBuilder Buffer = new();
		
		public long ID;
		public readonly int VecSize;
		public int NumSize, DenSize;
		public BaseUnits BaseUnits;
		public int s, kg, m, A, K, mol, cd, rad;
		
		public int Complexity => NumSize + DenSize;
		public bool HasUnit => (NumSize + DenSize) > 0;
		public long ScalarID => (ID & ~((long)15)) | (long)1;
		

		public Fraction(int vecSize)
		{
			VecSize = vecSize;
			NumSize = DenSize = 0;
			BaseUnits = 0;
			ID = 0;
			s = kg = m = A = K = mol = cd = rad = 0;
			Init();
		}

		public void Add(string unit, int power)
		{
			switch (unit)
			{
				case "s":
					s = power;
					break;
				case "kg":
					kg = power;
					break;
				case "m":
					m = power;
					break;
				case "A":
					A = power;
					break;
				case "K":
					K = power;
					break;
				case "mol":
					mol = power;
					break;
				case "cd":
					cd = power;
					break;
				case "rad":
					rad = power;
					break;
			}
			Init();
		}

		public Fraction(in Fraction a, bool multiply, in Fraction b)
		{
			int powDir = multiply ? 1 : -1;
			VecSize = Mathf.Max(a.VecSize, b.VecSize);
			NumSize = DenSize = 0;
			BaseUnits = 0;
			ID = 0;

			s = a.s + powDir * b.s;
			kg = a.kg + powDir * b.kg;
			m = a.m + powDir * b.m;
			A = a.A + powDir * b.A;
			K = a.K + powDir * b.K;
			mol = a.mol + powDir * b.mol;
			cd = a.cd + powDir * b.cd;
			rad = a.rad + powDir * b.rad;

			Init();
		}
		
		public Fraction(int vecSize,in Fraction source, System.Func<BaseUnits, int, int> transform)
		{
			VecSize = vecSize;
			NumSize = DenSize = 0;
			BaseUnits = 0;
			ID = 0;
			s = kg = m = A = K = mol = cd = rad = 0;
			if (source.s != 0) s = transform(BaseUnits.s, source.s);
			if (source.kg != 0) kg = transform(BaseUnits.kg, source.kg);
			if (source.m != 0) m = transform(BaseUnits.m, source.m);
			if (source.A != 0) A = transform(BaseUnits.A, source.A);
			if (source.K != 0) K = transform(BaseUnits.K, source.K);
			if (source.mol != 0) mol = transform(BaseUnits.mol, source.mol);
			if (source.cd != 0) cd = transform(BaseUnits.cd, source.cd);
			if (source.rad != 0) rad = transform(BaseUnits.rad, source.rad);
			Init();
		}


		public static string DebugID(long id)
		{
			// 4 bit (16), +-8
			var units = new[] { "s", "kg", "m", "A", "K", "mol", "cd", "rad" };
			string s = $"v: {id & 15} ";
			for (int i = 0; i < units.Length; i++)
			{
				int offset = 4 * (i + 1);
				long p = ((id >> offset) & 15L) - 8L;
				long pp = p > 0 ? p : -p;
				string sign = p > 0 ? "" : superscriptMunus;
				if (p != 0) s += $"{units[i]}{sign}{superscripts[pp]}";
			}
			return s;
		}
		
		public string DebugID()
		{
			return DebugID(ID);
		}
		
		private void Init()
		{
			NumSize = DenSize = 0;
			// 4 bit (16), +-8
			ID = (long)VecSize |
			     ((long)(s + 8) << (4 * 1)) |
			     ((long)(kg + 8) << (4 * 2)) |
			     ((long)(m + 8) << (4 * 3)) |
			     ((long)(A + 8) << (4 * 4)) |
			     ((long)(K + 8) << (4 * 5)) |
			     ((long)(mol + 8) << (4 * 6)) |
			     ((long)(cd + 8) << (4 * 7)) |
			     ((long)(rad + 8) << (4 * 8));
			
			BaseUnits = 0;
			if(s != 0) BaseUnits |= BaseUnits.s;
			if(kg != 0) BaseUnits |= BaseUnits.kg;
			if(m != 0) BaseUnits |= BaseUnits.m;
			if(A != 0) BaseUnits |= BaseUnits.A;
			if(K != 0) BaseUnits |= BaseUnits.K;
			if(mol != 0) BaseUnits |= BaseUnits.mol;
			if(cd != 0) BaseUnits |= BaseUnits.cd;
			if(rad != 0) BaseUnits |= BaseUnits.rad;
			if (BaseUnits == 0) BaseUnits |= BaseUnits.dimentionless;
			
			NumSize = (s > 0 ? s : 0) +
			          (kg > 0 ? kg : 0) +
			          (m > 0 ? m : 0) +
			          (A > 0 ? A : 0) +
			          (K > 0 ? K : 0) +
			          (mol > 0 ? mol : 0) +
			          (cd > 0 ? cd : 0) +
			          (rad > 0 ? rad : 0);
			
			DenSize = (s < 0 ? -s : 0) +
			          (kg < 0 ? -kg : 0) +
			          (m < 0 ? -m : 0) +
			          (A < 0 ? -A : 0) +
			          (K < 0 ? -K : 0) +
			          (mol < 0 ? -mol : 0) +
			          (cd < 0 ? -cd : 0) +
			          (rad < 0 ? -rad : 0);
		}

		private void ProcessString(bool positive, System.Action<string, int> proc)
		{
			int mul = positive ? 1 : -1;
			if (s * mul > 0) proc("s", s* mul);
			if (kg * mul > 0) proc("kg", kg* mul);
			if (m * mul > 0) proc("m", m* mul);
			if (A * mul > 0) proc("A", A* mul);
			if (K * mul > 0) proc("K", K* mul);
			if (mol * mul > 0) proc("mol", mol* mul);
			if (cd * mul > 0) proc("cd", cd* mul);
			if (rad * mul > 0) proc("rad", rad* mul);
		}

		public bool All(System.Func<BaseUnits, int, bool> proc)
		{
			if (s != 0 && !proc(BaseUnits.s, s)) return false;
			if (kg != 0 && !proc(BaseUnits.kg, kg)) return false;
			if (m != 0 && !proc(BaseUnits.m, m)) return false;
			if (A != 0 && !proc(BaseUnits.A, A)) return false;
			if (K != 0 && !proc(BaseUnits.K, K)) return false;
			if (mol != 0 && !proc(BaseUnits.mol, mol)) return false;
			if (cd != 0 && !proc(BaseUnits.cd, cd)) return false;
			if (rad != 0 && !proc(BaseUnits.rad, rad)) return false;
			return true;
		}

		private static void NameProcessor(string u, int p)
		{
			Buffer.Append(u);
			if (p > 1) Buffer.Append(p);
		}
		
		public string GetName()
		{
			if (!HasUnit) return string.Empty;
			Buffer.Clear();

			if (NumSize > 0)
			{
				ProcessString(true, NameProcessor);
			}
			else
			{
				Buffer.Append("_1");
			}

			if (DenSize > 0)
			{
				Buffer.Append('p');
				ProcessString(false, NameProcessor);
			}

			var result = Buffer.ToString();
			Buffer.Clear();
			return result;
		}


		private static void DescriptionProcessor(string u, int p)
		{
			Buffer.Append(u);
			if (p > 1)
			{
				Buffer.Append(superscripts[p]);
			}
			Buffer.Append('·');
		}

		public string GetDescription()
		{
			if (!HasUnit) return string.Empty;
			Buffer.Clear();

			if (NumSize > 0)
			{
				ProcessString(true, DescriptionProcessor);
				Buffer.Remove(Buffer.Length - 1, 1);
			}
			else
			{
				Buffer.Append("1");
			}

			if (DenSize > 0)
			{
				Buffer.Append('/');
				ProcessString(false, DescriptionProcessor);
				Buffer.Remove(Buffer.Length - 1, 1);
			}

			var result = Buffer.ToString();
			Buffer.Clear();
			return result;
		}

		public IEnumerator<Fraction> GetEnumerator()
		{
			throw new NotImplementedException();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}