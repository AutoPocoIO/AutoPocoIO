using AutoPocoIO.DynamicSchema.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace AutoPocoIO.test.DynamicSchema.Models
{
    [TestClass]
    [TestCategory(TestCategories.Unit)]
    public class PocoBaseTests
    {
        private class PocoImplementations : PocoBase
        {
            public string Prop1 { get; set; }
            public int Prop2 { get; set; }
            public byte[] Prop3 { get; set; }
            public PocoImplementations Prop4 { get; set; }
            public int? Prop5 { get; set; }
        }

        private class PocoNoProperties : PocoBase
        { }

        private class PocoWithEvent : PocoBase
        {

            public void PocoWithEvent_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
            {
                ProperyName = e.PropertyName;
            }

            public string ProperyName { get; set; }
        }

        [TestMethod]
        public void StringValueWithProperties()
        {
            var obj = new PocoImplementations()
            {
                Prop1 = "abc",
                Prop2 = 123,
                Prop3 = new byte[0],
                Prop4 = new PocoImplementations(),
                Prop5 = 456,
            };

            Assert.AreEqual("Prop1=abc,Prop2=123,Prop5=456", obj.ToString());
        }

        [TestMethod]
        public void StringValueWithNullProperties()
        {
            Assert.AreEqual("Prop1=null,Prop2=0,Prop5=null", new PocoImplementations().ToString());
        }

        [TestMethod]
        public void StringValueWithNoPropertiesShowsTypeName()
        {
            Assert.AreEqual("AutoPocoIO.test.DynamicSchema.Models.PocoBaseTests+PocoNoProperties", new PocoNoProperties().ToString());
        }

        [TestMethod]
        public void RaisedEventWithoutRegisteringDoesNothing()
        {
            var obj = new PocoWithEvent();
            obj.OnRaisePropertyChanged(this, "test");

            Assert.IsNull(obj.ProperyName);
        }

        [TestMethod]
        public void RaisedEventWithoutRegisteringSetsValue()
        {
            var obj = new PocoWithEvent();
            obj.PropertyChanged += (sender, prop) => obj.PocoWithEvent_PropertyChanged(sender, prop);
            obj.OnRaisePropertyChanged(this, "test");

            Assert.AreEqual("test", obj.ProperyName);
        }

        [TestMethod]
        public void CustomListProperyToDynamicList()
        {
            var list = new CustomList<string>() { "a", "b" };
            var dynamicList = list.AsDynamic();

            Assert.AreEqual("a", dynamicList.First());
            Assert.AreEqual("b", dynamicList.Last());


        }
    }
}
