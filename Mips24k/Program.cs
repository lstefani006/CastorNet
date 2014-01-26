using System;
using System.Collections.Generic;
using ULib.LLParserLexerLib;


namespace Mips24k
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {

#if OLD
			Context ctx = new Context();
			StmtBlock s4 = new StmtBlock();
			if (true)
			{
				var s1 = new StmtExpr(new ExprAss(new ExprVar("a"), new ExprNum(10) * new ExprVar("a")));
				var s2 = new StmtIf(
					new ExprVar("a") == new ExprVar("b"),
					new StmtExpr(new ExprAss(new ExprVar("a"), new ExprNum(10))));
				var s3 = new StmtExpr(new ExprAss(new ExprVar("b"), new ExprNum(20)));
				s4.Add(s1);
				s4.Add(s2);
				s4.Add(s3);
			}


			StmtBlock ss = new StmtBlock();
			ss.Add(new StmtExpr(
					new ExprAss(
						new ExprArray(new ExprVar("b"), new ExprVar("a") + new ExprNum(12)),
						new ExprVar("a") / (new ExprVar("a") + new ExprNum(2)))));
			ss.Add(new StmtIf(
				new ExprVar("a") > new ExprNum(6),
				new StmtBreak()));
			ss.Add(new StmtIf(
				new ExprVar("a") > new ExprNum(4),
				new StmtContinue()));
			ss.Add(new StmtIf(
				new ExprVar("a") > new ExprNum(2),
				new StmtExpr(new ExprAss(
					new ExprVar("a"),
					new ExprCall("leo", true, new ExprVar("a") + new ExprVar("b"), new ExprNum(22))))));

			StmtWhile s = new StmtWhile(
				new ExprVar("a") == new ExprNum(100),
				ss);
			StmtBlock s5 = new StmtBlock();
			s5.Add(s);
			s5.Add(new StmtReturn(new ExprNum(333)));

			
			Function ciao = new Function("ciao", new List<string>{"a", "b"}, null, true, s5);
			Function leo = new Function("leo", new List<string> { "aa", "bb" }, null, true, new StmtReturn(new ExprVar("aa") * new ExprVar("bb") + new ExprNum(19)));

			
			Function fact1 = new Function("fact", new List<string> { "n" }, null, true,
				new StmtBlock(
					new StmtIf(new ExprVar("n") == new ExprNum(0), new StmtReturn(new ExprNum(1))),
					new StmtReturn(new ExprVar("n") * new ExprCall("fact", true, new ExprCall("leo", true, new ExprVar("n"), new ExprVar("n") + new ExprNum(1))))));
			
			Function fact = new Function("fact", new List<string> { "n" }, null, true,
				new StmtBlock(
					new StmtIf(new ExprVar("n") <= new ExprNum(1), new StmtReturn(new ExprNum(1))),
					new StmtReturn(new ExprVar("n") * new ExprCall("fact", true, new ExprVar("n") - new ExprNum(1)))));

			Function vmult = new Function("vmult", new List<string> { "a", "b", "n", "c", "i" }, null, true,
				new StmtBlock(
					new StmtExpr(new ExprAss(new ExprVar("c"), new ExprNum(0))),
					new StmtFor(
						new ExprAss(new ExprVar("i"), new ExprNum(0)),
						new ExprVar("i") < new ExprVar("n"),
						new ExprAss(new ExprVar("i"), new ExprVar("i") + new ExprNum(1)),
						new StmtExpr(new ExprAss(new ExprVar("c"), 
							new ExprVar("c") + new ExprArray(new ExprVar("a"), new ExprVar("i")) * new ExprArray(new ExprVar("b"), new ExprVar("i"))))),
					new StmtReturn(new ExprVar("c"))));

			bool ok = true;
		//	ok = ok && ctx.GenerateCode(ciao);
		//	ok = ok && ctx.GenerateCode(leo);
			ok = ok && ctx.GenerateCode(fact);
		//	ok = ok && ctx.GenerateCode(fact1);
			//	ok = ok && ctx.GenerateCode(vmult);
			if (ok)
			{
				string rrr = ctx.ToString();
				Console.Write(rrr);
			}
			else
			{
				Console.WriteLine("Code generation failure");
			}
#else
			DeclList fl = null;
			using (LexReader rd = new LexReader("m.txt"))
			{
				MParser p = new MParser();
				fl = (DeclList)p.Start(rd);
			}

			var ctx = new Context();

			foreach (Decl d in fl)
			{
				if (d is Delegate)
					ctx.AddType((Delegate)d);
				else if (d is Function)
					ctx.AddFunction((Function)d);
			}

			foreach (Decl f in fl)
			{
				if (f is Function)
				{
					bool ok = ctx.GenerateCode((Function)f);
					if (ok)
					{
						string rrr = ctx.ToString();
						Console.Write(rrr);
					}
					else
					{
						Console.WriteLine("Code generation failure");
						break;
					}
				}
			}
#endif
		}
    }

	public partial class MParser
	{
		public MParser()
			: base(0)
		{
		}
		public IAST Start(LexReader rd)
		{
			this.init(rd);
			return this.start(null);
		}
	}
}
