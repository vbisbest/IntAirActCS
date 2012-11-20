using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RestSharp;

namespace IntAirAct
{
    public class RestSharpClient : IAClient
    {
        public void SendRequest(IARequest request, IADevice device)
        {
            this.SendRequest(request, device, null);
        }

        public void SendRequest(IARequest request, IADevice device, Action<IAResponse, Exception> action)
        {
            RestClient client = ClientFromDevice(device);
            RestRequest restRequest = RequestFromIARequest(request);
            client.ExecuteAsync(restRequest, response =>
            {
                action(IAResponseFromRestResponse(response), response.ErrorException);
            });
        }

        private RestClient ClientFromDevice(IADevice device)
        {
            return new RestClient(String.Format("http://{0}:{1}", device.Host, device.Port));
        }

        private RestRequest RequestFromIARequest(IARequest request)
        {
            RestRequest result = new RestRequest();
            switch (request.Route.Action)
            {
                case "GET":
                    result.Method = Method.GET;
                    break;
                case "POST":
                    result.Method = Method.POST;
                    break;
                case "PUT":
                    result.Method = Method.PUT;
                    break;
                case "OPTIONS":
                    result.Method = Method.OPTIONS;
                    break;
                case "DELETE":
                    result.Method = Method.DELETE;
                    break;
                case "HEAD":
                    result.Method = Method.HEAD;
                    break;
                case "PATCH":
                    result.Method = Method.PATCH;
                    break;
                default:
                    result.Method = Method.GET;
                    break;
            }
            result.Resource = request.Route.Resource;
            if (request.Origin != null)
            {
                result.AddHeader("X-IA-Origin", request.Origin.Name);
            }
            return result;
        }

        private IAResponse IAResponseFromRestResponse(IRestResponse response)
        {
            IAResponse result = new IAResponse();
            result.StatusCode = (int)response.StatusCode;
            result.ContentType = response.ContentType;
            result.Body = response.RawBytes;
            return result;
        }
    }
}
