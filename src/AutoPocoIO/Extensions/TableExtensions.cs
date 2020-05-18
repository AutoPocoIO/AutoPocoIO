using AutoPocoIO.DynamicSchema.Db;
using System.ComponentModel;

namespace AutoPocoIO.Extensions
{
    internal static class TableExtensions
    {
        public static PKInfo[] GetTableKeys(this PKInfo[] PKs, string keys)
        {
            var arrKeys = keys.Split(';');

            for (int i = 0; i < arrKeys.Length; i++)
            {
                var PK = PKs[i];
                var keyValue = arrKeys[i];

                var converter = TypeDescriptor.GetConverter(PK.Type);
                PK.Value = converter.ConvertFrom(keyValue);
            }

            return PKs;
        }
    }
}