const primavera = require('../config/primavera');
const request = require('request');
const Product = require('./Product');

module.exports.getAll = function (serie, filial) {
    return new Promise(function (fulfill, reject) {
		request({
			url: primavera.url + '/api/encomenda/' + filial + '/' + serie,
			headers: {
				'Authorization': primavera.auth
			}}, function (error, response, body) {
				if(error){
					reject('Error:', error);
					return;
				}
				if(response.statusCode == 204) {
					fulfill([]);
					return;
				}
				if(response.statusCode !== 200){
					reject('Invalid Status Code Returned:', response.statusCode);
					return;
				}

				salesOrdersRaw = JSON.parse(body);
				salesOrders = salesOrdersRaw.filter(function(order) {
					for(let i = 0; i < order.Artigos.length; ++i) {
						const item = order.Artigos[i];
						if (item.Quantidade - item.QuantidadeSatisfeita > 0) {
							return true;
						}
					}
					return false;
				}).map(function(order) {
					order.DataMinimaEncomenda = order.DataMinimaEncomenda.substr(0,10);
					return order;
				});

				fulfill(salesOrders);
			}
		);
	});
}

module.exports.getItems = function (serie, filial, id) {
    return new Promise(function (fulfill, reject) {
		request({
			url: primavera.url + '/api/encomenda/' + filial + '/' + serie + '/' + id,
			headers: {
				'Authorization': primavera.auth
			}}, function (error, response, body) {
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
					request({url: primavera.url + '/api/artigo/' + item.ArtigoID,
			headers: {
				'Authorization': primavera.auth
			}}, function (error, response, body) {
							if(error){
							console.log('Error:', error);
							reject();
							}
						if(response.statusCode !== 200){
							console.log('Invalid Status Code Returned:', response.statusCode);
							reject();
							}				
						
						itemRaw = JSON.parse(body);
						
						fulfill({id: itemRaw.CodArtigo, units: itemRaw.UnidadeVenda, name:itemRaw.DescArtigo, quantity:item.Quantidade, satisfied: item.QuantidadeSatisfeita});
					});
				});
			});
			
			Promise.all(itemInfoPromises).then(function(items) {
				fulfill({id: id, client:salesOrderRaw.Cliente, items: items});
			});
		});
    });
};

module.exports.getAllToShip = function(serie, filial) {
	return new Promise(function (fulfill, reject) {
		request({
			url: primavera.url + '/api/encomenda-pronto/' + filial + '/' + serie,
			headers: {
				'Authorization': primavera.auth
			}}, function (error, response, body) {
				if(error){
					return console.log('Error:', error);
				}if(response.statusCode == 204) {
					fulfill([]);
					return;
				}
				if(response.statusCode !== 200){
					return console.log('Invalid Status Code Returned:', response.statusCode);
				}
				salesOrdersRaw = JSON.parse(body);
				salesOrders = salesOrdersRaw.map(function(order) {
					order.DataMinimaEncomenda = order.DataMinimaEncomenda.substr(0,10);
					for(let i = 0; i < order.Artigos.length; ++i) {
						const item = order.Artigos[i];
						if (item.Quantidade - item.QuantidadeSatisfeita > 0) {
							order.ready = false;
							return order;
						}
					}
					order.ready = true;
					return order;
				});
				fulfill(salesOrders);
			}
		);
	});
}

module.exports.getItemsToShip = function (serie, filial, id) {
    return new Promise(function (fulfill, reject) {
		request({
			url: primavera.url + '/api/encomenda-pronto/' + filial + '/' + serie + '/' + id,
			headers: {
				'Authorization': primavera.auth
			}}, function (error, response, body) {
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
					request({url: primavera.url + '/api/artigo/' + item.ArtigoID,
			headers: {
				'Authorization': primavera.auth
			}}, function (error, response, body) {
							if(error){
							console.log('Error:', error);
							reject();
							}
						if(response.statusCode !== 200){
							console.log('Invalid Status Code Returned:', response.statusCode);
							reject();
							}				
						
						itemRaw = JSON.parse(body);
						
						fulfill({id: itemRaw.CodArtigo, units: itemRaw.UnidadeVenda, name:itemRaw.DescArtigo, quantity:item.Quantidade, satisfied: item.QuantidadeSatisfeita});
					});
				});
			});
			
			Promise.all(itemInfoPromises).then(function(items) {
				fulfill({id: id, client:salesOrderRaw.Cliente, items: items});
			});
		});
    });
};

module.exports.ship = function(serie, filial, id) {
	return new Promise(function (fulfill, reject) {
        request({
			url: primavera.url + '/api/encomenda',
			headers: {
				'Authorization': primavera.auth,
				'Content-Type': 'application/json'
			},
			method: 'post',
			body: JSON.stringify({
				filial: filial,
				serie: serie,
				nDoc: id
			})
		}, function (error, response, body) {
			if(error){
        		reject('Error:' + error);
				return;
    		}
			if(response.statusCode !== 200){
        		reject('Invalid Status Code Returned:' + response.statusCode);
				return;
	    	}
			fulfill();
		});
    });
}