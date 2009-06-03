using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using Cornerstone.Database.CustomTypes;

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

        public List<T> Filter(List<T> input) {
            if (!_active) return input;

            List<T> results = new List<T>();

            foreach (T currItem in input) {
                object currItemVal = Field.GetValue(currItem);
                switch (Operator) {
                    case OperatorEnum.EQUAL:
                        if (Field.Type == typeof(StringList)) {
                            if (((StringList)currItemVal).Contains(Value.ToString()))
                                results.Add(currItem);
                        }
                        else if (Field.Type == typeof(string)) {
                            if (currItemVal.ToString().ToLower().Equals(Value.ToString().ToLower()))
                                results.Add(currItem);
                        }
                        else {
                            if (currItemVal.Equals(Value))
                                results.Add(currItem);
                        }
                        break;

                    case OperatorEnum.NOT_EQUAL:
                        if (Field.Type == typeof(StringList)) {
                            if (!((StringList)currItemVal).Contains(Value.ToString()))
                                results.Add(currItem);
                        }
                        else if (Field.Type == typeof(string)) {
                            if (!currItemVal.ToString().ToLower().Equals(Value.ToString().ToLower()))
                                results.Add(currItem);
                        }
                        else {
                            if (!currItemVal.Equals(Value))
                                results.Add(currItem);
                        }
                        break;

                    case OperatorEnum.CONTAINS:
                        if (currItemVal.ToString().ToLower().Contains(Value.ToString().ToLower()))
                            results.Add(currItem);
                        break;

                    case OperatorEnum.NOT_CONTAIN:
                        if (!currItemVal.ToString().ToLower().Contains(Value.ToString().ToLower()))
                            results.Add(currItem);
                        break;

                    case OperatorEnum.GREATER_THAN:
                        if (currItemVal is int) {
                            if ((int)currItemVal > (int)Value)
                                results.Add(currItem);
                        }
                        else if (currItemVal is float) {
                            if ((float)currItemVal > (float)Value)
                                results.Add(currItem);
                        }
                        break;

                    case OperatorEnum.LESS_THAN:
                        if (currItemVal is int) {
                            if ((int)currItemVal < (int)Value)
                                results.Add(currItem);
                        } 
                        else if (currItemVal is float) {
                            if ((float)currItemVal < (float)Value)
                                results.Add(currItem);
                        }
                        break;

                    case OperatorEnum.BEGINS_WITH:
                        if (Field.Type == typeof(StringList)) {
                            foreach (string currStr in (StringList)currItemVal) {
                                if (currStr.ToLower().StartsWith(Value.ToString().ToLower())) {
                                    results.Add(currItem);
                                    break;
                                }
                            }
                        }
                        else {
                            if (currItemVal.ToString().ToLower().StartsWith(Value.ToString().ToLower()))
                                results.Add(currItem);
                        }
                        break;

                    case OperatorEnum.NOT_BEGIN_WITH:
                        if (Field.Type == typeof(StringList)) {
                            foreach (string currStr in (StringList)currItemVal) {
                                if (!currStr.ToLower().StartsWith(Value.ToString().ToLower())) {
                                    results.Add(currItem);
                                    break;
                                }
                            }
                        }
                        else {
                            if (!currItemVal.ToString().ToLower().StartsWith(Value.ToString().ToLower()))
                                results.Add(currItem);
                        }
                        break;

                    case OperatorEnum.ENDS_WITH:
                        if (Field.Type == typeof(StringList)) {
                            foreach (string currStr in (StringList)currItemVal) {
                                if (currStr.ToLower().EndsWith(Value.ToString().ToLower())) {
                                    results.Add(currItem);
                                    break;
                                }
                            }
                        }
                        else {
                            if (currItemVal.ToString().ToLower().EndsWith(Value.ToString().ToLower()))
                                results.Add(currItem);
                        }
                        break;
                    
                    case OperatorEnum.NOT_ENDS_WITH:
                        if (Field.Type == typeof(StringList)) {
                            foreach (string currStr in (StringList)currItemVal) {
                                if (!currStr.ToLower().EndsWith(Value.ToString().ToLower())) {
                                    results.Add(currItem);
                                    break;
                                }
                            }
                        }
                        else {
                            if (!currItemVal.ToString().ToLower().EndsWith(Value.ToString().ToLower()))
                                results.Add(currItem);
                        }
                        break;
                }
            }

            return results;
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
                if (Field == null || value.GetType() == Field.Type)
                    _value = value;
                else if (Field.Type == typeof(StringList))
                    _value = value.ToString();
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
    }


}
