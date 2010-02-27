using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel;
using Cornerstone.GUI.DesignMode;
using Cornerstone.Database.Tables;
using Cornerstone.Database;

namespace Cornerstone.GUI.Filtering {
    public class CriteriaListPanel : UserControl, IGenericCriteriaListPanel {
        private Control genericCriteriaListPanel;

        public CriteriaListPanel() {
            InitializeComponent();
        }

        // The database object type that this object displays data about.
        [Category("Cornerstone")]
        [Description("The datatype operated on by the Criteria this list panel displays.")]
        [TypeConverter(typeof(DatabaseTableTypeConverter))]
        public Type Table {
            get {
                return _table;
            }
            set {
                if (_table != value) {
                    _table = value;

                    InitializeComponent();
                }
            }
        } private Type _table = null;

        #region IGenericCriteriaListPanel Members

        public DatabaseManager DBManager {
            get { return ((IGenericCriteriaListPanel)genericCriteriaListPanel).DBManager; }
            set { ((IGenericCriteriaListPanel)genericCriteriaListPanel).DBManager = value; } 
        }

        public IGenericFilter SelectedCriteria {
            get { return ((IGenericCriteriaListPanel)genericCriteriaListPanel).SelectedCriteria; }
        }

        public void AddCriteria(IGenericFilter criteria) {
            if (genericCriteriaListPanel != null) {
                ((IGenericCriteriaListPanel)genericCriteriaListPanel).AddCriteria(criteria);
            }
        }

        public void RemoveSelectedCriteria() {
            if (genericCriteriaListPanel != null) {
                ((IGenericCriteriaListPanel)genericCriteriaListPanel).RemoveSelectedCriteria();
            }
        }

        public void ClearCriteria() {
            ((IGenericCriteriaListPanel)genericCriteriaListPanel).ClearCriteria();
        }

        #endregion

        private void InitializeComponent() {
            // determine the type the filter applies to
            Type filterType = typeof(DatabaseTable);
            if (_table != null) filterType = _table;

            // create an instance of the filter panel specific to the type we are filtering
            genericCriteriaListPanel = null;
            Type genericType = typeof(GenericCriteriaListPanel<>).GetGenericTypeDefinition();
            Type specificType = genericType.MakeGenericType(new Type[] { filterType });
            genericCriteriaListPanel = (Control)specificType.GetConstructor(Type.EmptyTypes).Invoke(null);
            genericCriteriaListPanel.Dock = System.Windows.Forms.DockStyle.Fill;

            SuspendLayout();
            Controls.Clear();
            Controls.Add(genericCriteriaListPanel);
            this.BorderStyle = BorderStyle.None;

            Name = "CriteriaListPanel";
            Size = new System.Drawing.Size(406, 57);

            ResumeLayout(false);
        }
    }
}
