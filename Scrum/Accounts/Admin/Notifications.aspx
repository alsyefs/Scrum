<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="Notifications.aspx.cs" Inherits="Scrum.Accounts.Admin.Notifications" %>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container"> 
    <br />
        <h2><%: Title %></h2>
        <div class="panel panel-default">
            <div class="panel-body">
                <table>
                    <tr>
                        <td>
                            <asp:Label ID="lblNewUsers" runat="server" Text="Label" Width="100%"></asp:Label></td>
                        <td>
                            <asp:Button ID="btnNewUsers" runat="server" Text="Review Users" Width="100%" BackColor="Green" Font-Bold="True" Font-Size="Medium" Height="34px" OnClick="btnNewUsers_Click" /></td>
                    </tr>
                </table>
                <br />
                <%--General error message:--%>
                <asp:Label ID="lblError" runat="server" Text="Label" Visible="false" Font-Bold="true" ForeColor="Red" Font-Size="Medium"></asp:Label>
            </div>
        </div>
    </div>
</asp:Content>
