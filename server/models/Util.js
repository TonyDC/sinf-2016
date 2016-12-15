const primavera = require('../config/primavera');
const request = require('request');

module.exports.getSeries = function() {
	return new Promise(function (fulfill, reject) {
		request({
			url: primavera.url + '/api/util/series',
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
			series = JSON.parse(body);
			fulfill(series);
		});
	});
}

module.exports.getFiliais = function() {
	return new Promise(function (fulfill, reject) {
		request({
			url: primavera.url + '/api/util/filiais',
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
			filiais = JSON.parse(body);
			fulfill(filiais);
		});
	});
}