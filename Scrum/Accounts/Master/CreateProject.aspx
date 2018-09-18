<%@ Page Title="Create Project" Language="C#" MasterPageFile="~/Master.Master" AutoEventWireup="true" CodeBehind="CreateProject.aspx.cs" Inherits="Scrum.Accounts.Master.CreateProject" %>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
     <%--Body start--%>
    <div class="container">
        <br />
        <h2><%: Title %></h2>
        <div class="panel panel-default">
            <div class="panel-body">
                <asp:UpdatePanel ID="upAjax" UpdateMode="Conditional" runat="server">
                    <ContentTemplate>
                <table>
                    <tr>
                        <td><asp:Label ID="lblTitle" runat="server" Text="Project name" Width="100%"></asp:Label></td>
                        <td><asp:TextBox ID="txtTitle" runat="server" Width="100%"></asp:TextBox></td>
                        <td><asp:Label ID="lblTitleError" runat="server" Text="Invalid input: Please type the project name." Visible="false" ForeColor="red" ></asp:Label></td>
                    </tr>
                    <tr>
                        <td><asp:Label ID="lblDescription" runat="server" Text="Description" Width="100%"></asp:Label></td>
                        <td><asp:TextBox ID="txtDescription" runat="server" Width="100%" TextMode="MultiLine" CssClass="content" ></asp:TextBox></td>
                        <td><asp:Label ID="lblDescriptionError" runat="server" Text="Invalid input: Please type a description." Visible="false" ForeColor="red" ></asp:Label></td>
                    </tr>
                    
                    <tr>
                        <td><asp:Label ID="lblStartDate" runat="server" Text="Start date" Width="100%"></asp:Label></td>
                        <td><asp:Calendar runat="server" ID="calStartDate" Width="100%"></asp:Calendar></td>
                        <td><asp:Label ID="lblStartDateError" runat="server" Text="calendar" Visible="false" ForeColor="red" ></asp:Label></td>
                    </tr>
                    <tr>
                        <td><asp:Label ID="lblFileUpload" runat="server" Text="Upload images" Width="100%"></asp:Label></td>
                        <td><asp:FileUpload ID="FileUpload1" runat="server" Width="100%" AllowMultiple="true" onchange="onInputChange(event)" class="btn btn-primary" /></td>
                        <td><div id='fileNames' ></div></td>
                        <td><asp:Label ID="lblImageError" runat="server" Text="Image" Visible="false" ForeColor="red" ></asp:Label></td>
                    </tr>
                    <tr>
                        <td><asp:Button ID="btnSubmit" runat="server" Text="Submit" BackColor="Green" Font-Bold="True" Font-Size="Medium" Width="140px" OnClick="btnSubmit_Click" /></td>
                        <td> </td>
                        <td><asp:Button ID="btnCancel" runat="server" Text="Go back" BackColor="red" Font-Bold="True" Font-Size="Medium" Width="140px" OnClick="btnCancel_Click" /></td>
                    </tr>
                    <tr>
                        <td><asp:Label ID="lblError" runat="server" ForeColor="Red" Text="Label" Visible="False" Width="100%"></asp:Label></td>
                    </tr>
                </table>
                        </ContentTemplate>
                    <Triggers>
                        <asp:AsyncPostBackTrigger ControlID="calStartDate" EventName="SelectionChanged" />
                        
                    </Triggers>
                </asp:UpdatePanel>
                <style>.content {min-width: 100%;}</style>
                <script type="text/javascript">
                    function onInputChange(e) {
                        var res = "";
                        for (var i = 0; i < $('#<%= FileUpload1.ClientID %>').get(0).files.length; i++) {
                            res += $('#<%= FileUpload1.ClientID %>').get(0).files[i].name + "<br />";
                        }
                        $('#fileNames').html(res);
                    }
                </script>
            </div>
        </div>
    </div>
    <%--Body end--%>
</asp:Content>
