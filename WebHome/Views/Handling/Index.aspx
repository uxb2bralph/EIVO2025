<%@ Page Title="" Language="C#" AutoEventWireup="true" Inherits="System.Web.Mvc.ViewPage" %>
<%@ Import Namespace="WebHome.Helper" %>

<script>
    $(function () {
        alert('<%= HttpUtility.JavaScriptStringEncode((String)ViewBag.Message) %>');
    });
</script>
