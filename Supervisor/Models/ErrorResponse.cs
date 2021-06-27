using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Supervisor.Models
{
    public class ValidationError
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Field { get; }

        public string Message { get; }

        public ValidationError(string field, string message)
        {
            Field = field != string.Empty ? field : null;
            Message = message;
        }
    }
    public class ErrorResponse
    {
        public string ResponseCode { get; set; }
        public string ResponseMessage { get; set; }
        public List<ValidationError> Data { get; }

        public ErrorResponse() { }

        public ErrorResponse(ModelStateDictionary modelState)
        {
            this.ResponseMessage = "Validation Failed";
            Data = modelState.Keys
                    .SelectMany(key => modelState[key].Errors.Select(x => new ValidationError(key, x.ErrorMessage)))
                    .ToList();
            ResponseCode = "01";
            ResponseMessage = "Validation error occured";
        }
    }

    public class ValidationFailedResult : ObjectResult
    {
        public ValidationFailedResult(ModelStateDictionary modelState)
            : base(new ErrorResponse(modelState))
        {
            StatusCode = StatusCodes.Status400BadRequest;
        }
    }
}
