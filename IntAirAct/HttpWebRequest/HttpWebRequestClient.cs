using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;

namespace IntAirAct
{

    public class HttpWebRequestClient : IAClient
    {
        public void SendRequest(IARequest request, IADevice device)
        {
            this.SendRequest(request, device, null);
        }

        public void SendRequest(IARequest request, IADevice device, Action<IAResponse, Exception> action)
        {
            HttpWebRequest webRequest = CreateRequest(device, request);
            IAsyncResult result = webRequest.BeginGetRequestStream(delegate(IAsyncResult asResult1)
            {
                Stream postStream = webRequest.EndGetRequestStream(asResult1);
                postStream.Write(request.Body, 0, request.Body.Length);
                postStream.Close();

                webRequest.BeginGetResponse(delegate(IAsyncResult asResult2)
                {
                    IAResponse res = null;
                    Exception e = null;
                    try
                    {
                        HttpWebResponse response = (HttpWebResponse)webRequest.EndGetResponse(asResult2);
                        res = IAResponseFromHttpWebResponse(response);
                    }
                    catch (WebException exception)
                    {
                        e = new Exception(exception.Message);
                    }

                    Console.WriteLine("Request done");
                    action(res, e);
                }, webRequest);
            }, webRequest);
        }

        private HttpWebRequest CreateRequest(IADevice device, IARequest iaRequest)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>(iaRequest.Parameters);
            string path = ReplaceParameters(iaRequest.Route.Resource, parameters);

            if (parameters.Count > 0)
            {
                path += "?";
            }

            foreach (KeyValuePair<string, string> parameter in parameters)
           { 
                string key = Nancy.Helpers.HttpUtility.UrlEncode(parameter.Key);
                string value = Nancy.Helpers.HttpUtility.UrlEncode(parameter.Value);
                path += String.Format("{0}={1}&", key, value);
            }

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(String.Format("http://{0}:{1}{2}", device.Host, device.Port, path));
            request.ContentType = iaRequest.ContentType;
            request.Method = iaRequest.Route.Action;

            if (iaRequest.Origin != null)
            {
                request.Headers.Add(String.Format("X-IA-Origin: {0}", iaRequest.Origin.Name));
            }
            foreach (KeyValuePair<string, string> metaentry in iaRequest.Metadata)
            {
                request.Headers.Add(String.Format("{0}: {1}", metaentry.Key, metaentry.Value));
            }
            
            return request;
        }

        private IAResponse IAResponseFromHttpWebResponse(HttpWebResponse webResponse)
        {
            IAResponse response = new IAResponse();

            response.StatusCode = (int)webResponse.StatusCode;
            response.ContentType = webResponse.ContentType;
            
            for(int i = 0; i < webResponse.Headers.Count; i++)
            {
                response.Metadata.Add(webResponse.Headers.Keys[i], webResponse.Headers[i]);   
            }

            Stream stream = webResponse.GetResponseStream();
            byte[] b;
            using (MemoryStream ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                b = ms.ToArray();
            }
            response.Body = b;

            return response;
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
