using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;

namespace AutoPocoIO.CustomAttributes
{
    public class UseJsonAttribute : ProducesAttribute
    {
        public UseJsonAttribute() : base("application/json", MediaTypeNames.Text.Plain)
        {
        }
    }
}
