<%@ Page Title="Change your security questions and answers" Language="C#" MasterPageFile="~/Developer.Master" AutoEventWireup="true" CodeBehind="ChangeSecurityQuestions.aspx.cs" Inherits="Scrum.Accounts.Developer.CahngeSecurityQuestions" %>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
     <div class="container">
        <br />
        <h2><%: Title %></h2>
        <div class="panel panel-default">
            <div class="panel-body">
                <%--Update Panel start--%>
                <asp:UpdatePanel ID="upQuestions" UpdateMode="Conditional" runat="server">
                    <ContentTemplate>
                        <table>
                            <tr>
                                <%--First question & answer:--%>
                                <td><asp:Label ID="lblQ1" runat="server" Text="Question#1" Width="100%"></asp:Label></td>
                                <td><asp:DropDownList ID="drpQ1" runat="server" AutoPostBack="true" Width="100%" OnSelectedIndexChanged="drpQ1_SelectedIndexChanged"></asp:DropDownList></td>
                                <td><asp:Label ID="lblQ1Error" runat="server" Text="Error" ForeColor="Red" Visible="False" Width="100%"></asp:Label></td>
                            </tr>
                            <tr>
                                <td><asp:Label ID="lblA1" runat="server" Text="Answer#1" Width="100%"></asp:Label></td>
                                <td><asp:TextBox ID="txtA1" runat="server" Width="100%"></asp:TextBox></td>
                                <td><asp:Label ID="lblA1Error" runat="server" Text="Error" ForeColor="Red" Visible="False" Width="100%"></asp:Label></td>
                            </tr>
                            <tr>
                                <%--Second Q&A:--%>
                                <td><asp:Label ID="lblQ2" runat="server" Text="Question#2" Width="100%"></asp:Label></td>
                                <td><asp:DropDownList ID="drpQ2" runat="server" AutoPostBack="true" Width="100%" OnSelectedIndexChanged="drpQ2_SelectedIndexChanged"></asp:DropDownList></td>
                                <td><asp:Label ID="lblQ2Error" runat="server" Text="Error" ForeColor="Red" Visible="False" Width="100%"></asp:Label></td>
                            </tr>
                            <tr>
                                <td><asp:Label ID="lblA2" runat="server" Text="Answer#2" Width="100%"></asp:Label></td>
                                <td><asp:TextBox ID="txtA2" runat="server" Width="100%"></asp:TextBox></td>
                                <td><asp:Label ID="lblA2Error" runat="server" Text="Error" ForeColor="Red" Visible="False" Width="100%"></asp:Label></td>
                            </tr>
                            <tr>
                                <%--Third Q&A:--%>
                                <td><asp:Label ID="lblQ3" runat="server" Text="Question#3" Width="100%"></asp:Label></td>
                                <td><asp:DropDownList ID="drpQ3" runat="server" AutoPostBack="true" Width="100%" OnSelectedIndexChanged="drpQ3_SelectedIndexChanged"></asp:DropDownList></td>
                                <td><asp:Label ID="lblQ3Error" runat="server" Text="Error" ForeColor="Red" Visible="False" Width="100%"></asp:Label></td>
                            </tr>
                            <tr>
                                <td><asp:Label ID="lblA3" runat="server" Text="Answer#3" Width="100%"></asp:Label></td>
                                <td><asp:TextBox ID="txtA3" runat="server" Width="100%"></asp:TextBox></td>
                                <td><asp:Label ID="lblA3Error" runat="server" Text="Error" ForeColor="Red" Visible="False" Width="100%"></asp:Label></td>
                            </tr>
                            <tr>
                                <td><asp:Button ID="btnSubmit" runat="server" Text="Submit" BackColor="Green" Font-Bold="True" Font-Size="Medium" Width="100%" OnClick="btnSubmit_Click" /></td>
                                <td><asp:Button ID="btnClearAll" runat="server" Text="Clear fields" BackColor="Yellow" Font-Bold="True" Font-Size="Medium" Width="100%" OnClick="btnClearAll_Click" /></td>
                                <td><asp:Button ID="btnCancel" runat="server" Text="Cancel" BackColor="Red" Font-Bold="True" Font-Size="Medium" Width="100" OnClick="btnCancel_Click" /></td>
                            </tr>
                        </table>
                        <br />
                        <asp:Label ID="lblSuccess" runat="server" Text="Error" ForeColor="green" Visible="False" Width="100%"></asp:Label>
                    </ContentTemplate>
                    <Triggers>
                        <asp:AsyncPostBackTrigger ControlID="drpQ1" EventName="SelectedIndexChanged" />
                        <asp:AsyncPostBackTrigger ControlID="drpQ2" EventName="SelectedIndexChanged" />
                        <asp:AsyncPostBackTrigger ControlID="drpQ3" EventName="SelectedIndexChanged" />
                        <asp:AsyncPostBackTrigger ControlID="btnSubmit" EventName="Click" />
                        <asp:AsyncPostBackTrigger ControlID="btnClearAll" EventName="Click" />
                        <asp:AsyncPostBackTrigger ControlID="btnCancel" EventName="Click" />
                    </Triggers>
                </asp:UpdatePanel>
            </div>
        </div>
    </div>
</asp:Content>
