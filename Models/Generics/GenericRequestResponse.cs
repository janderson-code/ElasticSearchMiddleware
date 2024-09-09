using br.com.sharklab.elasticsearch.Models.Tasks;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace br.com.sharklab.elasticsearch.Models.Generics
{
    public class GenericRequestResponse
    {
        public object Request { get; private set; }
        public object Response { get; private set; }
        public string RequestMethod { get; set; }
        public string RequestQuery { get; set; }
        public string RequestPath { get; set; }
        public DateTime TimeStamp { get; private set; }
        public int StatusCode { get; private set; }
        public double ElapsedResponse { get; private set; }
        public Dictionary<string, string[]> RequestHeader { get; set; }
        public Dictionary<string, string[]> ResponseHeader { get; set; }

        public static implicit operator GenericRequestResponse(IndexingTask indexingTask)
        {
            return GenerateDataToLog(indexingTask.RequestObj, indexingTask.ResponseContent, indexingTask.StatusCode, indexingTask.ElapsedTime,
                indexingTask.RequestHeader, indexingTask.ResponseHeader, indexingTask.RequestMethod, indexingTask.RequestQuery, indexingTask.RequestPath);
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

        internal GenericRequestResponse()
        {
            TimeStamp = DateTime.Now;
        }

        private static GenericRequestResponse GenerateDataToLog
            (string dataStringRequest, string dataStringResponse, int statusCode,
                double elapsedTime, Dictionary<string, string[]> requestHeader, Dictionary<string, string[]> responseHeader, string requestMethod, string requestQuery, string requestPath)
        {
            var jsonDictRequest = JsonHelper.DeserializeJson(dataStringRequest);
            var jsonDictResponse = JsonHelper.DeserializeJson(dataStringResponse);
            var response = new GenericRequestResponse
            {
                Request = jsonDictRequest,
                Response = jsonDictResponse,
                StatusCode = statusCode,
                ElapsedResponse = elapsedTime,
                RequestHeader = requestHeader,
                ResponseHeader = responseHeader,
                RequestMethod = requestMethod,
                RequestQuery = requestQuery,
                RequestPath = requestPath
            };

            return response;
        }
    }
}