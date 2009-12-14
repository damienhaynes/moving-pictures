using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel;
using Cornerstone.GUI.Controls;
using Cornerstone.Database.Tables;
using Cornerstone.Database;
using System.Drawing;

namespace Cornerstone.GUI.Filtering {
    public class MenuTreePanel : UserControl, IMenuTreePanel, IFieldDisplaySettingsOwner, IDisposable {

        private Control genericMenuTreePanel;

        public MenuTreePanel() {
            InitializeComponent();
        }


        #region IDisposable Members

        void IDisposable.Dispose() {
            genericMenuTreePanel.Dispose();
        }

        #endregion

        #region IMenuTreePanel Members

        public event DBNodeEventHandler SelectedNodeChanged;

        [Browsable(false)]
        public DatabaseManager DBManager {
            get { return ((IMenuTreePanel)genericMenuTreePanel).DBManager; }
            set { ((IMenuTreePanel)genericMenuTreePanel).DBManager = value; }
        } 

        [ReadOnly(true)]
        public IDBMenu Menu {
            get { return ((IMenuTreePanel)genericMenuTreePanel).Menu; }
            set { ((IMenuTreePanel)genericMenuTreePanel).Menu = value; }
        }

        [ReadOnly(true)]
        public TranslationParserDelegate TranslationParser {
            get { return ((IMenuTreePanel)genericMenuTreePanel).TranslationParser; }
            set { ((IMenuTreePanel)genericMenuTreePanel).TranslationParser = value; }
        } 

        #endregion

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

                if (genericMenuTreePanel != null) {
                    ((IFieldDisplaySettingsOwner)genericMenuTreePanel).FieldDisplaySettings = value;
                    ((IFieldDisplaySettingsOwner)genericMenuTreePanel).FieldDisplaySettings.Owner = this;
                }

                InitializeComponent();
            }
        } private FieldDisplaySettings _fieldSettings = null;

        public void OnFieldPropertiesChanged() {
            InitializeComponent();

            if (genericMenuTreePanel != null)
                ((IFieldDisplaySettingsOwner)genericMenuTreePanel).OnFieldPropertiesChanged();
        }

        #endregion

        public void InitializeComponent() {
            // determine the type the filter applies to
            Type filterType = typeof(DatabaseTable);
            if (_fieldSettings != null) filterType = _fieldSettings.Table;

            // create an instance of the filter panel specific to the type we are filtering
            genericMenuTreePanel = null;
            Type genericType = typeof(GenericMenuTreePanel<>).GetGenericTypeDefinition();
            Type specificType = genericType.MakeGenericType(new Type[] { filterType });
            genericMenuTreePanel = (Control)specificType.GetConstructor(Type.EmptyTypes).Invoke(null);
            genericMenuTreePanel.Dock = System.Windows.Forms.DockStyle.Fill;
            ((IMenuTreePanel)genericMenuTreePanel).SelectedNodeChanged += new DBNodeEventHandler(MenuTreePanel_SelectedNodeChanged);

            // link new control to existing settings
            ((IMenuTreePanel)genericMenuTreePanel).DBManager = DBManager;
            ((IFieldDisplaySettingsOwner)genericMenuTreePanel).FieldDisplaySettings = FieldDisplaySettings;

            SuspendLayout();
            Controls.Clear();
            Controls.Add(genericMenuTreePanel);

            Name = "FilterEditorPane";
            Size = new System.Drawing.Size(689, 364);

            ResumeLayout(false);
        }

        void MenuTreePanel_SelectedNodeChanged(IDBNode node, Type type) {
            if (SelectedNodeChanged != null)
                SelectedNodeChanged(node, type);
        }

    }
}
