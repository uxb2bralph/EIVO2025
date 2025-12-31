# WebClient (Vue 3 + Vite)

簡單的 Vue 3 + Vite 前端範本，放在 `ClientApp` 資料夾內。

快速使用：

1. 切換到專案資料夾：

```powershell
cd WebClient\ClientApp
```

2. 安裝相依套件並啟動開發伺服器：

```powershell
npm install
npm run dev
```
 
 VS Code 同步啟動 (.NET + Vite)
 - 開啟左側 Run 面板，選擇 `Launch .NET + Vite`，按 F5 啟動。這會先執行 `dotnet build`，並啟動 Vite（由 `npm run dev` 提供）。
 - 注意：Vite / npm 需已安裝於系統路徑中；後端啟動使用 `WebHome` 專案的輸出路徑 `bin/Debug/net7.0/WebHome.dll`，請確認該專案可建立且支援目標 framework。
 - 開啟左側 Run 面板，選擇 `Launch .NET + Vite`，按 F5 啟動。這會先執行 `dotnet build`，並啟動 Vite（由 `npm run dev` 提供）。
 - 注意：Vite / npm 需已安裝於系統路徑中；後端啟動使用 `WebClient` 專案的輸出路徑 `bin/Debug/net7.0/WebClient.dll`，請確認該專案可建立且支援目標 framework。

3. 建置生產檔案：

```powershell
npm run build
```

備註：`WebClient.csproj` 只作為容器/範本，開發時直接使用 `npm` / `vite`。如需透過 .NET 主機整合前端，請依需求調整 `csproj` 與中介層。
