using System;

namespace ConditionSystem
{
    public class RestartNotFoundException : Exception
    {
        public string Name { get; private set; }

        public RestartNotFoundException(string name)
        {
            if (null == name)
                throw new ArgumentNullException("name");
            this.Name = name;
        }
    }
}
