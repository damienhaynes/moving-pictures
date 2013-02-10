using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Cornerstone.Tools {
    public delegate void ProcessProgressDelegate(AbstractBackgroundProcess process, double progress);
    public delegate void ProcessStatusChangedDelegate(AbstractBackgroundProcess process, ProcessStatus status);

    public enum ProcessStatus {
        INITIALIZED,
        RUNNING,
        COMPLETED,
        FAILED,
        ABORTED
    }
    
    public class BackgroundProcessManager {
        
        private HashSet<AbstractBackgroundProcess> activeProcesses;
        Dictionary<AbstractBackgroundProcess, Thread> threadLookup;

        public BackgroundProcessManager() {
            activeProcesses = new HashSet<AbstractBackgroundProcess>();
            threadLookup = new Dictionary<AbstractBackgroundProcess, Thread>();

            progressDelegate = new ProcessProgressDelegate(OnProgress);
            statusChangedDelegate = new ProcessStatusChangedDelegate(OnStatusChanged);
        }

        #region Events

        /// <summary>
        /// Triggered when progress is reported by any process. Should pass a double between 0.0 and 1.0
        /// indicating progress to completion.
        /// </summary>
        public event ProcessProgressDelegate Progress;

        /// <summary>
        /// Triggered when the status of a process has changed.
        /// </summary>
        public event ProcessStatusChangedDelegate StatusChanged;

        private ProcessProgressDelegate progressDelegate;
        private ProcessStatusChangedDelegate statusChangedDelegate;

        private void OnProgress(AbstractBackgroundProcess process, double progress) {
            if (Progress != null) Progress(process, progress);
        }

        private void OnStatusChanged(AbstractBackgroundProcess process, ProcessStatus status) {
            lock (activeProcesses) {
                if ((process.Status == ProcessStatus.FAILED ||
                    process.Status == ProcessStatus.COMPLETED ||
                    process.Status == ProcessStatus.ABORTED) && 
                    activeProcesses.Contains(process)) {

                    activeProcesses.Remove(process);
                    threadLookup.Remove(process);
                }
            }
            
            if (StatusChanged != null) StatusChanged(process, status);
        }

        #endregion

        public void StartProcess(AbstractBackgroundProcess process) {
            lock (activeProcesses)
                if (!activeProcesses.Contains(process)) {
                    activeProcesses.Add(process);
                    OnStatusChanged(process, process.Status);

                    process.StatusChanged += statusChangedDelegate;
                    process.Progress += progressDelegate;

                    Thread thread = new Thread(new ThreadStart(process.WorkRunner));
                    thread.Name = process.Name;
                    threadLookup[process] = thread;

                    thread.Start();
                }
        }

        public void CancelProcess(AbstractBackgroundProcess process) {
            lock(activeProcesses) 
                if (activeProcesses.Contains(process) && threadLookup[process].IsAlive) {
                    threadLookup[process].Abort();
                    activeProcesses.Remove(process);
                    threadLookup.Remove(process);
                }
        }

        public void CancelProcess(Type processType) {
            List<AbstractBackgroundProcess> processesToKill = new List<AbstractBackgroundProcess>();

            lock (activeProcesses) {
                foreach (AbstractBackgroundProcess process in activeProcesses) {
                    if (process.GetType() == processType) processesToKill.Add(process);
                }
            }

            foreach (AbstractBackgroundProcess process in processesToKill) {
                CancelProcess(process);
            }

        }

        public void CancelAllProcesses() {
            List<AbstractBackgroundProcess> allProcesses = new List<AbstractBackgroundProcess>();

            foreach (AbstractBackgroundProcess process in activeProcesses) 
                allProcesses.Add(process);

            foreach (AbstractBackgroundProcess process in allProcesses) {
                CancelProcess(process);
            }
        }

        public void WaitFor(AbstractBackgroundProcess process) {
            if (!activeProcesses.Contains(process))
                return;

            while (process.Status == ProcessStatus.INITIALIZED ||
                   process.Status == ProcessStatus.RUNNING) {

                Thread.Sleep(100);
            }
        }


        public void WaitFor(Type processType) {
            List<AbstractBackgroundProcess> processes = new List<AbstractBackgroundProcess>();
            foreach (AbstractBackgroundProcess currProcess in activeProcesses)
                processes.Add(currProcess);

            foreach (AbstractBackgroundProcess currProcess in processes) {
                while (currProcess.Status == ProcessStatus.INITIALIZED ||
                       currProcess.Status == ProcessStatus.RUNNING) {

                    Thread.Sleep(100);
                }
            }

        }

        public HashSet<AbstractBackgroundProcess> GetAllProcesses() {
            return activeProcesses;
        }


    }


}
