using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Cornerstone.GUI.Controls;
using Cornerstone.GUI.Filtering;
using Cornerstone.Database.Tables;
using Cornerstone.Database;

namespace Cornerstone.GUI {
    public partial class GenericFilterEditorPane<T> : 
        UserControl, IFieldDisplaySettingsOwner, IFilterEditorPane where T: DatabaseTable {

        [Category("Cornerstone Settings")]
        [Description("Manage the type of database table this control connects to and which fields should be displayed.")]
        public FieldDisplaySettings FieldDisplaySettings {
            get {
                if (_fieldSettings == null) {
                    _fieldSettings = new FieldDisplaySettings();
                    _fieldSettings.Owner = this;
                }

                return _fieldSettings;
            }

            set {
                _fieldSettings = value;
                _fieldSettings.Owner = this;
                initControls();
            }
        } private FieldDisplaySettings _fieldSettings = null;


        [Category("Cornerstone Settings")]
        [Description("The friendly name for the type of objects being filtered (plural).")]
        public string DisplayName {
            get { return _displayName; }
            set { 
                _displayName = value;
                updateLabels();
            }
        } private string _displayName = "items";

        [Browsable(false)]
        public DatabaseManager DBManager {
            get {
                return _dbManager;
            }
            set {
                _dbManager = value;
            }
        } private DatabaseManager _dbManager;

        public IDBFilter AttachedFilter {
            get { return _attachedFilter; }
            
            set {
                if (value is DBFilter<T>) {
                    _attachedFilter = (DBFilter<T>)value;
                    updateScreen();
                }
            }
        } private DBFilter<T> _attachedFilter;

        public GenericFilterEditorPane() {
            InitializeComponent();
            updateLabels();
        }

        private void updateScreen() {
            whiteList.DatabaseObjects.Clear();
            foreach (T currItem in _attachedFilter.WhiteList)
                whiteList.DatabaseObjects.Add(currItem);

            blackList.DatabaseObjects.Clear();
            foreach (T currItem in _attachedFilter.BlackList)
                blackList.DatabaseObjects.Add(currItem);
            
            filterNameTextBox.Text = _attachedFilter.Name;

            switch (_attachedFilter.CriteriaGrouping) {
                case DBFilter<T>.CriteriaGroupingEnum.ALL:
                    filterGroupingCombo.SelectedIndex = 0;
                    break;
                case DBFilter<T>.CriteriaGroupingEnum.ONE:
                    filterGroupingCombo.SelectedIndex = 1;
                    break;
                case DBFilter<T>.CriteriaGroupingEnum.NONE:
                    filterGroupingCombo.SelectedIndex = 2;
                    break;
            }

            //criteriaListPanel1
            criteriaListPanel1.ClearCriteria();
            foreach (DBCriteria<T> currCriteria in _attachedFilter.Criteria) {
                criteriaListPanel1.AddCriteria(currCriteria);
            }
        }

        private void updateLabels() {
            this.label1.Text = string.Format("Include {0} matching", DisplayName);
            this.label8.Text = string.Format("Always include these {0}:", DisplayName);
            this.label9.Text = string.Format("Always exclude these {0}:", DisplayName);
        }

        public void OnFieldPropertiesChanged() {
            initControls();
        }

        private void initControls() {
            whiteList.FieldDisplaySettings.Table = FieldDisplaySettings.Table;
            whiteList.FieldDisplaySettings.FieldProperties = FieldDisplaySettings.FieldProperties;

            blackList.FieldDisplaySettings.Table = FieldDisplaySettings.Table;
            blackList.FieldDisplaySettings.FieldProperties = FieldDisplaySettings.FieldProperties;

            criteriaListPanel1.Table = FieldDisplaySettings.Table;
            criteriaListPanel1.DBManager = DBManager;
        }

        private void FilterEditorPane_Load(object sender, EventArgs e) {
            initControls();
            updateScreen();
        }

        public void AddCriteria(IGenericFilter criteria) {
            criteriaListPanel1.AddCriteria(criteria);
            _attachedFilter.Criteria.Add((DBCriteria<T>)criteria);
        }

        private void whitelistAddButton_Click(object sender, EventArgs e) {
            ItemSelectionPopup popup = new ItemSelectionPopup();
            popup.FieldDisplaySettings.Table = FieldDisplaySettings.Table;
            popup.FieldDisplaySettings.FieldProperties = FieldDisplaySettings.FieldProperties;
            popup.ItemList.DatabaseObjects.AddRange(DBManager.Get(FieldDisplaySettings.Table, null));

            foreach (DatabaseTable currItem in whiteList.DatabaseObjects)
                popup.ItemList.DatabaseObjects.Remove(currItem);

            foreach (DatabaseTable currItem in blackList.DatabaseObjects)
                popup.ItemList.DatabaseObjects.Remove(currItem);


            popup.ShowDialog();

            if (popup.ExitStatus == ItemSelectionPopup.ExitStatusEnum.OK) {
                whiteList.DatabaseObjects.AddRange(popup.ItemList.SelectedDatabaseObjects);
                foreach (DatabaseTable currItem in popup.ItemList.SelectedDatabaseObjects)
                    _attachedFilter.WhiteList.Add((T)currItem);
            }
        }

        private void whitelistRemoveButton_Click(object sender, EventArgs e) {
            foreach (DatabaseTable currItem in whiteList.SelectedDatabaseObjects) {
                whiteList.DatabaseObjects.Remove(currItem);
                _attachedFilter.WhiteList.Remove((T)currItem);
            }
        }

        private void blacklistAddButton_Click(object sender, EventArgs e) {
            ItemSelectionPopup popup = new ItemSelectionPopup();
            popup.FieldDisplaySettings.Table = FieldDisplaySettings.Table;
            popup.FieldDisplaySettings.FieldProperties = FieldDisplaySettings.FieldProperties;
            popup.ItemList.DatabaseObjects.AddRange(DBManager.Get(FieldDisplaySettings.Table, null));

            foreach (DatabaseTable currItem in whiteList.DatabaseObjects)
                popup.ItemList.DatabaseObjects.Remove(currItem);

            foreach (DatabaseTable currItem in blackList.DatabaseObjects)
                popup.ItemList.DatabaseObjects.Remove(currItem);

            
            popup.ShowDialog();

            if (popup.ExitStatus == ItemSelectionPopup.ExitStatusEnum.OK) {
                blackList.DatabaseObjects.AddRange(popup.ItemList.SelectedDatabaseObjects);
                foreach (DatabaseTable currItem in popup.ItemList.SelectedDatabaseObjects)
                    _attachedFilter.BlackList.Add((T)currItem);
            }
        }

        private void blacklistRemoveButton_Click(object sender, EventArgs e) {
            foreach (DatabaseTable currItem in blackList.SelectedDatabaseObjects) {
                blackList.DatabaseObjects.Remove(currItem);
                _attachedFilter.BlackList.Remove((T)currItem);
            }
        }

        private void filterNameTextBox_TextChanged(object sender, EventArgs e) {
            _attachedFilter.Name = filterNameTextBox.Text;
        }

        private void filterGroupingCombo_SelectedIndexChanged(object sender, EventArgs e) {
            switch (filterGroupingCombo.SelectedIndex) {
                case 0:
                    _attachedFilter.CriteriaGrouping = DBFilter<T>.CriteriaGroupingEnum.ALL;
                    break;
                case 1:
                    _attachedFilter.CriteriaGrouping = DBFilter<T>.CriteriaGroupingEnum.ONE;
                    break;
                case 2:
                    _attachedFilter.CriteriaGrouping = DBFilter<T>.CriteriaGroupingEnum.NONE;
                    break;

            }
        }

        private void addCriteriaButton_ButtonClick(object sender, EventArgs e) {
            DBCriteria<T> newCriteria = new DBCriteria<T>();
            AddCriteria(newCriteria);
        }

        private void removeCriteriaButton_Click(object sender, EventArgs e) {
            _attachedFilter.Criteria.Remove((DBCriteria<T>)criteriaListPanel1.SelectedCriteria); 
            criteriaListPanel1.RemoveSelectedCriteria();
        }

        private void button1_Click(object sender, EventArgs e) {
            ItemSelectionPopup popup = new ItemSelectionPopup();
            popup.FieldDisplaySettings.Table = FieldDisplaySettings.Table;
            popup.FieldDisplaySettings.FieldProperties = FieldDisplaySettings.FieldProperties;
            popup.ShowCancelButton = false;
            popup.Text = "Results";
            
            List<T> allItems = DBManager.Get<T>(null);
            List<T> filteredItems = _attachedFilter.Filter(allItems);
            
            foreach(T currItem in filteredItems) 
                popup.ItemList.DatabaseObjects.Add(currItem);

            popup.ShowDialog();
        }
    }
}
