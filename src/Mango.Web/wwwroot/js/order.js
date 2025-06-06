let dataTable;

$(document).ready(function () {
	const urlParams = new URLSearchParams(window.location.search);
	const status = urlParams.get('status');
	loadDataTable(status);
})

function loadDataTable(status) {
	$('#tblData').DataTable({
		order: [[0, 'desc']],
		ajax: {url: "/order/getAll?status=" + status},
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