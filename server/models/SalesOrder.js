const db = require('../config/db');
const request = require('request');


module.exports.getAll = function () {
    return new Promise(function (fulfill, reject) {
        let sales = [];
        const sale1 = {'id': '1', 'shipping-date': '11/12/2016', 'client': 'Joaquim Almeida', 'client-id': '2'};
        const sale2 = {'id': '2', 'shipping-date': '15/12/2016', 'client': 'Joaquim Martins', 'client-id': '2'};
        sales.push(sale1);
        sales.push(sale2);

	request('http://localhost:52313/api/encomenda/000/2016', function (error, response, body) {
    		if(error){
        		return console.log('Error:', error);
    		}
		if(response.statusCode !== 200){
        		return console.log('Invalid Status Code Returned:', response.statusCode);
	    	}

		salesOrdersRaw = JSON.parse(body);
		salesOrders = []
		
		
		salesOrdersRaw.forEach(function(order) {
			salesOrders.push({id: order.NumeroDocumento, 'shipping-date': order.Artigos[0].DataEntrega, client: order.Client});
		});

		fulfill(salesOrders);

	});
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