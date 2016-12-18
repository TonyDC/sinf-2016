const request = require('request');
const primavera = require('../config/primavera');

module.exports.get = function (id) {
    return new Promise(function(fulfill, reject) {
		request({url: primavera.url + '/api/artigo/' + id,
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
			
			fulfill({id, name:itemRaw.DescArtigo});
		});
	});
};