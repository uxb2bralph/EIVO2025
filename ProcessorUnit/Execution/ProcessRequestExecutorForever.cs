using ModelCore.DataEntity;
using ModelCore.Locale;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonLib.Utility;
using CommonLib.Core.Utility;
using ModelCore.Notification;

namespace ProcessorUnit.Execution
{
    public class ProcessRequestExecutorForever : ExecutorForeverBase
    {

        protected ProcessRequestQueue queueItem = null!;
        protected Naming.InvoiceProcessType? appliedProcessType;
        protected override void DoSomething()
        {
            int? taskID = null;
            using (models = new ModelSource<InvoiceItem>())
            {
                try
                {

                    _ = models.ExecuteCommand("exec ApplyProcessRequest @ProcessorID, @ProcessType, @TaskID OUTPUT", new { ProcessorID = ProcessorUnit.Properties.AppSettings.Default.ProcessorID, ProcessType = (int?)appliedProcessType, TaskID = taskID });

                    if (taskID.HasValue)
                    {
                        queueItem = models.GetTable<ProcessRequestQueue>().Where(q => q.TaskID == taskID).First();
                        ProcessRequestItem();
                    }

                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                    queueItem.ProcessRequest.ExceptionLog = new ExceptionLog
                    {
                        DataContent = ex.Message
                    };
                    queueItem.ProcessRequest.ProcessComplete = DateTime.Now;
                    models.PushProcessExceptionNotification(queueItem.ProcessRequest, queueItem.ProcessRequest.Agent!, DateTime.Now);

                    models.GetTable<ProcessRequestQueue>().Remove(queueItem);
                    models.SubmitChanges();

                }
            }

            base.DoSomething();
        }

        protected virtual void ProcessRequestItem()
        {

        }

        public virtual void ProcessRequestItem(int taskID) 
        {
            using (models = new ModelSource<InvoiceItem>())
            {
                try
                {
                    var requestItem = models.GetTable<ProcessRequest>()
                        .Where(q => q.TaskID == taskID)
                        .Where(q => q.ProcessType == (int?)appliedProcessType)
                        .FirstOrDefault();

                    if (requestItem!=null)
                    {
                        queueItem = models.GetTable<ProcessRequestQueue>().Where(q => q.TaskID == taskID).FirstOrDefault()!;
                        if (queueItem == null)
                        {
                            queueItem = new ProcessRequestQueue
                            {
                                TaskID = taskID,
                                ProcessRequest = requestItem
                            };
                        }
                        if (queueItem != null)
                        {
                            ProcessRequestItem();
                        }
                    }

                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                    queueItem.ProcessRequest.ExceptionLog = new ExceptionLog
                    {
                        DataContent = ex.Message
                    };
                    queueItem.ProcessRequest.ProcessComplete = DateTime.Now;
                    models.PushProcessExceptionNotification(queueItem.ProcessRequest, queueItem.ProcessRequest.Agent!, DateTime.Now);

                    models.GetTable<ProcessRequestQueue>().Remove(queueItem);
                    models.SubmitChanges();

                }
            }

        }
    }
}
