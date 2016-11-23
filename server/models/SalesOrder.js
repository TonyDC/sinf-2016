const db = require('../config/db');

module.exports.getAll = function () {
    return new Promise(function (fulfill, reject) {
        let sales = [];
        const sale1 = {'id': '1', 'shipping-date': '11/12/2016', 'client': 'Joaquim Almeida', 'client-id': '2'};
        const sale2 = {'id': '2', 'shipping-date': '15/12/2016', 'client': 'Joaquim Martins', 'client-id': '2'};
        sales.push(sale1);
        sales.push(sale2);

        fulfill(sales);
    });
};

module.exports.getItems = function (id) {
    return new Promise(function (fulfill, reject) {
        let items = [];
        items.push({item: '1', quantity: 2, name: 'CPU', status: 'ready'});
        items.push({item: '2', quantity: 4, name: 'Motherboard',  status: 'not ready'});
        const salesOrder = {'id': '1', 'shipping-date': '11/12/2016', 'client': 'Joaquim Almeida', 'client-id': '2', items: items};

        fulfill(salesOrder);
    });
};

module.exports.ship = function(id) {
  return new Promise(function (fulfill, reject) {
      const document = 'shippingOrder123e.pdf';

      fulfill(document);
  });
};