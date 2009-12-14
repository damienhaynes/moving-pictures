using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Reflection;

namespace MediaPortal.Plugins.MovingPictures.ConfigScreen {
    public partial class SplashPane : UserControl {
        private delegate object PropertySetDelegate(object obj, object[] parameters);
        
        public bool ShowProgressComponents {
            get { return _showProgressComponents; }
            set { 
                _showProgressComponents = value;
                updateComponentVisibility();
            }
        } private bool _showProgressComponents;

        private void updateComponentVisibility() {
            progressBar.Visible = _showProgressComponents;
            statusLabel.Visible = _showProgressComponents;
        } 
        
        public int Progress {
            get { return _progress; }
            set {
                if (InvokeRequired) {
                    Invoke(getPropertyDelegate("Progress"), new Object[] {this, new object[] {value}});
                    return;
                }

                _progress = value;

                if (_progress < 0)
                    _progress = 0;

                if (_progress > 100)
                    _progress = 100;

                progressBar.Value = _progress;
            }
        } int _progress = 0;

        public string Status {
            get { return statusLabel.Text; }
            set {
                if (InvokeRequired) {
                    Invoke(getPropertyDelegate("Status"), new Object[] { this, new object[] { value } });
                    return;
                }

                statusLabel.Text = value;
            }
        } 
        
        public SplashPane() {
            InitializeComponent();
        }

        private void SplashPane_Load(object sender, EventArgs e) {
            versionLabel.Text = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            updateComponentVisibility();
        }

        private PropertySetDelegate getPropertyDelegate(string propertyName) {
            return new PropertySetDelegate(this.GetType().GetProperty(propertyName).GetSetMethod().Invoke);
        }
    }
}
