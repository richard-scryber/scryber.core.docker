using System;
namespace Scryber.Online.API.Models
{
    public enum GenerationResponseFormat
    {
        Inline,
        Attachment,
        Base64
    }

    public enum GenerationSourceFormat
    {
        XML,
        HTML,
        Markdown,
        Auto = 30
    }

    public class GenerationRequest
    {
        public GenerationRequest()
        {
        }

        public GenerationParamData[] Params { get; set; }

        public GenerationTemplateData Template { get; set; }

        public GenerationOutputData Output { get; set; }
    }

    public class GenerationParamData
    {
        public string Key { get; set; }

        public dynamic Value { get; set; }
    }

    public class GenerationTemplateData
    {
        public string Url { get; set; }

        public GenerationSourceFormat Format { get; set; }
    }

    public class GenerationOutputData
    {
        public string FileName { get; set; }

        public GenerationResponseFormat Format { get; set; }
    }
}
