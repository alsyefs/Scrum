<%@ Page Title="Change Password" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="ChangePassword.aspx.cs" Inherits="Scrum.Accounts.Admin.ChangePassword" %>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container">
        <br />
    <h2><%: Title %></h2>
        <div class="panel panel-default">
        <div class="panel-body">
            <table>
                <tr>
                    <%--Type the new password:--%>
                    <td><asp:Label ID="lblP1" runat="server" Text="Type new password" Font-Size="Medium" Width="100%"></asp:Label></td>
                    <td><asp:TextBox ID="txtP1" runat="server" type="password" Font-Size="Medium" Width="100%"></asp:TextBox></td>
                    <td><asp:Label ID="lblP1Error" runat="server" Text="P1 error" ForeColor="Red" Visible="False" Font-Size="Medium" Width="100%"></asp:Label></td>
                </tr>
                <tr>
                    <%--Repeat the new password:--%>
                    <td><asp:Label ID="lblP2" runat="server" Text="Repeat new password" Font-Size="Medium" Width="100%"></asp:Label></td>
                    <td><asp:TextBox ID="txtP2" runat="server" type="password" Font-Size="Medium" Width="100%"></asp:TextBox></td>
                    <td><asp:Label ID="lblP2Error" runat="server" Text="P1 error" ForeColor="Red" Visible="False" Font-Size="Medium" Width="100%"></asp:Label></td>
                </tr>
            </table>
            
            <br />
            <br />
            <%--Password rules:--%>
            <pre style="font-family: Consolas; font-size:large; text-align:left; color: #dadada; background: #e9e894;">
                <span style="color:Black;"> The&nbsp;password&nbsp;must&nbsp;match&nbsp;the&nbsp;following password&nbsp;requirements:</span>
                <span style="color:Black;"> 1.&nbsp;At&nbsp;least&nbsp;eight&nbsp;characters&nbsp;long.</span>
                <span style="color:Black;"> 2.&nbsp;Contains&nbsp;at&nbsp;least&nbsp;one&nbsp;upper-case&nbsp;letter.</span>
                <span style="color:Black"> 3.&nbsp;Contains&nbsp;at&nbsp;least&nbsp;one&nbsp;lower-case&nbsp;letter.</span>
                <span style="color:Black"> 4.&nbsp;Contains&nbsp;at&nbsp;least&nbsp;one&nbsp;digit&nbsp;(0-9).</span>
                <span style="color:Black;"> 5.&nbsp;Contains&nbsp;one&nbsp;of&nbsp;the&nbsp;following&nbsp;special&nbsp;characters&nbsp;;&nbsp;,&nbsp;.&nbsp;!&nbsp;@&nbsp;#&nbsp;$&nbsp;%&nbsp;^&nbsp;&amp;&nbsp;*&nbsp;(&nbsp;)</span>
            </pre>
            
            <table>
               <tr>
                   <%--Submit--%>
               <td><asp:Button ID="btnSubmit" runat="server" Text="Submit" BackColor="Green" Font-Bold="True" Font-Size="Medium" Width="100%" OnClick="btnSubmit_Click"/></td>
                <td> </td>
                <%--Cancel button--%>
                <td><asp:Button ID="btnCancel" runat="server" Text="Go back" BackColor="red" Font-Bold="True" Font-Size="Medium" Width="100%" OnClick="btnCancel_Click"  /></td>
                   </tr>
            </table>
            <asp:Label ID="lblError" runat="server" ForeColor="Red" Text="Label" Visible="False"></asp:Label>
            <asp:Label ID="lblSuccess" runat="server" ForeColor="green" Text="Label" Visible="False"></asp:Label>

            </div>
            </div>
        </div>
</asp:Content>
