using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Drive.v3;
using Google.Apis.Services;

namespace Aijkl.VRChat.EventCalendar
{
    public static class CalendarHelper
    {
        public static DriveService CreateDriveService(string json)
        {
            var credential = GoogleCredential.FromJson(json).CreateScoped(DriveService.Scope.Drive).UnderlyingCredential as ServiceAccountCredential;
            return new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential
            });
        }
        public static CalendarService CreateCalendarService(string json)
        {
            var credential = GoogleCredential.FromJson(json).CreateScoped(CalendarService.Scope.Calendar).UnderlyingCredential as ServiceAccountCredential;
            return new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential
            });
        }      
    }
}
