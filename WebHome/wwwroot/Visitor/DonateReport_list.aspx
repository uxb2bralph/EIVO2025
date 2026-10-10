<%@ Page Title="" Language="C#" MasterPageFile="~/template/main_page.Master" AutoEventWireup="true" CodeBehind="base_page.aspx.cs" Inherits="eIVOGo.template.base_page" StylesheetTheme="Visitor" %>
<%@ Register src="../Module/Inquiry/BonusReport.ascx" tagname="BonusReport" tagprefix="uc1" %>
<%@ Register src="../Module/Inquiry/DonateReportList.ascx" tagname="DonateReportList" tagprefix="uc2" %>
<%@ Register src="../Module/Inquiry/DonateReport.ascx" tagname="DonateReport" tagprefix="uc3" %>
<asp:Content ID="header" ContentPlaceHolderID="headContent" runat="server">
</asp:Content>
<asp:Content ID="mainContent" ContentPlaceHolderID="mainContent" runat="server">
    
    <uc3:DonateReport ID="DonateReport1" runat="server" />
    
</asp:Content>
