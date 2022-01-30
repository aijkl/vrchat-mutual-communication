using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Aijkl.VRChat.EventCalendar
{
    public class MetaDataGroup : List<MetaData>
    {
        [JsonIgnore]
        public string FilePath { set; get; }
        public MetaData this[string path]
        {
            get
            {
                return this.FirstOrDefault(x => path == x.FilePath);
            }
        }
        public bool Exists(string path)
        {
            return this.Any(x => x.FilePath == path);
        }
        public void HashEvaluation(string path)
        {
            this.Where(x => x.FilePath == path).ToList().ForEach(x =>
            {
                x.MD5HashEvaluation();
            });
        }
        public void SaveToFile()
        {
            File.WriteAllText(FilePath, JsonConvert.SerializeObject(this));
        }
        public void DeleteUnUsedMetaData()
        {
            List<MetaData> deleteMetaDatas = new List<MetaData>();
            for (int i = 0; i < Count; i++)
            {
                if (!File.Exists(this[i].FilePath))
                {
                    deleteMetaDatas.Add(this[i]);
                }
            }
            deleteMetaDatas.ForEach(x =>
            {
                Remove(x);
            });
        }
        public static MetaDataGroup FromFile(string filePath)
        {
            string file = File.ReadAllText(filePath);
            file = string.IsNullOrEmpty(file) ? JsonConvert.SerializeObject(new MetaDataGroup()) : file;
            MetaDataGroup posterMetaDatas = JsonConvert.DeserializeObject<MetaDataGroup>(file, new JsonSerializerSettings());
            posterMetaDatas.FilePath = filePath;
            return posterMetaDatas;
        }        
    }
}
