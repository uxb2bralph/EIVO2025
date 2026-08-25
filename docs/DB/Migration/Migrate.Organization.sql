/*=============================================================================
  資料遷移：Organization 及其相關資料表   (Microsoft SQL Server)
  -----------------------------------------------------------------------------
  來源端與目的端 schema 完全相同（皆為 dbo），CompanyID 一律沿用來源值。

  遷移對象（依外鍵相依順序）：
    1. dbo.Organization                （PK CompanyID，IDENTITY）
    2. dbo.OrganizationExtension
    3. dbo.OrganizationCustomSetting
    4. dbo.OrganizationSettings        （PK = CompanyID + Settings）
    5. dbo.OrganizationStatus          （FK → LevelExpression / UserToken）
    6. dbo.OrganizationToken           （Thumbprint 唯一索引）
    7. dbo.OrganizationCategory        （PK OrgaCateID，IDENTITY；FK → CategoryDefinition）

  前置條件（不在本腳本範圍內，需先完成）：
    * 目的端已建立上述 7 個資料表與其索引／外鍵。
    * 目的端已存在被參照的父表資料：
        dbo.CategoryDefinition（OrganizationCategory.CategoryID）
        dbo.LevelExpression   （OrganizationStatus.CurrentLevel）
        dbo.UserToken         （OrganizationStatus.TokenID）
      父表資料若缺漏，本腳本會依 @NullifyMissingFK 設定「填 NULL」或「跳過該列」，
      並在最後的「遷移結果」報表中列出。

  執行方式：
    1. SSMS 開啟本檔 → 功能表【查詢】→【SQLCMD 模式】（或 sqlcmd -i 執行）。
    2. 修改下方 :setvar 的來源／目的資料庫名稱。
    3. 直接執行；全部 DML 在單一交易內，任何錯誤都會整批 ROLLBACK。
    4. 若不想使用 SQLCMD 模式，把 $(SRC) 取代成 [來源DB].[dbo]，
       $(TGTDB) 取代成目的資料庫名稱即可。

  特性：
    * 可重複執行：以 PK／自然索引鍵 MERGE，已存在者 UPDATE、不存在者 INSERT。
    * 不刪除目的端既有資料；如需與來源完全同步，見檔尾「附錄 A：選用的同步刪除」。
    * IDENTITY 欄位以 IDENTITY_INSERT 沿用原值，結束後 DBCC CHECKIDENT 重設種子。
=============================================================================*/

/* 來源：同一台實例用 [來源DB].[dbo]；跨實例改成 [連結伺服器].[來源DB].[dbo] */
/* 注意：:setvar 這一行不可以加註解或任何其他文字，否則 sqlcmd 會回報
         「Incorrect syntax was encountered while parsing :setvar.」        */
:setvar SRC "[EIVO03].[dbo]"
/* 目的資料庫名稱 */
:setvar TGTDB "EIVO2025"

USE [$(TGTDB)];
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

-----------------------------------------------------------------------------
-- 0. 選項
-----------------------------------------------------------------------------
DECLARE @NullifyMissingFK        bit = 1;   /* 1 = FK 對應不到父表時該欄填 NULL（其餘欄位照樣遷移）
                                               0 = FK 對應不到父表時整列跳過                        */
DECLARE @AbortOnCompanyIDConflict bit = 1;  /* 1 = 目的端同一 CompanyID 卻是不同統編時中止（建議）
                                               0 = 直接以來源資料覆寫目的端該筆                     */

-----------------------------------------------------------------------------
-- 1. 遷移範圍（預設：來源端全部機構）
-----------------------------------------------------------------------------
IF OBJECT_ID(N'tempdb..#Scope') IS NOT NULL DROP TABLE #Scope;
CREATE TABLE #Scope (CompanyID int NOT NULL PRIMARY KEY);

INSERT INTO #Scope (CompanyID)
SELECT s.CompanyID
FROM $(SRC).[Organization] AS s
/* 只遷移部分機構時，取消下面條件的註解並自行調整：
WHERE s.ReceiptNo IN (N'12345678', N'87654321')
*/
;
DECLARE @ScopeCount int = @@ROWCOUNT;
RAISERROR (N'遷移範圍：%d 家機構。', 0, 1, @ScopeCount) WITH NOWAIT;

IF OBJECT_ID(N'tempdb..#Issue') IS NOT NULL DROP TABLE #Issue;
CREATE TABLE #Issue
(
    TableName sysname       NOT NULL,
    CompanyID int           NULL,
    Issue     nvarchar(400) NOT NULL
);

IF OBJECT_ID(N'tempdb..#Act') IS NOT NULL DROP TABLE #Act;
CREATE TABLE #Act (Action nvarchar(10) NOT NULL);

IF OBJECT_ID(N'tempdb..#Audit') IS NOT NULL DROP TABLE #Audit;
CREATE TABLE #Audit
(
    Seq       int identity(1, 1) NOT NULL,
    TableName sysname            NOT NULL,
    Action    nvarchar(20)       NOT NULL,
    Rows      int                NOT NULL
);

-----------------------------------------------------------------------------
-- 2. 前置檢查
-----------------------------------------------------------------------------
/* 2-1 CompanyID 撞號：目的端已有同一 CompanyID，但統編不同 → 代表兩邊號碼空間不一致，
       若硬寫入會讓所有子表資料掛到錯誤的機構上。 */
IF EXISTS
(
    SELECT 1
    FROM $(SRC).[Organization] AS s
        INNER JOIN #Scope             AS p ON p.CompanyID = s.CompanyID
        INNER JOIN [dbo].[Organization] AS t ON t.CompanyID = s.CompanyID
    WHERE ISNULL(s.ReceiptNo, N'~') <> ISNULL(t.ReceiptNo, N'~')
)
BEGIN
    SELECT N'CompanyID 撞號' AS [問題],
           s.CompanyID,
           s.ReceiptNo   AS [來源統編],
           s.CompanyName AS [來源名稱],
           t.ReceiptNo   AS [目的統編],
           t.CompanyName AS [目的名稱]
    FROM $(SRC).[Organization] AS s
        INNER JOIN #Scope             AS p ON p.CompanyID = s.CompanyID
        INNER JOIN [dbo].[Organization] AS t ON t.CompanyID = s.CompanyID
    WHERE ISNULL(s.ReceiptNo, N'~') <> ISNULL(t.ReceiptNo, N'~')
    ORDER BY s.CompanyID;

    IF @AbortOnCompanyIDConflict = 1
    BEGIN
        RAISERROR (N'目的端存在相同 CompanyID 但不同統編的機構，已中止遷移；請先確認上表並縮小 #Scope 或調整 @AbortOnCompanyIDConflict。', 16, 1);
        RETURN;
    END;

    INSERT INTO #Issue (TableName, CompanyID, Issue)
    SELECT N'Organization', s.CompanyID, N'CompanyID 撞號，已以來源資料覆寫目的端'
    FROM $(SRC).[Organization] AS s
        INNER JOIN #Scope             AS p ON p.CompanyID = s.CompanyID
        INNER JOIN [dbo].[Organization] AS t ON t.CompanyID = s.CompanyID
    WHERE ISNULL(s.ReceiptNo, N'~') <> ISNULL(t.ReceiptNo, N'~');
END;

/* 2-2 父表資料缺漏 */
INSERT INTO #Issue (TableName, CompanyID, Issue)
SELECT N'OrganizationCategory', s.CompanyID,
       N'CategoryID=' + CONVERT(nvarchar(20), s.CategoryID) + N' 不存在於目的端 CategoryDefinition，該列跳過'
FROM $(SRC).[OrganizationCategory] AS s
    INNER JOIN #Scope AS p ON p.CompanyID = s.CompanyID
WHERE NOT EXISTS (SELECT 1 FROM [dbo].[CategoryDefinition] AS c WHERE c.CategoryID = s.CategoryID);

INSERT INTO #Issue (TableName, CompanyID, Issue)
SELECT N'OrganizationStatus', s.CompanyID,
       N'CurrentLevel=' + CONVERT(nvarchar(20), s.CurrentLevel) + N' 不存在於目的端 LevelExpression，'
       + CASE WHEN @NullifyMissingFK = 1 THEN N'已改寫為 NULL' ELSE N'該列跳過' END
FROM $(SRC).[OrganizationStatus] AS s
    INNER JOIN #Scope AS p ON p.CompanyID = s.CompanyID
WHERE s.CurrentLevel IS NOT NULL
      AND NOT EXISTS (SELECT 1 FROM [dbo].[LevelExpression] AS l WHERE l.LevelID = s.CurrentLevel);

INSERT INTO #Issue (TableName, CompanyID, Issue)
SELECT N'OrganizationStatus', s.CompanyID,
       N'TokenID=' + CONVERT(nvarchar(50), s.TokenID) + N' 不存在於目的端 UserToken，'
       + CASE WHEN @NullifyMissingFK = 1 THEN N'已改寫為 NULL' ELSE N'該列跳過' END
FROM $(SRC).[OrganizationStatus] AS s
    INNER JOIN #Scope AS p ON p.CompanyID = s.CompanyID
WHERE s.TokenID IS NOT NULL
      AND NOT EXISTS (SELECT 1 FROM [dbo].[UserToken] AS u WHERE u.Token = s.TokenID);

/* 2-3 OrganizationToken.Thumbprint 唯一索引衝突（同一憑證指紋已被別的機構占用） */
INSERT INTO #Issue (TableName, CompanyID, Issue)
SELECT N'OrganizationToken', s.CompanyID,
       N'Thumbprint 已被目的端 CompanyID=' + CONVERT(nvarchar(20), t.CompanyID) + N' 使用，該列跳過'
FROM $(SRC).[OrganizationToken] AS s
    INNER JOIN #Scope                    AS p ON p.CompanyID = s.CompanyID
    INNER JOIN [dbo].[OrganizationToken] AS t ON t.Thumbprint = s.Thumbprint
                                                 AND t.CompanyID <> s.CompanyID;

-----------------------------------------------------------------------------
-- 3. 資料遷移（單一交易）
-----------------------------------------------------------------------------
BEGIN TRY
    BEGIN TRANSACTION;

    ---------------------------------------------------------------------
    -- 3-1 dbo.Organization   （IDENTITY：CompanyID）
    ---------------------------------------------------------------------
    RAISERROR (N'[1/7] dbo.Organization ...', 0, 1) WITH NOWAIT;

    SET IDENTITY_INSERT [dbo].[Organization] ON;

    MERGE [dbo].[Organization] AS t
    USING
    (
        SELECT s.*
        FROM $(SRC).[Organization] AS s
            INNER JOIN #Scope AS p ON p.CompanyID = s.CompanyID
    ) AS s
    ON t.CompanyID = s.CompanyID
    WHEN MATCHED THEN
        UPDATE SET t.ContactName            = s.ContactName,
                   t.Fax                    = s.Fax,
                   t.LogoURL                = s.LogoURL,
                   t.CompanyName            = s.CompanyName,
                   t.ReceiptNo              = s.ReceiptNo,
                   t.Phone                  = s.Phone,
                   t.ContactFax             = s.ContactFax,
                   t.ContactPhone           = s.ContactPhone,
                   t.ContactMobilePhone     = s.ContactMobilePhone,
                   t.RegAddr                = s.RegAddr,
                   t.UndertakerName         = s.UndertakerName,
                   t.Addr                   = s.Addr,
                   t.EnglishName            = s.EnglishName,
                   t.EnglishAddr            = s.EnglishAddr,
                   t.EnglishRegAddr         = s.EnglishRegAddr,
                   t.ContactEmail           = s.ContactEmail,
                   t.UndertakerPhone        = s.UndertakerPhone,
                   t.UndertakerFax          = s.UndertakerFax,
                   t.UndertakerMobilePhone  = s.UndertakerMobilePhone,
                   t.InvoiceSignature       = s.InvoiceSignature,
                   t.UndertakerID           = s.UndertakerID,
                   t.ContactTitle           = s.ContactTitle
    WHEN NOT MATCHED BY TARGET THEN
        INSERT (CompanyID, ContactName, Fax, LogoURL, CompanyName, ReceiptNo, Phone,
                ContactFax, ContactPhone, ContactMobilePhone, RegAddr, UndertakerName,
                Addr, EnglishName, EnglishAddr, EnglishRegAddr, ContactEmail,
                UndertakerPhone, UndertakerFax, UndertakerMobilePhone, InvoiceSignature,
                UndertakerID, ContactTitle)
        VALUES (s.CompanyID, s.ContactName, s.Fax, s.LogoURL, s.CompanyName, s.ReceiptNo, s.Phone,
                s.ContactFax, s.ContactPhone, s.ContactMobilePhone, s.RegAddr, s.UndertakerName,
                s.Addr, s.EnglishName, s.EnglishAddr, s.EnglishRegAddr, s.ContactEmail,
                s.UndertakerPhone, s.UndertakerFax, s.UndertakerMobilePhone, s.InvoiceSignature,
                s.UndertakerID, s.ContactTitle)
    OUTPUT $action INTO #Act (Action);

    SET IDENTITY_INSERT [dbo].[Organization] OFF;

    INSERT INTO #Audit (TableName, Action, Rows)
    SELECT N'Organization', Action, COUNT(*) FROM #Act GROUP BY Action;
    DELETE FROM #Act;

    ---------------------------------------------------------------------
    -- 3-2 dbo.OrganizationExtension
    ---------------------------------------------------------------------
    RAISERROR (N'[2/7] dbo.OrganizationExtension ...', 0, 1) WITH NOWAIT;

    MERGE [dbo].[OrganizationExtension] AS t
    USING
    (
        SELECT s.*
        FROM $(SRC).[OrganizationExtension] AS s
            INNER JOIN #Scope AS p ON p.CompanyID = s.CompanyID
        WHERE EXISTS (SELECT 1 FROM [dbo].[Organization] AS o WHERE o.CompanyID = s.CompanyID)
    ) AS s
    ON t.CompanyID = s.CompanyID
    WHEN MATCHED THEN
        UPDATE SET t.CustomerNo                    = s.CustomerNo,
                   t.TaxNo                         = s.TaxNo,
                   t.CustomNotification            = s.CustomNotification,
                   t.BusinessContactPhone          = s.BusinessContactPhone,
                   t.ExpirationDate                = s.ExpirationDate,
                   t.GoLiveDate                    = s.GoLiveDate,
                   t.AutoBlankTrack                = s.AutoBlankTrack,
                   t.AutoBlankTrackEmittance       = s.AutoBlankTrackEmittance,
                   t.CreationDate                  = s.CreationDate,
                   t.InvoiceNoSafetyStock          = s.InvoiceNoSafetyStock,
                   t.MailSubjectAlias              = s.MailSubjectAlias,
                   t.AutoTrackCodeAssignment       = s.AutoTrackCodeAssignment,
                   t.AutoPartsTrackCodeAssignment  = s.AutoPartsTrackCodeAssignment,
                   t.ReservedBooklets              = s.ReservedBooklets,
                   t.AuthorizationNotBefore        = s.AuthorizationNotBefore,
                   t.AuthorizationNotAfter         = s.AuthorizationNotAfter,
                   t.InvoiceRequestNotBefore       = s.InvoiceRequestNotBefore,
                   t.InvoiceRequestNotAfter        = s.InvoiceRequestNotAfter
    WHEN NOT MATCHED BY TARGET THEN
        INSERT (CompanyID, CustomerNo, TaxNo, CustomNotification, BusinessContactPhone,
                ExpirationDate, GoLiveDate, AutoBlankTrack, AutoBlankTrackEmittance,
                CreationDate, InvoiceNoSafetyStock, MailSubjectAlias, AutoTrackCodeAssignment,
                AutoPartsTrackCodeAssignment, ReservedBooklets, AuthorizationNotBefore,
                AuthorizationNotAfter, InvoiceRequestNotBefore, InvoiceRequestNotAfter)
        VALUES (s.CompanyID, s.CustomerNo, s.TaxNo, s.CustomNotification, s.BusinessContactPhone,
                s.ExpirationDate, s.GoLiveDate, s.AutoBlankTrack, s.AutoBlankTrackEmittance,
                s.CreationDate, s.InvoiceNoSafetyStock, s.MailSubjectAlias, s.AutoTrackCodeAssignment,
                s.AutoPartsTrackCodeAssignment, s.ReservedBooklets, s.AuthorizationNotBefore,
                s.AuthorizationNotAfter, s.InvoiceRequestNotBefore, s.InvoiceRequestNotAfter)
    OUTPUT $action INTO #Act (Action);

    INSERT INTO #Audit (TableName, Action, Rows)
    SELECT N'OrganizationExtension', Action, COUNT(*) FROM #Act GROUP BY Action;
    DELETE FROM #Act;

    ---------------------------------------------------------------------
    -- 3-3 dbo.OrganizationCustomSetting
    ---------------------------------------------------------------------
    RAISERROR (N'[3/7] dbo.OrganizationCustomSetting ...', 0, 1) WITH NOWAIT;

    MERGE [dbo].[OrganizationCustomSetting] AS t
    USING
    (
        SELECT s.*
        FROM $(SRC).[OrganizationCustomSetting] AS s
            INNER JOIN #Scope AS p ON p.CompanyID = s.CompanyID
        WHERE EXISTS (SELECT 1 FROM [dbo].[Organization] AS o WHERE o.CompanyID = s.CompanyID)
    ) AS s
    ON t.CompanyID = s.CompanyID
    WHEN MATCHED THEN
        UPDATE SET t.SettingData = s.SettingData
    WHEN NOT MATCHED BY TARGET THEN
        INSERT (CompanyID, SettingData)
        VALUES (s.CompanyID, s.SettingData)
    OUTPUT $action INTO #Act (Action);

    INSERT INTO #Audit (TableName, Action, Rows)
    SELECT N'OrganizationCustomSetting', Action, COUNT(*) FROM #Act GROUP BY Action;
    DELETE FROM #Act;

    ---------------------------------------------------------------------
    -- 3-4 dbo.OrganizationSettings   （全部欄位皆為主鍵 → 只需 INSERT）
    ---------------------------------------------------------------------
    RAISERROR (N'[4/7] dbo.OrganizationSettings ...', 0, 1) WITH NOWAIT;

    INSERT INTO [dbo].[OrganizationSettings] (CompanyID, Settings)
    SELECT s.CompanyID, s.Settings
    FROM $(SRC).[OrganizationSettings] AS s
        INNER JOIN #Scope AS p ON p.CompanyID = s.CompanyID
    WHERE EXISTS (SELECT 1 FROM [dbo].[Organization] AS o WHERE o.CompanyID = s.CompanyID)
          AND NOT EXISTS
          (
              SELECT 1
              FROM [dbo].[OrganizationSettings] AS t
              WHERE t.CompanyID = s.CompanyID
                    AND t.Settings = s.Settings
          );

    INSERT INTO #Audit (TableName, Action, Rows)
    VALUES (N'OrganizationSettings', N'INSERT', @@ROWCOUNT);

    ---------------------------------------------------------------------
    -- 3-5 dbo.OrganizationStatus   （FK：LevelExpression / UserToken）
    ---------------------------------------------------------------------
    RAISERROR (N'[5/7] dbo.OrganizationStatus ...', 0, 1) WITH NOWAIT;

    WITH src AS
    (
        SELECT s.*,
               LevelOK = CONVERT(bit, CASE
                                          WHEN s.CurrentLevel IS NULL THEN 1
                                          WHEN EXISTS (SELECT 1 FROM [dbo].[LevelExpression] AS l WHERE l.LevelID = s.CurrentLevel) THEN 1
                                          ELSE 0
                                      END),
               TokenOK = CONVERT(bit, CASE
                                          WHEN s.TokenID IS NULL THEN 1
                                          WHEN EXISTS (SELECT 1 FROM [dbo].[UserToken] AS u WHERE u.Token = s.TokenID) THEN 1
                                          ELSE 0
                                      END)
        FROM $(SRC).[OrganizationStatus] AS s
            INNER JOIN #Scope AS p ON p.CompanyID = s.CompanyID
        WHERE EXISTS (SELECT 1 FROM [dbo].[Organization] AS o WHERE o.CompanyID = s.CompanyID)
    )
    MERGE [dbo].[OrganizationStatus] AS t
    USING
    (
        SELECT src.*,
               /* FK 對應不到父表時填 NULL，避免整列無法遷移 */
               SafeCurrentLevel = CASE WHEN src.LevelOK = 1 THEN src.CurrentLevel ELSE NULL END,
               SafeTokenID      = CASE WHEN src.TokenOK = 1 THEN src.TokenID      ELSE NULL END
        FROM src
        WHERE @NullifyMissingFK = 1
              OR (src.LevelOK = 1 AND src.TokenOK = 1)
    ) AS s
    ON t.CompanyID = s.CompanyID
    WHEN MATCHED THEN
        UPDATE SET t.CurrentLevel                        = s.SafeCurrentLevel,
                   t.LastTimeToAcknowledge               = s.LastTimeToAcknowledge,
                   t.RequestPeriodicalInterval           = s.RequestPeriodicalInterval,
                   t.SetToPrintInvoice                   = s.SetToPrintInvoice,
                   t.InvoicePrintView                    = s.InvoicePrintView,
                   t.IronSteelIndustry                   = s.IronSteelIndustry,
                   t.Entrusting                          = s.Entrusting,
                   t.AuthorizationNo                     = s.AuthorizationNo,
                   t.TokenID                             = s.SafeTokenID,
                   t.SetToOutsourcingCS                  = s.SetToOutsourcingCS,
                   t.AllowancePrintView                  = s.AllowancePrintView,
                   t.SetToNotifyCounterpartBySMS         = s.SetToNotifyCounterpartBySMS,
                   t.DownloadDataNumber                  = s.DownloadDataNumber,
                   t.DownloadDispatch                    = s.DownloadDispatch,
                   t.UploadBranchTrackBlank              = s.UploadBranchTrackBlank,
                   t.PrintAll                            = s.PrintAll,
                   t.SettingInvoiceType                  = s.SettingInvoiceType,
                   t.SubscribeB2BInvoicePDF              = s.SubscribeB2BInvoicePDF,
                   t.UseB2BStandalone                    = s.UseB2BStandalone,
                   t.DisableIssuingNotice                = s.DisableIssuingNotice,
                   t.DisableWinningNotice                = s.DisableWinningNotice,
                   t.EntrustToPrint                      = s.EntrustToPrint,
                   t.EnableTrackCodeInvoiceNoValidation  = s.EnableTrackCodeInvoiceNoValidation,
                   t.EnableBuyerIDValidation             = s.EnableBuyerIDValidation,
                   t.InvoiceNoticeSetting                = s.InvoiceNoticeSetting,
                   t.NotificationFooterView              = s.NotificationFooterView,
                   t.IgnoreDuplicatedDataNumber          = s.IgnoreDuplicatedDataNumber,
                   t.InvoiceClientDefaultProcessType     = s.InvoiceClientDefaultProcessType,
                   t.CustomNotificationView              = s.CustomNotificationView
    WHEN NOT MATCHED BY TARGET THEN
        INSERT (CompanyID, CurrentLevel, LastTimeToAcknowledge, RequestPeriodicalInterval,
                SetToPrintInvoice, InvoicePrintView, IronSteelIndustry, Entrusting,
                AuthorizationNo, TokenID, SetToOutsourcingCS, AllowancePrintView,
                SetToNotifyCounterpartBySMS, DownloadDataNumber, DownloadDispatch,
                UploadBranchTrackBlank, PrintAll, SettingInvoiceType, SubscribeB2BInvoicePDF,
                UseB2BStandalone, DisableIssuingNotice, DisableWinningNotice, EntrustToPrint,
                EnableTrackCodeInvoiceNoValidation, EnableBuyerIDValidation, InvoiceNoticeSetting,
                NotificationFooterView, IgnoreDuplicatedDataNumber,
                InvoiceClientDefaultProcessType, CustomNotificationView)
        VALUES (s.CompanyID, s.SafeCurrentLevel, s.LastTimeToAcknowledge, s.RequestPeriodicalInterval,
                s.SetToPrintInvoice, s.InvoicePrintView, s.IronSteelIndustry, s.Entrusting,
                s.AuthorizationNo, s.SafeTokenID, s.SetToOutsourcingCS, s.AllowancePrintView,
                s.SetToNotifyCounterpartBySMS, s.DownloadDataNumber, s.DownloadDispatch,
                s.UploadBranchTrackBlank, s.PrintAll, s.SettingInvoiceType, s.SubscribeB2BInvoicePDF,
                s.UseB2BStandalone, s.DisableIssuingNotice, s.DisableWinningNotice, s.EntrustToPrint,
                s.EnableTrackCodeInvoiceNoValidation, s.EnableBuyerIDValidation, s.InvoiceNoticeSetting,
                s.NotificationFooterView, s.IgnoreDuplicatedDataNumber,
                s.InvoiceClientDefaultProcessType, s.CustomNotificationView)
    OUTPUT $action INTO #Act (Action);

    INSERT INTO #Audit (TableName, Action, Rows)
    SELECT N'OrganizationStatus', Action, COUNT(*) FROM #Act GROUP BY Action;
    DELETE FROM #Act;

    ---------------------------------------------------------------------
    -- 3-6 dbo.OrganizationToken   （Thumbprint 唯一索引）
    ---------------------------------------------------------------------
    RAISERROR (N'[6/7] dbo.OrganizationToken ...', 0, 1) WITH NOWAIT;

    MERGE [dbo].[OrganizationToken] AS t
    USING
    (
        SELECT s.*
        FROM $(SRC).[OrganizationToken] AS s
            INNER JOIN #Scope AS p ON p.CompanyID = s.CompanyID
        WHERE EXISTS (SELECT 1 FROM [dbo].[Organization] AS o WHERE o.CompanyID = s.CompanyID)
              /* 指紋已被其他機構占用者跳過（已記錄於 #Issue） */
              AND NOT EXISTS
              (
                  SELECT 1
                  FROM [dbo].[OrganizationToken] AS x
                  WHERE x.Thumbprint = s.Thumbprint
                        AND x.CompanyID <> s.CompanyID
              )
    ) AS s
    ON t.CompanyID = s.CompanyID
    WHEN MATCHED THEN
        UPDATE SET t.X509Certificate = s.X509Certificate,
                   t.Thumbprint      = s.Thumbprint,
                   t.PKCS12          = s.PKCS12,
                   t.KeyID           = s.KeyID,
                   t.IsActivated     = s.IsActivated
    WHEN NOT MATCHED BY TARGET THEN
        INSERT (CompanyID, X509Certificate, Thumbprint, PKCS12, KeyID, IsActivated)
        VALUES (s.CompanyID, s.X509Certificate, s.Thumbprint, s.PKCS12, s.KeyID, s.IsActivated)
    OUTPUT $action INTO #Act (Action);

    INSERT INTO #Audit (TableName, Action, Rows)
    SELECT N'OrganizationToken', Action, COUNT(*) FROM #Act GROUP BY Action;
    DELETE FROM #Act;

    ---------------------------------------------------------------------
    -- 3-7 dbo.OrganizationCategory   （IDENTITY：OrgaCateID；自然鍵：CompanyID + CategoryID）
    --     Pass A：OrgaCateID 在目的端未被占用 → 沿用原號
    --     Pass B：OrgaCateID 已被占用       → 由目的端重新配號（自然鍵才是業務鍵）
    ---------------------------------------------------------------------
    RAISERROR (N'[7/7] dbo.OrganizationCategory ...', 0, 1) WITH NOWAIT;

    SET IDENTITY_INSERT [dbo].[OrganizationCategory] ON;

    INSERT INTO [dbo].[OrganizationCategory] (OrgaCateID, CompanyID, CategoryID)
    SELECT s.OrgaCateID, s.CompanyID, s.CategoryID
    FROM $(SRC).[OrganizationCategory] AS s
        INNER JOIN #Scope AS p ON p.CompanyID = s.CompanyID
    WHERE EXISTS (SELECT 1 FROM [dbo].[Organization] AS o WHERE o.CompanyID = s.CompanyID)
          AND EXISTS (SELECT 1 FROM [dbo].[CategoryDefinition] AS c WHERE c.CategoryID = s.CategoryID)
          AND NOT EXISTS
          (
              SELECT 1
              FROM [dbo].[OrganizationCategory] AS t
              WHERE t.CompanyID = s.CompanyID
                    AND t.CategoryID = s.CategoryID
          )
          AND NOT EXISTS
          (
              SELECT 1 FROM [dbo].[OrganizationCategory] AS t WHERE t.OrgaCateID = s.OrgaCateID
          );

    INSERT INTO #Audit (TableName, Action, Rows)
    VALUES (N'OrganizationCategory', N'INSERT(原ID)', @@ROWCOUNT);

    SET IDENTITY_INSERT [dbo].[OrganizationCategory] OFF;

    INSERT INTO [dbo].[OrganizationCategory] (CompanyID, CategoryID)
    SELECT s.CompanyID, s.CategoryID
    FROM $(SRC).[OrganizationCategory] AS s
        INNER JOIN #Scope AS p ON p.CompanyID = s.CompanyID
    WHERE EXISTS (SELECT 1 FROM [dbo].[Organization] AS o WHERE o.CompanyID = s.CompanyID)
          AND EXISTS (SELECT 1 FROM [dbo].[CategoryDefinition] AS c WHERE c.CategoryID = s.CategoryID)
          AND NOT EXISTS
          (
              SELECT 1
              FROM [dbo].[OrganizationCategory] AS t
              WHERE t.CompanyID = s.CompanyID
                    AND t.CategoryID = s.CategoryID
          );

    INSERT INTO #Audit (TableName, Action, Rows)
    VALUES (N'OrganizationCategory', N'INSERT(重配號)', @@ROWCOUNT);

    COMMIT TRANSACTION;
    RAISERROR (N'遷移完成，交易已認可。', 0, 1) WITH NOWAIT;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0
        ROLLBACK TRANSACTION;

    /* IDENTITY_INSERT 為連線層級設定，錯誤時一併還原 */
    IF OBJECTPROPERTY(OBJECT_ID(N'dbo.Organization'), 'TableHasIdentity') = 1
        SET IDENTITY_INSERT [dbo].[Organization] OFF;
    IF OBJECTPROPERTY(OBJECT_ID(N'dbo.OrganizationCategory'), 'TableHasIdentity') = 1
        SET IDENTITY_INSERT [dbo].[OrganizationCategory] OFF;

    THROW;
END CATCH;
GO

-----------------------------------------------------------------------------
-- 4. 重設 IDENTITY 種子（沿用原 ID 後必做，否則新增資料會撞號）
-----------------------------------------------------------------------------
DBCC CHECKIDENT (N'dbo.Organization', RESEED) WITH NO_INFOMSGS;
DBCC CHECKIDENT (N'dbo.OrganizationCategory', RESEED) WITH NO_INFOMSGS;
GO

-----------------------------------------------------------------------------
-- 5. 遷移結果與筆數驗證
-----------------------------------------------------------------------------
SELECT Seq, TableName AS [資料表], Action AS [動作], Rows AS [筆數]
FROM #Audit
ORDER BY Seq;

SELECT TableName AS [資料表], CompanyID, Issue AS [說明]
FROM #Issue
ORDER BY TableName, CompanyID;

SELECT [資料表], [來源筆數], [目的筆數], [差異] = [來源筆數] - [目的筆數]
FROM
(
    SELECT [資料表] = N'1.Organization',
           [來源筆數] = (SELECT COUNT(*) FROM $(SRC).[Organization] s JOIN #Scope p ON p.CompanyID = s.CompanyID),
           [目的筆數] = (SELECT COUNT(*) FROM [dbo].[Organization] t JOIN #Scope p ON p.CompanyID = t.CompanyID)
    UNION ALL
    SELECT N'2.OrganizationExtension',
           (SELECT COUNT(*) FROM $(SRC).[OrganizationExtension] s JOIN #Scope p ON p.CompanyID = s.CompanyID),
           (SELECT COUNT(*) FROM [dbo].[OrganizationExtension] t JOIN #Scope p ON p.CompanyID = t.CompanyID)
    UNION ALL
    SELECT N'3.OrganizationCustomSetting',
           (SELECT COUNT(*) FROM $(SRC).[OrganizationCustomSetting] s JOIN #Scope p ON p.CompanyID = s.CompanyID),
           (SELECT COUNT(*) FROM [dbo].[OrganizationCustomSetting] t JOIN #Scope p ON p.CompanyID = t.CompanyID)
    UNION ALL
    SELECT N'4.OrganizationSettings',
           (SELECT COUNT(*) FROM $(SRC).[OrganizationSettings] s JOIN #Scope p ON p.CompanyID = s.CompanyID),
           (SELECT COUNT(*) FROM [dbo].[OrganizationSettings] t JOIN #Scope p ON p.CompanyID = t.CompanyID)
    UNION ALL
    SELECT N'5.OrganizationStatus',
           (SELECT COUNT(*) FROM $(SRC).[OrganizationStatus] s JOIN #Scope p ON p.CompanyID = s.CompanyID),
           (SELECT COUNT(*) FROM [dbo].[OrganizationStatus] t JOIN #Scope p ON p.CompanyID = t.CompanyID)
    UNION ALL
    SELECT N'6.OrganizationToken',
           (SELECT COUNT(*) FROM $(SRC).[OrganizationToken] s JOIN #Scope p ON p.CompanyID = s.CompanyID),
           (SELECT COUNT(*) FROM [dbo].[OrganizationToken] t JOIN #Scope p ON p.CompanyID = t.CompanyID)
    UNION ALL
    SELECT N'7.OrganizationCategory',
           (SELECT COUNT(*) FROM $(SRC).[OrganizationCategory] s JOIN #Scope p ON p.CompanyID = s.CompanyID),
           (SELECT COUNT(*) FROM [dbo].[OrganizationCategory] t JOIN #Scope p ON p.CompanyID = t.CompanyID)
) AS x
ORDER BY [資料表];
GO

/* 差異若不為 0，對照第 2 份報表（#Issue）即可知道被跳過的原因。
   目的筆數大於來源筆數屬正常：代表目的端原本就有本次範圍外的既有資料。 */

DROP TABLE IF EXISTS #Act;
DROP TABLE IF EXISTS #Audit;
DROP TABLE IF EXISTS #Issue;
DROP TABLE IF EXISTS #Scope;
GO

/*=============================================================================
  附錄 A：選用的同步刪除
  -----------------------------------------------------------------------------
  預設腳本「只新增／更新，不刪除」。若要讓目的端與來源完全一致
  （刪掉來源已不存在的設定列），在第 3 節 COMMIT 之前插入下列語法。
  ※ 會刪除目的端資料，請先備份並確認 #Scope 範圍。

    DELETE t
    FROM [dbo].[OrganizationSettings] AS t
        INNER JOIN #Scope AS p ON p.CompanyID = t.CompanyID
    WHERE NOT EXISTS
    (
        SELECT 1 FROM $(SRC).[OrganizationSettings] AS s
        WHERE s.CompanyID = t.CompanyID AND s.Settings = t.Settings
    );

    DELETE t
    FROM [dbo].[OrganizationCategory] AS t
        INNER JOIN #Scope AS p ON p.CompanyID = t.CompanyID
    WHERE NOT EXISTS
    (
        SELECT 1 FROM $(SRC).[OrganizationCategory] AS s
        WHERE s.CompanyID = t.CompanyID AND s.CategoryID = t.CategoryID
    );

  附錄 B：跨 SQL 實例
  -----------------------------------------------------------------------------
  1. 在目的端建立連結伺服器（一次性）：
       EXEC sp_addlinkedserver   @server = N'SRCSRV', @srvproduct = N'', @provider = N'SQLNCLI';
       EXEC sp_addlinkedsrvlogin @rmtsrvname = N'SRCSRV', @useself = N'FALSE',
                                 @rmtuser = N'sa', @rmtpassword = N'******';
       EXEC sp_serveroption      N'SRCSRV', N'rpc out', N'true';
  2. 把檔頭 SRC 變數的值改成 "[SRCSRV].[EIVO03].[dbo]"
  3. 連結伺服器讀取 nvarchar(max)（X509Certificate / PKCS12 / SettingData / CustomNotification）
     效能較差；資料量大時建議改用備份還原成暫存資料庫，再以本腳本從本機資料庫遷移。

  附錄 C：後續資料表
  -----------------------------------------------------------------------------
  Organization 的其他子表（UserProfile / OrganizationUser、InvoiceNoInterval、
  InvoiceNoAllocation、center.MasterOrganization、billing.* 等）不在本腳本範圍；
  遷移時請沿用同一原則：先父表後子表、沿用原 ID、以 PK/自然鍵 MERGE、最後 CHECKIDENT。
=============================================================================*/
