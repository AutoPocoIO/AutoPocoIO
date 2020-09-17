using AutoPocoIO.Extensions;
using AutoPocoIO.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;

namespace AutoPocoIO.test.Extensions
{
    [TestClass]
    [TestCategory(TestCategories.Unit)]
    public class DynamicObjectExtensionTests
    {
        [TestMethod]
        public void ConvertAnoymousToDynamic()
        {
            var obj = new { prop1 = 1, prop2 = "test" };
            var dynamicObject = obj.ToDynamic();

            Assert.AreEqual(1, dynamicObject.prop1);
            Assert.AreEqual("test", dynamicObject.prop2);
            Assert.IsInstanceOfType(dynamicObject, typeof(ExpandoObject));
        }

        [TestMethod]
        public void JobjectToObject()
        {
            JObject jobject = new JObject
            {
                ["prop1"] = 1,
                ["prop2"] = "test"
            };

            dynamic dotnetObj = jobject.JTokenToConventionalDotNetObject();

            Assert.AreEqual(1, dotnetObj["prop1"]);
            Assert.AreEqual("test", dotnetObj["prop2"]);
            Assert.IsInstanceOfType(dotnetObj, typeof(Dictionary<string, object>));
        }

        [TestMethod]
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

            Assert.AreEqual("test", dotnetObj["array1"][0]);
            Assert.AreEqual("test2", dotnetObj["array1"][1]);
            Assert.IsInstanceOfType(dotnetObj, typeof(Dictionary<string, object>));
        }

        [TestMethod]
        public void PopulateObjectPropertiesFromJson()
        {
            JObject jobject = new JObject
            {
                ["prop1"] = 1,
                ["prop2"] = "test"
            };

            var obj = new Entity2 { Prop1 = 34, Prop3 = "notchanging" };
            jobject.PopulateObjectFromJToken(obj);

            Assert.AreEqual(1, obj.Prop1);
            Assert.AreEqual("notchanging", obj.Prop3);
        }

        [TestMethod]
        public void PopulateModelNull()
        {
            var obj = DynamicObjectExtensions.PopulateModel(null, typeof(Entity));
            Assert.IsNull(obj);
        }

        [TestMethod]
        public void PopiulateModelWithNoProperties()
        {
            Connector conn = new Connector();
            var obj = DynamicObjectExtensions.PopulateModel(conn, typeof(NoProps));
            Assert.IsInstanceOfType(obj, typeof(NoProps));
        }

        [TestMethod]
        public void PopulateFromSameType()
        {
            Entity source = new Entity
            {
                Prop1 = 1,
                Prop2 = "prop22"
            };

            var result = DynamicObjectExtensions.PopulateModel(source, typeof(Entity));
            Assert.AreEqual(1, result.Prop1);
            Assert.AreEqual("prop22", result.Prop2);
        }

        [TestMethod]
        public void PopulateLessProperties()
        {
            var source = new { prop1 = 23 };
            var result = DynamicObjectExtensions.PopulateModel(source, typeof(Entity));

            Assert.AreEqual(23, result.Prop1);
            Assert.AreEqual(null, result.Prop2);
        }

        [TestMethod]
        public void PopulateNoMatchingProperties()
        {
            var source = new { Prop3 = 23 };
            var result = DynamicObjectExtensions.PopulateModel(source, typeof(Entity));

            Assert.AreEqual(0, result.Prop1);
            Assert.AreEqual(null, result.Prop2);
        }

        [TestMethod]
        public void PopiulateTModelWithNoProperties()
        {
            Connector conn = new Connector();
            var obj = conn.PopulateModel<NoProps>();
            Assert.IsInstanceOfType(obj, typeof(NoProps));
        }

        [TestMethod]
        public void PopulateTModelWithNullValue()
        {
            Connector conn = null;
            var obj = conn.PopulateModel<NoProps>();
            Assert.IsNull(obj);
        }

        [TestMethod]
        public void PopulateTFromSameType()
        {
            Entity source = new Entity
            {
                Prop1 = 1,
                Prop2 = "prop22"
            };

            var result = source.PopulateModel<Entity>();
            Assert.AreEqual(1, result.Prop1);
            Assert.AreEqual("prop22", result.Prop2);
        }

        [TestMethod]
        public void PopulateTLessProperties()
        {
            var source = new { prop1 = 23 };
            var result = source.PopulateModel<Entity>();

            Assert.AreEqual(23, result.Prop1);
            Assert.AreEqual(null, result.Prop2);
        }

        [TestMethod]
        public void PopulateTNoMatchingProperties()
        {
            var source = new { prop3 = 23 };
            var result = source.PopulateModel<Entity>();

            Assert.AreEqual(0, result.Prop1);
            Assert.AreEqual(null, result.Prop2);
        }

        [TestMethod]
        public void PopulateUpdates()
        {
            var source = new { prop1 = 1, prop3 = "aa" };
            var model = new Entity { Prop1 = 11, Prop2 = "bb" };

            DynamicObjectExtensions.PopulateModel(source, model);
            Assert.AreEqual(1, model.Prop1);
            Assert.AreEqual("bb", model.Prop2);
        }

        [TestMethod]
        public void AsDictionary()
        {
            var obj = new { prop1 = 1, prop2 = "abc" };
            var dictionary = obj.AsDictionary();

            Assert.AreEqual(1, dictionary["prop1"]);
            Assert.AreEqual("abc", dictionary["prop2"]);
        }

        [TestMethod]
        public void AsDictionaryNoProps()
        {
            var obj = new NoProps();
            var dictionary = obj.AsDictionary();
            Assert.AreEqual(0, dictionary.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AsDictionaryNull()
        {
            Entity obj = null;
            _ = obj.AsDictionary();
            Assert.Fail("Exception not thrown");
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
