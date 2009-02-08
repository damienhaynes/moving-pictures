using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace MediaPortal.Plugins.MovingPictures.ConfigScreen.Popups {
    public partial class ProgressPopup : Form {
        Thread workerThread;
        
        WorkerDelegate worker;
        TrackableWorkerDelegate trackableWorker;

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

        private void ProgressPopup_Load(object sender, EventArgs e) {
            if (!DesignMode) {
                center();

                // try setting up the vanilla worker / progress bar
                if (worker != null) {
                    workerThread = new Thread(new ThreadStart(worker));
                    workerThread.Start();
                    timer.Enabled = true;
                }

                // try to setup the trackable worker / progress bar
                else if (trackableWorker != null) {
                    progressBar.Style = ProgressBarStyle.Blocks;
                    workerThread = new Thread(new ThreadStart(paramThreadWrapper));
                    workerThread.Start();
                    timer.Enabled = true;
                }

                // if both are null, then just shut down
                else {
                    this.Close();
                    return;
                }

            }
        }

        private void paramThreadWrapper() {
            trackableWorker.Invoke(new ProgressDelegate(Progress));
        }

        private void timer_Tick(object sender, EventArgs e) {
            if (!workerThread.IsAlive)
                this.Close();
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
