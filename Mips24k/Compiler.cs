using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using ULib.LLParserLexerLib;

namespace Mips24k
{
	public class ParserRoot : SourceTrackable, IAST
	{
		public ParserRoot(string f, int n) : base(f, n) { }
		public ParserRoot(ISourceTrackable n) : base(n != null ? n.fileName : "", n != null ? n.lineNu : 0) { }

		protected static Value EvalNumInReg(int ra, Context ctx, string rdest)
		{
			string r = Util.F("#{0}", ra);
			return EvalNumInReg(r, ctx, rdest);
		}
		protected static Value EvalNumInReg(int ra, Context ctx)
		{
			string r = Util.F("#{0}", ra);
			return EvalNumInReg(r, ctx);
		}
		protected static Value EvalNumInReg(Value ra, Context ctx, string rdest)
		{
			return EvalNumInReg(ra.value, ctx, rdest);
		}
		protected static Value EvalNumInReg(Value ra, Context ctx)
		{
			return EvalNumInReg(ra.value, ctx);
		}

		/// <summary>
		/// mette il numero in un registo.
		/// Se il numero e` zero ritorna r0.
		/// Altrimenti mette un numero nel registro rdest
		/// </summary>
		/// <param name="ra"></param>
		/// <param name="ctx"></param>
		/// <param name="rdest"></param>
		/// <returns></returns>
		protected static Value EvalNumInReg(string ra, Context ctx, string rdest)
		{
			Debug.Assert(rdest != null);

			decimal d = Util.ParseDecimal(ra.Substring(1));
			if (d == 0)
			{
				ctx.R(Func_R.add, rdest, MipsAssembly.regZero, MipsAssembly.regZero);
			}
			else if (short.MinValue <= d && d <= short.MaxValue)
			{
				ctx.I(OpCode.I_addi, rdest, MipsAssembly.regZero, (int)d);
			}
			else
			{
				ctx.I(OpCode.I_lui, rdest, MipsAssembly.regZero, (int)((uint)d) >> 16);
				ctx.I(OpCode.I_ori, rdest, rdest, (int)((uint)d) & 0xffff);
			}
			return new Value(rdest, TypeRoot.Int);
		}
		protected static Value EvalNumInReg(string ra, Context ctx)
		{
			decimal d = Util.ParseDecimal(ra.Substring(1));
			if (d == 0)
			{
				return new Value(MipsAssembly.regZero, TypeRoot.Int);
			}
			else if (short.MinValue <= d && d <= short.MaxValue)
			{
				ra = ctx.NewTemp();
				ctx.I(OpCode.I_addi, ra, MipsAssembly.regZero, (int)d);
			}
			else
			{
				ra = ctx.NewTemp();
				ctx.I(OpCode.I_lui, ra, MipsAssembly.regZero, (int)((uint)d) >> 16);
				ctx.I(OpCode.I_ori, ra, ra, (int)((uint)d) & 0xffff);
			}
			return new Value(ra, TypeRoot.Int);
		}
		protected static bool IsNum(Value ra)
		{
			return ra.value.StartsWith("#");
		}
		protected static bool IsShort(Value ra)
		{
			if (ra.value.StartsWith("#"))
			{
				decimal d = Util.ParseDecimal(ra.value.Substring(1));
				return short.MinValue <= d && d <= short.MaxValue;
			}
			return false;
		}
		protected static bool IsNegShort(Value ra)
		{
			if (ra.value.StartsWith("#"))
			{
				decimal d = -Util.ParseDecimal(ra.value.Substring(1));
				return short.MinValue <= d && d <= short.MaxValue;
			}
			return false;
		}
		protected static short ParseShort(Value ra)
		{
			if (ra.value.StartsWith("#"))
			{
				decimal d = Util.ParseDecimal(ra.value.Substring(1));
				Debug.Assert(short.MinValue <= d && d <= short.MaxValue);
				return (short)d;
			}
			Debug.Assert(false);
			return 0;
		}
		protected static short ParseNegShort(Value ra)
		{
			if (ra.value.StartsWith("#"))
			{
				decimal d = -Util.ParseDecimal(ra.value.Substring(1));
				Debug.Assert(short.MinValue <= d && d <= short.MaxValue);
				return (short)d;
			}
			Debug.Assert(false);
			return 0;
		}
		protected static int ParseInt(Value ra)
		{
			if (ra.value.StartsWith("#"))
			{
				decimal d = Util.ParseDecimal(ra.value.Substring(1));
				Debug.Assert(int.MinValue <= d && d <= int.MaxValue);
				return (int)d;
			}
			Debug.Assert(false);
			return 0;
		}
	}

	public class Value
	{
		public Value(string value, TypeRoot type) { this.value = value; this.type = type; }
		public readonly string value;
		public readonly TypeRoot type;
	}

	public abstract class ExprRoot : ParserRoot
	{
		protected ExprRoot(ISourceTrackable sc) : base(sc) {}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
		public override bool Equals(object obj)
		{
			return base.Equals(obj);
		}

		/// <summary>
		/// Valuta l'espressione e 
		/// ritorna un registro o un numero
		/// se ritorna un registro non e` detto che sia rdest.
		/// se il numero e` zero ritorna il numero
		/// </summary>
		/// <param name="ctx"></param>
		/// <param name="rdest"></param>
		/// <returns></returns>
		public abstract Value EvalForValue(Context ctx, string rdest);
		public abstract Value EvalForAssign(Context ctx);
		public virtual void EvalForBool(Context ctx, string lbl_true, string lbl_false)
		{
			Value r = EvalForAssign(ctx);
			if (r.type != TypeRoot.Bool)
				throw new SyntaxError(this, "statement requires a boolean expression");

			if (IsNum(r))
			{
				int num = ParseInt(r);
				if (lbl_true != null)
				{
					if (num != 0)
						ctx.J(OpCode.J_j, lbl_true);
				}
				else if (lbl_false != null)
				{
					if (num == 0)
						ctx.J(OpCode.J_j, lbl_false);
				}
				else
					Debug.Assert(false);
			}
			else
			{
				if (lbl_true != null)
					ctx.I(OpCode.I_bne, r.value, MipsAssembly.regZero, lbl_true);
				else if (lbl_false != null)
					ctx.I(OpCode.I_beq, r.value, MipsAssembly.regZero, lbl_false);
				else Debug.Assert(false);
			}
		}

		//public static ExprRoot operator +(ExprRoot a, ExprRoot b) { return new ExprAdd(a, b); }
		//public static ExprRoot operator -(ExprRoot a, ExprRoot b) { return new ExprSub(a, b); }
		//public static ExprRoot operator *(ExprRoot a, ExprRoot b) { return new ExprBinOp(a, "*", b); }
		//public static ExprRoot operator /(ExprRoot a, ExprRoot b) { return new ExprBinOp(a, "/", b); }
		//public static ExprRoot operator %(ExprRoot a, ExprRoot b) { return new ExprBinOp(a, "%", b); }

		//public static ExprRoot operator ==(ExprRoot a, ExprRoot b) { return new ExprBinOp(a, "==", b); }
		//public static ExprRoot operator !=(ExprRoot a, ExprRoot b) { return new ExprBinOp(a, "!=", b); }
		//public static ExprRoot operator >(ExprRoot a, ExprRoot b) { return new ExprBinOp(a, ">", b); }
		//public static ExprRoot operator >=(ExprRoot a, ExprRoot b) { return new ExprBinOp(a, ">=", b); }
		//public static ExprRoot operator <(ExprRoot a, ExprRoot b) { return new ExprBinOp(a, "<", b); }
		//public static ExprRoot operator <=(ExprRoot a, ExprRoot b) { return new ExprBinOp(a, "<=", b); }

		//public static ExprRoot AndAnd(ExprRoot a, ExprRoot b) { return new ExprBinOp(a, "&&", b); }
		//public static ExprRoot OrOr(ExprRoot a, ExprRoot b) { return new ExprBinOp(a, "||", b); }

		//public ExprRoot this[ExprRoot a] { get { return new ExprArray(this, a); } }
	}

	public class ExprDiv : ExprBinOp { public ExprDiv(ExprRoot a, TokenAST op, ExprRoot b) : base(a, op, b) { } }
	public class ExprMul : ExprBinOp { public ExprMul(ExprRoot a, TokenAST op, ExprRoot b) : base(a, op, b) { } }
	public class ExprRem : ExprBinOp { public ExprRem(ExprRoot a, TokenAST op, ExprRoot b) : base(a, op, b) { } }
	public class ExprNum : ExprRoot
	{
		int num;
		TypeRoot ty;

		public ExprNum(int n, ISourceTrackable tk) :base(tk) { num = n; ty = TypeRoot.Int; }
		public ExprNum(TokenAST ast) : base(ast)
		{
			if (ast.v == "true" || ast.v == "false")
			{
				ty = TypeRoot.Bool;
				num = ast.v == "true" ? 1 : 0;
			}
			else
			{
				ty = TypeRoot.Int;
				num = int.Parse(ast.v, CultureInfo.InvariantCulture);
			}
		}

		public override Value EvalForValue(Context ctx, string rdest)
		{
			// se num==0  ==> r0
			// altrimenti ==> #num
			// ignoro SEMPRE rdest.
			string v = (num == 0) ? MipsAssembly.regZero : v = Util.F("#{0}", num);
			return new Value(v, this.ty);
		}
		public override Value EvalForAssign(Context ctx)
		{
			throw new ApplicationException("Cannot obtain l-value from a number");
		}
	}
	public class ExprVar : ExprRoot
	{
		public readonly ExprVar _n;
		public readonly TokenAST _var;

		public ExprVar(TokenAST var) : base(var) { this._var = var; }
		public ExprVar(string var) : base(null) { this._var = new TokenAST("", 0, 0, var, var); }
		public ExprVar(ExprVar n, TokenAST op, TokenAST var) : base(op) { this._n = n; this._var = var; }

		public override Value EvalForValue(Context ctx, string rdest)
		{
			switch (ctx.ResolveSymbol(_var.v))
			{
			case Context.SymbolType.Function:
				return new Value(U.F("#{0}", _var.v), new TypeDef(_var.v));

			case Context.SymbolType.Variable:
				return ctx.GetRegisterForLocalVariable(_var);

			default:
				throw new SyntaxError(_var, "variable '{0}' not found", _var.v);
			}
		}
		public override Value EvalForAssign(Context ctx)
		{
			return ctx.GetRegisterForLocalVariable(_var);
		}
	}
	public class ExprAdd : ExprBinOp
	{
		public ExprAdd(ExprRoot a, ISourceTrackable sc, ExprRoot b)
			: base(a, new TokenAST(sc, '+'), b)
		{
		}
		public override Value EvalForValue(Context ctx, string rdest)
		{
			var ra = a.EvalForValue(ctx, null);
			var rb = b.EvalForValue(ctx, null);

			if (ra.type != rb.type)
				throw new SyntaxError(this, "binary operator '{0}' requires equal types", this.op); 

			if (IsNum(ra) == true && IsNum(rb) == false)
			{
				// <num> + <reg> diventa <reg> + <num>
				U.Swap(ref ra, ref rb);
			}

			// se sono qui ho
			// <reg> + <reg>
			// <reg> + <num>
			// <num> + <num>

			if (IsNum(ra) && IsNum(rb))
			{
				// <num> + <num>
				int C = ParseInt(ra) + ParseInt(rb);
				var e = new ExprNum(C, this);
				return e.EvalForValue(ctx, rdest);
			}

			// se sono qui ho
			// <reg> + <reg>
			// <reg> + <num>

			if (IsNum(rb))
			{
				// <reg> + <num>

				if (IsShort(rb))
				{
					string tr = rdest != null ? rdest : ctx.NewTemp();
					ctx.I(OpCode.I_addi, tr, ra.value, ParseShort(rb));
					return new Value(tr, TypeRoot.Int);
				}

				// converto il numero in un registro.
				rb = EvalNumInReg(rb.value, ctx);
			}

			if (true)
			{
				// <reg> + <reg>

				string tr = rdest != null ? rdest : ctx.NewTemp();
				ctx.R(Func_R.add, tr, ra.value, rb.value);
				return new Value(tr, TypeRoot.Int);
			}
		}
	}
	public class ExprSub : ExprBinOp
	{
		public ExprSub(ExprRoot a, ISourceTrackable sc, ExprRoot b)
			: base(a, new TokenAST(sc, '-'), b)
		{
		}
		public override Value EvalForValue(Context ctx, string rdest)
		{
			var ra = a.EvalForValue(ctx, null);
			var rb = b.EvalForValue(ctx, null);

			// se sono qui ho
			// <reg> - <reg>
			// <reg> - <num>
			// <num> - <reg>
			// <num> - <num>

			if (IsNum(ra) && IsNum(rb))
			{
				// <num> - <num>
				int C = ParseInt(ra) - ParseInt(rb);
				var e = new ExprNum(C, this);
				return e.EvalForValue(ctx, rdest);
			}

			// se sono qui ho
			// <reg> - <reg>
			// <reg> - <num>
			// <num> - <reg>

			if (IsNum(ra) == false && IsNum(rb) == true)
			{
				// <reg> - <num>

				if (IsNegShort(rb))
				{
					string tr = rdest != null ? rdest : ctx.NewTemp();
					ctx.I(OpCode.I_addi, tr, ra.value, ParseNegShort(rb));
					return new Value(tr, TypeRoot.Int);
				}
			}

			if (IsNum(ra))
			{
				// <num> - <reg>
				ra = EvalNumInReg(ra.value, ctx);
			}

			if (IsNum(rb))
			{
				// <num> - <reg>
				rb = EvalNumInReg(rb.value, ctx);
			}

			if (true)
			{
				// <reg> + <reg>

				string tr = rdest != null ? rdest : ctx.NewTemp();
				ctx.R(Func_R.sub, tr, ra.value, rb.value);
				return new Value(tr, ra.type);
			}
		}
	}
	public class ExprNeg : ExprBinOp
	{
		public ExprNeg(TokenAST tk, ExprRoot a)
			: base(new ExprNum(0, tk), tk, a)
		{
		}
	}
	public class ExprPlus : ExprBinOp
	{
		public ExprPlus(TokenAST tk, ExprRoot a)
			: base(new ExprNum(0, tk), tk, a)
		{
		}
	}
	public class ExprBinOp : ExprRoot
	{
		readonly public ExprRoot a, b;
		readonly public string op;

		public ExprBinOp(ExprRoot a, TokenAST op, ExprRoot b)
			: base(op)
		{
			this.a = a;
			this.op = op.v;
			this.b = b;
		}

		public override void EvalForBool(Context ctx, string lbl_true, string lbl_false)
		{
			switch (op)
			{
			case "||":
				{
					if (lbl_true != null)
					{
						string lbl_or_true = lbl_true;
						this.a.EvalForBool(ctx, lbl_or_true, null);
						this.b.EvalForBool(ctx, lbl_or_true, null);
						// qui continue con <false>
					}
					else if (lbl_false != null)
					{
						string lbl_or_true = ctx.NewLbl();
						this.a.EvalForBool(ctx, lbl_or_true, null);
						this.b.EvalForBool(ctx, lbl_or_true, null);
						ctx.J(OpCode.J_j, lbl_false);
						ctx.L(lbl_or_true);
					}
					else
						Debug.Assert(false);
				}
				break;

			case "&&":
				{
					if (lbl_true != null)
					{
						string lbl_and_false = ctx.NewLbl();
						this.a.EvalForBool(ctx, null, lbl_and_false);
						this.b.EvalForBool(ctx, null, lbl_and_false);
						ctx.J(OpCode.J_j, lbl_true);
						ctx.L(lbl_and_false);
					}
					else if (lbl_false != null)
					{
						string lbl_and_false = lbl_false;
						this.a.EvalForBool(ctx, null, lbl_and_false);
						this.b.EvalForBool(ctx, null, lbl_and_false);
					}
					else
						Debug.Assert(false);
				}
				break;

			case "==":
			case "!=":
			case ">":
			case ">=":
			case "<":
			case "<=":
				{
					var aa = a.EvalForValue(ctx, null);
					var bb = b.EvalForValue(ctx, null);

					if (aa.type != bb.type)
						throw new SyntaxError(this, "operands must have the same type");

					if (IsNum(aa)) aa = EvalNumInReg(aa.value, ctx);
					if (IsNum(bb)) bb = EvalNumInReg(bb.value, ctx);

					if (lbl_true != null)
					{
						switch (op)
						{
						case "==": ctx.I(OpCode.I_beq, aa.value, bb.value, lbl_true); break;
						case "!=": ctx.I(OpCode.I_bne, aa.value, bb.value, lbl_true); break;
						case ">": ctx.I(OpCode.I_bgt, aa.value, bb.value, lbl_true); break;
						case ">=": ctx.I(OpCode.I_bge, aa.value, bb.value, lbl_true); break;
						case "<": ctx.I(OpCode.I_blt, aa.value, bb.value, lbl_true); break;
						case "<=": ctx.I(OpCode.I_ble, aa.value, bb.value, lbl_true); break;
						default: Debug.Assert(false); break;
						}
					}
					else if (lbl_false != null)
					{
						switch (op)
						{
						case "==": ctx.I(OpCode.I_bne, aa.value, bb.value, lbl_false); break;
						case "!=": ctx.I(OpCode.I_beq, aa.value, bb.value, lbl_false); break;
						case ">": ctx.I(OpCode.I_ble, aa.value, bb.value, lbl_false); break;
						case ">=": ctx.I(OpCode.I_blt, aa.value, bb.value, lbl_false); break;
						case "<": ctx.I(OpCode.I_bge, aa.value, bb.value, lbl_false); break;
						case "<=": ctx.I(OpCode.I_bgt, aa.value, bb.value, lbl_false); break;
						default: Debug.Assert(false); break;
						}
					}
					else
						Debug.Assert(false);
				}
				break;

			default:
				base.EvalForBool(ctx, lbl_true, lbl_false);
				break;
			}
		}

		public override Value EvalForValue(Context ctx, string rdest)
		{
			var ra = a.EvalForValue(ctx, null);
			var rb = b.EvalForValue(ctx, null);

			if (ra.type != rb.type)
				throw new SyntaxError(this, "binary operator '{0}' requires equal types", this.op); 

			if (IsNum(ra))
			{
				// il primo argomento deve essere sempre un registro
				// --> lo converto in registro prima....
				ra = EvalNumInReg(ra.value, ctx);
			}

			if (IsNum(rb))
			{
				if ((this.op == ">>" || this.op == "<<") && IsShort(rb))
				{
					string tr = rdest != null ? rdest : ctx.NewTemp();
					ctx.R(op == "<<" ? Func_R.sll : Func_R.srl, tr, ra.value, ParseInt(rb));

					return new Value(tr, TypeRoot.Int);
				}
				rb = EvalNumInReg(rb.value, ctx);
			}

			string t = rdest != null ? rdest : ctx.NewTemp();
			switch (op)
			{
			case "-":
				ctx.R(Func_R.sub, t, ra.value, rb.value);
				return new Value(t, TypeRoot.Int);

			case "*":
				ctx.R(Func_R.mult, t, ra.value, rb.value);
				return new Value(t, TypeRoot.Int);

			case "/":
				ctx.R(Func_R.div, t, ra.value, rb.value);
				return new Value(t, TypeRoot.Int);

			case "&":
				ctx.R(Func_R.and, t, ra.value, rb.value);
				return new Value(t, TypeRoot.Int);

			case "|":
				ctx.R(Func_R.or_, t, ra.value, rb.value);
				return new Value(t, TypeRoot.Int);

			case "^":
				ctx.R(Func_R.xor, t, ra.value, rb.value);
				return new Value(t, TypeRoot.Int);

			case "%":
				ctx.R(Func_R.rem, t, ra.value, rb.value);
				return new Value(t, TypeRoot.Int);

			case "<<":
				ctx.R(Func_R.sllv, t, ra.value, rb.value);
				return new Value(t, TypeRoot.Int);

			case ">>":
				ctx.R(Func_R.srlv, t, ra.value, rb.value);
				return new Value(t, TypeRoot.Int);

			case "==":
			case "!=":
			case ">":
			case ">=":
			case "<":
			case "<=":
				{
					string lbl_true = ctx.NewLbl();
					ctx.I(OpCode.I_addi, t, MipsAssembly.regZero, 1);
					EvalForBool(ctx, lbl_true, null);
					ctx.R(Func_R.add, t, MipsAssembly.regZero, MipsAssembly.regZero);
					ctx.L(lbl_true);
					return new Value(t, TypeRoot.Bool);
				}

			default:
				Debug.Assert(false);
				return null;
			}
		}

		public override Value EvalForAssign(Context ctx)
		{
			throw new ApplicationException("Cannot obtain l-value from an expression");
		}

	}
	public class ExprArray : ExprRoot
	{
		public readonly ExprRoot a;
		public readonly ExprRoot b;

		public ExprArray(ExprRoot a, TokenAST op, ExprRoot b)
			: base(op)
		{
			this.a = a;
			this.b = b;
		}
		public ExprArray(ExprRoot a, TokenAST op, ExprList b)
			: base(op)
		{
			// per ora
			this.a = a;
			this.b = b[0];
		}

		public override Value EvalForValue(Context ctx, string rdest)
		{
			var ra = a.EvalForValue(ctx, null);
			var rb = b.EvalForValue(ctx, null);

			if (IsNum(ra))
				throw new ApplicationException("deve essere una variabile");

			int disp = 16; // provo a dare un displacement "fisso" (gli array avranno sempre un disp fisso).

			if (IsNum(rb))
			{
				int C = disp + 4 * ParseInt(rb);
				if (short.MinValue <= C && C <= short.MaxValue)
				{
					// e` un indice numerico.
					string t = rdest != null ? rdest : ctx.NewTemp();
					ctx.I(OpCode.I_lw, t, ra.value, C);
					return new Value(t, TypeRoot.Int);
				}

				// se sono qua vuol dire che rb + disp non sta in 16 bit.
				if (true)
				{
					rb = EvalNumInReg(C, ctx);
					string t = rdest != null ? rdest : ctx.NewTemp();
					ctx.X(OpCode.X_ldi, t, ra.value, rb.value, 2, 0);
					return new Value(t, TypeRoot.Int);
				}
			}
			else
			{
				// ra e` sempre un registro
				// rb e` un registro.

				if (disp >= 0 && disp <= 0x1ff)
				{
					string t1 = rdest != null ? rdest : ctx.NewTemp();
					ctx.X(OpCode.X_ldi, t1, ra.value, rb.value, 2, (uint)disp);
					return new Value(t1, TypeRoot.Int);
				}
				else
				{
					var t1 = EvalNumInReg(disp, ctx);
					var t2 = ctx.NewTemp();
					ctx.R(Func_R.add, t2, ra.value, t1.value);
					string t3 = rdest != null ? rdest : ctx.NewTemp();
					ctx.X(OpCode.X_ldi, t3, t2, rb.value, 2, 0);
					
					return new Value(t3, TypeRoot.Int);
				}
			}
		}
		public override Value EvalForAssign(Context ctx)
		{
			var ra = a.EvalForValue(ctx, null);
			var rb = b.EvalForValue(ctx, null);

			if (IsNum(ra))
				throw new ApplicationException("deve essere una variabile");

			int disp = 16; // provo a dare un displacement "fisso" (gli array avranno sempre un disp fisso).

			// codice per un array "puro" ossia senza displacement rispetto a ra
			// (oltre l'indice)
			if (IsNum(rb))
			{
				int C = disp + 4 * ParseInt(rb);
				if (short.MinValue <= C && C <= short.MaxValue)
				{
					// e` un indice numerico.
					return new Value(Util.F("[sw,{0},{1}]", ra, C), TypeRoot.Int);
				}

				// se sono qua vuol dire che rb + disp non sta in 16 bit.
				if (true)
				{
					var t1 = EvalNumInReg(C, ctx);
					string t2 = ctx.NewTemp();
					ctx.R(Func_R.add, t2, ra.value, t1.value);
					return new Value(Util.F("[sw,{0},0]", t2), TypeRoot.Int);
				}
			}
			else
			{
				// ra e` sempre un registro
				// rb e` un registro.

				if (disp >= 0 && disp <= 0x1ff)
				{
					return new Value(Util.F("[sti,{0},{1},{2},{3}]", ra.value, rb.value, 2, disp), TypeRoot.Int);
				}
				else if (short.MinValue <= disp && disp <= short.MaxValue)
				{
					// disp e` "grosso" ma sta in uno short.
					string t1 = ctx.NewTemp();
					ctx.I(OpCode.I_addi, t1, ra.value, disp);
					return new Value(Util.F("[sti,{0},{1},{2},{3}]", t1, rb.value, 2, 0), TypeRoot.Int);
				}
				else
				{
					var t1 = EvalNumInReg(disp, ctx);
					var t2 = ctx.NewTemp();
					ctx.R(Func_R.add, t2, ra.value, t1.value);
					return new Value(Util.F("[sti,{0},{1},{2},{3}]", t2, rb.value, 2, 0), TypeRoot.Int);
				}
			}
		}
	}
	public class ExprAss : ExprRoot
	{
		public ExprAss(ExprRoot a, TokenAST op, ExprRoot b) : base(op) { this.a = a; this.b = b; Debug.Assert((object)b != null); }
		readonly ExprRoot a, b;

		public override Value EvalForValue(Context ctx, string rddest)
		{
			var ra = a.EvalForAssign(ctx);

			if (ra.value.StartsWith("[") == false)
			{
				// ra e` un registro... 

				// se e` zero ritorna r0
				// un numero ritorno un numero (con #)
				// altrimenti rispetta la variabile.
				var rb2 = b.EvalForValue(ctx, ra.value);
				if (IsNum(rb2))
					rb2 = EvalNumInReg(rb2, ctx, ra.value);
				if (rb2.value != ra.value)
					ctx.R(Func_R.add, ra.value, rb2.value, MipsAssembly.regZero);

				return ra;
			}

			// ra e` un [... ossia un accesso in memoria
			// rb deve diventare un registro.

			// qualunque cosa sia, ottengo b nel registro rb
			// (le istruzione di ld st sono sempre da e per un registro).

			string t = ctx.NewTemp();
			Value rb = b.EvalForValue(ctx, t);
			if (IsNum(rb))
				rb = EvalNumInReg(rb, ctx, t);

			if (ra.value.StartsWith("[sw,"))
			{
				var ss = ra.value.Substring(1, ra.value.Length - 2).Split(',');
				string rt = ss[1];
				short C = Util.ParseShort(ss[2]);
				ctx.I(OpCode.I_sw, rb.value, rt, C);
				return rb;
			}
			else if (ra.value.StartsWith("[sti,"))
			{
				var ss = ra.value.Substring(1, ra.value.Length - 2).Split(',');
				var rd = rb;
				string rt = ss[1];
				string rs = ss[2];
				int kk = Util.ParseShort(ss[3]);
				int imm = Util.ParseShort(ss[4]);
				ctx.X(OpCode.X_sti, rd.value, rt, rs, kk, (uint)imm);
				return rb;
			}
			else
				Debug.Assert(false);
			return new Value("", TypeRoot.Void);
		}
		public override Value EvalForAssign(Context ctx)
		{
			return a.EvalForAssign(ctx);
		}
	}
	public class ExprCall : ExprRoot
	{
		public ExprCall(ExprRoot fun, TokenAST par, ExprList el)
			: base(par)
		{
			// per ora ...
			this.fun = fun;
			this.args = el.ToArray();
			this.ret = true;
		}

		//public ExprCall(string fun, bool ret, params ExprRoot[] args)
		//{
		//	this.fun = fun;
		//	this.args = args;
		//	this.ret = ret;
		//}

		public readonly ExprRoot[] args;
		public readonly ExprRoot fun;
		public readonly bool ret;

		public string Fun
		{
			get
			{
				if (fun is ExprVar)
				{
					ExprVar vv = (ExprVar)fun;
					return vv._var.v;
				}
				return "TODO";
			}
		}

		public override Value EvalForValue(Context ctx, string rdest)
		{
			/*
			 * Arg n         <-- parametri da leggere con rl e scrivere con rs
			 * Arg n-1
			 * Arg n-2
			 * Arg ...
			 * (valore zero) r0
			 * (window)      r1
			 * (ret PC)      r2
			 * (reseved)     r3
			 * Arg regMax    r4 <-- numero massimo parametri passato per registro
			 * Arg ...       r(5...
			 * Arg 2         r(5+regMax-1)
			 * Arg 1         r(5+regMax-0)
			 * r(5+regMax+3)... r31 (local + parametri)
			 * r32           valori letti scritti con rl/rs (quando non ci sono piu` registri).
			 * 
			 * Se il numero di parametri e` maggiore di regMax
			 * allora la sezione Arg n - Arg ... esiste.
			 * 
			 * Se il numero di parametri e` minore di regMax
			 * 
			 * I parametri sono in ordine inverso.
			 */

			if (true)
			{
				ctx.PushCall();
				Util.Set<string> rUsed = new Util.Set<string>();

				int regMax = 4;

				string c0 = null;
				string cn;
				string cret = null;

				// devo mettere la sezione Arg n -- Arg ...
				int i = args.Length - 1;

				for (; i >= regMax; --i)
				{
					cn = ctx.NewParam();
					rUsed.Add(cn);
					var ee = args[i].EvalForValue(ctx, null);
					if (IsNum(ee))
						ee = EvalNumInReg(ee, ctx);
					ctx.AddSw(ctx.I(OpCode.I_sr, ee.value, MipsAssembly.regZero, cn));
				}
				c0 = ctx.NewParam();  // r0
				rUsed.Add(c0);
				ctx.AddSw(ctx.I(OpCode.I_sr, MipsAssembly.regZero, MipsAssembly.regZero, c0));

				cn = ctx.NewParam();       // r1->window
				rUsed.Add(cn);
				ctx.AddSw(ctx.I(OpCode.I_sr, MipsAssembly.regZero, MipsAssembly.regZero, cn));

				cn = ctx.NewParam();       // r2->ret
				rUsed.Add(cn);
				ctx.AddSw(ctx.I(OpCode.I_sr, MipsAssembly.regZero, MipsAssembly.regZero, cn));

				cn = ctx.NewParam();       // r3->reserved
				rUsed.Add(cn);
				ctx.AddSw(ctx.I(OpCode.I_sr, MipsAssembly.regZero, MipsAssembly.regZero, cn));

				for (; i >= 0; --i)
				{
					// r4-r5-r6-r7 (se regMax=4)
					cn = ctx.NewParam();
					rUsed.Add(cn);
					if (ret && cret == null) cret = cn; // r4 --> valore di ritorno della funzione

					var ee = args[i].EvalForValue(ctx, null);
					if (IsNum(ee))
						ee = EvalNumInReg(ee, ctx, cn);
					ctx.AddSw(ctx.I(OpCode.I_sr, ee.value, MipsAssembly.regZero, cn));
				}

				if (ret && cret == null)
				{
					Debug.Assert(args.Length == 0 || regMax == 0);
					cret = ctx.NewParam();
					// r4 --> valore di ritorno della funzione (quando non ha parametri o senza passaggio con i registri)
				}

				Util.Set<string> rOut = new Util.Set<string>();
				if (ret) rOut.Add(cret);
				ctx.AddSw(ctx.I(OpCode.I_mvw, null, null, c0));

				Value ff = this.fun.EvalForValue(ctx, null);
				if (IsNum(ff))
					ctx.J(OpCode.J_js, Fun, rUsed, rOut);
				else
					ctx.R(Func_R.js, ff.value, ff.value, ff.value);
				if (ret)
				{
					string ra = ctx.NewTemp();
					ctx.AddSw(ctx.I(OpCode.I_lr, ra, MipsAssembly.regZero, cret));
					return new Value(ra, TypeRoot.Int);
				}

				return new Value(MipsAssembly.regZero, TypeRoot.Void); // void
			}
			else
			{
				ctx.PushCall();
				Util.Set<string> rUsed = new Util.Set<string>();

				// questo e` lo spazio per r0
				string c0 = ctx.NewParam();
				ctx.R(Func_R.add, c0, MipsAssembly.regZero, MipsAssembly.regZero);
				rUsed.Add(c0);

				// seguono gli argomenti o il ritorno.
				int na = Math.Max(args.Length, ret ? 1 : 0);
				string cret = null;

				if (args.Length > 0)
				{
					foreach (var e in args.Reverse())
					{
						string cn = ctx.NewParam();
						if (cret == null) cret = cn;
						rUsed.Add(cn);
						Value ee = e.EvalForValue(ctx, cn); // non e` tenuto a rispettare <rc> --> deve solo tornare un registro
						if (IsNum(ee))
							ee = EvalNumInReg(ee.value, ctx, cn);
						if (ee.value != cn)
							ctx.R(Func_R.add, cn, MipsAssembly.regZero, ee.value);
					}
				}
				else
				{
					cret = ctx.NewParam();
				}

				ctx.PopCall();
				Util.Set<string> rOut = new Util.Set<string>();
				if (ret) rOut.Add(cret);
				ctx.AddSw(ctx.I(OpCode.I_mvw, null, null, c0));


				ctx.J(OpCode.J_js, this.Fun, rUsed, rOut);
				if (ret) return new Value(cret, TypeRoot.Int);

				return new Value(MipsAssembly.regZero, TypeRoot.Void) ; // void
			}
		}
		public override Value EvalForAssign(Context ctx)
		{
			throw new ApplicationException("functions cannot be on the left side of an expression");
		}
	}
	public class ExprNew : ExprRoot
	{
		public readonly TypeRoot ty;
		public readonly ExprList el;
		public ExprNew(TokenAST tk, TypeRoot ty, ExprList el)
			: base(tk)
		{
			this.ty = ty;
			this.el = el;
		}
		public override Value EvalForValue(Context ctx, string rdest)
		{
			Debug.Assert(false, "TODO");
			return null;
		}
		public override Value EvalForAssign(Context ctx)
		{
			throw new ApplicationException("functions cannot be on the left side of an expression");
		}
	}
	public class ExprPreDec : ExprRoot
	{
		public readonly ExprRoot e;
		public ExprPreDec(TokenAST tk, ExprRoot e) : base(tk) { this.e = e; }

		public override Value EvalForValue(Context ctx, string rdest)
		{
			var r = new ExprAss(this.e, new TokenAST(this, '='), new ExprSub(this.e, this, new ExprNum(1, this)));
			return r.EvalForValue(ctx, rdest);
		}
		public override Value EvalForAssign(Context ctx)
		{
			throw new ApplicationException("functions cannot be on the left side of an expression");
		}
	}
	public class ExprPreInc : ExprRoot
	{
		public readonly ExprRoot e;
		public ExprPreInc(TokenAST tk, ExprRoot e) : base(tk) { this.e = e; }

		public override Value EvalForValue(Context ctx, string rdest)
		{
			var r = new ExprAss(this.e, new TokenAST(this, '='), new ExprAdd(this.e, new TokenAST(this, '+'), new ExprNum(1, this)));
			return r.EvalForValue(ctx, rdest);
		}
		public override Value EvalForAssign(Context ctx)
		{
			throw new ApplicationException("functions cannot be on the left side of an expression");
		}
	}
	public class ExprPostInc : ExprRoot
	{
		public readonly ExprRoot e;
		public ExprPostInc(ExprRoot e, TokenAST tk) : base(tk) { this.e = e; }

		public override Value EvalForValue(Context ctx, string rdest)
		{
			Context c = ctx.Clone();
			TypeRoot et = this.e.EvalForValue(c, rdest).type;

			ctx.PushBlock();
			string tmp = ctx.AddLocal(null, et);
			new ExprAss(new ExprVar(tmp), new TokenAST(this, '='), this.e).EvalForValue(ctx, null);
			new ExprAss(this.e, new TokenAST(this, '='), new ExprAdd(new ExprVar(tmp), this, new ExprNum(1, this))).EvalForValue(ctx, null);
			var ret = new ExprVar(tmp).EvalForValue(ctx, rdest);
			ctx.PopBlock();
			return ret;
		}
		public override Value EvalForAssign(Context ctx)
		{
			throw new ApplicationException("poist inc cannot be on the left side of an expression");
		}
	}
	public class ExprPostDec : ExprRoot
	{
		public readonly ExprRoot e;
		public ExprPostDec(ExprRoot e, TokenAST tk) : base(tk) { this.e = e; }

		public override Value EvalForValue(Context ctx, string rdest)
		{
			Context c = ctx.Clone();
			TypeRoot et = this.e.EvalForValue(c, rdest).type;

			ctx.PushBlock();
			string tmp = ctx.AddLocal(null, et);
			new ExprAss(new ExprVar(tmp), new TokenAST(this, '='), this.e).EvalForValue(ctx, null);
			new ExprAss(this.e, new TokenAST(this, '='), new ExprSub(new ExprVar(tmp), this, new ExprNum(1, this))).EvalForValue(ctx, null);
			var ret = new ExprVar(tmp).EvalForValue(ctx, rdest);
			ctx.PopBlock();
			return ret;
		}
		public override Value EvalForAssign(Context ctx)
		{
			throw new ApplicationException("poist inc cannot be on the left side of an expression");
		}
	}
	public class ExprCast : ExprRoot
	{
		public readonly TypeRoot type;
		public readonly ExprRoot e;
		public ExprCast(TokenAST tk, TypeRoot ty, ExprRoot e) : base(tk) { this.type = ty; this.e = e; }

		public override Value EvalForValue(Context ctx, string rdest)
		{
			// per ora
			var v = e.EvalForValue(ctx, rdest);
			if (this.type == TypeRoot.Int)
			{
				if (v.type == TypeRoot.Bool)
					throw new SyntaxError(this, "cannot convert bool to int");
			}

			return new Value(v.value, this.type);
		}

		public override Value EvalForAssign(Context ctx)
		{
			throw new ApplicationException("Cannot obtain l-value from cast expressions");
		}
	}
	public class ExprList : ListAST<ExprRoot>, IAST
	{
		public ExprList() { }
		public ExprList(ExprRoot a) : base(a) {}
	}
	//////////////////////////////////////////////


	public abstract class StmtRoot : ParserRoot
	{
		public StmtRoot(ISourceTrackable sc) : base(sc) { }
		public abstract void GenerateCode(Context ctx);
	}
	public class StmtNull : StmtRoot
	{
		public StmtNull(ISourceTrackable sc) : base(sc) { }
		public override void GenerateCode(Context ctx)
		{
		}
	}
	public class StmtIf : StmtRoot
	{
		ExprRoot e;
		StmtRoot a;
		StmtRoot b;

		public StmtIf(ISourceTrackable sc, ExprRoot e, StmtRoot a, StmtRoot b) : base(sc)
		{
			this.e = e;
			this.a = a;
			this.b = b;
		}
		public StmtIf(ISourceTrackable sc, ExprRoot e, StmtRoot a) : base(sc)
		{
			this.e = e;
			this.a = a;
			this.b = null;
		}

		public override void GenerateCode(Context ctx)
		{
			if ((object)this.b == null)
			{
				// non c'e` else

				string lbl_false = ctx.NewLbl();
				e.EvalForBool(ctx, null, lbl_false);
				a.GenerateCode(ctx);
				ctx.L(lbl_false);
			}
			else
			{
				string lbl_out = ctx.NewLbl();
				string lbl_false = ctx.NewLbl();
				e.EvalForBool(ctx, null, lbl_false);

				a.GenerateCode(ctx);
				ctx.J(OpCode.J_j, lbl_out);
				ctx.L(lbl_false);
				b.GenerateCode(ctx);
				ctx.L(lbl_out);
			}
		}
	}
	public class StmtWhile : StmtRoot
	{
		ExprRoot e;
		StmtRoot a;

		public StmtWhile(ISourceTrackable sc, ExprRoot e, StmtRoot a) : base(sc)
		{
			this.e = e;
			this.a = a;
		}
		public override void GenerateCode(Context ctx)
		{
			string lbl_loop = ctx.NewLbl();
			string lbl_continue = ctx.NewLbl();
			string lbl_break = ctx.NewLbl();
			ctx.J(OpCode.J_j, lbl_continue);
			
			ctx.L(lbl_loop);
			ctx._breakLabel.Push(lbl_break);
			ctx._continueLabel.Push(lbl_continue);
			a.GenerateCode(ctx);
			ctx._continueLabel.Pop();
			ctx._breakLabel.Pop();

			ctx.L(lbl_continue);
			e.EvalForBool(ctx, lbl_loop, null);

			ctx.L(lbl_break);
		}
	}
	public class StmtFor : StmtRoot
	{
		ExprRoot e1;
		StmtDef d;

		ExprRoot e2;
		ExprRoot e3;
		StmtRoot a;

		public StmtFor(ISourceTrackable sc, StmtDef d, ExprRoot e2, ExprRoot e3, StmtRoot a) : base(sc)
		{
			this.d = d;
			this.e2 = e2;
			this.e3 = e3;
			this.a = a;
		}
		public StmtFor(ISourceTrackable sc, ExprRoot e1, ExprRoot e2, ExprRoot e3, StmtRoot a) : base(sc)
		{
			this.e1 = e1;
			this.e2 = e2;
			this.e3 = e3;
			this.a = a;
		}
		public override void GenerateCode(Context ctx)
		{
			bool old = false;

			if (old)
			{
				if (d != null)
				{
					ctx.PushBlock();
					d.GenerateCode(ctx);
				}
				else if ((object)e1 != null)
					e1.EvalForValue(ctx, null);

				string lbl_loop = ctx.NewLbl();
				string lbl_break = ctx.NewLbl();
				string lbl_continue = ctx.NewLbl();
				ctx.L(lbl_loop);
				if ((object)e2 != null) e2.EvalForBool(ctx, null, lbl_break);
				ctx._breakLabel.Push(lbl_break);
				ctx._continueLabel.Push(lbl_continue);
				a.GenerateCode(ctx);
				ctx._continueLabel.Pop();
				ctx._breakLabel.Pop();
				ctx.L(lbl_continue);
				if ((object)e3 != null) e3.EvalForValue(ctx, null);
				ctx.J(OpCode.J_j, lbl_loop);
				ctx.L(lbl_break);

				if (d != null)
					ctx.PopBlock();
			}
			else
			{
				if (d != null)
				{
					ctx.PushBlock();
					d.GenerateCode(ctx);
				}
				else if ((object)e1 != null)
					e1.EvalForValue(ctx, null);

				string lbl_check = ctx.NewLbl();
				string lbl_loop = ctx.NewLbl();
				string lbl_break = ctx.NewLbl();
				string lbl_continue = ctx.NewLbl();

				if ((object)e2 != null) ctx.J(OpCode.J_j, lbl_check);

				ctx.L(lbl_loop);
				ctx._breakLabel.Push(lbl_break);
				ctx._continueLabel.Push(lbl_continue);
				a.GenerateCode(ctx);
				ctx._continueLabel.Pop();
				ctx._breakLabel.Pop();

				ctx.L(lbl_continue);
				if ((object)e3 != null) e3.EvalForValue(ctx, null);

				ctx.L(lbl_check);
				if ((object)e2 != null) e2.EvalForBool(ctx, lbl_loop, null); else ctx.J(OpCode.J_j, lbl_loop);

				ctx.L(lbl_break);
				if (d != null)
					ctx.PopBlock();
			}
		}
	}
	public class StmtExpr : StmtRoot
	{
		ExprRoot e;
		public StmtExpr(ExprRoot e) : base(e) { this.e = e; }

		public override void GenerateCode(Context ctx)
		{
			e.EvalForValue(ctx, null);
		}
	}
	public class StmtBlock : StmtRoot
	{
		List<StmtRoot> s;
		public StmtBlock(ISourceTrackable sc) : base(sc) { this.s = new List<StmtRoot>(); }
		public StmtBlock Add(StmtRoot s) { this.s.Add(s); return this; }

		public override void GenerateCode(Context ctx)
		{
			ctx.PushBlock();
			foreach (var s in this.s)
				s.GenerateCode(ctx);
			ctx.PopBlock();
		}
	}
	public class StmtContinue : StmtRoot
	{
		public StmtContinue(ISourceTrackable sc) : base(sc) { }

		public override void GenerateCode(Context ctx)
		{
			if (ctx._continueLabel.Count == 0)
				throw new ApplicationException("continue be executed");
			ctx.J(OpCode.J_j, ctx._continueLabel.Peek());
		}
	}
	public class StmtBreak : StmtRoot
	{
		public StmtBreak(ISourceTrackable sc) : base(sc) { }

		public override void GenerateCode(Context ctx)
		{
			if (ctx._continueLabel.Count == 0)
				throw new ApplicationException("break be executed");
			ctx.J(OpCode.J_j, ctx._breakLabel.Peek());
		}
	}
	public class StmtSwitch : StmtRoot
	{
		private Dictionary<int, StmtRoot> Case = new Dictionary<int, StmtRoot>();
		private StmtRoot Default;
		private ExprRoot e;

		public StmtSwitch(ISourceTrackable sc, ExprRoot e) : base(sc)
		{
			this.e = e;
		}
		public void AddCase(int v, StmtRoot s)
		{
			if (Case.ContainsKey(v) == true)
				throw new ApplicationException("Duplicated case");
			Case[v] = s;
		}
		public void AddDefault(StmtRoot s)
		{
			if ((object)Default != null)
				throw new ApplicationException("Duplicated case");
			Default = s;
		}

		public override void GenerateCode(Context ctx)
		{
			var ra = e.EvalForValue(ctx, null);
			if (IsNum(ra))
				ra = EvalNumInReg(ra, ctx);

			string lbl_out = ctx.NewLbl();
			Dictionary<int, string> ss = new Dictionary<int, string>();
			foreach (var k in Case)
			{
				string lbl = ctx.NewLbl();
				var rb = EvalNumInReg(k.Key, ctx);
				ctx.I(OpCode.I_beq, ra.value, rb.value, lbl);
				ss[k.Key] = lbl;
			}
			ctx._breakLabel.Push(lbl_out);
			if ((object)Default != null)
				Default.GenerateCode(ctx);

			foreach (var k in Case)
			{
				ctx.L(ss[k.Key]);
				k.Value.GenerateCode(ctx);
			}
			ctx._breakLabel.Pop();
			ctx.L(lbl_out);
		}
	}
	public class StmtReturn : StmtRoot
	{
		readonly ExprRoot e;

		public StmtReturn(ISourceTrackable sc) : base(sc)
		{
		}
		public StmtReturn(ISourceTrackable sc, ExprRoot e) : base(sc)
		{
			this.e = e;
		}

		public override void GenerateCode(Context ctx)
		{
			if ((object)e != null)
			{
				var r = e.EvalForValue(ctx, MipsAssembly.regReturn);
				if (IsNum(r))
					r = EvalNumInReg(r, ctx, MipsAssembly.regReturn);
				if (r.value != MipsAssembly.regReturn)
					ctx.R(Func_R.add, MipsAssembly.regReturn, MipsAssembly.regZero, r.value);
			}
			else
				ctx.R(Func_R.add, MipsAssembly.regReturn, MipsAssembly.regZero, MipsAssembly.regZero);

			Util.Set<string> rUsed = new Util.Set<string>();
			Util.Set<string> rOut = new Util.Set<string>();
			rUsed.Add(MipsAssembly.regReturn);
			//rOut.Add(regRet);
			ctx.J(OpCode.J_ret, null, rUsed, null);
		}
	}
	public class StmtDef : StmtRoot
	{
		public StmtDef(ISourceTrackable sc, TokenAST id, TypeRoot type, ExprRoot e) : base(sc)
		{
			this.id = id;
			this.type = type;
			this.e = e;
		}
		public StmtDef(ISourceTrackable sc, TokenAST id, TypeRoot type) : base(sc)
		{
			this.id = id;
			this.type = type;
			this.e = new ExprNum(0, this.id);
		}
		public StmtDef(ISourceTrackable sc, TokenAST id, ExprRoot e)
			: base(sc)
		{
			this.id = id;
			this.e = e;
		}

		public override void GenerateCode(Context ctx)
		{
			if (this.type != null)
			{
				ctx.AddLocal(this.id, this.type);
				var a = new ExprAss(new ExprVar(this.id), this.id, this.e);
				a.EvalForValue(ctx, null);
			}
			else
			{
				Context c = ctx.Clone();
				TypeRoot et = this.e.EvalForValue(c, null).type;

				ctx.AddLocal(this.id, et);
				var a = new ExprAss(new ExprVar(this.id), id, this.e);
				a.EvalForValue(ctx, null);
			}
		}

		public readonly TokenAST id;
		public readonly TypeRoot type;
		public readonly ExprRoot e;
	}



	public class Args : ListAST<Tuple<TokenAST,TypeRoot>>, IAST
	{
		public Args() { }
		public Args(TokenAST a, TypeRoot ty) : base(Tuple.Create(a, ty)) { }
		public Args Add(TokenAST a, TypeRoot ty) { base.Add(Tuple.Create(a, ty)); return this; }
	}

	[Serializable]
	public abstract class Decl : IAST
	{
	}

	[Serializable]
	public class Function : Decl
	{
		public Function(TokenAST name, Args args, TypeRoot ret, StmtRoot s)
		{
			this.name = name;
			this.args = new List<Tuple<TokenAST,TypeRoot>>();
			foreach (var aa in args)
				this.args.Add(aa);

			this.ret = ret;
			this.s = s;
		}

		public bool IsVoid
		{
			get
			{
				if (this.ret is TypeNat && ((TypeNat)this.ret).tk == MParser.VOID) return true;
				return false;
			}
		}

		public readonly TokenAST name;
		public List<Tuple<TokenAST, TypeRoot>> args;
		public TypeRoot ret;

		[NonSerialized]
		public StmtRoot s;
	}

	[Serializable]
	public class Delegate : Decl
	{
		public Delegate(TokenAST name, Args args, TypeRoot ret)
		{
			this.name = name;
			this.args = new List<Tuple<TokenAST, TypeRoot>>();
			foreach (var aa in args)
				this.args.Add(aa);

			this.ret = ret;
		}

		public readonly TokenAST name;
		public List<Tuple<TokenAST, TypeRoot>> args;
		public TypeRoot ret;
	}


	public class DeclList : ListAST<Decl>, IAST
	{
		public DeclList() { }
		public DeclList(Decl f) : base(f) { }
	}
	///////////////////////////////////////////

	[Serializable]
	public abstract class TypeRoot : IAST, IEquatable<TypeRoot>
	{
		private static TypeNat _int;
		private static TypeNat _void;
		private static TypeNat _bool;

		public static TypeNat Int { get { return _int ?? (_int = new TypeNat(MParser.INT)); } }
		public static TypeNat Void { get { return _void ?? (_void = new TypeNat(MParser.VOID)); } }
		public static TypeNat Bool { get { return _bool ?? (_bool = new TypeNat(MParser.BOOL)); } }

		public abstract bool Equals(TypeRoot n);
		public override bool Equals(object obj)
		{
			if (obj is TypeRoot == false) return false;
			return Equals((TypeRoot)obj);
		}
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public static bool operator ==(TypeRoot a, TypeRoot b) {
			if ((object)a == null && (object)b == null) return true;
			if ((object)a == null || (object)b == null) return false;
			return a.Equals(b);
		}
		public static bool operator !=(TypeRoot a, TypeRoot b)
		{
			return !(a == b);
		}


	}
	[Serializable]
	public class TypeNat : TypeRoot
	{
		public TypeNat(TokenAST v) { tk = v.ch; }
		public TypeNat(int t) { tk = t; }
		public readonly int tk;

		public override bool Equals(TypeRoot n)
		{
			TypeNat a = n as TypeNat;
			return a != null && a.tk == this.tk;
		}
	}
	[Serializable]
	class TypeDef : TypeRoot
	{
		public TypeDef(TypeRoot ty, TokenAST v) { this.ty = ty; this.id = v.v; }
		public TypeDef(string id) { this.id = id; }
		public TypeDef(TokenAST v) { id = v.v; }
		public readonly TypeRoot ty;
		public readonly string id;

		public override bool Equals(TypeRoot n)
		{
			TypeDef a = n as TypeDef;
			return a != null && a.id == this.id;
		}
	}
	[Serializable]
	class TypeArray : TypeRoot
	{
		public TypeArray(TypeRoot t) { this.ty = t; }
		public readonly TypeRoot ty;

		public override bool Equals(TypeRoot n)
		{
			TypeArray a = n as TypeArray;
			return a != null && this.ty.Equals(a.ty);
		}
	}

	///////////////////////////////////////////
	[Serializable]
	public class Context
	{
		public Context()
		{
			_st = new Dictionary<string, Tuple<string, int, TypeRoot>>();
			_ty = new Dictionary<string, Tuple<int, Delegate>>();
			_sw = new Util.Set<Mips24k.MipsI>();
			_f = new Dictionary<string, Function>();
			_blockLevel = 0;
		}

		public Context Clone() { return this.DeepCopy<Context>(); }

		int _r = 0; // r0 e` sempre 0
		int _t = 0;
		int _lbl = 0;
		int _paramCounter = 0;
		int _tmpVar = 0;

		[NonSerialized]
		Util.Set<Mips24k.MipsI> _sw = new Util.Set<Mips24k.MipsI>();

		[NonSerialized]
		List<MipsAssembly> _ass = new List<MipsAssembly>();

		public Stack<string> _breakLabel;
		public Stack<string> _continueLabel;

		private Dictionary<string, Tuple<string, int, TypeRoot>> _st;
		private Dictionary<string, Tuple<int, Delegate>> _ty;
		private int _blockLevel = 0;
		private Dictionary<string, Function> _f;

		private void StartFunction(Function f)
		{
			_breakLabel = new Stack<string>();
			_continueLabel = new Stack<string>();
			_r = 3;
			_t = 0;
			_paramCounter = 0;
		}
		public void AddFunction(Function f)
		{
			_f.Add(f.name.v, f);
		}
		public void AddParameter(TokenAST varName, TypeRoot ty)
		{
			Debug.Assert(_blockLevel == 0);

			if (_st.ContainsKey(varName.v) == true)
				throw new SyntaxError(varName, "duplicated variable '{0}'", varName.v);

			string r = Util.F("r{0}", _r++);
			_st.Add(varName.v, Tuple.Create(r, _blockLevel, ty));
		}
		public string AddLocal(TokenAST astName, TypeRoot ty)
		{
			Debug.Assert(_blockLevel > 0);

			string varName;
			if (astName == null)
				varName = U.F("#{0}", _tmpVar++);
			else
				varName = astName.v;

			if (_st.ContainsKey(varName) == true)
				throw new SyntaxError(astName, "duplicated variable '{0}'", varName);

			string r = NewTemp();
			_st.Add(varName, Tuple.Create(r, _blockLevel, ty));

			return varName;
		}

		public void PushBlock()
		{
			_blockLevel += 1;
		}
		public void PopBlock()
		{
			var r1 = _st.Where(p => p.Value.Item2 == _blockLevel).ToArray();
			foreach (var v in r1)
				_st.Remove(v.Key);

			var r2 = _ty.Where(p => p.Value.Item1 == _blockLevel).ToArray();
			foreach (var v in r2)
				_st.Remove(v.Key);

			_blockLevel -= 1;
		}

		internal Value GetRegisterForLocalVariable(TokenAST varName)
		{
			if (_st.ContainsKey(varName.v) == false)
				throw new SyntaxError(varName, "variable '{0}' not found", varName.v);
			return new Value(_st[varName.v].Item1, _st[varName.v].Item3);
		}

		internal enum SymbolType { Variable, Function, None }
		internal SymbolType ResolveSymbol(string n)
		{
			if (_f.ContainsKey(n)) return SymbolType.Function;
			if (_st.ContainsKey(n)) return SymbolType.Variable;
			return SymbolType.None;
		}

		internal MipsI I(Mips24k.OpCode op, string rt, string rs, int C) { var r = new MipsI(op, rs, rt, C); _ass.Add(r); return r; }
		internal MipsI I(Mips24k.OpCode op, string rt, string rs, string lbl) { var r = new MipsI(op, rs, rt, lbl); _ass.Add(r); return r; }

		internal void R(Func_R func, string rd, string rt, string rs)
		{
			_ass.Add(new MipsR(OpCode.R, rd, rs, rt, func));
		}
		internal void R(Func_R func, string rd, string rt, int shamat)
		{
			switch (func)
			{
			case Func_R.sll:
			case Func_R.srl:
			case Func_R.sra:
				_ass.Add(new MipsR(OpCode.R, rd, null, rt, shamat, func));
				break;

			default:
				Debug.Assert(false);
				break;
			}
		}
		internal void X(OpCode op, string rd, string rt, string rs, int kk, uint imm)
		{
			switch (op)
			{
			case OpCode.X_ldi:
			case OpCode.X_sti:
				_ass.Add(new MipsX(op, rd, rt, rs, kk, imm));
				break;

			default:
				Debug.Assert(false);
				break;
			}
		}
		internal void J(OpCode op, string lbl, Util.Set<string> rUsed, Util.Set<string> rOut) { _ass.Add(new MipsJ(op, lbl, rUsed, rOut)); }
		internal void J(OpCode op, string lbl) { _ass.Add(new MipsJ(op, lbl, null, null)); }
		internal void J(OpCode op) { _ass.Add(new MipsJ(op, "", null, null)); }

		internal void L(string lbl)
		{
			MipsAssembly.SetLabel(lbl);
		}

		internal string NewTemp()
		{
			return U.F("${0}", _t++);
		}

		Stack<int> c = new Stack<int>();
		internal string NewParam()
		{
			return U.F("c{0}", _paramCounter++);
		}
		internal void PushCall()
		{
			c.Push(_paramCounter);
		}
		internal string PopCall()
		{
			var r = c.Peek();
			c.Pop();
			_paramCounter = r;
			return Util.F("c{0}", r);
		}
		internal string NewLbl()
		{
			return Util.F("@{0}", _lbl++);
		}

		private void ComputeLive(int istart, int iend)
		{
			// capisco per ogni istruzione quale sono
			// le istruzioni che la seguono.
			// puo` essere quella immediatamente successiva 
			// o un jmp da qualche parte nel codice.
			this.ComputeSucc(istart, iend);
			if (false)
			{
				for (var i = istart; i < iend; ++i)
				{
					var c = _ass[i];
					Console.WriteLine("{0,-3} {1, -10} {2}", i, c.Succ, c.ToString());
				}
			}

			bool changed;
			do
			{
				changed = false;
				for (int i = iend - 1; i >= istart; --i)
				{
					var c = _ass[i];

					// cerco gli stmt 
					Util.Set<string> rout = new Util.Set<string>();
					foreach (var t in c.Succ)
						rout = rout + _ass[t].In;

					bool b = c.ComputeLive(rout);
					if (b) changed = true;

					//Console.WriteLine(this);

				}
				//Console.WriteLine(this);
			}
			while (changed);
			//Console.WriteLine(this.ToString(istart, iend));
		}

		public string ToString(int istart, int iend)
		{
			string r = "";
			for (int i = istart; i < iend; ++i)
			{
				var v = _ass[i];
				r += v.ToString() + "\n";
			}

			if (true)
			{
				r += "[";
				bool first = true;
				foreach (var v in _ass[_ass.Count - 1].Out)
				{
					if (first == false) r += ", ";
					first = false;
					r += v;
				}
				r += "]\n";
			}

			return r;
		}
		public override string ToString()
		{
			return ToString(0, _ass.Count);
		}

		private Graph ComputeGraph(int istart, int iend, string prefix)
		{
			Graph gr = new Graph();

			for (int i = istart; i < iend; ++i)
			{
				var c = _ass[i];

				for (int j = 0; j < c.In.Count; ++j)
				{
					if (c.In[j].StartsWith(prefix) || c.In[j].StartsWith("r"))
					{
						bool giaFissato = c.In[j].StartsWith("r");

						if (gr.ExistsNode(c.In[j]) == false)
							gr.CreateNode(c.In[j], giaFissato);
					}
				}
			}


			for (int i = istart; i < iend; ++i)
			{
				var c = _ass[i];

				for (int j = 0; j < c.In.Count; ++j)
				{
					if (gr.ExistsNode(c.In[j]) == true)
					{
						for (int k = j + 1; k < c.In.Count; ++k)
							if (gr.ExistsNode(c.In[k]) == true)
								gr.AddEdge(c.In[j], c.In[k]);
					}
				}
			}

			var rr = _ass[iend - 1].Out;
			for (int j = 0; j < rr.Count; ++j)
			{
				if (gr.ExistsNode(rr[j]) == true)
				{
					for (int k = j + 1; k < rr.Count; ++k)
					{
						if (gr.ExistsNode(rr[k]))
							gr.AddEdge(rr[j], rr[k]);
					}
				}
			}

			return gr;
		}

		private void SetTemps(int istart, int iend, Dictionary<string, string> regs)
		{
			for (int i = istart; i < iend; ++i)
			{
				var stmt = _ass[i];
				foreach (var t in regs)
					stmt.Substitute(t.Key, t.Value);
			}
		}

		private void ComputeSucc(int istart, int iend)
		{
			for (int i = istart; i < iend; ++i)
				_ass[i].Succ = _ass[i].GetSucc(i, this);
		}

		public int GetAddrFromLabel(string lbl)
		{
			for (int i = 0; i < _ass.Count; ++i)
				if (_ass[i].Lbl.Contains(lbl))
					return i;
			return _ass.Count;
		}

		public bool GenerateCode(Function f)
		{
			StartFunction(f);

			foreach (var v in f.args)
				AddParameter(v.Item1, v.Item2);

			int istart = _ass.Count;
			L(f.name.v);
			f.s.GenerateCode(this);
			if (f.IsVoid)
				J(OpCode.J_ret);
			int iend = _ass.Count;

			this.ComputeLive(istart, iend);

			Console.WriteLine("Codice generato per la funzione {0} {1}/{2}", f.name, istart, iend);
			Console.WriteLine("Live variables");
			Console.WriteLine(this.ToString(istart, iend));

			var gr = this.ComputeGraph(istart, iend, "$");

			Console.WriteLine("Grafo");
			Console.WriteLine(gr);

			bool ok = false;
			int k;
			for (k = 0; k < 32; ++k)
				if (gr.Color(k))
				{
					Console.WriteLine("Ci vogliono k={0} da r0 a r{1}, prossimo per var locali/parametri r{0}", k, k - 1);
					var regs = gr.GetRegs();
					this.SetTemps(istart, iend, regs);
					ok = true;
					break;
				}

			if (ok == false)
			{
				Console.WriteLine("Cannot allocate registers");
				return false;
			}

			if (true)
			{
				// calcolo il maggiore dei registri r utilizzati (es r4)
				// c0 iniziera` con r5

				var rrr = new Dictionary<string, string>();
				for (int ci = 0; ci < 1024; ci++)
				{
					string cc = Util.F("c{0}", ci);
					string ff = Util.F("r{0}", ci + k);
					rrr[cc] = ff;
				}
				this.SetTemps(istart, iend, rrr);

				// ora assegno i valori del wnd
				foreach (MipsI t in _sw)
				{
					Debug.Assert(t.Window.StartsWith("c"));
					string w = rrr[t.Window];
					Debug.Assert(w.StartsWith("r"));
					int b = Util.ParseInt(w.Substring(1));
					t.SetC(b);
				}

			}

			return ok;
		}


		private class Graph
		{
			public Graph() { }
			List<NodeReg> _nodes = new List<NodeReg>();

			public NodeReg CreateNode(string reg, bool giaAssegnato)
			{
				Debug.Assert(ExistsNode(reg) == false);

				NodeReg n = new NodeReg(this, reg, giaAssegnato);
				_nodes.Add(n);
				return n;
			}

			public NodeReg GetNode(string reg)
			{
				foreach (var r in _nodes)
					if (r.Name == reg)
						return r;
				Debug.Assert(false);
				return null;
			}


			public bool ExistsNode(string reg)
			{
				return _nodes.Find(r => r.Name == reg) != null;
			}

			public void AddEdge(string a, string b)
			{
				Debug.Assert(ExistsNode(a));
				Debug.Assert(ExistsNode(b));

				NodeReg na = GetNode(a);
				NodeReg nb = GetNode(b);

				na.AddEdge(nb);
				nb.AddEdge(na);
			}

			public override string ToString()
			{
				string r = "";
				foreach (var n in _nodes)
				{
					r += n.ToString();
					r += "\n";
				}
				return r;
			}


			public bool Color(int k)
			{
				Stack<string> st = new Stack<string>();
				return Color(k, st);
			}
			private bool Color(int k, Stack<string> st)
			{
				// rimuovo dal grafo un nodo nodo l'altro
				// il nodo da rimuovere deve avere meno di k vicini
				Graph gr = this;
				while (gr._nodes.Count > 0)
				{
					NodeReg nd = gr._nodes.Find(n => n.Neighbors.Count < k);
					if (nd == null)
					{
						// proviamo con l'optimistic coloring
						// scegliamo un nodo qualunque, sperando che i suoi vicini
						// vengano assegnati a registri in comune.

						// cerco il nodo nel grafo che ha meno vicini
						var q = from nn in gr._nodes
								orderby nn.Neighbors.Count ascending
								select nn;

						nd = q.FirstOrDefault();
						if (nd == null)
							return false;
					}

					st.Push(nd.Name);
					gr = gr.Remove(nd);
				}

				while (st.Count > 0)
				{
					var nd = st.Pop();

					// se inizia per r e` un registro gia` assegnato
					// se inizia per c e` un registro che non si assegna ora.
					if (nd.StartsWith("r") /*|| nd.StartsWith("c")*/)
						continue;
					// i registri da assegnare iniziano per $n

					// assegno un registro non assegnato gia` ai vicini
					// rg = lista dei registri assegnati ai vicini
					Util.Set<string> rg = new Util.Set<string>();
					foreach (var nv in this.GetNode(nd).Neighbors)
						if (nv.Reg != null)
							rg.Add(nv.Reg);

					if (rg.Count >= k)
					{
						// siamo nel caso dell'optimistic coloring, ma ci e` andata male
						return false;
					}

					// fra tutti i registri disponibili prendo il primo
					// non assegnato ai vicini.
					for (int i = 0; i < 32; ++i)
					{
						string r = Util.F("r{0}", i);
						if (rg.Contains(r) == false)
						{
							this.GetNode(nd).Reg = r;
							break;
						}
					}

					Debug.Assert(this.GetNode(nd).Reg != null);
				}

				return true;
			}

			Graph Remove(NodeReg src)
			{
				Graph gr = new Graph();
				foreach (var s in this._nodes)
				{
					if (s == src) continue;

					// creo il nodo nel nuovo grafo
					if (gr.ExistsNode(s.Name) == false)
						gr.CreateNode(s.Name, s.Name.StartsWith("r"));

					// e tutti gli altri nodi, purche non siano il nodo di ingresso.
					foreach (var s_neighbor in s.Neighbors)
					{
						if (s_neighbor.Name != src.Name)
						{
							if (gr.ExistsNode(s_neighbor.Name) == false)
								gr.CreateNode(s_neighbor.Name, s.Name.StartsWith("r"));
							gr.AddEdge(s.Name, s_neighbor.Name);
						}
					}
				}
				return gr;
			}

			public Dictionary<string, string> GetRegs()
			{
				Dictionary<string, string> ret = new Dictionary<string, string>();
				foreach (var n in _nodes)
					if (n.Name != n.Reg)
						ret[n.Name] = n.Reg;
				return ret;
			}
		}

		private class NodeReg
		{
			public NodeReg(Graph gr, string rg, bool giaFissato)
			{
				this._name = rg;
				this.gr = gr;
				this._neighbors = new List<NodeReg>();

				if (giaFissato)
					this._reg = rg;
			}

			public void AddEdge(NodeReg nd)
			{
				if (_neighbors.Contains(nd) == false)
					_neighbors.Add(nd);
			}

			public override string ToString()
			{
				string r = this.Name;
				if (this.Reg != null && this.Name != this.Reg) r += "/" + this.Reg;
				r += " :";
				foreach (var k in this._neighbors)
				{
					r += " " + k.Name;
					if (k.Reg != null && k.Reg != k.Name) r += "/" + k.Reg;
				}
				return r;
			}

			public List<NodeReg> Neighbors { get { return _neighbors; } }

			public string Name { get { return _name; } }
			public string Reg { get { return _reg; } set { _reg = value; } }

			readonly Graph gr;
			readonly string _name;
			string _reg;
			List<NodeReg> _neighbors;
		};

		internal void AddSw(MipsI v)
		{
			this._sw.Add(v);
		}

		internal void AddType(Delegate p)
		{
			if (_ty.ContainsKey(p.name.id))
				throw new SyntaxError(p.name, "duplicated type '{0}'", p.name.id);
			_ty.Add(p.name.id, Tuple.Create(this._blockLevel, p));
		}
	}
}
