$(document).ready(function () {
    $('.table').dataTable(
        {
            "order": [2, 'desc']
        })
              .columnFilter({
                  aoColumns: [
                      null,
                      null,
                      null,
                      { type: "select", values: ['High', 'Medium', 'Low'] },
                      { type: "select", values: ['Open', 'Pending', 'Resolved', 'Closed'] },
                      { type: "select", values: ['Bug', 'Feature Request', 'Documentation'] },
                      null
                  ]

              });
});