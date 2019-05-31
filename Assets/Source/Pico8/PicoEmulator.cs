using PicoMoonSharp.Interpreter;
using System;
using System.Collections.Generic;

namespace PicoUnity
{
    public class PicoEmulator
    {
        private Script luaEngine;
        private MemoryModule memory;

        public PicoEmulator()
        {
            memory = new MemoryModule();
            memory.InitRam();

            luaEngine = new Script();

            RegisterAPI( memory.GetApiTable() );
        }

        private void RegisterAPI(EmulatorModule.ApiTable api)
        {
            foreach (var item in api)
            {
                luaEngine.Globals[item.Key] = item.Value;
            }
        }

        private void Call(string func, params object[] args)
        {
            if (luaEngine.Globals[func] != null)
            {
                luaEngine.Call(luaEngine.Globals[func], args);
            }
        }

        public void Run(string script)
        {
            luaEngine.DoString(script);
        }

        public void Update()
        {
            Call("_update");
            //Call("_draw");
        }
    }
}
