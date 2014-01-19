using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Globalization;
using MediaPortal.Plugins.MovingPictures.ConfigScreen.Popups;
using Cornerstone.Tools.Translate;

namespace MediaPortal.Plugins.MovingPictures.ConfigScreen {
    public partial class AutoDataSourcesPanel : UserControl {
        
        private bool   initializing = false;
        private int    lineOffset = 1;
        private string additionalOptionsText = "Additional Options...";

        public bool HostedDesignMode {
            get {
                Control parent = Parent;
                while (parent != null && parent.Site != null) {
                    if (parent.Site.DesignMode) return true;
                    parent = parent.Parent;
                }
                return DesignMode;
            }
        }

        public bool AutoCommit {
            get { return _autoCommit; }
            set { _autoCommit = value; }
        } private bool _autoCommit = true;

        public AutoDataSourcesPanel() {
            InitializeComponent();
            languageComboBox.DrawMode = DrawMode.OwnerDrawFixed;
            languageComboBox.DrawItem += new DrawItemEventHandler(comboBox1_DrawItem);
        }

        void comboBox1_DrawItem(object sender, DrawItemEventArgs e) {
            if (!languageComboBox.DroppedDown) {
                e.Graphics.DrawRectangle(Pens.White, e.Bounds);
            }
            else {
                e.DrawBackground();

                if (e.Index == languageComboBox.Items.Count - (1 + lineOffset)) {
                    e.Graphics.DrawLine(Pens.DarkGray, new Point(e.Bounds.Left, e.Bounds.Bottom - 1),
                                        new Point(e.Bounds.Right, e.Bounds.Bottom - 1));
                }
            }

            if (e.Index != -1) {
                string text;
                Color color;
                
                if (languageComboBox.Items[e.Index] is CultureInfo) {
                    text = ((CultureInfo)languageComboBox.Items[e.Index]).DisplayName;
                    color = languageComboBox.ForeColor;
                }
                else {
                    text = languageComboBox.Items[e.Index].ToString();
                    color = Color.DarkBlue;
                }

                TextRenderer.DrawText(e.Graphics, text, languageComboBox.Font, e.Bounds, 
                                      color, TextFormatFlags.Left);
            }

            e.DrawFocusRectangle();
        }

        private void AutoDataSourcesPanel_Load(object sender, EventArgs e) {
            UpdateControls();
        }

        public void Commit() {
            if (autoRadioButton.Checked) {
                MovingPicturesCore.Settings.DataProviderManagementMethod = "auto";

                if (languageComboBox.SelectedItem is CultureInfo) {
                    MovingPicturesCore.Settings.UseTranslator = false;
                    MovingPicturesCore.Settings.DataProviderAutoLanguage = ((CultureInfo)languageComboBox.SelectedItem).TwoLetterISOLanguageName;
                    MovingPicturesCore.DataProviderManager.AutoArrangeDataProviders();
                } else {
                    MovingPicturesCore.Settings.UseTranslator = true;
                    MovingPicturesCore.Settings.DataProviderAutoLanguage = "en";
                    MovingPicturesCore.DataProviderManager.AutoArrangeDataProviders();
                }
            }

            if (manualRadioButton.Checked) {
                MovingPicturesCore.Settings.DataProviderManagementMethod = "manual";
            }
        }

        private void UpdateControls() {
            if (HostedDesignMode)
                return;

            initializing = true;
            lineOffset = 1;

            languageComboBox.Items.Clear();
            languageComboBox.Items.AddRange(MovingPicturesCore.DataProviderManager.GetAvailableLanguages().ToArray());

            if (MovingPicturesCore.Settings.TranslatorConfigured) {
                languageComboBox.Items.Add("Translated: " + MovingPicturesCore.Settings.TranslationLanguage);
                lineOffset = 2;
            }
            
            languageComboBox.Items.Add(additionalOptionsText);

            if (MovingPicturesCore.Settings.UseTranslator && MovingPicturesCore.Settings.TranslatorConfigured) 
                languageComboBox.SelectedIndex = languageComboBox.Items.Count - lineOffset;
            else 
                languageComboBox.SelectedItem = new CultureInfo(MovingPicturesCore.Settings.DataProviderAutoLanguage);



            if (MovingPicturesCore.Settings.DataProviderManagementMethod == "auto") {
                autoRadioButton.Checked = true;
                manualRadioButton.Checked = false;
                languageComboBox.Enabled = true;
                languageComboBox.ForeColor = SystemColors.ControlText;
            }
            else if (MovingPicturesCore.Settings.DataProviderManagementMethod == "undefined") {
                MovingPicturesCore.Settings.DataProviderManagementMethod = "auto";

                autoRadioButton.Checked = true;
                manualRadioButton.Checked = false;
                languageComboBox.Enabled = true;
                languageComboBox.ForeColor = SystemColors.ControlText;
            }
            else if (MovingPicturesCore.Settings.DataProviderManagementMethod == "manual") {
                autoRadioButton.Checked = false;
                manualRadioButton.Checked = true;
                languageComboBox.Enabled = false;
                languageComboBox.ForeColor = Color.DarkGray;
            }

            initializing = false;           
        }

        private void autoRadioButton_CheckedChanged(object sender, EventArgs e) {
            if (autoRadioButton.Checked) {
                if (AutoCommit) {
                    MovingPicturesCore.Settings.DataProviderManagementMethod = "auto";

                    if (MovingPicturesCore.Settings.UseTranslator) 
                        MovingPicturesCore.Settings.DataProviderAutoLanguage = "en";

                    MovingPicturesCore.DataProviderManager.AutoArrangeDataProviders();
                }

                UpdateControls();
                languageComboBox.Enabled = true;
                languageComboBox.ForeColor = SystemColors.ControlText;
            }
        }

        private void manualRadioButton_CheckedChanged(object sender, EventArgs e) {
            if (manualRadioButton.Checked) {
                if (AutoCommit)
                    MovingPicturesCore.Settings.DataProviderManagementMethod = "manual";

                languageComboBox.Enabled = false;
                languageComboBox.ForeColor = Color.DarkGray;
            }
        }

        private void languageComboBox_SelectedIndexChanged(object sender, EventArgs e) {
            if (initializing)
                return;

            if (languageComboBox.SelectedItem is CultureInfo) {
                MovingPicturesCore.Settings.UseTranslator = false;

                if (AutoCommit) {
                    MovingPicturesCore.Settings.DataProviderAutoLanguage = ((CultureInfo)languageComboBox.SelectedItem).TwoLetterISOLanguageName;
                    MovingPicturesCore.DataProviderManager.AutoArrangeDataProviders();
                }
            }
            else if (languageComboBox.SelectedItem is string && ((string)languageComboBox.SelectedItem) != additionalOptionsText) {
                if (AutoCommit) {
                    MovingPicturesCore.Settings.UseTranslator = true;
                    MovingPicturesCore.Settings.DataProviderAutoLanguage = "en";
                    MovingPicturesCore.DataProviderManager.AutoArrangeDataProviders();
                }
            }
            else {
                TranslationPopup popup = new TranslationPopup();
                popup.Owner = FindForm();
                DialogResult result = popup.ShowDialog();

                if (result == DialogResult.OK) {
                    if (AutoCommit) {
                        MovingPicturesCore.Settings.DataProviderAutoLanguage = "en";
                        MovingPicturesCore.DataProviderManager.AutoArrangeDataProviders();
                    }

                    MovingPicturesCore.Settings.TranslatorConfigured = true;

                }
                                
                UpdateControls();
            }         
        }
    }
}
