# 一起讓貓咪的值班畫面更好分辨

歡迎透過 [Issues](https://github.com/LeeWeiCheng/CatScreenConverter/issues) 回報問題或提出想法，也歡迎提交 Pull Request。貢獻的原創程式碼、文件與插畫採用本專案的 MIT 授權；加入第三方內容時，請註明來源與授權。

## 回報問題

請提供 Windows 版本、程式版本、螢幕縮放比例、是否使用 HDR／多螢幕、重現步驟及預期結果。若附上截圖，請先移除個人資訊。全桌面色彩效果不一定會出現在截圖中，請另外描述肉眼觀察到的變化。

疑似安全問題請依 [SECURITY.md](SECURITY.md) 回報。

## 開發與編譯

需要 Windows x64、.NET SDK（版本見 `global.json`）與 Inno Setup 7.1.0 x64。從專案根目錄執行：

```powershell
dotnet build CatVideo.csproj -c Release
.\packaging\Build-Release.ps1 -InnoCompiler 'C:\path\to\Inno Setup 7\ISCC.exe'
```

建置腳本會產生安裝檔、授權文件、來源紀錄及 SHA-256 校驗碼。GitHub Actions 的 `Build Windows installer` 只負責編譯與打包，不啟動程式、不修改桌面色彩，也不自動發佈 Release。

## 提交變更

- 說明使用者遇到的問題、修改後的行為，以及完成哪些驗證。
- 色彩、快捷鍵及關閉流程的變更，請注意原本色彩矩陣的保存與還原。
- 視窗排版變更請確認縮小、放大及不同 DPI 下的顯示。
- `bin/`、`obj/`、`artifacts/` 與私人憑證不加入版本控制。

外部貢獻須由維護者審查後合併。編譯通過與實際安裝／桌面效果測試應分別記錄；進行螢幕效果測試前，請讓操作電腦的人知道即將改變全桌面色彩。
