using System;

namespace AutoPocoIO.SwaggerAddons
{
    public class SwaggerExampleType
    {
        public Guid Id { get; set; }
        public string Column1 { get; set; }
        public int Column2 { get; set; }
    }

    public class SwaggerTableDefType
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Length { get; set; }
        public bool IsNullable { get; set; }
    }

    public class SwaggerColumnDefType : SwaggerTableDefType
    {
        public bool IsPK { get; set; }
        public bool IsFK { get; set; }
    }
}