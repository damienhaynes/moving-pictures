using System.Collections.Generic;

namespace Cornerstone.Tools.Translate
{
    /// <summary>
    /// Utility class for language and language codes.
    /// </summary>
    public static class LanguageUtility
    {
        private static readonly IDictionary<TranslatorLanguage, string> s_LanguageCodeDict;

        private static readonly IList<TranslatorLanguage> s_TranslatableList;

        private static readonly IDictionary<TranslatorLanguage, System.Text.Encoding> s_LanguangeEncoderList;

        static LanguageUtility()
        {
            s_LanguageCodeDict = new Dictionary<TranslatorLanguage, string>();
            s_LanguageCodeDict[TranslatorLanguage.Unknown] = string.Empty;
            s_LanguageCodeDict[TranslatorLanguage.Afrikaans] = "af";
            s_LanguageCodeDict[TranslatorLanguage.Albanian] = "sq";
            s_LanguageCodeDict[TranslatorLanguage.Amharic] = "am";
            s_LanguageCodeDict[TranslatorLanguage.Arabic] = "ar";
            s_LanguageCodeDict[TranslatorLanguage.Armenian] = "hy";
            s_LanguageCodeDict[TranslatorLanguage.Azerbaijani] = "az";
            s_LanguageCodeDict[TranslatorLanguage.Basque] = "eu";
            s_LanguageCodeDict[TranslatorLanguage.Belarusian] = "be";
            s_LanguageCodeDict[TranslatorLanguage.Bengali] = "bn";
            s_LanguageCodeDict[TranslatorLanguage.Bihari] = "bh";
            s_LanguageCodeDict[TranslatorLanguage.Bulgarian] = "bg";
            s_LanguageCodeDict[TranslatorLanguage.Burmese] = "my";
            s_LanguageCodeDict[TranslatorLanguage.Catalan] = "ca";
            s_LanguageCodeDict[TranslatorLanguage.Cherokee] = "chr";
            s_LanguageCodeDict[TranslatorLanguage.Chinese] = "zh";
            s_LanguageCodeDict[TranslatorLanguage.ChineseSimplified] = "zh-CN";
            s_LanguageCodeDict[TranslatorLanguage.ChineseTraditional] = "zh-TW";
            s_LanguageCodeDict[TranslatorLanguage.Croatian] = "hr";
            s_LanguageCodeDict[TranslatorLanguage.Czech] = "cs";
            s_LanguageCodeDict[TranslatorLanguage.Danish] = "da";
            s_LanguageCodeDict[TranslatorLanguage.Dhivehi] = "dv";
            s_LanguageCodeDict[TranslatorLanguage.Dutch] = "nl";
            s_LanguageCodeDict[TranslatorLanguage.English] = "en";
            s_LanguageCodeDict[TranslatorLanguage.Esperanto] = "eo";
            s_LanguageCodeDict[TranslatorLanguage.Estonian] = "et";
            s_LanguageCodeDict[TranslatorLanguage.Filipino] = "tl";
            s_LanguageCodeDict[TranslatorLanguage.Finnish] = "fi";
            s_LanguageCodeDict[TranslatorLanguage.French] = "fr";
            s_LanguageCodeDict[TranslatorLanguage.Galician] = "gl";
            s_LanguageCodeDict[TranslatorLanguage.Georgian] = "ka";
            s_LanguageCodeDict[TranslatorLanguage.German] = "de";
            s_LanguageCodeDict[TranslatorLanguage.Greek] = "el";
            s_LanguageCodeDict[TranslatorLanguage.Guarani] = "gn";
            s_LanguageCodeDict[TranslatorLanguage.Gujarati] = "gu";
            s_LanguageCodeDict[TranslatorLanguage.Hebrew] = "iw";
            s_LanguageCodeDict[TranslatorLanguage.Hindi] = "hi";
            s_LanguageCodeDict[TranslatorLanguage.Hungarian] = "hu";
            s_LanguageCodeDict[TranslatorLanguage.Icelandic] = "is";
            s_LanguageCodeDict[TranslatorLanguage.Indonesian] = "id";
            s_LanguageCodeDict[TranslatorLanguage.Inuktitut] = "iu";
            s_LanguageCodeDict[TranslatorLanguage.Italian] = "it";
            s_LanguageCodeDict[TranslatorLanguage.Japanese] = "ja";
            s_LanguageCodeDict[TranslatorLanguage.Kannada] = "kn";
            s_LanguageCodeDict[TranslatorLanguage.Kazakh] = "kk";
            s_LanguageCodeDict[TranslatorLanguage.Khmer] = "km";
            s_LanguageCodeDict[TranslatorLanguage.Korean] = "ko";
            s_LanguageCodeDict[TranslatorLanguage.Kurdish] = "ku";
            s_LanguageCodeDict[TranslatorLanguage.Kyrgyz] = "ky";
            s_LanguageCodeDict[TranslatorLanguage.Laothian] = "lo";
            s_LanguageCodeDict[TranslatorLanguage.Latvian] = "lv";
            s_LanguageCodeDict[TranslatorLanguage.Lithuanian] = "lt";
            s_LanguageCodeDict[TranslatorLanguage.Macedonian] = "mk";
            s_LanguageCodeDict[TranslatorLanguage.Malay] = "ms";
            s_LanguageCodeDict[TranslatorLanguage.Malayalam] = "ml";
            s_LanguageCodeDict[TranslatorLanguage.Maltese] = "mt";
            s_LanguageCodeDict[TranslatorLanguage.Marathi] = "mr";
            s_LanguageCodeDict[TranslatorLanguage.Mongolian] = "mn";
            s_LanguageCodeDict[TranslatorLanguage.Nepali] = "ne";
            s_LanguageCodeDict[TranslatorLanguage.Norwegian] = "no";
            s_LanguageCodeDict[TranslatorLanguage.Oriya] = "or";
            s_LanguageCodeDict[TranslatorLanguage.Pashto] = "ps";
            s_LanguageCodeDict[TranslatorLanguage.Persian] = "fa";
            s_LanguageCodeDict[TranslatorLanguage.Polish] = "pl";
            s_LanguageCodeDict[TranslatorLanguage.Portuguese] = "pt-PT";
            s_LanguageCodeDict[TranslatorLanguage.Punjabi] = "pa";
            s_LanguageCodeDict[TranslatorLanguage.Romanian] = "ro";
            s_LanguageCodeDict[TranslatorLanguage.Russian] = "ru";
            s_LanguageCodeDict[TranslatorLanguage.Sanskrit] = "sa";
            s_LanguageCodeDict[TranslatorLanguage.Serbian] = "sr";
            s_LanguageCodeDict[TranslatorLanguage.Sindhi] = "sd";
            s_LanguageCodeDict[TranslatorLanguage.Sinhalese] = "si";
            s_LanguageCodeDict[TranslatorLanguage.Slovak] = "sk";
            s_LanguageCodeDict[TranslatorLanguage.Slovenian] = "sl";
            s_LanguageCodeDict[TranslatorLanguage.Spanish] = "es";
            s_LanguageCodeDict[TranslatorLanguage.Swahili] = "sw";
            s_LanguageCodeDict[TranslatorLanguage.Swedish] = "sv";
            s_LanguageCodeDict[TranslatorLanguage.Tajik] = "tg";
            s_LanguageCodeDict[TranslatorLanguage.Tamil] = "ta";
            s_LanguageCodeDict[TranslatorLanguage.Tagalog] = "tl";
            s_LanguageCodeDict[TranslatorLanguage.Telugu] = "te";
            s_LanguageCodeDict[TranslatorLanguage.Thai] = "th";
            s_LanguageCodeDict[TranslatorLanguage.Tibetan] = "bo";
            s_LanguageCodeDict[TranslatorLanguage.Turkish] = "tr";
            s_LanguageCodeDict[TranslatorLanguage.Ukrainian] = "uk";
            s_LanguageCodeDict[TranslatorLanguage.Urdu] = "ur";
            s_LanguageCodeDict[TranslatorLanguage.Uzbek] = "uz";
            s_LanguageCodeDict[TranslatorLanguage.Uighur] = "ug";
            s_LanguageCodeDict[TranslatorLanguage.Vietnamese] = "vi";

            s_TranslatableList = new TranslatorLanguage[]
                {
                    TranslatorLanguage.Albanian,
                    TranslatorLanguage.Arabic,
                    TranslatorLanguage.Bulgarian,
                    TranslatorLanguage.ChineseSimplified,
                    TranslatorLanguage.ChineseTraditional,
                    TranslatorLanguage.Catalan,
                    TranslatorLanguage.Croatian,
                    TranslatorLanguage.Czech,
                    TranslatorLanguage.Danish,
                    TranslatorLanguage.Dutch,
                    TranslatorLanguage.English,
                    TranslatorLanguage.Estonian,
                    TranslatorLanguage.Filipino,
                    TranslatorLanguage.Finnish,
                    TranslatorLanguage.French,
                    TranslatorLanguage.Galician,
                    TranslatorLanguage.German,
                    TranslatorLanguage.Greek,
                    TranslatorLanguage.Hebrew,
                    TranslatorLanguage.Hindi,
                    TranslatorLanguage.Hungarian,
                    TranslatorLanguage.Indonesian,
                    TranslatorLanguage.Italian,
                    TranslatorLanguage.Japanese,
                    TranslatorLanguage.Korean,
                    TranslatorLanguage.Latvian,
                    TranslatorLanguage.Lithuanian,
                    TranslatorLanguage.Maltese,
                    TranslatorLanguage.Norwegian,
                    TranslatorLanguage.Polish,
                    TranslatorLanguage.Portuguese,
                    TranslatorLanguage.Romanian,
                    TranslatorLanguage.Russian,
                    TranslatorLanguage.Spanish,
                    TranslatorLanguage.Serbian,
                    TranslatorLanguage.Slovak,
                    TranslatorLanguage.Slovenian,
                    TranslatorLanguage.Swedish,
                    TranslatorLanguage.Thai,
                    TranslatorLanguage.Turkish,
                    TranslatorLanguage.Ukrainian,
                    TranslatorLanguage.Vietnamese,
                };

            s_LanguangeEncoderList = new Dictionary<TranslatorLanguage, System.Text.Encoding>();
            s_LanguangeEncoderList[TranslatorLanguage.Albanian] = System.Text.Encoding.GetEncoding("ISO-8859-15");
            s_LanguangeEncoderList[TranslatorLanguage.Arabic] = System.Text.Encoding.GetEncoding("Windows-1256");
            s_LanguangeEncoderList[TranslatorLanguage.Bulgarian] = System.Text.Encoding.GetEncoding("Windows-1251");
            s_LanguangeEncoderList[TranslatorLanguage.ChineseSimplified] = System.Text.Encoding.GetEncoding("GB18030");
            s_LanguangeEncoderList[TranslatorLanguage.ChineseTraditional] = System.Text.Encoding.GetEncoding("Big5");
            s_LanguangeEncoderList[TranslatorLanguage.Catalan] = System.Text.Encoding.GetEncoding("ISO-8859-15");
            s_LanguangeEncoderList[TranslatorLanguage.Croatian] = System.Text.Encoding.GetEncoding("Windows-1250");
            s_LanguangeEncoderList[TranslatorLanguage.Czech] = System.Text.Encoding.GetEncoding("ISO-8859-15");
            s_LanguangeEncoderList[TranslatorLanguage.Danish] = System.Text.Encoding.GetEncoding("ISO-8859-15");
            s_LanguangeEncoderList[TranslatorLanguage.Dutch] = System.Text.Encoding.GetEncoding("ISO-8859-15");
            s_LanguangeEncoderList[TranslatorLanguage.English] = System.Text.Encoding.Default;
            s_LanguangeEncoderList[TranslatorLanguage.Estonian] = System.Text.Encoding.GetEncoding("Windows-1257");
            s_LanguangeEncoderList[TranslatorLanguage.Filipino] = System.Text.Encoding.Default;
            s_LanguangeEncoderList[TranslatorLanguage.Finnish] = System.Text.Encoding.GetEncoding("iso-8859-15");
            s_LanguangeEncoderList[TranslatorLanguage.French] = System.Text.Encoding.GetEncoding("ISO-8859-15");
            s_LanguangeEncoderList[TranslatorLanguage.Galician] = System.Text.Encoding.GetEncoding("ISO-8859-15");
            s_LanguangeEncoderList[TranslatorLanguage.German] = System.Text.Encoding.GetEncoding("ISO-8859-15");
            s_LanguangeEncoderList[TranslatorLanguage.Greek] = System.Text.Encoding.GetEncoding("Windows-1253");
            s_LanguangeEncoderList[TranslatorLanguage.Hebrew] = System.Text.Encoding.GetEncoding("Windows-1255");
            s_LanguangeEncoderList[TranslatorLanguage.Hindi] = System.Text.Encoding.UTF8;
            s_LanguangeEncoderList[TranslatorLanguage.Hungarian] = System.Text.Encoding.GetEncoding("Windows-1250");
            s_LanguangeEncoderList[TranslatorLanguage.Indonesian] = System.Text.Encoding.Default;
            s_LanguangeEncoderList[TranslatorLanguage.Italian] = System.Text.Encoding.GetEncoding("ISO-8859-15");
            s_LanguangeEncoderList[TranslatorLanguage.Japanese] = System.Text.Encoding.GetEncoding("ISO-2022-JP");
            s_LanguangeEncoderList[TranslatorLanguage.Korean] = System.Text.Encoding.GetEncoding("ISO-2022-KR");
            s_LanguangeEncoderList[TranslatorLanguage.Latvian] = System.Text.Encoding.GetEncoding("Windows-1257");
            s_LanguangeEncoderList[TranslatorLanguage.Lithuanian] = System.Text.Encoding.GetEncoding("Windows-1257");
            s_LanguangeEncoderList[TranslatorLanguage.Maltese] = System.Text.Encoding.UTF8;
            s_LanguangeEncoderList[TranslatorLanguage.Polish] = System.Text.Encoding.GetEncoding("Windows-1250");
            s_LanguangeEncoderList[TranslatorLanguage.Portuguese] = System.Text.Encoding.GetEncoding("ISO-8859-15");
            s_LanguangeEncoderList[TranslatorLanguage.Romanian] = System.Text.Encoding.GetEncoding("Windows-1250");
            s_LanguangeEncoderList[TranslatorLanguage.Russian] = System.Text.Encoding.GetEncoding("Windows-1251");
            s_LanguangeEncoderList[TranslatorLanguage.Spanish] = System.Text.Encoding.GetEncoding("ISO-8859-15");
            s_LanguangeEncoderList[TranslatorLanguage.Serbian] = System.Text.Encoding.GetEncoding("Windows-1250");
            s_LanguangeEncoderList[TranslatorLanguage.Slovak] = System.Text.Encoding.GetEncoding("Windows-1250");
            s_LanguangeEncoderList[TranslatorLanguage.Slovenian] = System.Text.Encoding.GetEncoding("Windows-1250");
            s_LanguangeEncoderList[TranslatorLanguage.Swedish] = System.Text.Encoding.GetEncoding("ISO-8859-15");
            s_LanguangeEncoderList[TranslatorLanguage.Thai] = System.Text.Encoding.GetEncoding("Windows-874");
            s_LanguangeEncoderList[TranslatorLanguage.Turkish] = System.Text.Encoding.GetEncoding("Windows-1254");
            s_LanguangeEncoderList[TranslatorLanguage.Ukrainian] = System.Text.Encoding.GetEncoding("Windows-1251");
            s_LanguangeEncoderList[TranslatorLanguage.Vietnamese] = System.Text.Encoding.GetEncoding("Windows-1258");




        }

        /// <summary>
        /// Get translatable language collection.
        /// </summary>
        public static ICollection<TranslatorLanguage> TranslatableCollection
        {
            get
            {
                return s_TranslatableList;
            }
        }

        /// <summary>
        /// Get language collection.
        /// </summary>
        public static ICollection<TranslatorLanguage> LanguageCollection
        {
            get
            {
                return LanguageCodeDict.Keys;
            }
        }

        /// <summary>
        /// Get language code dictionary.
        /// </summary>
        internal static IDictionary<TranslatorLanguage, string> LanguageCodeDict
        {
            get
            {
                return s_LanguageCodeDict;
            }
        }


        /// <summary>
        /// Whether this language is translatable.
        /// </summary>
        /// <param name="language">The language.</param>
        /// <returns>Return true if the language is translatable.</returns>
        public static bool IsTranslatable(TranslatorLanguage language)
        {
            return TranslatableCollection.Contains(language);
        }

        /// <summary>
        /// Returns the proper encoding type for language given.
        /// </summary>
        /// <param name="language"></param>
        /// <returns></returns>
        public static System.Text.Encoding GetEncoding(TranslatorLanguage language)
        {
            if (s_LanguangeEncoderList.ContainsKey(language))
            {
                return s_LanguangeEncoderList[language];
            }
            else
            {
                return System.Text.Encoding.Default;
            }
        }

        /// <summary>
        /// Get language from a language code.
        /// </summary>
        /// <param name="languageCode">The language code.</param>
        /// <returns>The language of this code or unknown language if of language match this code.</returns>
        internal static TranslatorLanguage GetLanguage(string languageCode)
        {
            languageCode = languageCode.Trim();
            if (string.IsNullOrEmpty(languageCode))
            {
                return TranslatorLanguage.Unknown;
            }
            foreach (KeyValuePair<TranslatorLanguage, string> pair in LanguageCodeDict)
            {
                if (languageCode == pair.Value)
                {
                    return pair.Key;
                }
            }
            if (string.Compare(languageCode, "zh-Hant", true) == 0)
            {
                return TranslatorLanguage.ChineseTraditional;
            }
            return TranslatorLanguage.Unknown;
        }


        public static string ToString(TranslatorLanguage language)
        {
            return language.ToString();
        }


        public static TranslatorLanguage FromString(string strLanguage)
        {
            ICollection<TranslatorLanguage> language = TranslatableCollection;
            IEnumerator<TranslatorLanguage> _enum = language.GetEnumerator();
            while (_enum.MoveNext())
            {
                if (_enum.Current.ToString().ToLower() == strLanguage.ToLower())
                {
                    return _enum.Current;
                }
            }
            return TranslatorLanguage.Unknown;
        }

        /// <summary>
        /// Get the language code of a language.
        /// </summary>
        /// <param name="language">The language</param>
        /// <returns>The language code of this language or code for unknown language.</returns>
        internal static string GetLanguageCode(TranslatorLanguage language)
        {
            string code;
            if (!LanguageCodeDict.TryGetValue(language, out code))
            {
                code = LanguageCodeDict[TranslatorLanguage.Unknown];
            }
            return code;
        }
    }

    /// <summary>
    /// The enum of languages.
    /// </summary>
    public enum TranslatorLanguage
    {
        /// <summary>
        /// Unknown. Default value.
        /// </summary>
        Unknown = 0,
        /// <summary>
        /// Afrikaans.
        /// </summary>
        Afrikaans,
        /// <summary>
        /// Albanian.
        /// </summary>
        Albanian,
        /// <summary>
        /// Amharic.
        /// </summary>
        Amharic,
        /// <summary>
        /// Arabic.
        /// </summary>
        Arabic,
        /// <summary>
        /// Armenian.
        /// </summary>
        Armenian,
        /// <summary>
        /// Azerbaijani.
        /// </summary>
        Azerbaijani,
        /// <summary>
        /// Basque.
        /// </summary>
        Basque,
        /// <summary>
        /// Belarusian.
        /// </summary>
        Belarusian,
        /// <summary>
        /// Bengali.
        /// </summary>
        Bengali,
        /// <summary>
        /// Bihari.
        /// </summary>
        Bihari,
        /// <summary>
        /// Bulgarian.
        /// </summary>
        Bulgarian,
        /// <summary>
        /// Burmese.
        /// </summary>
        Burmese,
        /// <summary>
        /// Catalan.
        /// </summary>
        Catalan,
        /// <summary>
        /// Cherokee.
        /// </summary>
        Cherokee,
        /// <summary>
        /// Chinese.
        /// </summary>
        Chinese,
        /// <summary>
        /// Chinese simplified.
        /// </summary>
        ChineseSimplified,
        /// <summary>
        /// Chinese traditional.
        /// </summary>
        ChineseTraditional,
        /// <summary>
        /// Croatian.
        /// </summary>
        Croatian,
        /// <summary>
        /// Czech.
        /// </summary>
        Czech,
        /// <summary>
        /// Danish.
        /// </summary>
        Danish,
        /// <summary>
        /// Dhivehi.
        /// </summary>
        Dhivehi,
        /// <summary>
        /// Dutch.
        /// </summary>
        Dutch,
        /// <summary>
        /// English.
        /// </summary>
        English,
        /// <summary>
        /// Esperanto.
        /// </summary>
        Esperanto,
        /// <summary>
        /// Estonian.
        /// </summary>
        Estonian,
        /// <summary>
        /// Filipino.
        /// </summary>
        Filipino,
        /// <summary>
        /// Finnish.
        /// </summary>
        Finnish,
        /// <summary>
        /// French.
        /// </summary>
        French,
        /// <summary>
        /// Galician.
        /// </summary>
        Galician,
        /// <summary>
        /// Georgian.
        /// </summary>
        Georgian,
        /// <summary>
        /// German.
        /// </summary>
        German,
        /// <summary>
        /// Greek.
        /// </summary>
        Greek,
        /// <summary>
        /// Guarani.
        /// </summary>
        Guarani,
        /// <summary>
        /// Gujarati.
        /// </summary>
        Gujarati,
        /// <summary>
        /// Hebrew.
        /// </summary>
        Hebrew,
        /// <summary>
        /// Hindi.
        /// </summary>
        Hindi,
        /// <summary>
        /// Hungarian.
        /// </summary>
        Hungarian,
        /// <summary>
        /// Icelandic.
        /// </summary>
        Icelandic,
        /// <summary>
        /// Indonesian.
        /// </summary>
        Indonesian,
        /// <summary>
        /// Inuktitut.
        /// </summary>
        Inuktitut,
        /// <summary>
        /// Italian.
        /// </summary>
        Italian,
        /// <summary>
        /// Japanese.
        /// </summary>
        Japanese,
        /// <summary>
        /// Kannada.
        /// </summary>
        Kannada,
        /// <summary>
        /// Kazakh.
        /// </summary>
        Kazakh,
        /// <summary>
        /// Khmer.
        /// </summary>
        Khmer,
        /// <summary>
        /// Korean.
        /// </summary>
        Korean,
        /// <summary>
        /// Kurdish.
        /// </summary>
        Kurdish,
        /// <summary>
        /// Kyrgyz.
        /// </summary>
        Kyrgyz,
        /// <summary>
        /// Laothian.
        /// </summary>
        Laothian,
        /// <summary>
        /// Latvian.
        /// </summary>
        Latvian,
        /// <summary>
        /// Lithuanian.
        /// </summary>
        Lithuanian,
        /// <summary>
        /// Macedonian.
        /// </summary>
        Macedonian,
        /// <summary>
        /// Malay.
        /// </summary>
        Malay,
        /// <summary>
        /// Malayalam.
        /// </summary>
        Malayalam,
        /// <summary>
        /// Maltese.
        /// </summary>
        Maltese,
        /// <summary>
        /// Marathi.
        /// </summary>
        Marathi,
        /// <summary>
        /// Mongolian.
        /// </summary>
        Mongolian,
        /// <summary>
        /// Nepali.
        /// </summary>
        Nepali,
        /// <summary>
        /// Norwegian.
        /// </summary>
        Norwegian,
        /// <summary>
        /// Oriya.
        /// </summary>
        Oriya,
        /// <summary>
        /// Pashto.
        /// </summary>
        Pashto,
        /// <summary>
        /// Persian.
        /// </summary>
        Persian,
        /// <summary>
        /// Polish.
        /// </summary>
        Polish,
        /// <summary>
        /// Portuguese.
        /// </summary>
        Portuguese,
        /// <summary>
        /// Punjabi.
        /// </summary>
        Punjabi,
        /// <summary>
        /// Romanian.
        /// </summary>
        Romanian,
        /// <summary>
        /// Russian.
        /// </summary>
        Russian,
        /// <summary>
        /// Sanskrit.
        /// </summary>
        Sanskrit,
        /// <summary>
        /// Serbian.
        /// </summary>
        Serbian,
        /// <summary>
        /// Sindhi.
        /// </summary>
        Sindhi,
        /// <summary>
        /// Sinhalese.
        /// </summary>
        Sinhalese,
        /// <summary>
        /// Slovak.
        /// </summary>
        Slovak,
        /// <summary>
        /// Slovenian.
        /// </summary>
        Slovenian,
        /// <summary>
        /// Spanish.
        /// </summary>
        Spanish,
        /// <summary>
        /// Swahili.
        /// </summary>
        Swahili,
        /// <summary>
        /// Swedish.
        /// </summary>
        Swedish,
        /// <summary>
        /// Tajik.
        /// </summary>
        Tajik,
        /// <summary>
        /// Tamil.
        /// </summary>
        Tamil,
        /// <summary>
        /// Tagalog.
        /// </summary>
        Tagalog,
        /// <summary>
        /// Telugu.
        /// </summary>
        Telugu,
        /// <summary>
        /// Thai.
        /// </summary>
        Thai,
        /// <summary>
        /// Tibetan.
        /// </summary>
        Tibetan,
        /// <summary>
        /// Turkish.
        /// </summary>
        Turkish,
        /// <summary>
        /// Ukrainian.
        /// </summary>
        Ukrainian,
        /// <summary>
        /// Urdu.
        /// </summary>
        Urdu,
        /// <summary>
        /// Uzbek.
        /// </summary>
        Uzbek,
        /// <summary>
        /// Uighur.
        /// </summary>
        Uighur,
        /// <summary>
        /// Vietnamese.
        /// </summary>
        Vietnamese,
    }

}
