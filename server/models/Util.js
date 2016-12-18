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

module.exports.getCapacidade = function() {
	return new Promise(function (fulfill, reject) {
		request({
			url: primavera.url + '/api/definitions/capacidade',
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
			capacidade = JSON.parse(body);
			fulfill(capacidade);
		});
	});
}

module.exports.setCapacidade = function(capacidade) {
	return new Promise(function (fulfill, reject) {
		request({
			url: primavera.url + '/api/definitions/capacidade',
			headers: {
				'Authorization': primavera.auth,
				'Content-type': 'application/json'
			},
			method: 'post',
			body: JSON.stringify({capacidade: capacidade})
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

module.exports.getArmazemPrincipal = function() {
	return new Promise(function (fulfill, reject) {
		request({
			url: primavera.url + '/api/definitions/armazem-principal',
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
			armazem = JSON.parse(body);
			fulfill(armazem);
		});
	});
}

module.exports.setArmazemPrincipal = function(armazem) {
	return new Promise(function (fulfill, reject) {
		request({
			url: primavera.url + '/api/definitions/armazem-principal',
			headers: {
				'Authorization': primavera.auth,
				'Content-type': 'application/json'
			},
			method: 'post',
			body: JSON.stringify({armazem: armazem})
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

module.exports.getAvisos = function() {
	return new Promise(function (fulfill, reject) {
		request({
			url: primavera.url + '/api/avisos',
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
			avisos = JSON.parse(body);
			fulfill(avisos);
		});
	});
}

module.exports.getNumAvisos = function() {
	return new Promise(function (fulfill, reject) {
		request({
			url: primavera.url + '/api/avisos/existe',
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
			avisos = JSON.parse(body);
			fulfill(avisos);
		});
	});
}