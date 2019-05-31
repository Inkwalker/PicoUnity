using System.Collections.Generic;

using PicoMoonSharp.Interpreter.Execution;


namespace PicoMoonSharp.Interpreter.Tree.Statements
{
	class CompositeStatement : Statement 
	{
		List<Statement> m_Statements = new List<Statement>();

		public CompositeStatement(ScriptLoadingContext lcontext, bool oneLine = false)
			: base(lcontext)
		{
            int line = lcontext.Lexer.Current.FromLine;

            while (true)
			{
				Token t = lcontext.Lexer.Current;
				if (t.IsEndOfBlock() || (oneLine && line != t.FromLine)) break;

				bool forceLast;
				
				Statement s = Statement.CreateStatement(lcontext, out forceLast, oneLine);
				m_Statements.Add(s);

				if (forceLast) break;
			}

			// eat away all superfluos ';'s
			while (lcontext.Lexer.Current.Type == TokenType.SemiColon)
				lcontext.Lexer.Next();
		}


		public override void Compile(Execution.VM.ByteCode bc)
		{
			if (m_Statements != null)
			{
				foreach (Statement s in m_Statements)
				{
					s.Compile(bc);
				}
			}
		}
	}
}
