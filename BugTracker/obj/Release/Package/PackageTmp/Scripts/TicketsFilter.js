$(document).ready(function () {
    $('.table').dataTable()
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