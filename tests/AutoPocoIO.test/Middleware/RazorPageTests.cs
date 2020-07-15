using AutoPocoIO.Middleware;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPocoIO.test.Middleware
{
    [TestClass]
    [TestCategory(TestCategories.Unit)]
    public class RazorPageTests
    {
        private class RazorPublic : RazorPage
        {
            public RazorPublic(ILayoutPage layout, string title) 
                : base(layout, title)
            {
            }

            public RazorPublic()
            {
            }

            public bool ExecuteCalled { get; set; }

            public new string TransformUrl(string url) => base.TransformUrl(url);
            public new void WriteLiteral(string txt) => base.WriteLiteral(txt);
            public new void Write(object txt) => base.Write(txt);
            public new void WriteAttribute(string name, Tuple<string, int> prefix, Tuple<string, int> suffix, params object[] fragments) =>
               base.WriteAttribute(name, prefix, suffix, fragments);
            public new void DefineSection(string scriptTag, Action renderAction)
            {
                base.DefineSection(scriptTag, renderAction);
            }
            public new object RenderSection(string scriptTag)
            {
                return base.RenderSection(scriptTag);
            }

            public static new NonEscapedString TransformArray(object list) => RazorPage.TransformArray(list);

            public new T GetViewBagValue<T>(string key) where T : class => base.GetViewBagValue<T>(key);
            public new string GetError(string errorName, string errorKey) => base.GetError(errorName, errorKey);
            public override void Execute()
            {
                ExecuteCalled = true;
            }
        }

        [TestMethod]
        public void AssignSetsContext()
        {
            var req = new Mock<IMiddlewareRequest>();
            req.Setup(c => c.Path).Returns("reqPath");

            var context = new Mock<IMiddlewareContext>();
            context.Setup(c => c.Request).Returns(req.Object);

            var page = new RazorPublic();
            page.Assign(context.Object);

            Assert.AreEqual(req.Object, page.Request);
        }

        [TestMethod]
        public void AssignParentPage()
        {
            var reqParent = new Mock<IMiddlewareRequest>();
            reqParent.Setup(c => c.PathBase).Returns("parentPath");

            var contextParent = new Mock<IMiddlewareContext>();
            contextParent.Setup(c => c.Request).Returns(reqParent.Object);

            var pageParent = new RazorPublic();
            pageParent.Assign(contextParent.Object);

            var pageChild = new RazorPublic();
            pageChild.Assign(pageParent);

            Assert.AreEqual("parentPath/test", pageChild.TransformUrl("/test"));
            Assert.AreEqual(contextParent.Object, pageChild.Context);
        }

        [TestMethod]
        public void DefinieSectionDoesntChangePageContent()
        {
            var page = new RazorPublic();
            page.WriteLiteral("test");

            page.DefineSection("section", () => page.WriteLiteral("section"));

            Assert.AreEqual("test", page.ToString());
        }

        [TestMethod]
        public void DefineSectionAddsToLayout()
        {
            string sectionValue = ""; 
            var layout = new Mock<ILayoutPage>();
            layout.SetupSet(c => c.Sections["s1"] = It.IsAny<string>())
                .Callback<string, string>((key, val) => sectionValue = val);

            var page = new RazorPublic(layout.Object, "");
            page.DefineSection("s1", () => page.WriteLiteral("sval"));

            Assert.AreEqual("sval", sectionValue);
        }

        [TestMethod]
        public void WriteAttributeWithFragments()
        {

            Tuple<string, int> prefix = new Tuple<string, int>("pre/", 1);
            Tuple<string, int> suffix = new Tuple<string, int>("post", 2);

            var fragments = new object[]
            {
                new Tuple<Tuple<string, int>, Tuple<string, int>, bool>(new Tuple<string, int>("sf1-1/", 1), new Tuple<string, int>("sf1-2/", 2), true),
                new Tuple<Tuple<string, int>, Tuple<string, int>, bool>(new Tuple<string, int>("sf2-1/", 1), new Tuple<string, int>("sf2-2/", 2), true)
            };

            var page = new RazorPublic();
            page.WriteAttribute("attr", prefix, suffix, fragments);

            Assert.AreEqual("pre/sf1-2/sf2-1/sf2-2/post", page.ToString());
        }

        [TestMethod]
        public void WriteAttributeWithOFFragments()
        {

            Tuple<string, int> prefix = new Tuple<string, int>("pre/", 1);
            Tuple<string, int> suffix = new Tuple<string, int>("/post", 2);

            var fragments = new object[]
            {
                new Tuple<Tuple<string, int>, Tuple<object, int>, bool>(new Tuple<string, int>("sf1-1/", 1), new Tuple<object, int>(1, 2), true),
                new Tuple<Tuple<string, int>, Tuple<object, int>, bool>(new Tuple<string, int>("/sf2-1/", 1), new Tuple<object, int>(2, 2), true)
            };

            var page = new RazorPublic();
            page.WriteAttribute("attr", prefix, suffix, fragments);

            Assert.AreEqual("pre/1/sf2-1/2/post", page.ToString());
        }

        [TestMethod]
        public void WriteAttributeWithNonTupleFragments()
        {

            Tuple<string, int> prefix = new Tuple<string, int>("pre/", 1);
            Tuple<string, int> suffix = new Tuple<string, int>("post", 2);

            var fragments = new object[]
            {
                1
            };

            var page = new RazorPublic();
            page.WriteAttribute("attr", prefix, suffix, fragments);

            Assert.AreEqual("pre/post", page.ToString());
        }

        [TestMethod]
        public void WriteNull()
        {
            var page = new RazorPublic();
            page.Write(null);

            Assert.AreEqual("", page.ToString());
        }

        [TestMethod]
        public void TransformStringArray()
        {
            var list = new[] { "a", "b", "cde" };
            var results = RazorPublic.TransformArray(list);

            Assert.AreEqual("['a','b','cde']", results.ToString());
        }

        [TestMethod]
        public void TransformIntArray()
        {
            var list = new[] { 1, 2, 345 };
            var results = RazorPublic.TransformArray(list);

            Assert.AreEqual("[1,2,345]", results.ToString());
        }

        [TestMethod]
        public void TransformObjectArray()
        {
            var obj = new { Id = 1 };
            var list = new[] { obj };
            var results = RazorPublic.TransformArray(list);

            Assert.AreEqual($"{obj.GetType()}[]", results.ToString());
        }

        private class ViewBag1
        {
            public int id { get; set; }
        }

        [TestMethod]
        public void GetViewBagValueOfT()
        {

            var page = new RazorPublic();
            page.ViewBag["v1"] = new ViewBag1 { id = 1 };

            Assert.AreEqual(1, page.GetViewBagValue<ViewBag1>("v1").id);
        }

        [TestMethod]
        public void GetViewBagValueTypeMismatchReturnsNull()
        {
            var page = new RazorPublic();
            page.ViewBag["v1"] = new ViewBag1 { id = 1 };

            Assert.IsNull(page.GetViewBagValue<RazorPublic>("v1"));
        }
        [TestMethod]
        public void GetViewBagValueKeyMissingReturnsNull()
        {
            var page = new RazorPublic();
            page.ViewBag["v1"] = new ViewBag1 { id = 1 };

            Assert.IsNull(page.GetViewBagValue<ViewBag1>("v2"));
        }

        [TestMethod]
        public void ErrorsValueNotADictionary()
        {
            var page = new RazorPublic();
            page.ViewBag["v1"] = new ViewBag1();

            Assert.AreEqual("", page.GetError("v1", "anykey"));
        }

        [TestMethod]
        public void ErrorsKeyNotFound()
        {
            var page = new RazorPublic();
            page.ViewBag["v1"] = new Dictionary<string, string>();

            Assert.AreEqual("", page.GetError("v1", "anykey"));
        }

        [TestMethod]
        public void ErrorsKeyFound()
        {
            var page = new RazorPublic();
            page.ViewBag["v1"] = new Dictionary<string, string>() { { "anykey", "error123" } };

            Assert.AreEqual("error123", page.GetError("v1", "anykey"));
        }

    }
}
