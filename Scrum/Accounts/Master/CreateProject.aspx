<%@ Page Title="Create Project" Language="C#" MasterPageFile="~/Master.Master" AutoEventWireup="true" CodeBehind="CreateProject.aspx.cs" Inherits="Scrum.Accounts.Master.CreateProject" %>

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
                        <div id="modal" class="modal" style="background-color:rgba(64,64,64,0.5);width:100%;height:100%;z-index:1000;display:none"></div>
                        <div id="wait" runat="server" class="modal" style="width:200px;height:20px;margin:100px auto 0 auto;display:none;background-color:#fff;z-index:1001;text-align:center;">PLEASE WAIT...</div>
                        <table>
                            <tr>
                                <td>
                                    <asp:Label ID="lblTitle" runat="server" Text="Project name" Width="100%"></asp:Label></td>
                                <td>
                                    <asp:TextBox ID="txtTitle" runat="server" Width="100%"></asp:TextBox></td>
                                <td>
                                    <asp:Label ID="lblTitleError" runat="server" Text="Invalid input: Please type the project name." Visible="false" ForeColor="red"></asp:Label></td>
                            </tr>
                            <tr>
                                <td>
                                    <asp:Label ID="lblDescription" runat="server" Text="Description" Width="100%"></asp:Label></td>
                                <td>
                                    <asp:TextBox ID="txtDescription" runat="server" Width="100%" TextMode="MultiLine" CssClass="content"></asp:TextBox></td>
                                <td>
                                    <asp:Label ID="lblDescriptionError" runat="server" Text="Invalid input: Please type a description." Visible="false" ForeColor="red"></asp:Label></td>
                            </tr>
                            <tr>
                                <td><asp:Label ID="lblFileUpload" runat="server" Text="Select files" Width="100%"></asp:Label></td>
                                <td>
                                    <input type="file" ID="FileUpload1" runat="server" multiple style="float:left; width:70%; height:34px;" onChange="onInputChange(event)" class="btn-primary">
                                    <asp:Button ID="btnUpload" runat="server" Text="Upload"  class=" btn-primary" Height="34px" Width="30%" Font-Bold="True" Font-Size="Medium" OnClick="btnUpload_Click" />
                                </td>
                                <td>
                                    <div runat="server" ClientIDMode="Static" id='fileNames'></div>
                                </td>
                                <td>
                                    <asp:Label ID="lblImageError" runat="server" Text="Image" Visible="false" ForeColor="red"></asp:Label></td>
                            </tr>
                            <tr>
                                <td>
                                    <asp:Label ID="lblStartDate" runat="server" Text="Start date" Width="100%"></asp:Label></td>
                                <td>
                                    <asp:Calendar runat="server" ID="calStartDate" Width="100%" OnDayRender="dayRender"></asp:Calendar>
                                </td>
                                <td>
                                    <asp:Label ID="lblStartDateError" runat="server" Text="calendar" Visible="false" ForeColor="red"></asp:Label></td>

                            </tr>
                        </table>
                        <table style="width: 100%">
                            <tr>
                                <td>
                                    <asp:Button ID="btnSubmit" runat="server" Text="Submit" BackColor="Green" Font-Bold="True" Font-Size="Medium" Width="140px" OnClick="btnSubmit_Click" OnClientClick="pleaseWait();"/></td>
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
                        <asp:AsyncPostBackTrigger ControlID="calStartDate" EventName="SelectionChanged" />
                        <asp:AsyncPostBackTrigger ControlID="btnSubmit" EventName="Click" />
                        <asp:PostBackTrigger ControlID="btnUpload" />
                    </Triggers>
                </asp:UpdatePanel>
                <style>
                    .content {
                        min-width: 100%;
                    }
                </style>
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
                </script>
            </div>
        </div>
    </div>
    <%--Body end--%>
</asp:Content>
