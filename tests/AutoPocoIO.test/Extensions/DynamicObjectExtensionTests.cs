using AutoPocoIO.Extensions;
using AutoPocoIO.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using Xunit;

namespace AutoPocoIO.test.Extensions
{
    
    [Trait("Category", TestCategories.Unit)]
    public class DynamicObjectExtensionTests
    {
        [FactWithName]
        public void ConvertAnoymousToDynamic()
        {
            var obj = new { prop1 = 1, prop2 = "test" };
            var dynamicObject = obj.ToDynamic();

            Assert.Equal(1, dynamicObject.prop1);
            Assert.Equal("test", dynamicObject.prop2);
            Assert.IsType<ExpandoObject>(dynamicObject);
        }

        [FactWithName]
        public void JobjectToObject()
        {
            JObject jobject = new JObject
            {
                ["prop1"] = 1,
                ["prop2"] = "test"
            };

            dynamic dotnetObj = jobject.JTokenToConventionalDotNetObject();

            Assert.Equal(1, dotnetObj["prop1"]);
            Assert.Equal("test", dotnetObj["prop2"]);
            Assert.IsType<Dictionary<string, object>>(dotnetObj);
        }

        [FactWithName]
        public void JArrayToObject()
        {
            JArray jarray = new JArray
            {
                "test",
                "test2"
            };

            JObject jobject = new JObject
            {
                ["array1"] = jarray
            };

            dynamic dotnetObj = jobject.JTokenToConventionalDotNetObject();

            Assert.Equal("test", dotnetObj["array1"][0]);
            Assert.Equal("test2", dotnetObj["array1"][1]);
            Assert.IsType<Dictionary<string, object>>(dotnetObj);
        }

        [FactWithName]
        public void PopulateObjectPropertiesFromJson()
        {
            JObject jobject = new JObject
            {
                ["prop1"] = 1,
                ["prop2"] = "test"
            };

            var obj = new Entity2 { Prop1 = 34, Prop3 = "notchanging" };
            jobject.PopulateObjectFromJToken(obj);

            Assert.Equal(1, obj.Prop1);
            Assert.Equal("notchanging", obj.Prop3);
        }

        [FactWithName]
        public void PopulateModelNull()
        {
            var obj = DynamicObjectExtensions.PopulateModel(null, typeof(Entity));
             Assert.Null(obj);
        }

        [FactWithName]
        public void PopiulateModelWithNoProperties()
        {
            Connector conn = new Connector();
            var obj = DynamicObjectExtensions.PopulateModel(conn, typeof(NoProps));
            Assert.IsType<NoProps>(obj);
        }

        [FactWithName]
        public void PopulateFromSameType()
        {
            Entity source = new Entity
            {
                Prop1 = 1,
                Prop2 = "prop22"
            };

            var result = DynamicObjectExtensions.PopulateModel(source, typeof(Entity));
            Assert.Equal(1, result.Prop1);
            Assert.Equal("prop22", result.Prop2);
        }

        [FactWithName]
        public void PopulateLessProperties()
        {
            var source = new { prop1 = 23 };
            var result = DynamicObjectExtensions.PopulateModel(source, typeof(Entity));

            Assert.Equal(23, result.Prop1);
            Assert.Equal(null, result.Prop2);
        }

        [FactWithName]
        public void PopulateNoMatchingProperties()
        {
            var source = new { Prop3 = 23 };
            var result = DynamicObjectExtensions.PopulateModel(source, typeof(Entity));

            Assert.Equal(0, result.Prop1);
            Assert.Equal(null, result.Prop2);
        }

        [FactWithName]
        public void PopiulateTModelWithNoProperties()
        {
            Connector conn = new Connector();
            var obj = conn.PopulateModel<NoProps>();
            Assert.IsType<NoProps>(obj);
        }

        [FactWithName]
        public void PopulateTModelWithNullValue()
        {
            Connector conn = null;
            var obj = conn.PopulateModel<NoProps>();
             Assert.Null(obj);
        }

        [FactWithName]
        public void PopulateTFromSameType()
        {
            Entity source = new Entity
            {
                Prop1 = 1,
                Prop2 = "prop22"
            };

            var result = source.PopulateModel<Entity>();
            Assert.Equal(1, result.Prop1);
            Assert.Equal("prop22", result.Prop2);
        }

        [FactWithName]
        public void PopulateTLessProperties()
        {
            var source = new { prop1 = 23 };
            var result = source.PopulateModel<Entity>();

            Assert.Equal(23, result.Prop1);
            Assert.Null(result.Prop2);
        }

        [FactWithName]
        public void PopulateTNoMatchingProperties()
        {
            var source = new { prop3 = 23 };
            var result = source.PopulateModel<Entity>();

            Assert.Equal(0, result.Prop1);
            Assert.Null(result.Prop2);
        }

        [FactWithName]
        public void PopulateUpdates()
        {
            var source = new { prop1 = 1, prop3 = "aa" };
            var model = new Entity { Prop1 = 11, Prop2 = "bb" };

            DynamicObjectExtensions.PopulateModel(source, model);
            Assert.Equal(1, model.Prop1);
            Assert.Equal("bb", model.Prop2);
        }

        [FactWithName]
        public void AsDictionary()
        {
            var obj = new { prop1 = 1, prop2 = "abc" };
            var dictionary = obj.AsDictionary();

            Assert.Equal(1, dictionary["prop1"]);
            Assert.Equal("abc", dictionary["prop2"]);
        }

        [FactWithName]
        public void AsDictionaryNoProps()
        {
            var obj = new NoProps();
            var dictionary = obj.AsDictionary();
            Assert.Equal(0, dictionary.Count);
        }

        [FactWithName]
        public void AsDictionaryNull()
        {
            Entity obj = null;
             void act() => obj.AsDictionary();
            Assert.Throws<ArgumentNullException>(act);
        }

        private class NoProps { }
        private class Entity
        {
            public int Prop1 { get; set; }
            public string Prop2 { get; set; }
        }
        private class Entity2
        {
            public int Prop1 { get; set; }
            public string Prop3 { get; set; }
        }
    }
}
