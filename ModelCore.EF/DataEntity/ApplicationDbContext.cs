using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ModelCore.DataEntity;

public partial class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Attachment> Attachment { get; set; }

    public virtual DbSet<AuthorizeToVoid> AuthorizeToVoid { get; set; }

    public virtual DbSet<BillSubmission> BillSubmission { get; set; }

    public virtual DbSet<BillingExtension> BillingExtension { get; set; }

    public virtual DbSet<BillingGrade> BillingGrade { get; set; }

    public virtual DbSet<BillingIncrement> BillingIncrement { get; set; }

    public virtual DbSet<BusinessRelationship> BusinessRelationship { get; set; }

    public virtual DbSet<BusinessType> BusinessType { get; set; }

    public virtual DbSet<CDS_Document> CDS_Document { get; set; }

    public virtual DbSet<CategoryDefinition> CategoryDefinition { get; set; }

    public virtual DbSet<CurrencyType> CurrencyType { get; set; }

    public virtual DbSet<CustomSmtpHost> CustomSmtpHost { get; set; }

    public virtual DbSet<CustomerDefined> CustomerDefined { get; set; }

    public virtual DbSet<DataNotice> DataNotice { get; set; }

    public virtual DbSet<DataProcessLog> DataProcessLog { get; set; }

    public virtual DbSet<DataProcessQueue> DataProcessQueue { get; set; }

    public virtual DbSet<DerivedDocument> DerivedDocument { get; set; }

    public virtual DbSet<DocumentAuthorization> DocumentAuthorization { get; set; }

    public virtual DbSet<DocumentDownloadLog> DocumentDownloadLog { get; set; }

    public virtual DbSet<DocumentDownloadQueue> DocumentDownloadQueue { get; set; }

    public virtual DbSet<DocumentFlow> DocumentFlow { get; set; }

    public virtual DbSet<DocumentFlowBranch> DocumentFlowBranch { get; set; }

    public virtual DbSet<DocumentFlowControl> DocumentFlowControl { get; set; }

    public virtual DbSet<DocumentFlowStep> DocumentFlowStep { get; set; }

    public virtual DbSet<DocumentMappingQueue> DocumentMappingQueue { get; set; }

    public virtual DbSet<DocumentOwner> DocumentOwner { get; set; }

    public virtual DbSet<DocumentPostLog> DocumentPostLog { get; set; }

    public virtual DbSet<DocumentPrintLog> DocumentPrintLog { get; set; }

    public virtual DbSet<DocumentPrintQueue> DocumentPrintQueue { get; set; }

    public virtual DbSet<DocumentProcessLog> DocumentProcessLog { get; set; }

    public virtual DbSet<DocumentReasonForRefusal> DocumentReasonForRefusal { get; set; }

    public virtual DbSet<DocumentReplication> DocumentReplication { get; set; }

    public virtual DbSet<DocumentReturn> DocumentReturn { get; set; }

    public virtual DbSet<DocumentSubscriptionQueue> DocumentSubscriptionQueue { get; set; }

    public virtual DbSet<DocumentType> DocumentType { get; set; }

    public virtual DbSet<DocumentTypeFlow> DocumentTypeFlow { get; set; }

    public virtual DbSet<EnterpriseGroup> EnterpriseGroup { get; set; }

    public virtual DbSet<EnterpriseGroupMember> EnterpriseGroupMember { get; set; }

    public virtual DbSet<EnterpriseGroupMemberToken> EnterpriseGroupMemberToken { get; set; }

    public virtual DbSet<ExceptionLog> ExceptionLog { get; set; }

    public virtual DbSet<ExceptionReplication> ExceptionReplication { get; set; }

    public virtual DbSet<ExtraBillingItem> ExtraBillingItem { get; set; }

    public virtual DbSet<InboxItems> InboxItems { get; set; }

    public virtual DbSet<InvoiceAllowance> InvoiceAllowance { get; set; }

    public virtual DbSet<InvoiceAllowanceBuyer> InvoiceAllowanceBuyer { get; set; }

    public virtual DbSet<InvoiceAllowanceCancellation> InvoiceAllowanceCancellation { get; set; }

    public virtual DbSet<InvoiceAllowanceItem> InvoiceAllowanceItem { get; set; }

    public virtual DbSet<InvoiceAllowanceItemExtension> InvoiceAllowanceItemExtension { get; set; }

    public virtual DbSet<InvoiceAllowanceSeller> InvoiceAllowanceSeller { get; set; }

    public virtual DbSet<InvoiceAmountType> InvoiceAmountType { get; set; }

    public virtual DbSet<InvoiceBusiness> InvoiceBusiness { get; set; }

    public virtual DbSet<InvoiceBuyer> InvoiceBuyer { get; set; }

    public virtual DbSet<InvoiceByHousehold> InvoiceByHousehold { get; set; }

    public virtual DbSet<InvoiceCancellation> InvoiceCancellation { get; set; }

    public virtual DbSet<InvoiceCancellationUpload> InvoiceCancellationUpload { get; set; }

    public virtual DbSet<InvoiceCarrier> InvoiceCarrier { get; set; }

    public virtual DbSet<InvoiceDeliveryTracking> InvoiceDeliveryTracking { get; set; }

    public virtual DbSet<InvoiceDonation> InvoiceDonation { get; set; }

    public virtual DbSet<InvoiceIssuerAgent> InvoiceIssuerAgent { get; set; }

    public virtual DbSet<InvoiceItem> InvoiceItem { get; set; }

    public virtual DbSet<InvoiceItemExtension> InvoiceItemExtension { get; set; }

    public virtual DbSet<InvoiceMail> InvoiceMail { get; set; }

    public virtual DbSet<InvoiceMailTracking> InvoiceMailTracking { get; set; }

    public virtual DbSet<InvoiceNoAllocation> InvoiceNoAllocation { get; set; }

    public virtual DbSet<InvoiceNoAssignment> InvoiceNoAssignment { get; set; }

    public virtual DbSet<InvoiceNoInterval> InvoiceNoInterval { get; set; }

    public virtual DbSet<InvoiceNoMainAssignment> InvoiceNoMainAssignment { get; set; }

    public virtual DbSet<InvoiceNoSegment> InvoiceNoSegment { get; set; }

    public virtual DbSet<InvoiceNoSegmentDisposition> InvoiceNoSegmentDisposition { get; set; }

    public virtual DbSet<InvoicePaperRequest> InvoicePaperRequest { get; set; }

    public virtual DbSet<InvoicePeriod> InvoicePeriod { get; set; }

    public virtual DbSet<InvoicePeriodExchangeRate> InvoicePeriodExchangeRate { get; set; }

    public virtual DbSet<InvoicePrintAssertion> InvoicePrintAssertion { get; set; }

    public virtual DbSet<InvoicePrintQueue> InvoicePrintQueue { get; set; }

    public virtual DbSet<InvoicePrizeWinningNumbers> InvoicePrizeWinningNumbers { get; set; }

    public virtual DbSet<InvoiceProduct> InvoiceProduct { get; set; }

    public virtual DbSet<InvoiceProductItem> InvoiceProductItem { get; set; }

    public virtual DbSet<InvoicePurchaseOrder> InvoicePurchaseOrder { get; set; }

    public virtual DbSet<InvoicePurchaseOrderAudit> InvoicePurchaseOrderAudit { get; set; }

    public virtual DbSet<InvoicePurchaseOrderUpload> InvoicePurchaseOrderUpload { get; set; }

    public virtual DbSet<InvoiceSeller> InvoiceSeller { get; set; }

    public virtual DbSet<InvoiceTrackCode> InvoiceTrackCode { get; set; }

    public virtual DbSet<InvoiceTrackCodeAssignment> InvoiceTrackCodeAssignment { get; set; }

    public virtual DbSet<InvoiceUserCarrier> InvoiceUserCarrier { get; set; }

    public virtual DbSet<InvoiceUserCarrierType> InvoiceUserCarrierType { get; set; }

    public virtual DbSet<InvoiceWelfareAgency> InvoiceWelfareAgency { get; set; }

    public virtual DbSet<InvoiceWinningNumber> InvoiceWinningNumber { get; set; }

    public virtual DbSet<IssuingNotice> IssuingNotice { get; set; }

    public virtual DbSet<LevelExpression> LevelExpression { get; set; }

    public virtual DbSet<MasterOrganization> MasterOrganization { get; set; }

    public virtual DbSet<MemberCode> MemberCode { get; set; }

    public virtual DbSet<MenuControl> MenuControl { get; set; }

    public virtual DbSet<MessageType> MessageType { get; set; }

    public virtual DbSet<MonthlyBilling> MonthlyBilling { get; set; }

    public virtual DbSet<MonthlyExtraBilling> MonthlyExtraBilling { get; set; }

    public virtual DbSet<Organization> Organization { get; set; }

    public virtual DbSet<OrganizationBranch> OrganizationBranch { get; set; }

    public virtual DbSet<OrganizationCategory> OrganizationCategory { get; set; }

    public virtual DbSet<OrganizationCategoryUserRole> OrganizationCategoryUserRole { get; set; }

    public virtual DbSet<OrganizationCustomSetting> OrganizationCustomSetting { get; set; }

    public virtual DbSet<OrganizationDepartment> OrganizationDepartment { get; set; }

    public virtual DbSet<OrganizationExtension> OrganizationExtension { get; set; }

    public virtual DbSet<OrganizationSettings> OrganizationSettings { get; set; }

    public virtual DbSet<OrganizationStatus> OrganizationStatus { get; set; }

    public virtual DbSet<OrganizationToken> OrganizationToken { get; set; }

    public virtual DbSet<POSDevice> POSDevice { get; set; }

    public virtual DbSet<POSInvoiceNoSegment> POSInvoiceNoSegment { get; set; }

    public virtual DbSet<ProcessCompletionNotification> ProcessCompletionNotification { get; set; }

    public virtual DbSet<ProcessExceptionNotification> ProcessExceptionNotification { get; set; }

    public virtual DbSet<ProcessRequest> ProcessRequest { get; set; }

    public virtual DbSet<ProcessRequestCondition> ProcessRequestCondition { get; set; }

    public virtual DbSet<ProcessRequestDocument> ProcessRequestDocument { get; set; }

    public virtual DbSet<ProcessRequestQueue> ProcessRequestQueue { get; set; }

    public virtual DbSet<ProcessRequestType> ProcessRequestType { get; set; }

    public virtual DbSet<ProcessRequestTypeLocale> ProcessRequestTypeLocale { get; set; }

    public virtual DbSet<ProcessorUnit> ProcessorUnit { get; set; }

    public virtual DbSet<ProductCatalog> ProductCatalog { get; set; }

    public virtual DbSet<ProductItemCategory> ProductItemCategory { get; set; }

    public virtual DbSet<ReceiptCancellation> ReceiptCancellation { get; set; }

    public virtual DbSet<ReceiptDetail> ReceiptDetail { get; set; }

    public virtual DbSet<ReceiptItem> ReceiptItem { get; set; }

    public virtual DbSet<ReplicationNotification> ReplicationNotification { get; set; }

    public virtual DbSet<ResetUserPassword> ResetUserPassword { get; set; }

    public virtual DbSet<SMSNotificationLog> SMSNotificationLog { get; set; }

    public virtual DbSet<SMSNotificationQueue> SMSNotificationQueue { get; set; }

    public virtual DbSet<Settlement> Settlement { get; set; }

    public virtual DbSet<SystemEvent> SystemEvent { get; set; }

    public virtual DbSet<SystemMessage> SystemMessage { get; set; }

    public virtual DbSet<UnassignedInvoiceNo> UnassignedInvoiceNo { get; set; }

    public virtual DbSet<UniformInvoiceWinningNumber> UniformInvoiceWinningNumber { get; set; }

    public virtual DbSet<UserAuth> UserAuth { get; set; }

    public virtual DbSet<UserInbox> UserInbox { get; set; }

    public virtual DbSet<UserMail> UserMail { get; set; }

    public virtual DbSet<UserMenu> UserMenu { get; set; }

    public virtual DbSet<UserProfile> UserProfile { get; set; }

    public virtual DbSet<UserProfileExtension> UserProfileExtension { get; set; }

    public virtual DbSet<UserProfileProperty> UserProfileProperty { get; set; }

    public virtual DbSet<UserProfileStatus> UserProfileStatus { get; set; }

    public virtual DbSet<UserRole> UserRole { get; set; }

    public virtual DbSet<UserRoleDefinition> UserRoleDefinition { get; set; }

    public virtual DbSet<UserToken> UserToken { get; set; }

    public virtual DbSet<VacantInvoiceNo> VacantInvoiceNo { get; set; }

    public virtual DbSet<VoidInvoiceRequest> VoidInvoiceRequest { get; set; }

    public virtual DbSet<WelfareAgency> WelfareAgency { get; set; }

    public virtual DbSet<WelfareReplication> WelfareReplication { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Attachment>(entity =>
        {
            entity.HasKey(e => e.KeyName);

            entity.HasIndex(e => e.DocID, "IX_Attachment");

            entity.Property(e => e.KeyName).HasMaxLength(256);
            entity.Property(e => e.StoredPath).HasMaxLength(256);

            entity.HasOne(d => d.Doc).WithMany(p => p.Attachment)
                .HasForeignKey(d => d.DocID)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Attachment_CDS_Document");
        });

        modelBuilder.Entity<AuthorizeToVoid>(entity =>
        {
            entity.HasKey(e => e.InvoiceID);

            entity.Property(e => e.InvoiceID)
                .ValueGeneratedNever()
                .HasComment("Primary Key");

            entity.HasOne(d => d.Invoice).WithOne(p => p.AuthorizeToVoid)
                .HasForeignKey<AuthorizeToVoid>(d => d.InvoiceID)
                .HasConstraintName("FK_AuthorizeToVoid_InvoiceItem");
        });

        modelBuilder.Entity<BillSubmission>(entity =>
        {
            entity.HasKey(e => e.BillID);

            entity.ToTable("BillSubmission", "billing");

            entity.Property(e => e.BillDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<BillingExtension>(entity =>
        {
            entity.HasKey(e => e.CompanyID);

            entity.ToTable("BillingExtension", "billing");

            entity.Property(e => e.CompanyID)
                .ValueGeneratedNever()
                .HasComment("主鍵");

            entity.HasOne(d => d.Company).WithOne(p => p.BillingExtension)
                .HasForeignKey<BillingExtension>(d => d.CompanyID)
                .HasConstraintName("FK_BillingExtension_Organization");
        });

        modelBuilder.Entity<BillingGrade>(entity =>
        {
            entity.HasKey(e => new { e.CompanyID, e.GradeCount });

            entity.ToTable("BillingGrade", "billing");

            entity.Property(e => e.CompanyID).HasComment("主鍵");

            entity.HasOne(d => d.Company).WithMany(p => p.BillingGrade)
                .HasForeignKey(d => d.CompanyID)
                .HasConstraintName("FK_BillingGrade_Organization");
        });

        modelBuilder.Entity<BillingIncrement>(entity =>
        {
            entity.HasKey(e => new { e.CompanyID, e.UpperBound });

            entity.ToTable("BillingIncrement", "billing");

            entity.Property(e => e.CompanyID).HasComment("主鍵");
            entity.Property(e => e.UnitFee).HasColumnType("decimal(12, 4)");

            entity.HasOne(d => d.Company).WithMany(p => p.BillingIncrement)
                .HasForeignKey(d => d.CompanyID)
                .HasConstraintName("FK_BillingIncrement_Organization");
        });

        modelBuilder.Entity<BusinessRelationship>(entity =>
        {
            entity.HasKey(e => new { e.MasterID, e.RelativeID, e.BusinessID });

            entity.ToTable("BusinessRelationship", "center");

            entity.Property(e => e.Addr)
                .HasMaxLength(256)
                .HasComment("地址");
            entity.Property(e => e.CompanyName)
                .HasMaxLength(128)
                .HasComment("機關名稱");
            entity.Property(e => e.ContactEmail)
                .HasMaxLength(512)
                .HasComment("連絡人電子郵件");
            entity.Property(e => e.CustomerNo).HasMaxLength(16);
            entity.Property(e => e.Phone)
                .HasMaxLength(64)
                .HasComment("電話");

            entity.HasOne(d => d.Business).WithMany(p => p.BusinessRelationship)
                .HasForeignKey(d => d.BusinessID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BusinessRelationship_BusinessType");

            entity.HasOne(d => d.CurrentLevelNavigation).WithMany(p => p.BusinessRelationship)
                .HasForeignKey(d => d.CurrentLevel)
                .HasConstraintName("FK_BusinessRelationship_LevelExpression");

            entity.HasOne(d => d.Master).WithMany(p => p.BusinessRelationshipMaster)
                .HasForeignKey(d => d.MasterID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BusinessRelationship_Organization2");

            entity.HasOne(d => d.Relative).WithMany(p => p.BusinessRelationshipRelative)
                .HasForeignKey(d => d.RelativeID)
                .HasConstraintName("FK_BusinessRelationship_Organization3");
        });

        modelBuilder.Entity<BusinessType>(entity =>
        {
            entity.HasKey(e => e.BusinessID);

            entity.ToTable("BusinessType", "center");

            entity.Property(e => e.BusinessID).ValueGeneratedNever();
            entity.Property(e => e.Business).HasMaxLength(64);
        });

        modelBuilder.Entity<CDS_Document>(entity =>
        {
            entity.HasKey(e => e.DocID);

            entity.ToTable(tb => tb.HasComment("系統文件主檔"));

            entity.HasIndex(e => e.DocType, "IX_CDS_Document");

            entity.Property(e => e.DocDate)
                .HasComment("文件建立時間")
                .HasDefaultValueSql("(getdate())", "DF_CDS_Document_DocDate")
                .HasColumnType("datetime");

            entity.HasOne(d => d.CurrentStepNavigation).WithMany(p => p.CDS_Document)
                .HasForeignKey(d => d.CurrentStep)
                .HasConstraintName("FK_CDS_Document_LevelExpression");

            entity.HasOne(d => d.DocTypeNavigation).WithMany(p => p.CDS_Document)
                .HasForeignKey(d => d.DocType)
                .HasConstraintName("FK_CDS_Document_DocumentType");

            entity.HasMany(d => d.Type).WithMany(p => p.Doc)
                .UsingEntity<Dictionary<string, object>>(
                    "DocumentDispatch",
                    r => r.HasOne<DocumentType>().WithMany()
                        .HasForeignKey("TypeID")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_DocumentDispatch_DocumentType"),
                    l => l.HasOne<CDS_Document>().WithMany()
                        .HasForeignKey("DocID")
                        .HasConstraintName("FK_DocumentDispatch_CDS_Document"),
                    j =>
                    {
                        j.HasKey("DocID", "TypeID");
                    });
        });

        modelBuilder.Entity<CategoryDefinition>(entity =>
        {
            entity.HasKey(e => e.CategoryID).HasName("PK_OrganizationDefinition");

            entity.ToTable(tb => tb.HasComment("機關類別定義檔"));

            entity.HasIndex(e => e.Category, "IX_OrganizationDefinition").IsUnique();

            entity.Property(e => e.CategoryID).ValueGeneratedNever();
            entity.Property(e => e.Category)
                .HasMaxLength(32)
                .HasComment("分類名稱");
            entity.Property(e => e.CharacterURL)
                .HasMaxLength(64)
                .HasComment("圖識來源網址");
        });

        modelBuilder.Entity<CurrencyType>(entity =>
        {
            entity.HasKey(e => e.CurrencyID);

            entity.ToTable(tb => tb.HasComment("幣別主檔"));

            entity.Property(e => e.CurrencyID).ValueGeneratedNever();
            entity.Property(e => e.AbbrevName)
                .HasMaxLength(16)
                .HasComment("簡稱");
            entity.Property(e => e.CurrencyName)
                .HasMaxLength(64)
                .HasComment("貨幣完整名稱");
            entity.Property(e => e.FormatPattern).HasMaxLength(24);
        });

        modelBuilder.Entity<CustomSmtpHost>(entity =>
        {
            entity.HasKey(e => e.HostID);

            entity.Property(e => e.CompanyID).HasComment("主鍵");
            entity.Property(e => e.Host).HasMaxLength(64);
            entity.Property(e => e.MailFrom).HasMaxLength(64);
            entity.Property(e => e.Password).HasMaxLength(64);
            entity.Property(e => e.UserName).HasMaxLength(64);

            entity.HasOne(d => d.Company).WithMany(p => p.CustomSmtpHost)
                .HasForeignKey(d => d.CompanyID)
                .HasConstraintName("FK_CustomSmtpHost_Organization");
        });

        modelBuilder.Entity<CustomerDefined>(entity =>
        {
            entity.HasKey(e => e.DocID);

            entity.Property(e => e.DocID).ValueGeneratedNever();
            entity.Property(e => e.IsolationFolder).HasMaxLength(64);

            entity.HasOne(d => d.Doc).WithOne(p => p.CustomerDefined)
                .HasForeignKey<CustomerDefined>(d => d.DocID)
                .HasConstraintName("FK_CustomerDefined_CDS_Document");
        });

        modelBuilder.Entity<DataNotice>(entity =>
        {
            entity.HasKey(e => e.NoticeID);

            entity.ToTable("DataNotice", "proc");
        });

        modelBuilder.Entity<DataProcessLog>(entity =>
        {
            entity.HasKey(e => e.LogID);

            entity.ToTable("DataProcessLog", "proc");

            entity.Property(e => e.LogDate).HasColumnType("datetime");

            entity.HasOne(d => d.Doc).WithMany(p => p.DataProcessLog)
                .HasForeignKey(d => d.DocID)
                .HasConstraintName("FK_DataProcessLog_CDS_Document");

            entity.HasOne(d => d.Notice).WithMany(p => p.DataProcessLog)
                .HasForeignKey(d => d.NoticeID)
                .HasConstraintName("FK_DataProcessLog_DataNotice");
        });

        modelBuilder.Entity<DataProcessQueue>(entity =>
        {
            entity.HasKey(e => new { e.DocID, e.StepID, e.ProcessType });

            entity.ToTable("DataProcessQueue", "proc");

            entity.Property(e => e.BookingTime).HasColumnType("datetime");
            entity.Property(e => e.DispatchDate).HasColumnType("datetime");

            entity.HasOne(d => d.Doc).WithMany(p => p.DataProcessQueue)
                .HasForeignKey(d => d.DocID)
                .HasConstraintName("FK_DataProcessQueue_CDS_Document");

            entity.HasOne(d => d.Notice).WithMany(p => p.DataProcessQueue)
                .HasForeignKey(d => d.NoticeID)
                .HasConstraintName("FK_DataProcessQueue_DataNotice");
        });

        modelBuilder.Entity<DerivedDocument>(entity =>
        {
            entity.HasKey(e => e.DocID);

            entity.ToTable(tb => tb.HasTrigger("TR_DerivedDocument"));

            entity.Property(e => e.DocID).ValueGeneratedNever();

            entity.HasOne(d => d.Doc).WithOne(p => p.DerivedDocumentDoc)
                .HasForeignKey<DerivedDocument>(d => d.DocID)
                .HasConstraintName("FK_DerivedDocument_CDS_Document");

            entity.HasOne(d => d.Source).WithMany(p => p.DerivedDocumentSource)
                .HasForeignKey(d => d.SourceID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DerivedDocument_CDS_Document1");
        });

        modelBuilder.Entity<DocumentAuthorization>(entity =>
        {
            entity.HasKey(e => e.DocID);

            entity.Property(e => e.DocID).ValueGeneratedNever();

            entity.HasOne(d => d.Doc).WithOne(p => p.DocumentAuthorization)
                .HasForeignKey<DocumentAuthorization>(d => d.DocID)
                .HasConstraintName("FK_DocumentAuthorization_CDS_Document");
        });

        modelBuilder.Entity<DocumentDownloadLog>(entity =>
        {
            entity.HasKey(e => e.LogID);

            entity.Property(e => e.DownloadDate).HasColumnType("datetime");

            entity.HasOne(d => d.Doc).WithMany(p => p.DocumentDownloadLog)
                .HasForeignKey(d => d.DocID)
                .HasConstraintName("FK_DocumentDownloadLog_CDS_Document");

            entity.HasOne(d => d.Type).WithMany(p => p.DocumentDownloadLog)
                .HasForeignKey(d => d.TypeID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DocumentDownloadLog_DocumentType");

            entity.HasOne(d => d.UIDNavigation).WithMany(p => p.DocumentDownloadLog)
                .HasForeignKey(d => d.UID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DocumentDownloadLog_UserProfile");
        });

        modelBuilder.Entity<DocumentDownloadQueue>(entity =>
        {
            entity.HasKey(e => e.DocID);

            entity.Property(e => e.DocID).ValueGeneratedNever();

            entity.HasOne(d => d.Doc).WithOne(p => p.DocumentDownloadQueue)
                .HasForeignKey<DocumentDownloadQueue>(d => d.DocID)
                .HasConstraintName("FK_DocumentDownloadQueue_CDS_Document");
        });

        modelBuilder.Entity<DocumentFlow>(entity =>
        {
            entity.HasKey(e => e.FlowID);

            entity.ToTable("DocumentFlow", "center");

            entity.Property(e => e.FlowName).HasMaxLength(64);

            entity.HasOne(d => d.InitialStepNavigation).WithMany(p => p.DocumentFlow)
                .HasForeignKey(d => d.InitialStep)
                .HasConstraintName("FK_DocumentFlow_DocumentFlowControl");
        });

        modelBuilder.Entity<DocumentFlowBranch>(entity =>
        {
            entity.HasKey(e => new { e.StepID, e.BranchStep });

            entity.ToTable("DocumentFlowBranch", "center");

            entity.Property(e => e.BranchName).HasMaxLength(64);

            entity.HasOne(d => d.BranchStepNavigation).WithMany(p => p.DocumentFlowBranchBranchStepNavigation)
                .HasForeignKey(d => d.BranchStep)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DocumentFlowBranch_DocumentFlowControl1");

            entity.HasOne(d => d.Flow).WithMany(p => p.DocumentFlowBranch)
                .HasForeignKey(d => d.FlowID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DocumentFlowBranch_DocumentFlow");

            entity.HasOne(d => d.Step).WithMany(p => p.DocumentFlowBranchStep)
                .HasForeignKey(d => d.StepID)
                .HasConstraintName("FK_DocumentFlowBranch_DocumentFlowControl");
        });

        modelBuilder.Entity<DocumentFlowControl>(entity =>
        {
            entity.HasKey(e => e.StepID);

            entity.ToTable("DocumentFlowControl", "center");

            entity.HasOne(d => d.Flow).WithMany(p => p.DocumentFlowControl)
                .HasForeignKey(d => d.FlowID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DocumentFlowControl_DocumentFlow");

            entity.HasOne(d => d.Level).WithMany(p => p.DocumentFlowControl)
                .HasForeignKey(d => d.LevelID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DocumentFlowControl_LevelExpression");

            entity.HasOne(d => d.NextStepNavigation).WithMany(p => p.InverseNextStepNavigation)
                .HasForeignKey(d => d.NextStep)
                .HasConstraintName("FK_DocumentFlowControl_DocumentFlowControl");

            entity.HasOne(d => d.PrevStepNavigation).WithMany(p => p.InversePrevStepNavigation)
                .HasForeignKey(d => d.PrevStep)
                .HasConstraintName("FK_DocumentFlowControl_DocumentFlowControl1");
        });

        modelBuilder.Entity<DocumentFlowStep>(entity =>
        {
            entity.HasKey(e => e.DocID);

            entity.ToTable("DocumentFlowStep", "center");

            entity.Property(e => e.DocID).ValueGeneratedNever();

            entity.HasOne(d => d.CurrentFlowStepNavigation).WithMany(p => p.DocumentFlowStep)
                .HasForeignKey(d => d.CurrentFlowStep)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DocumentFlowStep_DocumentFlowControl");

            entity.HasOne(d => d.Doc).WithOne(p => p.DocumentFlowStep)
                .HasForeignKey<DocumentFlowStep>(d => d.DocID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DocumentFlowStep_CDS_Document");
        });

        modelBuilder.Entity<DocumentMappingQueue>(entity =>
        {
            entity.HasKey(e => e.DocID);

            entity.Property(e => e.DocID).ValueGeneratedNever();

            entity.HasOne(d => d.Doc).WithOne(p => p.DocumentMappingQueue)
                .HasForeignKey<DocumentMappingQueue>(d => d.DocID)
                .HasConstraintName("FK_DocumentMappingQueue_CDS_Document");
        });

        modelBuilder.Entity<DocumentOwner>(entity =>
        {
            entity.HasKey(e => e.DocID);

            entity.Property(e => e.DocID).ValueGeneratedNever();
            entity.Property(e => e.ClientID).HasMaxLength(64);

            entity.HasOne(d => d.Doc).WithOne(p => p.DocumentOwner)
                .HasForeignKey<DocumentOwner>(d => d.DocID)
                .HasConstraintName("FK_DocumentOwner_CDS_Document");

            entity.HasOne(d => d.Owner).WithMany(p => p.DocumentOwner)
                .HasForeignKey(d => d.OwnerID)
                .HasConstraintName("FK_DocumentOwner_Organization");
        });

        modelBuilder.Entity<DocumentPostLog>(entity =>
        {
            entity.HasKey(e => e.LogID);

            entity.Property(e => e.ChkCode)
                .HasMaxLength(10)
                .HasComment("掛號檢查碼");
            entity.Property(e => e.CreateDate).HasColumnType("datetime");
            entity.Property(e => e.MailType)
                .HasMaxLength(10)
                .HasComment("郵件種類碼");
            entity.Property(e => e.PostCode)
                .HasMaxLength(10)
                .HasComment("郵局號");
            entity.Property(e => e.RegisterCode)
                .HasMaxLength(10)
                .HasComment("掛號號碼");
            entity.Property(e => e.ZipCode)
                .HasMaxLength(10)
                .HasComment("郵遞區號");

            entity.HasOne(d => d.Invoice).WithMany(p => p.DocumentPostLog)
                .HasForeignKey(d => d.InvoiceID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DocumentPostLog_InvoiceItem");
        });

        modelBuilder.Entity<DocumentPrintLog>(entity =>
        {
            entity.HasKey(e => e.LogID);

            entity.Property(e => e.PrintDate).HasColumnType("datetime");

            entity.HasOne(d => d.Doc).WithMany(p => p.DocumentPrintLog)
                .HasForeignKey(d => d.DocID)
                .HasConstraintName("FK_DocumentPrintLog_CDS_Document");

            entity.HasOne(d => d.Type).WithMany(p => p.DocumentPrintLog)
                .HasForeignKey(d => d.TypeID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DocumentPrintLog_DocumentType");

            entity.HasOne(d => d.UIDNavigation).WithMany(p => p.DocumentPrintLog)
                .HasForeignKey(d => d.UID)
                .HasConstraintName("FK_DocumentPrintLog_UserProfile");
        });

        modelBuilder.Entity<DocumentPrintQueue>(entity =>
        {
            entity.HasKey(e => e.DocID);

            entity.Property(e => e.DocID).ValueGeneratedNever();
            entity.Property(e => e.SubmitDate)
                .HasDefaultValueSql("(getdate())", "DF_DocumentPrintQueue_SubmitDate")
                .HasColumnType("datetime");
            entity.Property(e => e.SubmitID).ValueGeneratedOnAdd();

            entity.HasOne(d => d.Doc).WithOne(p => p.DocumentPrintQueue)
                .HasForeignKey<DocumentPrintQueue>(d => d.DocID)
                .HasConstraintName("FK_DocumentPrintQueue_CDS_Document");

            entity.HasOne(d => d.UIDNavigation).WithMany(p => p.DocumentPrintQueue)
                .HasForeignKey(d => d.UID)
                .HasConstraintName("FK_DocumentPrintQueue_UserProfile");
        });

        modelBuilder.Entity<DocumentProcessLog>(entity =>
        {
            entity.HasKey(e => new { e.DocID, e.StepDate });

            entity.ToTable(tb => tb.HasTrigger("TR_DocumentProcessLog"));

            entity.Property(e => e.StepDate).HasColumnType("datetime");

            entity.HasOne(d => d.Doc).WithMany(p => p.DocumentProcessLog)
                .HasForeignKey(d => d.DocID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DocumentProcessLog_CDS_Document");

            entity.HasOne(d => d.FlowStepNavigation).WithMany(p => p.DocumentProcessLog)
                .HasForeignKey(d => d.FlowStep)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DocumentProcessLog_LevelExpression");

            entity.HasOne(d => d.UIDNavigation).WithMany(p => p.DocumentProcessLog)
                .HasForeignKey(d => d.UID)
                .HasConstraintName("FK_DocumentProcessLog_UserProfile");
        });

        modelBuilder.Entity<DocumentReasonForRefusal>(entity =>
        {
            entity.HasKey(e => new { e.DocID, e.TimeToRefuse });

            entity.Property(e => e.TimeToRefuse).HasColumnType("datetime");

            entity.HasOne(d => d.DocumentProcessLog).WithOne(p => p.DocumentReasonForRefusal)
                .HasForeignKey<DocumentReasonForRefusal>(d => new { d.DocID, d.TimeToRefuse })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DocumentReasonForRefusal_DocumentProcessLog");
        });

        modelBuilder.Entity<DocumentReplication>(entity =>
        {
            entity.HasKey(e => new { e.DocID, e.TypeID });

            entity.ToTable(tb =>
                {
                    tb.HasComment("資料異動記錄檔");
                    tb.HasTrigger("TR_DocumentReplication");
                });

            entity.Property(e => e.LastActionTime)
                .HasComment("最近處理時間記錄")
                .HasColumnType("datetime");
            entity.Property(e => e.Message)
                .HasMaxLength(256)
                .HasComment("處理訊息");
            entity.Property(e => e.RetrialCount).HasComment("重試次數");

            entity.HasOne(d => d.Doc).WithMany(p => p.DocumentReplication)
                .HasForeignKey(d => d.DocID)
                .HasConstraintName("FK_DocumentReplication_CDS_Document");

            entity.HasOne(d => d.Type).WithMany(p => p.DocumentReplication)
                .HasForeignKey(d => d.TypeID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DocumentReplication_DocumentType");
        });

        modelBuilder.Entity<DocumentReturn>(entity =>
        {
            entity.HasKey(e => new { e.DocID, e.TypeID });

            entity.Property(e => e.ActionDate).HasColumnType("datetime");

            entity.HasOne(d => d.Doc).WithMany(p => p.DocumentReturn)
                .HasForeignKey(d => d.DocID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DocumentReturn_CDS_Document");

            entity.HasOne(d => d.Type).WithMany(p => p.DocumentReturn)
                .HasForeignKey(d => d.TypeID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DocumentReturn_DocumentType");
        });

        modelBuilder.Entity<DocumentSubscriptionQueue>(entity =>
        {
            entity.HasKey(e => e.DocID);

            entity.HasIndex(e => e.Status, "IX_DocumentSubscriptionQueue");

            entity.Property(e => e.DocID).ValueGeneratedNever();
            entity.Property(e => e.WaitUntil).HasColumnType("datetime");

            entity.HasOne(d => d.Doc).WithOne(p => p.DocumentSubscriptionQueue)
                .HasForeignKey<DocumentSubscriptionQueue>(d => d.DocID)
                .HasConstraintName("FK_DocumentSubscriptionQueue_CDS_Document");
        });

        modelBuilder.Entity<DocumentType>(entity =>
        {
            entity.HasKey(e => e.TypeID);

            entity.ToTable(tb => tb.HasComment("文件定對檔"));

            entity.Property(e => e.TypeID)
                .ValueGeneratedNever()
                .HasComment("主鍵");
            entity.Property(e => e.TypeName)
                .HasMaxLength(128)
                .HasComment("文件名稱");
        });

        modelBuilder.Entity<DocumentTypeFlow>(entity =>
        {
            entity.HasKey(e => new { e.TypeID, e.FlowID, e.CompanyID, e.BusinessID });

            entity.ToTable("DocumentTypeFlow", "center");

            entity.HasOne(d => d.Business).WithMany(p => p.DocumentTypeFlow)
                .HasForeignKey(d => d.BusinessID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DocumentTypeFlow_BusinessType");

            entity.HasOne(d => d.Company).WithMany(p => p.DocumentTypeFlow)
                .HasForeignKey(d => d.CompanyID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DocumentTypeFlow_Organization");

            entity.HasOne(d => d.Flow).WithMany(p => p.DocumentTypeFlow)
                .HasForeignKey(d => d.FlowID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DocumentTypeFlow_DocumentFlow");

            entity.HasOne(d => d.Type).WithMany(p => p.DocumentTypeFlow)
                .HasForeignKey(d => d.TypeID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DocumentTypeFlow_DocumentType");
        });

        modelBuilder.Entity<EnterpriseGroup>(entity =>
        {
            entity.HasKey(e => e.EnterpriseID);

            entity.Property(e => e.EnterpriseName).HasMaxLength(64);
        });

        modelBuilder.Entity<EnterpriseGroupMember>(entity =>
        {
            entity.HasKey(e => new { e.EnterpriseID, e.CompanyID });

            entity.HasOne(d => d.Company).WithMany(p => p.EnterpriseGroupMember)
                .HasForeignKey(d => d.CompanyID)
                .HasConstraintName("FK_EnterpriseGroupMember_Organization");

            entity.HasOne(d => d.Enterprise).WithMany(p => p.EnterpriseGroupMember)
                .HasForeignKey(d => d.EnterpriseID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EnterpriseGroupMember_EnterpriseGroup");
        });

        modelBuilder.Entity<EnterpriseGroupMemberToken>(entity =>
        {
            entity.HasKey(e => new { e.EnterpriseID, e.CompanyID });

            entity.HasIndex(e => e.Thumbprint, "IX_EnterpriseGroupMemberToken").IsUnique();

            entity.Property(e => e.Thumbprint).HasMaxLength(256);

            entity.HasOne(d => d.EnterpriseGroupMember).WithOne(p => p.EnterpriseGroupMemberToken)
                .HasForeignKey<EnterpriseGroupMemberToken>(d => new { d.EnterpriseID, d.CompanyID })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EnterpriseGroupMemberToken_EnterpriseGroupMember");
        });

        modelBuilder.Entity<ExceptionLog>(entity =>
        {
            entity.HasKey(e => e.LogID);

            entity.ToTable(tb => tb.HasTrigger("TR_ExceptionLog"));

            entity.HasIndex(e => e.DocID, "IX_ExceptionLog_LogID");

            entity.Property(e => e.LogTime)
                .HasDefaultValueSql("(getdate())", "DF_ExceptionLog_LogTime")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Company).WithMany(p => p.ExceptionLog)
                .HasForeignKey(d => d.CompanyID)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_ExceptionLog_Organization");

            entity.HasOne(d => d.Doc).WithMany(p => p.ExceptionLog)
                .HasForeignKey(d => d.DocID)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_ExceptionLog_CDS_Document");

            entity.HasOne(d => d.Type).WithMany(p => p.ExceptionLog)
                .HasForeignKey(d => d.TypeID)
                .HasConstraintName("FK_ExceptionLog_DocumentType");
        });

        modelBuilder.Entity<ExceptionReplication>(entity =>
        {
            entity.HasKey(e => e.LogID);

            entity.ToTable(tb => tb.HasComment("異常記錄待回應檔"));

            entity.Property(e => e.LogID).ValueGeneratedNever();

            entity.HasOne(d => d.Log).WithOne(p => p.ExceptionReplication)
                .HasForeignKey<ExceptionReplication>(d => d.LogID)
                .HasConstraintName("FK_ExceptionReplication_ExceptionLog");
        });

        modelBuilder.Entity<ExtraBillingItem>(entity =>
        {
            entity.HasKey(e => e.ItemID);

            entity.ToTable("ExtraBillingItem", "billing");

            entity.Property(e => e.BillingDate).HasColumnType("datetime");
            entity.Property(e => e.CompanyID).HasComment("主鍵");
            entity.Property(e => e.ItemName).HasMaxLength(64);

            entity.HasOne(d => d.Company).WithMany(p => p.ExtraBillingItem)
                .HasForeignKey(d => d.CompanyID)
                .HasConstraintName("FK_ExtraBillingItem_Organization");
        });

        modelBuilder.Entity<InboxItems>(entity =>
        {
            entity.HasKey(e => e.MessageID);

            entity.Property(e => e.MessageID).ValueGeneratedNever();

            entity.HasOne(d => d.Message).WithOne(p => p.InboxItems)
                .HasForeignKey<InboxItems>(d => d.MessageID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_InboxItems_MessageType");
        });

        modelBuilder.Entity<InvoiceAllowance>(entity =>
        {
            entity.HasKey(e => e.AllowanceID);

            entity.ToTable(tb =>
                {
                    tb.HasComment("發票折讓主檔");
                    tb.HasTrigger("TR_InvoiceAllowance");
                });

            entity.HasIndex(e => e.AllowanceNumber, "IX_InvoiceAllowance");

            entity.Property(e => e.AllowanceID).ValueGeneratedNever();
            entity.Property(e => e.AllowanceDate)
                .HasComment("折讓證明單日期")
                .HasColumnType("datetime");
            entity.Property(e => e.AllowanceNumber)
                .HasMaxLength(64)
                .HasComment("折讓證明單號碼");
            entity.Property(e => e.AllowanceType).HasComment("折讓種類\r\n1:買方開立折讓證明單\r\n2:賣方折讓證明單通知\r\n");
            entity.Property(e => e.BuyerId).HasMaxLength(10);
            entity.Property(e => e.IssueDate).HasColumnType("datetime");
            entity.Property(e => e.SellerId).HasMaxLength(10);
            entity.Property(e => e.TaxAmount)
                .HasComment("營業稅額合計")
                .HasColumnType("decimal(18, 5)");
            entity.Property(e => e.TotalAmount)
                .HasComment("金額(不含稅之進貨額)合計")
                .HasColumnType("decimal(18, 5)");

            entity.HasOne(d => d.Allowance).WithOne(p => p.InvoiceAllowance)
                .HasForeignKey<InvoiceAllowance>(d => d.AllowanceID)
                .HasConstraintName("FK_InvoiceAllowance_CDS_Document");

            entity.HasOne(d => d.Currency).WithMany(p => p.InvoiceAllowance)
                .HasForeignKey(d => d.CurrencyID)
                .HasConstraintName("FK_InvoiceAllowance_CurrencyType");

            entity.HasOne(d => d.Invoice).WithMany(p => p.InvoiceAllowance)
                .HasForeignKey(d => d.InvoiceID)
                .HasConstraintName("FK_InvoiceAllowance_InvoiceItem");

            entity.HasMany(d => d.Item).WithMany(p => p.Allowance)
                .UsingEntity<Dictionary<string, object>>(
                    "InvoiceAllowanceDetails",
                    r => r.HasOne<InvoiceAllowanceItem>().WithMany()
                        .HasForeignKey("ItemID")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_InvoiceAllowanceDetails_InvoiceAllowanceItem"),
                    l => l.HasOne<InvoiceAllowance>().WithMany()
                        .HasForeignKey("AllowanceID")
                        .HasConstraintName("FK_InvoiceAllowanceDetails_InvoiceAllowance"),
                    j =>
                    {
                        j.HasKey("AllowanceID", "ItemID");
                        j.ToTable(tb => tb.HasComment("折讓證明單明細檔"));
                    });
        });

        modelBuilder.Entity<InvoiceAllowanceBuyer>(entity =>
        {
            entity.HasKey(e => e.AllowanceID);

            entity.Property(e => e.AllowanceID).ValueGeneratedNever();
            entity.Property(e => e.Address).HasMaxLength(128);
            entity.Property(e => e.ContactName).HasMaxLength(64);
            entity.Property(e => e.CustomerID).HasMaxLength(64);
            entity.Property(e => e.CustomerName).HasMaxLength(64);
            entity.Property(e => e.EMail).HasMaxLength(512);
            entity.Property(e => e.Fax).HasMaxLength(64);
            entity.Property(e => e.Name).HasMaxLength(128);
            entity.Property(e => e.PersonInCharge).HasMaxLength(64);
            entity.Property(e => e.Phone).HasMaxLength(64);
            entity.Property(e => e.PostCode).HasMaxLength(8);
            entity.Property(e => e.ReceiptNo).HasMaxLength(10);
            entity.Property(e => e.RoleRemark).HasMaxLength(64);

            entity.HasOne(d => d.Allowance).WithOne(p => p.InvoiceAllowanceBuyer)
                .HasForeignKey<InvoiceAllowanceBuyer>(d => d.AllowanceID)
                .HasConstraintName("FK_InvoiceAllowanceBuyer_InvoiceAllowance");

            entity.HasOne(d => d.Buyer).WithMany(p => p.InvoiceAllowanceBuyer)
                .HasForeignKey(d => d.BuyerID)
                .HasConstraintName("FK_InvoiceAllowanceBuyer_Organization");
        });

        modelBuilder.Entity<InvoiceAllowanceCancellation>(entity =>
        {
            entity.HasKey(e => e.AllowanceID);

            entity.ToTable(tb =>
                {
                    tb.HasComment("作廢折讓主檔");
                    tb.HasTrigger("TR_InvoiceAllowanceCancellation");
                });

            entity.Property(e => e.AllowanceID).ValueGeneratedNever();
            entity.Property(e => e.CancelDate)
                .HasComment("作廢日期")
                .HasColumnType("datetime");
            entity.Property(e => e.CancelReason).HasMaxLength(256);
            entity.Property(e => e.Remark)
                .HasMaxLength(256)
                .HasComment("作廢折讓備註\r\n作廢折讓時必填，填寫作廢原因");

            entity.HasOne(d => d.Allowance).WithOne(p => p.InvoiceAllowanceCancellation)
                .HasForeignKey<InvoiceAllowanceCancellation>(d => d.AllowanceID)
                .HasConstraintName("FK_InvoiceAllowanceCancellation_InvoiceAllowance");
        });

        modelBuilder.Entity<InvoiceAllowanceItem>(entity =>
        {
            entity.HasKey(e => e.ItemID);

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 5)");
            entity.Property(e => e.Amount2).HasColumnType("decimal(18, 5)");
            entity.Property(e => e.InvoiceDate).HasColumnType("datetime");
            entity.Property(e => e.InvoiceNo).HasMaxLength(16);
            entity.Property(e => e.ItemNo).HasMaxLength(16);
            entity.Property(e => e.OriginalDescription).HasMaxLength(256);
            entity.Property(e => e.Piece).HasColumnType("decimal(18, 5)");
            entity.Property(e => e.Piece2).HasColumnType("decimal(18, 5)");
            entity.Property(e => e.PieceUnit).HasMaxLength(16);
            entity.Property(e => e.PieceUnit2).HasMaxLength(16);
            entity.Property(e => e.Tax).HasColumnType("decimal(18, 5)");
            entity.Property(e => e.UnitCost).HasColumnType("decimal(18, 5)");
            entity.Property(e => e.UnitCost2).HasColumnType("decimal(18, 5)");

            entity.HasOne(d => d.ProductItem).WithMany(p => p.InvoiceAllowanceItem)
                .HasForeignKey(d => d.ProductItemID)
                .HasConstraintName("FK_InvoiceAllowanceItem_InvoiceProductItem");
        });

        modelBuilder.Entity<InvoiceAllowanceItemExtension>(entity =>
        {
            entity.HasKey(e => e.AllowanceID);

            entity.Property(e => e.AllowanceID).ValueGeneratedNever();
            entity.Property(e => e.ExtraRemark).HasColumnType("xml");

            entity.HasOne(d => d.Allowance).WithOne(p => p.InvoiceAllowanceItemExtension)
                .HasForeignKey<InvoiceAllowanceItemExtension>(d => d.AllowanceID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_InvoiceAllowanceItemExtension_InvoiceAllowance");
        });

        modelBuilder.Entity<InvoiceAllowanceSeller>(entity =>
        {
            entity.HasKey(e => e.AllowanceID);

            entity.Property(e => e.AllowanceID).ValueGeneratedNever();
            entity.Property(e => e.Address).HasMaxLength(128);
            entity.Property(e => e.ContactName).HasMaxLength(64);
            entity.Property(e => e.CustomerID).HasMaxLength(64);
            entity.Property(e => e.CustomerName).HasMaxLength(64);
            entity.Property(e => e.EMail).HasMaxLength(512);
            entity.Property(e => e.Fax).HasMaxLength(64);
            entity.Property(e => e.Name).HasMaxLength(128);
            entity.Property(e => e.PersonInCharge).HasMaxLength(64);
            entity.Property(e => e.Phone).HasMaxLength(64);
            entity.Property(e => e.PostCode).HasMaxLength(8);
            entity.Property(e => e.ReceiptNo).HasMaxLength(10);
            entity.Property(e => e.RoleRemark).HasMaxLength(64);

            entity.HasOne(d => d.Allowance).WithOne(p => p.InvoiceAllowanceSeller)
                .HasForeignKey<InvoiceAllowanceSeller>(d => d.AllowanceID)
                .HasConstraintName("FK_InvoiceAllowanceSeller_InvoiceAllowance");

            entity.HasOne(d => d.Seller).WithMany(p => p.InvoiceAllowanceSeller)
                .HasForeignKey(d => d.SellerID)
                .HasConstraintName("FK_InvoiceAllowanceSeller_Organization");
        });

        modelBuilder.Entity<InvoiceAmountType>(entity =>
        {
            entity.HasKey(e => e.InvoiceID);

            entity.ToTable(tb => tb.HasComment("發票消售金額明細檔"));

            entity.HasIndex(e => e.InvoiceID, "IX_InvoiceAmountType");

            entity.Property(e => e.InvoiceID).ValueGeneratedNever();
            entity.Property(e => e.Adjustment)
                .HasComment("角分調整")
                .HasColumnType("decimal(18, 5)");
            entity.Property(e => e.DiscountAmount)
                .HasComment("扣抵金額")
                .HasColumnType("decimal(18, 5)");
            entity.Property(e => e.ExchangeRate)
                .HasComment("匯率")
                .HasColumnType("decimal(18, 5)");
            entity.Property(e => e.FreeTaxSalesAmount)
                .HasComment("")
                .HasColumnType("decimal(18, 5)");
            entity.Property(e => e.OriginalCurrencyAmount)
                .HasComment("原幣金額")
                .HasColumnType("decimal(18, 5)");
            entity.Property(e => e.SalesAmount)
                .HasComment("應稅銷售額合計(新台幣)")
                .HasColumnType("decimal(18, 5)");
            entity.Property(e => e.TaxAmount)
                .HasComment("營業稅額")
                .HasColumnType("decimal(18, 5)");
            entity.Property(e => e.TaxRate)
                .HasComment("稅率")
                .HasColumnType("decimal(18, 5)");
            entity.Property(e => e.TaxType).HasComment("課稅別\r\n1：應稅\r\n2：零稅率\r\n3：免稅\r\n9：混合應稅與免稅或零稅率 (限收銀機發票無法分辨時使用)");
            entity.Property(e => e.TotalAmount)
                .HasComment("總計\r\n整數\r\n(應稅銷售額合計+免稅銷售額合計+零稅率銷售額合計+營業稅額=此總計欄位) ，可為負數")
                .HasColumnType("decimal(18, 5)");
            entity.Property(e => e.TotalAmountInChinese)
                .HasMaxLength(32)
                .HasComment("中文國字大寫金額");
            entity.Property(e => e.ZeroTaxRateReason).HasMaxLength(4);
            entity.Property(e => e.ZeroTaxSalesAmount)
                .HasComment("")
                .HasColumnType("decimal(18, 5)");

            entity.HasOne(d => d.Currency).WithMany(p => p.InvoiceAmountType)
                .HasForeignKey(d => d.CurrencyID)
                .HasConstraintName("FK_InvoiceAmountType_CurrencyType");

            entity.HasOne(d => d.Invoice).WithOne(p => p.InvoiceAmountType)
                .HasForeignKey<InvoiceAmountType>(d => d.InvoiceID)
                .HasConstraintName("FK_InvoiceAmountType_InvoiceItem");
        });

        modelBuilder.Entity<InvoiceBusiness>(entity =>
        {
            entity.HasKey(e => new { e.SellerID, e.BuyerReceiptNo });

            entity.Property(e => e.BuyerReceiptNo).HasMaxLength(10);
            entity.Property(e => e.Address).HasMaxLength(128);
            entity.Property(e => e.ContactName).HasMaxLength(64);
            entity.Property(e => e.CustomerID).HasMaxLength(64);
            entity.Property(e => e.CustomerName).HasMaxLength(64);
            entity.Property(e => e.EMail).HasMaxLength(512);
            entity.Property(e => e.Fax).HasMaxLength(64);
            entity.Property(e => e.Name).HasMaxLength(128);
            entity.Property(e => e.PersonInCharge).HasMaxLength(64);
            entity.Property(e => e.Phone).HasMaxLength(64);
            entity.Property(e => e.PostCode).HasMaxLength(8);
            entity.Property(e => e.RoleRemark).HasMaxLength(64);

            entity.HasOne(d => d.Buyer).WithMany(p => p.InvoiceBusinessBuyer)
                .HasForeignKey(d => d.BuyerID)
                .HasConstraintName("FK_InvoiceBusiness_Organization1");

            entity.HasOne(d => d.Seller).WithMany(p => p.InvoiceBusinessSeller)
                .HasForeignKey(d => d.SellerID)
                .HasConstraintName("FK_InvoiceBusiness_Organization");
        });

        modelBuilder.Entity<InvoiceBuyer>(entity =>
        {
            entity.HasKey(e => e.InvoiceID);

            entity.ToTable(tb => tb.HasComment("發票買方明細檔"));

            entity.HasIndex(e => e.ReceiptNo, "IX_InvoiceBuyer");

            entity.HasIndex(e => e.CustomerID, "IX_InvoiceBuyer_CustomerID");

            entity.Property(e => e.InvoiceID).ValueGeneratedNever();
            entity.Property(e => e.Address).HasMaxLength(128);
            entity.Property(e => e.ContactName).HasMaxLength(64);
            entity.Property(e => e.CustomerID).HasMaxLength(64);
            entity.Property(e => e.CustomerName).HasMaxLength(64);
            entity.Property(e => e.CustomerNumber).HasMaxLength(20);
            entity.Property(e => e.EMail).HasMaxLength(512);
            entity.Property(e => e.Fax).HasMaxLength(64);
            entity.Property(e => e.Name).HasMaxLength(128);
            entity.Property(e => e.PersonInCharge).HasMaxLength(64);
            entity.Property(e => e.Phone).HasMaxLength(64);
            entity.Property(e => e.PostCode).HasMaxLength(8);
            entity.Property(e => e.ReceiptNo).HasMaxLength(10);
            entity.Property(e => e.RoleRemark).HasMaxLength(64);

            entity.HasOne(d => d.Buyer).WithMany(p => p.InvoiceBuyer)
                .HasForeignKey(d => d.BuyerID)
                .HasConstraintName("FK_InvoiceBuyer_Organization");

            entity.HasOne(d => d.Invoice).WithOne(p => p.InvoiceBuyer)
                .HasForeignKey<InvoiceBuyer>(d => d.InvoiceID)
                .HasConstraintName("FK_InvoiceBuyer_InvoiceItem");
        });

        modelBuilder.Entity<InvoiceByHousehold>(entity =>
        {
            entity.HasKey(e => e.InvoiceID);

            entity.ToTable(tb => tb.HasComment("發票歸戶關聯檔"));

            entity.Property(e => e.InvoiceID).ValueGeneratedNever();

            entity.HasOne(d => d.Carrier).WithMany(p => p.InvoiceByHousehold)
                .HasForeignKey(d => d.CarrierID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_InvoiceByHousehold_InvoiceUserCarrier");

            entity.HasOne(d => d.Invoice).WithOne(p => p.InvoiceByHousehold)
                .HasForeignKey<InvoiceByHousehold>(d => d.InvoiceID)
                .HasConstraintName("FK_InvoiceByHousehold_InvoiceItem");
        });

        modelBuilder.Entity<InvoiceCancellation>(entity =>
        {
            entity.HasKey(e => e.InvoiceID).HasName("PK_InvoiceCancellation_1");

            entity.ToTable(tb =>
                {
                    tb.HasComment("作廢發票主檔");
                    tb.HasTrigger("TR_InvoiceCancellation");
                });

            entity.Property(e => e.InvoiceID).ValueGeneratedNever();
            entity.Property(e => e.CancelDate)
                .HasComment("作廢日期")
                .HasColumnType("datetime");
            entity.Property(e => e.CancelReason).HasMaxLength(256);
            entity.Property(e => e.CancellationNo)
                .HasMaxLength(16)
                .HasComment("作廢發票號碼");
            entity.Property(e => e.Remark)
                .HasMaxLength(256)
                .HasComment("作廢備註\r\n作廢發票時必填，填寫作廢原因");
            entity.Property(e => e.ReturnTaxDocumentNo)
                .HasMaxLength(64)
                .HasComment("專案作廢核准文號\r\n若發票的作廢時間超過申報期間，則此欄位為必填欄位。若不填寫由上傳營業人自行負責。");

            entity.HasOne(d => d.Invoice).WithOne(p => p.InvoiceCancellation)
                .HasForeignKey<InvoiceCancellation>(d => d.InvoiceID)
                .HasConstraintName("FK_InvoiceCancellation_InvoiceItem");
        });

        modelBuilder.Entity<InvoiceCancellationUpload>(entity =>
        {
            entity.HasKey(e => e.UploadID);

            entity.Property(e => e.FilePath).HasMaxLength(256);
            entity.Property(e => e.UploadDate).HasColumnType("datetime");

            entity.HasOne(d => d.UIDNavigation).WithMany(p => p.InvoiceCancellationUpload)
                .HasForeignKey(d => d.UID)
                .HasConstraintName("FK_InvoiceCancellationUpload_UserProfile");

            entity.HasMany(d => d.Invoice).WithMany(p => p.Upload)
                .UsingEntity<Dictionary<string, object>>(
                    "InvoiceCancellationUploadList",
                    r => r.HasOne<InvoiceCancellation>().WithMany()
                        .HasForeignKey("InvoiceID")
                        .HasConstraintName("FK_InvoiceCancellationUploadList_InvoiceCancellation"),
                    l => l.HasOne<InvoiceCancellationUpload>().WithMany()
                        .HasForeignKey("UploadID")
                        .HasConstraintName("FK_InvoiceCancellationUploadList_InvoiceCancellationUpload"),
                    j =>
                    {
                        j.HasKey("UploadID", "InvoiceID");
                    });
        });

        modelBuilder.Entity<InvoiceCarrier>(entity =>
        {
            entity.HasKey(e => e.InvoiceID);

            entity.ToTable(tb => tb.HasComment("註記提示發票載具"));

            entity.Property(e => e.InvoiceID)
                .ValueGeneratedNever()
                .HasComment("Primary Key");
            entity.Property(e => e.CarrierNo)
                .HasMaxLength(64)
                .HasComment("載具卡號");
            entity.Property(e => e.CarrierNo2).HasMaxLength(64);
            entity.Property(e => e.CarrierType)
                .HasMaxLength(16)
                .HasComment("載具類別\r\n1：悠遊卡\r\n2：UXB2B條碼卡");

            entity.HasOne(d => d.Invoice).WithOne(p => p.InvoiceCarrier)
                .HasForeignKey<InvoiceCarrier>(d => d.InvoiceID)
                .HasConstraintName("FK_InvoiceCarrier_InvoiceItem");
        });

        modelBuilder.Entity<InvoiceDeliveryTracking>(entity =>
        {
            entity.HasKey(e => e.TrackingID);

            entity.HasIndex(e => e.DeliveryStatus, "IX_InvoiceDeliveryTracking");

            entity.HasIndex(e => new { e.TrackingNo1, e.TrackingNo2 }, "IX_InvoiceDeliveryTracking_1");

            entity.Property(e => e.DeliveryDate).HasColumnType("datetime");
            entity.Property(e => e.InvoiceID).HasComment("Primary Key");
            entity.Property(e => e.TrackingNo1).HasMaxLength(16);
            entity.Property(e => e.TrackingNo2).HasMaxLength(16);

            entity.HasOne(d => d.DeliveryStatusNavigation).WithMany(p => p.InvoiceDeliveryTracking)
                .HasForeignKey(d => d.DeliveryStatus)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_InvoiceDeliveryTracking_LevelExpression");

            entity.HasOne(d => d.Invoice).WithMany(p => p.InvoiceDeliveryTracking)
                .HasForeignKey(d => d.InvoiceID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_InvoiceDeliveryTracking_InvoiceItem");
        });

        modelBuilder.Entity<InvoiceDonation>(entity =>
        {
            entity.HasKey(e => e.InvoiceID);

            entity.ToTable(tb => tb.HasComment("註記發票捐贈"));

            entity.Property(e => e.InvoiceID)
                .ValueGeneratedNever()
                .HasComment("Primary Key");
            entity.Property(e => e.AgencyCode)
                .HasMaxLength(64)
                .HasComment("機構代碼");

            entity.HasOne(d => d.Invoice).WithOne(p => p.InvoiceDonation)
                .HasForeignKey<InvoiceDonation>(d => d.InvoiceID)
                .HasConstraintName("FK_InvoiceDonation_InvoiceItem");
        });

        modelBuilder.Entity<InvoiceIssuerAgent>(entity =>
        {
            entity.HasKey(e => new { e.AgentID, e.IssuerID }).HasName("PK_InvoiceIssurerAgent");

            entity.Property(e => e.AgentID).HasComment("主鍵");

            entity.HasOne(d => d.Agent).WithMany(p => p.InvoiceIssuerAgentAgent)
                .HasForeignKey(d => d.AgentID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_InvoiceIssurerAgent_Organization");

            entity.HasOne(d => d.Issuer).WithMany(p => p.InvoiceIssuerAgentIssuer)
                .HasForeignKey(d => d.IssuerID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_InvoiceIssurerAgent_Organization1");
        });

        modelBuilder.Entity<InvoiceItem>(entity =>
        {
            entity.HasKey(e => e.InvoiceID);

            entity.ToTable(tb =>
                {
                    tb.HasComment("電子發票主檔");
                    tb.HasTrigger("TR_InvoiceItem");
                });

            entity.HasIndex(e => e.InvoiceDate, "IX_InvoiceItem_1");

            entity.HasIndex(e => e.No, "IX_InvoiceItem_No");

            entity.HasIndex(e => e.TrackCode, "IX_InvoiceItem_TrackCode");

            entity.HasIndex(e => new { e.No, e.TrackCode, e.TrackID }, "IX_InvoiceItem_TrackCodeNo").IsUnique();

            entity.Property(e => e.InvoiceID)
                .ValueGeneratedNever()
                .HasComment("Primary Key");
            entity.Property(e => e.BuyerRemark).HasComment("買受人註記欄\r\n1：得抵扣之進貨及費用；\r\n2：得抵扣之固定資產；\r\n3：不得抵扣之進貨及費用；\r\n4：不得抵扣之固定資產\r\n");
            entity.Property(e => e.Category)
                .HasMaxLength(2)
                .HasComment("沖帳別");
            entity.Property(e => e.CheckNo)
                .HasMaxLength(10)
                .HasComment("發票檢查碼");
            entity.Property(e => e.CustomsClearanceMark).HasComment("通關方式註記\r\n1：非經海關出口;\r\n2：經海關出口(零稅率時，為必要欄位)\r\n");
            entity.Property(e => e.DonateMark)
                .HasMaxLength(1)
                .HasComment("捐贈註記\r\n以”0”表示 非捐贈發票\r\n以”1”表示 為捐贈發票\r\n");
            entity.Property(e => e.GroupMark)
                .HasMaxLength(2)
                .HasComment("彙開註記\r\n以”*”表示 彙開");
            entity.Property(e => e.InvoiceDate)
                .HasComment("發票日期")
                .HasColumnType("datetime");
            entity.Property(e => e.InvoiceType).HasComment("發票類別\r\n1: 三聯式;\r\n2: 二聯式;\r\n3: 二聯式收銀機;\r\n4. 特種稅額;\r\n5: 電子計算機;\r\n6: 三聯式收銀機\r\n");
            entity.Property(e => e.No)
                .HasMaxLength(16)
                .HasComment("發票號碼");
            entity.Property(e => e.PermitDate)
                .HasComment("核准日")
                .HasColumnType("datetime");
            entity.Property(e => e.PermitNumber)
                .HasMaxLength(20)
                .HasComment("核准號");
            entity.Property(e => e.PermitWord)
                .HasMaxLength(40)
                .HasComment("核准文");
            entity.Property(e => e.PrintMark).HasMaxLength(1);
            entity.Property(e => e.RandomNo)
                .HasMaxLength(10)
                .HasComment("發票防偽隨機碼\r\n前端隨機產生");
            entity.Property(e => e.RelateNumber)
                .HasMaxLength(20)
                .HasComment("相關號碼");
            entity.Property(e => e.Remark)
                .HasMaxLength(2048)
                .HasComment("總備註");
            entity.Property(e => e.TaxCenter)
                .HasMaxLength(40)
                .HasComment("稅捐稽徵處名稱");
            entity.Property(e => e.TrackCode).HasMaxLength(8);

            entity.HasOne(d => d.Donation).WithMany(p => p.InvoiceItemDonation)
                .HasForeignKey(d => d.DonationID)
                .HasConstraintName("FK_InvoiceItem_Organization1");

            entity.HasOne(d => d.Invoice).WithOne(p => p.InvoiceItem)
                .HasForeignKey<InvoiceItem>(d => d.InvoiceID)
                .HasConstraintName("FK_InvoiceItem_CDS_Document");

            entity.HasOne(d => d.Seller).WithMany(p => p.InvoiceItemSeller)
                .HasForeignKey(d => d.SellerID)
                .HasConstraintName("FK_InvoiceItem_Organization");

            entity.HasOne(d => d.Track).WithMany(p => p.InvoiceItem)
                .HasForeignKey(d => d.TrackID)
                .HasConstraintName("FK_InvoiceItem_InvoiceTrackCode");

            entity.HasMany(d => d.Product).WithMany(p => p.Invoice)
                .UsingEntity<Dictionary<string, object>>(
                    "InvoiceDetails",
                    r => r.HasOne<InvoiceProduct>().WithMany()
                        .HasForeignKey("ProductID")
                        .HasConstraintName("FK_InvoiceDetails_InvoiceProduct"),
                    l => l.HasOne<InvoiceItem>().WithMany()
                        .HasForeignKey("InvoiceID")
                        .HasConstraintName("FK_InvoiceDetails_InvoiceItem"),
                    j =>
                    {
                        j.HasKey("InvoiceID", "ProductID");
                        j.ToTable(tb => tb.HasComment("發票明細關聯檔"));
                    });
        });

        modelBuilder.Entity<InvoiceItemExtension>(entity =>
        {
            entity.HasKey(e => e.InvoiceID);

            entity.Property(e => e.InvoiceID)
                .ValueGeneratedNever()
                .HasComment("Primary Key");
            entity.Property(e => e.ExtraRemark).HasColumnType("xml");
            entity.Property(e => e.ProjectNo).HasMaxLength(64);
            entity.Property(e => e.PurchaseNo).HasMaxLength(64);
            entity.Property(e => e.StampDutyFlag).HasComment("顯示印花稅圖章\r\n0:不需要\r\n1:需要\r\n");

            entity.HasOne(d => d.Invoice).WithOne(p => p.InvoiceItemExtension)
                .HasForeignKey<InvoiceItemExtension>(d => d.InvoiceID)
                .HasConstraintName("FK_InvoiceItemExtension_InvoiceItem");
        });

        modelBuilder.Entity<InvoiceMail>(entity =>
        {
            entity.HasKey(e => e.InvoiceID).HasName("PK_InvoiceMail_1");

            entity.Property(e => e.InvoiceID)
                .ValueGeneratedNever()
                .HasComment("Primary Key");

            entity.HasOne(d => d.Invoice).WithOne(p => p.InvoiceMail)
                .HasForeignKey<InvoiceMail>(d => d.InvoiceID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_InvoiceMail_InvoiceItem");

            entity.HasOne(d => d.Mail).WithMany(p => p.InvoiceMail)
                .HasForeignKey(d => d.MailID)
                .HasConstraintName("FK_InvoiceMail_InvoiceMailTracking");
        });

        modelBuilder.Entity<InvoiceMailTracking>(entity =>
        {
            entity.HasKey(e => e.MailID);

            entity.Property(e => e.DeliveryDate).HasColumnType("datetime");
            entity.Property(e => e.TrackingNo1).HasMaxLength(16);
            entity.Property(e => e.TrackingNo2).HasMaxLength(16);

            entity.HasOne(d => d.DeliveryStatusNavigation).WithMany(p => p.InvoiceMailTracking)
                .HasForeignKey(d => d.DeliveryStatus)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_InvoiceMailTracking_LevelExpression");
        });

        modelBuilder.Entity<InvoiceNoAllocation>(entity =>
        {
            entity.HasKey(e => new { e.IntervalID, e.InvoiceNo });

            entity.Property(e => e.AllocateDate).HasColumnType("datetime");
            entity.Property(e => e.EncryptedContent).HasMaxLength(256);
            entity.Property(e => e.RandomNo).HasMaxLength(10);

            entity.HasOne(d => d.Interval).WithMany(p => p.InvoiceNoAllocation)
                .HasForeignKey(d => d.IntervalID)
                .HasConstraintName("FK_InvoiceNoAllocation_InvoiceNoInterval");
        });

        modelBuilder.Entity<InvoiceNoAssignment>(entity =>
        {
            entity.HasKey(e => e.InvoiceID);

            entity.HasIndex(e => new { e.IntervalID, e.InvoiceNo }, "IX_InvoiceNoAssignment").IsUnique();

            entity.Property(e => e.InvoiceID).ValueGeneratedNever();

            entity.HasOne(d => d.Interval).WithMany(p => p.InvoiceNoAssignment)
                .HasForeignKey(d => d.IntervalID)
                .HasConstraintName("FK_InvoiceNoAssignment_InvoiceNoInterval");

            entity.HasOne(d => d.Invoice).WithOne(p => p.InvoiceNoAssignment)
                .HasForeignKey<InvoiceNoAssignment>(d => d.InvoiceID)
                .HasConstraintName("FK_InvoiceNoAssignment_InvoiceItem");
        });

        modelBuilder.Entity<InvoiceNoInterval>(entity =>
        {
            entity.HasKey(e => e.IntervalID);

            entity.HasIndex(e => e.LockID, "IX_InvoiceNoInterval");

            entity.HasOne(d => d.InvoiceTrackCodeAssignment).WithMany(p => p.InvoiceNoInterval)
                .HasForeignKey(d => new { d.TrackID, d.SellerID })
                .HasConstraintName("FK_InvoiceNoInterval_InvoiceTrackCodeAssignment");
        });

        modelBuilder.Entity<InvoiceNoMainAssignment>(entity =>
        {
            entity.HasKey(e => new { e.TrackID, e.MasterID, e.StartNo });

            entity.HasIndex(e => e.AssignmentID, "IX_InvoiceNoMainAssignment").IsUnique();

            entity.Property(e => e.AssignmentID).ValueGeneratedOnAdd();

            entity.HasOne(d => d.InvoiceTrackCodeAssignment).WithMany(p => p.InvoiceNoMainAssignment)
                .HasForeignKey(d => new { d.TrackID, d.MasterID })
                .HasConstraintName("FK_InvoiceNoMainAssignment_InvoiceTrackCodeAssignment1");
        });

        modelBuilder.Entity<InvoiceNoSegment>(entity =>
        {
            entity.HasKey(e => e.SegmentID);

            entity.Property(e => e.SegmentID).ValueGeneratedNever();
            entity.Property(e => e.DeviceName).HasMaxLength(64);

            entity.HasOne(d => d.Segment).WithOne(p => p.InvoiceNoSegment)
                .HasForeignKey<InvoiceNoSegment>(d => d.SegmentID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_InvoiceNoSegment_InvoiceNoInterval");
        });

        modelBuilder.Entity<InvoiceNoSegmentDisposition>(entity =>
        {
            entity.HasKey(e => e.SegmentID);

            entity.Property(e => e.SegmentID).ValueGeneratedNever();

            entity.HasOne(d => d.Department).WithMany(p => p.InvoiceNoSegmentDisposition)
                .HasForeignKey(d => d.DepartmentID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_InvoiceNoSegmentDisposition_OrganizationDepartment");

            entity.HasOne(d => d.Segment).WithOne(p => p.InvoiceNoSegmentDisposition)
                .HasForeignKey<InvoiceNoSegmentDisposition>(d => d.SegmentID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_InvoiceNoSegmentDisposition_InvoiceNoSegment");

            entity.HasOne(d => d.UIDNavigation).WithMany(p => p.InvoiceNoSegmentDisposition)
                .HasForeignKey(d => d.UID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_InvoiceNoSegmentDisposition_UserProfile");
        });

        modelBuilder.Entity<InvoicePaperRequest>(entity =>
        {
            entity.HasKey(e => e.InvoiceID);

            entity.Property(e => e.InvoiceID).ValueGeneratedNever();
            entity.Property(e => e.RequestDate).HasColumnType("datetime");
            entity.Property(e => e.Token).HasMaxLength(64);

            entity.HasOne(d => d.Invoice).WithOne(p => p.InvoicePaperRequest)
                .HasForeignKey<InvoicePaperRequest>(d => d.InvoiceID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_InvoicePaperRequest_InvoiceItem");
        });

        modelBuilder.Entity<InvoicePeriod>(entity =>
        {
            entity.HasKey(e => e.PeriodID);

            entity.Property(e => e.PeriodID).ValueGeneratedNever();
        });

        modelBuilder.Entity<InvoicePeriodExchangeRate>(entity =>
        {
            entity.HasKey(e => new { e.PeriodID, e.CurrencyID });

            entity.Property(e => e.ExchangeRate)
                .HasComment("匯率")
                .HasColumnType("decimal(18, 5)");

            entity.HasOne(d => d.Currency).WithMany(p => p.InvoicePeriodExchangeRate)
                .HasForeignKey(d => d.CurrencyID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_InvoicePeriodExchangeRate_CurrencyType");

            entity.HasOne(d => d.Period).WithMany(p => p.InvoicePeriodExchangeRate)
                .HasForeignKey(d => d.PeriodID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_InvoicePeriodExchangeRate_InvoicePeriod");
        });

        modelBuilder.Entity<InvoicePrintAssertion>(entity =>
        {
            entity.HasKey(e => e.InvoiceID);

            entity.Property(e => e.InvoiceID).ValueGeneratedNever();
            entity.Property(e => e.PrintDate).HasColumnType("datetime");

            entity.HasOne(d => d.Invoice).WithOne(p => p.InvoicePrintAssertion)
                .HasForeignKey<InvoicePrintAssertion>(d => d.InvoiceID)
                .HasConstraintName("FK_InvoicePrintAssertion_InvoiceItem");
        });

        modelBuilder.Entity<InvoicePrintQueue>(entity =>
        {
            entity.HasKey(e => e.InvoiceID);

            entity.Property(e => e.InvoiceID).ValueGeneratedNever();
            entity.Property(e => e.SubmitDate)
                .HasDefaultValueSql("(getdate())", "DF_InvoicePrintQueue_SubmitDate")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Invoice).WithOne(p => p.InvoicePrintQueue)
                .HasForeignKey<InvoicePrintQueue>(d => d.InvoiceID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_InvoicePrintQueue_InvoiceItem");

            entity.HasOne(d => d.UIDNavigation).WithMany(p => p.InvoicePrintQueue)
                .HasForeignKey(d => d.UID)
                .HasConstraintName("FK_InvoicePrintQueue_UserProfile");
        });

        modelBuilder.Entity<InvoicePrizeWinningNumbers>(entity =>
        {
            entity.HasKey(e => e.BNID);

            entity.Property(e => e.AdditionalSixthPrize).HasMaxLength(50);
            entity.Property(e => e.FirstPrize).HasMaxLength(50);
            entity.Property(e => e.GrandPrize).HasMaxLength(50);
            entity.Property(e => e.Memo).HasMaxLength(250);
            entity.Property(e => e.SpecialPrize).HasMaxLength(50);
        });

        modelBuilder.Entity<InvoiceProduct>(entity =>
        {
            entity.HasKey(e => e.ProductID);

            entity.HasIndex(e => e.ProductID, "IX_InvoiceProduct");

            entity.HasIndex(e => e.Brief, "IX_InvoiceProduct_Brief");

            entity.Property(e => e.Brief).HasMaxLength(256);
        });

        modelBuilder.Entity<InvoiceProductItem>(entity =>
        {
            entity.HasKey(e => e.ItemID);

            entity.HasIndex(e => e.ProductID, "IX_InvoiceProductItem");

            entity.Property(e => e.CostAmount).HasColumnType("decimal(18, 7)");
            entity.Property(e => e.CostAmount2).HasColumnType("decimal(18, 7)");
            entity.Property(e => e.FreightAmount).HasColumnType("decimal(18, 7)");
            entity.Property(e => e.ItemNo).HasMaxLength(16);
            entity.Property(e => e.OriginalPrice).HasColumnType("decimal(18, 7)");
            entity.Property(e => e.Piece).HasColumnType("decimal(18, 7)");
            entity.Property(e => e.Piece2).HasColumnType("decimal(18, 7)");
            entity.Property(e => e.PieceUnit).HasMaxLength(16);
            entity.Property(e => e.PieceUnit2).HasMaxLength(16);
            entity.Property(e => e.RelateNumber).HasMaxLength(64);
            entity.Property(e => e.Remark).HasMaxLength(2048);
            entity.Property(e => e.Spec).HasMaxLength(128);
            entity.Property(e => e.UnitCost).HasColumnType("decimal(18, 7)");
            entity.Property(e => e.UnitCost2).HasColumnType("decimal(18, 7)");
            entity.Property(e => e.UnitFreight).HasColumnType("decimal(18, 7)");
            entity.Property(e => e.Weight).HasColumnType("decimal(18, 7)");
            entity.Property(e => e.WeightUnit).HasMaxLength(16);

            entity.HasOne(d => d.Product).WithMany(p => p.InvoiceProductItem)
                .HasForeignKey(d => d.ProductID)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_InvoiceProductItem_InvoiceProduct");
        });

        modelBuilder.Entity<InvoicePurchaseOrder>(entity =>
        {
            entity.HasKey(e => e.InvoiceID);

            entity.HasIndex(e => e.OrderNo, "IX_InvoicePurchaseOrder");

            entity.Property(e => e.InvoiceID).ValueGeneratedNever();
            entity.Property(e => e.OrderNo).HasMaxLength(64);
            entity.Property(e => e.PurchaseDate).HasColumnType("datetime");

            entity.HasOne(d => d.Invoice).WithOne(p => p.InvoicePurchaseOrder)
                .HasForeignKey<InvoicePurchaseOrder>(d => d.InvoiceID)
                .HasConstraintName("FK_InvoicePurchaseOrder_InvoiceItem");

            entity.HasOne(d => d.Upload).WithMany(p => p.InvoicePurchaseOrder)
                .HasForeignKey(d => d.UploadID)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_InvoicePurchaseOrder_InvoicePurchaseOrderUpload");
        });

        modelBuilder.Entity<InvoicePurchaseOrderAudit>(entity =>
        {
            entity.HasKey(e => new { e.SellerID, e.OrderNo });

            entity.Property(e => e.OrderNo).HasMaxLength(64);
            entity.Property(e => e.InvoiceID).HasComment("Primary Key");

            entity.HasOne(d => d.Invoice).WithMany(p => p.InvoicePurchaseOrderAudit)
                .HasForeignKey(d => d.InvoiceID)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_InvoicePurchaseOrderAudit_InvoiceItem");

            entity.HasOne(d => d.Seller).WithMany(p => p.InvoicePurchaseOrderAudit)
                .HasForeignKey(d => d.SellerID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_InvoicePurchaseOrderAudit_Organization");
        });

        modelBuilder.Entity<InvoicePurchaseOrderUpload>(entity =>
        {
            entity.HasKey(e => e.UploadID);

            entity.Property(e => e.FilePath).HasMaxLength(256);
            entity.Property(e => e.UploadDate).HasColumnType("datetime");

            entity.HasOne(d => d.UIDNavigation).WithMany(p => p.InvoicePurchaseOrderUpload)
                .HasForeignKey(d => d.UID)
                .HasConstraintName("FK_InvoicePurchaseOrderUpload_UserProfile");
        });

        modelBuilder.Entity<InvoiceSeller>(entity =>
        {
            entity.HasKey(e => e.InvoiceID);

            entity.Property(e => e.InvoiceID).ValueGeneratedNever();
            entity.Property(e => e.Address).HasMaxLength(128);
            entity.Property(e => e.ContactName).HasMaxLength(64);
            entity.Property(e => e.CustomerID).HasMaxLength(64);
            entity.Property(e => e.CustomerName).HasMaxLength(64);
            entity.Property(e => e.EMail).HasMaxLength(512);
            entity.Property(e => e.Fax).HasMaxLength(64);
            entity.Property(e => e.Name).HasMaxLength(128);
            entity.Property(e => e.PersonInCharge).HasMaxLength(64);
            entity.Property(e => e.Phone).HasMaxLength(64);
            entity.Property(e => e.PostCode).HasMaxLength(8);
            entity.Property(e => e.ReceiptNo).HasMaxLength(10);
            entity.Property(e => e.RoleRemark).HasMaxLength(64);

            entity.HasOne(d => d.Invoice).WithOne(p => p.InvoiceSeller)
                .HasForeignKey<InvoiceSeller>(d => d.InvoiceID)
                .HasConstraintName("FK_InvoiceSeller_InvoiceItem");

            entity.HasOne(d => d.Seller).WithMany(p => p.InvoiceSeller)
                .HasForeignKey(d => d.SellerID)
                .HasConstraintName("FK_InvoiceSeller_Organization");
        });

        modelBuilder.Entity<InvoiceTrackCode>(entity =>
        {
            entity.HasKey(e => e.TrackID);

            entity.HasIndex(e => new { e.PeriodNo, e.TrackCode, e.Year }, "IX_InvoiceTrackCode").IsUnique();

            entity.Property(e => e.InvoiceType).HasComment("發票類別\r\n1: 三聯式;\r\n2: 二聯式;\r\n3: 二聯式收銀機;\r\n4. 特種稅額;\r\n5: 電子計算機;\r\n6: 三聯式收銀機\r\n");
            entity.Property(e => e.TrackCode).HasMaxLength(2);

            entity.HasOne(d => d.Period).WithMany(p => p.InvoiceTrackCode)
                .HasForeignKey(d => d.PeriodID)
                .HasConstraintName("FK_InvoiceTrackCode_InvoicePeriod");
        });

        modelBuilder.Entity<InvoiceTrackCodeAssignment>(entity =>
        {
            entity.HasKey(e => new { e.TrackID, e.SellerID });

            entity.HasOne(d => d.Assignment).WithMany(p => p.InvoiceTrackCodeAssignmentNavigation)
                .HasPrincipalKey(p => p.AssignmentID)
                .HasForeignKey(d => d.AssignmentID)
                .HasConstraintName("FK_InvoiceTrackCodeAssignment_InvoiceNoMainAssignment");

            entity.HasOne(d => d.Seller).WithMany(p => p.InvoiceTrackCodeAssignment)
                .HasForeignKey(d => d.SellerID)
                .HasConstraintName("FK_InvoiceTrackCodeAssignment_Organization");

            entity.HasOne(d => d.Track).WithMany(p => p.InvoiceTrackCodeAssignment)
                .HasForeignKey(d => d.TrackID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_InvoiceTrackCodeAssignment_InvoiceTrackCode");
        });

        modelBuilder.Entity<InvoiceUserCarrier>(entity =>
        {
            entity.HasKey(e => e.CarrierID);

            entity.HasIndex(e => new { e.CarrierNo, e.CarrierNo2 }, "IX_InvoiceUserCarrier").IsUnique();

            entity.Property(e => e.CarrierNo).HasMaxLength(64);
            entity.Property(e => e.CarrierNo2).HasMaxLength(64);

            entity.HasOne(d => d.Code).WithMany(p => p.InvoiceUserCarrier)
                .HasForeignKey(d => d.CodeID)
                .HasConstraintName("FK_InvoiceUserCarrier_MemberCode");

            entity.HasOne(d => d.Type).WithMany(p => p.InvoiceUserCarrier)
                .HasForeignKey(d => d.TypeID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_InvoiceUserCarrier_InvoiceUserCarrierType");

            entity.HasOne(d => d.UIDNavigation).WithMany(p => p.InvoiceUserCarrier)
                .HasForeignKey(d => d.UID)
                .HasConstraintName("FK_InvoiceUserCarrier_UserProfile");
        });

        modelBuilder.Entity<InvoiceUserCarrierType>(entity =>
        {
            entity.HasKey(e => e.TypeID);

            entity.HasIndex(e => e.CarrierType, "IX_InvoiceUserCarrierType");

            entity.Property(e => e.CarrierType).HasMaxLength(16);
            entity.Property(e => e.Description).HasMaxLength(64);
        });

        modelBuilder.Entity<InvoiceWelfareAgency>(entity =>
        {
            entity.HasKey(e => e.WelfareID);

            entity.ToTable(tb => tb.HasTrigger("TR_InvoiceWelfareAgency"));

            entity.Property(e => e.CreateTime)
                .HasDefaultValueSql("(getdate())", "DF_InvoiceWelfareAgency_CreateTime")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Agency).WithMany(p => p.InvoiceWelfareAgency)
                .HasForeignKey(d => d.AgencyID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_InvoiceWelfareAgency_WelfareAgency");

            entity.HasOne(d => d.Seller).WithMany(p => p.InvoiceWelfareAgency)
                .HasForeignKey(d => d.SellerID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_InvoiceWelfareAgency_Organization");
        });

        modelBuilder.Entity<InvoiceWinningNumber>(entity =>
        {
            entity.HasKey(e => e.InvoiceID);

            entity.ToTable(tb => tb.HasComment("發票中獎號碼主檔"));

            entity.Property(e => e.InvoiceID).ValueGeneratedNever();
            entity.Property(e => e.DownloadDate).HasColumnType("datetime");
            entity.Property(e => e.PrizeType).HasMaxLength(16);

            entity.HasOne(d => d.Invoice).WithOne(p => p.InvoiceWinningNumber)
                .HasForeignKey<InvoiceWinningNumber>(d => d.InvoiceID)
                .HasConstraintName("FK_InvoiceWinningNumber_InvoiceItem");

            entity.HasOne(d => d.Winning).WithMany(p => p.InvoiceWinningNumber)
                .HasForeignKey(d => d.WinningID)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_InvoiceWinningNumber_UniformInvoiceWinningNumber");
        });

        modelBuilder.Entity<IssuingNotice>(entity =>
        {
            entity.HasKey(e => e.DocID);

            entity.Property(e => e.DocID).ValueGeneratedNever();
            entity.Property(e => e.IssueDate).HasColumnType("datetime");

            entity.HasOne(d => d.Doc).WithOne(p => p.IssuingNotice)
                .HasForeignKey<IssuingNotice>(d => d.DocID)
                .HasConstraintName("FK_IssuingNotice_CDS_Document");
        });

        modelBuilder.Entity<LevelExpression>(entity =>
        {
            entity.HasKey(e => e.LevelID);

            entity.Property(e => e.LevelID).ValueGeneratedNever();
            entity.Property(e => e.Description).HasMaxLength(50);
            entity.Property(e => e.Expression).HasMaxLength(50);
        });

        modelBuilder.Entity<MasterOrganization>(entity =>
        {
            entity.HasKey(e => e.MasterID);

            entity.ToTable("MasterOrganization", "center");

            entity.Property(e => e.MasterID)
                .ValueGeneratedNever()
                .HasComment("主鍵");
            entity.Property(e => e.EnterpriseName)
                .HasMaxLength(128)
                .HasComment("機關名稱");

            entity.HasOne(d => d.Master).WithOne(p => p.MasterOrganization)
                .HasForeignKey<MasterOrganization>(d => d.MasterID)
                .HasConstraintName("FK_MasterOrganization_Organization");

            entity.HasMany(d => d.Branch).WithMany(p => p.Master)
                .UsingEntity<Dictionary<string, object>>(
                    "MasterBranches",
                    r => r.HasOne<Organization>().WithMany()
                        .HasForeignKey("BranchID")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_MasterBranches_Organization"),
                    l => l.HasOne<MasterOrganization>().WithMany()
                        .HasForeignKey("MasterID")
                        .HasConstraintName("FK_MasterBranches_MasterOrganization"),
                    j =>
                    {
                        j.HasKey("MasterID", "BranchID");
                        j.ToTable("MasterBranches", "center");
                        j.IndexerProperty<int>("MasterID").HasComment("主鍵");
                        j.IndexerProperty<int>("BranchID").HasComment("主鍵");
                    });
        });

        modelBuilder.Entity<MemberCode>(entity =>
        {
            entity.HasKey(e => e.CodeID);

            entity.HasIndex(e => e.HashID, "IX_MemberCode").IsUnique();

            entity.Property(e => e.CreateTime)
                .HasDefaultValueSql("(getdate())", "DF_MemberCode_CreateTime")
                .HasColumnType("datetime");
            entity.Property(e => e.HashID).HasMaxLength(64);

            entity.HasOne(d => d.Company).WithMany(p => p.MemberCode)
                .HasForeignKey(d => d.CompanyID)
                .HasConstraintName("FK_MemberCode_Organization");
        });

        modelBuilder.Entity<MenuControl>(entity =>
        {
            entity.HasKey(e => e.MenuID);

            entity.Property(e => e.SiteMenu).HasMaxLength(64);
        });

        modelBuilder.Entity<MessageType>(entity =>
        {
            entity.HasKey(e => e.MessageID);

            entity.Property(e => e.DetailControl).HasMaxLength(256);
            entity.Property(e => e.MailControl).HasMaxLength(256);
            entity.Property(e => e.Message).HasMaxLength(128);
            entity.Property(e => e.UIControl).HasMaxLength(256);
        });

        modelBuilder.Entity<MonthlyBilling>(entity =>
        {
            entity.HasKey(e => new { e.SettlementID, e.CompanyID });

            entity.ToTable("MonthlyBilling", "billing");

            entity.Property(e => e.CompanyID).HasComment("主鍵");

            entity.HasOne(d => d.Bill).WithMany(p => p.MonthlyBilling)
                .HasForeignKey(d => d.BillID)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_MonthlyBilling_BillSubmission");

            entity.HasOne(d => d.Company).WithMany(p => p.MonthlyBilling)
                .HasForeignKey(d => d.CompanyID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MonthlyBilling_Organization");

            entity.HasOne(d => d.Settlement).WithMany(p => p.MonthlyBilling)
                .HasForeignKey(d => d.SettlementID)
                .HasConstraintName("FK_MonthlyBilling_Settlement");
        });

        modelBuilder.Entity<MonthlyExtraBilling>(entity =>
        {
            entity.HasKey(e => new { e.SettlementID, e.ItemID, e.CompanyID });

            entity.ToTable("MonthlyExtraBilling", "billing");

            entity.Property(e => e.CompanyID).HasComment("主鍵");
            entity.Property(e => e.ItemName).HasMaxLength(64);

            entity.HasOne(d => d.MonthlyBilling).WithMany(p => p.MonthlyExtraBilling)
                .HasForeignKey(d => new { d.SettlementID, d.CompanyID })
                .HasConstraintName("FK_MonthlyExtraBilling_MonthlyBilling");
        });

        modelBuilder.Entity<Organization>(entity =>
        {
            entity.HasKey(e => e.CompanyID)
                .HasName("PK12")
                .IsClustered(false);

            entity.ToTable(tb => tb.HasComment("機關、公司、組織單位"));

            entity.HasIndex(e => e.ReceiptNo, "IX_Organization");

            entity.Property(e => e.CompanyID).HasComment("主鍵");
            entity.Property(e => e.Addr)
                .HasMaxLength(256)
                .HasComment("地址");
            entity.Property(e => e.CompanyName)
                .HasMaxLength(128)
                .HasComment("機關名稱");
            entity.Property(e => e.ContactEmail)
                .HasMaxLength(512)
                .HasComment("連絡人電子郵件");
            entity.Property(e => e.ContactFax).HasMaxLength(20);
            entity.Property(e => e.ContactMobilePhone).HasMaxLength(20);
            entity.Property(e => e.ContactName)
                .HasMaxLength(50)
                .HasComment("連絡人");
            entity.Property(e => e.ContactPhone).HasMaxLength(20);
            entity.Property(e => e.ContactTitle)
                .HasMaxLength(16)
                .HasComment("連絡人職稱");
            entity.Property(e => e.EnglishAddr).HasMaxLength(256);
            entity.Property(e => e.EnglishName).HasMaxLength(50);
            entity.Property(e => e.EnglishRegAddr).HasMaxLength(256);
            entity.Property(e => e.Fax)
                .HasMaxLength(50)
                .HasComment("傳真");
            entity.Property(e => e.InvoiceSignature).HasMaxLength(64);
            entity.Property(e => e.LogoURL).HasMaxLength(200);
            entity.Property(e => e.Phone)
                .HasMaxLength(64)
                .HasComment("電話");
            entity.Property(e => e.ReceiptNo)
                .HasMaxLength(10)
                .HasComment("統一編號");
            entity.Property(e => e.RegAddr).HasMaxLength(256);
            entity.Property(e => e.UndertakerFax).HasMaxLength(20);
            entity.Property(e => e.UndertakerID).HasMaxLength(16);
            entity.Property(e => e.UndertakerMobilePhone).HasMaxLength(20);
            entity.Property(e => e.UndertakerName)
                .HasMaxLength(50)
                .HasComment("負責人姓名");
            entity.Property(e => e.UndertakerPhone).HasMaxLength(20);

            entity.HasMany(d => d.Product).WithMany(p => p.Supplier)
                .UsingEntity<Dictionary<string, object>>(
                    "ProductSupplier",
                    r => r.HasOne<ProductCatalog>().WithMany()
                        .HasForeignKey("ProductID")
                        .HasConstraintName("FK_ProductSupplier_ProductCatalog"),
                    l => l.HasOne<Organization>().WithMany()
                        .HasForeignKey("SupplierID")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_ProductSupplier_Organization"),
                    j =>
                    {
                        j.HasKey("SupplierID", "ProductID").HasName("PK_SUPPLIER_PRODUCTS_NUMBER");
                        j.ToTable("ProductSupplier", "scm");
                    });

            entity.HasMany(d => d.Type).WithMany(p => p.Company)
                .UsingEntity<Dictionary<string, object>>(
                    "BillingCalculation",
                    r => r.HasOne<DocumentType>().WithMany()
                        .HasForeignKey("TypeID")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_BillingCalculation_DocumentType"),
                    l => l.HasOne<Organization>().WithMany()
                        .HasForeignKey("CompanyID")
                        .HasConstraintName("FK_BillingCalculation_Organization"),
                    j =>
                    {
                        j.HasKey("CompanyID", "TypeID");
                        j.ToTable("BillingCalculation", "billing");
                        j.IndexerProperty<int>("CompanyID").HasComment("主鍵");
                        j.IndexerProperty<int>("TypeID").HasComment("主鍵");
                    });
        });

        modelBuilder.Entity<OrganizationBranch>(entity =>
        {
            entity.HasKey(e => e.BranchID).HasName("PK_OrganizationBranch_1");

            entity.Property(e => e.Addr)
                .HasMaxLength(256)
                .HasComment("地址");
            entity.Property(e => e.BranchName).HasMaxLength(64);
            entity.Property(e => e.BranchNo).HasMaxLength(32);
            entity.Property(e => e.ContactEmail)
                .HasMaxLength(512)
                .HasComment("連絡人電子郵件");
            entity.Property(e => e.Phone)
                .HasMaxLength(64)
                .HasComment("電話");

            entity.HasOne(d => d.Company).WithMany(p => p.OrganizationBranch)
                .HasForeignKey(d => d.CompanyID)
                .HasConstraintName("FK_OrganizationBranch_Organization1");
        });

        modelBuilder.Entity<OrganizationCategory>(entity =>
        {
            entity.HasKey(e => e.OrgaCateID);

            entity.HasIndex(e => new { e.CompanyID, e.CategoryID }, "IX_OrganizationCategory").IsUnique();

            entity.HasOne(d => d.Category).WithMany(p => p.OrganizationCategory)
                .HasForeignKey(d => d.CategoryID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OrganizationCategory_CategoryDefinition");

            entity.HasOne(d => d.Company).WithMany(p => p.OrganizationCategory)
                .HasForeignKey(d => d.CompanyID)
                .HasConstraintName("FK_OrganizationCategory_Organization");
        });

        modelBuilder.Entity<OrganizationCategoryUserRole>(entity =>
        {
            entity.HasKey(e => new { e.OrgaCateID, e.RoleID });

            entity.Property(e => e.MainMenu).HasColumnType("xml");

            entity.HasOne(d => d.OrgaCate).WithMany(p => p.OrganizationCategoryUserRole)
                .HasForeignKey(d => d.OrgaCateID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OrganizationCategoryUserRole_OrganizationCategory");

            entity.HasOne(d => d.Role).WithMany(p => p.OrganizationCategoryUserRole)
                .HasForeignKey(d => d.RoleID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OrganizationCategoryUserRole_UserRoleDefinition");
        });

        modelBuilder.Entity<OrganizationCustomSetting>(entity =>
        {
            entity.HasKey(e => e.CompanyID);

            entity.Property(e => e.CompanyID)
                .ValueGeneratedNever()
                .HasComment("主鍵");

            entity.HasOne(d => d.Company).WithOne(p => p.OrganizationCustomSetting)
                .HasForeignKey<OrganizationCustomSetting>(d => d.CompanyID)
                .HasConstraintName("FK_OrganizationCustomSetting_Organization");
        });

        modelBuilder.Entity<OrganizationDepartment>(entity =>
        {
            entity.HasKey(e => e.DepartmentID);

            entity.Property(e => e.Department).HasMaxLength(64);

            entity.HasOne(d => d.Company).WithMany(p => p.OrganizationDepartment)
                .HasForeignKey(d => d.CompanyID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OrganizationDepartment_Organization");
        });

        modelBuilder.Entity<OrganizationExtension>(entity =>
        {
            entity.HasKey(e => e.CompanyID);

            entity.HasIndex(e => e.ExpirationDate, "IX_OrganizationExtension");

            entity.Property(e => e.CompanyID)
                .ValueGeneratedNever()
                .HasComment("主鍵");
            entity.Property(e => e.AuthorizationNotAfter).HasColumnType("datetime");
            entity.Property(e => e.AuthorizationNotBefore).HasColumnType("datetime");
            entity.Property(e => e.BusinessContactPhone).HasMaxLength(64);
            entity.Property(e => e.CreationDate).HasColumnType("datetime");
            entity.Property(e => e.CustomerNo).HasMaxLength(16);
            entity.Property(e => e.ExpirationDate).HasColumnType("datetime");
            entity.Property(e => e.GoLiveDate).HasColumnType("datetime");
            entity.Property(e => e.InvoiceRequestNotAfter).HasColumnType("datetime");
            entity.Property(e => e.InvoiceRequestNotBefore).HasColumnType("datetime");
            entity.Property(e => e.MailSubjectAlias).HasMaxLength(64);
            entity.Property(e => e.TaxNo).HasMaxLength(16);

            entity.HasOne(d => d.Company).WithOne(p => p.OrganizationExtension)
                .HasForeignKey<OrganizationExtension>(d => d.CompanyID)
                .HasConstraintName("FK_OrganizationExtension_Organization");
        });

        modelBuilder.Entity<OrganizationSettings>(entity =>
        {
            entity.HasKey(e => new { e.CompanyID, e.Settings });

            entity.Property(e => e.CompanyID).HasComment("主鍵");
            entity.Property(e => e.Settings).HasMaxLength(256);

            entity.HasOne(d => d.Company).WithMany(p => p.OrganizationSettings)
                .HasForeignKey(d => d.CompanyID)
                .HasConstraintName("FK_OrganizationSettings_Organization");
        });

        modelBuilder.Entity<OrganizationStatus>(entity =>
        {
            entity.HasKey(e => e.CompanyID);

            entity.Property(e => e.CompanyID).ValueGeneratedNever();
            entity.Property(e => e.AllowancePrintView).HasMaxLength(256);
            entity.Property(e => e.AuthorizationNo).HasMaxLength(256);
            entity.Property(e => e.CustomNotificationView).HasMaxLength(256);
            entity.Property(e => e.InvoicePrintView).HasMaxLength(256);
            entity.Property(e => e.LastTimeToAcknowledge).HasColumnType("datetime");
            entity.Property(e => e.NotificationFooterView).HasMaxLength(256);

            entity.HasOne(d => d.Company).WithOne(p => p.OrganizationStatus)
                .HasForeignKey<OrganizationStatus>(d => d.CompanyID)
                .HasConstraintName("FK_OrganizationStatus_Organization");

            entity.HasOne(d => d.CurrentLevelNavigation).WithMany(p => p.OrganizationStatus)
                .HasForeignKey(d => d.CurrentLevel)
                .HasConstraintName("FK_OrganizationStatus_LevelExpression");

            entity.HasOne(d => d.Token).WithMany(p => p.OrganizationStatus)
                .HasForeignKey(d => d.TokenID)
                .HasConstraintName("FK_OrganizationStatus_UserToken");
        });

        modelBuilder.Entity<OrganizationToken>(entity =>
        {
            entity.HasKey(e => e.CompanyID);

            entity.HasIndex(e => e.Thumbprint, "IX_OrganizationToken").IsUnique();

            entity.HasIndex(e => e.KeyID, "IX_OrganizationToken_1");

            entity.Property(e => e.CompanyID).ValueGeneratedNever();
            entity.Property(e => e.Thumbprint).HasMaxLength(256);

            entity.HasOne(d => d.Company).WithOne(p => p.OrganizationToken)
                .HasForeignKey<OrganizationToken>(d => d.CompanyID)
                .HasConstraintName("FK_OrganizationToken_Organization");
        });

        modelBuilder.Entity<POSDevice>(entity =>
        {
            entity.HasKey(e => e.DeviceID);

            entity.HasIndex(e => new { e.CompanyID, e.POSNo }, "IX_POSDevice").IsUnique();

            entity.Property(e => e.CompanyID).HasComment("主鍵");
            entity.Property(e => e.POSNo).HasMaxLength(64);

            entity.HasOne(d => d.Company).WithMany(p => p.POSDevice)
                .HasForeignKey(d => d.CompanyID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_POSDevice_Organization");
        });

        modelBuilder.Entity<POSInvoiceNoSegment>(entity =>
        {
            entity.HasKey(e => e.SegmentID);

            entity.Property(e => e.SegmentID).ValueGeneratedNever();
            entity.Property(e => e.RequestDate).HasColumnType("datetime");

            entity.HasOne(d => d.Device).WithMany(p => p.POSInvoiceNoSegment)
                .HasForeignKey(d => d.DeviceID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_POSInvoiceNoSegment_POSDevice");

            entity.HasOne(d => d.Segment).WithOne(p => p.POSInvoiceNoSegment)
                .HasForeignKey<POSInvoiceNoSegment>(d => d.SegmentID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_POSInvoiceNoSegment_InvoiceNoSegment");
        });

        modelBuilder.Entity<ProcessCompletionNotification>(entity =>
        {
            entity.HasKey(e => e.TaskID);

            entity.ToTable("ProcessCompletionNotification", "proc");

            entity.Property(e => e.TaskID).ValueGeneratedNever();

            entity.HasOne(d => d.Task).WithOne(p => p.ProcessCompletionNotification)
                .HasForeignKey<ProcessCompletionNotification>(d => d.TaskID)
                .HasConstraintName("FK_ProcessCompletionNotification_ProcessRequest");
        });

        modelBuilder.Entity<ProcessExceptionNotification>(entity =>
        {
            entity.HasKey(e => new { e.TaskID, e.CompanyID });

            entity.ToTable("ProcessExceptionNotification", "proc");

            entity.Property(e => e.CompanyID).HasComment("主鍵");
            entity.Property(e => e.BookingTime).HasColumnType("datetime");

            entity.HasOne(d => d.Company).WithMany(p => p.ProcessExceptionNotification)
                .HasForeignKey(d => d.CompanyID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProcessExceptionNotification_Organization");

            entity.HasOne(d => d.Task).WithMany(p => p.ProcessExceptionNotification)
                .HasForeignKey(d => d.TaskID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProcessExceptionNotification_ProcessRequest");
        });

        modelBuilder.Entity<ProcessRequest>(entity =>
        {
            entity.HasKey(e => e.TaskID);

            entity.ToTable("ProcessRequest", "proc");

            entity.HasIndex(e => e.SubmitDate, "IX_ProcessRequest").IsDescending();

            entity.Property(e => e.ProcessComplete).HasColumnType("datetime");
            entity.Property(e => e.ProcessStart).HasColumnType("datetime");
            entity.Property(e => e.RequestPath).HasMaxLength(512);
            entity.Property(e => e.ResponsePath).HasMaxLength(512);
            entity.Property(e => e.SubmitDate)
                .HasDefaultValueSql("(getdate())", "DF_ProcessRequest_SubmitDate")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Agent).WithMany(p => p.ProcessRequest)
                .HasForeignKey(d => d.AgentID)
                .HasConstraintName("FK_ProcessRequest_Organization");

            entity.HasOne(d => d.Log).WithMany(p => p.ProcessRequest)
                .HasForeignKey(d => d.LogID)
                .HasConstraintName("FK_ProcessRequest_ExceptionLog");

            entity.HasOne(d => d.ProcessTypeNavigation).WithMany(p => p.ProcessRequest)
                .HasForeignKey(d => d.ProcessType)
                .HasConstraintName("FK_ProcessRequest_ProcessRequestType");

            entity.HasOne(d => d.SenderNavigation).WithMany(p => p.ProcessRequest)
                .HasForeignKey(d => d.Sender)
                .HasConstraintName("FK_ProcessRequest_UserProfile");
        });

        modelBuilder.Entity<ProcessRequestCondition>(entity =>
        {
            entity.HasKey(e => new { e.TaskID, e.ConditionID });

            entity.ToTable("ProcessRequestCondition", "proc");

            entity.HasOne(d => d.Task).WithMany(p => p.ProcessRequestCondition)
                .HasForeignKey(d => d.TaskID)
                .HasConstraintName("FK_ProcessRequestCondition_ProcessRequest");
        });

        modelBuilder.Entity<ProcessRequestDocument>(entity =>
        {
            entity.HasKey(e => e.DocID);

            entity.ToTable("ProcessRequestDocument", "proc");

            entity.Property(e => e.DocID).ValueGeneratedNever();
            entity.Property(e => e.CreateDate)
                .HasDefaultValueSql("(getdate())", "DF_ProcessRequestDocument_CreateDate")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Doc).WithOne(p => p.ProcessRequestDocument)
                .HasForeignKey<ProcessRequestDocument>(d => d.DocID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProcessRequestDocument_CDS_Document");

            entity.HasOne(d => d.Task).WithMany(p => p.ProcessRequestDocument)
                .HasForeignKey(d => d.TaskID)
                .HasConstraintName("FK_ProcessRequestDocument_ProcessRequest");
        });

        modelBuilder.Entity<ProcessRequestQueue>(entity =>
        {
            entity.HasKey(e => e.TaskID);

            entity.ToTable("ProcessRequestQueue", "proc");

            entity.Property(e => e.TaskID).ValueGeneratedNever();
            entity.Property(e => e.BookingTime).HasColumnType("datetime");

            entity.HasOne(d => d.Actor).WithMany(p => p.ProcessRequestQueue)
                .HasForeignKey(d => d.ActorID)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_ProcessRequestQueue_ProcessorUnit");

            entity.HasOne(d => d.Task).WithOne(p => p.ProcessRequestQueue)
                .HasForeignKey<ProcessRequestQueue>(d => d.TaskID)
                .HasConstraintName("FK_ProcessRequestQueue_ProcessRequest");
        });

        modelBuilder.Entity<ProcessRequestType>(entity =>
        {
            entity.HasKey(e => e.ProcessType);

            entity.ToTable("ProcessRequestType", "proc");

            entity.Property(e => e.ProcessType).ValueGeneratedNever();
            entity.Property(e => e.ChannelInProgress).HasMaxLength(64);
            entity.Property(e => e.ChannelName).HasMaxLength(64);
            entity.Property(e => e.ChannelResponse).HasMaxLength(64);
            entity.Property(e => e.DescriptionID).HasMaxLength(16);
        });

        modelBuilder.Entity<ProcessRequestTypeLocale>(entity =>
        {
            entity.HasKey(e => new { e.LocaleID, e.ProcessType });

            entity.ToTable("ProcessRequestTypeLocale", "proc");

            entity.Property(e => e.LocaleID).HasMaxLength(8);
            entity.Property(e => e.ChannelInProgress).HasMaxLength(64);
            entity.Property(e => e.ChannelName).HasMaxLength(64);
            entity.Property(e => e.ChannelResponse).HasMaxLength(64);

            entity.HasOne(d => d.ProcessTypeNavigation).WithMany(p => p.ProcessRequestTypeLocale)
                .HasForeignKey(d => d.ProcessType)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProcessRequestTypeLocale_ProcessRequestType");
        });

        modelBuilder.Entity<ProcessorUnit>(entity =>
        {
            entity.HasKey(e => e.ProcessorID);

            entity.ToTable("ProcessorUnit", "proc");

            entity.HasIndex(e => e.ProcessorToken, "IX_ProcessorUnit").IsUnique();
        });

        modelBuilder.Entity<ProductCatalog>(entity =>
        {
            entity.HasKey(e => e.ProductID).HasName("PK__ProductCatalog");

            entity.ToTable("ProductCatalog", "scm");

            entity.HasIndex(e => e.Barcode, "IX_ProductCatalog_Barcode");

            entity.Property(e => e.Barcode).HasMaxLength(64);
            entity.Property(e => e.PieceUnit).HasMaxLength(16);
            entity.Property(e => e.ProductName).HasMaxLength(128);
            entity.Property(e => e.PurchasePrice).HasColumnType("decimal(18, 5)");
            entity.Property(e => e.Remark).HasMaxLength(128);
            entity.Property(e => e.SalePrice).HasColumnType("decimal(18, 5)");
            entity.Property(e => e.Spec).HasMaxLength(128);
        });

        modelBuilder.Entity<ProductItemCategory>(entity =>
        {
            entity.HasKey(e => e.PICID);

            entity.Property(e => e.ItemName).HasMaxLength(1024);
            entity.Property(e => e.ItemNo)
                .HasMaxLength(50)
                .IsFixedLength();
            entity.Property(e => e.Unit).HasMaxLength(16);
            entity.Property(e => e.UnitePrice).HasColumnType("decimal(18, 5)");
        });

        modelBuilder.Entity<ReceiptCancellation>(entity =>
        {
            entity.HasKey(e => e.ReceiptID);

            entity.ToTable(tb => tb.HasTrigger("TR_ReceiptCancellation"));

            entity.Property(e => e.ReceiptID).ValueGeneratedNever();
            entity.Property(e => e.CancelDate).HasColumnType("datetime");
            entity.Property(e => e.CancellationNo).HasMaxLength(20);
            entity.Property(e => e.Remark).HasMaxLength(256);

            entity.HasOne(d => d.Receipt).WithOne(p => p.ReceiptCancellation)
                .HasForeignKey<ReceiptCancellation>(d => d.ReceiptID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ReceiptCancellation_ReceiptItem");
        });

        modelBuilder.Entity<ReceiptDetail>(entity =>
        {
            entity.HasKey(e => e.DetailID);

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.Description).HasMaxLength(256);
            entity.Property(e => e.Quantity).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.Remark).HasMaxLength(128);
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18, 4)");

            entity.HasOne(d => d.Receipt).WithMany(p => p.ReceiptDetail)
                .HasForeignKey(d => d.ReceiptID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ReceiptDetail_ReceiptItem");
        });

        modelBuilder.Entity<ReceiptItem>(entity =>
        {
            entity.HasKey(e => e.ReceiptID);

            entity.ToTable(tb => tb.HasTrigger("TR_ReceiptItem"));

            entity.Property(e => e.ReceiptID).ValueGeneratedNever();
            entity.Property(e => e.No)
                .HasMaxLength(20)
                .HasComment("發票號碼");
            entity.Property(e => e.ReceiptDate)
                .HasComment("發票日期")
                .HasColumnType("datetime");
            entity.Property(e => e.TotalAmount)
                .HasComment("數量")
                .HasColumnType("decimal(18, 0)");

            entity.HasOne(d => d.Buyer).WithMany(p => p.ReceiptItemBuyer)
                .HasForeignKey(d => d.BuyerID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ReceiptItem_Organization1");

            entity.HasOne(d => d.Receipt).WithOne(p => p.ReceiptItem)
                .HasForeignKey<ReceiptItem>(d => d.ReceiptID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ReceiptItem_CDS_Document");

            entity.HasOne(d => d.Seller).WithMany(p => p.ReceiptItemSeller)
                .HasForeignKey(d => d.SellerID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ReceiptItem_Organization");
        });

        modelBuilder.Entity<ReplicationNotification>(entity =>
        {
            entity.HasKey(e => new { e.DocID, e.TypeID });

            entity.ToTable(tb => tb.HasComment("資料異動待回應檔"));

            entity.HasOne(d => d.DocumentReplication).WithOne(p => p.ReplicationNotification)
                .HasForeignKey<ReplicationNotification>(d => new { d.DocID, d.TypeID })
                .HasConstraintName("FK_ReplicationNotification_DocumentReplication");
        });

        modelBuilder.Entity<ResetUserPassword>(entity =>
        {
            entity.HasKey(e => e.UID);

            entity.Property(e => e.UID).ValueGeneratedNever();

            entity.HasOne(d => d.UIDNavigation).WithOne(p => p.ResetUserPassword)
                .HasForeignKey<ResetUserPassword>(d => d.UID)
                .HasConstraintName("FK_ResetUserPassword_UserProfile");
        });

        modelBuilder.Entity<SMSNotificationLog>(entity =>
        {
            entity.HasKey(e => e.LogID);

            entity.Property(e => e.SubmitDate).HasColumnType("datetime");

            entity.HasOne(d => d.Doc).WithMany(p => p.SMSNotificationLog)
                .HasForeignKey(d => d.DocID)
                .HasConstraintName("FK_SMSNotificationLog_CDS_Document");

            entity.HasOne(d => d.Message).WithMany(p => p.SMSNotificationLog)
                .HasForeignKey(d => d.MessageID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SMSNotificationLog_MessageType");

            entity.HasOne(d => d.Owner).WithMany(p => p.SMSNotificationLog)
                .HasForeignKey(d => d.OwnerID)
                .HasConstraintName("FK_SMSNotificationLog_Organization");
        });

        modelBuilder.Entity<SMSNotificationQueue>(entity =>
        {
            entity.HasKey(e => new { e.DocID, e.MessageID });

            entity.Property(e => e.SubmitDate).HasColumnType("datetime");

            entity.HasOne(d => d.Doc).WithMany(p => p.SMSNotificationQueue)
                .HasForeignKey(d => d.DocID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SMSNotificationQueue_CDS_Document");

            entity.HasOne(d => d.Message).WithMany(p => p.SMSNotificationQueue)
                .HasForeignKey(d => d.MessageID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SMSNotificationQueue_MessageType");
        });

        modelBuilder.Entity<Settlement>(entity =>
        {
            entity.ToTable("Settlement", "billing");

            entity.Property(e => e.EndExclusiveDate).HasColumnType("datetime");
            entity.Property(e => e.SettlementDate).HasColumnType("datetime");
            entity.Property(e => e.StartDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<SystemEvent>(entity =>
        {
            entity.HasKey(e => e.EventID);

            entity.Property(e => e.EventDate)
                .HasDefaultValueSql("(getdate())", "DF_SystemEvent_EventDate")
                .HasColumnType("datetime");
            entity.Property(e => e.ReferenceUrl).HasColumnType("xml");
        });

        modelBuilder.Entity<SystemMessage>(entity =>
        {
            entity.HasKey(e => e.MsgID);

            entity.Property(e => e.CreateTime).HasColumnType("datetime");
            entity.Property(e => e.EndDate).HasColumnType("datetime");
            entity.Property(e => e.StartDate).HasColumnType("datetime");
            entity.Property(e => e.UpdateTime).HasColumnType("datetime");
        });

        modelBuilder.Entity<UnassignedInvoiceNo>(entity =>
        {
            entity.HasKey(e => e.UAID).HasName("PK_UnusedInvoiceNo");

            entity.HasOne(d => d.InvoiceTrackCodeAssignment).WithMany(p => p.UnassignedInvoiceNo)
                .HasForeignKey(d => new { d.TrackID, d.SellerID })
                .HasConstraintName("FK_UnusedInvoiceNo_InvoiceTrackCodeAssignment");
        });

        modelBuilder.Entity<UniformInvoiceWinningNumber>(entity =>
        {
            entity.HasKey(e => e.WinningID);

            entity.HasIndex(e => new { e.Year, e.Period }, "IX_UniformInvoiceWinningNumber");

            entity.Property(e => e.PrizeType).HasMaxLength(16);
            entity.Property(e => e.WinningNO).HasMaxLength(16);
        });

        modelBuilder.Entity<UserAuth>(entity =>
        {
            entity.HasKey(e => e.AuthID);

            entity.HasIndex(e => e.Thumbprint, "IX_UserAuth").IsUnique();

            entity.Property(e => e.Thumbprint).HasMaxLength(256);

            entity.HasOne(d => d.UIDNavigation).WithMany(p => p.UserAuth)
                .HasForeignKey(d => d.UID)
                .HasConstraintName("FK_UserAuth_UserProfile");
        });

        modelBuilder.Entity<UserInbox>(entity =>
        {
            entity.HasKey(e => e.MsgID);

            entity.ToTable(tb => tb.HasTrigger("TR_UserInbox"));

            entity.HasIndex(e => e.MessageID, "IX_UserInbox");

            entity.HasIndex(e => e.OrgaCateID, "IX_UserInbox_1");

            entity.HasIndex(e => e.RoleID, "IX_UserInbox_2");

            entity.Property(e => e.DataSource).HasColumnType("xml");
            entity.Property(e => e.MsgDate)
                .HasDefaultValueSql("(getdate())", "DF_UserInbox_MsgDate")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Doc).WithMany(p => p.UserInbox)
                .HasForeignKey(d => d.DocID)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_UserInbox_CDS_Document");

            entity.HasOne(d => d.Event).WithMany(p => p.UserInbox)
                .HasForeignKey(d => d.EventID)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_UserInbox_SystemEvent");

            entity.HasOne(d => d.Message).WithMany(p => p.UserInbox)
                .HasForeignKey(d => d.MessageID)
                .HasConstraintName("FK_UserInbox_MessageType");

            entity.HasOne(d => d.OrgaCate).WithMany(p => p.UserInbox)
                .HasForeignKey(d => d.OrgaCateID)
                .HasConstraintName("FK_UserInbox_OrganizationCategory");

            entity.HasOne(d => d.Role).WithMany(p => p.UserInbox)
                .HasForeignKey(d => d.RoleID)
                .HasConstraintName("FK_UserInbox_UserRoleDefinition");

            entity.HasOne(d => d.SenderNavigation).WithMany(p => p.UserInbox)
                .HasForeignKey(d => d.Sender)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_UserInbox_UserProfile");
        });

        modelBuilder.Entity<UserMail>(entity =>
        {
            entity.HasKey(e => e.MsgID);

            entity.Property(e => e.MsgID).ValueGeneratedNever();

            entity.HasOne(d => d.Msg).WithOne(p => p.UserMail)
                .HasForeignKey<UserMail>(d => d.MsgID)
                .HasConstraintName("FK_UserMail_UserInbox");
        });

        modelBuilder.Entity<UserMenu>(entity =>
        {
            entity.HasKey(e => new { e.RoleID, e.CategoryID, e.MenuID });

            entity.HasOne(d => d.Category).WithMany(p => p.UserMenu)
                .HasForeignKey(d => d.CategoryID)
                .HasConstraintName("FK_UserMenu_CategoryDefinition");

            entity.HasOne(d => d.Menu).WithMany(p => p.UserMenu)
                .HasForeignKey(d => d.MenuID)
                .HasConstraintName("FK_UserMenu_MenuControl");

            entity.HasOne(d => d.Role).WithMany(p => p.UserMenu)
                .HasForeignKey(d => d.RoleID)
                .HasConstraintName("FK_UserMenu_UserRoleDefinition");
        });

        modelBuilder.Entity<UserProfile>(entity =>
        {
            entity.HasKey(e => e.UID).HasName("PK_Customers");

            entity.HasIndex(e => e.PID, "IX_UserProfile").IsUnique();

            entity.HasIndex(e => e.UID, "IX_UserProfile_MailID");

            entity.Property(e => e.Address).HasMaxLength(128);
            entity.Property(e => e.City).HasMaxLength(16);
            entity.Property(e => e.ContactTitle).HasMaxLength(30);
            entity.Property(e => e.Country).HasMaxLength(16);
            entity.Property(e => e.EMail).HasMaxLength(512);
            entity.Property(e => e.Expiration).HasColumnType("datetime");
            entity.Property(e => e.Fax).HasMaxLength(64);
            entity.Property(e => e.MailID).HasMaxLength(64);
            entity.Property(e => e.MobilePhone).HasMaxLength(24);
            entity.Property(e => e.PID).HasMaxLength(32);
            entity.Property(e => e.Password).HasMaxLength(64);
            entity.Property(e => e.Password2).HasMaxLength(64);
            entity.Property(e => e.Phone).HasMaxLength(64);
            entity.Property(e => e.Phone2).HasMaxLength(64);
            entity.Property(e => e.PostalCode).HasMaxLength(16);
            entity.Property(e => e.Region).HasMaxLength(16);
            entity.Property(e => e.ThemeName).HasMaxLength(16);
            entity.Property(e => e.UserName).HasMaxLength(40);

            entity.HasOne(d => d.Auth).WithMany(p => p.InverseAuth)
                .HasForeignKey(d => d.AuthID)
                .HasConstraintName("FK_UserProfile_UserProfile1");

            entity.HasOne(d => d.CreatorNavigation).WithMany(p => p.InverseCreatorNavigation)
                .HasForeignKey(d => d.Creator)
                .HasConstraintName("FK_UserProfile_UserProfile");

            entity.HasOne(d => d.Level).WithMany(p => p.UserProfile)
                .HasForeignKey(d => d.LevelID)
                .HasConstraintName("FK_UserProfile_LevelExpression");

            entity.HasMany(d => d.Department).WithMany(p => p.UID)
                .UsingEntity<Dictionary<string, object>>(
                    "OrganizationDepartmentMember",
                    r => r.HasOne<OrganizationDepartment>().WithMany()
                        .HasForeignKey("DepartmentID")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_OrganizationDepartmentMember_OrganizationDepartment"),
                    l => l.HasOne<UserProfile>().WithMany()
                        .HasForeignKey("UID")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_OrganizationDepartmentMember_UserProfile"),
                    j =>
                    {
                        j.HasKey("UID", "DepartmentID");
                    });
        });

        modelBuilder.Entity<UserProfileExtension>(entity =>
        {
            entity.HasKey(e => e.UID);

            entity.Property(e => e.UID).ValueGeneratedNever();
            entity.Property(e => e.Birthday).HasColumnType("datetime");
            entity.Property(e => e.IDNo).HasMaxLength(10);
            entity.Property(e => e.NightPhone).HasMaxLength(24);
            entity.Property(e => e.Sex).HasMaxLength(1);
            entity.Property(e => e.TwoFactorKey).HasMaxLength(64);

            entity.HasOne(d => d.UIDNavigation).WithOne(p => p.UserProfileExtension)
                .HasForeignKey<UserProfileExtension>(d => d.UID)
                .HasConstraintName("FK_UserProfileExtension_UserProfile");
        });

        modelBuilder.Entity<UserProfileProperty>(entity =>
        {
            entity.HasKey(e => new { e.UID, e.PropertyID });

            entity.HasOne(d => d.UIDNavigation).WithMany(p => p.UserProfileProperty)
                .HasForeignKey(d => d.UID)
                .HasConstraintName("FK_UserProfileProperty_UserProfile");
        });

        modelBuilder.Entity<UserProfileStatus>(entity =>
        {
            entity.HasKey(e => e.UID);

            entity.ToTable(tb => tb.HasComment("會員狀態主檔"));

            entity.Property(e => e.UID).ValueGeneratedNever();

            entity.HasOne(d => d.CurrentLevelNavigation).WithMany(p => p.UserProfileStatus)
                .HasForeignKey(d => d.CurrentLevel)
                .HasConstraintName("FK_UserProfileStatus_LevelExpression");

            entity.HasOne(d => d.UIDNavigation).WithOne(p => p.UserProfileStatus)
                .HasForeignKey<UserProfileStatus>(d => d.UID)
                .HasConstraintName("FK_UserProfileStatus_UserProfile");
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => new { e.UID, e.RoleID, e.OrgaCateID });

            entity.HasOne(d => d.OrgaCate).WithMany(p => p.UserRole)
                .HasForeignKey(d => d.OrgaCateID)
                .HasConstraintName("FK_UserRole_OrganizationCategory");

            entity.HasOne(d => d.Role).WithMany(p => p.UserRole)
                .HasForeignKey(d => d.RoleID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserRole_UserRoleDefinition");

            entity.HasOne(d => d.UIDNavigation).WithMany(p => p.UserRole)
                .HasForeignKey(d => d.UID)
                .HasConstraintName("FK_UserRole_UserProfile");
        });

        modelBuilder.Entity<UserRoleDefinition>(entity =>
        {
            entity.HasKey(e => e.RoleID);

            entity.HasIndex(e => e.Role, "IX_UserRoleDefinition").IsUnique();

            entity.Property(e => e.RoleID).ValueGeneratedNever();
            entity.Property(e => e.Role).HasMaxLength(16);
            entity.Property(e => e.SiteMenu).HasMaxLength(64);
        });

        modelBuilder.Entity<UserToken>(entity =>
        {
            entity.HasKey(e => e.Token);

            entity.Property(e => e.Token).ValueGeneratedNever();
            entity.Property(e => e.LogonTime)
                .HasDefaultValueSql("(getdate())", "DF_UserToken_LogonTime")
                .HasColumnType("datetime");
            entity.Property(e => e.Thumbprint).HasMaxLength(256);

            entity.HasOne(d => d.UIDNavigation).WithMany(p => p.UserToken)
                .HasForeignKey(d => d.UID)
                .HasConstraintName("FK_UserToken_UserProfile");
        });

        modelBuilder.Entity<VacantInvoiceNo>(entity =>
        {
            entity.HasKey(e => e.VacancyID);

            entity.HasIndex(e => new { e.IntervalID, e.InvoiceNo }, "IX_VacantInvoiceNo");

            entity.HasIndex(e => e.NextID, "IX_VacantInvoiceNo_NextID");

            entity.HasIndex(e => e.PrevID, "IX_VacantInvoiceNo_PrevID");

            entity.HasOne(d => d.Interval).WithMany(p => p.VacantInvoiceNo)
                .HasForeignKey(d => d.IntervalID)
                .HasConstraintName("FK_VacantInvoiceNo_InvoiceNoInterval");

            entity.HasOne(d => d.Next).WithMany(p => p.InverseNext)
                .HasForeignKey(d => d.NextID)
                .HasConstraintName("FK_VacantInvoiceNo_VacantInvoiceNo1");

            entity.HasOne(d => d.Prev).WithMany(p => p.InversePrev)
                .HasForeignKey(d => d.PrevID)
                .HasConstraintName("FK_VacantInvoiceNo_VacantInvoiceNo");
        });

        modelBuilder.Entity<VoidInvoiceRequest>(entity =>
        {
            entity.HasKey(e => e.DocID);

            entity.HasIndex(e => e.InvoiceNo, "IX_VoidInvoiceRequest");

            entity.Property(e => e.DocID).ValueGeneratedNever();
            entity.Property(e => e.CommitDate).HasColumnType("datetime");
            entity.Property(e => e.InvoiceNo).HasMaxLength(16);
            entity.Property(e => e.Reason).HasMaxLength(64);
            entity.Property(e => e.VoidDate).HasColumnType("datetime");

            entity.HasOne(d => d.Doc).WithOne(p => p.VoidInvoiceRequest)
                .HasForeignKey<VoidInvoiceRequest>(d => d.DocID)
                .HasConstraintName("FK_VoidInvoiceRequest_CDS_Document");
        });

        modelBuilder.Entity<WelfareAgency>(entity =>
        {
            entity.HasKey(e => e.AgencyID);

            entity.ToTable(tb => tb.HasComment("社福機構"));

            entity.Property(e => e.AgencyID)
                .ValueGeneratedNever()
                .HasComment("主鍵");
            entity.Property(e => e.AgencyCode)
                .HasMaxLength(64)
                .HasComment("機構代碼");

            entity.HasOne(d => d.Agency).WithOne(p => p.WelfareAgency)
                .HasForeignKey<WelfareAgency>(d => d.AgencyID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_WelfareAgency_Organization");
        });

        modelBuilder.Entity<WelfareReplication>(entity =>
        {
            entity.HasKey(e => e.WelfareID);

            entity.ToTable(tb => tb.HasComment("資料異動待回應檔"));

            entity.Property(e => e.WelfareID).ValueGeneratedNever();

            entity.HasOne(d => d.Welfare).WithOne(p => p.WelfareReplication)
                .HasForeignKey<WelfareReplication>(d => d.WelfareID)
                .HasConstraintName("FK_WelfareReplication_InvoiceWelfareAgency");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
