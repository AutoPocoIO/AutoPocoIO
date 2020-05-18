using System;
using System.Reflection;

namespace Microsoft.VisualStudio.TestTools.UnitTesting
{
#if NETCORE
    internal class PrivateObject
    {
        readonly Type _type;
        readonly object _instance;
        public PrivateObject(object instance)
        {
            _type = instance.GetType();
            _instance = instance;
        }

        public object GetField(string name)
        {
            var field = _type.GetField(name, BindingFlags.NonPublic | BindingFlags.Instance);
            return field.GetValue(_instance);
        }
    }
#endif
}
