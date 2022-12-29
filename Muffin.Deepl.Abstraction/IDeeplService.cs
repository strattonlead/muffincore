using Newtonsoft.Json;
using System.Threading.Tasks;

namespace Muffin.Deepl.Abstraction
{
    public interface IDeeplService
    {
        Task<UsageResult> UsageAsync();
        Task<DeeplResult[]> TranslateAsync(DeeplRequest request);
        Task<DeeplResult[]> TranslateAsync(DeeplRequest[] requests);
        Task<DeeplResult[]> TranslateAsync(MatrixRequest request);
    }

    /// <summary>
    /// https://www.deepl.com/de/docs-api/translating-text/request/
    /// </summary>
    public class DeeplRequest
    {
        /// <summary>
        /// text
        /// Required
        /// Text to be translated. Only UTF8-encoded plain text is supported. The parameter may be specified multiple times and translations are returned in the same order as they are requested. Each of the parameter values may contain multiple sentences. Up to 50 texts can be sent for translation in one request.
        /// </summary>
        public string Text { get; set; }

        public string[] Texts { get; set; }

        /// <summary>
        /// source_lang	
        /// Optional
        /// Language of the text to be translated. Options currently available:
        /// "BG" - Bulgarian
        /// "CS" - Czech
        /// "DA" - Danish
        /// "DE" - German
        /// "EL" - Greek
        /// "EN" - English
        /// "ES" - Spanish
        /// "ET" - Estonian
        /// "FI" - Finnish
        /// "FR" - French
        /// "HU" - Hungarian
        /// "IT" - Italian
        /// "JA" - Japanese
        /// "LT" - Lithuanian
        /// "LV" - Latvian
        /// "NL" - Dutch
        /// "PL" - Polish
        /// "PT" - Portuguese(all Portuguese varieties mixed)
        /// "RO" - Romanian
        /// "RU" - Russian
        /// "SK" - Slovak
        /// "SL" - Slovenian
        /// "SV" - Swedish
        /// "ZH" - Chinese
        /// If this parameter is omitted, the API will attempt to detect the language of the text and translate it.
        /// </summary>
        public string SourceLang { get; set; }

        /// <summary>
        /// target_lang
        /// Required
        /// The language into which the text should be translated. Options currently available:
        /// "BG" - Bulgarian
        /// "CS" - Czech
        /// "DA" - Danish
        /// "DE" - German
        /// "EL" - Greek
        /// "EN-GB" - English(British)
        /// "EN-US" - English(American)
        /// "EN" - English(unspecified variant for backward compatibility; please select EN-GB or EN-US instead)
        /// "ES" - Spanish
        /// "ET" - Estonian
        /// "FI" - Finnish
        /// "FR" - French
        /// "HU" - Hungarian
        /// "IT" - Italian
        /// "JA" - Japanese
        /// "LT" - Lithuanian
        /// "LV" - Latvian
        /// "NL" - Dutch
        /// "PL" - Polish
        /// "PT-PT" - Portuguese(all Portuguese varieties excluding Brazilian Portuguese)
        /// "PT-BR" - Portuguese(Brazilian)
        /// "PT" - Portuguese(unspecified variant for backward compatibility; please select PT-PT or PT-BR instead)
        /// "RO" - Romanian
        /// "RU" - Russian
        /// "SK" - Slovak
        /// "SL" - Slovenian
        /// "SV" - Swedish
        /// "ZH" - Chinese
        /// </summary>
        public string TargetLang { get; set; }

        /// <summary>
        /// split_sentences
        /// Optional
        /// Sets whether the translation engine should first split the input into sentences. This is enabled by default. Possible values are:
        /// "0" - no splitting at all, whole input is treated as one sentence
        /// "1" (default) - splits on punctuation and on newlines
        /// "nonewlines" - splits on punctuation only, ignoring newlines
        /// For applications that send one sentence per text parameter, it is advisable to set split_sentences = 0, in order to prevent the engine from splitting the sentence unintentionally.
        /// </summary>
        public bool? SplitSentences { get; set; }

        /// <summary>
        /// preserve_formatting
        /// Optional
        /// Sets whether the translation engine should respect the original formatting, even if it would usually correct some aspects. Possible values are:
        /// "0" (default)
        /// "1"
        /// The formatting aspects affected by this setting include:
        /// Punctuation at the beginning and end of the sentence
        /// Upper/lower case at the beginning of the sentence
        /// </summary>
        public bool? PreserveFormatting { get; set; }

        /// <summary>
        /// formality
        /// Optional
        /// Sets whether the translated text should lean towards formal or informal language. This feature currently only works for target languages "DE" (German), "FR" (French), "IT" (Italian), "ES" (Spanish), "NL" (Dutch), "PL" (Polish), "PT-PT", "PT-BR" (Portuguese) and "RU" (Russian).Possible options are:
        /// "default" (default)
        /// "more" - for a more formal language
        /// "less" - for a more informal language
        /// </summary>
        public bool? Formality { get; set; }

        /// <summary>
        /// glossary_id
        /// Optional
        /// Specify the glossary to use for the translation. Important: This requires the source_lang parameter to be set and the language pair of the glossary has to match the language pair of the request.
        /// </summary>
        public string GlossaryId { get; set; }
    }

    public class DeeplResult
    {
        /// <summary>
        /// detected_source_language
        /// The language detected in the source text. It reflects the value of the source_lang parameter, when specified.
        /// </summary>
        [JsonProperty(PropertyName = "detected_source_language")]
        public string DetectedSourceLanguage { get; set; }

        /// <summary>
        /// text
        /// The translated text.
        /// </summary>
        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }

        [JsonProperty(PropertyName = "target_language")]
        public string TargetLang { get; set; }
    }

    public class DeeplResultContainer
    {
        [JsonProperty(PropertyName = "translations")]
        public DeeplResult[] Translations { get; set; }
    }

    public class UsageResult
    {
        [JsonProperty(PropertyName = "character_count")]
        public long CharacterCount { get; set; }

        [JsonProperty(PropertyName = "character_limit")]
        public long CharacterLimit { get; set; }
    }

    public class MatrixRequest
    {
        public string SourceLang { get; set; }
        public string[] TargetLangs { get; set; }
        public string[] Texts { get; set; }
    }
}
