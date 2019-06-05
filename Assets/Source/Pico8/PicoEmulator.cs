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

        private List<EmulatorModule> modules;

        public Texture2D ScreenTexture => gpu.Texture;

        public PicoEmulator()
        {
            modules = new List<EmulatorModule>();

            memory = new MemoryModule();
            memory.InitRam();

            gpu = new GraphicsModule(memory, "pico8");

            luaEngine = new Script();

            var storage = new PersistentDataStorage(memory);
            memory.AddMemoryListener(storage);

            AddModule(memory);
            AddModule(gpu);
            AddModule(storage);
        }

        public void AddModule(EmulatorModule module)
        {
            modules.Add(module);

            RegisterAPI(module.GetApiTable());
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

        public void Update(float dt)
        {
            foreach (var module in modules)
            {
                module.OnFrameStart(dt);
            }

            Call("_update");
            Call("_draw");

            foreach (var module in modules)
            {
                module.OnFrameEnd(dt);
            }
        }

        public void LoadCartridge(ACartridge cart)
        {
            memory.CopyFrom(cart.Rom, 0, 0, MemoryModule.ADDR_GENERAL);

            var script = cart.Lua;

            Run(script);
            Call("_init");
        }
    }
}
