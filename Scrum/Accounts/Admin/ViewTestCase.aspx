<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="ViewTestCase.aspx.cs" Inherits="Scrum.Accounts.Admin.ViewTestCase" %>

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
                        <div id="modal" class="modal" style="background-color:rgba(64,64,64,0.5);width:100%;height:100%;z-index:1000;display:none"></div>
                        <div id="wait" runat="server" class="modal" style="width:200px;height:20px;margin:100px auto 0 auto;display:none;background-color:#fff;z-index:1001;text-align:center;">PLEASE WAIT...</div>
                        <div runat="server" id="View">
                            <%--Message to be displayed if there is nothing to show:--%>
                            <asp:Label ID="lblTestCaseInfo" runat="server" Text=" " Font-Size="Medium" Font-Bold="true"></asp:Label>
                            <br />
                            <asp:Label ID="lblMessage" runat="server" Text="There are nothing to display!" Visible="false" ForeColor="Red" Font-Size="Medium" Font-Bold="true"></asp:Label>
                            <br />
                            <table style="width: 100%;">
                                <tr>
                                    <td> </td>
                                    <td><asp:Button ID="btnGoBack" runat="server" Text="Go Back" Width="100%" BackColor="red" Font-Bold="true" Font-Size="Medium" OnClick="btnGoBack_Click" /></td>
                                </tr>
                            </table>
                        </div>
                    </ContentTemplate>
                    <Triggers>
                        <asp:AsyncPostBackTrigger ControlID="btnGoBack" EventName="Click" />
                    </Triggers>
                </asp:UpdatePanel>
                <script type="text/javascript">
                    function pleaseWait() {
                        $(".modal").show();
                        return true;
                    }
                    function removeTestCase(testCaseId, creatorId) {
                        pleaseWait();
                        if (confirm('Are sure you want to remove the selected test case?'))
                            removeTestCaseConfirmed(testCaseId, creatorId);
                        else {
                            $(".modal").hide();
                        }
                    }
                    function removeTestCaseConfirmed(testCaseId, creatorId) {
                        console.log('You just confirmed!');
                        var testCaseID = parseInt(testCaseId);
                        var creatorID = parseInt(creatorId);
                        var obj = {
                            testCaseId: testCaseID,
                            entry_creatorId: creatorID
                        };
                        var param = JSON.stringify(obj);  // stringify the parameter
                        $.ajax({
                            method: "POST",
                            url: '<%= ResolveUrl("ViewTestCase.aspx/removeTestCase_Click") %>',
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
                    function editTestCase(testCaseId) {
                        window.location.href = "EditTestCase.aspx?id=" + testCaseId;
                    }
                    function updateStatus(testCaseId, creatorId) {
                        var e = document.getElementById("statuses");
                        var selectedStatus = e.options[e.selectedIndex].value;
                        var index = e.options[e.selectedIndex].index;
                        console.log(index);
                        if (index == 0) {
                            alert("Please, select a status to update");
                        }
                        else {
                            if (confirm("Are you sure you want to update the status to '" + selectedStatus + "'?")) {
                                updateTestCaseStatusConfirmed(testCaseId, creatorId, selectedStatus);
                            }
                        }
                    }
                    function updateTestCaseStatusConfirmed(testCaseId, creatorId, selectedStatus) {
                        console.log(testCaseId +" "+ creatorId +" "+ selectedStatus);
                        var testCaseID = parseInt(testCaseId);
                        var creatorID = parseInt(creatorId);
                        var obj = {
                            testCaseId: testCaseID,
                            entry_creatorId: creatorID,
                            newStatus: selectedStatus
                        };
                        var param = JSON.stringify(obj);  // stringify the parameter
                        console.log("Second time: "+testCaseId +" "+ creatorId +" "+ selectedStatus);
                        $.ajax({
                            method: "POST",
                            url: '<%= ResolveUrl("ViewTestCase.aspx/updateTestCaseStatus_Click") %>',
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
