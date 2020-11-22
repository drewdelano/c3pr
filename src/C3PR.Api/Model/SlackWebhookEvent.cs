namespace C3PR.Api.Models
{
    public class SlackWebhookEvent
    {
        public string Type { get; set; }
        public string Channel { get; set; }
        public string User { get; set; }
        public string Text { get; set; }
        public string Ts { get; set; }
        public string ChannelType { get; set; }
        public string SubType { get; set; }
    }

}
