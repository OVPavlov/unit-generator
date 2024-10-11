using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Metric.Editor.Generator
{

	[System.Flags]
	public enum Tag
	{
		/// <summary>
		/// has no special case
		/// </summary>
		AutoDerived = 1 << 0,

		/// <summary>
		/// Base Units:
		/// s	    second	    time
		/// m	    metre	    length
		/// kg	    kilogram	mass
		/// A	    ampere	    electric current
		/// K	    kelvin	    thermodynamic temperature
		/// mol	    mole	    amount of substance
		/// cd	    candela	    luminous intensity
		/// </summary>
		Base = 1 << 1,

		/// <summary>
		/// radian[N 1]	rad	plane angle	m/m	1
		/// steradian[N 1]	sr	solid angle	m2/m2	1
		/// hertz	Hz	frequency	s−1	
		/// newton	N	force	kg⋅m⋅s−2	
		/// pascal	Pa	pressure, stress	kg⋅m−1⋅s−2	N/m2 = J/m3
		/// joule	J	energy, work, amount of heat	kg⋅m2⋅s−2	N⋅m = Pa⋅m3
		/// watt	W	power, radiant flux	kg⋅m2⋅s−3	J/s
		/// coulomb	C	electric charge	s⋅A	
		/// volt	V	electric potential, (voltage in US; electric tension)	kg⋅m2⋅s−3⋅A−1	W/A = J/C
		/// farad	F	capacitance	kg−1⋅m−2⋅s4⋅A2	C/V = C2/J
		/// ohm	Ω	resistance	kg⋅m2⋅s−3⋅A−2	V/A = J⋅s/C2
		/// siemens	S	electrical conductance	kg−1⋅m−2⋅s3⋅A2	Ω−1
		/// weber	Wb	magnetic flux	kg⋅m2⋅s−2⋅A−1	V⋅s
		/// tesla	T	magnetic flux density	kg⋅s−2⋅A−1	Wb/m2
		/// henry	H	inductance	kg⋅m2⋅s−2⋅A−2	Wb/A
		/// degree Celsius	°C	temperature	K	
		/// lumen	lm	luminous flux	cd⋅m2/m2	cd⋅sr
		/// lux	lx	illuminance	cd⋅m2/m4	lm/m2 = cd⋅sr⋅m−2
		/// becquerel	Bq	activity referred to a radionuclide (decays per unit time)	s−1	
		/// gray	Gy	absorbed dose, kerma	m2⋅s−2	J/kg
		/// sievert	Sv	equivalent dose	m2⋅s−2	J/kg
		/// katal	kat	catalytic activity	mol⋅s−1	
		/// </summary>
		Special = 1 << 2,

		/// <summary>
		/// square metre	m2	area	A
		/// cubic metre	m3	volume	V
		/// metre per second	m/s	speed, velocity	v
		/// metre per second squared	m/s2	acceleration	a
		/// reciprocal metre	m−1	wavenumber	σ, ṽ
		/// vergence (optics)	V, 1/f
		/// kilogram per cubic metre	kg/m3	density	ρ
		/// kilogram per square metre	kg/m2	surface density	ρA
		/// cubic metre per kilogram	m3/kg	specific volume	v
		/// ampere per square metre	A/m2	current density	j
		/// ampere per metre	A/m	magnetic field strength	H
		/// mole per cubic metre	mol/m3	concentration	c
		/// kilogram per cubic metre	kg/m3	mass concentration	ρ, γ
		/// candela per square metre	cd/m2	luminance	Lv
		/// </summary>
		Coherent = 1 << 3,

		/// <summary>
		/// pascal-second	Pa⋅s	dynamic viscosity	m−1⋅kg⋅s−1
		/// newton-metre	N⋅m	moment of force	m2⋅kg⋅s−2
		/// newton per metre	N/m	surface tension	kg⋅s−2
		/// radian per second	rad/s	angular velocity, angular frequency	s−1
		/// radian per second squared	rad/s2	angular acceleration	s−2
		/// watt per square metre	W/m2	heat flux density, irradiance	kg⋅s−3
		/// joule per kelvin	J/K	entropy, heat capacity	m2⋅kg⋅s−2⋅K−1
		/// joule per kilogram-kelvin	J/(kg⋅K)	specific heat capacity, specific entropy	m2⋅s−2⋅K−1
		/// joule per kilogram	J/kg	specific energy	m2⋅s−2
		/// watt per metre-kelvin	W/(m⋅K)	thermal conductivity	m⋅kg⋅s−3⋅K−1
		/// joule per cubic metre	J/m3	energy density	m−1⋅kg⋅s−2
		/// volt per metre	V/m	electric field strength	m⋅kg⋅s−3⋅A−1
		/// coulomb per cubic metre	C/m3	electric charge density	m−3⋅s⋅A
		/// coulomb per square metre	C/m2	surface charge density, electric flux density, electric displacement	m−2⋅s⋅A
		/// farad per metre	F/m	permittivity	m−3⋅kg−1⋅s4⋅A2
		/// henry per metre	H/m	permeability	m⋅kg⋅s−2⋅A−2
		/// joule per mole	J/mol	molar energy	m2⋅kg⋅s−2⋅mol−1
		/// joule per mole-kelvin	J/(mol⋅K)	molar entropy, molar heat capacity	m2⋅kg⋅s−2⋅K−1⋅mol−1
		/// coulomb per kilogram	C/kg	exposure (x- and γ-rays)	kg−1⋅s⋅A
		/// gray per second	Gy/s	absorbed dose rate	m2⋅s−3
		/// watt per steradian	W/sr	radiant intensity	m2⋅kg⋅s−3
		/// watt per square metre-steradian	W/(m2⋅sr)	radiance	kg⋅s−3
		/// katal per cubic metre	kat/m3	catalytic activity concentration	m−3⋅s−1⋅mol
		/// </summary>
		DerivedFromSpecial = 1 << 4,

		Vector = 1 << 5,

	}


	internal class Unit
	{
		public string Name;
		public string Summary;
		public string VarName;
		public bool VisibleInEditor;
		public List<string> AddFields = new List<string>();
		public Fraction Fraction;
		public HashSet<Op> Ops = new();
		public int VecSize => Fraction.VecSize;

		public Tag Tag;
		public bool IsFundamental => Fraction.Dict.Count == 1 && Fraction.Dict.First().Value == 1;
		public char InField => Fraction.VecSize > 1 ? 'v' : 'f';

		public void Init()
		{
			if (Name == null)
			{
				Name = Fraction.GetName();
			}

			if (Summary == null)
			{
				Summary = Fraction.GetDescription();
			}
		}
	}
}