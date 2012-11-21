using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RestSharp;
using System.Text.RegularExpressions;

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
                if (action != null)
                {
                    action(IAResponseFromRestResponse(response), response.ErrorException);
                }
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
            result.Resource = ReplaceParameters(request.Route.Resource, request.Parameters);
            foreach (KeyValuePair<string, string> parameter in request.Parameters)
            {
                result.AddParameter(parameter.Key, parameter.Value);
            }
            if (request.Origin != null)
            {
                result.AddHeader("X-IA-Origin", request.Origin.Name);
            }
            foreach (KeyValuePair<string, string> metaentry in request.Metadata)
            {
                result.AddHeader(metaentry.Key, metaentry.Value);
            }
            return result;
        }

        private IAResponse IAResponseFromRestResponse(IRestResponse response)
        {
            IAResponse result = new IAResponse();
            result.StatusCode = (int)response.StatusCode;
            result.ContentType = response.ContentType;
            result.Body = response.RawBytes;
            foreach (Parameter p in response.Headers)
            {
                result.Metadata.Add(p.Name, "" + p.Value);
            }
            return result;
        }

        private string ReplaceParameters(string path, Dictionary<string, string> parameters)
        {
            string result = Regex.Replace(path, @"{(\w+)}", delegate(Match match)
            {
                string value = match.ToString();
                string key = value.Substring(1, value.Length - 2);
                if (parameters.ContainsKey(key))
                {
                    string keyValue = parameters[key];
                    parameters.Remove(key);
                    return keyValue;
                }
                else
                {
                    return value;
                }
            });
            return result;
        }
    }
}
