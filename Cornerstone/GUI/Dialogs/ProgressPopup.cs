using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace Cornerstone.GUI.Dialogs {
    public delegate void WorkCompleteDelegate();
    public delegate void WorkerDelegate();
    public delegate void TrackableWorkerDelegate(ProgressDelegate progress);
    public delegate void ProgressDelegate(string actionName, int percentDone);

    public partial class ProgressPopup : Form {
        Thread workerThread;
        
        WorkerDelegate worker;
        TrackableWorkerDelegate trackableWorker;

        bool working = false;

        System.Windows.Forms.Timer delayTimer;
        System.Threading.Timer checkDoneTimer;

        public event WorkCompleteDelegate WorkComplete;

        public ProgressPopup(WorkerDelegate worker) {
            InitializeComponent();

            this.worker = worker;
            trackableWorker = null;
        }

        public ProgressPopup(TrackableWorkerDelegate trackableWorker) {
            InitializeComponent();
            
            worker = null;
            this.trackableWorker = trackableWorker;
        }

        public void ShowDialogDelayed(int delay) {         
            delayTimer = new System.Windows.Forms.Timer();
            delayTimer.Interval = delay;
            delayTimer.Tick += new EventHandler(delayTimer_Tick);
            delayTimer.Start();

            startProcessing();
        }

        void delayTimer_Tick(object sender, EventArgs e) {
            delayTimer.Stop();

            if (IsDisposed)
                return;

            if ((worker == null && trackableWorker == null) || workerThread.IsAlive) 
                ShowDialog();
        }

        private void ProgressPopup_Load(object sender, EventArgs e) {
            if (!DesignMode) {
                center();

                startProcessing();
            }
        }

        private void startProcessing() {
            if (working)
                return;

            working = true;

            // try setting up the vanilla worker / progress bar
            if (worker != null) {
                workerThread = new Thread(new ThreadStart(worker));
                workerThread.IsBackground = true; 
                workerThread.Start();
                checkDoneTimer = new System.Threading.Timer(new TimerCallback(checkDoneTimer_Tick), null, 100, 100);
            }

            // try to setup the trackable worker / progress bar
            else if (trackableWorker != null) {
                progressBar.Style = ProgressBarStyle.Blocks;
                workerThread = new Thread(new ThreadStart(paramThreadWrapper));
                workerThread.IsBackground = true; 
                workerThread.Start();
                checkDoneTimer = new System.Threading.Timer(new TimerCallback(checkDoneTimer_Tick), null, 100, 100);
            }

            // if both are null, then just shut down
            else {
                this.Close();
                return;
            }
        }

        private void paramThreadWrapper() {
            trackableWorker.Invoke(new ProgressDelegate(Progress));
        }

        private void checkDoneTimer_Tick(object state) {
            if (!workerThread.IsAlive && working) {
                checkDoneTimer.Dispose();
                working = false;

                if (InvokeRequired) {
                    Invoke(new WorkerDelegate(Close));
                }
                
                if (WorkComplete != null) WorkComplete();
            }
        }

        public void Progress(string description, int percentage) {
            if (InvokeRequired) {
                Invoke(new ProgressDelegate(Progress), new object[] { description, percentage });
                return;
            }

            if (progressBar.Style != ProgressBarStyle.Blocks)
                progressBar.Style = ProgressBarStyle.Blocks;

            progressBar.Value = percentage;
            if (description.Trim().Length > 0)
                this.Text = description;
        }

        private void center() {
            if (Owner == null)
                return;

            Point center = new Point();
            center.X = Owner.Location.X + (Owner.Width / 2);
            center.Y = Owner.Location.Y + (Owner.Height / 2);

            Point newLocation = new Point();
            newLocation.X = center.X - (Width / 2);
            newLocation.Y = center.Y - (Height / 2);

            Location = newLocation;
        }



    }
}
