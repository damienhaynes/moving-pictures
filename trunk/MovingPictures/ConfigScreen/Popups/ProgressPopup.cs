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
        public delegate void ProgressDelegate(int current, int total);
        public delegate void TrackableWorkerDelegate(ProgressDelegate progress);
        public delegate void WorkerDelegate();

        Thread workerThread;
        
        WorkerDelegate worker;
        TrackableWorkerDelegate trackableWorker;

        public ProgressPopup(WorkerDelegate worker) {
            InitializeComponent();

            this.worker = worker;
            trackableWorker = null;
        }

        private void ProgressPopup_Load(object sender, EventArgs e) {
            if (!DesignMode) {
                center();

                workerThread = new Thread(new ThreadStart(worker));
                workerThread.Start();

                timer.Enabled = true;
            }
        }

        private void timer_Tick(object sender, EventArgs e) {
            if (!workerThread.IsAlive)
                this.Close();
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
