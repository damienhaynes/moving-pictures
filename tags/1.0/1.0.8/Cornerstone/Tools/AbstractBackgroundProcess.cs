using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Cornerstone.Tools {

    public abstract class AbstractBackgroundProcess {

        /// <summary>
        /// Triggered when progress is made by the process. Should pass a double between 0.0 and 1.0
        /// indicating progress to completion.
        /// </summary>
        public event ProcessProgressDelegate Progress;
        protected virtual void OnProgress(double progress) {
            if (Progress != null)
                Progress(this, progress);
        }


        /// <summary>
        /// Triggered when the process has run to completion or has failed.
        /// </summary>
        public event ProcessStatusChangedDelegate StatusChanged;
        protected virtual void OnStatusChanged() {
            if (StatusChanged != null)
                StatusChanged(this, _status);
        }


        public abstract string Name {
            get;
        }

        public abstract string Description {
             get;
        }

        public virtual ProcessStatus Status {
            get { return _status; }
            
            protected set {
                _status = value;
                OnStatusChanged();
            }
        }
        protected ProcessStatus _status = ProcessStatus.INITIALIZED;

        public void WorkRunner() {
            try {
                Status = ProcessStatus.RUNNING;
                
                Work();
                
                if (Status ==  ProcessStatus.RUNNING)
                    Status = ProcessStatus.COMPLETED;
            }
            catch (ThreadAbortException) {
                Status = ProcessStatus.ABORTED;
            }
            catch {
                Status = ProcessStatus.FAILED;
            }
        }

        public abstract void Work();

    }
}
