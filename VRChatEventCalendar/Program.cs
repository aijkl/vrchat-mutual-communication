using Aijkl.VRChat.EventCalendar.Settings;
using System.IO;
using System.Threading;

namespace Aijkl.VRChat.EventCalendar
{
    class Program
    {
        static void Main(string[] args)
        {            
            CalendarToVideoConverter calendarToVideoConverter = new CalendarToVideoConverter(LocalSettings.Load(Path.GetFullPath("./Resources/localSettings.json")));
            calendarToVideoConverter.BeginLoop(new CancellationTokenSource().Token);
            new AutoResetEvent(false).WaitOne();
        }
    }
}
