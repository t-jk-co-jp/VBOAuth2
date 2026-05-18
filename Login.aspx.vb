Imports System.Web.UI

Namespace OAuth2Sample

    ''' <summary>
    ''' ログインページ（Web Forms）
    ''' </summary>
    Public Class Login
        Inherits Page

        Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
            ' すでにログイン済みならトップへ
            If Session("UserEmail") IsNot Nothing Then
                Response.Redirect("~/Default.aspx")
            End If
        End Sub

        ''' <summary>
        ''' Google ログインボタン押下 → 認可エンドポイントへリダイレクト
        ''' </summary>
        Protected Sub btnGoogleLogin_Click(sender As Object, e As EventArgs)
            Try
                Dim oauthClient = OAuth2Client.CreateGoogleClient()
                Dim authUrl = oauthClient.BuildAuthorizationUrl()
                Response.Redirect(authUrl)
            Catch ex As Exception
                lblMessage.Text = "エラーが発生しました: " & Server.HtmlEncode(ex.Message)
            End Try
        End Sub

    End Class

End Namespace
