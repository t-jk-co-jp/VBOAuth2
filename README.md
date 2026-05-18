# ASP.NET VB.NET OAuth2 クライアント認証 サンプル

Google OAuth2 Authorization Code Flow を VB.NET / ASP.NET Web Forms で実装したサンプルです。

---

## ファイル構成

```
OAuth2Sample/
├── OAuth2Client.vb       # OAuth2 汎用クライアントクラス（メインロジック）
├── Login.aspx            # ログイン画面（HTML）
├── Login.aspx.vb         # ログイン画面 コードビハインド
├── Callback.aspx.vb      # OAuth2 コールバック処理
├── Default.aspx.vb       # ログイン後トップページ（保護ページ例）
└── Web.config            # 設定ファイル
```

---

## セットアップ手順

### 1. Google Cloud Console で設定

1. https://console.cloud.google.com/ にアクセス
2. 「APIとサービス」→「認証情報」→「認証情報を作成」→「OAuth 2.0 クライアント ID」
3. アプリケーションの種類: **ウェブ アプリケーション**
4. 承認済みのリダイレクト URI に追加:
   ```
   https://localhost:44300/Callback.aspx
   ```
5. **クライアント ID** と **クライアント シークレット** を控える

### 2. Web.config を編集

```xml
<add key="Google_ClientId"     value="取得したクライアントID" />
<add key="Google_ClientSecret" value="取得したクライアントシークレット" />
<add key="Google_RedirectUri"  value="https://localhost:44300/Callback.aspx" />
```

### 3. NuGet パッケージのインストール

```
Install-Package Newtonsoft.Json
```

### 4. Callback.aspx の作成

`Callback.aspx.vb` に対応する ASPX ファイルを作成（コードビハインドのみでも可）:

```aspx
<%@ Page Language="VB" AutoEventWireup="false"
         CodeBehind="Callback.aspx.vb"
         Inherits="OAuth2Sample.Callback" Async="true" %>
```

---

## フロー説明

```
ユーザー
  │
  ▼
[Login.aspx] ─── Google ログインボタン押下
  │
  ▼
[Google 認可エンドポイント]
  │  ユーザーがログイン＆許可
  ▼
[Callback.aspx] ─── 認可コード受け取り
  │                  ↓
  │              state 検証（CSRF 対策）
  │                  ↓
  │              コード → アクセストークン交換
  │                  ↓
  │              ユーザー情報取得
  │                  ↓
  │              セッション保存
  ▼
[Default.aspx] ─── ログイン完了（保護ページ）
```

---

## セキュリティ対策

| 対策 | 実装箇所 |
|------|----------|
| CSRF 対策 | `state` パラメータ生成・検証 (`OAuth2Client.vb`) |
| HTTPS 強制 | `Web.config` の URL Rewrite |
| HttpOnly Cookie | `Web.config` の `httpCookies` 設定 |
| SameSite Cookie | `Web.config` の `sameSite="Strict"` |
| 入力値エスケープ | `Server.HtmlEncode()` / `Server.UrlEncode()` |

---

## 本番環境での推奨事項

- `ClientSecret` は Web.config ではなく **環境変数** や **Azure Key Vault** に保存
- セッションは **SQL Server** または **Redis** で管理（`InProc` は複数サーバー非対応）
- アクセストークンは **暗号化して DB 保存**（セッションへの保存は最小限に）
- リフレッシュトークンは **安全なストレージ（DB + 暗号化）** に保存
