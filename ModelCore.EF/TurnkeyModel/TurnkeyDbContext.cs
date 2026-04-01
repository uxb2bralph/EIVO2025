using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ModelCore.TurnkeyModel;

public partial class TurnkeyDbContext : DbContext
{
    public TurnkeyDbContext(DbContextOptions<TurnkeyDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<DocumentDispatchQueue> DocumentDispatchQueue { get; set; }

    public virtual DbSet<FROM_CONFIG> FROM_CONFIG { get; set; }

    public virtual DbSet<SCHEDULE_CONFIG> SCHEDULE_CONFIG { get; set; }

    public virtual DbSet<SIGN_CONFIG> SIGN_CONFIG { get; set; }

    public virtual DbSet<TASK_CONFIG> TASK_CONFIG { get; set; }

    public virtual DbSet<TO_CONFIG> TO_CONFIG { get; set; }

    public virtual DbSet<TURNKEY_MESSAGE_LOG> TURNKEY_MESSAGE_LOG { get; set; }

    public virtual DbSet<TURNKEY_MESSAGE_LOG_DETAIL> TURNKEY_MESSAGE_LOG_DETAIL { get; set; }

    public virtual DbSet<TURNKEY_SEQUENCE> TURNKEY_SEQUENCE { get; set; }

    public virtual DbSet<TURNKEY_SYSEVENT_LOG> TURNKEY_SYSEVENT_LOG { get; set; }

    public virtual DbSet<TURNKEY_TRANSPORT_CONFIG> TURNKEY_TRANSPORT_CONFIG { get; set; }

    public virtual DbSet<TURNKEY_USER_PROFILE> TURNKEY_USER_PROFILE { get; set; }

    public virtual DbSet<TurnkeyTriggerLog> TurnkeyTriggerLog { get; set; }

    public virtual DbSet<V_Allowance> V_Allowance { get; set; }

    public virtual DbSet<V_Invoice> V_Invoice { get; set; }

    public virtual DbSet<V_Logs> V_Logs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DocumentDispatchQueue>(entity =>
        {
            entity.HasKey(e => new { e.DocType, e.DocNo });

            entity.Property(e => e.DocType).HasMaxLength(16);
            entity.Property(e => e.DocNo).HasMaxLength(32);
            entity.Property(e => e.FileName).HasMaxLength(128);
            entity.Property(e => e.Status).HasMaxLength(16);
        });

        modelBuilder.Entity<FROM_CONFIG>(entity =>
        {
            entity.HasNoKey();

            entity.HasIndex(e => e.SUBSTITUTE_PARTY_ID, "FROM_CONFIG_INDEX1");

            entity.HasIndex(e => e.PARTY_ID, "FROM_CONFIG_PK1").IsUnique();

            entity.Property(e => e.PARTY_DESCRIPTION)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.PARTY_ID)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.ROUTING_DESCRIPTION)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.ROUTING_ID)
                .HasMaxLength(39)
                .IsUnicode(false);
            entity.Property(e => e.SIGN_ID)
                .HasMaxLength(4)
                .IsUnicode(false);
            entity.Property(e => e.SUBSTITUTE_PARTY_ID)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.TRANSPORT_ID)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.TRANSPORT_PASSWORD)
                .HasMaxLength(45)
                .IsUnicode(false);
        });

        modelBuilder.Entity<SCHEDULE_CONFIG>(entity =>
        {
            entity.HasNoKey();

            entity.HasIndex(e => e.TASK, "SCHEDULE_CONFIG_PK1").IsUnique();

            entity.Property(e => e.ENABLE)
                .HasMaxLength(1)
                .IsUnicode(false);
            entity.Property(e => e.SCHEDULE_PERIOD)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.SCHEDULE_RANGE)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.SCHEDULE_TIME)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.SCHEDULE_TYPE)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.SCHEDULE_WEEK)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.TASK)
                .HasMaxLength(30)
                .IsUnicode(false);
        });

        modelBuilder.Entity<SIGN_CONFIG>(entity =>
        {
            entity.HasNoKey();

            entity.HasIndex(e => e.SIGN_ID, "SIGN_CONFIG_PK1").IsUnique();

            entity.Property(e => e.PFX_PATH)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasDefaultValueSql("(NULL)");
            entity.Property(e => e.SIGN_ID)
                .HasMaxLength(4)
                .IsUnicode(false);
            entity.Property(e => e.SIGN_PASSWORD)
                .HasMaxLength(60)
                .IsUnicode(false)
                .HasDefaultValueSql("(NULL)");
            entity.Property(e => e.SIGN_TYPE)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasDefaultValueSql("(NULL)");
        });

        modelBuilder.Entity<TASK_CONFIG>(entity =>
        {
            entity.HasNoKey();

            entity.HasIndex(e => new { e.CATEGORY_TYPE, e.PROCESS_TYPE, e.TASK }, "TASK_CONFIG_PK1").IsUnique();

            entity.Property(e => e.CATEGORY_TYPE)
                .HasMaxLength(5)
                .IsUnicode(false);
            entity.Property(e => e.ENCODING)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.FILE_FORMAT)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.PROCESS_TYPE)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.SRC_PATH)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.TARGET_PATH)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.TASK)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.TRANS_CHINESE_DATE)
                .HasMaxLength(1)
                .IsUnicode(false);
            entity.Property(e => e.VERSION)
                .HasMaxLength(5)
                .IsUnicode(false);
        });

        modelBuilder.Entity<TO_CONFIG>(entity =>
        {
            entity.HasNoKey();

            entity.HasIndex(e => new { e.FROM_PARTY_ID, e.PARTY_ID }, "TO_CONFIG_PK1").IsUnique();

            entity.Property(e => e.FROM_PARTY_ID)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.PARTY_DESCRIPTION)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.PARTY_ID)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.ROUTING_DESCRIPTION)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.ROUTING_ID)
                .HasMaxLength(39)
                .IsUnicode(false);
        });

        modelBuilder.Entity<TURNKEY_MESSAGE_LOG>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable(tb => tb.HasTrigger("TR_SendToLog"));

            entity.HasIndex(e => e.MESSAGE_DTS, "TURNKEY_MESSAGE_LOG_INDEX1");

            entity.HasIndex(e => e.UUID, "TURNKEY_MESSAGE_LOG_INDEX2");

            entity.HasIndex(e => new { e.SEQNO, e.SUBSEQNO }, "TURNKEY_MESSAGE_LOG_PK1").IsUnique();

            entity.Property(e => e.CATEGORY_TYPE)
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasDefaultValueSql("(NULL)");
            entity.Property(e => e.CHARACTER_COUNT)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasDefaultValueSql("(NULL)");
            entity.Property(e => e.FROM_PARTY_ID)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasDefaultValueSql("(NULL)");
            entity.Property(e => e.FROM_ROUTING_ID)
                .HasMaxLength(39)
                .IsUnicode(false)
                .HasDefaultValueSql("(NULL)");
            entity.Property(e => e.INVOICE_IDENTIFIER)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasDefaultValueSql("(NULL)");
            entity.Property(e => e.IN_OUT_BOUND)
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasDefaultValueSql("(NULL)");
            entity.Property(e => e.MESSAGE_DTS)
                .HasMaxLength(17)
                .IsUnicode(false)
                .HasDefaultValueSql("(NULL)");
            entity.Property(e => e.MESSAGE_TYPE)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasDefaultValueSql("(NULL)");
            entity.Property(e => e.PROCESS_TYPE)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasDefaultValueSql("(NULL)");
            entity.Property(e => e.SEQNO)
                .HasMaxLength(8)
                .IsUnicode(false);
            entity.Property(e => e.STATUS)
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasDefaultValueSql("(NULL)");
            entity.Property(e => e.SUBSEQNO)
                .HasMaxLength(5)
                .IsUnicode(false);
            entity.Property(e => e.TO_PARTY_ID)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasDefaultValueSql("(NULL)");
            entity.Property(e => e.TO_ROUTING_ID)
                .HasMaxLength(39)
                .IsUnicode(false)
                .HasDefaultValueSql("(NULL)");
            entity.Property(e => e.UUID)
                .HasMaxLength(40)
                .IsUnicode(false)
                .HasDefaultValueSql("(NULL)");
        });

        modelBuilder.Entity<TURNKEY_MESSAGE_LOG_DETAIL>(entity =>
        {
            entity.HasNoKey();

            entity.HasIndex(e => e.FILENAME, "TURNKEY_MESSAGE_LOG_DETAIL_INDEX1");

            entity.HasIndex(e => new { e.SEQNO, e.SUBSEQNO, e.TASK }, "TURNKEY_MESSAGE_LOG_DETAIL_PK1").IsUnique();

            entity.Property(e => e.FILENAME)
                .HasMaxLength(300)
                .IsUnicode(false);
            entity.Property(e => e.PROCESS_DTS)
                .HasMaxLength(17)
                .IsUnicode(false);
            entity.Property(e => e.SEQNO)
                .HasMaxLength(8)
                .IsUnicode(false);
            entity.Property(e => e.STATUS)
                .HasMaxLength(5)
                .IsUnicode(false);
            entity.Property(e => e.SUBSEQNO)
                .HasMaxLength(5)
                .IsUnicode(false);
            entity.Property(e => e.TASK)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.UUID)
                .HasMaxLength(40)
                .IsUnicode(false);
        });

        modelBuilder.Entity<TURNKEY_SEQUENCE>(entity =>
        {
            entity.HasNoKey();

            entity.HasIndex(e => e.SEQUENCE, "TURNKEY_SEQUENCE_PK1").IsUnique();

            entity.Property(e => e.SEQUENCE)
                .HasMaxLength(8)
                .IsUnicode(false);
        });

        modelBuilder.Entity<TURNKEY_SYSEVENT_LOG>(entity =>
        {
            entity.HasNoKey();

            entity.HasIndex(e => new { e.SEQNO, e.SUBSEQNO }, "TURNKEY_SYSEVENT_LOG_INDEX1");

            entity.HasIndex(e => e.UUID, "TURNKEY_SYSEVENT_LOG_INDEX2");

            entity.HasIndex(e => e.EVENTDTS, "TURNKEY_SYSEVENT_LOG_PK1").IsUnique();

            entity.Property(e => e.ERRORCODE)
                .HasMaxLength(4)
                .IsUnicode(false);
            entity.Property(e => e.EVENTDTS)
                .HasMaxLength(17)
                .IsUnicode(false);
            entity.Property(e => e.INFORMATION1)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.INFORMATION2)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.INFORMATION3)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.MESSAGE1)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.MESSAGE2)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.MESSAGE3)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.MESSAGE4)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.MESSAGE5)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.MESSAGE6)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.PARTY_ID)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.SEQNO)
                .HasMaxLength(8)
                .IsUnicode(false);
            entity.Property(e => e.SUBSEQNO)
                .HasMaxLength(5)
                .IsUnicode(false);
            entity.Property(e => e.UUID)
                .HasMaxLength(40)
                .IsUnicode(false);
        });

        modelBuilder.Entity<TURNKEY_TRANSPORT_CONFIG>(entity =>
        {
            entity.HasNoKey();

            entity.HasIndex(e => e.TRANSPORT_ID, "TURNKEY_TRANSPORT_CONFIG_PK1").IsUnique();

            entity.Property(e => e.TRANSPORT_ID)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.TRANSPORT_PASSWORD)
                .HasMaxLength(60)
                .IsUnicode(false);
        });

        modelBuilder.Entity<TURNKEY_USER_PROFILE>(entity =>
        {
            entity.HasNoKey();

            entity.HasIndex(e => e.USER_ID, "TURNKEY_USER_PROFILE_PK1").IsUnique();

            entity.Property(e => e.USER_ID)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.USER_PASSWORD)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.USER_ROLE)
                .HasMaxLength(2)
                .IsUnicode(false);
        });

        modelBuilder.Entity<TurnkeyTriggerLog>(entity =>
        {
            entity.HasKey(e => e.LogID);

            entity.Property(e => e.CATEGORY_TYPE)
                .HasMaxLength(5)
                .IsUnicode(false);
            entity.Property(e => e.CHARACTER_COUNT)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.FROM_PARTY_ID)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.FROM_ROUTING_ID)
                .HasMaxLength(39)
                .IsUnicode(false);
            entity.Property(e => e.INVOICE_IDENTIFIER)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.IN_OUT_BOUND)
                .HasMaxLength(1)
                .IsUnicode(false);
            entity.Property(e => e.MESSAGE_DTS)
                .HasMaxLength(17)
                .IsUnicode(false);
            entity.Property(e => e.MESSAGE_TYPE)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.PROCESS_TYPE)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.SEQNO)
                .HasMaxLength(8)
                .IsUnicode(false);
            entity.Property(e => e.STATUS)
                .HasMaxLength(5)
                .IsUnicode(false);
            entity.Property(e => e.SUBSEQNO)
                .HasMaxLength(5)
                .IsUnicode(false);
            entity.Property(e => e.TO_PARTY_ID)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.TO_ROUTING_ID)
                .HasMaxLength(39)
                .IsUnicode(false);
            entity.Property(e => e.UUID)
                .HasMaxLength(40)
                .IsUnicode(false);
        });

        modelBuilder.Entity<V_Allowance>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("V_Allowance");

            entity.Property(e => e.AllowanceDate).HasColumnType("datetime");
            entity.Property(e => e.AllowanceNo)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.DocType)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.MESSAGE_DTS).HasColumnType("datetime");
            entity.Property(e => e.SEQNO)
                .HasMaxLength(8)
                .IsUnicode(false);
            entity.Property(e => e.STATUS)
                .HasMaxLength(5)
                .IsUnicode(false);
            entity.Property(e => e.SUBSEQNO)
                .HasMaxLength(5)
                .IsUnicode(false);
        });

        modelBuilder.Entity<V_Invoice>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("V_Invoice");

            entity.Property(e => e.DocType)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.InvoiceDate).HasColumnType("datetime");
            entity.Property(e => e.InvoiceNo)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.MESSAGE_DTS).HasColumnType("datetime");
            entity.Property(e => e.No)
                .HasMaxLength(16)
                .IsUnicode(false);
            entity.Property(e => e.SEQNO)
                .HasMaxLength(8)
                .IsUnicode(false);
            entity.Property(e => e.STATUS)
                .HasMaxLength(5)
                .IsUnicode(false);
            entity.Property(e => e.SUBSEQNO)
                .HasMaxLength(5)
                .IsUnicode(false);
            entity.Property(e => e.TrackCode)
                .HasMaxLength(4)
                .IsUnicode(false);
        });

        modelBuilder.Entity<V_Logs>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("V_Logs");

            entity.Property(e => e.DocType)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.InvoiceDate).HasColumnType("datetime");
            entity.Property(e => e.MESSAGE_DTS).HasColumnType("datetime");
            entity.Property(e => e.No)
                .HasMaxLength(16)
                .IsUnicode(false);
            entity.Property(e => e.SEQNO)
                .HasMaxLength(8)
                .IsUnicode(false);
            entity.Property(e => e.STATUS)
                .HasMaxLength(5)
                .IsUnicode(false);
            entity.Property(e => e.SUBSEQNO)
                .HasMaxLength(5)
                .IsUnicode(false);
            entity.Property(e => e.TrackCode)
                .HasMaxLength(4)
                .IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
