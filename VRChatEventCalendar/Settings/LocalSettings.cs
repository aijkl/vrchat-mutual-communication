using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace Aijkl.VRChat.EventCalendar.Settings
{
    public class LocalSettings
    {
        [JsonProperty("ffmpegPath")]
        public string FFMpegPath { set; get; }

        [JsonProperty("codec")]
        public string Codec { set; get; }

        [JsonProperty("frameRate")]
        public string FrameRate { set; get; }

        [JsonProperty("pixelFormat")]
        public string PixelFormat { set; get; }

        [JsonProperty("crf")]
        public string Crf { set; get; }

        [JsonProperty("width")]
        public int Width { set; get; }

        [JsonProperty("height")]
        public int Height { set; get; }

        [JsonProperty("maxByteLength")]
        public int MaxByteLength { set; get; }

        [JsonProperty("authTokenPath")]
        public string AuthTokenPath { set; get; }                     

        [JsonProperty("cloudSettingsId")]
        public string CloudSettingsId { set; get; }        

        [JsonProperty("tempDirectory")]
        public string TempDirectory { set; get; }

        [JsonProperty("metaFileName")]
        public string MetaFileName { set; get; }

        [JsonProperty("saveDirectory")]
        public string SaveDirectory { set; get; }

        [JsonProperty("cloudFlareParameters")]
        public CloudFlareParameters CloudFlareParameters { set; get; }

        [JsonProperty("calendarID")]
        public List<CalendarSettings> CalendarSettings { set; get; }

        public static LocalSettings Load(string path)
        {
            return JsonConvert.DeserializeObject<LocalSettings>(File.ReadAllText(path));
        }
    } 
}
