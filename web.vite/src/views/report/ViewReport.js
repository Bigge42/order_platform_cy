function getAllUrlParams() {
    var arr = window.location.search.substr(1).split('#')[0].split("&"),
        Params = {},
        i;

    for (i = 0; i < arr.length; i++) {
        Params[arr[i].split("=")[0]] = arr[i].split("=")[1];
    }

    return Params;
}

function onReportLoaded(viewer) {
    var report = viewer.report,
        masterSQL = report.QuerySQL,
        detailSQL = report.DetailGrid ? report.DetailGrid.Recordset.QuerySQL : "",
        url = "../../data/SQLParam.ashx?QuerySQL=";

    if (!masterSQL && detailSQL) {
        masterSQL = detailSQL;
        detailSQL = "";
    }
    if (masterSQL) {
        url += encodeURIComponent(masterSQL)
        if (detailSQL) {
            url += "&QuerySQL2=" + encodeURIComponent(detailSQL);
        }

        window.rubylong.ajax(url, function (xmlhttp, success) {
            this.loadData(xmlhttp, success, 1);
        }, viewer);

        viewer.waitData = true; //阻止在报表模板定义后直接生成报表
    }
}

function runReport() {
    var params = getAllUrlParams(),
        reportid = params["report"],
        dataid = params["data"],
        reportURL = "../../grf/" + reportid + ".grf",
        dataURL,
        reportViewer;

    if (dataid) { //params.hasOwnProperty("data")) {
        dataURL = "../../data/DataCenter.ashx?";

        if (params.hasOwnProperty("report")) {
            delete params["report"];
        }

        for (var key in params) {
            var item = params[key];
            dataURL = dataURL + key + "=" + params[key] + "&"
        }

        dataURL = dataURL.slice(0, -1)
    }

    reportViewer = window.rubylong.grhtml5.insertReportViewer("report_holder", reportURL, dataURL);

    if (!dataURL) {
        //如果没有指定报表数据参数，则要从报表模板中设置的查询SQL中获取报表数据
        //此项任务只能在 onReportLoaded 事件中处理
        reportViewer.events.onReportLoaded = onReportLoaded;
    }

    reportViewer.start(); //启动报表运行，生成报表

    //用于记录辅助性的参数
    reportViewer.run_args = {
        reportid: reportid,
        dataid: dataid,
    };

    return reportViewer;
}

function prepareWebApp(viewer) {
    var path = window.location.pathname,
        index = path.lastIndexOf("/");

    webapp_urlprotocol_startup(); //启动WEB报表客户端程序，以便侦听接受 WebSocket 数据。这句特别重要*：只有在WEB报表客户端程序后，才能进行WebSocket通讯

    //应该还有服务器端确定的方式
    window.rootURL = window.location.protocol + "//" + window.location.host;
    if (index >= 0) {
        path = path.substr(0, index);

        //demmo的根目录在当前页面的2级目录之上
        index = path.lastIndexOf("/");
        if (index >= 0) {
            path = path.substr(0, index);

            index = path.lastIndexOf("/");
            if (index >= 0) {
                path = path.substr(0, index);
            }
        }

        window.rootURL += path;
    }
    window.rootURL += "/";
}