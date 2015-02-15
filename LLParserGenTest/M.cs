#pragma warning disable 0168 // variable declared but not used.
#pragma warning disable 0219 // variable assigned but not used.

using System;
using System.Collections.Generic;
using ULib.LLParserLexerLib;

namespace LLTest
{
	public partial class MParser : ParserBase
	{
		public const int NUM = -2;
		public const int ID = -3;
		
		Dictionary<int, string> _token;
		public override Dictionary<int, string> Token
		{
			get
			{
				if (_token == null)
				{
					_token = new Dictionary<int, string>();
					_token.Add(-1, "EOF");
					_token.Add(-2, "NUM");
					_token.Add(-3, "ID");
				}
				return _token;
			}
		}
		
		ExprRoot start(IAST start_i)
		{
			int alt = 0;
			switch (Next.ch)
			{
			case '-':
			case '+':
			case NUM:
			case ID:
			case '(':
				alt = 0;
				break;
			default:
				Error();
				break;
			}
			
			ExprRoot start_s = default(ExprRoot);
			switch (alt)
			{
			case 0:
				{
					var nt1_i = default(IAST);
					
					var nt1_s = e_add(nt1_i);
					start_s = nt1_s;
				}
				break;
			}
			
			switch (Next.ch)
			{
			case -1:
				break;
			default:
				Error();
				break;
			}
			return start_s;
		}
		
		ExprRoot e_add(IAST e_add_i)
		{
			int alt = 0;
			switch (Next.ch)
			{
			case '-':
			case '+':
			case NUM:
			case ID:
			case '(':
				alt = 0;
				break;
			default:
				Error();
				break;
			}
			
			ExprRoot e_add_s = default(ExprRoot);
			switch (alt)
			{
			case 0:
				{
					var nt1_i = default(IAST);
					var nt2_i = default(IAST);
					
					var nt1_s = e_mul(nt1_i);
					nt2_i = nt1_s; 
					var nt2_s = tmp_1(nt2_i);
					e_add_s = nt2_s; 
				}
				break;
			}
			
			switch (Next.ch)
			{
			case ')':
			case -1:
				break;
			default:
				Error();
				break;
			}
			return e_add_s;
		}
		
		ExprRoot tmp_1(IAST tmp_1_i)
		{
			int alt = 0;
			switch (Next.ch)
			{
			case ')':
			case -1:
				alt = 0;
				break;
			case '+':
				alt = 1;
				break;
			case '-':
				alt = 2;
				break;
			default:
				Error();
				break;
			}
			
			ExprRoot tmp_1_s = default(ExprRoot);
			switch (alt)
			{
			case 0:
				{
					var nt1_i = default(IAST);
					
					tmp_1_s = (ExprRoot)tmp_1_i; 
				}
				break;
			case 1:
				{
					var nt1_i = default(IAST);
					var nt2_i = default(IAST);
					var nt3_i = default(IAST);
					
					TokenAST nt1_s = Match('+', nt1_i);
					var nt2_s = e_mul(nt2_i);
					nt3_i = new ExprAdd(((ExprRoot)tmp_1_i), nt2_s); 
					var nt3_s = tmp_1(nt3_i);
					tmp_1_s = nt3_s; 
				}
				break;
			case 2:
				{
					var nt1_i = default(IAST);
					var nt2_i = default(IAST);
					var nt3_i = default(IAST);
					
					TokenAST nt1_s = Match('-', nt1_i);
					var nt2_s = e_mul(nt2_i);
					nt3_i = new ExprSub(((ExprRoot)tmp_1_i), nt2_s); 
					var nt3_s = tmp_1(nt3_i);
					tmp_1_s = nt3_s; 
				}
				break;
			}
			
			switch (Next.ch)
			{
			case ')':
			case -1:
				break;
			default:
				Error();
				break;
			}
			return tmp_1_s;
		}
		
		ExprRoot e_mul(IAST e_mul_i)
		{
			int alt = 0;
			switch (Next.ch)
			{
			case '-':
			case '+':
			case NUM:
			case ID:
			case '(':
				alt = 0;
				break;
			default:
				Error();
				break;
			}
			
			ExprRoot e_mul_s = default(ExprRoot);
			switch (alt)
			{
			case 0:
				{
					var nt1_i = default(IAST);
					var nt2_i = default(IAST);
					
					var nt1_s = e_una(nt1_i);
					nt2_i = nt1_s; 
					var nt2_s = tmp_2(nt2_i);
					e_mul_s = nt2_s; 
				}
				break;
			}
			
			switch (Next.ch)
			{
			case '+':
			case '-':
			case ')':
			case -1:
				break;
			default:
				Error();
				break;
			}
			return e_mul_s;
		}
		
		ExprRoot tmp_2(IAST tmp_2_i)
		{
			int alt = 0;
			switch (Next.ch)
			{
			case '+':
			case '-':
			case ')':
			case -1:
				alt = 0;
				break;
			case '*':
				alt = 1;
				break;
			case '/':
				alt = 2;
				break;
			default:
				Error();
				break;
			}
			
			ExprRoot tmp_2_s = default(ExprRoot);
			switch (alt)
			{
			case 0:
				{
					var nt1_i = default(IAST);
					
					tmp_2_s = (ExprRoot)tmp_2_i; 
				}
				break;
			case 1:
				{
					var nt1_i = default(IAST);
					var nt2_i = default(IAST);
					var nt3_i = default(IAST);
					
					TokenAST nt1_s = Match('*', nt1_i);
					var nt2_s = e_una(nt2_i);
					nt3_i = new ExprMul(((ExprRoot)tmp_2_i), nt2_s); 
					var nt3_s = tmp_2(nt3_i);
					tmp_2_s = nt3_s; 
				}
				break;
			case 2:
				{
					var nt1_i = default(IAST);
					var nt2_i = default(IAST);
					var nt3_i = default(IAST);
					
					TokenAST nt1_s = Match('/', nt1_i);
					var nt2_s = e_una(nt2_i);
					nt3_i = new ExprDiv(((ExprRoot)tmp_2_i), nt2_s); 
					var nt3_s = tmp_2(nt3_i);
					tmp_2_s = nt3_s; 
				}
				break;
			}
			
			switch (Next.ch)
			{
			case '+':
			case '-':
			case ')':
			case -1:
				break;
			default:
				Error();
				break;
			}
			return tmp_2_s;
		}
		
		ExprRoot e_una(IAST e_una_i)
		{
			int alt = 0;
			switch (Next.ch)
			{
			case NUM:
			case ID:
			case '(':
				alt = 0;
				break;
			case '-':
				alt = 1;
				break;
			case '+':
				alt = 2;
				break;
			default:
				Error();
				break;
			}
			
			ExprRoot e_una_s = default(ExprRoot);
			switch (alt)
			{
			case 0:
				{
					var nt1_i = default(IAST);
					
					var nt1_s = e_prim(nt1_i);
					e_una_s = nt1_s;
				}
				break;
			case 1:
				{
					var nt1_i = default(IAST);
					var nt2_i = default(IAST);
					
					TokenAST nt1_s = Match('-', nt1_i);
					var nt2_s = e_una(nt2_i);
					e_una_s = new ExprNeg(nt2_s);
				}
				break;
			case 2:
				{
					var nt1_i = default(IAST);
					var nt2_i = default(IAST);
					
					TokenAST nt1_s = Match('+', nt1_i);
					var nt2_s = e_una(nt2_i);
					e_una_s = new ExprPlus(nt2_s);
				}
				break;
			}
			
			switch (Next.ch)
			{
			case '*':
			case '/':
			case '+':
			case '-':
			case ')':
			case -1:
				break;
			default:
				Error();
				break;
			}
			return e_una_s;
		}
		
		ExprRoot e_prim(IAST e_prim_i)
		{
			int alt = 0;
			switch (Next.ch)
			{
			case NUM:
				alt = 0;
				break;
			case ID:
				alt = 1;
				break;
			case '(':
				alt = 2;
				break;
			default:
				Error();
				break;
			}
			
			ExprRoot e_prim_s = default(ExprRoot);
			switch (alt)
			{
			case 0:
				{
					var nt1_i = default(IAST);
					
					TokenAST nt1_s = Match(NUM, nt1_i);
					e_prim_s = new ExprNum(nt1_s);
				}
				break;
			case 1:
				{
					var nt1_i = default(IAST);
					
					TokenAST nt1_s = Match(ID, nt1_i);
					e_prim_s = new ExprNum(nt1_s);
				}
				break;
			case 2:
				{
					var nt1_i = default(IAST);
					var nt2_i = default(IAST);
					var nt3_i = default(IAST);
					
					TokenAST nt1_s = Match('(', nt1_i);
					var nt2_s = e_add(nt2_i);
					TokenAST nt3_s = Match(')', nt3_i);
					e_prim_s = nt2_s;
				}
				break;
			}
			
			switch (Next.ch)
			{
			case '*':
			case '/':
			case '+':
			case '-':
			case ')':
			case -1:
				break;
			default:
				Error();
				break;
			}
			return e_prim_s;
		}
		
		protected override RegAcceptList CreateRegAcceptList()
		{
			var acts = new RegAcceptList();
			acts.Add(new RegToken('+'), '+');
			acts.Add(new RegToken('-'), '-');
			acts.Add(new RegToken('*'), '*');
			acts.Add(new RegToken('/'), '/');
			acts.Add(new RegToken('('), '(');
			acts.Add(new RegToken(')'), ')');
			acts.Add(new RegAnd(new RegOr(new RegOr(new RegToken('_'), new RegTokenRange(97, 122)), new RegTokenRange(65, 90)), new RegZeroOrMore(new RegOr(new RegOr(new RegOr(new RegToken('_'), new RegTokenRange(97, 122)), new RegTokenRange(65, 90)), new RegTokenRange(48, 57)))), ID);
			acts.Add(new RegOneOrMore(new RegTokenRange(48, 57)), NUM);
			acts.Add(new RegOneOrMore(new RegOr(new RegOr(new RegOr(new RegToken(' '), new RegToken(10)), new RegToken(13)), new RegToken(9))));
			return acts;
		}
	}
}
