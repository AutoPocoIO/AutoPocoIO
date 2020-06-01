using AutoPocoIO.CustomAttributes;
using AutoPocoIO.Exceptions;
using AutoPocoIO.Services;
using Xunit;
using Moq;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dependencies;
using System.Web.Http.Filters;
using System.Web.Http.Hosting;
using System.Web.Http.Routing;
//using System.Web.Mvc;

namespace AutoPocoIO.AspNet.test.CustomAttribute
{
    
    [Trait("Category", TestCategories.Unit)]
    public class UseJsonAttributeTests
    {
        class ViewModel1
        {
            public string Prop1 { get; set; }
        }

        LoggingService logger;
        HttpActionExecutedContext context;

        public UseJsonAttributeTests()
        {
            logger = new LoggingService(Mock.Of<ITimeProvider>());

            var scope = new Mock<IDependencyScope>();
            scope.Setup(c => c.GetService(typeof(ILoggingService)))
               .Returns(logger);

            var resolver = new Mock<IDependencyResolver>();
            resolver.Setup(c => c.BeginScope())
                .Returns(scope.Object);

            HttpConfiguration config = new HttpConfiguration
            {
                DependencyResolver = resolver.Object
            };

            var controllerContext = new HttpControllerContext(config, Mock.Of<IHttpRouteData>(), new HttpRequestMessage());
            controllerContext.Request.Properties[HttpPropertyKeys.HttpConfigurationKey] = config;

            var actionContext = new HttpActionContext(
               controllerContext,
                Mock.Of<HttpActionDescriptor>()
            );

            context = new HttpActionExecutedContext
            {
                ActionContext = actionContext
            };
        }

        [FactWithName]
        public void SetJsonResponse()
        {
            var attr = new UseJsonAttribute();
            var mediatype = new Mock<MediaTypeFormatter>() { CallBase = true };
            mediatype.Setup(c => c.CanWriteType(typeof(ViewModel1))).Returns(true);

            context.Response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest,
                RequestMessage = context.Request,
                Content = new ObjectContent<ViewModel1>(new ViewModel1 { Prop1 = "abc" }, mediatype.Object)
            };

            attr.OnActionExecuted(context);

            Assert.Equal(HttpStatusCode.OK, context.Response.StatusCode);
            Assert.Equal(@"{""Prop1"":""abc""}", context.Response.Content.ReadAsStringAsync().Result);
            Assert.IsType<JsonMediaTypeFormatter>((context.Response.Content as ObjectContent).Formatter);
        }

        [FactWithName]
        public void SetJsonResponseIsOverriddenWithException()
        {
            var attr = new UseJsonAttribute();
            var mediatype = new Mock<MediaTypeFormatter>() { CallBase = true };
            mediatype.Setup(c => c.CanWriteType(typeof(ViewModel1))).Returns(true);

            context.Response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest,
                RequestMessage = context.Request,
                Content = new ObjectContent<ViewModel1>(new ViewModel1 { Prop1 = "abc" }, mediatype.Object)
            };

            attr.OnActionExecuted(context);

            Assert.Equal(HttpStatusCode.OK, context.Response.StatusCode);
            Assert.Equal(@"{""Prop1"":""abc""}", context.Response.Content.ReadAsStringAsync().Result);
            Assert.IsType< JsonMediaTypeFormatter>((context.Response.Content as ObjectContent).Formatter);
        }

        [FactWithName]
        public void SkipFormtingIfNoResponse()
        {
            var attr = new UseJsonAttribute();
            attr.OnActionExecuted(context);
            Assert.Null(context.Response);
        }


        [FactWithName]
        public void SkipExceptionIfNoLogger()
        {
            var attr = new UseJsonAttribute();
            var exception = new Mock<BaseCaughtException>();
            exception.Setup(c => c.Message).Returns("exMessage");
            exception.Setup(c => c.StackTrace).Returns("track123");
            exception.Setup(c => c.ResponseCode).Returns(HttpStatusCode.InternalServerError);
            context.Exception = exception.Object;

            var resolver = new Mock<IDependencyResolver>();
            resolver.Setup(c => c.BeginScope())
                .Returns(Mock.Of<IDependencyScope>());

            HttpConfiguration config = new HttpConfiguration
            {
                DependencyResolver = resolver.Object
            };

            var controllerContext = new HttpControllerContext(config, Mock.Of<IHttpRouteData>(), new HttpRequestMessage());
            controllerContext.Request.Properties[HttpPropertyKeys.HttpConfigurationKey] = config;

            var actionContext = new HttpActionContext(
               controllerContext,
                Mock.Of<HttpActionDescriptor>()
            );

            context.ActionContext = actionContext;

            attr.OnActionExecuted(context);
            Assert.Null(context.Response);
        }

        [FactWithName]
        public void WriteBaseCaughtExceptionToResponse()
        {
            var attr = new UseJsonAttribute();
            var exception = new Mock<BaseCaughtException>();
            exception.Setup(c => c.Message).Returns("exMessage");
            exception.Setup(c => c.StackTrace).Returns("track123");
            exception.Setup(c => c.ResponseCode).Returns(HttpStatusCode.InternalServerError);
            context.Exception = exception.Object;

            attr.OnActionExecuted(context);
#if DEBUG
            Assert.Equal("Exception: exMessage\nInner Exception: \nStack Trace: track123", context.Response.Content.ReadAsStringAsync().Result);
#else
            Assert.Equal("Exception: exMessage", context.Response.Content.ReadAsStringAsync().Result);
#endif
        }

        [FactWithName]
        public void BaseCaughtSetsLoggerErrorCodeAndMessage()
        {
            var attr = new UseJsonAttribute();
            var exception = new Mock<BaseCaughtException>();
            exception.Setup(c => c.Message).Returns("exMessage");
            exception.Setup(c => c.StackTrace).Returns("track123");
            exception.Setup(c => c.ResponseCode).Returns(HttpStatusCode.ProxyAuthenticationRequired);
            exception.Setup(c => c.HttpErrorMessage).Returns("test");
            context.Exception = exception.Object;

            attr.OnActionExecuted(context);
            Assert.Equal("Exception: exMessage\nInner Exception: \nStack Trace: track123", logger.Exception);
            Assert.Equal("407 : test", logger.StatusCode);
        }

        [FactWithName]
        public void LogException_InnerException_AndStackTrace()
        {
            var attr = new UseJsonAttribute();
            var innerException = new Exception("innerMessage");
            var exception = new Mock<Exception>("outerMessage", innerException);
            exception.Setup(c => c.Message).Returns("outerMessage");
            exception.Setup(c => c.StackTrace).Returns("track123");

            context.Exception = exception.Object;

            attr.OnActionExecuted(context);
            Assert.Equal("Exception: outerMessage\nInner Exception: innerMessage\nStack Trace: track123", logger.Exception);
        }
    }
}
