<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="ViewUserStory.aspx.cs" Inherits="Scrum.Accounts.Admin.ViewUserStory" %>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <%--body start:--%>
    <div class="container">
        <br />
        <h2><%: Title %></h2>
        <div class="panel panel-default">
            <div class="panel-body">
                <%--Content start--%>
                <asp:UpdatePanel ID="upContent" UpdateMode="Conditional" runat="server">
                    <ContentTemplate>
                        <div id="modal" class="modal" style="background-color: rgba(64,64,64,0.5); width: 100%; height: 100%; z-index: 1000; display: none"></div>
                        <div id="wait" runat="server" class="modal" style="width: 200px; height: 20px; margin: 100px auto 0 auto; display: none; background-color: #fff; z-index: 1001; text-align: center;">PLEASE WAIT...</div>
                        <div runat="server" id="View">
                            <%--Message to be displayed if there is nothing to show:--%>
                            <asp:Label ID="lblUserStoryInfo" runat="server" Text=" " Font-Size="Medium" Font-Bold="true"></asp:Label>
                            <br />
                            <asp:Label ID="lblMessage" runat="server" Text="There are no sprint tasks to display!" Visible="false" ForeColor="Red" Font-Size="Medium" Font-Bold="true"></asp:Label>
                            <div id="table">
                                <asp:GridView ID="grdSprintTasks" runat="server" Width="100%" HorizontalAlign="Center" BackColor="White" BorderColor="#DEDFDE" BorderStyle="None" BorderWidth="1px" CellPadding="4" ForeColor="Black" GridLines="Vertical" PageSize="10" AllowPaging="True" OnPageIndexChanging="grdSprintTasks_PageIndexChanging">
                                    <AlternatingRowStyle BackColor="White" />
                                    <FooterStyle BackColor="#CCCC99" />
                                    <HeaderStyle BackColor="#6B696B" Font-Bold="True" ForeColor="White" />
                                    <PagerStyle BackColor="#F7F7DE" ForeColor="Black" HorizontalAlign="Right" />
                                    <RowStyle BackColor="#F7F7DE" />
                                    <SelectedRowStyle BackColor="#CE5D5A" Font-Bold="True" ForeColor="White" />
                                    <SortedAscendingCellStyle BackColor="#FBFBF2" />
                                    <SortedAscendingHeaderStyle BackColor="#848384" />
                                    <SortedDescendingCellStyle BackColor="#EAEAD3" />
                                    <SortedDescendingHeaderStyle BackColor="#575357" />
                                </asp:GridView>
                            </div>
                            <br />
                            <table style="width: 100%;">
                                <tr>
                                    <td>
                                        <asp:Button ID="btnAddNewSprintTask" runat="server" Text="Add Sprint Task" Width="100%" BackColor="Green" Font-Bold="true" Font-Size="Medium" OnClick="btnAddNewSprintTask_Click" />
                                    </td>
                                    <td>
                                        <asp:Button ID="btnGoBack" runat="server" Text="Go Back" Width="100%" BackColor="red" Font-Bold="true" Font-Size="Medium" OnClick="btnGoBack_Click" />
                                    </td>
                                </tr>
                            </table>
                        </div>
                        <div runat="server" id="AddNewSprintTask">
                            <table style="width: 100%;">
                                <tr>
                                    <td><asp:Label ID="lblUniqueSprintTaskID" runat="server" Text="Sprint Task Unique ID" Font-Size="Medium" Width="100%"></asp:Label></td>
                                    <td><asp:TextBox ID="txtUniqueSprintTaskID" runat="server" Font-Size="Medium" Width="100%" Enabled="false"></asp:TextBox></td>
                                    <td><asp:Label ID="lblUniqueSprintTaskIDError" runat="server" Text="Label" Visible="false" ForeColor="Red" Font-Size="Medium" Font-Bold="true"></asp:Label></td>
                                </tr>
                                <tr>
                                    <td><asp:Label ID="lblUniqueUserStoryID" runat="server" Text="User Story Unique ID" Font-Size="Medium" Width="100%"></asp:Label></td>
                                    <td><asp:TextBox ID="txtUniqueUserStoryID" runat="server" Font-Size="Medium" Width="100%" Enabled="false"></asp:TextBox></td>
                                    <td><asp:Label ID="lblUniqueUserStoryIDError" runat="server" Text="Label" Visible="false" ForeColor="Red" Font-Size="Medium" Font-Bold="true"></asp:Label></td>
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
                                    <td><asp:Label ID="lblTaskDescription" runat="server" Text="Task Description" Font-Size="Medium" Width="100%"></asp:Label></td>
                                    <td><asp:TextBox ID="txtTaskDescription" runat="server" Width="100%" TextMode="MultiLine" CssClass="content"></asp:TextBox></td>
                                    <td><asp:Label ID="lblTaskDescriptionError" runat="server" Text="Label" Visible="false" ForeColor="Red" Font-Size="Medium" Font-Bold="true"></asp:Label></td>
                                </tr>
                                <tr>
                                    <td><asp:Label ID="lblDateIntroduced" runat="server" Text="Date introduced" Font-Size="Medium" Width="100%"></asp:Label></td>
                                    <td><asp:Calendar ID="calDateIntroduced" runat="server" Font-Size="Medium" Width="100%" OnSelectionChanged="calDateIntroduced_SelectionChanged" OnDayRender="dayRender"></asp:Calendar></td>
                                    <td><asp:Label ID="lblDateIntroducedError" runat="server" Text="Label" Visible="false" ForeColor="Red" Font-Size="Medium" Font-Bold="true"></asp:Label></td>
                                </tr>
                                <tr>
                                    <td><asp:Label ID="lblDateConsidered" runat="server" Text="Date considered for implementation" Font-Size="Medium" Width="100%"></asp:Label></td>
                                    <td><asp:Calendar ID="calDateConsidered" runat="server" Font-Size="Medium" Width="100%" OnSelectionChanged="calDateConsidered_SelectionChanged" OnDayRender="dayRender"></asp:Calendar></td>
                                    <td><asp:Label ID="lblDateConsideredError" runat="server" Text="Label" Visible="false" ForeColor="Red" Font-Size="Medium" Font-Bold="true"></asp:Label></td>
                                </tr>
                                <tr>
                                    <td><asp:Label ID="lblDeveloperResponsible" runat="server" Text="Developers responsible for" Font-Size="Medium" Width="100%"></asp:Label></td>
                                    <td><asp:TextBox ID="txtDeveloperResponsible" runat="server" Font-Size="Medium" Width="100%" AutoPostBack="true" OnTextChanged="txtDeveloperResponsible_TextChanged"></asp:TextBox></td>
                                    <td><asp:Label ID="lblDeveloperResponsibleError" runat="server" Text="Label" Visible="false" ForeColor="Red" Font-Size="Medium" Font-Bold="true"></asp:Label></td>
                                </tr>
                                <tr>
                                    <td><asp:Label ID="lblSelectUser" runat="server" Text="Select developer" Width="100%"></asp:Label></td>
                                    <td><asp:ListBox ID="drpFindUser" runat="server" Width="100%" AutoPostBack="true" OnSelectedIndexChanged="drpFindUser_SelectedIndexChanged"></asp:ListBox>
                                        <asp:Button ID="btnAddUserToList" runat="server" Text="Add to list" Width="100px" BackColor="yellow" Font-Bold="true" Font-Size="Medium" OnClick="btnAddUserToList_Click" />
                                        <br />
                                        <asp:Label ID="lblFindUserResult" runat="server" Text="" Visible="false" Width="100%"></asp:Label></td>
                                    <td><asp:Label ID="lblListOfUsers" runat="server" Text="" Visible="false" Width="100%"></asp:Label></td>
                                </tr>
                                <tr><td><asp:Label ID="lblCurrentStatus" runat="server" Text="Current Status" Font-Size="Medium" Width="100%"></asp:Label></td>
                                    <td>
                                        <asp:DropDownList ID="drpCurrentStatus" runat="server" Font-Size="Medium" Width="100%">
                                            <asp:ListItem>Select a status</asp:ListItem>
                                            <asp:ListItem>Not started</asp:ListItem>
                                            <asp:ListItem>In progress</asp:ListItem>
                                            <asp:ListItem>In current sprint</asp:ListItem>
                                            <asp:ListItem>Completed</asp:ListItem>
                                        </asp:DropDownList></td>
                                    <td><asp:Label ID="lblCurrentStatusError" runat="server" Text="Label" Visible="false" ForeColor="Red" Font-Size="Medium" Font-Bold="true"></asp:Label></td>
                                </tr>
                            </table>
                            <table style="width: 100%;">
                                <tr>
                                    <td><asp:Button ID="btnSaveSprintTask" runat="server" Text="Save new Sprint Task" Width="100%" BackColor="Green" Font-Bold="true" Font-Size="Medium" OnClick="btnSaveSprintTask_Click" OnClientClick="pleaseWait();" />
                                    </td>
                                    <td><asp:Button ID="btnGoBackToListOfSprintTasks" runat="server" Text="Go Back" Width="100%" BackColor="red" Font-Bold="true" Font-Size="Medium" OnClick="btnGoBackToListOfSprintTasks_Click" />
                                    </td>
                                </tr>
                            </table>
                            <br />
                            <asp:Label ID="lblAddSprintTaskMessage" runat="server" Text=" " Visible="false" Font-Size="Medium" Font-Bold="true"></asp:Label></td>
                        </div>
                        <div runat="server" id="divRemoveSprintTask" class="mainPopup" visible="false">
                            <div runat="server" class="internalPopup">
                                <asp:Label ID="lblRemoveSprintTaskMessage" runat="server" Text="" Width="100%"></asp:Label>
                                <asp:Label ID="lblSprintTaskId" Visible="false" runat="server" Text="" Width="100%"></asp:Label>
                                <br />
                                <br />
                                <table>
                                    <tr>
                                        <td>
                                            <asp:Button ID="btnConfirmRemoveSprintTask" runat="server" Text="Remove" CssClass="confirmRemove" Font-Bold="True" Font-Size="Medium" Width="140px" OnClick="btnConfirmRemoveSprintTask_Click" /></td>
                                        <td style="width: 140px;"></td>
                                        <td>
                                            <asp:Button ID="btnCancelRemoveSprintTask" runat="server" Text="Cancel" CssClass="cancelRemove" Font-Bold="True" Font-Size="Medium" Width="140px" OnClick="btnCancelRemoveSprintTask_Click" /></td>
                                    </tr>
                                </table>
                            </div>
                        </div>
                    </ContentTemplate>
                    <Triggers>
                        <asp:AsyncPostBackTrigger ControlID="btnAddNewSprintTask" EventName="Click" />
                        <asp:AsyncPostBackTrigger ControlID="btnGoBack" EventName="Click" />
                        <asp:AsyncPostBackTrigger ControlID="btnSaveSprintTask" EventName="Click" />
                        <asp:AsyncPostBackTrigger ControlID="btnGoBackToListOfSprintTasks" EventName="Click" />
                        <asp:AsyncPostBackTrigger ControlID="calDateIntroduced" EventName="SelectionChanged" />
                        <asp:AsyncPostBackTrigger ControlID="calDateConsidered" EventName="SelectionChanged" />
                        <asp:AsyncPostBackTrigger ControlID="txtDeveloperResponsible" EventName="TextChanged" />
                        <asp:AsyncPostBackTrigger ControlID="drpFindUser" EventName="SelectedIndexChanged" />
                        <asp:AsyncPostBackTrigger ControlID="btnAddUserToList" EventName="Click" />
                        <asp:PostBackTrigger ControlID="btnUpload" />
                        <asp:AsyncPostBackTrigger ControlID="btnConfirmRemoveSprintTask" EventName="Click" />
                        <asp:AsyncPostBackTrigger ControlID="btnCancelRemoveSprintTask" EventName="Click" />
                    </Triggers>
                </asp:UpdatePanel>
                <script type="text/javascript">
                    function onInputChange(e) {
                        console.log('started onChange');
                        var res = "";
                        for (var i = 0; i < $('#<%= FileUpload1.ClientID %>').get(0).files.length; i++) {
                            res += $('#<%= FileUpload1.ClientID %>').get(0).files[i].name + "<br />";
                            console.log('iterating: ' + i + ' ' + res);
                        }
                        $('#fileNames').html(res);
                        console.log('finished onChange.' + ' file names: ' + res);
                    }
                    function pleaseWait() {
                        $(".modal").show();
                        return true;
                    }
                    function removeUserStory(userStoryId, creatorId) {
                        pleaseWait();
                        if (confirm('Are sure you want to remove the selected user story?'))
                            removeUserStoryConfirmed(userStoryId, creatorId);
                        else {
                            $(".modal").hide();
                        }
                    }
                    function removeUserStoryConfirmed(userStoryId, creatorId) {
                        var userStoryID = parseInt(userStoryId);
                        var creatorID = parseInt(creatorId);
                        var obj = {
                            userStoryId: userStoryID,
                            entry_creatorId: creatorID
                        };
                        var param = JSON.stringify(obj);  // stringify the parameter
                        $.ajax({
                            method: "POST",
                            url: '<%= ResolveUrl("ViewUserStory.aspx/removeUserStory_Click") %>',
                            data: param,
                            contentType: "application/json; charset=utf-8",
                            dataType: "json",
                            async: true,
                            cache: false,
                            success: function (msg) {
                                window.location.href = window.location.href;
                            },
                            error: function (xhr, status, error) {
                                console.log(xhr.responseText);
                            }
                        });
                    }
                    function editUserStory(userStoryId) {
                        window.location.href = "EditUserStory.aspx?userStoryId=" + userStoryId;
                    }
                    function updateStatus(userStoryId, creatorId) {
                        var e = document.getElementById("userStoryStatuses");
                        var selectedStatus = e.options[e.selectedIndex].value;
                        var index = e.options[e.selectedIndex].index;
                        console.log(index);
                        if (index == 0) {
                            alert("Please, select a status to update");
                        }
                        else {
                            if (confirm("Are you sure you want to update the status to '" + selectedStatus + "'?")) {
                                updateUserStoryStatusConfirmed(userStoryId, creatorId, selectedStatus);
                            }
                        }
                    }
                    function updateUserStoryStatusConfirmed(userStoryId, creatorId, selectedStatus) {
                        console.log(userStoryId +" "+ creatorId +" "+ selectedStatus);
                        var userStoryID = parseInt(userStoryId);
                        var creatorID = parseInt(creatorId);
                        var obj = {
                            userStoryId: userStoryID,
                            entry_creatorId: creatorID,
                            newStatus: selectedStatus
                        };
                        var param = JSON.stringify(obj);  // stringify the parameter
                        $.ajax({
                            method: "POST",
                            url: '<%= ResolveUrl("ViewUserStory.aspx/updateUserStoryStatus_Click") %>',
                            data: param,
                            contentType: "application/json; charset=utf-8",
                            dataType: "json",
                            async: true,
                            cache: false,
                            success: function (msg) {
                                window.location.href = window.location.href;
                            },
                            error: function (xhr, status, error) {
                                console.log(xhr.responseText);
                            }
                        });
                    }
                </script>
                <%--Content end--%>
            </div>
        </div>
    </div>
    <%--body end.--%>
</asp:Content>
