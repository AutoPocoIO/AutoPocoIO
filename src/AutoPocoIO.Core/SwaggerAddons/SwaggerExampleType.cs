using System;

namespace AutoPocoIO.SwaggerAddons
{
    public class SwaggerExampleType
    {
        public Guid Id;
        public string Column1;
        public int Column2;
    }

    public class SwaggerTableDefType
    {
        public string Name;
        public string Type;
        public string Length;
        public bool IsNullable;
    }

    public class SwaggerColumnDefType : SwaggerTableDefType
    {
        public bool IsPK;
        public bool IsFK;
    }
}