using ModelCore.DataEntity;
using ModelCore.Locale;

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonLib.Utility;
using CommonLib.Core.Utility;
using ModelCore.Notification;
using Microsoft.Data.SqlClient;

namespace ProcessorUnit.Execution
{
    public class ProcessRequestExecutorForever : ExecutorForeverBase
    {

        protected ProcessRequestQueue queueItem = null!;
        protected Naming.InvoiceProcessType? appliedProcessType;
        protected override void DoSomething()
        {
            using (models = new ModelSource<InvoiceItem>())
            {
                try
                {
                    var taskIdParameter = new SqlParameter("@TaskID", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output,
                        Value = DBNull.Value
                    };

                    _ = models.ExecuteCommand(
                        "exec [proc].ApplyProcessRequest @ProcessorID, @ProcessType, @TaskID OUTPUT",
                        new SqlParameter("@ProcessorID", ProcessorUnit.Properties.AppSettings.Default.ProcessorID),
                        new SqlParameter("@ProcessType", SqlDbType.Int) { Value = (object?)(int?)appliedProcessType ?? DBNull.Value },
                        taskIdParameter);

                    int? taskID = taskIdParameter.Value is int value ? value : null;

                    if (taskID.HasValue)
                    {
                        queueItem = models.GetTable<ProcessRequestQueue>().Where(q => q.TaskID == taskID).First();
                        ProcessRequestItem();
                    }

                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                    if (queueItem != null)
                    {
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
