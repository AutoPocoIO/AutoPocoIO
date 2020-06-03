#pragma warning disable 1591
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace AutoPocoIO.Dashboard.Pages
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using AutoPocoIO.Dashboard.ViewModels;
    using AutoPocoIO.Middleware;
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("RazorGenerator", "2.0.0.0")]
    internal partial class SchemaPage : RazorPage
    {
#line hidden

        public override void Execute()
        {


WriteLiteral("\r\n\r\n");




WriteLiteral("\r\n");


  
    var Model = (SchemaViewModel)ViewBag["model"];


WriteLiteral("<div class=\"container pt-4 mt-5\">\r\n    <h2>\r\n        Data Dictionary - ");


                     Write(Model.ConnectorName);

WriteLiteral(" Connector\r\n    </h2>\r\n    <hr />\r\n    <div>\r\n");


         if (Model.Tables.Count() > 0)
        {

WriteLiteral(@"            <div class=""mt-3"">
                <h6>Tables</h6>
                <table class=""table table-hover selectable mt-3"">
                    <thead>
                        <tr class=""table-secondary"">
                            <th>Table Name</th>
                            <th>Primary Key</th>
                            <th># of Columns</th>
                        </tr>
                    </thead>
                    <tbody>
");


                         foreach (var row in Model.Tables)
                        {

WriteLiteral("                            <tr class=\"clickable-row\" data-url=\"");


                                                           Write(TransformUrl("/DataDictionary/Table/"));


                                                                                                  Write(Model.ConnectorId);

WriteLiteral("/");


                                                                                                                     Write(row.Name);

WriteLiteral("\">\r\n                                <td>");


                               Write(row.Name);

WriteLiteral("</td>\r\n                                <td>");


                               Write(row.PrimaryKeys);

WriteLiteral("</td>\r\n                                <td>");


                               Write(row.Columns.Count());

WriteLiteral("</td>\r\n                            </tr>\r\n");


                        }

WriteLiteral("                    </tbody>\r\n                </table>\r\n            </div>\r\n");


        }


         if (Model.Views.Count() > 0)
        {

WriteLiteral(@"            <div class=""mt-3"">
                <h6>Views</h6>
                <table class=""table table-hover selectable mt-3"">
                    <thead>
                        <tr class=""table-secondary"">
                            <th>View Name</th>
                            <th># of Columns</th>
                        </tr>
                    </thead>
                    <tbody>
");


                         foreach (var row in Model.Views)
                        {

WriteLiteral("                            <tr class=\"clickable-row\" data-url=\"");


                                                           Write(TransformUrl("/DataDictionary/View/"));


                                                                                                 Write(Model.ConnectorId);

WriteLiteral("/");


                                                                                                                    Write(row.Name);

WriteLiteral("\">\r\n                                <td>");


                               Write(row.Name);

WriteLiteral("</td>\r\n                                <td>");


                               Write(row.Columns.Count());

WriteLiteral("</td>\r\n                            </tr>\r\n");


                        }

WriteLiteral("                    </tbody>\r\n                </table>\r\n            </div>\r\n");


        }


         if (Model.StoredProcedures.Count() > 0)
        {

WriteLiteral(@"            <div class=""mt-3"">
                <h6>Stored Procedures</h6>
                <table class=""table table-hover selectable mt-3"">
                    <thead>
                        <tr class=""table-secondary"">
                            <th>Stored Procedure Name</th>
                        </tr>
                    </thead>
                    <tbody>
");


                         foreach (var row in Model.StoredProcedures)
                        {

WriteLiteral("                            <tr class=\"clickable-row\" data-url=\"");


                                                           Write(TransformUrl("/DataDictionary/StoredProcedure/"));


                                                                                                            Write(Model.ConnectorId);

WriteLiteral("/");


                                                                                                                               Write(row.Name);

WriteLiteral("\">\r\n                                <td>");


                               Write(row.Name);

WriteLiteral("</td>\r\n                            </tr>\r\n");


                        }

WriteLiteral("                    </tbody>\r\n                </table>\r\n            </div>\r\n");


        }

WriteLiteral("    </div>\r\n</div>\r\n\r\n");


DefineSection("script", () => {

WriteLiteral("\r\n    <script>\r\n        jQuery(document).ready(function ($) {\r\n            $(\".cl" +
"ickable-row\").click(function () {\r\n                window.location = $(this).dat" +
"a(\"url\");\r\n            });\r\n        });\r\n    </script>\r\n");


});


        }
    }
}
#pragma warning restore 1591
