<%@ Page Title="" Language="C#" MasterPageFile="~/Developer.Master" AutoEventWireup="true" CodeBehind="Home.aspx.cs" Inherits="Scrum.Accounts.Developer.Home" %>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
     <%--body start:--%>
    <div class="container">
        <br />
        <h2><%: Title %></h2>
        <div class="panel panel-default">
            <div class="panel-body">
                <asp:Label ID="lblWelcome" runat="server" Text="Welcome to the Scrum Tool"  Font-Size="XX-Large" Font-Bold="true"></asp:Label>
            </div>
        </div>
    </div>
    <%--body end.--%>
</asp:Content>
