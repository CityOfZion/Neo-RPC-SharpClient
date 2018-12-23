// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
$(document).ready(function () {
    $("#validateAddressButton").click(function () {
        var input = $("#validateAddressInput").val();
        $.ajax({
            url: 'RpcClient/ValidateAddress',
            type: "post",
            data: {
                input: input
            }, //if you need to post Model data, use this
            success: function (result) {
                $("#validateAddressResult").text("Is Valid: " + result);
            }
        });
    });


    $("#getBestBlockHash").click(function () {
        $.ajax({
            url: 'RpcClient/GetBestBlockHash',
            type: "post",
            success: function (result) {
                $("#getBestBlockHashResult").text(result);
            }
        });
    });
});