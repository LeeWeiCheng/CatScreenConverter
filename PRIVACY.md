# 隱私與桌面色彩

Cat螢幕轉換器在本機使用 Windows Magnification API 讀取、套用及還原全桌面色彩矩陣。程式本身不擷取螢幕影像，不讀取影片內容，不收集個人資料，也不包含遙測、廣告、網路傳輸或自動更新功能。

This program will not transfer any information to other networked systems unless specifically requested by the user or the person installing or operating it.

啟動時預設為「正常」，由使用者按「貓」後才套用桌面色彩轉換。可按「正常」或 `Ctrl + Alt + N` 還原；正常關閉時也會嘗試還原。若強制終止或斷電，程式可能無法執行還原，必要時可重新登入 Windows。

安裝程式會在目前使用者的程式資料夾寫入程式、加入解除安裝登錄資訊、建立開始功能表捷徑，並可依選擇建立桌面捷徑。Inno Setup 會產生本機安裝紀錄；本程式不會上傳這些紀錄。解除安裝會移除安裝內容與建立的捷徑。

GitHub 提供原始碼、議題、建置與下載服務，其網站行為適用 [GitHub 隱私聲明](https://docs.github.com/en/site-policy/privacy-policies/github-general-privacy-statement)。
