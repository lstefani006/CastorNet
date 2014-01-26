using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Mips24k
{

	abstract class MipsAssembly
	{
		public const string regZero = "r0";
		public const string regWin = "r1";
		public const string regJs = "r2";
		public const string regReserved = "r3";

		public const string regReturn = "r4";

		protected Util.Set<string> _in = new Util.Set<string>();
		protected Util.Set<string> _out = new Util.Set<string>();
		protected static Util.Set<string> S_lbl = new Util.Set<string>();
		protected Util.Set<string> _lbl = new Util.Set<string>();
		protected Util.Set<int> _prev = new Util.Set<int>();

		public Util.Set<int> Succ = new Util.Set<int>();

		public static void SetLabel(string lbl) { S_lbl.Add(lbl); }

		protected MipsAssembly()
		{
			_lbl = S_lbl;
			S_lbl = new Util.Set<string>();
		}

		public abstract bool ComputeLive(Util.Set<string> rout);
		public abstract void Substitute(string temp, string reg);
		//public abstract void Substitute(string temp, int reg);

		public abstract Util.Set<int> GetSucc(int pc, Context ctx);

		protected string InToString()
		{
			string r = "[";
			bool first = true;
			for (int i = 0; i < _in.Count; ++i)
			{
				if (first == false)
					r += ", ";
				first = false;
				r += _in[i];
			}
			r = r + "] ";

			r = r.PadRight(35);

			return r + Util.F("{0,-6}", this._lbl);
		}

		public Util.Set<string> In { get { return _in; } }
		public Util.Set<string> Out { get { return _out; } }
		public Util.Set<string> Lbl { get { return this._lbl; } }
	}
	class MipsX : MipsAssembly
	{
		OpCode op;
		string rd;
		string rt;
		string rs;
		int kk;
		uint imm;

		public MipsX(OpCode op, string rd, string rt, string rs, int kk, uint imm)
		{
			this.op = op;

			this.rd = rd;
			this.rt = rt;
			this.rs = rs;

			this.kk = kk;
			this.imm = imm;
		}
		public override bool ComputeLive(Util.Set<string> prev)
		{
			// in = (out - def) u use
			//
			// out varibili vive dopo l'istruzione
			// def variabili definite (scritte) nell'istruzione
			// use variabili argomenti (lette) nell'instruzione

			var rout = new Util.Set<string>(prev);
			var rin = new Util.Set<string>(prev);

			if (op == OpCode.X_sti || op == OpCode.X_stiu)
			{
				// rd e` un ingresso non una uscita !!!
				rin.Add(rs);
				rin.Add(rt);
				rin.Add(rd);
			}
			else
			{
				rin.Remove(rd);
				rin.Add(rs);
				rin.Add(rt);
			}

			bool changed = false;
			if (rin != _in || rout != _out)
				changed = true;

			_in = rin;
			_out = rout;

			return changed;
		}

		public override Util.Set<int> GetSucc(int pc, Context ctx)
		{
			return new Util.Set<int>() { pc + 1 };
		}


		public override string ToString()
		{
			string f = Enum.GetName(typeof(Mips24k.OpCode), op).Substring(2);
			switch (op)
			{
			case OpCode.X_sti:
			case OpCode.X_stiu:
				{
					string r = Util.F("{0,-6} [{2}, #{4}*{3}, #{5}], {1}", f, rd, rt, rs, 1 << kk, imm);
					return Util.F("{0} {1}", InToString(), r);
				}
			case OpCode.X_ldi:
			case OpCode.X_ldiu:
				{
					string r = Util.F("{0,-6} {1}, [{2}, #{4}*{3}, #{5}]", f, rd, rt, rs, 1 << kk, imm);
					return Util.F("{0} {1}", InToString(), r);
				}

			default:
				Debug.Assert(false);
				return null; ;
			}
		}

		public override void Substitute(string temp, string reg)
		{
			if (rd == temp) rd = reg;
			if (rs == temp) rs = reg;
			if (rt == temp) rt = reg;
		}

	}
	class MipsR : MipsAssembly
	{
		Mips24k.OpCode op;
		string rd, rs, rt;
		int shmat;
		Func_R func;

		public MipsR(Mips24k.OpCode op, string rd, string rs, string rt, Func_R func)
		{
			this.op = op;

			this.rd = rd;
			this.rs = rs;
			this.rt = rt;

			this.func = func;
		}
		public MipsR(Mips24k.OpCode op, string rd, string rs, string rt, int shmat, Func_R func)
		{
			this.op = op;

			this.rd = rd;
			this.rs = rs;
			this.rt = rt;

			this.shmat = shmat;
			this.func = func;
		}

		public override bool ComputeLive(Util.Set<string> prev)
		{
			var rout = new Util.Set<string>(prev);
			var rin = new Util.Set<string>(prev);

			rin.Remove(rd);
			rin.Add(rs);
			rin.Add(rt);

			bool changed = false;
			if (_in != rin || _out != rout)
				changed = true;

			_in = rin;
			_out = rout;

			return changed;
		}

		public override string ToString()
		{
			string f = Enum.GetName(typeof(Func_R), func);
			string r;

			switch (func)
			{
			case Func_R.sll:
			case Func_R.srl:
			case Func_R.sra:
				r = Util.F("{0,-6} {1}, {2}, #{3}", f, rd, rt, shmat);
				break;

			case Func_R.jr:
			case Func_R.js:
				r = Util.F("{0,-6} {1}", f, rd);
				break;

			default:
				r = Util.F("{0,-6} {1}, {2}, {3}", f, rd, rt, rs);
				break;
			}

			return Util.F("{0} {1}", InToString(), r);
		}

		public override void Substitute(string temp, string reg)
		{
			if (rd == temp) rd = reg;
			if (rs == temp) rs = reg;
			if (rt == temp) rt = reg;
		}

		public override Util.Set<int> GetSucc(int pc, Context ctx)
		{
			if (func == Func_R.jr)
				Debug.Assert(false, "Non si puo` calcolare il next di un j r2");

			// notare che invece Func_R.js ha con succ pc+1
			return new Util.Set<int>() { pc + 1 };
		}
	}
	class MipsI : MipsAssembly, IEquatable<MipsI>
	{
		Mips24k.OpCode op;
		string rs, rt;
		int C;
		string lbl;

		public MipsI(Mips24k.OpCode op, string rs, string rt, int C)
		{
			this.op = op;
			this.rs = rs;
			this.rt = rt;
			this.C = C;
		}
		public MipsI(Mips24k.OpCode op, string rs, string rt, string lbl)
		{
			this.op = op;
			this.rs = rs;
			this.rt = rt;
			this.lbl = lbl;
		}

		public override bool ComputeLive(Util.Set<string> prev)
		{
			var rout = new Util.Set<string>(prev);
			var rin = new Util.Set<string>(prev);
			switch (op)
			{
			case OpCode.I_sw:
			case OpCode.I_sh:
			case OpCode.I_sb:
				rin.Add(rs);
				rin.Add(rt);
				break;

			case OpCode.I_beq:
			case OpCode.I_bne:
			case OpCode.I_bgt:
			case OpCode.I_bge:
			case OpCode.I_blt:
			case OpCode.I_ble:
				rin.Add(rs);
				rin.Add(rt);
				break;

			case OpCode.I_mvw:
				break;

			case OpCode.I_lr:
				rin.Add(rs);
				rin.Add(lbl);
				rin.Remove(rt);
				break;

			case OpCode.I_sr:
				rin.Add(rt);
				rin.Add(rs);
				rin.Remove(lbl);
				break;

			default:
				rin.Remove(rt);
				rin.Add(rs);
				break;
			}
			bool changed = false;
			if (_in != rin || _out != rout)
				changed = true;

			_in = rin;
			_out = rout;

			return changed;
		}

		public override Util.Set<int> GetSucc(int pc, Context ctx)
		{
			var ret = new Util.Set<int>() { pc + 1 };
			if (op == OpCode.I_beq || op == OpCode.I_bne ||
				op == OpCode.I_bgt || op == OpCode.I_bge ||
				op == OpCode.I_blt || op == OpCode.I_ble)
			{
				if (this.lbl != null)
					ret.Add(ctx.GetAddrFromLabel(this.lbl));
				else
					ret.Add(this.C);
			}

			return ret;
		}

		public override string ToString()
		{
			string s = Enum.GetName(typeof(OpCode), op);
			s = s.Remove(0, 2); // tolgo I_

			string r;
			switch (op)
			{
			case OpCode.I_lw:
			case OpCode.I_lh:
			case OpCode.I_lb:
			case OpCode.I_lhu:
			case OpCode.I_lbu:
				// U[rt] = *(uint*)((byte*)mem + U[rs] + (uint)C);
				r = Util.F("lw     {2}, [{0}, {1}]", rs, C, rt);
				break;

			case OpCode.I_sw:
			case OpCode.I_sh:
			case OpCode.I_sb:
				// *(uint*)((byte*)mem + U[rs] + (uint)C) = U[rt];
				r = Util.F("sw     [{0}, {1}], {2}", rs, C, rt);
				break;

			case OpCode.I_lui:
				r = Util.F("lui    {0} #{1}", rt, C);
				break;

			case OpCode.I_beq:
			case OpCode.I_bne:
			case OpCode.I_bgt:
			case OpCode.I_bge:
			case OpCode.I_blt:
			case OpCode.I_ble:
				{
					if (this.lbl == null)
						r = Util.F("{0,-6} {1}, {2}, {3}", s, rt, rs, C);
					else
						r = Util.F("{0,-6} {1}, {2}, {3}", s, rt, rs, lbl);
				}
				break;

			case OpCode.I_mvw:
				{
					r = Util.F("{0,-6} {1}", s, this.C);
				}
				break;

			case OpCode.I_lr:
				if (this.lbl == null)
					r = Util.F("{0,-6} {1}, r[{2}, {3}]", s, rt, this.C, rs);
				else
					r = Util.F("{0,-6} {1}, r[{2}, {3}]", s, rt, this.lbl, rs);
				break;
			case OpCode.I_sr:
				if (this.lbl == null)
					r = Util.F("{0,-6} r[{2}, {3}], {1}", s, rt, this.C, rs);
				else
					r = Util.F("{0,-6} r[{2}, {3}], {1}", s, rt, this.lbl, rs);
				break;

			default:
				{
					if (this.lbl == null)
						r = Util.F("{0,-6} {1}, {2}, #{3}", s, rt, rs, C);
					else
						r = Util.F("{0,-6} {1}, {2}, #{3}", s, rt, rs, lbl);
				}
				break;
			}
			return Util.F("{0} {1}", InToString(), r);
		}

		public override void Substitute(string temp, string reg)
		{
			if (rs == temp) rs = reg;
			if (rt == temp) rt = reg;
		}
		public void SetC(int C)
		{
			this.C = C;
			this.lbl = null;
		}

		public bool Equals(MipsI other)
		{
			return this == other;
		}

		public string Window { get { return this.lbl; } }
	}
	class MipsJ : MipsAssembly, IEquatable<MipsJ>
	{
		Mips24k.OpCode op;
		int C;
		string lbl;
		Util.Set<string> rUsed;
		Util.Set<string> rOut;

		public MipsJ(Mips24k.OpCode op, int C)
		{
			this.op = op;
			this.C = C;
		}
		public MipsJ(Mips24k.OpCode op, string lbl, Util.Set<string> rUsed, Util.Set<string> rOut)
		{
			this.op = op;
			this.lbl = lbl;
			this.rUsed = rUsed;
			this.rOut = rOut;
		}
		public override bool ComputeLive(Util.Set<string> prev)
		{
			if (op == OpCode.J_ret)
			{
				prev.Add(MipsAssembly.regZero);
				prev.Add(MipsAssembly.regWin);
				prev.Add(MipsAssembly.regJs);
				prev.Add(MipsAssembly.regReserved);
			}

			var rout = new Util.Set<string>(prev);
			var rin = new Util.Set<string>(prev);

			if ((object)rOut != null)
				foreach (var r in this.rOut)
					rin.Remove(r);
			
			if ((object)rUsed != null) 
				foreach (var r in this.rUsed)
					rin.Add(r);




			bool changed = false;
			if (_in != rin || _out != rout)
				changed = true;

			_in = rin;
			_out = rout;
			return changed;
		}

		public override string ToString()
		{
			string r = Enum.GetName(typeof(Mips24k.OpCode), this.op).Remove(0, 2);
			if (op == OpCode.J_ret)
			{
				return Util.F("{0} {1,-6}", InToString(), r);
			}
			else
			{
				if (this.lbl == null || this.lbl == "")
					return Util.F("{0} {1,-6} {2}", InToString(), r, C);
				else
					return Util.F("{0} {1,-6} {2}", InToString(), r, lbl);
			}
		}
		public override void Substitute(string temp, string reg)
		{
		}

		public override Util.Set<int> GetSucc(int pc, Context ctx)
		{
			if (op == OpCode.J_ret) return new Util.Set<int>();
			if (op == OpCode.J_js) return new Util.Set<int>() { pc + 1 };
			return new Util.Set<int>() { ctx.GetAddrFromLabel(this.lbl) };
		}


		public bool Equals(MipsJ other)
		{
			return this == other;
		}
	}
}