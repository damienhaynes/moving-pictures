using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using MediaPortal.Plugins.MovingPictures.Database;
using System.Drawing;

namespace MediaPortal.Plugins.MovingPictures.ConfigScreen.Controls {
    public class DBTextBox: TextBox {
        #region Properties
        
        public DBField DatabaseField {
            get {
                return _databaseField;
            }
            set {
                _databaseField = value;
                updateText();
            }
        } private DBField _databaseField = null;

        public DatabaseTable DatabaseObject {
            get {
                return _databaseObject;
            }
            set {
                _databaseObject = value;
                updateText();
            }
        } private DatabaseTable _databaseObject = null;

        #endregion

        private object oldValue;

        #region Public Methods

        public DBTextBox() {
            BackColor = System.Drawing.SystemColors.Control;
            BorderStyle = System.Windows.Forms.BorderStyle.None;
            //Font = new System.Drawing.Font("Palatino Linotype", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));

            TextChanged += new System.EventHandler(DBTextBox_TextChanged);
            Leave += new System.EventHandler(DBTextBox_Leave);
            Enter += new System.EventHandler(DBTextBox_Enter);
        }

        ~DBTextBox() {
            Sync();
        }
       
        public bool IsValid() {
            if (DatabaseField == null || DatabaseObject == null)
                return false;

            try {
                switch (DatabaseField.DBType) {
                    case DBField.DBDataType.BOOL:
                        bool.Parse(Text);
                        break;
                    case DBField.DBDataType.INTEGER:
                        int.Parse(Text);
                        break;
                    case DBField.DBDataType.REAL:
                        float.Parse(Text);
                        break;
                }

                return true;
            }
            catch (Exception) {
                return false;
            }
        }

        public void Sync() {
            if (IsValid())
                DatabaseField.SetValue(DatabaseObject, Text);
            else
                Text = DatabaseField.GetValue(DatabaseObject).ToString();
        }

        #endregion

        private void updateText() {
            if (DatabaseField == null || DatabaseObject == null) {
                Text = "";
                return;
            }

            Text = DatabaseField.GetValue(DatabaseObject).ToString();
        }

        private void DBTextBox_TextChanged(object sender, EventArgs e) {
            if(IsValid())
                ForeColor = DefaultForeColor;
            else
                ForeColor = Color.Red;
        }

        private void DBTextBox_Enter(object sender, EventArgs e) {
            BorderStyle = BorderStyle.FixedSingle;
            BackColor = System.Drawing.SystemColors.Window;
        }

        private void DBTextBox_Leave(object sender, EventArgs e) {
            BorderStyle = BorderStyle.None;
            BackColor = System.Drawing.SystemColors.Control;

            Sync();
        }

    }
}
