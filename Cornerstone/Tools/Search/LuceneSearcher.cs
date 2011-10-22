using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cornerstone.Database.Tables;
using Cornerstone.Database;
using Lucene.Net.Analysis;
using Lucene.Net.Store;
using Lucene.Net.Index;
using Lucene.Net.Analysis.Standard;
using System.Text.RegularExpressions;
using Lucene.Net.Documents;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;

namespace Cornerstone.Tools.Search {
    public class LuceneSearcher<T>: AbstractSearcher<T> where T: DatabaseTable {

        private static Regex cleaner = new Regex(@"[^\w\s]", RegexOptions.IgnoreCase);

        private Directory directory;
        private Analyzer analyzer;
        private IndexWriter writer;
        private MultiFieldQueryParser parser;

        public LuceneSearcher(DatabaseManager db, ICollection<DBField> fields): 
            base(db, fields) {
            Init();
        }

        public LuceneSearcher(DatabaseManager db, string[] fieldNames) :
            base(db, fieldNames) {
            Init();
        }

        private void Init() {
            directory = new RAMDirectory();
            analyzer = new StandardAnalyzer();
            writer = new IndexWriter(directory, analyzer);


            int i = 0;
            string[] fieldList = new string[SearchFields.Count];
            foreach (DBField currField in SearchFields) {
                fieldList[i++] = currField.FieldName;
            }

            parser = new MultiFieldQueryParser(fieldList, analyzer);
        }

        public override void BuildIndex(List<T> items) {
            base.BuildIndex(items);
            writer.Optimize();
            writer.Flush();
        }

        public override void BuildDynamicIndex() {
            base.BuildDynamicIndex();
            writer.Optimize();
            writer.Flush();
            writer.Close();
        }

        public override void AddRange(ICollection<T> items) {
            base.AddRange(items);
            writer.Optimize();
            writer.Flush();
        }
        
        public override void Add(T item) {
            Document doc = new Document();
            doc.Add(new Field("id", item.ID.ToString(), Field.Store.YES, Field.Index.NO));

            foreach (DBField currField in SearchFields) {
                object value = currField.GetValue(item);
                if (value == null) continue;

                doc.Add(new Field(currField.FieldName, cleaner.Replace(value.ToString().Replace("|", " "), "").ToLower().Trim(), Field.Store.YES, Field.Index.TOKENIZED));
            }

            writer.AddDocument(doc);   
        }

        public override void Remove(T item) {
            writer.DeleteDocuments(new Term("id", item.ID.ToString()));
        }

        public override List<SearchResult> Search(string searchStr) {
            List<SearchResult> results = new List<SearchResult>();

            string cleanSearchStr = cleaner.Replace(searchStr, "").ToLower().Trim();

            IndexSearcher searcher = new IndexSearcher(directory);
            //QueryParser parser = new QueryParser("title", analyzer);
            //Query query = parser.Parse(cleanSearchStr + "~0.7");

            Query query = parser.Parse(cleanSearchStr + "~0.7");
            Hits hits = searcher.Search(query);

            int resultCount = hits.Length();
            for (int i = 0; i < resultCount; i++) {
                SearchResult result = new SearchResult();
                result.Item = DatabaseManager.Get<T>(int.Parse(hits.Doc(i).Get("id")));
                result.Score = hits.Score(i);

                results.Add(result);
            }

            return results;
        }

        public override void Clear() {
            Init();
        }
    }
}
