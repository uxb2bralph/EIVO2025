/*==============================================================================
  目的：優化「依開立人(SellerID) / 代理開立(InvoiceIssuerAgent) 分頁查詢發票」

  對應查詢（LINQ: ExtensionMethods.GetInvoiceByAgent）：
      FROM InvoiceItem t0
      JOIN CDS_Document t1 ON t1.DocID = t0.InvoiceID
      WHERE (t0.SellerID = @p0 OR EXISTS (SELECT 1 FROM InvoiceIssuerAgent t2
                                          WHERE t2.IssuerID = t0.SellerID AND t2.AgentID = @p1))
        AND t1.DocType = @p2
      ORDER BY ROW_NUMBER() OVER (ORDER BY t0.InvoiceID, ...)  -- 分頁

  執行計畫問題：
    * InvoiceItem 目前沒有 SellerID 索引（只有 PK(InvoiceID)、InvoiceDate、No、TrackCode），
      所以 SellerID 條件只能用 Clustered Index Scan 逐列過濾。
      TableCardinality = 2.09 億列，AvgRowSize = 2,295 bytes（含 Remark nvarchar(2048)）。
    * 計畫顯示 EstimateRows=111、SubtreeCost=0.29 是被 TOP 100 的 row goal 壓低的假象；
      EstimateRowsWithoutRowGoal = 1.88 億，EstimatedRowsRead = 2.09 億。
      只要該開立人的發票不是集中在 InvoiceID 前段，就會掃描整個叢集索引。

  對策：加上 (SellerID, InvoiceID) 非叢集索引，讓上述條件變成 Index Seek，
        且輸出順序 = InvoiceID，剛好符合分頁的 ROW_NUMBER 排序（免 Sort）。

  建議在離峰時間執行；索引大小約 2~3 GB（2.09 億列，PAGE 壓縮後更小）。
==============================================================================*/
USE [EIVO03]
GO

/*------------------------------------------------------------------------------
  1. 主要索引（必要）：InvoiceItem(SellerID, InvoiceID)

     - InvoiceID 是叢集索引鍵，本來就會被帶進非叢集索引；明確寫出只是表達意圖，
       不會增加額外空間。
     - 查詢會取回全部 24 個欄位（含 Remark nvarchar(2048)），做覆蓋索引不划算；
       分頁後只有 10~100 列需要 Key Lookup，成本可忽略。
------------------------------------------------------------------------------*/
IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE object_id = OBJECT_ID('dbo.InvoiceItem') AND name = 'IX_InvoiceItem_SellerID')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_InvoiceItem_SellerID]
    ON [dbo].[InvoiceItem] ([SellerID] ASC, [InvoiceID] ASC)
    WITH (SORT_IN_TEMPDB = ON, DATA_COMPRESSION = PAGE, MAXDOP = 4
        /*, ONLINE = ON, RESUMABLE = ON   -- Enterprise Edition 才支援，線上系統建議加上 */
    ) ON [PRIMARY];
END
GO

/*------------------------------------------------------------------------------
  2. 進階索引（選用，先觀察再決定）：InvoiceItem(SellerID, InvoiceDate) INCLUDE (No, TrackCode)

     適用於「開立人 + 開立期間」的查詢（發票明細查詢／統計表／月報表）。
     注意：
       - 第二鍵放 InvoiceDate 後，輸出順序不再是 InvoiceID，
         上面那支「無日期條件、依 InvoiceID 分頁」的查詢仍需要索引 1。
       - 2.09 億列的表每多一個非叢集索引就多 2~4 GB 與寫入成本，
         請先用 sys.dm_db_index_usage_stats 觀察索引 1 是否已足夠，再決定是否建立。
------------------------------------------------------------------------------*/
--IF NOT EXISTS (SELECT 1 FROM sys.indexes
--               WHERE object_id = OBJECT_ID('dbo.InvoiceItem') AND name = 'IX_InvoiceItem_SellerID_InvoiceDate')
--BEGIN
--    CREATE NONCLUSTERED INDEX [IX_InvoiceItem_SellerID_InvoiceDate]
--    ON [dbo].[InvoiceItem] ([SellerID] ASC, [InvoiceDate] ASC)
--    INCLUDE ([No], [TrackCode])
--    WITH (SORT_IN_TEMPDB = ON, DATA_COMPRESSION = PAGE, MAXDOP = 4) ON [PRIMARY];
--END
--GO

/*------------------------------------------------------------------------------
  3. 不需異動的部分

     - dbo.CDS_Document：IX_CDS_Document(DocType) 因為叢集鍵 DocID 會被附加，
       計畫中已是 (DocType, DocID) 兩欄 Seek，這是本查詢的最佳形式，不必再加索引。
     - dbo.InvoiceIssuerAgent：PK_InvoiceIssurerAgent(AgentID, IssuerID) 已可直接
       Seek (AgentID=@p1, IssuerID=t0.SellerID)，不必再加索引。
------------------------------------------------------------------------------*/

/*------------------------------------------------------------------------------
  4. 建立後驗證：確認變成 Index Seek、邏輯讀取數大幅下降
------------------------------------------------------------------------------*/
--SET STATISTICS IO, TIME ON;
--SET SHOWPLAN_XML OFF;
--DECLARE @p0 int = 16064, @p1 int = 16064, @p2 int = 10, @p3 int = 10, @p4 int = 10;
--SELECT t0.InvoiceID
--FROM dbo.InvoiceItem AS t0
--INNER JOIN dbo.CDS_Document AS t1 ON t1.DocID = t0.InvoiceID
--WHERE (t0.SellerID = @p0
--       OR EXISTS (SELECT 1 FROM dbo.InvoiceIssuerAgent AS t2
--                  WHERE t2.IssuerID = t0.SellerID AND t2.AgentID = @p1))
--  AND t1.DocType = @p2
--ORDER BY t0.InvoiceID
--OFFSET @p3 ROWS FETCH NEXT @p4 ROWS ONLY;

/*------------------------------------------------------------------------------
  5. 附註：光靠索引還無法解決的兩件事（需改程式端查詢）

  (a) ROW_NUMBER() OVER (ORDER BY 24 個欄位) —— 排序鍵應該只留 InvoiceID（或
      InvoiceDate, InvoiceID）。多欄排序不影響結果（InvoiceID 已唯一），
      卻讓最佳化程式難以判定索引可直接提供順序。

  (b) OR + EXISTS 的寫法讓最佳化程式難以對兩個分支都用 Seek。若加了索引 1 之後
      仍看到 Scan，改成先把「可存取的開立人清單」展開再 JOIN，會穩定得多：

      ;WITH Sellers AS (
          SELECT @p0 AS SellerID
          UNION
          SELECT IssuerID FROM dbo.InvoiceIssuerAgent WHERE AgentID = @p1
      ), Keys AS (
          SELECT t0.InvoiceID
          FROM Sellers s
          JOIN dbo.InvoiceItem t0 ON t0.SellerID = s.SellerID
          JOIN dbo.CDS_Document t1 ON t1.DocID = t0.InvoiceID AND t1.DocType = @p2
          ORDER BY t0.InvoiceID
          OFFSET @p3 ROWS FETCH NEXT @p4 ROWS ONLY
      )
      SELECT t0.*
      FROM Keys k JOIN dbo.InvoiceItem t0 ON t0.InvoiceID = k.InvoiceID
      ORDER BY t0.InvoiceID;

      對應的 LINQ 寫法（ExtensionMethods.GetInvoiceByAgent）：
          var sellers = mgr.GetTable<InvoiceIssuerAgent>()
                           .Where(a => a.AgentID == agentID).Select(a => a.IssuerID)
                           .Concat(new[] { agentID }.AsQueryable());   // 或前端先查出清單再 Contains
          return items.Where(i => sellers.Contains(i.SellerID.Value));

  (c) 此計畫的 CardinalityEstimationModelVersion = 70（舊版基數估算器），
      row goal 誤估會更嚴重。若日後有機會調整資料庫相容性層級，可一併評估，
      但屬於全域影響，需完整回歸測試，不建議與本次索引一起變更。
------------------------------------------------------------------------------*/
