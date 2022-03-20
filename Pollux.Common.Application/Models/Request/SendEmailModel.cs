namespace Pollux.Common.Application.Models.Request
{
    public class SendEmailModel
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string To { get; set; }
        public string Text { get; set; }
        public string Topic { get; set; }
        public string FromEmail { get; set; }

    }
}
