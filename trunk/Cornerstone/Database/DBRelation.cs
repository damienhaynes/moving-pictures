using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Collections.ObjectModel;
using System.Collections;
using Cornerstone.Database.Tables;
using Cornerstone.Database.CustomTypes;

namespace Cornerstone.Database {
    public class DBRelation {
        #region Private

        private static Dictionary<Type, List<DBRelation>> relations;
        private MethodInfo getRelationListMethod;
        private PropertyInfo _propertyInfo;

        #endregion

        #region Properties

        /// <summary>
        /// The primary DatabaseTable type that hosts this relation. This is the
        /// one in the one-to-many relationship.
        /// </summary>
        public Type PrimaryType {
            get { return _primaryType; }
        } private Type _primaryType;

        /// <summary>
        /// The secondary DatabaseTable type that links to the primary type. This
        /// is the many in the one-to-many relationship.
        /// </summary>
        public Type SecondaryType {
            get { return _secondaryType; }
        } private Type _secondaryType;

        /// <summary>
        /// The optional unique identifier for this relationship. Without a unique ID,
        /// relations gcan go both ways if defined from each type.
        /// </summary>
        public string Identifier {
            get { return _identifier; }
        } private string _identifier;

        /// <summary>
        /// If true, this relation will automatically be retrieved when the owner is retrieved.
        /// </summary>
        public bool AutoRetrieve {
            get { return _autoRetrieve; }
        } private bool _autoRetrieve;

        public bool Filterable {
            get { return _filterable; }
        } private bool _filterable;

        /// <summary>
        /// The name of the table this relationship data is stored in.
        /// </summary>
        public string TableName {
            get {
                if (_tableName == null) {
                    List<string> names = new List<string>();
                    names.Add(DatabaseManager.GetTableName(PrimaryType));
                    names.Add(DatabaseManager.GetTableName(SecondaryType));
                    names.Sort();

                    _tableName = names[0] + "__" + names[1];

                    if (Identifier != null && Identifier.Trim().Length > 0) {
                        _tableName += "__" + Identifier;
                    }
                }

                return _tableName;
            }
        } private string _tableName = null;

        #endregion

        #region Public Methods

        public IRelationList GetRelationList(DatabaseTable dbObject) {
            return (IRelationList)getRelationListMethod.Invoke(dbObject, null);
        }

        #endregion

        #region Public Static Methods

        public static ReadOnlyCollection<DBRelation> GetRelations(Type primaryType) {
            loadRelations(primaryType);
            return relations[primaryType].AsReadOnly();
        }

        public static DBRelation GetRelation(Type primaryType, Type secondaryType, string identifier) {
            loadRelations(primaryType);
            foreach (DBRelation currRelation in relations[primaryType]) {
                if (currRelation.SecondaryType == secondaryType && currRelation.Identifier.Equals(identifier))
                    return currRelation;
            }

            return null;
        }

        #endregion

        #region Private Methods

        static DBRelation() {
            relations = new Dictionary<Type, List<DBRelation>>();
        }

        private DBRelation() {
        }

        private static void loadRelations(Type primaryType) {
            if (relations.ContainsKey(primaryType))
                return;

            List<DBRelation> newRelations = new List<DBRelation>();

            foreach (PropertyInfo currProperty in primaryType.GetProperties())
                foreach (object currAttr in currProperty.GetCustomAttributes(true))
                    // if we have come to a relation property, lets process it
                    if (currAttr.GetType() == typeof(DBRelationAttribute)) {
                        DBRelation newRelation = new DBRelation();
                        newRelation._primaryType = primaryType;
                        newRelation._secondaryType = currProperty.PropertyType.GetGenericArguments()[1];
                        newRelation._identifier = ((DBRelationAttribute)currAttr).Identifier;
                        newRelation._propertyInfo = currProperty;
                        newRelation._autoRetrieve = ((DBRelationAttribute)currAttr).AutoRetrieve;
                        newRelation._filterable = ((DBRelationAttribute)currAttr).Filterable; 
                        newRelation.getRelationListMethod = currProperty.GetGetMethod();

                        newRelations.Add(newRelation);
                    }

            relations[primaryType] = newRelations;
        }
        
        #endregion

    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class DBRelationAttribute : System.Attribute {
        // DBRelations work two way. If you multiple or single directional relations are needed 
        // a unique identifier should be supplied. 
        public string Identifier {
            get { return _identifier; }
            set { _identifier = value; }
        } string _identifier = "";

        public bool AutoRetrieve {
            get { return _autoRetrieve; }
            set { _autoRetrieve = value; }
        } private bool _autoRetrieve = false;

        public bool Filterable {
            get { return _filterable; }
            set { _filterable = value; }
        } private bool _filterable = true;
    }
}
