// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function kek() {
    var connection = new signalR.HubConnectionBuilder().withUrl("/signalServer").build();

    connection.on("displayNotification", function () {
        debugger;

        $.ajax({
            type: 'GET',
            url: '/Notifications/GetNewNotifications',
            success: function (response) {
                $.each(response, function (index, value) {
                    $('#liveToast').addClass('toast show');
                    $('#liveToastBody').text(value.text);
                })
            },
            error: function (error) {
                console.error(error);
            }
        })
    });

    connection.start().catch(function (err) {
        return console.error(err.toString());
    });
}

kek();




