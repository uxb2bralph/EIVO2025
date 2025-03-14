<%@ Page Title="" Language="C#" AutoEventWireup="true" Inherits="System.Web.Mvc.ViewPage" %>

<%@ Import Namespace="System.IO" %>
<%@ Import Namespace="System.Linq.Expressions" %>
<%@ Import Namespace="System.Web.Mvc.Html" %>
<%@ Import Namespace="System.Web.Mvc" %>
<%@ Import Namespace="System.Security.Cryptography.X509Certificates" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title></title>
    <script src="<%= VirtualPathUtility.ToAbsolute("~/Scripts/jquery-3.7.1.js") %>"></script>
    <script src="<%= VirtualPathUtility.ToAbsolute("~/Scripts/jquery.form.js") %>"></script>
    <script src="<%= VirtualPathUtility.ToAbsolute("~/Scripts/FileSaver.min.js") %>"></script>
</head>
<body>
    <form id="theForm" method="post" enctype="multipart/form-data">
        <div>
            Find Certificate by Subject :
            <input type="text" name="Subject" />
            <br />
            Certificate Store Name :
                        <select name="CertStoreName">
                            <%  foreach (var item in Enum.GetValues(typeof(StoreName)))
                                {   %>
                            <option value="<%= (int)item %>"><%= item.ToString() %></option>
                            <%  }   %>
                        </select>
            <br />
            Certificate Store Location :
                        <select name="CertStoreLocation">
                            <%  foreach (var item in Enum.GetValues(typeof(StoreLocation)))
                                {   %>
                            <option value="<%= (int)item %>"><%= item.ToString() %></option>
                            <%  }   %>
                        </select>
            <br />
            Certificate List :
        <select name="CertName">
        </select>
            <button name="btnListCert" type="button" onclick="listCertificate();">List Certificates</button>
            <script>
                function listCertificate() {
                    var $select = $('select[name="CertName"]');
                    $select.empty();
                    var $formData = $('form').serializeObject();
                    $.post('<%= Url.Action("ListCertificate", "SignXml") %>', $formData, function (data) {
                        if ($.isPlainObject(data)) {
                            alert(data.message);
                        } else {
                            $(data).appendTo($select);
                        }
                    });
                }
            </script>
            <br />

            Certificate Store Password :
                        <input type="password" name="StorePass" />


            <br />
            CSP Name :
                        <input type="text" name="CspName" />


            <br />
            Preserve white space:<input type="checkbox" name="WhiteSpace" value="<%= true %>" />
            <br />
            Xml Context Upload :   
            <button name="btnUpload" type="button">Upload & Sign</button>
            <script>
                $(function () {
                    var $btn = $('button[name="btnUpload"]');
                    var $file = $('<input name="XmlFile" type="file" style="display: none;" />');
                    $btn.before($file);

                    $btn.click(function () {
                        $file.val('');
                        $file.click();
                    });

                    $file.on('change', function (event) {
                        uploadFile($file, $('#theForm').serializeObject(), '<%= Url.Action("Sign","SignXml") %>',
                            function (data) {
                                $btn.before($file);

                                if ($.isPlainObject(data)) {
                                    alert(data.message);
                                } else {
                                    var filename = "SignedContext.xml";

                                    var blob = new Blob([data], {
                                        type: "text/plain;charset=utf-8"
                                    });

                                    saveAs(blob, filename);
                                }
                            },
                            function () {
                                $btn.before($file);
                            });
                    });
                });
            </script>

            <br />
            Xml Context Upload : 
            &nbsp;
                    <button name="btnVerify" type="button">Upload & Verify</button>
            <script>
                $(function () {
                    var $btn = $('button[name="btnVerify"]');
                    var $file = $('<input name="XmlSigFile" type="file" style="display: none;" />');
                    $btn.before($file);

                    $btn.click(function () {
                        $file.val('');
                        $file.click();
                    });

                    $file.on('change', function (event) {
                        uploadFile($file, $('#theForm').serializeObject(), '<%= Url.Action("Verify","SignXml") %>',
                            function (data) {
                                $btn.before($file);
                                alert(data.message);
                            },
                            function () {
                                $btn.before($file);
                            });
                    });
                });
            </script>
            <br />
        </div>
    </form>
</body>
<script>
    $.fn.serializeObject = function () {
        var o = {};
        var a = this.serializeArray();
        $.each(a, function () {
            if (o[this.name] !== undefined) {
                if (!o[this.name].push) {
                    o[this.name] = [o[this.name]];
                }
                o[this.name].push(this.value || '');
            } else {
                o[this.name] = this.value || '';
            }
        });
        return o;
    };

    $.fn.launchDownload = function (url, params, target) {

        var data = this.serializeObject();
        if (params) {
            $.extend(data, params);
        }

        var form = $('<form></form>').attr('action', url).attr('method', 'post');//.attr('target', '_blank');
        if (target) {
            form.attr('target', target);
        }

        Object.keys(data).forEach(function (key) {
            var value = data[key];

            if (value instanceof Array) {
                value.forEach(function (v) {
                    form.append($("<input></input>").attr('type', 'hidden').attr('name', key).attr('value', v));
                });
            } else {
                form.append($("<input></input>").attr('type', 'hidden').attr('name', key).attr('value', value));
            }

        });

        //send request
        form.appendTo('body').submit().remove();
    };

    function uploadFile($file, postData, url, callback, errorback) {

        $('<form method="post" enctype="multipart/form-data"></form>')
            .append($file).ajaxForm({
                url: url,
                data: postData,
                beforeSubmit: function () {

                },
                success: function (data) {
                    callback(data);
                },
                error: function () {
                    errorback();
                }
            }).submit();
    }

</script>
</html>

