using System;
using System.Net.Http;
using System.Web;
using System.Web.Script.Serialization;
using System.Threading.Tasks;

namespace Pinboard
{
    public interface IPinboardRequest
    {
        void SetRequest(string API);
        void AddParameter(string name, string value);
        void AddParameter(string name, uint value);
        void AddParameter(string name, bool value);
        Task<string> SendRequestAsync();
    }

    class PinboardRequest : IPinboardRequest
    {
        private readonly string ApiToken;
        private readonly HttpClient WebClient = new HttpClient();
        private const string BaseApiURL = "https://api.pinboard.in/v1/{0}?{1}format=json";
        private string URL;

        public PinboardRequest(string ApiToken)
        {
            this.ApiToken = String.Format("auth_token={0}&", ApiToken);
        }

        public PinboardRequest(string UserName, string Password)
        {
            string authInfo = UserName + ":" + Password;
            authInfo = Convert.ToBase64String(System.Text.Encoding.Default.GetBytes(authInfo));
            WebClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authInfo);
        }

        public void SetRequest(string API)
        {
            URL = String.Format(BaseApiURL, API, ApiToken);
        }

        public void AddParameter(string name, string value)
        {
            URL += "&" + name + "=" + HttpUtility.UrlEncode(value);
        }

        public void AddParameter(string name, uint value)
        {
            AddParameter(name, value.ToString());
        }

        public void AddParameter(string name, bool value)
        {
            AddParameter(name, PinboardManager.BoolToString(value));
        }

        public Task<string> SendRequestAsync()
        {
            return WebClient.GetAsync(URL).ContinueWith(task =>
            {
                HttpResponseMessage msg = task.Result;

                if (!msg.IsSuccessStatusCode)
                {
                    return "";
                }
                return task.Result.Content.ReadAsStringAsync().Result;
            });
        }
    }
}
