namespace AIQXCommon.Models
{
    public class PagingOption
    {
#nullable enable
        public int? limit { get; set; }
        public int? page { get; set; }
#nullable disable
        public string order { get; set; }
    }
    public class PagingResult
    {
        public int count { get; set; }
        public int page { get; set; }
        public int pageCount { get; set; }
        public int total { get; set; }
    }
}
