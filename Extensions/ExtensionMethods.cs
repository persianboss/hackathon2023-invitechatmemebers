

using InviteChatMembers.Models;
using Microsoft.Graph.Models;
using Newtonsoft.Json;

namespace InviteChatMembers.Extensions
{
    public static class ExtensionMethods
    {
        public static OnlineMeeting GetOnlineMeeting(this CardResponse cardResponse)
        {
            //var attendees = cardResponse.SelectedUsers.Split(',');
            //var attendeesEmails = attendees.Select(x => x.Split("-")[1]).ToList();
            //var participantsInfo = attendeesEmails.Select(a => new MeetingParticipantInfo() { Identity = new IdentitySet() { User = new Identity() { Id = a } }, Upn = a }).ToList();
            var onlineMeeting = new OnlineMeeting()
            {
                StartDateTime = DateTimeOffset.Now, //DateTimeOffset.Parse(string.Join(" ", new[] { cardResponse.FromDate, cardResponse.FromTime })),
                EndDateTime =  DateTimeOffset.Now.AddHours(1), //DateTimeOffset.Parse(string.Join(" ", new[] { cardResponse.ToDate, cardResponse.ToTime })),
                Subject = cardResponse.MeetingTitle,
                //Participants = new MeetingParticipants()
                //{
                //    Attendees = participantsInfo
                //}

            };
            return onlineMeeting;
        }

        public static Event GetOnlineEvent(this CardResponse cardResponse)
        {
            var attendees = cardResponse.SelectedUsers.Split(',');
            var attendeesEmails = attendees.Select(x => x.Split("-")[1].Trim()).ToList();
            var attendeesNames = attendees.Select(x => x.Split("-")[0].Trim()).ToList();
            var listOfAttendees = attendeesEmails.Select(a => new { emailAddress = new  { address = a , name = attendeesNames[attendeesEmails.IndexOf(a)] }, type = AttendeeType.Required.ToString().ToLower() }).ToList();
            
            var onlineEvent = new
            {
                subject = cardResponse.MeetingTitle,
                body = new
                {
                    contentType = BodyType.Html.ToString(),
                    content = cardResponse.MeetingComment
                },
                start = new 
                {
                    DateTime = DateTime.Parse(string.Join(" ", new[] { cardResponse.FromDate, cardResponse.FromTime })),
                    TimeZone = "Pacific Standard Time"
                },
                end = new 
                {
                    DateTime = DateTime.Parse(string.Join(" ", new[] { cardResponse.ToDate, cardResponse.ToTime })),
                    TimeZone = "Pacific Standard Time"
                },
                isOnlineMeeting = true,
                location = new { displayName = "Microsoft Teams" },
                attendees = listOfAttendees,
                allowNewTimeProposals = true,
                transactionId = Guid.NewGuid().ToString()
            };
            var json = JsonConvert.SerializeObject(onlineEvent, Formatting.Indented);
            var newEvent = JsonConvert.DeserializeObject<Event>(json);
            return newEvent;
        }

        public static string GetOnlineEventJason(this CardResponse cardResponse)
        {
            var attendees = cardResponse.SelectedUsers.Split(',');
            var attendeesEmails = attendees.Select(x => x.Split("-")[1].Trim()).ToList();
            var attendeesNames = attendees.Select(x => x.Split("-")[0].Trim()).ToList();
            var listOfAttendees = attendeesEmails.Select(a => new { emailAddress = new { address = a, name = attendeesNames[attendeesEmails.IndexOf(a)] }, type = AttendeeType.Required.ToString().ToLower() }).ToList();

            var onlineEvent = new
            {
                subject = cardResponse.MeetingTitle,
                body = new
                {
                    contentType = BodyType.Html.ToString(),
                    content = cardResponse.MeetingComment
                },
                start = new
                {
                    DateTime = DateTime.Parse(string.Join(" ", new[] { cardResponse.FromDate, cardResponse.FromTime })),
                    TimeZone = "Pacific Standard Time"
                },
                end = new
                {
                    DateTime = DateTime.Parse(string.Join(" ", new[] { cardResponse.ToDate, cardResponse.ToTime })),
                    TimeZone = "Pacific Standard Time"
                },
                isOnlineMeeting = true,
                location = new { displayName = "Microsoft Teams" },
                attendees = listOfAttendees,
                allowNewTimeProposals = true,
                transactionId = Guid.NewGuid().ToString()
            };
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(onlineEvent, Formatting.Indented);
            return json;
        }
    }
}
