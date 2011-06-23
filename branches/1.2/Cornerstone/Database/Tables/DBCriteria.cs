using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using Cornerstone.Database.CustomTypes;
using Cornerstone.Extensions;

namespace Cornerstone.Database.Tables {
    [DBTableAttribute("criteria")]
    public class DBCriteria<T> : GenericDatabaseTable<T>, IFilter<T>, IGenericFilter
        where T : DatabaseTable {

        public enum OperatorEnum {
            // general operators
            [Description("equals")]
            EQUAL,
            
            [Description("does not equal")]
            NOT_EQUAL,


            // numeric operators
            [Description("is less than")]
            LESS_THAN,
            
            [Description("is greater than")]
            GREATER_THAN,


            // string operators
            [Description("contains")]
            CONTAINS,

            [Description("does not contain")]
            NOT_CONTAIN,

            [Description("begins with")]
            BEGINS_WITH,
            
            [Description("does not begin with")]
            NOT_BEGIN_WITH,

            [Description("ends with")]
            ENDS_WITH,

            [Description("does not end with")]
            NOT_ENDS_WITH
        }
        
        #region IFilter<T> Members

        public event FilterUpdatedDelegate<T> Updated;

        public HashSet<T> Filter(ICollection<T> input) {
            return Filter(input, false);
        }

        public HashSet<T> Filter(ICollection<T> input, bool forceActive) {
            bool active = forceActive || _active;
            HashSet<T> results = new HashSet<T>();

            // if we are not active, just return the inputs.
            if (!active) {
                if (input is HashSet<T>)
                    return (HashSet<T>)input;

                foreach (T currItem in input)
                    results.Add(currItem);
                return results;
            }
            
            foreach (T currItem in input) {
                if (Relation == null) {
                    if (isIncluded(Field.GetValue(currItem)))
                        results.Add(currItem);
                }
                else {
                    foreach (DatabaseTable currSubItem in Relation.GetRelationList(currItem))
                        if (isIncluded(Field.GetValue(currSubItem))) {
                            results.Add(currItem);
                            break;
                        }
                }
            }

            return results;
        }

        private bool isIncluded(object value) {
            switch (Operator) {
                case OperatorEnum.EQUAL:
                    if (value == null) {
                        if (Value == null || string.IsNullOrEmpty(Value.ToString()))
                            return true;

                        return false;
                    }
                    else if (Value == null) {
                        if (value == null || string.IsNullOrEmpty(value.ToString()))
                            return true;

                        return false;
                    }
                    else if (Field.Type == typeof(StringList)) {
                        if (((StringList)value).Contains(Value.ToString()))
                            return true;
                    }
                    else if (Field.Type == typeof(string)) {
                        if (value.ToString().ToLower().Equals(Value.ToString().ToLower()))
                            return true;
                    }
                    else if (Field.Type == typeof(DateTime)) {
                        if (((DateTime)value).Date.Equals(DoDateTimeConversion(Value)))
                            return true;
                    }
                    else {
                        if (value == null && Value != null)
                            return false;
                        
                        if (value == null && Value == null)
                            return true;

                        if (value != null && Value == null)
                            return false;

                        if (value.Equals(Value))
                            return true;
                    }
                    break;

                case OperatorEnum.NOT_EQUAL:
                    if (Field.Type == typeof(StringList)) {
                        if (!((StringList)value).Contains(Value.ToString()))
                            return true;
                    }
                    else if (Field.Type == typeof(string)) {
                        if (!value.ToString().ToLower().Equals(Value.ToString().ToLower()))
                            return true;
                    }
                    else if (Field.Type == typeof(DateTime)) {
                        if (!((DateTime)value).Date.Equals(DoDateTimeConversion(Value)))
                            return true;
                    }
                    else {
                        if (!value.Equals(Value))
                            return true;
                    }
                    break;

                case OperatorEnum.CONTAINS:
                    if (value.ToString().ToLower().Contains(Value.ToString().ToLower()))
                        return true;
                    break;

                case OperatorEnum.NOT_CONTAIN:
                    if (!value.ToString().ToLower().Contains(Value.ToString().ToLower()))
                        return true;
                    break;

                case OperatorEnum.GREATER_THAN:
                    if (value is int) {
                        if ((int)value > (int)Value)
                            return true;
                    }
                    else if (value is float) {
                        if ((float)value > (float)Value)
                            return true;
                    }
                    else if (Field.Type == typeof(DateTime)) {
                        if (((DateTime)value).Date > DoDateTimeConversion(Value))
                            return true;
                    }
                    break;

                case OperatorEnum.LESS_THAN:
                    if (value is int) {
                        if ((int)value < (int)Value)
                            return true;
                    }
                    else if (value is float) {
                        if ((float)value < (float)Value)
                            return true;
                    }
                    else if (Field.Type == typeof(DateTime)) {
                        if (((DateTime)value).Date < DoDateTimeConversion(Value))
                            return true;
                    }
                    break;
                case OperatorEnum.BEGINS_WITH:
                    if (Field.Type == typeof(StringList)) {
                        foreach (string currStr in (StringList)value) {
                            if (currStr.ToLower().StartsWith(Value.ToString().ToLower())) {
                                return true;
                            }
                        }
                    }
                    else {
                        if (value.ToString().ToLower().StartsWith(Value.ToString().ToLower()))
                            return true;
                    }
                    break;

                case OperatorEnum.NOT_BEGIN_WITH:
                    if (Field.Type == typeof(StringList)) {
                        foreach (string currStr in (StringList)value) {
                            if (!currStr.ToLower().StartsWith(Value.ToString().ToLower())) {
                                return true;
                            }
                        }
                    }
                    else {
                        if (!value.ToString().ToLower().StartsWith(Value.ToString().ToLower()))
                            return true;
                    }
                    break;

                case OperatorEnum.ENDS_WITH:
                    if (Field.Type == typeof(StringList)) {
                        foreach (string currStr in (StringList)value) {
                            if (currStr.ToLower().EndsWith(Value.ToString().ToLower())) {
                                return true;
                            }
                        }
                    }
                    else {
                        if (value.ToString().ToLower().EndsWith(Value.ToString().ToLower()))
                            return true;
                    }
                    break;

                case OperatorEnum.NOT_ENDS_WITH:
                    if (Field.Type == typeof(StringList)) {
                        foreach (string currStr in (StringList)value) {
                            if (!currStr.ToLower().EndsWith(Value.ToString().ToLower())) {
                                return true;
                            }
                        }
                    }
                    else {
                        if (!value.ToString().ToLower().EndsWith(Value.ToString().ToLower()))
                            return true;
                    }
                    break;
            }
            return false;
        }

        public bool Active {
            get { return _active; }
            set {
                if (_active != value) {
                    _active = value;

                    if (Updated != null)
                        Updated(this);
                }
            }
        }
        private bool _active = true;

        #endregion

        #region Database Fields

        [DBField]
        public DBField Field {
            get { return _field; }
            
            set {
                _field = value;
                commitNeeded = true;
            }
        } private DBField _field = null;

        [DBField]
        public DBRelation Relation {
            get { return _subTableRelationship; }

            set {
                _subTableRelationship = value;
                commitNeeded = true;
            }
        } private DBRelation _subTableRelationship;

        [DBField]
        public OperatorEnum Operator {
            get { return _operator;  }
            
            set {
                _operator = value;
                commitNeeded = true;
            }
        } private OperatorEnum _operator;

        [DBField]
        public object Value {
            get { 
                return _value;
            }

            set {
                if (value == null && (Field == null || Field.IsNullable))
                    _value = value;
                else if (Field == null || value.GetType() == Field.Type)
                    _value = value;
                else if (Field.Type == typeof(StringList))
                    _value = value.ToString();
                else if (Field.Type == typeof(DateTime)) {
                    DateTime newValue = DateTime.Now;
                    if (DateTime.TryParse((string)value, out newValue))
                        _value = newValue;
                    else
                        _value = value.ToString();
                }
                else if (value is string)
                    _value = Field.ConvertString(this.DBManager, (string)value);
                else
                    _value = null;

                commitNeeded = true;
            }
        } private object _value;

        #endregion

        public List<OperatorEnum> GetOperators() {
            return GetOperators(_field);
        }

        public List<OperatorEnum> GetOperators(DBField field) {
            List<OperatorEnum> rtn = new List<OperatorEnum>();
            
            if (!field.AllowManualFilterInput) {
                rtn.Add(DBCriteria<T>.OperatorEnum.EQUAL);
                rtn.Add(DBCriteria<T>.OperatorEnum.NOT_EQUAL);
                return rtn;
            }

            switch (field.DBType) {
                case DBField.DBDataType.ENUM:
                case DBField.DBDataType.BOOL:
                    rtn.Add(DBCriteria<T>.OperatorEnum.EQUAL);
                    rtn.Add(DBCriteria<T>.OperatorEnum.NOT_EQUAL);
                    break;

                case DBField.DBDataType.DATE_TIME:
                case DBField.DBDataType.INTEGER:
                case DBField.DBDataType.REAL:
                    rtn.Add(DBCriteria<T>.OperatorEnum.EQUAL);
                    rtn.Add(DBCriteria<T>.OperatorEnum.NOT_EQUAL);
                    rtn.Add(DBCriteria<T>.OperatorEnum.LESS_THAN);
                    rtn.Add(DBCriteria<T>.OperatorEnum.GREATER_THAN);
                    break;

                case DBField.DBDataType.TEXT:
                    rtn.Add(DBCriteria<T>.OperatorEnum.EQUAL);
                    rtn.Add(DBCriteria<T>.OperatorEnum.NOT_EQUAL);
                    rtn.Add(DBCriteria<T>.OperatorEnum.CONTAINS);
                    rtn.Add(DBCriteria<T>.OperatorEnum.NOT_CONTAIN);
                    rtn.Add(DBCriteria<T>.OperatorEnum.BEGINS_WITH);
                    rtn.Add(DBCriteria<T>.OperatorEnum.NOT_BEGIN_WITH);
                    rtn.Add(DBCriteria<T>.OperatorEnum.ENDS_WITH);
                    rtn.Add(DBCriteria<T>.OperatorEnum.NOT_ENDS_WITH);
                    break;

                case DBField.DBDataType.STRING_OBJECT:
                    if (field.Type == typeof(StringList)) {
                        rtn.Add(DBCriteria<T>.OperatorEnum.EQUAL);
                        rtn.Add(DBCriteria<T>.OperatorEnum.NOT_EQUAL);
                        rtn.Add(DBCriteria<T>.OperatorEnum.CONTAINS);
                        rtn.Add(DBCriteria<T>.OperatorEnum.NOT_CONTAIN);
                        rtn.Add(DBCriteria<T>.OperatorEnum.BEGINS_WITH);
                        rtn.Add(DBCriteria<T>.OperatorEnum.NOT_BEGIN_WITH);
                        rtn.Add(DBCriteria<T>.OperatorEnum.ENDS_WITH);
                        rtn.Add(DBCriteria<T>.OperatorEnum.NOT_ENDS_WITH);
                    }
                    break;
                case DBField.DBDataType.TYPE:
                case DBField.DBDataType.DB_FIELD:
                case DBField.DBDataType.DB_OBJECT:
                    break;
            }

            return rtn;
        }

        /// <summary>
        /// Returns a calculated date value
        /// </summary>
        /// <param name="date">actual date or relative format string</param>
        /// <returns>parsed date as given or calculated relative date</returns>
        private DateTime DoDateTimeConversion(object date) {
            DateTime newDate = DateTime.MinValue;
            string part = null;
            int diff = 0;

            string value = date.ToString();            
            
            // try to parse a date from the given object
            if (!DateTime.TryParse(date.ToString(), out newDate)) {
                // if parsing fails we have a relative string format
                newDate = DateTime.Today;
                int length = value.Length-1;
                part = value[length].ToString();
                
                if (length > 0) {
                    int.TryParse(value.Substring(0, length), out diff);
                }
            }
            
            // based on the given datepart in the relative string we get our calculated date
            // if no part is given we just return the newDate value
            switch (part) {
                // --- Days
                case "d": // within day (ago) or today
                    return newDate.AddDays(diff);
                // --- Weeks
                case "w": 
                case "W":
                    if (part == "w" && diff == 0 || part == "W") // start of (this/diff) week (by week number)
                        return newDate.GetStartOfWeek().AddDays(diff * 7);
                    else // within a week span (ago) (7 days)
                        return newDate.AddDays(diff * 7);                        
                // --- Months
                case "m": 
                case "M": 
                    if (part == "m" && diff == 0 || part == "M") // start of (this/diff) month
                        return newDate.GetStartOfMonth().AddMonths(diff);
                    else // within a month span (ago)
                        return newDate.AddMonths(diff);
                // --- Years
                case "y":
                case "Y": 
                    if (part == "y" && diff == 0 || part == "Y") // start of (this/diff) month
                        return newDate.GetStartOfYear().AddYears(diff);
                    else // within a year span (ago)
                        return newDate.AddYears(diff);                    
                // --- Today or parsed date value
                default:
                    return newDate;
            }
        }

    }


}
