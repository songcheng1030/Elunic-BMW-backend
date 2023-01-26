namespace AIQXCommon.Models
{
    public class DataResponse
    {
        public dynamic Data { get; set; }
        public dynamic Meta { get; set; }

        public DataResponse(dynamic data)
        {
            this.Data = data;
            this.Meta = new object();
        }
        public DataResponse(dynamic data, dynamic meta)
        {
            this.Data = data;
            this.Meta = meta;
        }
    }
    public class DataResponseMeta
    {

    }
    public class DataResponseSchema<TData, TMeta>
    {
        public TData Data { get; set; }
        public TMeta Meta { get; set; }
    }
}
