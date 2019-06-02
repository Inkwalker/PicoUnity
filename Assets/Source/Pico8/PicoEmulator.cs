using PicoMoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace PicoUnity
{
    public class PicoEmulator
    {
        private Script luaEngine;
        private MemoryModule memory;
        private GraphicsModule gpu;

        public Texture2D ScreenTexture => gpu.Texture;

        public PicoEmulator()
        {
            memory = new MemoryModule();
            memory.InitRam();

            gpu = new GraphicsModule(memory, "pico8");

            luaEngine = new Script();

            RegisterAPI( memory.GetApiTable() );
            RegisterAPI( gpu.GetApiTable() );
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
            Call("_draw");

            gpu.Flip();
        }

        public void LoadCartridge(Cartridge cart)
        {
            memory.CopyFrom(cart.Rom, 0, 0, MemoryModule.ADDR_GENERAL);

            var script = cart.ExtractScript();

            Run(script);
            Call("_init");
        }
    }
}
