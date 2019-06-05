using System.Collections.Generic;

namespace PicoUnity
{
    public abstract class EmulatorModule
    {
        public abstract ApiTable GetApiTable();
        public virtual void OnFrameStart(float dt) { }
        public virtual void OnFrameEnd(float dt) { }

        public class ApiTable : Dictionary<string, object> { }
    }
}
