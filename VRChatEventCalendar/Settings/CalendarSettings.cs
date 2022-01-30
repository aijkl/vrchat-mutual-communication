using Newtonsoft.Json;
using System;

namespace Aijkl.VRChat.EventCalendar.Settings
{
    public class CalendarSettings
    {
        [JsonProperty("calendarID")]
        public string CalendarID { set; get; }

        [JsonProperty("fileName")]
        public string FileName { set; get; }

        [JsonProperty("beginAddDays")]
        public int BeginAddDays { set; get; }

        [JsonProperty("endAddDays")]
        public int EndAddDays { set; get; }
    }
}
