Imports System.Web.UI

Namespace OAuth2Sample

    ''' <summary>
    ''' OAuth2 コールバックページ
    ''' リダイレクト URI として登録する URL（例: https://yourdomain.com/Callback.aspx）
    ''' </summary>
    Public Class Callback
        Inherits Page

        Protected Async Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
            ' エラーチェック（プロバイダ側でユーザーが拒否した場合など）
            Dim errorParam = Request.QueryString("error")
            If Not String.IsNullOrEmpty(errorParam) Then
                Dim errorDesc = Request.QueryString("error_description")
                Response.Redirect("~/Login.aspx?error=" & Server.UrlEncode(errorDesc))
                Return
            End If

            ' 認可コードと state を取得
            Dim code As String = Request.QueryString("code")
            Dim state As String = Request.QueryString("state")

            If String.IsNullOrEmpty(code) Then
                Response.Redirect("~/Login.aspx?error=code_missing")
                Return
            End If

            Try
                Dim oauthClient = OAuth2Client.CreateGoogleClient()

                ' 1. 認可コード → アクセストークン交換
                Dim tokenResponse = Await oauthClient.ExchangeCodeForTokenAsync(code, state)

                ' 2. アクセストークン → ユーザー情報取得
                Dim userInfo = Await oauthClient.GetUserInfoAsync(tokenResponse.AccessToken)

                ' 3. セッションにユーザー情報を保存
                Session("UserEmail") = userInfo.Email
                Session("UserName") = userInfo.Name
                Session("UserPicture") = userInfo.Picture
                Session("AccessToken") = tokenResponse.AccessToken

                ' リフレッシュトークンがある場合は保存（暗号化して DB 保存を推奨）
                If Not String.IsNullOrEmpty(tokenResponse.RefreshToken) Then
                    Session("RefreshToken") = tokenResponse.RefreshToken
                End If

                ' 4. ログイン後のページへリダイレクト
                Response.Redirect("~/Default.aspx")

            Catch ex As InvalidOperationException
                ' CSRF 検知
                Response.Redirect("~/Login.aspx?error=state_mismatch")
            Catch ex As Exception
                Response.Redirect("~/Login.aspx?error=" & Server.UrlEncode(ex.Message))
            End Try
        End Sub

    End Class

End Namespace
