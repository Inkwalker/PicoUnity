﻿using PicoMoonSharp.Interpreter.Execution;

namespace PicoMoonSharp.Interpreter.Tree.Statements
{
	class EmptyStatement : Statement
	{
		public EmptyStatement(ScriptLoadingContext lcontext)
			: base(lcontext)
		{
		}


		public override void Compile(Execution.VM.ByteCode bc)
		{
		}
	}
}
