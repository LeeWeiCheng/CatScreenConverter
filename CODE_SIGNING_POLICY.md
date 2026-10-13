# Code signing policy

## 目前狀態

Cat螢幕轉換器由 QxQ 維護，原始碼採 MIT 授權。目前公開的 1.0.0 安裝檔尚未簽署；專案正在準備 SignPath Foundation 免費開源簽署申請，尚未取得服務核准或簽署憑證。

如果申請核准並開始使用該服務，簽署版本將依要求標示：

> Free code signing provided by [SignPath.io](https://about.signpath.io/), certificate by [SignPath Foundation](https://signpath.org/).

上述文字描述預計採用的服務，不表示目前檔案已簽署。憑證上的發行者將是 SignPath Foundation；產品品牌維持 QxQ。

## 維護與核准角色

| 角色 | 成員 |
|---|---|
| Authors / Committers | [LeeWeiCheng（QxQ）](https://github.com/LeeWeiCheng) |
| Reviewers | [LeeWeiCheng（QxQ）](https://github.com/LeeWeiCheng) |
| Signing Approvers | [LeeWeiCheng（QxQ）](https://github.com/LeeWeiCheng) |

外部提交由維護者審查後合併。啟用 SignPath 前，所有持有原始碼或簽署權限的成員須使用多因素驗證；本文件不聲稱帳號設定已經完成。

## 建置與簽署來源

- 原始碼來源：[LeeWeiCheng/CatScreenConverter](https://github.com/LeeWeiCheng/CatScreenConverter)。
- GitHub-hosted Windows runner 透過 [build.yml](.github/workflows/build.yml) 編譯；不使用自行託管的 runner 提供開源簽署產物。
- 建置產物先上傳為 GitHub Actions artifact，保留提交 SHA、版本、工具版本與建置網址，以供核對來源。來源紀錄不是數位簽章，也不取代 SignPath 的來源驗證。
- 未簽署的 CI 產物與正式 Release 分別標示；目前的 workflow 不提交簽署請求，也不自動發佈正式版本。
- 正式簽署須由 Approver 逐次人工核准。完成簽章與時間戳驗證、重新計算校驗碼及必要的安裝測試後，才公開簽署版本。
- 僅簽署本專案自行維護的程式與安裝產物；第三方執行元件保留原有授權與簽章，不使用本專案名義替其重新簽署。

## 隱私

詳見 [PRIVACY.md](PRIVACY.md)。本程式不會向其他網路系統傳輸資訊；SignPath 的建置產物提交僅發生於維護者的簽署流程。

## 申請與接入

1. 維護者閱讀 [SignPath Foundation 條件](https://signpath.org/terms.html)，確認授權、維護、專案信譽及帳號多因素驗證要求。
2. 透過 [官方申請入口](https://signpath.org/apply.html) 提供儲存庫、MIT 授權、功能說明、下載頁、維護者及本政策。
3. 核准後，依 [GitHub 整合文件](https://docs.signpath.io/trusted-build-systems/github) 建立 SignPath 專案、GitHub trusted build system、artifact configuration 及人工核准的簽署政策。
4. 維護者自行設定 SignPath 帳號、GitHub App 權限與機密；API token 不放入原始碼或文件。
5. 收到實際 organization ID、project slug 與 signing policy 後再接入簽署 workflow。Inno Setup 安裝／解除安裝程式的簽署方式須與核准的 artifact configuration 一起確認。

本文件不保證申請核准、任何憑證取得時間，或 SmartScreen 立即停止提醒。
