using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using Cornerstone.Database;
using Cornerstone.Database.Tables;
using Cornerstone.GUI.DesignMode;

namespace Cornerstone.GUI.Controls {
    [Designer(typeof(DBObjectListDesigner))]
    public class DBObjectEditor : UserControl, IFieldDisplaySettingsOwner {

        #region Private Variables

        DataGridView grid;
        DataGridViewTextBoxColumn fieldColumn;
        DataGridViewTextBoxColumn valueColumn;

        #endregion

        #region Properties

        [Category("Cornerstone Settings")]
        [Description("Manage the type of database table this control connects to and which fields should be displayed.")]
        [ReadOnly(true)]
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
            }
        } private FieldDisplaySettings _fieldSettings = null;

        // the object containing values for display
        [Browsable(false)]
        public DatabaseTable DatabaseObject {
            get {
                return _dbObject;
            }
            set {
                _dbObject = value;
                repopulateValues();
            }
        } private DatabaseTable _dbObject = null;

        #endregion

        public DBObjectEditor()
            : base() {
            InitializeComponent();

            grid.Columns[0].DefaultCellStyle.BackColor = System.Drawing.SystemColors.Control;
            grid.Columns[1].DefaultCellStyle.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
        }

        public void OnFieldPropertiesChanged() {
            if (grid.Columns.Count == 0)
                return;

            grid.Rows.Clear();

            foreach (DBField currField in FieldDisplaySettings.Fields) {
                FieldProperty properties = FieldDisplaySettings.getProperties(currField.Name);
                if (properties == null) {
                    properties = new FieldProperty();
                    properties.FieldName = currField.Name;
                    properties.Visible = true;
                }

                if (properties.Visible == false)
                    continue;

                int rowNum = grid.Rows.Add();
                DataGridViewRow currRow = grid.Rows[rowNum];
                currRow.Tag = currField;
                currRow.ReadOnly = properties.ReadOnly;

                // build the custom value cell
                IDBFieldBackedControl valueCell;
                if (currField.DBType == DBField.DBDataType.ENUM)
                    valueCell = new DBComboBoxCell();
                else
                    valueCell = new DBTextBoxCell();

                valueCell.Table = FieldDisplaySettings.Table;
                valueCell.DatabaseFieldName = properties.FieldName;
                valueCell.DatabaseObject = DatabaseObject;
                currRow.Cells["valueColumn"] = (DataGridViewCell)valueCell;

                currRow.Cells["fieldColumn"].Value = properties.DisplayName;
            }
        }

        #region Private Methods

        private void repopulateValues() {
            foreach (DataGridViewRow currRow in grid.Rows) {
                DBField currField = (DBField)currRow.Tag;
                ((IDBBackedControl)currRow.Cells["valueColumn"]).DatabaseObject = DatabaseObject;
            }

            grid.AutoResizeRows();
        }

        private void InitializeComponent() {
            this.grid = new System.Windows.Forms.DataGridView();
            this.fieldColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.valueColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
            this.SuspendLayout();
            // 
            // grid
            // 
            this.grid.AllowUserToAddRows = false;
            this.grid.AllowUserToDeleteRows = false;
            this.grid.ColumnHeadersVisible = false;
            this.grid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.fieldColumn,
            this.valueColumn});
            this.grid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grid.Location = new System.Drawing.Point(0, 0);
            this.grid.Name = "grid";
            this.grid.RowHeadersVisible = false;
            this.grid.Size = new System.Drawing.Size(375, 552);
            this.grid.TabIndex = 0;
            // 
            // fieldColumn
            // 
            this.fieldColumn.HeaderText = "Field";
            this.fieldColumn.Name = "fieldColumn";
            this.fieldColumn.ReadOnly = true;
            // 
            // valueColumn
            // 
            this.valueColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.valueColumn.HeaderText = "Value";
            this.valueColumn.Name = "valueColumn";
            // 
            // DBObjectList
            // 
            this.Controls.Add(this.grid);
            this.Name = "DBObjectList";
            this.Size = new System.Drawing.Size(375, 552);
            ((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
    }
}
