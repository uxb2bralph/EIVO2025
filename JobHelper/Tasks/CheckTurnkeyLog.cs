using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonLib.Utility;
using ModelCore.DataEntity;
using ModelCore.Helper;
using JobHelper.Properties;
using CommonLib.Helper;
using CommonLib.DataAccess;
using CommonLib.Core.Utility;

namespace JobHelper.Tasks
{
    public static class CheckTurnkeyLog
    {
        private static QueuedProcessHandler __Handler;

        static CheckTurnkeyLog()
        {
            lock (typeof(CheckTurnkeyLog))
            {
                if (__Handler == null)
                {
                    __Handler = new QueuedProcessHandler(FileLogger.Logger)
                    {
                        Process = () =>
                        {
                            if(AppSettings.Default.ActiveEIVODBConnection?.Length>0)
                            {
                                foreach (var conn in AppSettings.Default.ActiveEIVODBConnection)
                                {
                                    DoCheckTurnkeyLog(conn);
                                }
                            }
                            else
                            {
                                DoCheckTurnkeyLog(global::ModelCore.Properties.AppSettings.Default.ConnectionString);
                            }
                        },
                        PeriodInMinutes = 5,
                    };
                }
            }
        }

        private static void DoCheckTurnkeyLog(String connString = null)
        {
            try
            {
                int idx = 0;
                TurnKey2DataContext turnkeyDB = new TurnKey2DataContext();
                {
                    EIVOEntityDataContext db = new EIVOEntityDataContext(connString);
                    {
                        ModelSource models = new ModelSource(new GenericManager<EIVOEntityDataContext>(db));
                        {
                            TurnkeyTriggerLog log = turnkeyDB.GetTable<TurnkeyTriggerLog>().FirstOrDefault();
                            if (log != null)
                            {
                                Console.WriteLine($"Turnkey log starts at {DateTime.Now}...");
                            }
                            while (log != null)
                            {
                                if ((++idx) % 1024 == 0)
                                {
                                    turnkeyDB.Dispose();
                                    turnkeyDB = new TurnKey2DataContext();
                                    db.Dispose();
                                    db = new EIVOEntityDataContext(connString);
                                    models.Dispose();
                                    models = new ModelSource(new GenericManager<EIVOEntityDataContext>(db));
                                }

                                var result = turnkeyDB.ExecuteCommand(
                                    @"Update TurnkeyTriggerLog set LockID = 1
                                                    WHERE (LogID = {0}) AND LockID is null", log.LogID);

                                if (result > 0)
                                {
                                    String dataNo = log.INVOICE_IDENTIFIER.Substring(5, log.INVOICE_IDENTIFIER.Length - 13);
                                    var docID = models.TurnkeyLogFeedback(log.MESSAGE_TYPE, log.STATUS, dataNo);

                                    if (docID.HasValue)
                                    {
                                        turnkeyDB.ExecuteCommand(
                                            @"Delete TurnkeyTriggerLog WHERE (LogID = {0})", log.LogID);
                                    }
                                    else
                                    {
                                        turnkeyDB.ExecuteCommand(
                                            @"Update TurnkeyTriggerLog set LockID = null
                                                            WHERE (LogID = {0})", log.LogID);
                                    }
                                }

                                log = turnkeyDB.GetTable<TurnkeyTriggerLog>()
                                    .Where(l => l.LogID > log.LogID || l.LogID < 1)
                                    .FirstOrDefault();
                            }
                            Console.WriteLine($"Turnkey log ends at {DateTime.Now}...");
                        }
                        models.Dispose();
                    }
                    db.Dispose();
                }
                turnkeyDB.Dispose();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        public static int ResetBusyCount()
        {
            return __Handler.ResetBusyCount();
        }

        public static void Notify()
        {
            __Handler.Notify();
        }
    }

}
