using System;

namespace AutoPocoIO.SwaggerAddons
{
    internal class SwaggerExampleType
    {
        public Guid Id;
        public string Column1;
        public int Column2;
    }

    internal class SwaggerTableDefType
    {
        public string Name;
        public string Type;
        public string Length;
        public bool IsNullable;
    }

    internal class SwaggerColumnDefType : SwaggerTableDefType
    {
        public bool IsPK;
        public bool IsFK;
    }
}