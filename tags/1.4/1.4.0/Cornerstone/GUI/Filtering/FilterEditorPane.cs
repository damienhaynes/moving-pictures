using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Cornerstone.GUI;
using Cornerstone.GUI.Controls;
using Cornerstone.Database;
using Cornerstone.Database.Tables;

namespace Cornerstone.GUI.Filtering {
    public class FilterEditorPane : UserControl, IFieldDisplaySettingsOwner, IFilterEditorPane {
        private Control genericFilterEditorPane;

        #region IFieldDisplaySettingsOwner Members
 
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

                if (genericFilterEditorPane != null) {
                    ((IFieldDisplaySettingsOwner)genericFilterEditorPane).FieldDisplaySettings = value;
                    ((IFieldDisplaySettingsOwner)genericFilterEditorPane).FieldDisplaySettings.Owner = this;
                }

                InitializeComponent();
            }
        } private FieldDisplaySettings _fieldSettings = null;

        public void OnFieldPropertiesChanged() {
            InitializeComponent();

            if (genericFilterEditorPane != null) 
                ((IFieldDisplaySettingsOwner)genericFilterEditorPane).OnFieldPropertiesChanged();
        }

        #endregion

        #region IFilterEditorPane Members

        [Category("Cornerstone Settings")]
        [Description("The friendly name for the type of objects being filtered (plural).")]
        public string DisplayName {
            get { 
                return _displayName; 
            }
            set {
                if (genericFilterEditorPane != null) 
                    ((IFilterEditorPane)genericFilterEditorPane).DisplayName = value;
                
                _displayName = value;
            }
        } private string _displayName = "items";


        [Browsable(false)]
        public DatabaseManager DBManager {
            get { return _dbManager; }
            set { 
                ((IFilterEditorPane)genericFilterEditorPane).DBManager = value;
                _dbManager = ((IFilterEditorPane)genericFilterEditorPane).DBManager;
            }
        } private DatabaseManager _dbManager;

        #endregion
        
        [ReadOnly(true)]
        [Browsable(false)]
        public IDBFilter AttachedFilter {
            get {
                if (genericFilterEditorPane == null) return null;
                return ((IFilterEditorPane)genericFilterEditorPane).AttachedFilter;
            }
            set {
                ((IFilterEditorPane)genericFilterEditorPane).AttachedFilter = value;
            }
        } 

        public FilterEditorPane() {
            InitializeComponent();
        }

        
        private void InitializeComponent() {
            // determine the type the filter applies to
            Type filterType = typeof(DatabaseTable);
            if (_fieldSettings != null && _fieldSettings.Table != null) filterType = _fieldSettings.Table;

            // create an instance of the filter panel specific to the type we are filtering
            this.genericFilterEditorPane = null;
            Type genericType = typeof(GenericFilterEditorPane<>).GetGenericTypeDefinition();
            Type specificType = genericType.MakeGenericType(new Type[] { filterType });
            genericFilterEditorPane = (Control)specificType.GetConstructor(Type.EmptyTypes).Invoke(null);
            genericFilterEditorPane.Dock = System.Windows.Forms.DockStyle.Fill;

            // link new control to existing settings
            ((IFilterEditorPane)genericFilterEditorPane).DBManager = DBManager;
            ((IFilterEditorPane)genericFilterEditorPane).DisplayName = DisplayName;
            ((IFieldDisplaySettingsOwner)genericFilterEditorPane).FieldDisplaySettings = FieldDisplaySettings;


            /*
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;           
            */

            SuspendLayout();
            Controls.Clear();
            Controls.Add(this.genericFilterEditorPane);

            Name = "FilterEditorPane";
            Size = new System.Drawing.Size(689, 364);

            ResumeLayout(false);
        }
    }
}
