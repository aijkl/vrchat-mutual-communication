using Newtonsoft.Json;

namespace Aijkl.VRChat.EventCalendar.Models
{
    class Message
    {
        [JsonProperty("content")]
        public string Content { set; get; }
    }
}
