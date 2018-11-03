<%@ Page Title="Search" Language="C#" MasterPageFile="~/Master.Master" AutoEventWireup="true" CodeBehind="Search.aspx.cs" Inherits="Scrum.Accounts.Master.Search" %>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <%--Body start--%>
    <div class="container">
        <br />
        <h2><%: Title %></h2>
        <div class="panel panel-default">
            <div class="panel-body">
                <%--Content start--%>
                <style type="text/css">
                    .inline {
                        display: inline;
                    }
                </style>
                <%--Ajax start--%>
                <asp:UpdatePanel ID="upDrpSearch" UpdateMode="Conditional" runat="server">
                    <ContentTemplate>
                        <asp:DropDownList ID="drpSearch" runat="server" OnSelectedIndexChanged="drpSearch_SelectedIndexChanged" AutoPostBack="true">
                            <asp:ListItem>Select search criteria</asp:ListItem>
                            <asp:ListItem>Search projects by keywords in project name</asp:ListItem>
                            <asp:ListItem>Search user stories by keywords in user story</asp:ListItem>
                            <asp:ListItem>Search sprint tasks by keywords in sprint task</asp:ListItem>
                            <asp:ListItem>Search test cases by keywords in test cases</asp:ListItem>
                            <asp:ListItem>Search projects within a time period</asp:ListItem>
                            <asp:ListItem>Search user stories within a time period</asp:ListItem>
                            <asp:ListItem>Search sprint tasks within a time period</asp:ListItem>
                            <asp:ListItem>Search test cases within a time period</asp:ListItem>
                            <asp:ListItem>Search for users by keywords in full name</asp:ListItem>
                        </asp:DropDownList>
                        <asp:TextBox ID="txtSearch" runat="server" OnTextChanged="txtSearch_TextChanged"></asp:TextBox>&nbsp;&nbsp;
                <asp:Button ID="btnSearch" runat="server" Text="Search" OnClick="btnSearch_Click" OnClientClick="pleaseWait();" Width="10%" BackColor="green" />
                        <br />
                        <br />

                        <asp:Label ID="lblFrom" runat="server" Text="From" Visible="false" Font-Size="Medium" Font-Bold="true" BackColor="yellow" ForeColor="blue"></asp:Label>
                        <asp:Calendar ID="calFrom" runat="server" Visible="false" CssClass="inline"></asp:Calendar>
                        <asp:Label ID="lblTo" runat="server" Text="To" Visible="false" Font-Size="Medium" Font-Bold="true" BackColor="yellow" ForeColor="blue"></asp:Label>
                        <asp:Calendar ID="calTo" runat="server" Visible="false" CssClass="inline"></asp:Calendar>


                        <br />
                        <%--Message to be displayed if there is an error in the search:--%>
                        <asp:Label ID="lblErrorMessage" runat="server" Text="" Visible="false" ForeColor="Red" Font-Size="Medium" Font-Bold="true"></asp:Label>
                        <div id="modal" class="modal" style="background-color:rgba(64,64,64,0.5);width:100%;height:100%;z-index:1000;display:none"></div>
                        <div id="wait" class="modal" style="width:200px;height:20px;margin:100px auto 0 auto;display:none;background-color:#fff;z-index:1001;text-align:center;">PLEASE WAIT...</div>
                        <br />
                        <%--Message to be displayed if there is nothing to show:--%>
                        <asp:Label ID="lblResultsMessage" runat="server" Text="There is nothing to display!" Visible="false" ForeColor="Red" Font-Size="Medium" Font-Bold="true"></asp:Label>                        
                        <%--Tables of results start--%>
                        <div id="table">
                            <asp:GridView ID="grdResults" runat="server" Width="100%" HorizontalAlign="Center" BackColor="White" BorderColor="#DEDFDE" BorderStyle="None" BorderWidth="1px" CellPadding="4" ForeColor="Black" GridLines="Vertical" PageSize="20" AllowPaging="True" OnPageIndexChanging="grdResults_PageIndexChanging">
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
                        <%--Tables of results end--%>
                    </ContentTemplate>
                    <Triggers>
                        <asp:AsyncPostBackTrigger ControlID="drpSearch" EventName="SelectedIndexChanged" />
                        <asp:AsyncPostBackTrigger ControlID="btnSearch" EventName="Click" />
                    </Triggers>
                </asp:UpdatePanel>
                <%--Ajax end--%>
                <script>
                    function pleaseWait() {
                        $(".modal").show();
                        return true;
                    }
                </script>
                <%--Content end--%>
            </div>
        </div>
    </div>
    <%--Body end--%>
</asp:Content>
