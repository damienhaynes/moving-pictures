using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MovingPicturesSocialAPI.Data {
    public class MpsUser {
        public string Name {
            get;
            internal set;
        }

        public string HashedPassword {
            get;
            internal set;
        }

        public string Email {
            get;
            internal set;
        }

        public string Locale {
            get;
            internal set;
        }

        public bool PrivateProfile {
            get;
            internal set;
        }

        public string PrivateUrl {
            get;
            internal set;
        }

        public bool AdultMoviesVisible {
            get;
            internal set;
        }

        public string ApiUrl {
            get;
            internal set;
        }

        public DateTime LastSeen {
            get;
            internal set;
        }
    }
}
