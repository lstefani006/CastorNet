using System;
using LLParserLexerLib;

namespace LLTest
{
	class _
	{
		public static void Main(string [] args)
		{
			using (LexReader rd = new LexReader(args[0])) 
			{ 
				MParser p = new MParser(); 
				var e = p.Start(rd); 

				Console.WriteLine(e.Exec());
			}
		}
	}


	public abstract class ExprRoot : IAST
	{
		public abstract int Exec();
	}

	public class ExprAdd : ExprRoot
	{
		public ExprAdd(ExprRoot a, ExprRoot b) { this.a = a; this.b = b; }
		ExprRoot a;
		ExprRoot b;
		public override int Exec() { return a.Exec() + b.Exec(); }
	}
	public class ExprSub : ExprRoot
	{
		public ExprSub(ExprRoot a, ExprRoot b) { this.a = a; this.b = b; }
		ExprRoot a;
		ExprRoot b;
		public override int Exec() { return a.Exec() - b.Exec(); }
	}
	public class ExprMul : ExprRoot
	{
		public ExprMul(ExprRoot a, ExprRoot b) { this.a = a; this.b = b; }
		ExprRoot a;
		ExprRoot b;
		public override int Exec() { return a.Exec() * b.Exec(); }
	}
	public class ExprDiv : ExprRoot
	{
		public ExprDiv(ExprRoot a, ExprRoot b) { this.a = a; this.b = b; }
		ExprRoot a;
		ExprRoot b;
		public override int Exec() { return a.Exec() / b.Exec(); }
	}
	public class ExprPlus : ExprRoot
	{
		public ExprPlus(ExprRoot a) { this.a = a; }
		ExprRoot a;
		public override int Exec() { return a.Exec(); }
	}
	public class ExprNeg : ExprRoot
	{
		public ExprNeg(ExprRoot a) { this.a = a; }
		ExprRoot a;
		public override int Exec() { return -a.Exec(); }
	}
	public class ExprNum : ExprRoot
	{
		public ExprNum(LLParserLexerLib.TokenAST a) { this.a = a; }
		LLParserLexerLib.TokenAST a;
		public override int Exec() { return int.Parse(a.v); }
	}

	////////////////////////

	public partial class MParser
	{
		public MParser()
			: base(0)
		{
		}
		public ExprRoot Start(LexReader rd)
		{
			this.init(rd);
			return this.start(null);
		}
	}
}
