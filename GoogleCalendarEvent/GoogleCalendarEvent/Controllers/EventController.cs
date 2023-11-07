using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Calendar.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using GoogleCalendarEvent.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using static Google.Apis.Calendar.v3.AclResource;
using Microsoft.AspNetCore.Authorization;
using System.Security.Cryptography.X509Certificates;

namespace GoogleCalendarEvent.Controllers
{
   
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class EventController : ControllerBase
    {
       
            private static string[] Scopes = { CalendarService.Scope.CalendarReadonly };
            private static string ApplicationName = "GoogleCalendarEvent";

            public static async Task<CalendarService> GetCalendarService()
            {

                UserCredential credential;
                using (var stream = new FileStream(Path.Combine(Directory.GetCurrentDirectory(), "cre", "cre.json"), FileMode.Open, FileAccess.Read))
                {
                    string credPath = "token.json";
                    credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                        GoogleClientSecrets.FromStream(stream).Secrets,
                        Scopes,
                        "user",
                        CancellationToken.None,
                        new FileDataStore(credPath, true)
                    );
                }
                return new CalendarService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = ApplicationName,
                });
            }
        
        [HttpPost]
        public async Task<IActionResult> CreateEvent([FromBody] GoogleEvent eventModel)
        {

            try
            {
                var Service = await GetCalendarService();

                var newEvent = new Event
                {
                    Summary = eventModel.Summary,
                    Location = eventModel.Location,
                    Description = eventModel.Description,
                    Start = new EventDateTime { DateTime = eventModel.StartDateTime, TimeZone = "UTC" },
                    End = new EventDateTime { DateTime = eventModel.EndDateTime, TimeZone = "UTC" }
                };
                if (eventModel.StartDateTime < DateTime.Now)
                {
                    return BadRequest("Can not create event in the past");
                }
                if (eventModel.StartDateTime.DayOfWeek == DayOfWeek.Friday || eventModel.StartDateTime.DayOfWeek == DayOfWeek.Saturday)
                {
                    return BadRequest("Can not create event on friday or saturday");
                }

                EventsResource.InsertRequest request = Service.Events.Insert(newEvent, "primary");
                Event createdEvent = request.Execute();

                //return Ok($"Event with ID {createdEvent.Id} has been created.");
                 return CreatedAtAction("CreateEvent", newEvent, new { eventId = createdEvent.Id });


            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }


        [HttpGet]
        public async Task<IActionResult> GetEvents()
        {
            try
            {
                var Service = await GetCalendarService();

                EventsResource.ListRequest request = Service.Events.List("primary");
                request.TimeMin = DateTime.Now;
                request.ShowDeleted = false;
                request.SingleEvents = true;
                request.MaxResults = 10;
                request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;

                Events events = await request.ExecuteAsync();

                var calendarEvents = new List<GoogleEvent>();

                if (events.Items != null && events.Items.Any())
                {
                    foreach (var eventItem in events.Items)
                    {
                        var calendarEvent = new GoogleEvent
                        {
                            Summary = eventItem.Summary,
                            Location = eventItem.Location,
                            Description = eventItem.Description,
                            StartDateTime = eventItem.Start.DateTime ?? DateTime.MinValue,
                            EndDateTime = eventItem.End.DateTime ?? DateTime.MinValue
                        };

                        calendarEvents.Add(calendarEvent);
                    }
                }

                return Ok(calendarEvents);
            }
            catch (Exception ex)
            {
                return BadRequest();//($"Error: {ex.Message}");
            }
            
        }

                
            
            [HttpDelete]
        public async Task<IActionResult> DeleteEventAsync(string Id)
        {
            try
            {
                var Service = await GetCalendarService();

                EventsResource.DeleteRequest request = Service.Events.Delete("primary", Id);
                request.Execute();

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }


        [HttpGet("Search")]
        public async Task<IActionResult> GetEvents([FromQuery] string search)
        {
            try
            {

                var Service = await GetCalendarService();
                EventsResource.ListRequest request = Service.Events.List("primary");
                request.TimeMin = DateTime.Now; 
                request.ShowDeleted = false;
                request.SingleEvents = true;
                request.MaxResults = 10; 
                request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;

                if (!string.IsNullOrEmpty(search))
                {
                    // Apply search
                    request.Q = search;
                }

                Events events = await request.ExecuteAsync();

                var calendarEvents = new List<GoogleEvent>();

                if (events.Items != null && events.Items.Any())
                {
                    foreach (var eventItem in events.Items)
                    {
                        var calendarEvent = new GoogleEvent
                        {
                            
                            Summary = eventItem.Summary,
                            Location = eventItem.Location,
                            Description = eventItem.Description,
                            StartDateTime = eventItem.Start.DateTime ?? DateTime.MinValue,
                            EndDateTime = eventItem.End.DateTime ?? DateTime.MinValue
                        };

                        calendarEvents.Add(calendarEvent);
                    }
                }

                return Ok(calendarEvents);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }

    }
}
 


      