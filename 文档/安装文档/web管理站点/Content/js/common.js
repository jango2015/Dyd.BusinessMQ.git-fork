$(function () {
    $("td[title]").dblclick(function () {
        var title = $(this).attr('title'); alert(title);
    });
    $("td[title]").each(function () {
        $(this).attr('title', $(this).attr('title') + "\r\n" + "【双击弹框】");
    });
    $("img[title]").each(function () {
        $(this).attr('title', "【帮助】" + $(this).attr('title') + "\r\n" + "【双击弹框】");
    });
    $("img[title]").dblclick(function () {
        var title = $(this).attr('title'); alert(title);
    });
});


function DeleteLog(url1) {
        if (confirm("确定清空日志吗？")) {
            $.ajax({
                url: url1,
                type: "post",
                data: {
                },
                success: function (data) {
                    if (data.code > 0) {
                        alert("成功！");
                        window.location = window.location;
                    }
                    else {
                        alert(data.msg);
                    }
                }
            })
        }
    }

