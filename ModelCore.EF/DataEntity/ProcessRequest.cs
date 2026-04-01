using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class ProcessRequest
{
    public int TaskID { get; set; }

    public int? Sender { get; set; }

    public DateTime SubmitDate { get; set; }

    public DateTime? ProcessStart { get; set; }

    public DateTime? ProcessComplete { get; set; }

    public string? RequestPath { get; set; }

    public string? ResponsePath { get; set; }

    public int? ProcessType { get; set; }

    public int? AgentID { get; set; }

    public int? LogID { get; set; }

    public string? ViewModel { get; set; }

    public int? TotalCount { get; set; }

    public int? ProgressCount { get; set; }

    public virtual Organization? Agent { get; set; }

    public virtual ExceptionLog? Log { get; set; }

    public virtual ProcessCompletionNotification? ProcessCompletionNotification { get; set; }

    public virtual ICollection<ProcessExceptionNotification> ProcessExceptionNotification { get; set; } = new List<ProcessExceptionNotification>();

    public virtual ICollection<ProcessRequestCondition> ProcessRequestCondition { get; set; } = new List<ProcessRequestCondition>();

    public virtual ICollection<ProcessRequestDocument> ProcessRequestDocument { get; set; } = new List<ProcessRequestDocument>();

    public virtual ProcessRequestQueue? ProcessRequestQueue { get; set; }

    public virtual ProcessRequestType? ProcessTypeNavigation { get; set; }

    public virtual UserProfile? SenderNavigation { get; set; }
}
