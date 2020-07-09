using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;

namespace AutoPocoIO.CustomAttributes
{
    /// <summary>
    /// End point produces json or plain text
    /// </summary>
    public class UseJsonAttribute : ProducesAttribute
    {
        /// <summary>
        /// Initialize produce attribute with json and plain text content types
        /// </summary>
        public UseJsonAttribute() : base("application/json", MediaTypeNames.Text.Plain)
        {
        }
    }
}
