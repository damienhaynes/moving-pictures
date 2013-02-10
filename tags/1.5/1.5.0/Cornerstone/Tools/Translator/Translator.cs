using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Cornerstone.Tools;
using System.Threading;

namespace Cornerstone.Tools.Translate {
    public class Translator {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public bool Debug {
            get { return _debug; }
            set { _debug = value; }
        } private bool _debug = false;

        /// <summary>
        /// Language that string is in before translation.
        /// </summary>
        public TranslatorLanguage FromLanguage {
            get { return _fromLanguage; }
            set { _fromLanguage = value; }
        } private TranslatorLanguage _fromLanguage;

        /// <summary>
        /// Language that string will be translated into.
        /// </summary>
        public TranslatorLanguage ToLanguage {
            get { return _toLanguage; }
            set { _toLanguage = value; }
        } private TranslatorLanguage _toLanguage = TranslatorLanguage.English;

        private string _strLanguagePare = String.Empty;


        private void BuildLanguagePair() {

            //builds url encoded language pare for translation eg: en|es for English to Spanish
            _strLanguagePare = LanguageUtility.GetLanguageCode(_fromLanguage) + "%7C" + LanguageUtility.GetLanguageCode(_toLanguage);

            if (_debug) logger.Debug("Translation tool: Language Pair: {0}", _strLanguagePare);
        }

        public string Translate(string input) {            
            //if To and From languages are the same, do nothing.
            if (_fromLanguage == _toLanguage) 
                return input;

            if (input == String.Empty) 
                return input;

            if (_fromLanguage == TranslatorLanguage.Unknown) {
                DetectLanguage(input);
            }
            
            BuildLanguagePair();
            
            //The StringSplit funtion fills a List<string> (_strToTranslate) with strings that are no more than 500 characters long.
            List<string> inputList = SplitAndEncode(input);

            string output = string.Empty;
            foreach (string currStr in inputList) {
                output += GetTranslation(currStr);
            }

            return output;
        }

        //function processes a string to translate.
        private string GetTranslation(string toTrans) {
            string translatedString = String.Empty;
            string url = String.Format("http://ajax.googleapis.com/ajax/services/language/translate?v=1.0&langpair={0}&q={1}", _strLanguagePare, toTrans);

            WebGrabber webGrabber = new WebGrabber(url);
            webGrabber.GetResponse();
            translatedString = webGrabber.GetString();

            if (_debug) logger.Debug("Translation tool: Uncleaned Translation: {0}", translatedString);
            // remove all json from responce and return only translated string.
            if (translatedString.Length > 0) { translatedString = ResponseClean(translatedString); }

            return translatedString;
        }

        //function that will be called to attempt to determine the from languange if none is given.
        private void DetectLanguage(string input) {
            try {
                Regex reg = new Regex(@"language"".""(?<languageCode>[^""]+)");
                string url = String.Format("http://ajax.googleapis.com/ajax/services/language/detect?v=1.0&q={0}", input);
                WebGrabber webGrabber = new WebGrabber(url);
                webGrabber.GetResponse();
                string result = webGrabber.GetString();
                Match match = reg.Match(result);
                string matched = match.Groups["languageCode"].Value;
                _fromLanguage = LanguageUtility.GetLanguage(matched);
                if (_debug) logger.Debug("Translation tool: Detect Language: Source: {0} : Detected Language: {1}", result, LanguageUtility.ToString(_fromLanguage));
            }
            catch (Exception e) {
                if (e is ThreadAbortException) throw e;

                _fromLanguage = LanguageUtility.GetLanguage("en");
            }
        }

        //function used to clean the json responce from Google Translate.
        private string ResponseClean(string response) {
            Regex reg = new Regex(@"translatedText.{3}(?<translatedText>[^}]+)""}");
            Match match = reg.Match(response);
            string cleanedResponse = match.Groups["translatedText"].Value;
            cleanedResponse = HttpUtility.HtmlDecode(cleanedResponse.Replace(@"\u0026", "&"));
            cleanedResponse = cleanedResponse.Replace(@"\", "");
            if (cleanedResponse.Contains("detectedSourceLanguage")) {
                cleanedResponse = cleanedResponse.Remove(cleanedResponse.LastIndexOf("detectedSourceLanguage") - 3);
            }
            if (_debug) logger.Debug("Translation tool: Cleaned Translation: {0}", cleanedResponse);
            return cleanedResponse;
        }

        // This function takes the string given to be translated, and if it is over 500 characters, 
        // it will split it into seperate string for translation.
        private List<string> SplitAndEncode(string input) {
            List<string> output = new List<string>();

            int groups = (int)Math.Ceiling((double)input.Length / 500);
            if (groups > 1) {
                Regex reg = new Regex(@"([^.?!]+[.?!])");
                MatchCollection matchC = reg.Matches(input);
                string temp = String.Empty;
                foreach (Match mtch in matchC) {
                    if (mtch.Length + temp.Length < 500) {
                        temp += mtch.Value;
                    }
                    else {
                        output.Add(HttpUtility.UrlEncode(temp));
                        temp = mtch.Value;
                    }
                }
                if (temp.Length > 0) { output.Add(HttpUtility.UrlEncode(temp)); }

            }
            else { output.Add(HttpUtility.UrlEncode(input)); }

            return output;
        }
    }
}
