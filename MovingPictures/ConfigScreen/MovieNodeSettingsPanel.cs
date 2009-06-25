using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MediaPortal.Plugins.MovingPictures.Database;
using Cornerstone.GUI.Controls;
using MediaPortal.Plugins.MovingPictures.MainUI;
using Cornerstone.Database.Tables;

namespace MediaPortal.Plugins.MovingPictures.ConfigScreen {
    public partial class MovieNodeSettingsPanel : UserControl {
        private DBMovieNodeSettings settings;
        private bool updating = false;

        public DBNode<DBMovieInfo> Node {
            get { return _node; }
            set {
                _node = value;
                if (_node != null && _node.AdditionalSettings == null) 
                    _node.AdditionalSettings = new DBMovieNodeSettings();
                
                settings = _node == null ? null : (DBMovieNodeSettings)_node.AdditionalSettings;

                updateControls();
            }
        } 
        private DBNode<DBMovieInfo> _node;

        public MovieNodeSettingsPanel() {
            InitializeComponent();
        }

        private void updateControls() {
            bool active = settings != null;

            // reset enabled status for all controls
            randomBackdropRadioButton.Enabled = active;
            specificBackdropRadioButton.Enabled = active;
            fileBackdropRadioButton.Enabled = active;
            backdropMovieCombo.Enabled = active;
            backdropFileTextBox.Enabled = active;
            fileBrowserButton.Enabled = active;
            defaultSortRadioButton.Enabled = active;
            customSortRadioButton.Enabled = active;
            sortDirectionCombo.Enabled = active;
            sortFieldCombo.Enabled = active;

            // if we dont have any settings to modify, exit
            if (!active)
                return;

            updating = true;

            // set the checked status for backdrop options
            switch (settings.BackdropType) {
                case MenuBackdropType.RANDOM:
                    randomBackdropRadioButton.Checked = true;
                    break;
                case MenuBackdropType.MOVIE:
                    specificBackdropRadioButton.Checked = true;
                    break;
                case MenuBackdropType.FILE:
                    fileBackdropRadioButton.Checked = true;
                    break;
            }

            if (settings.UseDefaultSorting)
                defaultSortRadioButton.Checked = true;
            else
                customSortRadioButton.Checked = true;

            // set the sort options radio buttons appropriately
            switch (settings.SortDirection) {
                case MediaPortal.Plugins.MovingPictures.MainUI.SortingDirections.Ascending:
                    sortDirectionCombo.SelectedItem = "Ascending";
                    break;
                case MediaPortal.Plugins.MovingPictures.MainUI.SortingDirections.Descending:
                    sortDirectionCombo.SelectedItem = "Descending";
                    break;
            }

            // set enabled status for sub controls
            setSubControlAccessibility();

            // add all sorting options to combo
            Dictionary<Enum, FormatedComboBoxItem> comboItems = new Dictionary<Enum, FormatedComboBoxItem>();
            foreach (Enum currValue in Enum.GetValues(typeof(SortingFields))) {
                FormatedComboBoxItem newItem = new FormatedComboBoxItem();
                newItem.Value = currValue;
                comboItems[currValue] = newItem;
                sortFieldCombo.Items.Add(newItem);
            }
            sortFieldCombo.SelectedItem = comboItems[settings.SortField];

            // clear backdrop movie combo and set the selected movie. when user clicks it will be populated
            backdropMovieCombo.Items.Clear();
            if (settings.BackdropMovie != null) {
                backdropMovieCombo.Items.Add(settings.BackdropMovie);
                backdropMovieCombo.SelectedItem = settings.BackdropMovie;
            }

            backdropFileTextBox.Text = settings.BackdropFilePath;

            updating = false;
        }

        private void setSubControlAccessibility() {
            backdropMovieCombo.Enabled = specificBackdropRadioButton.Checked;
            fileBrowserButton.Enabled = fileBackdropRadioButton.Checked;
            backdropFileTextBox.Enabled = fileBackdropRadioButton.Checked;
            sortFieldCombo.Enabled = customSortRadioButton.Checked;
            sortDirectionCombo.Enabled = customSortRadioButton.Checked;
        }

        private void randomBackdropRadioButton_CheckedChanged(object sender, EventArgs e) {
            if (settings == null || updating) return;

            if (randomBackdropRadioButton.Checked)
                settings.BackdropType = MenuBackdropType.RANDOM;

            setSubControlAccessibility();
        }

        private void specificBackdropRadioButton_CheckedChanged(object sender, EventArgs e) {
            if (settings == null || updating) return;
            
            if (specificBackdropRadioButton.Checked)
                settings.BackdropType = MenuBackdropType.MOVIE;

            setSubControlAccessibility();
        }

        private void backdropMovieCombo_SelectedIndexChanged(object sender, EventArgs e) {
            if (settings == null || updating) return;

            settings.BackdropMovie = (DBMovieInfo)backdropMovieCombo.SelectedItem;
        }

        private void fileBackdropRadioButton_CheckedChanged(object sender, EventArgs e) {
            if (settings == null || updating) return;

            if (fileBackdropRadioButton.Checked)
                settings.BackdropType = MenuBackdropType.FILE;

            setSubControlAccessibility();
        }

        private void backdropFileTextBox_TextChanged(object sender, EventArgs e) {
            if (settings == null || updating) return;

            settings.BackdropFilePath = backdropFileTextBox.Text;

            if (settings.BackdropFile != null && !settings.BackdropFile.Exists) {
                backdropFileTextBox.ForeColor = Color.Red;
            }
            else backdropFileTextBox.ForeColor = SystemColors.ControlText;
        }

        private void fileBrowserButton_Click(object sender, EventArgs e) {
            if (settings == null || updating) return;

            if (openFileDialog.ShowDialog() == DialogResult.OK) {
                settings.BackdropFilePath = openFileDialog.FileName;
                backdropFileTextBox.Text = openFileDialog.FileName;
            }
        }

        private void defaultSortRadioButton_CheckedChanged(object sender, EventArgs e) {
            if (settings == null || updating) return;

            if (defaultSortRadioButton.Checked)
                settings.UseDefaultSorting = true;

            setSubControlAccessibility();
        }

        private void customSortRadioButton_CheckedChanged(object sender, EventArgs e) {
            if (settings == null || updating) return;

            if (customSortRadioButton.Checked)
                settings.UseDefaultSorting = false;

            setSubControlAccessibility();
        }

        private void sortFieldCombo_SelectedIndexChanged(object sender, EventArgs e) {
            if (settings == null || updating) return;

            settings.SortField = (SortingFields)((FormatedComboBoxItem)sortFieldCombo.SelectedItem).Value;
        }

        private void sortDirectionCombo_SelectedIndexChanged(object sender, EventArgs e) {
            if (settings == null || updating) return;


            if (((string)sortDirectionCombo.SelectedItem) == "Ascending")
                settings.SortDirection = MediaPortal.Plugins.MovingPictures.MainUI.SortingDirections.Ascending;
            else
                settings.SortDirection = MediaPortal.Plugins.MovingPictures.MainUI.SortingDirections.Descending;
        }

        private void backdropMovieCombo_DropDown(object sender, EventArgs e) {
            if (_node == null || updating)
                return;

            if (_node.DBManager == null)
                _node.DBManager = MovingPicturesCore.DatabaseManager;

            object selected = backdropMovieCombo.SelectedItem;
            
            backdropMovieCombo.Items.Clear();
            HashSet<DBMovieInfo> unsortedMovies = _node.GetPossibleFilteredItems();
            IOrderedEnumerable<DBMovieInfo> sortedMovies = unsortedMovies.OrderBy((movie) => movie.SortBy);
            backdropMovieCombo.Items.AddRange(sortedMovies.ToArray());

            backdropMovieCombo.SelectedItem = selected;
        }
    }
}
