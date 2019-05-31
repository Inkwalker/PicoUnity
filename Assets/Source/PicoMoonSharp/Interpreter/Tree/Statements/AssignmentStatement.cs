using System;
using System.Collections.Generic;
using PicoMoonSharp.Interpreter.Debugging;
using PicoMoonSharp.Interpreter.Execution;

using PicoMoonSharp.Interpreter.Tree.Expressions;

namespace PicoMoonSharp.Interpreter.Tree.Statements
{
	class AssignmentStatement : Statement
	{
		List<IVariable> m_LValues = new List<IVariable>();
		List<Expression> m_RValues;
		SourceRef m_Ref;
        TokenType mod = TokenType.Op_Assignment;

        public AssignmentStatement(ScriptLoadingContext lcontext, Token startToken)
			: base(lcontext)
		{
			List<string> names = new List<string>();

			Token first = startToken;

			while (true)
			{
				Token name = CheckTokenType(lcontext, TokenType.Name);
				names.Add(name.Text);

				if (lcontext.Lexer.Current.Type != TokenType.Comma)
					break;

				lcontext.Lexer.Next();
			}

            if (lcontext.Lexer.Current.IsAssignmentOperator())
            {
                mod = lcontext.Lexer.Current.Type;
                lcontext.Lexer.Next();
                m_RValues = Expression.ExprList(lcontext);
            }
            else
            {
                m_RValues = new List<Expression>();
            }

            foreach (string name in names)
			{
				var localVar = lcontext.Scope.TryDefineLocal(name);
				var symbol = new SymbolRefExpression(lcontext, localVar);
				m_LValues.Add(symbol);
			}

            WrapRValues(lcontext);

            Token last = lcontext.Lexer.Current;
			m_Ref = first.GetSourceRefUpTo(last);
			lcontext.Source.Refs.Add(m_Ref);

		}


		public AssignmentStatement(ScriptLoadingContext lcontext, Expression firstExpression, Token first)
			: base(lcontext)
		{
			m_LValues.Add(CheckVar(lcontext, firstExpression));

			while (lcontext.Lexer.Current.Type == TokenType.Comma)
			{
				lcontext.Lexer.Next();
				Expression e = Expression.PrimaryExp(lcontext);
				m_LValues.Add(CheckVar(lcontext, e));
			}

            if (lcontext.Lexer.Current.IsAssignmentOperator())
            {
                mod = lcontext.Lexer.Current.Type;
                lcontext.Lexer.Next();
            }
            else
            {
                UnexpectedTokenType(lcontext.Lexer.Current);
                lcontext.Lexer.Next();
            }

            m_RValues = Expression.ExprList(lcontext);

            WrapRValues(lcontext);

            Token last = lcontext.Lexer.Current;
			m_Ref = first.GetSourceRefUpTo(last);
			lcontext.Source.Refs.Add(m_Ref);

		}

		private IVariable CheckVar(ScriptLoadingContext lcontext, Expression firstExpression)
		{
			IVariable v = firstExpression as IVariable;

			if (v == null)
				throw new SyntaxErrorException(lcontext.Lexer.Current, "unexpected symbol near '{0}' - not a l-value", lcontext.Lexer.Current);

			return v;
		}

        private void WrapRValues(ScriptLoadingContext lcontext)
        {
            if (mod == TokenType.Op_Assignment) return;

            for (int i = 0; i < m_LValues.Count; i++)
            {
                if (i < m_RValues.Count)
                {
                    Expression e = null;
                    switch (mod)
                    {
                        case TokenType.Op_AssignmentAdd:
                            e = BinaryOperatorExpression.CreateAddEpression(m_LValues[i] as Expression, m_RValues[i], lcontext);
                            break;
                        case TokenType.Op_AssignmentDiv:
                            e = BinaryOperatorExpression.CreateDivEpression(m_LValues[i] as Expression, m_RValues[i], lcontext);
                            break;
                        case TokenType.Op_AssignmentMod:
                            e = BinaryOperatorExpression.CreateModEpression(m_LValues[i] as Expression, m_RValues[i], lcontext);
                            break;
                        case TokenType.Op_AssignmentMul:
                            e = BinaryOperatorExpression.CreateMulEpression(m_LValues[i] as Expression, m_RValues[i], lcontext);
                            break;
                        case TokenType.Op_AssignmentSub:
                            e = BinaryOperatorExpression.CreateSubEpression(m_LValues[i] as Expression, m_RValues[i], lcontext);
                            break;
                    }

                    if (e != null) m_RValues[i] = e;
                }
            }
        }

        public override void Compile(Execution.VM.ByteCode bc)
		{
			using (bc.EnterSource(m_Ref))
			{
				foreach (var exp in m_RValues)
				{
					exp.Compile(bc);
				}

				for (int i = 0; i < m_LValues.Count; i++)
					m_LValues[i].CompileAssignment(bc,
							Math.Max(m_RValues.Count - 1 - i, 0), // index of r-value
							i - Math.Min(i, m_RValues.Count - 1)); // index in last tuple

				bc.Emit_Pop(m_RValues.Count);
			}
		}

	}
}
