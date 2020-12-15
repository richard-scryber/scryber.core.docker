using System;
namespace Scryber.Online.API.Models
{
    public class GenerationResult
    {
        public GenerationResult()
        {
        }

        public string FileName { get; set; }

        public string Errors { get; set; }

        public bool Success { get; set; }

        public string OriginalSource { get; set; }

        public GenerationResultData Data { get; set; }
    }

    public class GenerationResultData
    {
        public string Base64 { get; set; }

        public long Length { get; set; }

        public long DecodedLength { get; set; }
    }
}
