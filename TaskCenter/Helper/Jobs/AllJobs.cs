using CommonLib.Core.Helper;
using CommonLib.Core.Utility;
using ModelCore.InvoiceManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TaskCenter.Helper.Jobs
{
    public class AllJobs
    {
        static AllJobs()
        {
            var jobList = JobScheduler.JobList;

            if (jobList == null || !jobList.Any(j => j.AssemblyQualifiedName == typeof(MatchAttachment).AssemblyQualifiedName))
            {
                JobScheduler.AddJob(new JobItem
                {
                    AssemblyQualifiedName = typeof(MatchAttachment)?.AssemblyQualifiedName,
                    Description = "對應發票附件",
                    Schedule = DateTime.Now
                });
            }
        }

        public static void StartUp()
        {

        }
    }

    public class MatchAttachment : IJob
    {

        public void Dispose()
        {
        }

        public void DoJob()
        {
            Logger.Info($"MatchAttachment => {AttachmentManager.MatchAttachment()}");
        }

        public DateTime GetScheduleToNextTurn(DateTime current)
        {
            return current.AddMinutes(5);
        }
    }
}