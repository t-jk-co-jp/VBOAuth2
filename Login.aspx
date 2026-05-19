<%@ Page Title="Login" Language="VB" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Login.aspx.vb" Inherits="VBOAuth2.Login" Async="true" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <main>
        <div class="actions">
            <asp:Button ID="btnGoogleLogin" runat="server" Text="Google ログイン"
                CssClass="btn btn-secondary"
                OnClick="btnGoogleLogin_Click" />
        </div>

        <asp:Label ID="lblMessage" runat="server" CssClass="message"
            Visible="false" />
    </main>

</asp:Content>
