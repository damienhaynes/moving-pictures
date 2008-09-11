using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using MediaPortal.Plugins.MovingPictures.Database;
using MediaPortal.Plugins.MovingPictures.ConfigScreen.DesignMode;
using MediaPortal.Plugins.MovingPictures.Database.MovingPicturesTables;
using MediaPortal.Plugins.MovingPictures.ConfigScreen.Popups;
using Cornerstone.Database;
using Cornerstone.Database.Tables;

namespace MediaPortal.Plugins.MovingPictures.ConfigScreen.Controls {
    public partial class AttributesEditor : UserControl {
        public AttributesEditor() {
            InitializeComponent();
        }

        // the object containing values for display
        [Browsable(false)]
        public IAttributeOwner DatabaseObject {
            get {
                return _dbObject;
            }
            set {
                _dbObject = value;
                generateRows();
            }
        } private IAttributeOwner _dbObject = null;
        
        #region Private Methods
        private void generateRows() {
            if (DatabaseObject == null)
                return;
            
            grid.Rows.Clear();

            foreach (DBAttribute currAttr in DatabaseObject.Attributes) {
                int rowNum = grid.Rows.Add();
                DataGridViewRow currRow = grid.Rows[rowNum];
                currRow.Tag = currAttr;

                // build the custom value cell
                IDBFieldBackedControl valueCell;
                if (currAttr.Description.SelectionMode == DBAttrDescription.SelectionModeEnum.Dynamic ||
                    currAttr.Description.SelectionMode == DBAttrDescription.SelectionModeEnum.Selection) {
                    valueCell = new DBComboBoxCell();
                    ((DBComboBoxCell)valueCell).SetCustomChoices(currAttr.Description.PossibleValues);
                } else
                    valueCell = new DBTextBoxCell();

                valueCell.Table = typeof(DBAttribute);
                valueCell.DatabaseFieldName = "Value";
                
                switch (currAttr.Description.ValueType) {
                    case DBAttrDescription.ValueTypeEnum.INT:
                        valueCell.DBTypeOverride = DBField.DBDataType.INTEGER;
                        break;
                    case DBAttrDescription.ValueTypeEnum.FLOAT:
                        valueCell.DBTypeOverride = DBField.DBDataType.REAL;
                        break;
                    case DBAttrDescription.ValueTypeEnum.BOOL:
                        valueCell.DBTypeOverride = DBField.DBDataType.BOOL;
                        break;
                    case DBAttrDescription.ValueTypeEnum.STRING:
                        valueCell.DBTypeOverride = DBField.DBDataType.TEXT;
                        break;
                }
                
                valueCell.DatabaseObject = currAttr;
                currRow.Cells["valueColumn"] = (DataGridViewCell)valueCell;

                currRow.Cells["fieldColumn"].Value = currAttr.Description.Name;
            }
        }
        #endregion

        private void toolStripButton1_Click(object sender, EventArgs e) {
            AttributeTypeEditor popup = new AttributeTypeEditor();
            popup.ShowDialog();
        }

        private void toolStripButton3_Click(object sender, EventArgs e) {
            if (grid.SelectedCells.Count == 0)
                return;

            AttributeTypeEditor popup = new AttributeTypeEditor();
            DBAttrDescription attr = ((DBAttribute) grid.Rows[grid.SelectedCells[0].RowIndex].Tag).Description;
            popup.Attribute = attr;           
            popup.ShowDialog();
        }
    }
}
