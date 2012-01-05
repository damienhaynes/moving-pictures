using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cornerstone.Tools.Search {
    public class SearchException: Exception {
        public enum ErrorTypeEnum { MISSING_FIELDS, INVALID_FIELDS, MISSING_DATABASE }

        public override string Message {
            get { return _message; }
        } protected string _message;

        public ErrorTypeEnum ErrorType {
            get;
            protected set;
        }

        public SearchException(ErrorTypeEnum type) {
            ErrorType = type;

            switch (ErrorType) {
                case ErrorTypeEnum.INVALID_FIELDS:
                    _message = "A field that does not belong to the specified DatabaseTable was used as a search field.";          
                    break;
                case ErrorTypeEnum.MISSING_DATABASE:
                    _message = "A database manager must be specified to use a Searcher.";
                    break;
                case ErrorTypeEnum.MISSING_FIELDS:
                    _message = "No fields have been defined to search over.";
                    break;
            }
        }
    }
}
