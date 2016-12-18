const request = require('request');
const primavera = require('../config/primavera');

module.exports.getAll = function() {
	return new Promise(function (fulfill, reject) {
		request({
			url: primavera.url + '/api/Armazem',
			headers: {
				'Authorization': primavera.auth
			}
		}, function (error, response, body) {
			if(error){
        		reject('Error:' + error);
				return;
    		}
			if(response.statusCode !== 200){
        		reject('Invalid Status Code Returned:' + response.statusCode);
				return;
	    	}
			armazens = JSON.parse(body);
			fulfill(armazens);
		});
	});
}