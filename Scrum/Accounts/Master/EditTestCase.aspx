<%@ Page Title="Edit Test Case" Language="C#" MasterPageFile="~/Master.Master" AutoEventWireup="true" CodeBehind="EditTestCase.aspx.cs" Inherits="Scrum.Accounts.Master.EditTestCase" %>

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


                        <table style="width: 100%;">
                            <tr>
                                <td>
                                    <asp:Label ID="lblUniqueTestCaseID" runat="server" Text="Test Case Unique ID" Font-Size="Medium" Width="100%"></asp:Label></td>
                                <td>
                                    <asp:TextBox ID="txtUniqueTestCaseID" runat="server" Font-Size="Medium" Width="100%" Enabled="false"></asp:TextBox></td>
                                <td>
                                    <asp:Label ID="lblUniqueTestCaseIDError" runat="server" Text="Label" Visible="false" ForeColor="Red" Font-Size="Medium" Font-Bold="true"></asp:Label></td>
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
                                    <asp:Label ID="lblUniqueSprintTaskID" runat="server" Text="Sprint Task Unique ID" Font-Size="Medium" Width="100%"></asp:Label></td>
                                <td>
                                    <asp:TextBox ID="txtUniqueSprintTaskID" runat="server" Font-Size="Medium" Width="100%" Enabled="false"></asp:TextBox></td>
                                <td>
                                    <asp:Label ID="lblUniqueSprintTaskIDError" runat="server" Text="Label" Visible="false" ForeColor="Red" Font-Size="Medium" Font-Bold="true"></asp:Label></td>
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
                                    <asp:Label ID="lblTestCaseScenario" runat="server" Text="Test Case Scneario" Font-Size="Medium" Width="100%"></asp:Label></td>
                                <td>
                                    <asp:TextBox ID="txtTestCaseScenario" runat="server" Width="100%" TextMode="MultiLine" CssClass="content"></asp:TextBox></td>
                                <td>
                                    <asp:Label ID="lblTestCaseScenarioError" runat="server" Text="Label" Visible="false" ForeColor="Red" Font-Size="Medium" Font-Bold="true"></asp:Label></td>
                            </tr>
                            <tr>
                                <td>
                                    <asp:Label ID="lblInputParameters" runat="server" Text="Input Parameter" Font-Size="Medium" Width="100%"></asp:Label></td>
                                <td>
                                    <asp:TextBox ID="txtInputParameters" runat="server" Width="100%" CssClass="content"></asp:TextBox>
                                    <asp:Button ID="btnAddParameter" runat="server" Text="Add Parameter" Width="140px" BackColor="yellow" Font-Bold="true" Font-Size="Medium" OnClick="btnAddParameter_Click" />
                                </td>
                                <td>
                                    <asp:Label ID="lblInputParametersError" runat="server" Text="Label" Visible="false" ForeColor="Red" Font-Size="Medium" Font-Bold="true"></asp:Label></td>
                            </tr>
                            <tr>
                                <td>
                                    <asp:Label ID="lblInputParametersList" runat="server" Text="Added parameters" Font-Size="Medium" Width="100%"></asp:Label></td>
                                <td>
                                    <asp:ListBox ID="drpInputParametersList" runat="server" Font-Size="Medium" Width="100%"></asp:ListBox>
                                    <asp:Button ID="btnRemoveParameter" runat="server" Text="Remove Parameter" Width="200px" ForeColor="red" Font-Bold="true" Font-Size="Medium" OnClick="btnRemoveParameter_Click" />
                                </td>
                                <td>
                                    <asp:Label ID="lblInputParametersListError" runat="server" Text="Label" Visible="false" ForeColor="Red" Font-Size="Medium" Font-Bold="true"></asp:Label></td>
                            </tr>
                            <tr>
                                <td>
                                    <asp:Label ID="lblExpectedOutput" runat="server" Text="Expected Output" Font-Size="Medium" Width="100%"></asp:Label></td>
                                <td>
                                    <asp:TextBox ID="txtExpectedOutput" runat="server" Width="100%" TextMode="MultiLine" CssClass="content"></asp:TextBox></td>
                                <td>
                                    <asp:Label ID="lblExpectedOutputError" runat="server" Text="Label" Visible="false" ForeColor="Red" Font-Size="Medium" Font-Bold="true"></asp:Label></td>
                            </tr>
                            <tr>
                                <td>
                                    <asp:Label ID="lblCurrentStatus" runat="server" Text="Current Status" Font-Size="Medium" Width="100%"></asp:Label></td>
                                <td>
                                    <asp:DropDownList ID="drpCurrentStatus" runat="server" Font-Size="Medium" Width="100%">
                                        <asp:ListItem>Select a status</asp:ListItem>
                                        <asp:ListItem>Not started</asp:ListItem>
                                        <asp:ListItem>Test failed</asp:ListItem>
                                        <asp:ListItem>Test passed</asp:ListItem>
                                        <asp:ListItem>Test needs revision</asp:ListItem>
                                    </asp:DropDownList></td>
                                <td>
                                    <asp:Label ID="lblCurrentStatusError" runat="server" Text="Label" Visible="false" ForeColor="Red" Font-Size="Medium" Font-Bold="true"></asp:Label></td>
                            </tr>
                        </table>
                        <table style="width: 100%;">
                            <tr>
                                <td>
                                    <asp:Button ID="btnSaveTestCase" runat="server" Text="Save new Test Case" Width="100%" BackColor="Green" Font-Bold="true" Font-Size="Medium" OnClick="btnSaveTestCase_Click" OnClientClick="pleaseWait();" />
                                </td>
                                <td>
                                    <asp:Button ID="btnGoBackToListOfTestCases" runat="server" Text="Go Back" Width="100%" BackColor="red" Font-Bold="true" Font-Size="Medium" OnClick="btnGoBackToListOfTestCases_Click" />
                                </td>
                            </tr>
                        </table>
                        <br />
                        <asp:Label ID="lblAddTestCaseMessage" runat="server" Text=" " Visible="false" Font-Size="Medium" Font-Bold="true"></asp:Label></td>
                    </ContentTemplate>
                    <Triggers>
                        <asp:AsyncPostBackTrigger ControlID="btnAddParameter" EventName="Click" />
                        <asp:AsyncPostBackTrigger ControlID="btnRemoveParameter" EventName="Click" />
                        <asp:AsyncPostBackTrigger ControlID="btnSaveTestCase" EventName="Click" />
                        <asp:AsyncPostBackTrigger ControlID="btnGoBackToListOfTestCases" EventName="Click" />
                        <asp:PostBackTrigger ControlID="btnUpload" />
                    </Triggers>
                </asp:UpdatePanel>
                <script type="text/javascript">
                    function onInputChange(e) {
                        var res = "";
                        for (var i = 0; i < $('#<%= FileUpload1.ClientID %>').get(0).files.length; i++) {
                            res += $('#<%= FileUpload1.ClientID %>').get(0).files[i].name + "<br />";
                            console.log('iterating: ' + i + ' ' + res);
                        }
                        $('#fileNames').html(res);
                    }
                    function pleaseWait() {
                        $(".modal").show();
                        return true;
                    }
                </script>
                <%--Content end--%>
            </div>
        </div>
    </div>
    <%--body end.--%>
</asp:Content>
