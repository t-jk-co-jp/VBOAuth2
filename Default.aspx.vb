Partial Public Class _Default
    Inherits Page

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        ' ログインチェック
        If Session("UserEmail") Is Nothing Then
            Response.Redirect("~/Login.aspx")
            Return
        End If

        If Not IsPostBack Then
            lblWelcome.Text = "ようこそ、" & Server.HtmlEncode(CStr(Session("UserName"))) & " さん！"
            lblEmail.Text = "メールアドレス: " & Server.HtmlEncode(CStr(Session("UserEmail")))

            Dim picture = TryCast(Session("UserPicture"), String)
            If Not String.IsNullOrEmpty(picture) Then
                imgProfile.ImageUrl = picture
                imgProfile.Visible = True
            End If
        End If
    End Sub

    ''' <summary>
    ''' ログアウト処理
    ''' </summary>
    Protected Sub btnLogout_Click(sender As Object, e As EventArgs)
        Session.Clear()
        Session.Abandon()
        Response.Redirect("~/Login.aspx")
    End Sub

    ''' <summary>
    ''' トークン更新のサンプル
    ''' </summary>
    Protected Async Sub btnRefreshToken_Click(sender As Object, e As EventArgs)
        Dim refreshToken = TryCast(Session("RefreshToken"), String)
        If String.IsNullOrEmpty(refreshToken) Then
            lblMessage.Text = "リフレッシュトークンがありません。"
            Return
        End If

        Try
            Dim oauthClient = OAuth2Client.CreateGoogleClient()
            Dim newToken = Await oauthClient.RefreshAccessTokenAsync(refreshToken)
            Session("AccessToken") = newToken.AccessToken
            lblMessage.Text = "アクセストークンを更新しました。有効期限: " & newToken.ExpiresAt.ToLocalTime().ToString("yyyy/MM/dd HH:mm:ss")
        Catch ex As Exception
            lblMessage.Text = "トークン更新エラー: " & Server.HtmlEncode(ex.Message)
        End Try
    End Sub

End Class
