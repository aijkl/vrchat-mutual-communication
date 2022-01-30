using Google.Apis.Calendar.v3.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aijkl.VRChat.EventCalendar
{
    public class EventDataGroup
    {                
        [JsonProperty("calendars")]

        private readonly Dictionary<string, List<string>> calendarDataMap;
        public EventDataGroup()
        {            
            calendarDataMap = new Dictionary<string, List<string>>();
        }

        [JsonProperty("begin")]
        public int Begin { private set; get; }        

        [JsonProperty("end")]
        public int End { private set; get; }

        public void Add(DateTime dateTime, IEnumerable<Event> events)
        {
            IEnumerable<Event> temp = events.Where(x => !string.IsNullOrEmpty(x.Start.Date) && x.Start.Date == dateTime.ToString("yyyy-MM-dd"));
            if (temp.Count() == 0) temp = events.Where(x => x.Start.DateTime != null && x.Start.DateTime.Value.Date == dateTime.Date);
            if (temp.Count() != 0)
            {
                calendarDataMap.Add($"{dateTime.Year}/{dateTime:MM}/{dateTime:dd}", temp.Select(x => GetDateString(x)).ToList());
            }
        }        
        public byte[] ToJsonByteArray()
        {
            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(this));
        }
        public bool IsExsits(DateTime dateTime, IList<Event> events)
        {
            return events.Any(x => !string.IsNullOrEmpty(x.Start.Date) && x.Start.Date == dateTime.ToString("yyyy-MM-dd")) || events.Any(x => x.Start.DateTime != null && x.Start.DateTime.Value.Date == dateTime.Date);
        }
        public void TrimByteLength(int maxLength)
        {
            foreach (var item in calendarDataMap)
            {
                if (item.Value.Count == 0)
                {
                    calendarDataMap.Remove(item.Key);
                }
            }            

            while (GetByteLength() > maxLength)
            {                
                calendarDataMap.Last().Value.RemoveAt(calendarDataMap.Last().Value.Count - 1);
                foreach (var item in calendarDataMap)
                {
                    if (item.Value.Count == 0)
                    {
                        calendarDataMap.Remove(item.Key);
                    }
                }                
            }

            string beginDateTimeString = calendarDataMap.Select(x => x.Key).First();
            string endDateTimeString = calendarDataMap.Select(x => x.Key).Last();
            Begin = Math.Abs((DateTime.Now - new DateTime(int.Parse(beginDateTimeString.Substring(0, 4)), int.Parse(beginDateTimeString.Substring(5, 2)), int.Parse(beginDateTimeString.Substring(8, 2)))).Days);
            End = (new DateTime(int.Parse(endDateTimeString.Substring(0, 4)), int.Parse(endDateTimeString.Substring(5, 2)), int.Parse(endDateTimeString.Substring(8, 2))) - DateTime.Now).Days;
            End++;
        }
        private int GetByteLength()
        {                        
            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(this)).Length;
        }                
        private string GetDateString(Event eventData)
        {            
            if (!string.IsNullOrEmpty(eventData.Start.Date))
            {
                return $"00002359{eventData.Summary}";
            }
            return $"{eventData.Start.DateTime.Value:HH}{eventData.Start.DateTime.Value:mm}{eventData.End.DateTime.Value:HH}{eventData.End.DateTime.Value:mm}{eventData.Summary}";
        }
    }
}
