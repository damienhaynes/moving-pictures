using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Cornerstone.Database.Tables;
using Cornerstone.Database;

namespace Cornerstone.GUI.Filtering {
    public partial class GenericCriteriaListPanel<T> : UserControl, IGenericCriteriaListPanel
        where T : DatabaseTable {

        private CriteriaPanel<T> selectedItem = null;
        private Dictionary<IGenericFilter,CriteriaPanel<T>> itemLookup = new Dictionary<IGenericFilter,CriteriaPanel<T>>();

        [Browsable(false)]
        public DatabaseManager DBManager {
            get {
                return _dbManager;
            }
            set {
                _dbManager = value;
            }
        } private DatabaseManager _dbManager;

        public IGenericFilter SelectedCriteria {
            get {
                if (selectedItem == null)
                    return null;

                return selectedItem.Criteria;
            }
        } 

        public GenericCriteriaListPanel() {
            InitializeComponent();
        }

        public void AddCriteria(IGenericFilter criteria) {
            DBCriteria<T> newCriteria = (DBCriteria<T>)criteria;

            CriteriaPanel<T> newPanel = new CriteriaPanel<T>();
            listPanel.Controls.Add(newPanel);
            newPanel.DBManager = DBManager;
            newPanel.Selected += ItemSelected;
            newPanel.Criteria = newCriteria;
            itemLookup[newCriteria] = newPanel;
        }

        public void RemoveSelectedCriteria() {
            if (selectedItem != null) {
                CriteriaPanel<T> removedItem = selectedItem;
                selectedItem = null;
                listPanel.Controls.Remove(removedItem);
            }
        }

        public void ClearCriteria() {
            foreach (CriteriaPanel<T> currPanel in itemLookup.Values) 
                listPanel.Controls.Remove(currPanel);

            itemLookup.Clear();
        }

        private void ItemSelected(CriteriaPanel<T> obj) {
            selectedItem = obj;
        }
    }

    public interface IGenericCriteriaListPanel {
        DatabaseManager DBManager { get; set; }

        IGenericFilter SelectedCriteria { get; } 

        void AddCriteria(IGenericFilter criteria);
        void RemoveSelectedCriteria();
        void ClearCriteria();
    }
}
