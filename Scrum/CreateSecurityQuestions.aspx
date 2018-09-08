<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="CreateSecurityQuestions.aspx.cs" Inherits="Scrum.CreateSecurityQuestions" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
     <div class="container">
        <h2><%: Title %></h2>
        <div class="panel panel-default">
            <div class="panel-body">

                <%--Update Panel start--%>
                <asp:UpdatePanel ID="upQuestions" UpdateMode="Conditional" runat="server">
                    <ContentTemplate>
                        <%--First question & answer:--%>
                        <asp:Label ID="lblQ1" runat="server" Text="Question#1"></asp:Label>
                        <asp:DropDownList ID="drpQ1" runat="server" AutoPostBack="true" OnSelectedIndexChanged="drpQ1_SelectedIndexChanged"></asp:DropDownList>
                        <asp:Label ID="lblQ1Error" runat="server" Text="Error" ForeColor="Red" Visible="False"></asp:Label>
                        <br />
                        &nbsp;&nbsp;
                <asp:Label ID="lblA1" runat="server" Text="Answer#1"></asp:Label>
                        <asp:TextBox ID="txtA1" runat="server"></asp:TextBox>
                        <asp:Label ID="lblA1Error" runat="server" Text="Error" ForeColor="Red" Visible="False"></asp:Label>
                        <br />
                        <br />
                        <%--Second Q&A:--%>
                        <asp:Label ID="lblQ2" runat="server" Text="Question#2"></asp:Label>
                        <asp:DropDownList ID="drpQ2" runat="server" AutoPostBack="true" OnSelectedIndexChanged="drpQ2_SelectedIndexChanged"></asp:DropDownList>
                        <asp:Label ID="lblQ2Error" runat="server" Text="Error" ForeColor="Red" Visible="False"></asp:Label>
                        <br />
                        &nbsp;&nbsp;
                <asp:Label ID="lblA2" runat="server" Text="Answer#2"></asp:Label>
                        <asp:TextBox ID="txtA2" runat="server"></asp:TextBox>
                        <asp:Label ID="lblA2Error" runat="server" Text="Error" ForeColor="Red" Visible="False"></asp:Label>
                        <br />
                        <br />
                        <%--Third Q&A:--%>
                        <asp:Label ID="lblQ3" runat="server" Text="Question#3"></asp:Label>
                        <asp:DropDownList ID="drpQ3" runat="server" AutoPostBack="true" OnSelectedIndexChanged="drpQ3_SelectedIndexChanged"></asp:DropDownList>
                        <asp:Label ID="lblQ3Error" runat="server" Text="Error" ForeColor="Red" Visible="False"></asp:Label>
                        <br />
                        &nbsp;&nbsp;
                <asp:Label ID="lblA3" runat="server" Text="Answer#3"></asp:Label>
                        <asp:TextBox ID="txtA3" runat="server"></asp:TextBox>
                        <asp:Label ID="lblA3Error" runat="server" Text="Error" ForeColor="Red" Visible="False"></asp:Label>
                    </ContentTemplate>
                    <Triggers>
                        <asp:AsyncPostBackTrigger ControlID="drpQ1" EventName="SelectedIndexChanged" />
                        <asp:AsyncPostBackTrigger ControlID="drpQ2" EventName="SelectedIndexChanged" />
                        <asp:AsyncPostBackTrigger ControlID="drpQ3" EventName="SelectedIndexChanged" />
                    </Triggers>
                </asp:UpdatePanel>
                <%--Submit--%>
                <br />
                <br />
                <br />
                <asp:Button ID="btnSubmit" runat="server" Text="Submit" BackColor="Green" Font-Bold="True" Font-Size="Medium" Height="34px" Width="140px" OnClick="btnSubmit_Click" />
                <%--Clear all fields--%>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                <asp:Button ID="btnClearAll" runat="server" Text="Clear fields" BackColor="Yellow" Font-Bold="True" Font-Size="Medium" Height="34px" Width="140px" OnClick="btnClearAll_Click" />
                <%--Cancel--%>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                <asp:Button ID="btnCancel" runat="server" Text="Cancel" BackColor="Red" Font-Bold="True" Font-Size="Medium" Height="34px" Width="140px" OnClick="btnCancel_Click" />
            </div>
        </div>
    </div>
</asp:Content>
