using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;

namespace Mips24k
{
	// 6 bit ==> max 64 istruzioni
	public enum OpCode : byte
	{
		// R opcode(6) rs(5) rt(5) rd(5) shamt(5) funct(6) 
		R, // rd = rs op rt

		// I opcode(6) rs(5) rt(5) immediate(16) 
		// immediate = C (senza segno) o S (con segno)
		I_addi,  // rt = rs + C/S

		I_lw,    // rt = Mem[rs + S];
		I_lh,    // rt = Mem[rs + S];
		I_lhu,   // rt = Mem[rs + S];
		I_lb,    // rt = Mem[rs + S];
		I_lbu,   // rt = Mem[rs + S];

		I_sw,    // Mem[rs + S] = rt;
		I_sh,    // Mem[rs + S] = rt;
		I_sb,    // Mem[rs + S] = rt;

		I_lr,    // rt = Reg[rs + S];
		I_sr,    // Reg[rs + S] = rt;


		I_lui,   // rt = C << 16

		I_andi,  // rt = rs & C
		I_ori,   // rt = rs | C
		I_xori,  // rt = rs ^ C

		I_slti,  // rt = rs < S

		I_beq,   // if (rs == rt) PC += 4*S
		I_bne,   // if (rs != rt) PC += 4*S
		I_bgt,   // if (rs > rt) PC += 4*S
		I_bge,   // if (rs >= rt) PC += 4*S
		I_blt,   // if (rs < rt) PC += 4*S
		I_ble,   // if (rs <= rt) PC += 4*S

		I_mvw,   // sposta di S i registri R


		// J opcode(6) address(26)
		J_j,     // PC += 4*C (PC in bytes)
		J_js,
		J_ret,

		// X opcode(6) rs(5) rt(5) rd(5) k(2) C(9) con (k=0=1/1=2/2=4/3=8)
		X_ldi,    // rd = Mem[rs + k*rt + C]
		X_ldiu,   // rd = Mem[rs + k*rt + C]
		X_sti,    // Mem[rs + k*rt + C] = rd
		X_stiu,   // Mem[rs + k*rt + C] = rd
	}

	// 5 bit ==> max 64 funzioni
	public enum Func_R : byte
	{
		// rd = rs op rt
		add,
		sub, subu,
		mult, multu,
		div, divu,
		rem, remu, // int/uint --> non esiste nel processore MIPS

		slt,        // int/uint rd = rs < rt

		and,  // uint
		or_,  // uint
		xor,  // uint
		nor,  // uint

		sll,  // uint << utilizza shamt 
		srl,  // uint >> utilizza shamt 
		sra,  // int >> (preserva il segno) utilizza shamt 

		sllv,  // rd = rs << rt
		srlv,  // rd = rs >> rt
		srav,  // rd = rs >> rt

		jr,   // PC = rs
		js,   // PC = rs ma per fare una call
	}

	public class Emulator
	{
		public int Run()
		{
			uint[] pgm = new uint[1024];
			uint[] regs = new uint[1024];
			uint[] mem = new uint[1024];

			int ret = 0;
			unsafe
			{
				fixed (uint* p = &pgm[0])
					fixed (uint* m = &mem[0])
						fixed (uint *r = &regs[0])
							ret = Run(p, m, r);
			}
			return ret;
		}

		unsafe private int Run(uint* pgm, void* mem, void *regs)
		{
			/*
			-31- format (bits)                                 -0-
			R opcode (6) $rs (5) $rt (5) $rd (5) shamt (5) funct (6) 
			I opcode (6) $rs (5) $rt (5) immediate (16) 
			J opcode (6) address (26)
			X opcode (6) $rs (5) $rt (5) $rd (5) k(2) imm(9) (k=0=1/1=2/2=4/3=8 ==> rd = Mem[rs + rt * k + imm])
			*/

			const uint OpCodeMask = ((1u << 6) - 1);
			const int OpCodePos = (0 + 5 + 5 + 5 + 5 + 6);

			const uint RegMask = ((1u << 5) - 1);

			const int RsPos = (0 + 0 + 5 + 5 + 5 + 6);
			const int RtPos = (0 + 0 + 0 + 5 + 5 + 6);
			const int RdPos = (0 + 0 + 0 + 0 + 5 + 6);
			const uint ShMatMask = ((1u << 5) - 1);
			const int ShMatPos = (0 + 0 + 0 + 0 + 0 + 6);
			const uint FuncMask = ((1u << 6) - 1);
			const int FuncPos = (0 + 0 + 0 + 0 + 0 + 0);

			const uint I_ImmMask = ((1u << 16) - 1);
			const int I_ImmPos = 0;

			const uint X_KKMask = ((1u << 2) - 1);
			const int X_KKPos = 9;
			const uint X_ImmMask = ((1u << 9) - 1);
			const int X_ImmPos = 0;

			const uint J_AddrMask = (1u << 26) - 1;

			int mvWindow = 0;
			uint PC = 0u;
			for (; ; )
			{
				uint A = pgm[PC++];

				OpCode op = (OpCode)((A & (OpCodeMask << OpCodePos)) >> OpCodePos);
				switch (op)
				{
				case OpCode.X_ldi:
				case OpCode.X_sti:
				case OpCode.X_ldiu:
				case OpCode.X_stiu:
					{
						// X opcode(6) rs(5) rt(5) rd(5) k(2) C(9) con (k=0=1/1=2/2=4/3=8)
						uint rs = (A & (RegMask << RsPos)) >> RsPos;
						uint rt = (A & (RegMask << RtPos)) >> RtPos;
						uint rd = (A & (RegMask << RdPos)) >> RdPos;
						uint kk = (A & (X_KKMask << X_KKPos)) >> X_KKPos;
						uint C = (A & (X_ImmMask << X_ImmPos)) >> X_ImmPos; // imm e` senza segno perche` e` inutile andare indietro in memoria

						uint* U = (uint*)regs;
						int* R = (int*)regs;

						void* p = ((byte*)mem + C + U[rt] + (U[rs] << (int)kk));
						switch (op)
						{
						case OpCode.X_ldi:
							switch (kk)
							{
							case 0: R[rd] = *((sbyte*)p); break;
							case 1: R[rd] = *((short*)p); break;
							case 2: R[rd] = *((int*)p); break;
							case 3: Debug.Assert(false); break;
							}
							break;

						case OpCode.X_ldiu:
							switch (kk)
							{
							case 0: U[rd] = *((byte*)p); break;
							case 1: U[rd] = *((ushort*)p); break;
							case 2: U[rd] = *((uint*)p); break;
							case 3: Debug.Assert(false); break;
							}
							break;

						case OpCode.X_sti:
							switch (kk)
							{
							case 0: *((sbyte*)p) = (sbyte)R[rd]; break;
							case 1: *((short*)p) = (short)R[rd]; break;
							case 2: *((int*)p) = (int)R[rd]; break;
							case 3: Debug.Assert(false); break;
							}
							break;
						case OpCode.X_stiu:
							switch (kk)
							{
							case 0: *((byte*)p) = (byte)U[rd]; break;
							case 1: *((ushort*)p) = (ushort)U[rd]; break;
							case 2: *((uint*)p) = (uint)U[rd]; break;
							case 3: Debug.Assert(false); break;
							}
							break;
						}
					}
					break;

				case OpCode.R:
					{
						int* R = (int*)regs;
						uint* U = (uint*)regs;

						uint rs = (A & (RegMask << RsPos)) >> RsPos;
						uint rt = (A & (RegMask << RtPos)) >> RtPos;
						uint rd = (A & (RegMask << RdPos)) >> RdPos;
						uint sh = (A & (ShMatMask << ShMatPos)) >> ShMatPos;

						Func_R funct = (Func_R)((A & (FuncMask << FuncPos)) >> FuncPos);
						switch (funct)
						{
						case Func_R.add: R[rd] = R[rs] + R[rt]; break;

						case Func_R.sub: R[rd] = R[rs] - R[rt]; break;
						case Func_R.subu: U[rd] = U[rs] - U[rt]; break;

						case Func_R.mult: R[rd] = R[rs] * R[rt]; break;
						case Func_R.multu: U[rd] = U[rs] * U[rt]; break;

						case Func_R.div: R[rd] = R[rs] / R[rt]; break;
						case Func_R.divu: U[rd] = U[rs] / U[rt]; break;

						case Func_R.rem: R[rs] = R[rs] % R[rt]; break;
						case Func_R.remu: U[rs] = U[rs] % U[rt]; break;

						case Func_R.and: U[rd] = U[rs] & U[rt]; break;
						case Func_R.or_: U[rd] = U[rs] | U[rt]; break;
						case Func_R.xor: U[rd] = U[rs] ^ U[rt]; break;
						case Func_R.nor: U[rd] = ~(U[rs] | U[rt]); break;

						case Func_R.sll: U[rd] = U[rt] << (int)sh; break;
						case Func_R.srl: U[rd] = U[rt] >> (int)sh; break;
						case Func_R.sra: R[rd] = R[rt] >> (int)sh; break;

						case Func_R.sllv: U[rd] = U[rt] << R[rs]; break;
						case Func_R.srlv: U[rd] = U[rt] >> R[rs]; break;
						case Func_R.srav: R[rd] = R[rt] >> R[rs]; break;

						case Func_R.slt: R[rd] = R[rs] < R[rt] ? 1 : 0; break;

						case Func_R.jr: PC = U[rs]; break;

						case Func_R.js:
							{
								U[31] = PC;
								R[30] = mvWindow;
								uint addr = U[rs];

								regs = ((uint*)regs) + mvWindow;
								mvWindow = 0;

								PC = addr;
							}
							break;

						default: Debug.Assert(false); break;
						}
					}
					break;

				case OpCode.J_j:
					{
						uint addr = A & J_AddrMask;  // 26 bit
						if ((((J_AddrMask + 1) >> 1) & addr) != 0) // controllo il bit del segno.
						{
							// complemento a 2 sul 25 bit
							addr = addr | ~J_AddrMask;  // aggiungo degli uni se il bit del segno e` settato
						}
						PC += addr;
					}
					break;

				case OpCode.J_js:
					{
						uint* U = (uint*)regs;
						int* R = (int*)regs;
						U[31] = PC;
						R[30] = mvWindow;

						regs = ((uint*)regs) + mvWindow;
						mvWindow = 0;

						uint addr = A & J_AddrMask;  // 26 bit
						if ((((J_AddrMask + 1) >> 1) & addr) != 0) // controllo il bit del segno.
						{
							// complemento a 2 sul 25 bit
							addr = addr | ~J_AddrMask;  // aggiungo degli uni se il bit del segno e` settato
						}
						PC += addr;
					}
					break;

				case OpCode.J_ret:
					{
						int* R = (int*)regs;
						uint* U = (uint*)regs;

						int wr = R[30];
						uint r = U[31];
						
						regs = ((uint*)regs) - wr;
						PC = r;
					}
					break;


				case OpCode.I_addi:
				case OpCode.I_andi:
				case OpCode.I_ori:
				case OpCode.I_beq:
				case OpCode.I_bne:
				case OpCode.I_bgt:
				case OpCode.I_bge:
				case OpCode.I_blt:
				case OpCode.I_ble:

				case OpCode.I_lui:

				case OpCode.I_lw:
				case OpCode.I_lh:
				case OpCode.I_lhu:
				case OpCode.I_lb:
				case OpCode.I_lbu:

				case OpCode.I_sw:
				case OpCode.I_sb:
				case OpCode.I_sh:

				case OpCode.I_slti:

				case OpCode.I_mvw:

				case OpCode.I_lr:
				case OpCode.I_sr:
					{
						int* R = (int*)regs;
						uint* U = (uint*)regs;

						uint rs = (A & (RegMask << RsPos)) >> RsPos;
						uint rt = (A & (RegMask << RtPos)) >> RtPos;
						int S = (short)((A & (I_ImmMask << I_ImmPos)) >> I_ImmPos);   
						uint C = ((A & (I_ImmMask << I_ImmPos)) >> I_ImmPos);

						switch (op)
						{
						case OpCode.I_beq: if (R[rs] == R[rt]) PC += (uint)S; break;
						case OpCode.I_bne: if (R[rs] != R[rt]) PC += (uint)S; break;
						case OpCode.I_bgt: if (R[rs] >  R[rt]) PC += (uint)S; break;
						case OpCode.I_bge: if (R[rs] >= R[rt]) PC += (uint)S; break;
						case OpCode.I_blt: if (R[rs] <  R[rt]) PC += (uint)S; break;
						case OpCode.I_ble: if (R[rs] <= R[rt]) PC += (uint)S; break;

						case OpCode.I_ori: U[rt] = U[rs] | C; break;
						case OpCode.I_andi: U[rt] = U[rs] & C; break;

						case OpCode.I_addi: U[rt] = U[rs] + (uint)S; break;

						case OpCode.I_lui: U[rt] = (C << 16); break;

						case OpCode.I_lw: R[rt] = *((int*)((byte*)mem + U[rs] + (uint)S)); break;
						case OpCode.I_lh: R[rt] = *((short*)((byte*)mem + U[rs] + (uint)S)); break;
						case OpCode.I_lhu: U[rt] = *((ushort*)((byte*)mem + U[rs] + (uint)S)); break;
						case OpCode.I_lb: R[rt] = *((sbyte*)((byte*)mem + U[rs] + (uint)S)); break;
						case OpCode.I_lbu: U[rt] = *((byte*)((byte*)mem + U[rs] + (uint)S)); break;

						case OpCode.I_sw: *(uint*)((byte*)mem + U[rs] + (uint)S) = U[rt]; break;
						case OpCode.I_sb: *(byte*)((byte*)mem + U[rs] + (uint)S) = (byte)U[rt]; break;
						case OpCode.I_sh: *(short*)((byte*)mem + U[rs] + (uint)S) = (short)U[rt]; break;

						case OpCode.I_slti: R[rt] = R[rs] < S ? 1 : 0; break;

						case OpCode.I_mvw: mvWindow = S; break;

						case OpCode.I_lr: R[rt] = R[S + R[rs]]; break;
						case OpCode.I_sr: R[S + R[rs]] = R[rt]; break;
						}
					}
					break;
				}
			}
		}
	}
}
