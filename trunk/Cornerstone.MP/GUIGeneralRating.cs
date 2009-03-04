using System;
using System.Collections.Generic;
using System.Text;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using NLog;
using System.ComponentModel;
using Cornerstone.MP;

namespace MediaPortal.Plugins.MovingPictures.MainUI {
    public class GUIGeneralRating : GUIDialogWindow {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        public const int ID = 28380;

        public GUIGeneralRating() {
            GetID = ID;
        }

        [SkinControlAttribute(6)]
        protected GUILabelControl lblText = null;
        [SkinControlAttribute(100)]
        protected GUIToggleButtonControl btnStar1 = null;
        [SkinControlAttribute(101)]
        protected GUIToggleButtonControl btnStar2 = null;
        [SkinControlAttribute(102)]
        protected GUIToggleButtonControl btnStar3 = null;
        [SkinControlAttribute(103)]
        protected GUIToggleButtonControl btnStar4 = null;
        [SkinControlAttribute(104)]
        protected GUIToggleButtonControl btnStar5 = null;

        public string Text {
            get {
                return lblText.Label;
            }

            set {
                LoadSkin();
                AllocResources();
                InitControls();

                lblText.Label = value;
            }
        }
        public int Rating { get; set; }
        public bool IsSubmitted { get; set; }

        public override void Reset() {
            base.Reset();

            SetHeading("");
            SetLine(1, "");
            SetLine(2, "");
            SetLine(3, "");
        }

        public override bool Init() {
            return Load(GUIGraphicsContext.Skin + @"\dialogGeneralRating.xml");
        }

        public override void OnAction(Action action) {
            switch (action.wID) {
                case Action.ActionType.REMOTE_1:
                    Rating = 1;
                    UpdateRating();
                    break;
                case Action.ActionType.REMOTE_2:
                    Rating = 2;
                    UpdateRating();
                    break;
                case Action.ActionType.REMOTE_3:
                    Rating = 3;
                    UpdateRating();
                    break;
                case Action.ActionType.REMOTE_4:
                    Rating = 4;
                    UpdateRating();
                    break;
                case Action.ActionType.REMOTE_5:
                    Rating = 5;
                    UpdateRating();
                    break;
                case Action.ActionType.ACTION_SELECT_ITEM:
                    IsSubmitted = true;
                    PageDestroy();
                    return;
                case Action.ActionType.ACTION_PREVIOUS_MENU:
                case Action.ActionType.ACTION_CLOSE_DIALOG:
                case Action.ActionType.ACTION_CONTEXT_MENU:
                    IsSubmitted = false;
                    PageDestroy();
                    return;
            }

            base.OnAction(action);
        }



        protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType) {
            base.OnClicked(controlId, control, actionType);
            if (control == btnStar1) {
                Rating = 1;
                IsSubmitted = true;
                PageDestroy();
                return;
            }
            else if (control == btnStar2) {
                Rating = 2;
                IsSubmitted = true;
                PageDestroy();
                return;
            }
            else if (control == btnStar3) {
                Rating = 3;
                IsSubmitted = true;
                PageDestroy();
                return;
            }
            else if (control == btnStar4) {
                Rating = 4;
                IsSubmitted = true;
                PageDestroy();
                return;
            }
            else if (control == btnStar5) {
                Rating = 5;
                IsSubmitted = true;
                PageDestroy();
                return;
            }
        }

        public override bool OnMessage(GUIMessage message) {
            switch (message.Message) {
                case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT:
                    base.OnMessage(message);
                    IsSubmitted = false;
                    UpdateRating();
                    return true;

                case GUIMessage.MessageType.GUI_MSG_SETFOCUS:
                    if (message.TargetControlId < 100 || message.TargetControlId > 105)
                        break;

                    Rating = message.TargetControlId - 99;
                    UpdateRating();
                    break;
            }
            return base.OnMessage(message);
        }

        private void UpdateRating() {
            GUIToggleButtonControl[] btnStars = new GUIToggleButtonControl[5] { btnStar1, btnStar2, btnStar3, btnStar4, btnStar5 };
            for (int i = 0; i < 5; i++) {
                btnStars[i].Selected = (Rating >= i + 1);
            }
            btnStars[Rating - 1].Focus = true;
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
