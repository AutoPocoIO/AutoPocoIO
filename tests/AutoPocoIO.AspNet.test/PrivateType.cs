using System;
using System.Reflection;

namespace AutoPocoIO.AspNet.test
{
    internal class PrivateType
    {
        readonly Type _type;

        public PrivateType(Type type)
        {
            _type = type;
        }

        public void SetStaticField(string name, object value)
        {
            var field = _type.GetField(name, BindingFlags.NonPublic | BindingFlags.Static);
            field.SetValue(null, value);
        }
    }

    internal class PrivateObject
    {
        readonly Type _type;
        readonly object _instance;
        public PrivateObject(Type type)
        {
            _type = type;
            _instance = Activator.CreateInstance(_type);
        }

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

        public void SetField(string name, object value)
        {
            var field = _type.GetField(name, BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(_instance, value);
        }
    }
}
