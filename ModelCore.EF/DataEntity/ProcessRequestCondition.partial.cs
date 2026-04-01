using System;
using System.Collections.Generic;
using System.Text;

namespace ModelCore.DataEntity
{
    public partial class ProcessRequestCondition
    {
        public enum ConditionType
        {
            ImmediateIssueNotice = 1,
            DeferredIssueNotice = 2,
            UseLastPeriodTrackCodeNo = 3,
        }
    }
}
