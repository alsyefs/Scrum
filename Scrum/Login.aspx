<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="Scrum.Login" %>

<asp:Content ID="Content1" ContentPlaceHolderID="default" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container">
        <h2>Login</h2>
        <div class="panel panel-default">
            <div class="panel-body">
                <br />
                <asp:Label ID="lblUsername" runat="server" Text="Username:"></asp:Label>
                &nbsp;<asp:TextBox ID="txtUsername" runat="server"></asp:TextBox>
                &nbsp;
    <asp:Label ID="lblUsernameError" runat="server" ForeColor="Red" Text="Label" Visible="False"></asp:Label>
                <br />
                <br />
                <asp:Label ID="lblPassword" runat="server" Text="Password:"></asp:Label>
                &nbsp;<asp:TextBox ID="txtPassword" runat="server" type="password"></asp:TextBox>
                &nbsp;
    <asp:Label ID="lblPasswordError" runat="server" ForeColor="Red" Text="Label" Visible="False"></asp:Label>
                <br />
                <br />

                <asp:Button ID="btnLogin" runat="server" Text="Login" OnClick="btnLogin_Click" BackColor="Green" Font-Bold="True" Font-Size="Medium" Height="34px" Width="140px" />
                &nbsp;
    <asp:Label ID="lblError" runat="server" ForeColor="Red" Text="Label" Visible="False"></asp:Label>
                <%--Cancel button--%>    
    &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
    &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
    
    <asp:Button ID="btnCancel" runat="server" Text="Go back" BackColor="red" Font-Bold="True" Font-Size="Medium" Height="34px" Width="140px" OnClick="btnCancel_Click" />

            </div>
        </div>
    </div>
</asp:Content>
