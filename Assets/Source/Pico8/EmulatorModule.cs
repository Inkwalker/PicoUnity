using System.Collections.Generic;

namespace PicoUnity
{
    public abstract class EmulatorModule
    {
        public abstract ApiTable GetApiTable();

        public class ApiTable : Dictionary<string, object> { }
    }
}
