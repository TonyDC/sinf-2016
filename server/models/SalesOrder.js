const db = require('../config/db');
const request = require('request');


module.exports.getAll = function () {
    return new Promise(function (fulfill, reject) {
	request('http://localhost:52313/api/encomenda/000/2016', function (error, response, body) {
    		if(error){
        		return console.log('Error:', error);
    		}
		if(response.statusCode !== 200){
        		return console.log('Invalid Status Code Returned:', response.statusCode);
	    	}

		salesOrdersRaw = JSON.parse(body);
		salesOrders = salesOrdersRaw.map(function(order) {
			return {id: order.NumeroDocumento, 'shipping-date': order.Artigos[0].DataEntrega, client: order.Cliente};
		});

		fulfill(salesOrders);
	});
    });
};

module.exports.getItems = function (id) {
    return new Promise(function (fulfill, reject) {
		request('http://localhost:52313/api/encomenda/000/2016/' + id, function (error, response, body) {
			if(error){
				console.log('Error:', error);
				reject();
			}
			if(response.statusCode !== 200){
				console.log('Invalid Status Code Returned:', response.statusCode);
				reject();
			}

			salesOrderRaw = JSON.parse(body)[0];

			itemInfoPromises = salesOrderRaw.Artigos.map(function(item) {
				return new Promise(function(fulfill, reject) {
					request('http://localhost:52313/api/artigo/' + item.ArtigoID, function (error, response, body) {
							if(error){
							console.log('Error:', error);
							reject();
							}
						if(response.statusCode !== 200){
							console.log('Invalid Status Code Returned:', response.statusCode);
							reject();
							}				
						
						itemRaw = JSON.parse(body);
						
						fulfill({name:itemRaw.DescArtigo, quantity:item.Quantidade - item.QuantidadeSatisfeita});
					});
				});
			});
			
			Promise.all(itemInfoPromises).then(function(items) {
				fulfill({id: id, client:salesOrderRaw.Cliente, items: items});
			});
		});
    });
};

module.exports.ship = function(id) {
  return new Promise(function (fulfill, reject) {
      const document = 'shippingOrder123e.pdf';

      fulfill(document);
  });
};