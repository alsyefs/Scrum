<%@ Page Title="Edit Sprint Task" Language="C#" MasterPageFile="~/Master.Master" AutoEventWireup="true" CodeBehind="EditSprintTask.aspx.cs" Inherits="Scrum.Accounts.Master.EditSprintTask" %>

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
                        <div runat="server" id="AddNewTestCase">
                            <table style="width: 100%;">
                                <tr>
                                    <td>
                                        <asp:Label ID="lblUniqueSprintTaskID" runat="server" Text="Sprint Task Unique ID" Font-Size="Medium" Width="100%"></asp:Label></td>
                                    <td>
                                        <asp:TextBox ID="txtUniqueSprintTaskID" runat="server" Font-Size="Medium" Width="100%" Enabled="false"></asp:TextBox></td>
                                    <td>
                                        <asp:Label ID="lblUniqueSprintTaskIDError" runat="server" Text="Label" Visible="false" ForeColor="Red" Font-Size="Medium" Font-Bold="true"></asp:Label></td>
                                </tr>
                                <tr>
                                    <td>
                                        <asp:Label ID="lblUniqueUserStoryID" runat="server" Text="User Story Unique ID" Font-Size="Medium" Width="100%"></asp:Label></td>
                                    <td>
                                        <asp:TextBox ID="txtUniqueUserStoryID" runat="server" Font-Size="Medium" Width="100%" Enabled="false"></asp:TextBox></td>
                                    <td>
                                        <asp:Label ID="lblUniqueUserStoryIDError" runat="server" Text="Label" Visible="false" ForeColor="Red" Font-Size="Medium" Font-Bold="true"></asp:Label></td>
                                </tr>
                                <tr>
                                    <td>
                                        <asp:Label ID="lblFileUpload" runat="server" Text="Select files" Width="100%"></asp:Label></td>
                                    <td>
                                        <input type="file" id="FileUpload1" runat="server" multiple style="float: left; width: 70%; height: 34px;" onchange="onInputChange(event)" class="btn-primary">
                                        <asp:Button ID="btnUpload" name="btnUpload_name" runat="server" Text="Upload" class=" btn-primary" Height="34px" Width="30%" Font-Bold="True" Font-Size="Medium" OnClick="btnUpload_Click" />
                                    </td>
                                    <td>
                                        <div runat="server" clientidmode="Static" id='fileNames'></div>
                                    </td>
                                    <td>
                                        <asp:Label ID="lblImageError" runat="server" Text="Image" Visible="false" ForeColor="red"></asp:Label></td>
                                </tr>
                                <tr>
                                    <td>
                                        <asp:Label ID="lblTaskDescription" runat="server" Text="Task Description" Font-Size="Medium" Width="100%"></asp:Label></td>
                                    <td>
                                        <asp:TextBox ID="txtTaskDescription" runat="server" Width="100%" TextMode="MultiLine" CssClass="content"></asp:TextBox></td>
                                    <td>
                                        <asp:Label ID="lblTaskDescriptionError" runat="server" Text="Label" Visible="false" ForeColor="Red" Font-Size="Medium" Font-Bold="true"></asp:Label></td>
                                </tr>
                                <tr>
                                    <td>
                                        <asp:Label ID="lblDateIntroduced" runat="server" Text="Date introduced" Font-Size="Medium" Width="100%"></asp:Label></td>
                                    <td>
                                        <asp:Calendar ID="calDateIntroduced" runat="server" Font-Size="Medium" Width="100%" OnSelectionChanged="calDateIntroduced_SelectionChanged" OnDayRender="dayRender"></asp:Calendar>
                                    </td>
                                    <td>
                                        <asp:Label ID="lblDateIntroducedError" runat="server" Text="Label" Visible="false" ForeColor="Red" Font-Size="Medium" Font-Bold="true"></asp:Label></td>
                                </tr>
                                <tr>
                                    <td>
                                        <asp:Label ID="lblDateConsidered" runat="server" Text="Date considered for implementation" Font-Size="Medium" Width="100%"></asp:Label></td>
                                    <td>
                                        <asp:Calendar ID="calDateConsidered" runat="server" Font-Size="Medium" Width="100%" OnSelectionChanged="calDateConsidered_SelectionChanged" OnDayRender="dayRender"></asp:Calendar>
                                    </td>
                                    <td>
                                        <asp:Label ID="lblDateConsideredError" runat="server" Text="Label" Visible="false" ForeColor="Red" Font-Size="Medium" Font-Bold="true"></asp:Label></td>
                                </tr>
                                <tr>
                                    <td>
                                        <asp:Label ID="lblDeveloperResponsible" runat="server" Text="Developers responsible for" Font-Size="Medium" Width="100%"></asp:Label></td>
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
                                    <td>
                                        <asp:Label ID="lblListOfUsers" runat="server" Text="" Visible="false" Width="100%"></asp:Label></td>
                                </tr>
                                <tr>
                                <td>
                                    <asp:Label ID="lblCurrentUsers" runat="server" Text="Selected users" Width="100%"></asp:Label></td>
                                <td>
                                    <asp:ListBox ID="drpSelectedUsers" runat="server" Width="100%" SelectionMode="Multiple" Height="250px"></asp:ListBox></td>
                                <td>
                                    <asp:Button ID="btnRemoveSelectedUser" runat="server" Text="Remove selected User" ForeColor="Red" Width="220px" Font-Size="Medium" OnClick="btnRemoveSelectedUser_Click" /></td>
                            </tr>
                                <tr>
                                    <td>
                                        <asp:Label ID="lblCurrentStatus" runat="server" Text="Current Status" Font-Size="Medium" Width="100%"></asp:Label></td>
                                    <td>
                                        <asp:DropDownList ID="drpCurrentStatus" runat="server" Font-Size="Medium" Width="100%">
                                            <asp:ListItem>Select a status</asp:ListItem>
                                            <asp:ListItem>Not started</asp:ListItem>
                                            <asp:ListItem>In progress</asp:ListItem>
                                            <asp:ListItem>In current sprint</asp:ListItem>
                                            <asp:ListItem>Completed</asp:ListItem>
                                        </asp:DropDownList></td>
                                    <td>
                                        <asp:Label ID="lblCurrentStatusError" runat="server" Text="Label" Visible="false" ForeColor="Red" Font-Size="Medium" Font-Bold="true"></asp:Label></td>
                                </tr>
                            </table>
                            <table style="width: 100%;">
                                <tr>
                                    <td>
                                        <asp:Button ID="btnSaveSprintTask" runat="server" Text="Save new Sprint Task" Width="100%" BackColor="Green" Font-Bold="true" Font-Size="Medium" OnClick="btnSaveSprintTask_Click" OnClientClick="pleaseWait();" />
                                    </td>
                                    <td>
                                        <asp:Button ID="btnGoBackToListOfSprintTasks" runat="server" Text="Go Back" Width="100%" BackColor="red" Font-Bold="true" Font-Size="Medium" OnClick="btnGoBackToListOfSprintTasks_Click" />
                                    </td>
                                </tr>
                            </table>
                            <br />
                            <asp:Label ID="lblAddSprintTaskMessage" runat="server" Text=" " Visible="false" Font-Size="Medium" Font-Bold="true"></asp:Label></td>
                        </div>
                    </ContentTemplate>
                    <Triggers>
                        <asp:AsyncPostBackTrigger ControlID="btnSaveSprintTask" EventName="Click" />
                        <asp:AsyncPostBackTrigger ControlID="btnGoBackToListOfSprintTasks" EventName="Click" />
                        <asp:AsyncPostBackTrigger ControlID="calDateIntroduced" EventName="SelectionChanged" />
                        <asp:AsyncPostBackTrigger ControlID="calDateConsidered" EventName="SelectionChanged" />
                        <asp:AsyncPostBackTrigger ControlID="txtDeveloperResponsible" EventName="TextChanged" />
                        <asp:AsyncPostBackTrigger ControlID="drpFindUser" EventName="SelectedIndexChanged" />
                        <asp:AsyncPostBackTrigger ControlID="btnAddUserToList" EventName="Click" />
                        <asp:PostBackTrigger ControlID="btnUpload" />
                        <asp:AsyncPostBackTrigger ControlID="drpSelectedUsers" EventName="SelectedIndexChanged" />
                        <asp:AsyncPostBackTrigger ControlID="btnRemoveSelectedUser" EventName="Click" />
                    </Triggers>
                </asp:UpdatePanel>
                <style>
                    .content {
                        min-width: 100%;
                    }
                </style>
            </div>
        </div>
    </div>
    <%--Body end--%>
</asp:Content>
