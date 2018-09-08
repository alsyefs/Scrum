<%@ Page Title="Register" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Register.aspx.cs" Inherits="Scrum.Register" %>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container">
        <br />
        <h2>Please fill the registration form</h2>
        <div class="panel panel-default">
            <div class="panel-body">
                <asp:UpdatePanel ID="upPatientId" UpdateMode="Conditional" runat="server">
                    <ContentTemplate>
                        <table class="tableEdit">
                            <tr>
                                <td>
                                    <asp:Label ID="lblFirstname" runat="server" Text="First name" Font-Size="Medium" Width="100%"></asp:Label></td>
                                <td>
                                    <asp:TextBox ID="txtFirstname" runat="server" Font-Size="Medium" Width="100%"></asp:TextBox></td>
                                <td>
                                    <asp:Label ID="lblFirstnameError" runat="server" Text="Firstname error" ForeColor="Red" Visible="False" Font-Size="Medium" Width="100%"></asp:Label></td>
                            </tr>
                            <tr>
                                <td>
                                    <asp:Label ID="lblLastname" runat="server" Text="Last name" Font-Size="Medium" Width="100%"></asp:Label></td>
                                <td>
                                    <asp:TextBox ID="txtLastname" runat="server" Font-Size="Medium" Width="100%"></asp:TextBox></td>
                                <td>
                                    <asp:Label ID="lblLastnameError" runat="server" Text="Lastname error" ForeColor="Red" Visible="False" Font-Size="Medium" Width="100%"></asp:Label></td>
                            </tr>
                            <tr>
                                <td>
                                    <asp:Label ID="lblEmail" runat="server" Text="Email" Font-Size="Medium" Width="100%"></asp:Label></td>
                                <td>
                                    <asp:TextBox ID="txtEmail" runat="server" Font-Size="Medium" Width="100%"></asp:TextBox></td>
                                <td>
                                    <asp:Label ID="lblEmailError" runat="server" Text="Email error" ForeColor="Red" Visible="False" Font-Size="Medium" Width="100%"></asp:Label></td>
                            </tr>
                            
                            <tr>
                                <td>
                                    <asp:Label ID="lblPhone" runat="server" Text="Phone#" Font-Size="Medium" Width="100%"></asp:Label></td>
                                <td>
                                    <asp:TextBox ID="txtPhone" runat="server" Font-Size="Medium" Width="100%"></asp:TextBox></td>
                                <td>
                                    <asp:Label ID="lblPhoneError" runat="server" Text="Phone error" ForeColor="Red" Visible="False" Font-Size="Medium" Width="100%"></asp:Label></td>
                            </tr>
                            <tr>
                                <td>
                                    <asp:Label ID="lblRole" runat="server" Text="Role" Font-Size="Medium" Width="100%"></asp:Label></td>
                                <td>
                                    <asp:DropDownList ID="drpRole" runat="server" Font-Size="Medium" AutoPostBack="true" Width="100%" OnSelectedIndexChanged="drpRole_SelectedIndexChanged">
                                        <asp:ListItem>Select a role</asp:ListItem>
                                        <asp:ListItem>Admin</asp:ListItem>
                                        <asp:ListItem>Master</asp:ListItem>
                                        <asp:ListItem>Developer</asp:ListItem>
                                    </asp:DropDownList>
                                </td>
                                <td>
                                    <asp:Label ID="lblRoleError" runat="server" Text="Role error" ForeColor="Red" Visible="False" Font-Size="Medium" Width="100%"></asp:Label></td>
                            </tr>
                            <tr>
                                <td>
                                    <asp:Button ID="btnSubmit" runat="server" Text="Submit" BackColor="Green" Font-Bold="True" Font-Size="Medium" Height="34px" Width="140px" OnClick="btnSubmit_Click" /></td>
                                <td>
                                    <asp:Button ID="btnCancel" runat="server" Text="Go back" BackColor="red" Font-Bold="True" Font-Size="Medium" Height="34px" Width="140px" OnClick="btnCancel_Click" /></td>

                            </tr>
                            <tr>
                                <td></td>
                                <td><asp:Label ID="lblResult" runat="server" Text="Result" Visible="False" Font-Size="Medium"></asp:Label></td>
                            </tr>
                        </table>
                    </ContentTemplate>
                    <Triggers>
                        <asp:AsyncPostBackTrigger ControlID="drpRole" EventName="SelectedIndexChanged" />
                    </Triggers>
                </asp:UpdatePanel>
                <br />

            </div>
        </div>
    </div>
</asp:Content>
