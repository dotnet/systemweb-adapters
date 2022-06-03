<%@ Page Title="Login" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="FormsAuth.Login" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <div>
        <h1>Login</h1>
        <p>Username:<asp:TextBox ID="UserName" runat="server"></asp:TextBox></p>
        <p>Password:<asp:TextBox ID="Password" runat="server" TextMode="Password"></asp:TextBox></p>
        <p><asp:CheckBox ID="RememberMe" runat="server" Text="Remember Me" /></p>
        <p><asp:Button ID="LoginButton" runat="server" Text="Login" OnClick="LoginButton_Click" /></p>
        <p><asp:Label ID="InvalidCredentialsMessage" runat="server" ForeColor="Red" Text="Your username or password is invalid. Please try again." Visible="False"></asp:Label></p>
    </div>
</asp:Content>