let dataTable;

$(document).ready(function () {
	loadDataTable();
})

function loadDataTable() {
	$('#tblData').DataTable({
		ajax: {url: "/order/getAll"},
		columns: [
			{data: 'orderHeaderId', "width": "5%"},
			{data: 'email', "width": "25%"},
			{
				data: null,
				render: function (data, type, row) {
					return `${row.firstName} ${row.lastName}`;
				},
				width: "20%"
			},
			{data: 'phone', "width": "10%"},
			{data: 'status', "width": "10%"},
			{data: 'orderTotal', "width": "10%"},
			{
				data: 'orderHeaderId',
				render: function (data, type, rowIndex) {
					return `<div class="w-75 btn-group" role="group">
								<a href="/order/orderDetail?orderId=${data}" class="btn btn-primary mx-2">
									<i class="bi bi-pencil-square"></i>
								</a>
							</div>`
				}
			},
		]
	});
}