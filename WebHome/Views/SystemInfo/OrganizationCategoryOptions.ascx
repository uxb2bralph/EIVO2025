﻿<%@ Control Language="C#" AutoEventWireup="true" Inherits="System.Web.Mvc.ViewUserControl" %>
<%@ Import Namespace="System.IO" %>
<%@ Import Namespace="System.Linq.Expressions" %>
<%@ Import Namespace="System.Web.Mvc.Html" %>
<%@ Import Namespace="System.Web.Mvc" %>
<%@ Import Namespace="eIVOGo.Helper" %>
<%@ Import Namespace="Model.Locale" %>
<%@ Import Namespace="eIVOGo.Models.ViewModel" %>
<%@ Import Namespace="Model.Models.ViewModel" %>
<%@ Import Namespace="ModelCore.DataEntity" %>
<%@ Import Namespace="eIVOGo.Controllers" %>

<%  foreach (var item in Enum.GetValues(typeof(CategoryDefinition.CategoryEnum)))
    {
        var t = (CategoryDefinition.CategoryEnum)item;  %>
<option value="<%= (int)t %>"><%= t %></option>
<%  } %>

<script runat="server">

    ModelStateDictionary _modelState;
    ModelSource<InvoiceItem> models;

    protected override void OnInit(EventArgs e)
    {
        base.OnInit(e);
        _modelState = (ModelStateDictionary)ViewBag.ModelState;
        models = ((SampleController<InvoiceItem>)ViewContext.Controller).DataSource;
    }

</script>
