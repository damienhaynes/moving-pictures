using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Cornerstone.Database.Tables;
using Cornerstone.GUI.Filtering;
using System.ComponentModel;
using Cornerstone.Database;

namespace Cornerstone.GUI.Controls {
    public class FilterComboBox : ComboBox, IMessageFilter {

        private bool stateChanging = false;

        [ReadOnly(true)]
        public IDBMenu Menu {
            get { return _menu; }
            set { 
                _menu = value;

                BuildTreePanel();
            }
        }  IDBMenu _menu;

        [ReadOnly(true)]
        public IDBNode SelectedNode {
            get { return _selectedNode; }
            set {
                _selectedNode = value;
                if (!stateChanging) _treePanel.SelectedNode = value;
                
                string name = value == null ? "..." : _treePanel.TranslationParser(_treePanel.SelectedNode.Name);
                this.Items.Clear();
                this.Items.Add(name);
                this.SelectedItem = name;
            }
        } protected IDBNode _selectedNode;

        public MenuTreePanel TreePanel {
            get { return _treePanel; }
        } protected MenuTreePanel _treePanel;

        public new bool DroppedDown {
            get { return _treePanel.Visible; }
        } 

        public FilterComboBox() : base() {
            _treePanel = new MenuTreePanel();
            _treePanel.SelectedNodeChanged += new DBNodeEventHandler(SelectedNodeChanged);

            DropDownStyle = ComboBoxStyle.DropDownList;
        }

        protected void DBManager_ObjectUpdated(DatabaseTable obj) {
            if (InvokeRequired) {
                Invoke(new MethodInvoker(delegate() { DBManager_ObjectUpdated(obj); }));
                return;
            }

            if (obj == Menu)
                BuildTreePanel();
        }

        protected void SelectedNodeChanged(IDBNode node) {
            if (stateChanging)
                return;

            if (_treePanel.SelectedNode != null &&
                !_treePanel.SelectedNode.HasChildren &&
                _treePanel.SelectedNode.HasFilter) {

                stateChanging = true;
                SelectedNode = _treePanel.SelectedNode;
                stateChanging = false;

                HideTreePanel();
            }

            stateChanging = false;
        }

        protected override void WndProc(ref Message m) {
            if ((m.Msg == 0x0201) || (m.Msg == 0x0203)) {
                if (DroppedDown)
                    HideTreePanel();
                else {
                    ShowTreePanel();
                }
            }
            else {
                base.WndProc(ref m);
            }
        }

        public bool PreFilterMessage(ref Message m) {
            // intercept mouse events
            if ((m.Msg >= 0x0200) && (m.Msg <= 0x020A)) {
                // clicks inside the user control, handle normally
                if (_treePanel.RectangleToScreen(_treePanel.DisplayRectangle).Contains(Cursor.Position)) 
                    return false;
                else {
                    // clicks outside the user control collapse it.
                    if ((m.Msg == 0x0201) || (m.Msg == 0x0203))
                        HideTreePanel();
                    return true;
                }
            }
            else return false;
        }

        private void BuildTreePanel() {
            if (_menu == null || stateChanging)
                return;

            stateChanging = true;

            // reset the tree and populate it with the menu
            Type filterType = _menu.GetType().GetGenericArguments()[0];
            _treePanel.FieldDisplaySettings.Table = filterType;
            _treePanel.InitializeComponent();
            _treePanel.DBManager = ((DatabaseTable)_menu).DBManager;
            _treePanel.Menu = _menu;
            _treePanel.IsEditable = false;
            _treePanel.Width = 200;
            _treePanel.Height = 150;
            _treePanel.Visible = false;
            
            stateChanging = false;
        }

        protected void ShowTreePanel() {
            if (!this.Visible || stateChanging)
                return;

            stateChanging = true;

            _treePanel.RepopulateTree();
            _treePanel.SelectedNode = _selectedNode;

            _treePanel.Anchor = this.Anchor;
            _treePanel.Font = this.Font;
            _treePanel.Top = this.Bottom;
            _treePanel.Left = this.Left;
            _treePanel.Width = Math.Max(_treePanel.Width, this.Width);

            Parent.Controls.Add(_treePanel);
            _treePanel.Visible = true;
            _treePanel.BringToFront();
            
            stateChanging = false;

            Application.AddMessageFilter(this);
            base.OnDropDown(EventArgs.Empty);
        }

        protected void HideTreePanel() {
            if (stateChanging)
                return;

            stateChanging = true;

            Application.RemoveMessageFilter(this);
            base.OnDropDownClosed(EventArgs.Empty);
            _treePanel.Visible = false;
            Parent.Controls.Remove(_treePanel);

            this.Focus();

            stateChanging = false;
            
        }
    }
}
