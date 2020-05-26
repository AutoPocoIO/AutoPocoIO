using AutoPocoIO.Constants;
using AutoPocoIO.DynamicSchema.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AutoPocoIO.DynamicSchema.Util
{
    internal static class Utils
    {
        public static string AssemblyName(Table table, string parentTableName, int requestHashCode)
        {
            var suffixHash = table.GetHashCode();
            return ($"dynamicassembly.{table.VariableName}{suffixHash}.{parentTableName}{requestHashCode}").ToUpperInvariant();
        }

        public static Type FindLoadedAssembly(this IEnumerable<Assembly> assemblyList, bool loadDuplicate = false)
        {
            Type loadedType = null;
            bool foundNonLoadedAssemblies = false;

            foreach (var assembly in assemblyList)
            {
                try
                {
                    //Check if types are loaded in assembly
                    _ = assembly.GetTypes();
                    loadedType = assembly.GetTypes().First();
                }
                catch (Exception ex) when
                    (ex is InvalidOperationException ||
                     ex is ArgumentNullException ||
                     ex is ReflectionTypeLoadException)
                {
                    foundNonLoadedAssemblies = true;
                }
            }

            if (loadDuplicate && foundNonLoadedAssemblies)
                throw new InvalidOperationException(ExceptionMessages.AssemblyFoundWithNoTypes);

            return loadedType;
        }

        public static string GetFancyLabel(string aLabel)
        {
            string ret = aLabel;
            System.Globalization.TextInfo ti = new System.Globalization.CultureInfo("en-US", false).TextInfo;
            if (ret.IndexOf(" ", StringComparison.InvariantCultureIgnoreCase) >= 0)
            {
                ret = ti.ToTitleCase(ret);
            }
            else
            {
                ret = ret.Replace("_", " ");
                string finalString = "";
                for (int i = 0; i <= ret.Length - 1; i++)
                {
                    if (Char.IsUpper(ret[i]))
                    {
                        finalString = finalString + " " + ret[i];
                    }
                    else
                    {
                        finalString += ret[i];
                    }
                }
                finalString = finalString.Trim();
                ret = "";
                for (int i = 0; i <= finalString.Length - 1; i++)
                {
                    if (finalString[i] == ' ')
                    {
                        if (i == finalString.Length - 2)
                        {
                            if (Char.IsUpper(finalString[i - 1]) && Char.IsUpper(finalString[i + 1]))
                            {
                                ret += "_";
                            }
                            else
                            {
                                ret += finalString[i];
                            }
                        }
                        else
                        {
                            if (i < finalString.Length - 2)
                            {
                                if (Char.IsUpper(finalString[i - 1]) && Char.IsUpper(finalString[i + 1]) && (finalString[i + 2] == ' '))
                                {
                                    ret += "_";
                                }
                                else
                                {
                                    ret += finalString[i];
                                }
                            }
                        }
                    }
                    else
                    {
                        ret += finalString[i];
                    }
                }
                finalString = ret;
                finalString = finalString.Replace("_", "");
                ret = finalString;
                ret = ti.ToTitleCase(ret);
            }
            return ret;
        }

        public static string ColumnToPropertyName(this Column column)
        {
            string propertyName = column.ColumnName.Replace(".", "_");
            if (Char.IsDigit(propertyName[0]))
                propertyName = "C" + propertyName;

            return propertyName;
        }
    }
}