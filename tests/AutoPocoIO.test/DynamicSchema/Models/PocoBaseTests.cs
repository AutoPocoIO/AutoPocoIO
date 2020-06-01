using AutoPocoIO.DynamicSchema.Models;
using Xunit;
using System.Linq;

namespace AutoPocoIO.test.DynamicSchema.Models
{
    
     [Trait("Category", TestCategories.Unit)]
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

        [FactWithName]
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

            Assert.Equal("Prop1=abc,Prop2=123,Prop5=456", obj.ToString());
        }

        [FactWithName]
        public void StringValueWithNullProperties()
        {
            Assert.Equal("Prop1=null,Prop2=0,Prop5=null", new PocoImplementations().ToString());
        }

        [FactWithName]
        public void StringValueWithNoPropertiesShowsTypeName()
        {
            Assert.Equal("AutoPocoIO.test.DynamicSchema.Models.PocoBaseTests+PocoNoProperties", new PocoNoProperties().ToString());
        }

        [FactWithName]
        public void RaisedEventWithoutRegisteringDoesNothing()
        {
            var obj = new PocoWithEvent();
            obj.OnRaisePropertyChanged(this, "test");

             Assert.Null(obj.ProperyName);
        }

        [FactWithName]
        public void RaisedEventWithoutRegisteringSetsValue()
        {
            var obj = new PocoWithEvent();
            obj.PropertyChanged += (sender, prop) => obj.PocoWithEvent_PropertyChanged(sender, prop);
            obj.OnRaisePropertyChanged(this, "test");

            Assert.Equal("test", obj.ProperyName);
        }

        [FactWithName]
        public void CustomListProperyToDynamicList()
        {
            var list = new CustomList<string>() { "a", "b" };
            var dynamicList = list.AsDynamic();

            Assert.Equal("a", dynamicList.First());
            Assert.Equal("b", dynamicList.Last());


        }
    }
}
