Imports System
Imports System.Net.Http
Imports System.Net.Http.Headers
Imports System.Text
Imports System.Threading.Tasks
Imports System.Web
Imports Newtonsoft.Json

''' <summary>
''' OAuth2 クライアント認証ヘルパークラス
''' 汎用的な OAuth2 Authorization Code Flow をサポート
''' </summary>
Public Class OAuth2Client

    ' ---- 設定値（Web.config や appsettings から読み込むことを推奨） ----
    Public Property ClientId As String
    Public Property ClientSecret As String
    Public Property RedirectUri As String
    Public Property AuthorizationEndpoint As String
    Public Property TokenEndpoint As String
    Public Property UserInfoEndpoint As String
    Public Property Scope As String

    ' ---- Google OAuth2 設定例 ----
    Public Shared Function CreateGoogleClient() As OAuth2Client
        Return New OAuth2Client With {
            .ClientId = System.Configuration.ConfigurationManager.AppSettings("Google_ClientId"),
            .ClientSecret = System.Configuration.ConfigurationManager.AppSettings("Google_ClientSecret"),
            .RedirectUri = System.Configuration.ConfigurationManager.AppSettings("Google_RedirectUri"),
            .AuthorizationEndpoint = "https://accounts.google.com/o/oauth2/v2/auth",
            .TokenEndpoint = "https://oauth2.googleapis.com/token",
            .UserInfoEndpoint = "https://www.googleapis.com/oauth2/v3/userinfo",
            .Scope = "openid email profile"
        }
    End Function

    ''' <summary>
    ''' 認可リクエスト URL を生成する（CSRF 対策の state パラメータ付き）
    ''' </summary>
    Public Function BuildAuthorizationUrl() As String
        ' state パラメータ生成（CSRF 対策）
        Dim state As String = GenerateState()
        HttpContext.Current.Session("OAuth2_State") = state

        Dim queryParams As New StringBuilder()
        queryParams.Append("response_type=code")
        queryParams.Append("&client_id=" & Uri.EscapeDataString(ClientId))
        queryParams.Append("&redirect_uri=" & Uri.EscapeDataString(RedirectUri))
        queryParams.Append("&scope=" & Uri.EscapeDataString(Scope))
        queryParams.Append("&state=" & Uri.EscapeDataString(state))
        queryParams.Append("&access_type=offline")  ' リフレッシュトークン取得用

        Return AuthorizationEndpoint & "?" & queryParams.ToString()
    End Function

    ''' <summary>
    ''' 認可コードをアクセストークンに交換する
    ''' </summary>
    Public Async Function ExchangeCodeForTokenAsync(code As String, state As String) As Task(Of TokenResponse)
        ' state 検証（CSRF 対策）
        Dim savedState As String = TryCast(HttpContext.Current.Session("OAuth2_State"), String)
        If String.IsNullOrEmpty(savedState) OrElse savedState <> state Then
            Throw New InvalidOperationException("OAuth2 state パラメータが一致しません。CSRF 攻撃の可能性があります。")
        End If
        HttpContext.Current.Session.Remove("OAuth2_State")

        Using client As New HttpClient()
            Dim requestBody As New FormUrlEncodedContent(New Dictionary(Of String, String) From {
                {"grant_type", "authorization_code"},
                {"code", code},
                {"redirect_uri", RedirectUri},
                {"client_id", ClientId},
                {"client_secret", ClientSecret}
            })

            Dim response = Await client.PostAsync(TokenEndpoint, requestBody)
            Dim responseBody = Await response.Content.ReadAsStringAsync()

            If Not response.IsSuccessStatusCode Then
                Throw New HttpException(CInt(response.StatusCode),
                    "トークン取得エラー: " & responseBody)
            End If

            Return JsonConvert.DeserializeObject(Of TokenResponse)(responseBody)
        End Using
    End Function

    ''' <summary>
    ''' アクセストークンを使ってユーザー情報を取得する
    ''' </summary>
    Public Async Function GetUserInfoAsync(accessToken As String) As Task(Of UserInfo)
        Using client As New HttpClient()
            client.DefaultRequestHeaders.Authorization =
                New AuthenticationHeaderValue("Bearer", accessToken)

            Dim response = Await client.GetAsync(UserInfoEndpoint)
            Dim responseBody = Await response.Content.ReadAsStringAsync()

            If Not response.IsSuccessStatusCode Then
                Throw New HttpException(CInt(response.StatusCode),
                    "ユーザー情報取得エラー: " & responseBody)
            End If

            Return JsonConvert.DeserializeObject(Of UserInfo)(responseBody)
        End Using
    End Function

    ''' <summary>
    ''' リフレッシュトークンで新しいアクセストークンを取得する
    ''' </summary>
    Public Async Function RefreshAccessTokenAsync(refreshToken As String) As Task(Of TokenResponse)
        Using client As New HttpClient()
            Dim requestBody As New FormUrlEncodedContent(New Dictionary(Of String, String) From {
                {"grant_type", "refresh_token"},
                {"refresh_token", refreshToken},
                {"client_id", ClientId},
                {"client_secret", ClientSecret}
            })

            Dim response = Await client.PostAsync(TokenEndpoint, requestBody)
            Dim responseBody = Await response.Content.ReadAsStringAsync()

            If Not response.IsSuccessStatusCode Then
                Throw New HttpException(CInt(response.StatusCode),
                    "トークン更新エラー: " & responseBody)
            End If

            Return JsonConvert.DeserializeObject(Of TokenResponse)(responseBody)
        End Using
    End Function

    ' ---- プライベートメソッド ----

    Private Function GenerateState() As String
        Dim bytes(31) As Byte
        Using rng = System.Security.Cryptography.RandomNumberGenerator.Create()
            rng.GetBytes(bytes)
        End Using
        Return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").Replace("=", "")
    End Function

End Class

' ---- データモデル ----

Public Class TokenResponse
    <JsonProperty("access_token")>
    Public Property AccessToken As String

    <JsonProperty("token_type")>
    Public Property TokenType As String

    <JsonProperty("expires_in")>
    Public Property ExpiresIn As Integer

    <JsonProperty("refresh_token")>
    Public Property RefreshToken As String

    <JsonProperty("id_token")>
    Public Property IdToken As String

    <JsonProperty("scope")>
    Public Property Scope As String

    Public ReadOnly Property ExpiresAt As DateTime
        Get
            Return DateTime.UtcNow.AddSeconds(ExpiresIn)
        End Get
    End Property
End Class

Public Class UserInfo
    <JsonProperty("sub")>
    Public Property Sub_ As String

    <JsonProperty("email")>
    Public Property Email As String

    <JsonProperty("name")>
    Public Property Name As String

    <JsonProperty("picture")>
    Public Property Picture As String

    <JsonProperty("email_verified")>
    Public Property EmailVerified As Boolean
End Class
