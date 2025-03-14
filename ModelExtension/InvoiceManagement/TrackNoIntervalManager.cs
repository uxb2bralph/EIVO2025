using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using ModelCore.DataEntity;
using ModelCore.Helper;
using ModelCore.InvoiceManagement.Validator;
using ModelCore.InvoiceManagement.zhTW;
using ModelCore.Locale;
using ModelCore.Schema.EIVO;
using CommonLib.Utility;
using CommonLib.DataAccess;
using CommonLib.Core.Utility;

namespace ModelCore.InvoiceManagement
{
    public partial class TrackNoIntervalManager : EIVOEntityManager<InvoiceNoInterval>
    {
        public TrackNoIntervalManager() : base() { }
        public TrackNoIntervalManager(GenericManager<EIVOEntityDataContext> mgr) : base(mgr) { }

        public Dictionary<int, Exception> SaveUploadBranchTrackInterval(BranchTrack item, OrganizationToken owner)
        {
            Dictionary<int, Exception> result = new Dictionary<int, Exception>();

            if (item != null && item.Main != null && item.Main.Length > 0)
            {
                //Organization donatory = owner.Organization.InvoiceWelfareAgencies.Select(w => w.WelfareAgency.Organization).FirstOrDefault();
                EventItems = new List<InvoiceNoInterval>();
                TrackNoIntervalValidator validator = new TrackNoIntervalValidator(this, owner);
                for (int idx = 0; idx < item.Main.Length; idx++)
                {
                    try
                    {
                        var invItem = item.Main[idx];

                        Exception ex;
                        if ((ex = validator.Validate(invItem)) != null)
                        {
                            result.Add(idx, ex);
                            continue;
                        }

                        var newItem = validator.DataItem;

                        this.EntityList.InsertOnSubmit(newItem);
                        this.SubmitChanges();

                        EventItems.Add(newItem);

                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                        result.Add(idx, ex);
                    }
                }

            }

            return result;
        }

        public void SettleUnassignedInvoiceNO(int sellerID, int trackID)
        {
            var assignment = this.GetTable<InvoiceTrackCodeAssignment>().Where(t => t.SellerID == sellerID
                && t.TrackID == trackID).FirstOrDefault();

            SettleUnassignedInvoiceNO(assignment);
        }

        public void SettleUnassignedInvoiceNOPeriodically(int year, int periodNo, int? sellerID = null)
        {
            IQueryable<InvoiceTrackCodeAssignment> trackCodeAssignment = this.GetTable<InvoiceTrackCodeAssignment>();
            if (sellerID.HasValue)
            {
                trackCodeAssignment = trackCodeAssignment.Where(t => t.SellerID == sellerID);
            }

            var items = this.GetTable<InvoiceTrackCode>()
                .Where(t => t.Year == year)
                .Where(t => t.PeriodNo == periodNo)
                .Join(trackCodeAssignment,
                    t => t.TrackID, a => a.TrackID, (t, a) => a);

            foreach (var item in items)
            {
                SettleUnassignedInvoiceNO(item);
            }
        }

        public IQueryable<InvoiceTrackCodeAssignment> PromptTrackCodeAssignment(int year, int periodNo)
        {
            IQueryable<InvoiceTrackCodeAssignment> trackCodeAssignment = this.GetTable<InvoiceTrackCodeAssignment>();

            var items = this.GetTable<InvoiceTrackCode>()
                .Where(t => t.Year == year)
                .Where(t => t.PeriodNo == periodNo)
                .Join(trackCodeAssignment,
                    t => t.TrackID, a => a.TrackID, (t, a) => a);

            return items;
        }


        public void SettleUnassignedInvoiceNO(InvoiceTrackCodeAssignment assignment)
        {
            if (assignment == null)
                return;

            var noIntervals = this.EntityList.Where(i => i.TrackID == assignment.TrackID && i.SellerID == assignment.SellerID);
            this.DeleteAll<UnassignedInvoiceNo>(u => u.SellerID == assignment.SellerID && u.TrackID == assignment.TrackID);

            var trackCodeItem = assignment.InvoiceTrackCode;

            foreach (var interval in noIntervals)
            {
                String startNo = String.Format("{0:00000000}", interval.StartNo),
                    endNo = String.Format("{0:00000000}", interval.EndNo);
                var items = this.GetTable<InvoiceItem>().Where(i => i.SellerID == assignment.SellerID && i.TrackCode == trackCodeItem.TrackCode
                    && String.Compare(i.No, startNo) >= 0 && String.Compare(i.No, endNo) <= 0);

                int recordCount = interval.EndNo - interval.StartNo + 1;

                if (items.Count() >= recordCount)
                    continue;

                var table = this.GetTable<UnassignedInvoiceNo>();

                List<int> allInvoiceNo = Enumerable.Range(interval.StartNo, recordCount).ToList();
                foreach (var item in items)
                {
                    if (int.TryParse(item.No, out int no))
                    {
                        allInvoiceNo.Remove(no);
                    }
                }

                Console.WriteLine($"\r\ninterval({interval.IntervalID}) unassigned numbers count: {allInvoiceNo.Count}");

                int startIndex = allInvoiceNo[0];
                int count = 1;
                for (int idx = 1; idx < allInvoiceNo.Count; idx++)
                {
                    if ((startIndex + count) == allInvoiceNo[idx])
                    {
                        count++;
                        continue;
                    }
                    else
                    {
                        table.InsertOnSubmit(
                            new UnassignedInvoiceNo
                            {
                                InvoiceBeginNo = startIndex,
                                InvoiceEndNo = startIndex + count - 1,
                                SellerID = assignment.SellerID,
                                TrackID = assignment.TrackID
                            });

                        count = 1;
                        startIndex = allInvoiceNo[idx];

                        Console.Write("+");
                        this.SubmitChanges();
                    }
                }

                if (count > 0)
                {
                    table.InsertOnSubmit(
                        new UnassignedInvoiceNo
                        {
                            InvoiceBeginNo = startIndex,
                            InvoiceEndNo = startIndex + count - 1,
                            SellerID = assignment.SellerID,
                            TrackID = assignment.TrackID
                        });

                    Console.Write("+");
                    this.SubmitChanges();
                }

            }
        }

        public void SettleVacantInvoiceNo(int year, int periodNo, int? sellerID = null)
        {
            IQueryable<InvoiceTrackCodeAssignment> trackCodeAssignment = this.GetTable<InvoiceTrackCodeAssignment>();
            if (sellerID.HasValue)
            {
                trackCodeAssignment = trackCodeAssignment.Where(t => t.SellerID == sellerID);
            }

            var items = this.GetTable<InvoiceTrackCode>()
                .Where(t => t.Year == year)
                .Where(t => t.PeriodNo == periodNo)
                .Join(trackCodeAssignment,
                    t => t.TrackID, a => a.TrackID, (t, a) => a)
                .Join(this.GetTable<InvoiceNoInterval>(), a => new { a.TrackID, a.SellerID }, n => new { n.TrackID, n.SellerID }, (a, n) => n);

            foreach (var item in items)
            {
                SettleVacantInvoiceNo(item);
            }
        }

        public void SettleVacantInvoiceNo(InvoiceNoInterval interval)
        {
            String startNo = String.Format("{0:00000000}", interval.StartNo),
                endNo = String.Format("{0:00000000}", interval.EndNo);

            var trackCode = interval.InvoiceTrackCodeAssignment;
            var items = this.GetTable<InvoiceItem>().Where(i => i.SellerID == trackCode.SellerID 
                && i.TrackCode == trackCode.InvoiceTrackCode.TrackCode
                && String.Compare(i.No, startNo) >= 0 && String.Compare(i.No, endNo) <= 0);

            int recordCount = interval.EndNo - interval.StartNo + 1;

            if (items.Count() == recordCount)
                return;

            List<int> allInvoiceNo = Enumerable.Range(interval.StartNo, recordCount).ToList();
            List<int> issuedNo = items.Select(i => int.Parse(i.No)).ToList();

            var vacancyNo = allInvoiceNo.Except(issuedNo);

            this.ExecuteCommand("delete VacantInvoiceNo where IntervalID = {0}", interval.IntervalID);
            var table = this.GetTable<VacantInvoiceNo>();

            foreach (var no in vacancyNo)
            {
                table.InsertOnSubmit(new VacantInvoiceNo 
                {
                    InvoiceNo = no,
                    IntervalID = interval.IntervalID
                });
                this.SubmitChanges();
            }

            this.ExecuteCommand(@"
                UPDATE          VacantInvoiceNo
                SET                   NextID = v2.VacancyID
                FROM              VacantInvoiceNo INNER JOIN
                                            VacantInvoiceNo AS v2 ON VacantInvoiceNo.IntervalID = v2.IntervalID AND 
                                            VacantInvoiceNo.InvoiceNo + 1 = v2.InvoiceNo
                WHERE          (VacantInvoiceNo.IntervalID = {0})", interval.IntervalID);

            this.ExecuteCommand(@"
                UPDATE          VacantInvoiceNo
                SET                   PrevID = v2.VacancyID
                FROM              VacantInvoiceNo INNER JOIN
                                            VacantInvoiceNo AS v2 ON VacantInvoiceNo.IntervalID = v2.IntervalID AND 
                                            VacantInvoiceNo.InvoiceNo - 1 = v2.InvoiceNo
                WHERE          (VacantInvoiceNo.IntervalID = {0})", interval.IntervalID);

            Logger.Info($"Processed IntervalID: {interval.IntervalID}");

        }
    }
}
