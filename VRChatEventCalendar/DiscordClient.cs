using Aijkl.VRChat.EventCalendar.Models;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;

namespace Aijkl.VRChat.EventCalendar
{
    public class DiscordClient : IDisposable
    {
        private HttpClient httpClient;
        public DiscordClient(HttpClient httpClient = null)
        {
            this.httpClient = httpClient ?? new HttpClient();
        }

        public void Dispose()
        {
            httpClient?.Dispose();
            httpClient = null;
        }

        public void PostMessage(string url, string message)
        {
            Message messageJson = new Message
            {
                Content = message
            };
            string json = JsonConvert.SerializeObject(messageJson);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            httpClient.PostAsync(url, content).Wait();
        }
    }
}
