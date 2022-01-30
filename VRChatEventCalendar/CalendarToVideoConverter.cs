using Google.Apis.Calendar.v3;
using System.IO;
using System.Threading;
using System;
using Google.Apis.Drive.v3;
using System.Collections.Generic;
using Aijkl.CloudFlare.API;
using Aijkl.VRChat.EventCalendar.Settings;
using System.Threading.Tasks;
using System.Linq;
using Aijkl.VRChat.MutualCommunication;
using SkiaSharp;
using System.Diagnostics;
using Google.Apis.Calendar.v3.Data;

namespace Aijkl.VRChat.EventCalendar
{
    public class CalendarToVideoConverter
    {
        private readonly CalendarService calendarService;
        private readonly DriveService driveService;
        private readonly LocalSettings localSettings;
        private readonly DiscordClient discordClient;
        private readonly MetaDataGroup metaDataGroup;        
        private CloudFlareAPIClient cloudFlareAPIClient;
        private CloudSettings cloudSettings;
        public CalendarToVideoConverter(LocalSettings localSettings)
        {
            this.localSettings = localSettings;
            discordClient = new DiscordClient();
            calendarService = CalendarHelper.CreateCalendarService(File.ReadAllText(Path.GetFullPath(localSettings.AuthTokenPath)));
            driveService = CalendarHelper.CreateDriveService(File.ReadAllText(Path.GetFullPath(localSettings.AuthTokenPath)));
            cloudSettings = CloudSettings.Fetch(driveService, localSettings.CloudSettingsId);
            cloudFlareAPIClient = new CloudFlareAPIClient(localSettings.CloudFlareParameters.EmailAdress, localSettings.CloudFlareParameters.AuthToken);

            string metaFilePath = $"{localSettings.TempDirectory}{Path.DirectorySeparatorChar}{localSettings.MetaFileName}";
            if (!Directory.Exists(localSettings.TempDirectory)) Directory.CreateDirectory(localSettings.TempDirectory);
            if (!File.Exists(metaFilePath)) File.WriteAllText(metaFilePath, string.Empty);            
            metaDataGroup = MetaDataGroup.FromFile(metaFilePath);
        }        
        public void BeginLoop(CancellationToken cancellationToken)
        {
            while (true)
            {
                if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    cloudSettings = CloudSettings.Fetch(driveService, localSettings.CloudSettingsId);
                    List<string> cloudFlareUnnecessaryCaches = new List<string>();
                    List<string> createdFiles = new List<string>();
                    
                    Parallel.ForEach(cloudSettings.CalendarSettings, calendar =>
                    {
                        try
                        {
                            EventsResource.ListRequest request = calendarService.Events.List(calendar.CalendarID);
                            request.TimeMin = DateTime.Now.AddHours(calendar.BeginAddDays);
                            request.TimeMax = new DateTime(DateTime.Now.AddDays(calendar.EndAddDays).Year, DateTime.Now.AddDays(calendar.EndAddDays).Month, DateTime.Now.AddDays(calendar.EndAddDays).Day, 23, 59, 59);
                            request.ShowDeleted = false;
                            request.SingleEvents = true;                            
                            request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;
                            var events = request.Execute();                            

                            DateTime dateTime = DateTime.Now;
                            EventDataGroup eventDataGroup = new EventDataGroup();                                                        
                            for (int i = 0; i < Math.Abs(calendar.BeginAddDays) + calendar.EndAddDays + 1; i++)
                            {
                                if (eventDataGroup.IsExsits(dateTime, events.Items))
                                {
                                    Console.WriteLine(dateTime);
                                    eventDataGroup.Add(dateTime, events.Items);
                                }                                
                                                           
                                dateTime = dateTime.AddDays(1);
                            }
                            eventDataGroup.TrimByteLength(localSettings.MaxByteLength);
                            
                            using SKBitmap skBitmap = PixelConverter.ConvertToBitmap(eventDataGroup.ToJsonByteArray(), localSettings.Width, localSettings.Height);
                            using SKCanvas skCanvas = new SKCanvas(skBitmap);
                            string imageFilePath = Path.GetFullPath(Path.Combine(localSettings.TempDirectory, $"{calendar.FileName}.png"));
                            string videoFilePath = Path.GetFullPath(Path.Combine(localSettings.SaveDirectory, $"{calendar.FileName}.avi"));
                            using (FileStream fileStream = new FileStream(Path.Combine(Directory.GetCurrentDirectory(), imageFilePath), FileMode.Create))
                            {
                                skBitmap.Encode(SKEncodedImageFormat.Png, 100).SaveTo(fileStream);
                            }
                            try
                            {
                                RunFFMpeg(imageFilePath, videoFilePath);
                                Console.WriteLine($"[Save] {calendar.FileName}");
                                createdFiles.Add(videoFilePath);
                                if (!metaDataGroup.Exists(videoFilePath)) metaDataGroup.Add(new MetaData(videoFilePath));
                                if (!metaDataGroup[videoFilePath].Equals(new MetaData(videoFilePath)))
                                {
                                    cloudFlareUnnecessaryCaches.Add($"{localSettings.CloudFlareParameters.BaseUrl}/{calendar.FileName}.avi");
                                    metaDataGroup.HashEvaluation(videoFilePath);
                                }
                            }
                            catch (Exception ex)
                            {
                                CatchError(ex);
                            }
                            finally
                            {
                                if (File.Exists(imageFilePath))
                                {
                                    File.Delete(imageFilePath);
                                }
                            }                            
                        }
                        catch (Exception ex)
                        {
                            CatchError(ex, cloudSettings.DiscordWebHookURL);
                        }
                    });

                    metaDataGroup.Where(x => !createdFiles.Contains(x.FilePath)).ToList().ForEach(x =>
                    {
                        File.Delete(x.FilePath);
                        cloudFlareUnnecessaryCaches.Add($"{localSettings.CloudFlareParameters.BaseUrl}/{x.FilePath.Replace($"{localSettings.SaveDirectory}{Path.DirectorySeparatorChar}", string.Empty).Replace(Path.DirectorySeparatorChar.ToString(), "/")}");
                    });
                    
                    metaDataGroup.DeleteUnUsedMetaData();
                    metaDataGroup.SaveToFile();

                    if (cloudFlareUnnecessaryCaches.Count > 0)
                    {
                        cloudFlareAPIClient.Zone.PurgeFilesByUrl(localSettings.CloudFlareParameters.ZoneId, cloudFlareUnnecessaryCaches);
                        Console.WriteLine($"[DeleteCache] {string.Join(" ", cloudFlareUnnecessaryCaches.Select(x => Path.GetFileName(x)))}");
                        cloudFlareUnnecessaryCaches.Clear();
                    }
                }
                catch (Exception ex)
                {
                    CatchError(ex, cloudSettings.DiscordWebHookURL);
                }
                GC.Collect();
                Thread.Sleep(cloudSettings.UpdateInterval);
            }
        }
        private void CatchError(Exception ex, string url = "")
        {
            try
            {
                Console.WriteLine($"[Error] {ex.Message}");
                if (!string.IsNullOrEmpty(url)) discordClient.PostMessage(url, $"{ex.Message}{ex.StackTrace}{ex.InnerException}");
            }
            catch
            {
            }
        }
        private void RunFFMpeg(string inputFilePath, string outputFilePath)
        {
            using Process process = new Process();
            process.StartInfo.FileName = localSettings.FFMpegPath;
            process.StartInfo.Arguments = $"-loglevel quiet -framerate {localSettings.FrameRate} -i {inputFilePath} -vcodec {localSettings.Codec} -pix_fmt {localSettings.PixelFormat} -crf {localSettings.Crf} {outputFilePath} -y";
            process.Start();
            process.WaitForExit();
        }
    }
}
