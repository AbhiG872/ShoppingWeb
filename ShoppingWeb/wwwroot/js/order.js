var dataTable;

$(function () {
    var url = window.location.search;
    if (url.includes("inprocess")) {
        loadDatatable("inprocess");
    } else if (url.includes("completed")) {
        loadDatatable("completed");
    } else if (url.includes("pending")) {
        loadDatatable("pending");
    } else if (url.includes("approved")) {
        loadDatatable("approved");
    } else {
        loadDatatable("all");
    }   
});

function loadDatatable(status) {

    // 🔹 Destroy previous instance safely
    if ($.fn.DataTable.isDataTable('#tblData')) {
        $('#tblData').DataTable().clear().destroy();
    }

    dataTable = $('#tblData').DataTable({
        ajax: {
            url: "/Admin/Order/getall?status=" + status,
            type: "GET",
            dataType: "json",

            // 🔹 Handle API response
            dataSrc: function (json) {
                console.log("API Response:", json);
                return json?.data || [];
            },

            // 🔹 Error handling
            error: function (xhr, error) {
                console.error("AJAX Error:", {
                    status: xhr.status,
                    error: error,
                    response: xhr.responseText
                });
            }
        },

        columns: [
            { data: 'id' },

            {
                data: null,
                render: function (data) {
                    return data.applicationUser?.name || '';
                }
            },

            { data: 'phoneNumber' },

            {
                data: null,
                render: function (data) {
                    return data.applicationUser?.email || '';
                }
            },

            {
                data: 'orderStatus',
                render: function (data) {

                    if (!data) return '';

                    let badgeClass = '';

                    switch (data.toLowerCase()) {
                        case 'pending':
                            badgeClass = 'bg-warning text-dark';
                            break;

                        case 'approved':
                            badgeClass = 'bg-success';
                            break;

                        case 'rejected':
                            badgeClass = 'bg-danger';
                            break;

                        default:
                            badgeClass = 'bg-secondary';
                    }

                    return `<span class="badge ${badgeClass}">${data}</span>`;
                }
            },

            { data: 'orderTotal' },

            {
                data: 'id',
                render: function (data) {
                    return `<a href="/admin/order/details?orderId=${data}" class="btn btn-primary btn-sm">Details</a>`;
                }
            }
        ],

        // 🔹 UI options
        paging: true,
        searching: true,
        ordering: true,
        responsive: true,
        autoWidth: false
    });
}
// 🔍 Custom search
$('#customSearch').on('keyup', function () {
    dataTable.search(this.value).draw();
});

// 🔽 Status filter
$('#statusFilter').on('change', function () {
    var value = $(this).val();
    dataTable.column(4).search(value).draw();
});