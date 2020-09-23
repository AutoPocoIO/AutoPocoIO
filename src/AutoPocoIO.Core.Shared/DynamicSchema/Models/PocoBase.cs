using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace AutoPocoIO.DynamicSchema.Models
{
    /// <summary>
    /// Base type for all dynamically generated database models
    /// </summary>
    public abstract class PocoBase : INotifyPropertyChanged

    {
        /// <summary>
        /// Cleanly show table name
        /// </summary>
        /// <returns> A string that represents the current object.</returns>
        public override string ToString()
        {
            List<PropertyInfo> propertyInfo = this.GetType()
                    .GetProperties()
                    .Where(o => (o.PropertyType != typeof(byte[]) && (!o.PropertyType.IsClass || o.PropertyType == typeof(string))))
                    .ToList();

            if (propertyInfo.Count < 1)
            {
                return base.ToString();
            }
            else
            {

                string[] s = propertyInfo.ConvertAll(o => string.Format(CultureInfo.InvariantCulture, "{0}={1}", o.Name, o.GetValue(this, null)?.ToString() ?? "null")).ToArray();
                return string.Join(",", s);

            }
        }

        /// <summary>
        /// Event delegate to show changers
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raise the on change event
        /// </summary>
        /// <param name="sender">Dbcontext</param>
        /// <param name="PropertyName">Column name</param>
        public void OnRaisePropertyChanged(object sender, string PropertyName)
        {
            PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(PropertyName));
        }
    }
}
