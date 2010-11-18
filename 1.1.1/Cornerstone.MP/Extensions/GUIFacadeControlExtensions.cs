using MediaPortal.GUI.Library;
using NLog;

namespace Cornerstone.MP.Extensions {

    /// <summary>
    /// a set of extension methods for the MediaPortal GUIFacadeControl
    /// </summary>
    public static class GUIFacadeControlExtensions {

        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Get a value indicating wether the given control is related to this facade
        /// </summary>
        /// <param name="self"></param>
        /// <param name="parent">GUIControl instance to check</param>
        /// <returns>True if the control is related</returns>
        public static bool IsRelated(this GUIFacadeControl self, GUIControl parent) {
            return (parent == self || parent == self.FilmstripView || parent == self.ThumbnailView || parent == self.ListView || parent == self.AlbumListView);
        }

        /// <summary>
        /// Performs clear on the facade and all children lists
        /// </summary>
        /// <param name="self"></param>
        public static void ClearAll(this GUIFacadeControl self) {
            self.Clear();
            if (self.ListView != null) self.ListView.Clear();
            if (self.ThumbnailView != null) self.ThumbnailView.Clear();
            if (self.FilmstripView != null) self.FilmstripView.Clear();
            if (self.AlbumListView != null) self.AlbumListView.Clear();
        }

        /// <summary>
        /// Sets the specified focus property on the facade and all children lists
        /// </summary>
        /// <param name="self"></param>
        /// <param name="value"></param>
        public static void Focus(this GUIFacadeControl self, bool value) {
            self.Focus = value;
            if (self.ListView != null) self.ListView.Focus = value;
            if (self.ThumbnailView != null) self.ThumbnailView.Focus = value;
            if (self.AlbumListView != null) self.AlbumListView.Focus = value;
            if (self.FilmstripView != null) self.FilmstripView.Focus = value;
        }  

        /// <summary>
        /// Sets the specified visible property on the facade and all children lists
        /// </summary>
        /// <param name="self"></param>
        /// <param name="value"></param>
        public static void Visible(this GUIFacadeControl self, bool value) {
            self.Visible = value;
            if (self.ListView != null) self.ListView.Visible = value;
            if (self.ThumbnailView != null) self.ThumbnailView.Visible = value;
            if (self.AlbumListView != null) self.AlbumListView.Visible = value;
            if (self.FilmstripView != null) self.FilmstripView.Visible = value;
        }

        /// <summary>
        /// Selects the requested, or if not available the first, object of the specified type in the facade
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <param name="item">the object to select or null </param>
        /// <returns>selected object</returns>
        public static T SyncToFacade<T>(this GUIFacadeControl self, T item) where T : class {
            int i = 0;
            return self.SyncToFacade<T>(item, out i);
        }

        /// <summary>
        /// Selects the requested, or if not available the first, object of the specified type in the facade
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <param name="item">the object to select or null </param>
        /// <param name="selectedIndex">the index of the selection</param>
        /// <returns>selected object</returns>
        public static T SyncToFacade<T>(this GUIFacadeControl self, T item, out int selectedIndex) where T : class {
            lock (self) {

                object selectedItem = null;
                selectedIndex = -1;

                // no use in syncing when we got no items
                if (self.Count == 0)
                    return null;                

                // Check if the current selected item already is the item we want
                // if so we don't have to do the routine check
                if (item != null & self.SelectedListItem != null && self.SelectedListItem.TVTag is T) {
                    if (self.SelectedListItem.TVTag == item) {
                        selectedItem = self.SelectedListItem.TVTag;
                        selectedIndex = self.SelectedListItemIndex;
                    }
                }

                // Find the item in the facade and mark the first item found
                if (selectedItem == null) {
                    for (int i = 0; i < self.Count; i++) {
                        if (self[i].TVTag is T) {
                            // mark the first item found
                            if (selectedIndex == -1) {
                                selectedIndex = i;
                            }
                            // if we found the item or had no selection break the loop;
                            if (item == null || item == self[i].TVTag) {
                                selectedIndex = i;
                                selectedItem = self[i].TVTag;
                                break;
                            }
                        }
                    }

                    // if no item was found during the iteration we use the first item found
                    if (selectedItem == null) {
                        if (selectedIndex == -1)
                            return null;

                        selectedItem = self[selectedIndex].TVTag;
                    }

                    // select the item in the facade
                    if (self.SelectedListItemIndex != selectedIndex) {
                        logger.Debug("SyncToFacade<{0}>: Index={1}, Item={2}", typeof(T).Name, selectedIndex, selectedItem);
                        self.SelectIndex(selectedIndex);
                    }
                }

                // return the (new) selected item
                return selectedItem as T;
            }
        }

        /// <summary>
        /// Gets a value indicating wether the item is selected in the facade
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <param name="item"></param>
        /// <returns>True if the item is selected</returns>
        public static bool IsSelected<T>(this GUIFacadeControl self, T item) where T : class {
            if (item != null & self.SelectedListItem != null && self.SelectedListItem.TVTag is T) {
                return (self.SelectedListItem.TVTag == item);
            }

            return false;
        }

        /// <summary>
        /// Selects the specified item in the facade
        /// </summary>
        /// <param name="self"></param>
        /// <param name="index">index of the item</param>
        public static void SelectIndex(this GUIFacadeControl self, int index) {
            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECT, self.WindowId, 0, self.GetID, index, 0, null);
            GUIGraphicsContext.SendMessage(msg);
        }

    }

}
