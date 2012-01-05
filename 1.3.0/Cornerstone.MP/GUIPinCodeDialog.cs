using System;
using System.Collections.Generic;
using System.Text;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using System.ComponentModel;
using System.Threading;

namespace Cornerstone.MP {
    public class GUIPinCodeDialog : GUIDialogWindow {
        public const int ID = 9915;

        public GUIPinCodeDialog() {
            GetID = ID;
        }

        [SkinControlAttribute(6)]
        protected GUILabelControl labelFeedback = null;

        [SkinControlAttribute(100)]
        protected GUIImage imagePin1 = null;
        [SkinControlAttribute(101)]
        protected GUIImage imagePin2 = null;
        [SkinControlAttribute(102)]
        protected GUIImage imagePin3 = null;
        [SkinControlAttribute(103)]
        protected GUIImage imagePin4 = null;

        public string EnteredPinCode { get; set; }
        public string MasterCode { get; set; }
        public bool IsCorrect { get; set; }

        /// <summary>
        /// Message reported to use when Pin is incorrect
        /// </summary>
        public string InvalidPinMessage { get; set; }

        public override void Reset() {
            base.Reset();

            SetHeading("");
            SetLine(1, "");
            SetLine(2, "");
            SetLine(3, "");
            SetLine(4, "");
        }

        public override void DoModal(int ParentID) {
            LoadSkin();
            AllocResources();
            InitControls();
            ClearPinCode();

            EnteredPinCode = "";

            base.DoModal(ParentID);
        }

        public override bool Init() {
            return Load(GUIGraphicsContext.Skin + @"\DialogPinCode.xml");
        }

        public override void OnAction(MediaPortal.GUI.Library.Action action) {
             switch (action.wID) {
                case MediaPortal.GUI.Library.Action.ActionType.REMOTE_1:
                    if (EnteredPinCode.Length >= 4) return;
                    EnteredPinCode = EnteredPinCode + "1";
                    UpdatePinCode(EnteredPinCode.Length);
                    break;
                case MediaPortal.GUI.Library.Action.ActionType.REMOTE_2:
                    if (EnteredPinCode.Length >= 4) return;
                    EnteredPinCode = EnteredPinCode + "2";
                    UpdatePinCode(EnteredPinCode.Length);
                    break;
                case MediaPortal.GUI.Library.Action.ActionType.REMOTE_3:
                    if (EnteredPinCode.Length >= 4) return;
                    EnteredPinCode = EnteredPinCode + "3";
                    UpdatePinCode(EnteredPinCode.Length);
                    break;
                case MediaPortal.GUI.Library.Action.ActionType.REMOTE_4:
                    if (EnteredPinCode.Length >= 4) return;
                    EnteredPinCode = EnteredPinCode + "4";
                    UpdatePinCode(EnteredPinCode.Length);
                    break;
                case MediaPortal.GUI.Library.Action.ActionType.REMOTE_5:
                    if (EnteredPinCode.Length >= 4) return;
                    EnteredPinCode = EnteredPinCode + "5";
                    UpdatePinCode(EnteredPinCode.Length);
                    break;
                case MediaPortal.GUI.Library.Action.ActionType.REMOTE_6:
                    if (EnteredPinCode.Length >= 4) return;
                    EnteredPinCode = EnteredPinCode + "6";
                    UpdatePinCode(EnteredPinCode.Length);
                    break;
                case MediaPortal.GUI.Library.Action.ActionType.REMOTE_7:
                    if (EnteredPinCode.Length >= 4) return;
                    EnteredPinCode = EnteredPinCode + "7";
                    UpdatePinCode(EnteredPinCode.Length);
                    break;
                case MediaPortal.GUI.Library.Action.ActionType.REMOTE_8:
                    if (EnteredPinCode.Length >= 4) return;
                    EnteredPinCode = EnteredPinCode + "8";
                    UpdatePinCode(EnteredPinCode.Length);
                    break;
                case MediaPortal.GUI.Library.Action.ActionType.REMOTE_9:
                    if (EnteredPinCode.Length >= 4) return;
                    EnteredPinCode = EnteredPinCode + "9";
                    UpdatePinCode(EnteredPinCode.Length);
                    break;
                case MediaPortal.GUI.Library.Action.ActionType.REMOTE_0:
                    if (EnteredPinCode.Length >= 4) return;
                    EnteredPinCode = EnteredPinCode + "0";
                    UpdatePinCode(EnteredPinCode.Length);
                    break;
                case MediaPortal.GUI.Library.Action.ActionType.ACTION_KEY_PRESSED:
                    // some types of remotes send ACTION_KEY_PRESSED instead of REMOTE_0 - REMOTE_9 commands
                    if (EnteredPinCode.Length >= 4) return;
                    if (action.m_key != null && action.m_key.KeyChar >= '0' && action.m_key.KeyChar <= '9') {
                        char key = (char)action.m_key.KeyChar;
                        EnteredPinCode = EnteredPinCode + key;
                        UpdatePinCode(EnteredPinCode.Length);
                    }
                    break;
                case MediaPortal.GUI.Library.Action.ActionType.ACTION_DELETE_ITEM:
                case MediaPortal.GUI.Library.Action.ActionType.ACTION_MOVE_DOWN:
                case MediaPortal.GUI.Library.Action.ActionType.ACTION_MOVE_LEFT:
                    if (EnteredPinCode.Length > 0) {
                        EnteredPinCode = EnteredPinCode.Substring(0, EnteredPinCode.Length - 1);
                        UpdatePinCode(EnteredPinCode.Length);
                    }
                    break;
                case MediaPortal.GUI.Library.Action.ActionType.ACTION_SELECT_ITEM:
                    PageDestroy();
                    return;
                case MediaPortal.GUI.Library.Action.ActionType.ACTION_PREVIOUS_MENU:
                case MediaPortal.GUI.Library.Action.ActionType.ACTION_CLOSE_DIALOG:
                case MediaPortal.GUI.Library.Action.ActionType.ACTION_CONTEXT_MENU:
                    PageDestroy();
                    return;
            }

            base.OnAction(action);
        }

        protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType) {
            base.OnClicked(controlId, control, actionType);
        }

        public override bool OnMessage(GUIMessage message) {
            return base.OnMessage(message);
        }

        private void UpdatePinCode(int pinLength) {
            GUIImage[] imagePins = new GUIImage[4] { imagePin1, imagePin2, imagePin3, imagePin4 };

            // Visually indicate to user the number of digits entered
            ClearPinCode();
            for (int i = 0; i < pinLength; i++) {
                imagePins[i].Visible = true;
            }

            // Check if PinCode entered is correct
            if (pinLength == 4) {
                ConfirmPinCode();
            }

        }

        private void ClearPinCode() {
            GUIImage[] imagePins = new GUIImage[4] { imagePin1, imagePin2, imagePin3, imagePin4 };

            for (int i = 0; i < 4; i++) {
                imagePins[i].Visible = false;
            }

            IsCorrect = false;
            if (labelFeedback != null) labelFeedback.Label = " ";
        }

        private void ConfirmPinCode() {
            // Show Feedback to user that PinCode is incorrect
            // otherwise nothing more to do, exit
            if (EnteredPinCode != MasterCode) {
                labelFeedback.Label = InvalidPinMessage;
            }
            else {
                IsCorrect = true;

                // delay shutting down the dialog so the user gets visual confirmation of the last input
                ThreadStart actions = delegate {
                    Thread.Sleep(500);
                    PageDestroy();
                };

                Thread thread = new Thread(actions);
                thread.Start();
                return;
            }
        }

        public void SetHeading(string HeadingLine) {
            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_SET, GetID, 0, 1, 0, 0, null);
            msg.Label = HeadingLine;
            OnMessage(msg);
        }

        public void SetLine(int LineNr, string Line) {
            if (LineNr < 1) return;
            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_SET, GetID, 0, 1 + LineNr, 0, 0, null);
            msg.Label = Line;
            if ((msg.Label == string.Empty) || (msg.Label == "")) msg.Label = "  ";
            OnMessage(msg);
        }

    }
}