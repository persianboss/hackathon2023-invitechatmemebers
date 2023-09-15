namespace InviteChatMembers.Models
{
    public class CardResponse
    {
        public string id { get; set; }
        public string Action { get; set; }
        public string MeetingTitle { get; set; }
        public string SelectedUsers { get; set; }
        public string ToDate { get; set; }
        public string FromDate { get; set; }
        public string FromTime { get; set; }
        public string ToTime { get; set; }
        public string MeetingComment { get;set; }
    }
}
