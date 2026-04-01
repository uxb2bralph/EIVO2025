using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class CustomSmtpHost
{
    public int HostID { get; set; }

    /// <summary>
    /// 主鍵
    /// </summary>
    public int CompanyID { get; set; }

    public string Host { get; set; } = null!;

    public int? Port { get; set; }

    public bool? EnableSsl { get; set; }

    public string? UserName { get; set; }

    public string? Password { get; set; }

    public string MailFrom { get; set; } = null!;

    public int? Status { get; set; }

    public virtual Organization Company { get; set; } = null!;
}
