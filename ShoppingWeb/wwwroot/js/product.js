var dataTable;

$(function () {
    console.log("Product JS Loaded");
    loadDatatable();
});

function loadDatatable() {

    if ($.fn.DataTable.isDataTable('#tblData')) {
        $('#tblData').DataTable().clear().destroy();
    }

    dataTable = $('#tblData').DataTable({
        ajax: {
            url: "/Admin/Product/getall",   // ✅ fixed
            type: "GET",
            dataType: "json",

            dataSrc: function (json) {
                console.log("API Response:", json);
                return json?.data || [];
            },

            error: function (xhr, error) {
                console.error("AJAX Error:", xhr.responseText);
            }
        },

        columns: [

            { data: 'title' },

            { data: 'isbn' },

            { data: 'listPrice' },

            { data: 'author' },

            {
                data: null,
                render: function (data) {
                    return data.category?.categoryName || '';
                }
            }
        ],

        paging: true,
        searching: true,
        ordering: true,
        responsive: true,
        autoWidth: false
    });
}