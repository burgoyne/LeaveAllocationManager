$(document).ready(function () {
    $('#tbl-data').DataTable();

    $(function () {
        $(".datepicker").datepicker({
            dateFormat: "yy-mm-dd"
        });
    });
});