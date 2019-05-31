using System.Collections.Generic;
using PicoMoonSharp.Interpreter.Debugging;
using PicoMoonSharp.Interpreter.Execution;
using PicoMoonSharp.Interpreter.Execution.VM;


namespace PicoMoonSharp.Interpreter.Tree.Statements
{
	class IfStatement : Statement
	{
		private class IfBlock
		{
			public Expression Exp;
			public Statement Block;
			public RuntimeScopeBlock StackFrame;
			public SourceRef Source;
		}

		List<IfBlock> m_Ifs = new List<IfBlock>();
		IfBlock m_Else = null;
		SourceRef m_End;

		public IfStatement(ScriptLoadingContext lcontext)
			: base(lcontext)
		{
            bool shorthand = false;

            while (lcontext.Lexer.Current.Type != TokenType.Else && lcontext.Lexer.Current.Type != TokenType.End)
			{
				m_Ifs.Add(CreateIfBlock(lcontext, out shorthand));
                if (shorthand) break;
            }

			if (lcontext.Lexer.Current.Type == TokenType.Else)
			{
				m_Else = CreateElseBlock(lcontext, shorthand);
			}

            if (!shorthand)
            {
                m_End = CheckTokenType(lcontext, TokenType.End).GetSourceRef();
                lcontext.Source.Refs.Add(m_End);
            }
		}

		IfBlock CreateIfBlock(ScriptLoadingContext lcontext, out bool shorthand)
		{
			Token type = CheckTokenType(lcontext, TokenType.If, TokenType.ElseIf);

			lcontext.Scope.PushBlock();

			var ifblock = new IfBlock();
			ifblock.Exp = Expression.Expr(lcontext);

            //shorthand
            Token t = lcontext.Lexer.Current;
            if (type.Type == TokenType.If && t.Type != TokenType.Then)
            {
                shorthand = true;

                if (lcontext.Lexer.Current.FromLine == type.FromLine)
                {
                    ifblock.Source = type.GetSourceRef(t);
                    ifblock.Block = new CompositeStatement(lcontext, true);
                }
                else
                {
                    UnexpectedTokenType(t);
                }
            }
            else
            {
                shorthand = false;

                ifblock.Source = type.GetSourceRef(CheckTokenType(lcontext, TokenType.Then));
                ifblock.Block = new CompositeStatement(lcontext);
            }

            ifblock.StackFrame = lcontext.Scope.PopBlock();
            lcontext.Source.Refs.Add(ifblock.Source);

            return ifblock;
		}

		IfBlock CreateElseBlock(ScriptLoadingContext lcontext, bool oneLine)
		{
			Token type = CheckTokenType(lcontext, TokenType.Else);

			lcontext.Scope.PushBlock();

			var ifblock = new IfBlock();
			ifblock.Block = new CompositeStatement(lcontext, oneLine);
			ifblock.StackFrame = lcontext.Scope.PopBlock();
			ifblock.Source = type.GetSourceRef();
			lcontext.Source.Refs.Add(ifblock.Source);
			return ifblock;
		}


		public override void Compile(Execution.VM.ByteCode bc)
		{
			List<Instruction> endJumps = new List<Instruction>();

			Instruction lastIfJmp = null;

			foreach (var ifblock in m_Ifs)
			{
				using (bc.EnterSource(ifblock.Source))
				{
					if (lastIfJmp != null)
						lastIfJmp.NumVal = bc.GetJumpPointForNextInstruction();

					ifblock.Exp.Compile(bc);
					lastIfJmp = bc.Emit_Jump(OpCode.Jf, -1);
					bc.Emit_Enter(ifblock.StackFrame);
					ifblock.Block.Compile(bc);
				}

				using (bc.EnterSource(m_End))
					bc.Emit_Leave(ifblock.StackFrame);

				endJumps.Add(bc.Emit_Jump(OpCode.Jump, -1));
			}

			lastIfJmp.NumVal = bc.GetJumpPointForNextInstruction();

			if (m_Else != null)
			{
				using (bc.EnterSource(m_Else.Source))
				{
					bc.Emit_Enter(m_Else.StackFrame);
					m_Else.Block.Compile(bc);
				}

				using (bc.EnterSource(m_End))
					bc.Emit_Leave(m_Else.StackFrame);
			}

			foreach(var endjmp in endJumps)
				endjmp.NumVal = bc.GetJumpPointForNextInstruction();
		}



	}
}
