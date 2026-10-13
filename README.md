# Cat螢幕轉換器 · QxQ

提供貓專用的色差顯示，讓值班畫面更好分辨。

本專案採用 [MIT 授權](LICENSE)，著作權標示為 QxQ。歡迎使用、修改、分享與商用，請保留授權及著作權聲明。第三方元件依各自的授權散佈，詳見 [第三方聲明](THIRD-PARTY-NOTICES.md)。

## 下載安裝

前往 [GitHub Releases](https://github.com/LeeWeiCheng/CatScreenConverter/releases)，下載 `QxQ-CatScreenConverter-Setup-1.0.0-win-x64.exe`。

執行安裝程式後，可從開始功能表開啟「Cat螢幕轉換器」，也可選擇建立桌面捷徑。安裝版包含 .NET 執行元件，不需另外安裝 .NET；適用 Windows 10 22H2／Windows 11 x64。解除安裝前請先關閉程式，再從 Windows「已安裝的應用程式」移除。

安裝檔尚未簽署發行者憑證；下載時可用發佈頁面的 `SHA256SUMS.txt` 核對檔案。

Windows 64 位元桌面程式，提供「正常／貓」兩個模式。透過 Windows Magnification API 的全桌面色彩矩陣套用近似重映射，不自行擷取畫面、不開啟影片預覽視窗，也不變更螢幕解析度或縮放。

## 使用

1. 先關閉已開啟的舊版，從開始功能表或桌面捷徑開啟「Cat螢幕轉換器」。預設為「正常」，啟動時不套用濾鏡。
2. 按「貓」讓貓咪上工，套用全桌面效果。可以調整「貓用色差強度」、「工作畫面亮度」與「畫面明暗對比」。
3. 按「正常」或 `Ctrl + Alt + N` 還原啟用貓模式之前的色彩矩陣。
4. 正常關閉程式時也會還原。若 Windows 拒絕還原，程式會提示並保留視窗供重試。

快捷鍵若被其他程式佔用，介面會提示改用「正常」按鈕。程式限制單一執行個體，避免兩個 CatVideo 實例互相覆寫效果。

## 貓咪主題介面

介面以「提升貓的工作效率」為主題，使用人類／貓咪值班的語氣，搭配奶油色背景、咖啡色文字、橘黃模式卡片與粉橘狀態列。貓模式卡片有貓耳裝飾，滑桿使用貓掌與貓頭滑塊，並加入魚、腳印、貓咪與毛線球的可縮放繪圖。

「回到預設工作設定」會將三個滑桿還原成 75%、100%、100%，保持目前的模式；「交回人類值班：Ctrl + Alt + N」會切回正常模式。值班與工作效率是介面的趣味文案，實際功能是近似色差轉換。

滑桿可點擊或拖曳；按 Tab 聚焦後，可用方向鍵微調、Page Up／Page Down 調整 5 格、Home／End 移到最小／最大值。模式卡片與還原按鈕保留按鈕的鍵盤操作。選中的卡片會填上橘黃色。

介面使用 WPF 內建的 `Viewbox`，以 `Stretch="Uniform"` 將完整內容等比例縮放。拖拉視窗時，文字、按鈕、滑桿、插畫與點擊範圍一起縮放；視窗長寬比例不同時，內容置中，其餘區域維持奶油色背景。

初始視窗為 560 × 700 個 WPF 邏輯單位，最小尺寸為 360 × 460；完整內容會縮放到視窗內，不需要捲動。Windows 11 的標題列配色與貓咪圖示也會搭配主題。

WPF 版本已在目前螢幕環境完成初始尺寸、縮小與最大化排版檢查，以及滑桿拖曳、鍵盤微調、模式切換、預設值還原、快捷鍵與關閉流程檢查。最小尺寸與其他螢幕／DPI 尚未分別測試，詳細紀錄保存在 `artifacts/test-results.json`。

## 模型與限制

- 這是給貓觀看的近似辨識增強，不是供人觀看的貓視覺模擬，也不是經過行為實驗驗證的貓色覺校正。
- 完整轉換採用 `R' = G' = 0.20R + 0.80G`、`B' = 0.65R + 0.35B`，將部分紅色線索轉往藍色方向，綠色線索轉往黃色方向。係數是設計上的近似值，不代表量測的感受器敏感度。
- 預設強度 75%，以原色矩陣與上述矩陣混合；預設亮度、對比皆為 100%。黑白在未調整亮度、對比時維持中性。
- 這是整體線性轉換，無法依物體分別配色，有些顏色仍可能合併。提高對比或亮度可能使亮部、暗部失去細節。
- 「畫面亮度」調整像素輸出，不控制螢幕實體背光。
- 效果作用於桌面，沒有單獨選擇螢幕的功能。HDR、多螢幕、影片與獨佔全螢幕的實際相容性尚待執行測試。
- 系統放大鏡與其他桌面濾鏡可能共用色彩效果。使用時避免其他程式同時切換效果；還原目標是啟用貓模式前讀取的矩陣。
- 正常關閉與可處理的例外會嘗試還原；強制終止、斷電等情況無法保證執行清理。如果效果未還原，可重新登入 Windows。

## 建置

### 安裝版

需要 .NET SDK（版本見 `global.json`）與 Inno Setup 7.1.0 x64。使用下列指令建立包含執行元件的安裝檔：

```powershell
.\packaging\Build-Release.ps1 -InnoCompiler 'C:\path\to\Inno Setup 7\ISCC.exe'
```

輸出位於 `artifacts/release`，包含安裝檔、SHA-256 校驗碼、發佈說明及 `Build-Info.json` 來源紀錄。安裝包包含本專案 MIT 授權與實際 runtime 套件的第三方聲明。此指令只編譯與打包，不執行安裝或啟動測試。`Assets/CatScreen.ico` 使用與視窗相同的貓臉向量，可用 Windows PowerShell 執行 `packaging/Export-Icon.ps1` 重新產生。

### GitHub 自動建置

[Build Windows installer](https://github.com/LeeWeiCheng/CatScreenConverter/actions/workflows/build.yml) 在提交至 `main`、Pull Request 及手動執行時，使用 GitHub-hosted Windows runner 編譯及打包，並上傳名稱包含 `unsigned` 的建置產物。

SDK、GitHub Actions 及 Inno Setup 下載檔版本固定在專案中，方便核對來源。此流程不簽署、不執行程式或安裝測試，也不自動建立 GitHub Release。CI artifact 是供檢查的編譯產物；一般使用者請下載正式 Releases。

### 開發用版本

需要 .NET 10 SDK；輸出的版本需要 .NET 10 Windows Desktop Runtime（x64）。

```powershell
dotnet publish CatVideo.csproj -c Release -r win-x64 --self-contained false -o artifacts/CatVideo-WPF
```

發佈是編譯驗證，不代表程式已執行或已完成螢幕效果測試。

輸出為程式資料夾，移動或分享時請保留其中的所有檔案。若套件服務無法連線，且本機已安裝對應的 .NET 10 參考套件，可在上述指令加上 `-p:RestoreSources=D:\git\CatVideo`，僅使用本機來源建置。

## 參與專案

功能建議與問題回報請使用 [Issues](https://github.com/LeeWeiCheng/CatScreenConverter/issues)。提交變更前請閱讀 [CONTRIBUTING.md](CONTRIBUTING.md)，安全問題請依 [SECURITY.md](SECURITY.md) 回報。程式如何處理資料，請見 [PRIVACY.md](PRIVACY.md)。

## 參考

- [Windows 全桌面色彩轉換](https://learn.microsoft.com/en-us/windows/win32/api/magnification/nf-magnification-magsetfullscreencoloreffect)
- [WPF Viewbox 等比例縮放](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/controls/viewbox)
- [Magnification API 與 64 位元限制](https://learn.microsoft.com/en-us/windows/win32/winauto/magapi/magapi-intro)
- [家貓二色視覺行為研究](https://www.sciencedirect.com/science/article/pii/S0014483516303165)
