namespace TaskCenter.Core.DTOs
{
    public class ProcessRequestNotificationItemDto
    {
        public int TaskID { get; set; }
        public string? ChannelName { get; set; }
        public string? ChannelResponse { get; set; }
        public string? ResponseName { get; set; }
        public string? TxnPath { get; set; }
    }

    public class NotifyRequestCompletionResultDto
    {
        public bool result { get; set; }
        public ProcessRequestNotificationItemDto[]? data { get; set; }
    }

    public class ProcessExceptionNotificationItemDto
    {
        public int TaskID { get; set; }
        public string? OriginalData { get; set; }
        public string? RequestName { get; set; }
        public string? ChannelName { get; set; }
        public string? ExceptionMessage { get; set; }
    }

    public class NotifyRequestExceptionResultDto
    {
        public bool result { get; set; }
        public ProcessExceptionNotificationItemDto[]? data { get; set; }
    }
}
