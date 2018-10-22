<%@ Page Title="Edit User Story" Language="C#" MasterPageFile="~/Master.Master" AutoEventWireup="true" CodeBehind="EditUserStory.aspx.cs" Inherits="Scrum.Accounts.Master.EditUserStory" %>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
     <%--Body start--%>
    <div class="container">
        <br />
        <h2><%: Title %></h2>
        <div class="panel panel-default">
            <div class="panel-body">
                <script>
                    function pleaseWait() {
                        $(".modal").show();
                        return true;
                    }
                </script>
                <asp:UpdatePanel ID="upAjax" UpdateMode="Conditional" runat="server">
                    <ContentTemplate>
                        <div id="modal" class="modal" style="background-color: rgba(64,64,64,0.5); width: 100%; height: 100%; z-index: 1000; display: none"></div>
                        <div id="wait" runat="server" class="modal" style="width: 200px; height: 20px; margin: 100px auto 0 auto; display: none; background-color: #fff; z-index: 1001; text-align: center;">PLEASE WAIT...</div>
                        <table>
                            <tr>
                                <tr>
                                    <td>
                                        <asp:Label ID="lblAsRole" runat="server" Text="As a (type of user)" Font-Size="Medium" Width="100%"></asp:Label></td>
                                    <td>
                                        <asp:ListBox ID="drpAsRole" runat="server" SelectionMode="Multiple" Font-Size="Medium" Width="100%" >
                                            <asp:ListItem>Select Role</asp:ListItem>
                                            <asp:ListItem>Admin</asp:ListItem>
                                            <asp:ListItem>Master</asp:ListItem>
                                            <asp:ListItem>Developer</asp:ListItem>
                                        </asp:ListBox>
                                    </td>
                                    <td>
                                        <asp:Label ID="lblAsRoleError" runat="server" Text="Label" Visible="false" ForeColor="Red" Font-Size="Medium" Font-Bold="true"></asp:Label></td>
                                </tr>
                                <tr>
                                    <td>
                                        <asp:Label ID="lblIWantTo" runat="server" Text="I want to (some goal)" Font-Size="Medium" Width="100%"></asp:Label></td>
                                    <td>
                                        <asp:TextBox ID="txtIWantTo" runat="server" Font-Size="Medium" Width="100%"></asp:TextBox></td>
                                    <td>
                                        <asp:Label ID="lblIWantToError" runat="server" Text="Label" Visible="false" ForeColor="Red" Font-Size="Medium" Font-Bold="true"></asp:Label></td>
                                </tr>
                                <tr>
                                    <td>
                                        <asp:Label ID="lblSoThat" runat="server" Text="So that (reason)" Font-Size="Medium" Width="100%"></asp:Label></td>
                                    <td>
                                        <asp:TextBox ID="txtSoThat" runat="server" Font-Size="Medium" Width="100%"></asp:TextBox></td>
                                    <td>
                                        <asp:Label ID="lblSoThatError" runat="server" Text="Label" Visible="false" ForeColor="Red" Font-Size="Medium" Font-Bold="true"></asp:Label></td>
                                </tr>
                            <tr>
                                    <td>
                                        <asp:Label ID="lblCurrentStatus" runat="server" Text="Current Status" Font-Size="Medium" Width="100%"></asp:Label></td>
                                    <td>
                                        <asp:DropDownList ID="drpCurrentStatus" runat="server" Font-Size="Medium" Width="100%">
                                            <asp:ListItem>Select a status</asp:ListItem>
                                            <asp:ListItem>Not started</asp:ListItem>
                                            <asp:ListItem>In progress</asp:ListItem>
                                            <asp:ListItem>Completed</asp:ListItem>
                                        </asp:DropDownList></td>
                                    <td>
                                        <asp:Label ID="lblCurrentStatusError" runat="server" Text="Label" Visible="false" ForeColor="Red" Font-Size="Medium" Font-Bold="true"></asp:Label></td>
                                </tr>
                            <tr>
                                <td>
                                    <asp:Label ID="lblDeveloperResponsible" runat="server" Text="Developers" Font-Size="Medium" Width="100%"></asp:Label></td>
                                <td>
                                    <asp:TextBox ID="txtDeveloperResponsible" runat="server" Font-Size="Medium" Width="100%" AutoPostBack="true" OnTextChanged="txtDeveloperResponsible_TextChanged"></asp:TextBox></td>
                                <td>
                                    <asp:Label ID="lblDeveloperResponsibleError" runat="server" Text="Label" Visible="false" ForeColor="Red" Font-Size="Medium" Font-Bold="true"></asp:Label></td>
                            </tr>
                            <tr>
                                <td>
                                    <asp:Label ID="lblSelectUser" runat="server" Text="Select developer" Width="100%"></asp:Label></td>
                                <td>
                                    <asp:ListBox ID="drpFindUser" runat="server" Width="100%" AutoPostBack="true" OnSelectedIndexChanged="drpFindUser_SelectedIndexChanged"></asp:ListBox>
                                    <asp:Button ID="btnAddUserToList" runat="server" Text="Add to list" Width="100px" BackColor="yellow" Font-Bold="true" Font-Size="Medium" OnClick="btnAddUserToList_Click" />
                                    <br />
                                    <asp:Label ID="lblFindUserResult" runat="server" Text="" Visible="false" Width="100%"></asp:Label></td>
                                <td><asp:Label ID="lblListOfUsers" runat="server" Text="" Visible="false" Width="100%"></asp:Label></td>
                            </tr>
                            <tr>
                                <td><asp:Label ID="lblCurrentUsers" runat="server" Text="Selected users" Width="100%"></asp:Label></td>
                                <td><asp:ListBox ID="drpUserStoryUsers" runat="server" Width="100%" SelectionMode="Multiple" Height="250px"></asp:ListBox></td>
                                <td><asp:Button ID="btnRemoveUserStoryUser" runat="server" Text="Remove selected User" ForeColor="Red" Width="220px" Font-Size="Medium" OnClick="btnRemoveUserStoryUser_Click" /></td>
                            </tr>
                        </table>
                        <table style="width: 100%">
                            <tr>
                                <td>
                                    <asp:Button ID="btnSubmit" runat="server" Text="Submit" BackColor="Green" Font-Bold="True" Font-Size="Medium" Width="140px" OnClick="btnSubmit_Click" OnClientClick="pleaseWait();" /></td>
                                <td></td>
                                <td>
                                    <asp:Button ID="btnCancel" runat="server" Text="Go back" BackColor="red" Font-Bold="True" Font-Size="Medium" Width="140px" OnClick="btnCancel_Click" /></td>
                            </tr>
                            <tr>
                                <td>
                                    <asp:Label ID="lblError" runat="server" ForeColor="Red" Text="Label" Visible="False" Width="100%"></asp:Label></td>
                            </tr>
                        </table>
                    </ContentTemplate>
                    <Triggers>
                        <asp:AsyncPostBackTrigger ControlID="btnSubmit" EventName="Click" />
                        <asp:AsyncPostBackTrigger ControlID="txtDeveloperResponsible" EventName="TextChanged" />
                        <asp:AsyncPostBackTrigger ControlID="drpFindUser" EventName="SelectedIndexChanged" />
                        <asp:AsyncPostBackTrigger ControlID="btnAddUserToList" EventName="Click" />
                        <asp:AsyncPostBackTrigger ControlID="drpUserStoryUsers" EventName="SelectedIndexChanged" />
                        <asp:AsyncPostBackTrigger ControlID="btnRemoveUserStoryUser" EventName="Click" />
                    </Triggers>
                </asp:UpdatePanel>
                <style>
                    .content {
                        min-width: 100%;
                    }
                </style>
                <script type="text/javascript">
                </script>
            </div>
        </div>
    </div>
    <%--Body end--%>
</asp:Content>
