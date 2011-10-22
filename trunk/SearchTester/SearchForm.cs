using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MediaPortal.Plugins.MovingPictures.Database;
using Cornerstone.Tools;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Lucene.Net.Index;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.QueryParsers;
using Cornerstone.Tools.Search;
using MediaPortal.Plugins.MovingPictures;
using SearchResult = Cornerstone.Tools.Search.LevenshteinSubstringSearcher<MediaPortal.Plugins.MovingPictures.Database.DBMovieInfo>.SearchResult;


namespace SearchTester {
    public partial class SearchForm : Form {
        List<DBMovieInfo> allMovies;

        Dictionary<SearchMode, LevenshteinSubstringSearcher<DBMovieInfo>> levenshtein;
        Dictionary<SearchMode, LuceneSearcher<DBMovieInfo>> lucene;

        public SearchMode SelectedMode {
            get {
                switch (searchModeComboBox.SelectedIndex) {
                    case 0:
                        return SearchMode.TITLE;
                    case 1:
                        return SearchMode.PERSON;
                    case 2:
                        return SearchMode.SUMMARY;
                    default:
                        return SearchMode.TITLE;
                }
            }
        }

        public SearchForm() {
            InitializeComponent();

            allMovies = DBMovieInfo.GetAll();
            BuildIndexes();

            foreach(SearchMode mode in Enum.GetValues(typeof(SearchMode))) 
                searchModeComboBox.Items.Add(mode.GetName());
            searchModeComboBox.SelectedIndex = 0;
        }

        private void BuildIndexes() {
            levenshtein = new Dictionary<SearchMode, LevenshteinSubstringSearcher<DBMovieInfo>>();
            lucene = new Dictionary<SearchMode, LuceneSearcher<DBMovieInfo>>();

            List<string> titleFields = new List<string>();
            titleFields.Add("title");
            if (altTitleCheckBox.Checked) titleFields.Add("alternate_titles");

            string[] peopleFields = new string[] { "directors", "writers", "actors"};
            string[] themeFields = new string[] { "summary"};

            levenshtein[SearchMode.TITLE] = new LevenshteinSubstringSearcher<DBMovieInfo>(MovingPicturesCore.DatabaseManager, titleFields.ToArray());
            levenshtein[SearchMode.TITLE].BuildDynamicIndex();

            levenshtein[SearchMode.PERSON] = new LevenshteinSubstringSearcher<DBMovieInfo>(MovingPicturesCore.DatabaseManager, peopleFields);
            levenshtein[SearchMode.PERSON].BuildDynamicIndex();

            levenshtein[SearchMode.SUMMARY] = new LevenshteinSubstringSearcher<DBMovieInfo>(MovingPicturesCore.DatabaseManager, themeFields);
            levenshtein[SearchMode.SUMMARY].BuildDynamicIndex();

            lucene[SearchMode.TITLE] = new LuceneSearcher<DBMovieInfo>(MovingPicturesCore.DatabaseManager, titleFields.ToArray());
            lucene[SearchMode.TITLE].BuildDynamicIndex();

            lucene[SearchMode.PERSON] = new LuceneSearcher<DBMovieInfo>(MovingPicturesCore.DatabaseManager, peopleFields);
            lucene[SearchMode.PERSON].BuildDynamicIndex();

            lucene[SearchMode.SUMMARY] = new LuceneSearcher<DBMovieInfo>(MovingPicturesCore.DatabaseManager, themeFields);
            lucene[SearchMode.SUMMARY].BuildDynamicIndex();
        }

        private void PerformSearch() {
            DateTime start = DateTime.Now;
            List<SearchResult> results = levenshtein[SelectedMode].Search(searchTextBox.Text);
            TimeSpan processing = DateTime.Now - start;

            processingTimeLabel.Text = string.Format("{0:0} ms", processing.TotalMilliseconds);
            PopulateLevResults(results);


            start = DateTime.Now;
            results = lucene[SelectedMode].Search(searchTextBox.Text);
            processing = DateTime.Now - start;

            luceneTimeLabel.Text = string.Format("{0:0} ms", processing.TotalMilliseconds);
            PopulateLuceneResults(results);

            searchTextBox.SelectAll();
        }

        private void PopulateLevResults(List<SearchResult> results) {
            resultsListView.Items.Clear();

            foreach (SearchResult currResult in results) {
                ListViewItem listItem = new ListViewItem(new string[] { currResult.Item.Title, currResult.Score.ToString() });
                listItem.Tag = currResult;
                if (currResult.Score <= 1) listItem.ForeColor = Color.Green;
                if (currResult.Score > 2) listItem.ForeColor = Color.LightGray;

                resultsListView.Items.Add(listItem);
            }
        }


        private void PopulateLuceneResults(List<SearchResult> results) {
            luceneListView.Items.Clear();

            foreach (SearchResult currResult in results) {
                ListViewItem listItem = new ListViewItem(new string[] { currResult.Item.Title, currResult.Score.ToString() });
                listItem.Tag = currResult;
                if (currResult.Score == 1) listItem.ForeColor = Color.Green;
                //if (currResult.Score > 2) listItem.ForeColor = Color.LightGray;

                luceneListView.Items.Add(listItem);
            }
        }

        private void searchButton_Click(object sender, EventArgs e) {
            PerformSearch();
        }

        private void resultsListView_DoubleClick(object sender, EventArgs e) {
            ListViewItem listItem = ((ListView)sender).FocusedItem;
            SearchResult searchResult = (SearchResult)listItem.Tag;
            Process.Start(searchResult.Item.LocalMedia[0].FullPath);
        }

        private void searchModeComboBox_SelectedIndexChanged(object sender, EventArgs e) {
            altTitleCheckBox.Enabled = searchModeComboBox.SelectedIndex == (int)SearchMode.TITLE;
        }

        private void altTitleCheckBox_CheckedChanged(object sender, EventArgs e) {
            BuildIndexes();
        }
    }

    public enum SearchMode { TITLE = 0, PERSON, SUMMARY }
    public static class SearchModeExtensions {
        public static string GetName(this SearchMode self) {
            switch (self) {
                case SearchMode.TITLE:
                    return "Title";
                case SearchMode.PERSON:
                    return "Cast and Crew";
                case SearchMode.SUMMARY:
                    return "Theme";
            }

            return "???";
        }
    }
}
