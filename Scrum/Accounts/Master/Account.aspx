<%@ Page Title="Account Settings" Language="C#" MasterPageFile="~/Master.Master" AutoEventWireup="true" CodeBehind="Account.aspx.cs" Inherits="Scrum.Accounts.Master.Account" %>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <%--Body start--%>
    <div class="container">
        <br />
        <h2><%: Title %></h2>
        <div class="panel panel-default">
            <div class="panel-body">
                <%--Content start--%>
                <table>
                    <tr>
                        <td><asp:Button ID="btnChangePassword" runat="server" Text="Change Password" Width="100%" BackColor="Green" Font-Bold="True" Font-Size="Medium" Height="34px" OnClick="btnChangePassword_Click" /></td>
                    </tr>
                    <tr>
                        <td><asp:Button ID="btnChangeSecurityQuestions" runat="server" Text="Change Security Questions" Width="100%" BackColor="Green" Font-Bold="True" Font-Size="Medium" Height="34px" OnClick="btnChangeSecurityQuestions_Click" /></td>
                    </tr>
                </table>
                <%--General error message:--%>
                <asp:Label ID="lblError" runat="server" Text="Label" Visible="false" Font-Bold="true" ForeColor="Red" Font-Size="Medium"></asp:Label>
                <%--Content end--%>
            </div>
        </div>
    </div>
    <%--Body end--%>
</asp:Content>
