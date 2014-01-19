using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing.Design;
using System.ComponentModel;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.Reflection;
using System.Globalization;
using Cornerstone.Database.Tables;
using Cornerstone.Database;

namespace Cornerstone.GUI.DesignMode {
    internal class DatabaseTableTypeConverter : TypeConverter {

        private List<Type> validTypeList = new List<Type>();
        private List<Assembly> loadedAssemblies = new List<Assembly>();

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
            try {
                return new StandardValuesCollection(GetAllTables());
            }
            catch (Exception e) {
                MessageBox.Show(e.GetType().Name + ": " + e.Message + "\n" + e.StackTrace);
                return new StandardValuesCollection(new List<Type>());
            }
        }
        

        // Overrides the ConvertFrom method of TypeConverter.
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) {
            if (value is string) {
                List<Type> typeList = GetAllTables();
                foreach (Type currType in typeList) 
                    if (currType.Name.Equals((string)value)) 
                        return currType;
            }

            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) {
            if (destinationType == typeof(string)) 
                return ((Type)value).Name;

            return base.ConvertTo(context, culture, value, destinationType);
        }

        public List<Type> GetAllTables() {
            // loop through all types in all loaded assemblies and see which have
            // been tagged by our custom database attribute
            Assembly[] assemblyList = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly currAssembly in assemblyList) {
                if (!currAssembly.GlobalAssemblyCache)
                    if (!loadedAssemblies.Contains(currAssembly)) {
                        try {
                            Type[] typeList = currAssembly.GetTypes();
                            foreach (Type currType in typeList) {
                                object[] customAttrArray = currType.GetCustomAttributes(true);
                                foreach (object currAttr in customAttrArray)
                                    if (currAttr.GetType() == typeof(DBTableAttribute))
                                        validTypeList.Add(currType);
                            }
                        }
                        catch (Exception) {}
                    }

                loadedAssemblies.Add(currAssembly);
            }

            return validTypeList;
        }

    }
}
