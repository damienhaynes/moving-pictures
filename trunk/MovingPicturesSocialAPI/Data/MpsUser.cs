using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MovingPicturesSocialAPI.Data {
    public class MpsUser {
        public string Name {
            get;
            set;
        }

        public string HashedPassword {
            get;
            internal set;
        }

        public string Email {
            get;
            set;
        }

        public string Locale {
            get;
            set;
        }

        public bool PrivateProfile {
            get;
            set;
        }

        public string ApiUrl {
            get;
            internal set;
        }
    }
}
