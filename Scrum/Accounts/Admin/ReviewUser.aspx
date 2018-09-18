<%@ Page Title="Review New User" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="ReviewUser.aspx.cs" Inherits="Scrum.Accounts.Admin.ReviewUser" %>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
     <%--Body start--%>
    <div class="container">
        <br />
        <h2><%: Title %></h2>
        <div class="panel panel-default">
            <div class="panel-body">
                <%--Content start--%>
                <asp:Label ID="lblUserInformation" runat="server" Text="Label"></asp:Label>
                <table>
                    <tr>
                        <td><asp:Button ID="btnApprove" runat="server" Text="Approve" BackColor="Green" Font-Bold="True" Font-Size="Medium" Width="100%" OnClick="btnApprove_Click"/></td>
                        <td style="width:140px;"></td>
                        <td><asp:Button ID="btnDeny" runat="server" Text="Deny" BackColor="red" Font-Bold="True" Font-Size="Medium" Width="100%" OnClick="btnDeny_Click"/></td>
                        <td style="width:140px;"></td>
                        <td><asp:Button ID="btnCancel" runat="server" Text="Go Back" BackColor="yellow" Font-Bold="True" Font-Size="Medium" Width="100%" OnClick="btnCancel_Click" /></td>
                    </tr>
                </table>
                <%--General error message:--%>
                <asp:Label ID="lblMessage" runat="server" ForeColor="green" Text="Label" Visible="False"></asp:Label>
                <%--Content end--%>
            </div>
        </div>
    </div>
    <%--Body end--%>
</asp:Content>
