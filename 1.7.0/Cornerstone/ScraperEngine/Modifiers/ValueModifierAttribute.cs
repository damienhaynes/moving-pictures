using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cornerstone.ScraperEngine.Modifiers
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ValueModifierAttribute : Attribute
    {
        private string _name;

        public string Name
        {
            get { return _name; }
        }

        public bool LoadNameAttribute
        {
            get { return loadNameAttribute; }
            set { loadNameAttribute = value; }
        } private bool loadNameAttribute = true;

        public ValueModifierAttribute(string name)
        {
            this._name = name;
        }
    }
}
