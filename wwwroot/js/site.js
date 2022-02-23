// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function kek() {
    var connection = new signalR.HubConnectionBuilder().withUrl("/signalServer").build();

    connection.on("displayNotification", function (message) {
        debugger;
        $('#liveToast').addClass('toast show');
        $('#liveToast').toast({ delay: 5000, animation: false });
        $('#liveToastBody').text = message;
        
    });

    connection.start().catch(function (err) {
        return console.error(err.toString());
    });
}

kek();



