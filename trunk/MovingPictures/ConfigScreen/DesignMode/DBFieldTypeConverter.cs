using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing.Design;
using System.ComponentModel;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.Reflection;
using MediaPortal.Plugins.MovingPictures.Database;
using System.Globalization;
using MediaPortal.Plugins.MovingPictures.ConfigScreen.Controls;

namespace MediaPortal.Plugins.MovingPictures.ConfigScreen.DesignMode {
    internal class DBFieldTypeConverter : TypeConverter {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) {
            if (sourceType == typeof(string) && context.Instance is IDBBackedControl) {
                return true;
            }
            return base.CanConvertFrom(context, sourceType);
        }

        // Indicates this converter provides a list of standard values.
        public override bool GetStandardValuesSupported(System.ComponentModel.ITypeDescriptorContext context) {
            if (context.Instance is IDBBackedControl)
                return true;
            else
                return false;
        }

        // Returns a StandardValuesCollection of standard value objects.
        public override System.ComponentModel.TypeConverter.StandardValuesCollection GetStandardValues(System.ComponentModel.ITypeDescriptorContext context) {
            return new StandardValuesCollection(getTypeList((IDBBackedControl)context.Instance));
        }

        // Overrides the ConvertFrom method of TypeConverter.
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) {
            if (value is string)
                foreach (string currField in getTypeList((IDBBackedControl)context.Instance))
                    if (currField.Equals((string)value))
                        return value;

            return "";
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) {
            if (destinationType == typeof(string))
                return value;

            return base.ConvertTo(context, culture, value, destinationType);
        }

        private List<string> getTypeList(IDBBackedControl control) {
            List<string> fieldNameList = new List<string>();
            foreach (DBField currField in DBField.GetFieldList(control.Table)) {
                fieldNameList.Add(currField.Name);
            }

            return fieldNameList;
        }

    }
}
