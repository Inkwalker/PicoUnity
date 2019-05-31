using PicoMoonSharp.Interpreter.DataStructs;
using PicoMoonSharp.Interpreter.Execution.VM;

namespace PicoMoonSharp.Interpreter.Execution
{
	interface ILoop
	{
		void CompileBreak(ByteCode bc);
		bool IsBoundary();
	}


	internal class LoopTracker
	{
		public FastStack<ILoop> Loops = new FastStack<ILoop>(16384);
	}
}
