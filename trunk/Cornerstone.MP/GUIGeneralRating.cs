using System;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using NLog;

namespace Cornerstone.MP {
    public class GUIGeneralRating : GUIDialogWindow {
        private static Logger logger = LogManager.GetCurrentClassLogger();		
        public const int ID = 28380;

        public GUIGeneralRating() {
            GetID = ID;
        }
	
		public enum StarDisplay {
			FIVE_STARS = 5,
			TEN_STARS = 10
		}

        [SkinControlAttribute(6)]
        protected GUILabelControl lblText = null;
		[SkinControlAttribute(7)]
		protected GUILabelControl lblRating = null;
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
		[SkinControlAttribute(105)]
		protected GUIToggleButtonControl btnStar6 = null;
		[SkinControlAttribute(106)]
		protected GUIToggleButtonControl btnStar7 = null;
		[SkinControlAttribute(107)]
		protected GUIToggleButtonControl btnStar8 = null;
		[SkinControlAttribute(108)]
		protected GUIToggleButtonControl btnStar9 = null;
		[SkinControlAttribute(109)]
		protected GUIToggleButtonControl btnStar10 = null;

		#region properties
        public string Text {
            get {
                return lblText.Label;
            }

            set {
                lblText.Label = value;
            }
        }

		public StarDisplay DisplayStars {
			get {
				return _displayStars;
			}
			set {
				_displayStars = value;
			}
		} public StarDisplay _displayStars = StarDisplay.FIVE_STARS;

        public int Rating { get; set; }		
        public bool IsSubmitted { get; set; }

		#region Rate Description Properties
		public string FiveStarRateOneDesc { get; set; }
		public string FiveStarRateTwoDesc { get; set; }
		public string FiveStarRateThreeDesc { get; set; }
		public string FiveStarRateFourDesc { get; set; }
		public string FiveStarRateFiveDesc { get; set; }

		public string TenStarRateOneDesc { get; set; }
		public string TenStarRateTwoDesc { get; set; }
		public string TenStarRateThreeDesc { get; set; }
		public string TenStarRateFourDesc { get; set; }
		public string TenStarRateFiveDesc { get; set; }
		public string TenStarRateSixDesc { get; set; }
		public string TenStarRateSevenDesc { get; set; }
		public string TenStarRateEightDesc { get; set; }
		public string TenStarRateNineDesc { get; set; }
		public string TenStarRateTenDesc { get; set; }
		#endregion

		#endregion

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
			UpdateStarVisibility();

			base.DoModal(ParentID);
		}

        public override bool Init() {
            return Load(GUIGraphicsContext.Skin + @"\dialogGeneralRating.xml");
        }

        public override void OnAction(MediaPortal.GUI.Library.Action action) {
            switch (action.wID) {
                case MediaPortal.GUI.Library.Action.ActionType.REMOTE_1:
                    Rating = 1;
                    UpdateRating();
                    break;
                case MediaPortal.GUI.Library.Action.ActionType.REMOTE_2:
                    Rating = 2;
                    UpdateRating();
                    break;
                case MediaPortal.GUI.Library.Action.ActionType.REMOTE_3:
                    Rating = 3;
                    UpdateRating();
                    break;
                case MediaPortal.GUI.Library.Action.ActionType.REMOTE_4:
                    Rating = 4;
                    UpdateRating();
                    break;
                case MediaPortal.GUI.Library.Action.ActionType.REMOTE_5:
                    Rating = 5;
                    UpdateRating();
                    break;
				case MediaPortal.GUI.Library.Action.ActionType.REMOTE_6:
					if (DisplayStars == StarDisplay.FIVE_STARS) break;
					Rating = 6;
					UpdateRating();
					break;
				case MediaPortal.GUI.Library.Action.ActionType.REMOTE_7:
					if (DisplayStars == StarDisplay.FIVE_STARS) break;
					Rating = 7;
					UpdateRating();
					break;
				case MediaPortal.GUI.Library.Action.ActionType.REMOTE_8:
					if (DisplayStars == StarDisplay.FIVE_STARS) break;
					Rating = 8;
					UpdateRating();
					break;
				case MediaPortal.GUI.Library.Action.ActionType.REMOTE_9:
					if (DisplayStars == StarDisplay.FIVE_STARS) break;
					Rating = 9;
					UpdateRating();
					break;
				case MediaPortal.GUI.Library.Action.ActionType.REMOTE_0:
					if (DisplayStars == StarDisplay.FIVE_STARS) break;
					Rating = 10;
					UpdateRating();
					break;
                case MediaPortal.GUI.Library.Action.ActionType.ACTION_SELECT_ITEM:
                    IsSubmitted = true;
                    PageDestroy();
                    return;
                case MediaPortal.GUI.Library.Action.ActionType.ACTION_PREVIOUS_MENU:
                case MediaPortal.GUI.Library.Action.ActionType.ACTION_CLOSE_DIALOG:
                case MediaPortal.GUI.Library.Action.ActionType.ACTION_CONTEXT_MENU:
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
			else if (control == btnStar6) {
				Rating = 6;
				IsSubmitted = true;
				PageDestroy();
				return;
			} 
			else if (control == btnStar7) {
				Rating = 7;
				IsSubmitted = true;
				PageDestroy();
				return;
			} 
			else if (control == btnStar8) {
				Rating = 8;
				IsSubmitted = true;
				PageDestroy();
				return;
			} 
			else if (control == btnStar9) {
				Rating = 9;
				IsSubmitted = true;
				PageDestroy();
				return;
			} 
			else if (control == btnStar10) {
				Rating = 10;
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
                    if (message.TargetControlId < 100 || message.TargetControlId > (100 + (int)DisplayStars))
                        break;

                    Rating = message.TargetControlId - 99;
                    UpdateRating();
                    break;
            }
            return base.OnMessage(message);
        }

        private void UpdateRating() {
			GUIToggleButtonControl[] btnStars;
			if (DisplayStars == StarDisplay.FIVE_STARS) {
				btnStars = new GUIToggleButtonControl[5] { btnStar1, btnStar2, btnStar3, btnStar4, btnStar5 };
			} else {
				btnStars = new GUIToggleButtonControl[10] { btnStar1, btnStar2, btnStar3, btnStar4, btnStar5,
															btnStar6, btnStar7, btnStar8, btnStar9, btnStar10 };
			}

            for (int i = 0; i < (int)DisplayStars; i++) {
                btnStars[i].Selected = (Rating >= i + 1);
            }
            btnStars[Rating - 1].Focus = true;

			// Display Rating Description
			if (lblRating != null) {			
				lblRating.Label = string.Format("({0}) {1} / {2}", GetRatingDescription(), Rating.ToString(), (int)DisplayStars);				
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
		
		private void UpdateStarVisibility() {

			// Check skin supports 10 stars, if not fallback to 5 stars
			if (btnStar10 == null && DisplayStars == StarDisplay.TEN_STARS)
				DisplayStars = StarDisplay.FIVE_STARS;

			// Hide star controls 6-10
			if (DisplayStars == StarDisplay.FIVE_STARS) {
				if (btnStar6 != null) btnStar6.Visible = false;
				if (btnStar7 != null) btnStar7.Visible = false;
				if (btnStar8 != null) btnStar8.Visible = false;
				if (btnStar9 != null) btnStar9.Visible = false;
				if (btnStar10 != null) btnStar10.Visible = false;
			}
		}

		private string GetRatingDescription() {

			string description = string.Empty;

			if (DisplayStars == StarDisplay.FIVE_STARS) {
				switch (Rating) {
					case 1:
						description = FiveStarRateOneDesc;
						break;
					case 2:
						description = FiveStarRateTwoDesc;
						break;
					case 3:
						description = FiveStarRateThreeDesc;
						break;
					case 4:
						description = FiveStarRateFourDesc;
						break;
					case 5:
						description = FiveStarRateFiveDesc;
						break;
				}
			} 
			else {
				switch (Rating) {
					case 1:
						description = TenStarRateOneDesc;
						break;
					case 2:
						description = TenStarRateTwoDesc;
						break;
					case 3:
						description = TenStarRateThreeDesc;
						break;
					case 4:
						description = TenStarRateFourDesc;
						break;
					case 5:
						description = TenStarRateFiveDesc;
						break;
					case 6:
						description = TenStarRateSixDesc;
						break;
					case 7:
						description = TenStarRateSevenDesc;
						break;
					case 8:
						description = TenStarRateEightDesc;
						break;
					case 9:
						description = TenStarRateNineDesc;
						break;
					case 10:
						description = TenStarRateTenDesc;
						break;
				}
			}
			return description;
		}

    }
}
