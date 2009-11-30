using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Cornerstone.Tools.Translate;
using System.Globalization;

namespace MediaPortal.Plugins.MovingPictures.ConfigScreen.Popups {
    public partial class TranslationPopup : Form {

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

        public TranslationPopup() {
            InitializeComponent();

            if (!HostedDesignMode) {
                BuildComboItems();
            }
        }

        private void BuildComboItems() {
            languageComboBox.Items.Clear();

            List<TranslatorLanguage> languages = new List<TranslatorLanguage>();
            languages.AddRange(LanguageUtility.TranslatableCollection);    
       
            // removes languages that are already provided by the data provider
            //List<TranslatorLanguage> toRemove = new List<TranslatorLanguage>();
            //foreach (CultureInfo currCulture in MovingPicturesCore.DataProviderManager.GetAvailableLanguages()) {
            //    foreach (TranslatorLanguage currLanguage in languages) {
            //        if (LanguageUtility.GetLanguageCode(currLanguage) == currCulture.TwoLetterISOLanguageName)
            //            toRemove.Add(currLanguage);
            //    }
            //}

            //foreach (TranslatorLanguage currLang in toRemove)
            //    languages.Remove(currLang);

            foreach (TranslatorLanguage currLang in languages)
                languageComboBox.Items.Add(currLang);

            languageComboBox.SelectedItem = MovingPicturesCore.Settings.TranslationLanguage;
        }

        private void TranslationPopup_Load(object sender, EventArgs e) {
            Point center = new Point();
            center.X = Owner.Location.X + (Owner.Width / 2);
            center.Y = Owner.Location.Y + (Owner.Height / 2);

            Point newLocation = new Point();
            newLocation.X = center.X - (Width / 2);
            newLocation.Y = center.Y - (Height / 2);

            Location = newLocation;
        }

        private void okButton_Click(object sender, EventArgs e) {
            MovingPicturesCore.Settings.UseTranslator = true;
            MovingPicturesCore.Settings.TranslationLanguage = (TranslatorLanguage) languageComboBox.SelectedItem;

            DialogResult = DialogResult.OK;
            Close();
        }

        private void cancelButton_Click(object sender, EventArgs e) {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
