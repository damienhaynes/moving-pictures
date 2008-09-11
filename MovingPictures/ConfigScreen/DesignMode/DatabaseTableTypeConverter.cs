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
using Cornerstone.Database.Tables;

namespace MediaPortal.Plugins.MovingPictures.ConfigScreen.DesignMode {
    internal class DatabaseTableTypeConverter : TypeConverter {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) {
            if (sourceType == typeof(string)) {
                return true;
            }
            return base.CanConvertFrom(context, sourceType);
        }

        
        // Indicates this converter provides a list of standard values.
        public override bool GetStandardValuesSupported(System.ComponentModel.ITypeDescriptorContext context) {
            return true;
        }

        // Returns a StandardValuesCollection of standard value objects.
        public override System.ComponentModel.TypeConverter.StandardValuesCollection GetStandardValues(System.ComponentModel.ITypeDescriptorContext context) {
            return new StandardValuesCollection(getTypeList());
        }
        

        // Overrides the ConvertFrom method of TypeConverter.
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) {
            if (value is string) 
                foreach (Type currType in getTypeList()) 
                    if (currType.Name.Equals((string)value))
                        return currType;
            
            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) {
            if (destinationType == typeof(string)) 
                return ((Type)value).Name;
            
            return base.ConvertTo(context, culture, value, destinationType);
        }

        private List<Type> getTypeList() {
            List<Type> validTypeList = new List<Type>();

            // loop through all types in the assembly and see which have been tagged by our 
            // custom database attribute
            Type[] typeList = Assembly.GetExecutingAssembly().GetTypes();
            foreach (Type currType in typeList) {
                object[] customAttrArray = currType.GetCustomAttributes(true);
                foreach (object currAttr in customAttrArray)
                    if (currAttr.GetType() == typeof(DBTableAttribute)) 
                        validTypeList.Add(currType);
                    
            }

            return validTypeList;
        }
        
    }
}
