using AIQXCommon.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AIQXCommon.Middlewares
{
    public class ResponseWrapperResultFilter : IResultFilter
    {
        public void OnResultExecuting(ResultExecutingContext context)
        {
            var result = context.Result as ObjectResult;
            if (result is ObjectResult && result.Value != null)
            {
                if (result.Value is DataResponse)
                {
                    return;
                }
                // wrap the inner object
                var newValue = new DataResponse(result.Value);

                // replace the result
                context.Result = new ObjectResult(newValue)
                {
                    // copy the status code
                    StatusCode = result.StatusCode,
                };
            }
        }

        public void OnResultExecuted(ResultExecutedContext context)
        {
        }
    }
}