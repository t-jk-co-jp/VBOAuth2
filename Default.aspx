<%@ Page Title="Home Page" Language="VB" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.vb" Inherits="VBOAuth2._Default" Async="true" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <main>
        <div class="profile">
            <asp:Image ID="imgProfile" runat="server" Visible="false"
                AlternateText="プロフィール画像" />
            <div class="info">
                <asp:Label ID="lblWelcome" runat="server" />
                <p><asp:Label ID="lblEmail" runat="server" /></p>
            </div>
        </div>

        <div class="actions">
            <asp:Button ID="btnRefreshToken" runat="server" Text="トークンを更新"
                CssClass="btn btn-primary"
                OnClick="btnRefreshToken_Click" />
            <asp:Button ID="btnLogout" runat="server" Text="ログアウト"
                CssClass="btn btn-secondary"
                OnClick="btnLogout_Click" />
        </div>

        <asp:Label ID="lblMessage" runat="server" CssClass="message" />
    </main>

</asp:Content>
