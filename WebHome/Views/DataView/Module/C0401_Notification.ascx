<%@ Control Language="C#" AutoEventWireup="true" Inherits="System.Web.Mvc.ViewUserControl" %>

<%@ Import Namespace="System.Linq" %>
<%@ Import Namespace="System.IO" %>
<%@ Import Namespace="System.Linq.Expressions" %>
<%@ Import Namespace="System.Web.Mvc.Html" %>
<%@ Import Namespace="Newtonsoft.Json" %>
<%@ Import Namespace="eIVOGo.Controllers" %>
<%@ Import Namespace="eIVOGo.Helper" %>
<%@ Import Namespace="eIVOGo.Models" %>
<%@ Import Namespace="eIVOGo.Models.ViewModel" %>
<%@ Import Namespace="Model.Models.ViewModel" %>
<%@ Import Namespace="ModelCore.DataEntity" %>
<%@ Import Namespace="Model.Helper" %>
<%@ Import Namespace="Model.Locale" %>
<%@ Import Namespace="Utility" %>
<%@ Import Namespace="Uxnet.Web.WebUI" %>

<% if (_model != null)
    { %>
<div style="page-break-after: always">
    <div class="container">
        <table>
            <tr>
                <td>
                    <div class="cutfield">
                        <h3 style="width: 4.8cm;"><%= _model.InvoiceSeller.CustomerName  %></h3>
                        <h2 style="font-size: small;">*此為發票開立通知，非正式發票*</h2>
                        <h2><%= _model.InvoiceDate.Value.Year-1911 %>年<%= (_model.InvoiceDate.Value.Month % 2).Equals(0) ? String.Format("{0:00}-{1:00}", _model.InvoiceDate.Value.Month - 1, _model.InvoiceDate.Value.Month) : String.Format("{0:00}-{1:00}", _model.InvoiceDate.Value.Month, _model.InvoiceDate.Value.Month+1)%>月 </h2>
                        <h2><%= _model.TrackCode + "-" + _model.No %></h2>
                        <p>
                            <%= String.Format("{0:yyyy-MM-dd HH:mm:ss}", _model.InvoiceDate.Value)%> <%= _buyer.IsB2C()
                                                                                                ? ""
                                                                                                : _model.InvoiceBuyer.BuyerMark==3 || _model.InvoiceBuyer.BuyerMark==4
                                                                                                    ? "不得扣抵" 
                                                                                                    : String.Format("格式 {0}", ((int)_model.InvoiceType.Value).InvoiceTypeToFormat()) %><br />
                            隨機碼 <%= _model.RandomNo %>&nbsp;&nbsp;&nbsp;&nbsp;總計 <%= String.Format("{0:##,###,###,##0.##}",_model.InvoiceAmountType.TotalAmount) %><br />
                            賣方<%= _model.Organization.ReceiptNo%> <%= _buyer.IsB2C() ? null : String.Format("買方{0}", _buyer.ReceiptNo)%>
                        </p>
                        <div class="code1">
                            <%  String data = $"{_model.InvoiceDate.Value.Year - 1911:000}{_model.InvoiceDate.Value.Month:00}{_model.TrackCode}{_model.No}{_model.RandomNo}";  %>
                            <img id="barcode" alt="" width="160" height="22" src="<%= data.GetBarcode39ImageSrc() %>" />
                            <%--                <img id="barcode" alt="" width="160" height="22" src="<%= String.Format("{0}{1}?{2}", Uxnet.Web.Properties.Settings.Default.HostUrl, VirtualPathUtility.ToAbsolute("~/Published/GetBarCode39.ashx"), data) %>" />--%>
                        </div>
                        <div class="code2">
                            <img id="qrcode1" alt="" width="80" height="80" src="<%= _model.GetLeftQRCodeImageSrc() %>" />
                            <img id="qrcode2" alt="" width="80" height="80" src="<%= "**".CreateQRCodeImageSrc(width:180,height:180,qrVersion:10) %>" />
                            <%--                <img id="qrcode1" alt="" width="80" height="80" src="<%= String.Format("{0}{1}?invoiceID={2}", Uxnet.Web.Properties.Settings.Default.HostUrl, VirtualPathUtility.ToAbsolute("~/DataView/GetLeftQRCode"), _model.InvoiceID) %>" />
                <img id="qrcode2" alt="" width="80" height="80" src="<%= String.Format("{0}{1}?invoiceID={2}", Uxnet.Web.Properties.Settings.Default.HostUrl, VirtualPathUtility.ToAbsolute("~/DataView/GetRightQRCode"), _model.InvoiceID) %>" />--%>
                        </div>
                    </div>
                </td>
                <td style="vertical-align: top;">
                    <div class="listfield">
                        <div class="content_box">
                            <p class="productname">品名</p>
                            <p class="quantity">數量</p>
                            <p class="price">單價</p>
                            <p class="totalPrice">金額</p>
                        </div>
                        <% int _itemIdx;
                            for (_itemIdx = 0; _productItem != null && _itemIdx < Math.Min(_productItem.Length, _ItemPagingCount); _itemIdx++)
                            {
                                var item = _productItem[_itemIdx];
                        %>
                        <div class="content_box">
                            <p class="productname"><%= item.InvoiceProduct.Brief%></p>
                            <p class="quantity"><%= String.Format("{0:##,###,###,##0.##}", item.Piece)%></p>
                            <p class="price"><%= String.Format("{0:##,###,###,##0.##}", item.UnitCost) %></p>
                            <p class="totalPrice"><%= String.Format("{0:##,###,###,##0.##}", item.CostAmount)%></p>
                        </div>
                        <% } %>
                        <% if (_productItem != null && _productItem.Length <= _FirstCheckCount)
                            { %>
                        <div class="content_box">
                            <p style="border-top: 1px dotted #808080;">
                                <span style="font-size: 12pt; font-weight: bold;">總計：<%=_model.InvoiceDetails.Count%>項&nbsp;&nbsp;
                    金額：<%= String.Format("{0:##,###,###,##0.##}", _model.InvoiceAmountType.TotalAmount)%></span><br />
                                課稅別：<%= (_model.InvoiceAmountType.TaxType == (byte)2 || _model.InvoiceAmountType.TaxType == (byte)3) ? "TZ" : "TX"%>&nbsp;&nbsp;
                    <%  if (!_buyer.IsB2C())
                        {   %>
                    應稅銷售額：<%=String.Format("{0:##,###,###,##0.##}", _model.InvoiceAmountType.TaxType == 1 || _model.InvoiceAmountType.TaxType == 9 
                                                                        ? _model.InvoiceAmountType.SalesAmount 
                                                                        : _model.InvoiceAmountType.TaxType == 4
                                                                            ? _model.InvoiceAmountType.TotalAmount
                                                                            : 0) %>  &nbsp;&nbsp;
                    零稅率銷售額：<%=String.Format("{0:##,###,###,##0.##}",_model.InvoiceAmountType.TaxType == (byte)2 ? _model.InvoiceAmountType.SalesAmount : 0) %>  &nbsp;&nbsp;
                    免稅銷售額：<%=String.Format("{0:##,###,###,##0.##}",_model.InvoiceAmountType.TaxType == (byte)3 ? _model.InvoiceAmountType.SalesAmount : 0) %> &nbsp;&nbsp;
                    稅額：<%= String.Format("{0:##,###,###,##0.##} ",_model.InvoiceBuyer.BuyerMark==3 || _model.InvoiceBuyer.BuyerMark==4 ? 0 : _model.InvoiceAmountType.TaxAmount) %>&nbsp;&nbsp;
                    <%  } %>
                    備註：<%= String.Join(";", _model.InvoiceDetails.Select(d => d.InvoiceProduct.InvoiceProductItem.FirstOrDefault().Remark)).Replace("\r\n","<br/>").Replace("\n","<br/>") %><br />
                                <%--退貨請憑電子發票證明聯辦理--%>
                            </p>
                        </div>
                        <% } %>
                    </div>
                </td>
            </tr>
        </table>
    </div>
    <div class="listfield">
        <div class="content_box">
            Google Asia Pacific Pte Ltd以網路銷售電子勞務予中華民國境內個人買受人，自2019年1月起開立雲端發票，為方便您查詢雲端發票及兌領統一發票中獎獎金，請使用「財政部行動應用程式」或「財政部電子發票整合服務平台」【https://einvoice.nat.gov.tw】，將連結雲端發票之載具(即消費者之電子郵件信箱)歸戶至手機條碼，由財政部提供查詢發票、自動對獎及中獎主動通知服務。
        </div>
    </div>
</div>
<% if (_viewModel?.PrintBack == true || (_productItem != null && _productItem.Length > _FirstCheckCount))
    { %>
<div class="br" id="backPage" style="page-break-after: always">
    <div class="bspace listfield">
        <% for (; _itemIdx < Math.Min(_productItem.Length, _ItemPagingCount * 2); _itemIdx++)
            {
                var item = _productItem[_itemIdx];
        %>
        <div class="content_box">
            <p class="productname"><%= item.InvoiceProduct.Brief%></p>
            <p class="quantity"><%= String.Format("{0:##,###,###,##0.##}", item.Piece)%></p>
            <p class="price"><%= String.Format("{0:##,###,###,##0.##}", item.UnitCost) %></p>
            <p class="totalPrice"><%= String.Format("{0:##,###,###,##0.##}", item.CostAmount)%></p>
        </div>
        <% } %>
        <% if (_productItem.Length > _FirstCheckCount && _productItem.Length <= _SecondCheckCount)
            { %>
        <div class="content_box">
            <p style="border-top: 1px dotted #808080;">
                <span style="font-size: 12pt; font-weight: bold;">總計：<%=_model.InvoiceDetails.Count%>項&nbsp;&nbsp;金額：<%= String.Format("{0:##,###,###,##0.##}", _model.InvoiceAmountType.TotalAmount)%></span><br />
                課稅別：<%= (_model.InvoiceAmountType.TaxType == (byte)2 || _model.InvoiceAmountType.TaxType == (byte)3) ? "TZ" : "TX"%>&nbsp;&nbsp;
                <%  if (!_buyer.IsB2C())
                    {   %>
                應稅銷售額：<%=String.Format("{0:##,###,###,##0.##}", _model.InvoiceAmountType.TaxType == 1 || _model.InvoiceAmountType.TaxType == 9 
                                                                        ? _model.InvoiceAmountType.SalesAmount 
                                                                        : _model.InvoiceAmountType.TaxType == 4
                                                                            ? _model.InvoiceAmountType.TotalAmount
                                                                            : 0) %>  &nbsp;&nbsp;
                零稅率銷售額：<%=String.Format("{0:##,###,###,##0.##}",_model.InvoiceAmountType.TaxType == (byte)2 ? _model.InvoiceAmountType.SalesAmount : 0) %>  &nbsp;&nbsp;
                免稅銷售額：<%=String.Format("{0:##,###,###,##0.##}",_model.InvoiceAmountType.TaxType == (byte)3 ? _model.InvoiceAmountType.SalesAmount : 0) %> &nbsp;&nbsp;
                稅額：<%= String.Format("{0:##,###,###,##0.##} ",_model.InvoiceBuyer.BuyerMark==3 || _model.InvoiceBuyer.BuyerMark==4 ? 0 : _model.InvoiceAmountType.TaxAmount) %>&nbsp;&nbsp;
                <%  } %>
                備註：<%= String.Join(";", _model.InvoiceDetails.Select(d => d.InvoiceProduct.InvoiceProductItem.FirstOrDefault().Remark)).Replace("\r\n","<br/>").Replace("\n","<br/>") %><br />
                <%--退貨請憑電子發票證明聯辦理--%>
            </p>
        </div>
        <% } %>
    </div>
</div>
<% } %>
<% if (_productItem != null && _productItem.Length > _SecondCheckCount)
    { %>
<div class="br" style="page-break-after: always">
    <div class="listfield">
        <% for (; _itemIdx < _productItem.Length; _itemIdx++)
            {
                var item = _productItem[_itemIdx];
        %>
        <div class="content_box">
            <p class="productname"><%= item.InvoiceProduct.Brief%></p>
            <p class="quantity"><%= String.Format("{0:##,###,###,##0.##}", item.Piece)%></p>
            <p class="price"><%= String.Format("{0:##,###,###,##0.##}", item.UnitCost) %></p>
            <p class="totalPrice"><%= String.Format("{0:##,###,###,##0.##}", item.CostAmount)%></p>
        </div>
        <% } %>
        <div class="content_box">
            <p style="border-top: 1px dotted #808080;">
                <span style="font-size: 12pt; font-weight: bold;">總計：<%=_model.InvoiceDetails.Count%>項&nbsp;&nbsp;金額：<%= String.Format("{0:##,###,###,##0.##}", _model.InvoiceAmountType.TotalAmount)%></span><br />
                課稅別：<%= (_model.InvoiceAmountType.TaxType == (byte)2 || _model.InvoiceAmountType.TaxType == (byte)3) ? "TZ" : "TX"%>&nbsp;&nbsp;
                <%  if (!_buyer.IsB2C())
                    {   %>
                應稅銷售額：<%=String.Format("{0:##,###,###,##0.##}", _model.InvoiceAmountType.TaxType == 1 || _model.InvoiceAmountType.TaxType == 9 
                                                                        ? _model.InvoiceAmountType.SalesAmount 
                                                                        : _model.InvoiceAmountType.TaxType == 4
                                                                            ? _model.InvoiceAmountType.TotalAmount
                                                                            : 0) %>  &nbsp;&nbsp;
                零稅率銷售額：<%=String.Format("{0:##,###,###,##0.##}",_model.InvoiceAmountType.TaxType == (byte)2 ? _model.InvoiceAmountType.SalesAmount : 0) %>  &nbsp;&nbsp;
                免稅銷售額：<%=String.Format("{0:##,###,###,##0.##}",_model.InvoiceAmountType.TaxType == (byte)3 ? _model.InvoiceAmountType.SalesAmount : 0) %> &nbsp;&nbsp;
                稅額：<%= String.Format("{0:##,###,###,##0.##} ",_model.InvoiceBuyer.BuyerMark==3 || _model.InvoiceBuyer.BuyerMark==4 ? 0 : _model.InvoiceAmountType.TaxAmount) %>&nbsp;&nbsp;
                <%  } %>
                備註：<%= String.Join(";", _model.InvoiceDetails.Select(d => d.InvoiceProduct.InvoiceProductItem.FirstOrDefault().Remark)).Replace("\r\n","<br/>").Replace("\n","<br/>") %><br />
                <%--退貨請憑電子發票證明聯辦理--%>
            </p>
        </div>
    </div>
</div>
<% if ((_productItem.Length - _SecondCheckCount) / 43 % 2 == 0)
    { %>
<div class="br" style="page-break-after: always">&nbsp;</div>
<% } %>
<% } %>
<% } %>

<script runat="server">

    RenderStyleViewModel _viewModel;

    ModelSource<InvoiceItem> models;
    InvoiceItem _model;
    InvoiceProductItem[] _productItem;
    InvoiceBuyer _buyer;
    Organization _buyerOrg;
    bool _isDuplicatedPrint = false;
    const int _FirstCheckCount = 6;
    const int _SecondCheckCount = 16;
    const int _ItemPagingCount = 10;

    protected override void OnInit(EventArgs e)
    {
        base.OnInit(e);

        models = ((SampleController<InvoiceItem>)ViewContext.Controller).DataSource;
        _model = (InvoiceItem)this.Model;
        _viewModel = ViewBag.ViewModel as RenderStyleViewModel;

        _productItem = models.GetTable<InvoiceDetail>().Where(d => d.InvoiceID == _model.InvoiceID)
            .Join(models.GetTable<InvoiceProduct>(), d => d.ProductID, p => p.ProductID, (d, p) => p)
            .Join(models.GetTable<InvoiceProductItem>(), p => p.ProductID, i => i.ProductID, (p, i) => i).ToArray();

        _buyer = _model.InvoiceBuyer;
        if (_buyer != null && _buyer.BuyerID.HasValue)
            _buyerOrg = _buyer.Organization;

        _isDuplicatedPrint = /*_buyer.IsB2C() &&*/ _model.CDS_Document.DocumentPrintLog.Count > 0 && _model.Organization.OrganizationStatus.EntrustToPrint != true;

    }



</script>
