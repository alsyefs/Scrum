<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="SecurityQuestions.aspx.cs" Inherits="Scrum.SecurityQuestions" %>

<asp:Content ID="Content1" ContentPlaceHolderID="default" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container">
        <h2>Please answer the security question</h2>
        <div class="panel panel-default">
            <div class="panel-body">
                <%--Question--%>
                <asp:Label ID="lblQ" runat="server" Text="Question"></asp:Label>
                <%--Answer--%>
                <br />
                <asp:TextBox ID="txtA" runat="server"></asp:TextBox>
                &nbsp;
                <asp:Label ID="lblAError" runat="server" Text="Error" ForeColor="Red" Visible="False"></asp:Label>
                <%--Submit--%>
                <br />
                <br />
                <asp:Button ID="btnSubmit" runat="server" Text="Submit" BackColor="Green" Font-Bold="True" Font-Size="Medium" Height="34px" Width="140px" OnClick="btnSubmit_Click" />
                <%--Cancel button--%>    
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                <asp:Button ID="btnCancel" runat="server" Text="Go back" BackColor="red" Font-Bold="True" Font-Size="Medium" Height="34px" Width="140px" OnClick="btnCancel_Click" />
                <br />
                <asp:Label ID="lblError" runat="server" ForeColor="Red" Text="Label" Visible="False"></asp:Label>
            </div>
        </div>
    </div>
</asp:Content>
