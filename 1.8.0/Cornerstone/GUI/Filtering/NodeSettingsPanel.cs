using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Cornerstone.GUI.Controls;
using System.ComponentModel;
using Cornerstone.Database.Tables;
using Cornerstone.Database;

namespace Cornerstone.GUI.Filtering {
    public class NodeSettingsPanel : UserControl, IFieldDisplaySettingsOwner, INodeSettingsPanel {
        Control genericNodeSettingsPanel;

        #region INodeSettingsPanel Members

        [Category("Cornerstone Settings")]
        [Description("The friendly name for the type of objects being filtered (plural).")]
        public string DisplayName {
            get { return _displayName; }
            set {
                _displayName = value;
                
                if (genericNodeSettingsPanel != null)
                    ((INodeSettingsPanel)genericNodeSettingsPanel).DisplayName = value;
            }
        } private string _displayName = "items";

        [Browsable(false)]
        public DatabaseManager DBManager {
            get {
                return _dbManager;
            }
            set {
                _dbManager = value;

                if (genericNodeSettingsPanel != null)
                    ((INodeSettingsPanel)genericNodeSettingsPanel).DBManager = value;
            }
        } private DatabaseManager _dbManager;

        public IDBNode Node {
            get {
                return ((INodeSettingsPanel)genericNodeSettingsPanel).Node;
            }
            set {
                if (genericNodeSettingsPanel != null)
                    ((INodeSettingsPanel)genericNodeSettingsPanel).Node = value;
            }
        }

        [ReadOnly(true)]
        public TranslationParserDelegate TranslationParser {
            get { return ((INodeSettingsPanel)genericNodeSettingsPanel).TranslationParser; }
            set { ((INodeSettingsPanel)genericNodeSettingsPanel).TranslationParser = value; }
        }

        [ReadOnly(true)]
        public bool ShowFilterHelpButton {
            get { return ((INodeSettingsPanel)genericNodeSettingsPanel).ShowFilterHelpButton; }
            set { ((INodeSettingsPanel)genericNodeSettingsPanel).ShowFilterHelpButton = value; }
        }

        [ReadOnly(true)]
        public HelpActionDelegate FilterHelpAction {
            get { return ((INodeSettingsPanel)genericNodeSettingsPanel).FilterHelpAction; }
            set { ((INodeSettingsPanel)genericNodeSettingsPanel).FilterHelpAction = value; }
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

                if (genericNodeSettingsPanel != null) {
                    ((IFieldDisplaySettingsOwner)genericNodeSettingsPanel).FieldDisplaySettings = value;
                    ((IFieldDisplaySettingsOwner)genericNodeSettingsPanel).FieldDisplaySettings.Owner = this;
                }

                InitializeComponent();
            }
        } private FieldDisplaySettings _fieldSettings = null;

        public void OnFieldPropertiesChanged() {
            InitializeComponent();

            if (genericNodeSettingsPanel != null)
                ((IFieldDisplaySettingsOwner)genericNodeSettingsPanel).OnFieldPropertiesChanged();
        }

        #endregion


        private void InitializeComponent() {
            // determine the type the filter applies to
            Type filterType = typeof(DatabaseTable);
            if (_fieldSettings != null && _fieldSettings.Table != null) filterType = _fieldSettings.Table;

            // create an instance of the filter panel specific to the type we are filtering
            this.genericNodeSettingsPanel = null;
            Type genericType = typeof(GenericNodeSettingsPanel<>).GetGenericTypeDefinition();
            Type specificType = genericType.MakeGenericType(new Type[] { filterType });
            genericNodeSettingsPanel = (Control)specificType.GetConstructor(Type.EmptyTypes).Invoke(null);
            genericNodeSettingsPanel.Dock = System.Windows.Forms.DockStyle.Fill;

            // link new control to existing settings
            ((IFieldDisplaySettingsOwner)genericNodeSettingsPanel).FieldDisplaySettings = FieldDisplaySettings;
            ((INodeSettingsPanel)genericNodeSettingsPanel).DisplayName = _displayName;
            ((INodeSettingsPanel)genericNodeSettingsPanel).DBManager = _dbManager;

            SuspendLayout();
            Controls.Clear();
            Controls.Add(this.genericNodeSettingsPanel);

            Name = "NodeSettingsPanel";
            Size = new System.Drawing.Size(689, 364);

            ResumeLayout(false);
        }
    }
}
