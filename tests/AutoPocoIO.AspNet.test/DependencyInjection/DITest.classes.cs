using System;
using System.Collections;
using System.Collections.Generic;

namespace AutoPocoIO.AspNet.test.DependencyInjection
{
    public class Class1 : IInterface1 { public int prop1 { get; set; } }

    public class Class2 : IInterface1 { }

    public interface IInterface1 { }


    public class ClassWtihTwoSingleConstructor
    {
        public ClassWtihTwoSingleConstructor(Class1 c1) { }
        public ClassWtihTwoSingleConstructor(Class2 c2) { }
    }

    public class ClassWithPrivateConstructor
    {
        private ClassWithPrivateConstructor() { }
    }

    public class ClassWtihSingleConstructor
    {
        public ClassWtihSingleConstructor(Class1 c1) { }
    }

    public class ClassWtihConstructors
    {
        public ClassWtihConstructors(Class1 c1) 
        {
            this.c1 = c1;
        }
        public ClassWtihConstructors(Class1 c1, Class2 c2)
        {
            this.c1 = c1;
            this.c2 = c2;
        }

        public Class1 c1 { get; set; }
        public Class2 c2 { get; set; }
    }

    public class ClassWithListParameter
    {
        public ClassWithListParameter(IEnumerable<IInterface1> c1) { this.c1 = c1; }
        public IEnumerable<IInterface1> c1 { get; }
    }

    public class Disposable1 : IDisposable
    {
        public bool IsDisposed { get; private set; }
        public void Dispose()
        {
            IsDisposed = true;
        }
    }
}
