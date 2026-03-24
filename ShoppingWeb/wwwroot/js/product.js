var dataTable;

$(document).ready(function () {
    loadDatatable();
});

function loadDatatable() {
    dataTable = $('#tblData').DataTable({
        ajax: {
            url: "/admin/product/getall",
            type: "GET",
            datatype: "json",
            dataSrc: "data"
        },
        columns: [
            { data: 'title', width: "20%" },
            { data: 'isbn', width: "15%" },
            { data: 'listPrice', width: "10%" },
            { data: 'author', width: "25%" },
            { data: 'category.categoryName', width: "20%" },
            {
                data: 'id',
                render: function (data) {
                    return `<div class="w-75 btn-group">
                                <a href="/Admin/Product/Upsert?id=${data}" class="btn btn-primary mx-2">
                                    Edit
                                </a>
                                <a onclick="Delete('/Admin/Product/Delete/${data}')"
                                   class="btn btn-danger mx-2">
                                    Delete
                                </a>
                            </div>`;
                },
                width: "10%"
            }
        ]
    });
}
