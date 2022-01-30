using Google.Apis.Drive.v3;
using System.IO;
using Newtonsoft.Json;
using Aijkl.VRChat.EventCalendar.Extensions;
using System.Text;
using System.Collections.Generic;

namespace Aijkl.VRChat.EventCalendar.Settings
{
    public class CloudSettings
    {
        [JsonIgnore]
        public string FileId { private set; get; }

        [JsonProperty("updateInterval")]
        public int UpdateInterval { set; get; }      

        [JsonProperty("discordWebHookURL")]
        public string DiscordWebHookURL { set; get; }

        [JsonProperty("calendars")]
        public List<CalendarSettings> CalendarSettings { set; get; }
        
        public static CloudSettings Fetch(DriveService driveService,string fileID)
        {
            using Stream stream = driveService.DownLoadAsSteamAsync(fileID).Result;
            stream.Position = 0;
            using StreamReader streamReader = new StreamReader(stream);
            using JsonReader jsonReader = new JsonTextReader(streamReader);
            JsonSerializer jsonSerializer = new JsonSerializer();
            CloudSettings cloudSettings = jsonSerializer.Deserialize<CloudSettings>(jsonReader);
            cloudSettings.FileId = fileID;
            return cloudSettings;
        }
        public void Push(DriveService driveService)
        {
            MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(this, Formatting.Indented)));
            driveService.UpdateStreamAsync(memoryStream, FileId).Wait();
        }
    }    
}
