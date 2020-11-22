namespace C3PR.Api.Models
{
    public class SlackWebhook
    {
        public string Token { get; set; }
        public string Challenge { get; set; }
        public string Type { get; set; }
        public SlackWebhookEvent Event { get; set; }
        public SlackWebhookBotInfo Bot { get; set; }
    }

}
