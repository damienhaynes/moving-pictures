using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Design;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using MediaPortal.Plugins.MovingPictures.ConfigScreen.Controls;
using System.ComponentModel.Design;

namespace MediaPortal.Plugins.MovingPictures.ConfigScreen.DesignMode {
    internal partial class DBObjectListFieldEditorDialog : Form {
        #region Private Variables

        DBObjectList instance;
        
        #endregion

        public DBObjectListFieldEditorDialog() {
            InitializeComponent();
        }

        public DBObjectListFieldEditorDialog(DBObjectList instance) {
            InitializeComponent();

            this.instance = instance;

            // populate field list
            foreach (FieldProperty currProperties in instance.FieldProperties) {
                ListViewItem newItem = new ListViewItem(currProperties.FieldName);
                newItem.Tag = currProperties;
                newItem.Checked = currProperties.Visible;
                if (!currProperties.Visible)
                    newItem.ForeColor = System.Drawing.SystemColors.GrayText;

                fieldList.Items.Add(newItem);
            }
                

        }

        public List<FieldProperty> GetFieldList() {
            List<FieldProperty> rtn = new List<FieldProperty>();
            foreach (ListViewItem currItem in fieldList.Items) {
                rtn.Add((FieldProperty)currItem.Tag);
            }

            return rtn;
        }

        private void updateList() {
            foreach (ListViewItem currItem in fieldList.Items) {
                if (((FieldProperty)currItem.Tag).Visible) {
                    currItem.ForeColor = System.Drawing.SystemColors.WindowText;
                    currItem.Checked = true;
                }
                else {
                    currItem.ForeColor = System.Drawing.SystemColors.GrayText;
                    currItem.Checked = false;
                }
            }
        }

        private void fieldList_SelectedIndexChanged(object sender, EventArgs e) {
            if (fieldList.SelectedItems.Count == 0 || !(fieldList.SelectedItems[0].Tag is FieldProperty))
                return;

            FieldProperty properties = (FieldProperty)fieldList.SelectedItems[0].Tag;
            propertyGrid.SelectedObject = properties;
        }

        private void fieldList_ItemChecked(object sender, ItemCheckedEventArgs e) {
            ((FieldProperty)e.Item.Tag).Visible = e.Item.Checked;
            updateList();

            // select the clicked item
            fieldList.SelectedItems.Clear();
            e.Item.Selected = true;
        }

        private void propertyGrid_PropertyValueChanged(object s, PropertyValueChangedEventArgs e) {
            updateList();
        }


    }

    internal class DBObjectListFieldEditor : UITypeEditor {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context) {
            return UITypeEditorEditStyle.Modal;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value) {
            // Access the Property Browser's UI display service
            IWindowsFormsEditorService editorService = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
            DBObjectList instance = ((DBObjectList)context.Instance);

            // display
            DBObjectListFieldEditorDialog dialog = new DBObjectListFieldEditorDialog(instance);
            editorService.ShowDialog(dialog);

            // update screen and return results
            instance.GenerateRows();
            return dialog.GetFieldList();
        }
    }

    internal class DBObjectListDesigner : ControlDesigner {
        public override DesignerVerbCollection Verbs {
            get {
                if (_verbs == null) {
                    // Create and initialize the collection of verbs
                    _verbs = new DesignerVerbCollection();
                    _verbs.Add(new DesignerVerb("Edit Rows", new EventHandler(OnEditRowsSelected)));
                }
                return _verbs;
            }
        } private DesignerVerbCollection _verbs;

        void OnEditRowsSelected(object sender, EventArgs args) {
            DBObjectList instance = ((DBObjectList)Control);

            DBObjectListFieldEditorDialog dialog = new DBObjectListFieldEditorDialog(instance);
            dialog.ShowDialog();

            // Change property value
            PropertyDescriptor property = TypeDescriptor.GetProperties(typeof(DBObjectList))["FieldProperties"];
            RaiseComponentChanging(property);
            instance.FieldProperties = dialog.GetFieldList();
            RaiseComponentChanged(property, null, instance.FieldProperties);

            // update screen
            instance.GenerateRows();

        }
    }
}
