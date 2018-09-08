<%@ Page Title="About" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="About.aspx.cs" Inherits="Scrum.About" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container">
        <br />
        <h2><%: Title %></h2>
        <div class="panel panel-default">
            <div class="panel-body">
                <h3>Scrum tool</h3>
                <p>This is a tool to help in making SCRUM projects. There are various options to support multiple users collaborating in a project, and assigning tasks to specific users within the SCRUM lifecycle. In this system, there are three different types of users, which are Admins, Scrum Masters, and Participants (or developers).</p>
            </div>
        </div>
    </div>
</asp:Content>
