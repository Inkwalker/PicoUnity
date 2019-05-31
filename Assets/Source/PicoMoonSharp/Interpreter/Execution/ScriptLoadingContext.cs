using PicoMoonSharp.Interpreter.Debugging;
using PicoMoonSharp.Interpreter.Tree;

namespace PicoMoonSharp.Interpreter.Execution
{
	class ScriptLoadingContext
	{
		public Script Script { get; private set; }
		public BuildTimeScope Scope { get; set; }
		public SourceCode Source { get; set; }
		public bool Anonymous { get; set; }
		public bool IsDynamicExpression { get; set; }
		public Lexer Lexer { get; set; }

		public ScriptLoadingContext(Script s)
		{
			Script = s;
		}

	}
}
