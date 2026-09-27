USE [EIVO03]
GO
SET NOCOUNT ON;
GO
/*==============================================================================
  查詢 dbo.InvoiceIssuerAgent 中「循環對應」的 Agent / Issuer

  資料表語意：一列代表一條有向邊 AgentID -> IssuerID
             （AgentID 為代理／主機構，IssuerID 為被代理的開立人）

  以下提供三段查詢，可依需要單獨執行：
    [1] 自我對應 (self loop)：AgentID = IssuerID
    [2] 互為代理 (2 節點循環)：A -> B 且 B -> A
    [3] 任意長度循環 (A -> B -> C -> ... -> A)，以遞迴 CTE 偵測
  實測 (EIVO03, 2026-09-14, 6106 筆邊)：
    自我對應 89 筆、互為代理 1 組、無 3 層以上循環；
    整張圖最長路徑僅 4 層，共展開 11171 條路徑，@MaxDepth = 10 綽綽有餘。
==============================================================================*/


/*------------------------------------------------------------------------------
  [1] 自我對應：自己代理自己
------------------------------------------------------------------------------*/
SELECT
    N'自我對應'                 AS RelationKind,
    a.AgentID,
    oa.ReceiptNo                AS AgentReceiptNo,
    oa.CompanyName              AS AgentCompanyName,
    a.IssuerID,
    a.RelationType
FROM dbo.InvoiceIssuerAgent AS a
    LEFT JOIN dbo.Organization AS oa ON oa.CompanyID = a.AgentID
WHERE a.AgentID = a.IssuerID
ORDER BY a.AgentID;
GO


/*------------------------------------------------------------------------------
  [2] 互為代理：A -> B 且 B -> A
      以 AgentID < IssuerID 過濾，讓同一組關係只輸出一列
------------------------------------------------------------------------------*/
SELECT
    N'互為代理'                 AS RelationKind,
    a.AgentID,
    oa.ReceiptNo                AS AgentReceiptNo,
    oa.CompanyName              AS AgentCompanyName,
    a.RelationType              AS RelationType_A2B,
    a.IssuerID,
    oi.ReceiptNo                AS IssuerReceiptNo,
    oi.CompanyName              AS IssuerCompanyName,
    b.RelationType              AS RelationType_B2A
FROM dbo.InvoiceIssuerAgent AS a
    INNER JOIN dbo.InvoiceIssuerAgent AS b
        ON  b.AgentID  = a.IssuerID
        AND b.IssuerID = a.AgentID
    LEFT JOIN dbo.Organization AS oa ON oa.CompanyID = a.AgentID
    LEFT JOIN dbo.Organization AS oi ON oi.CompanyID = a.IssuerID
WHERE a.AgentID < a.IssuerID
ORDER BY a.AgentID, a.IssuerID;
GO


/*------------------------------------------------------------------------------
  [3] 任意長度循環 (含自我對應與互為代理)

      作法：從每條邊出發沿 AgentID -> IssuerID 往下走，
            - 路徑中已出現過的節點不再展開（避免無限遞迴）
            - 走回起點 RootID 即判定為循環 (IsCycle = 1) 並停止
            - 最後只保留 RootID = 該循環中最小 ID 的列，
              讓同一個環不會因為起點不同而重複輸出

      @MaxDepth：循環長度上限，資料量大時可調小以控制成本
------------------------------------------------------------------------------*/
DECLARE @MaxDepth int = 10;

WITH Edges AS
(
    SELECT AgentID, IssuerID, RelationType
    FROM dbo.InvoiceIssuerAgent
),
Walk AS
(
    -- 起始邊
    SELECT
        e.AgentID                                                        AS RootID,
        e.IssuerID                                                       AS CurrentID,
        CAST('/' + CAST(e.AgentID  AS varchar(11)) +
             '/' + CAST(e.IssuerID AS varchar(11)) + '/' AS varchar(4000)) AS NodePath,
        1                                                                AS Depth,
        CASE WHEN e.AgentID < e.IssuerID THEN e.AgentID ELSE e.IssuerID END AS MinID,
        CAST(CASE WHEN e.IssuerID = e.AgentID THEN 1 ELSE 0 END AS bit)  AS IsCycle
    FROM Edges AS e

    UNION ALL

    -- 往下一層展開
    SELECT
        w.RootID,
        e.IssuerID,
        CAST(w.NodePath + CAST(e.IssuerID AS varchar(11)) + '/' AS varchar(4000)),
        w.Depth + 1,
        CASE WHEN e.IssuerID < w.MinID THEN e.IssuerID ELSE w.MinID END,
        CAST(CASE WHEN e.IssuerID = w.RootID THEN 1 ELSE 0 END AS bit)
    FROM Walk AS w
        INNER JOIN Edges AS e ON e.AgentID = w.CurrentID
    WHERE w.IsCycle = 0
      AND w.Depth < @MaxDepth
      AND ( e.IssuerID = w.RootID                                            -- 允許走回起點以封閉環
            OR w.NodePath NOT LIKE '%/' + CAST(e.IssuerID AS varchar(11)) + '/%' )
)
SELECT
    CASE WHEN w.Depth = 1 THEN N'自我對應'
         WHEN w.Depth = 2 THEN N'互為代理'
         ELSE N'多層循環' END        AS RelationKind,
    w.Depth                          AS CycleLength,
    w.RootID                         AS StartCompanyID,
    o.ReceiptNo                      AS StartReceiptNo,
    o.CompanyName                    AS StartCompanyName,
    w.NodePath                       AS CyclePath        -- 例：/12/34/56/12/
FROM Walk AS w
    LEFT JOIN dbo.Organization AS o ON o.CompanyID = w.RootID
WHERE w.IsCycle = 1
  AND w.RootID = w.MinID            -- 同一個環只以最小 ID 為起點輸出一次
ORDER BY w.Depth, w.RootID
OPTION (MAXRECURSION 0);
GO
