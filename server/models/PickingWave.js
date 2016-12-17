const primavera = require('../config/primavera');
const request = require('request');
const Product = require('./Product');

module.exports.getAllPicking = function (completed) {
     return new Promise(function (fulfill, reject) {
		request({
			url: primavera.url + '/api/wave/status/picking/' ,
			headers: {
				'Authorization': primavera.auth
			}
		}, function (error, response, body) {
			if(error){
        		reject('Error:' + error);
				return;
    		}
			if (response.statusCode == 204) {
				fulfill([]);
				return;
			}
			if(response.statusCode !== 200){
        		reject('Invalid Status Code Returned:' + response.statusCode);
				return;
	    	}
			waves = JSON.parse(body);
			fulfill(waves);
		});
	});
};

module.exports.getAllReplenishment = function (completed) {
     return new Promise(function (fulfill, reject) {
		request({
			url: primavera.url + '/api/wave/status/replenishment/' ,
			headers: {
				'Authorization': primavera.auth
			}
		}, function (error, response, body) {
			if(error){
        		reject('Error:' + error);
				return;
    		}
			if (response.statusCode == 204) {
				fulfill([]);
				return;
			}
			if(response.statusCode !== 200){
        		reject('Invalid Status Code Returned:' + response.statusCode);
				return;
	    	}
			waves = JSON.parse(body);
			fulfill(waves);
		});
	});
};

module.exports.getAssignedToEmployee = function (employeeId) {
    return new Promise(function (fulfill, reject) {
		request({
			url: primavera.url + '/api/wave/assign?funcionario=' + employeeId,
			headers: {
				'Authorization': primavera.auth
			}
		}, function (error, response, body) {
			if(error){
        		reject('Error:' + error);
				return;
    		}
			if(response.statusCode == 204) {
				fulfill();
				return;
			}
			if(response.statusCode !== 200){
        		reject('Invalid Status Code Returned:' + response.statusCode);
				return;
	    	}
			task = JSON.parse(body);
			fulfill(task);
		});
	});
};

module.exports.generate = function(serie, filial, selection) {
    return new Promise(function (fulfill, reject) {
        request({
			url: primavera.url + '/api/wave/generate',
			headers: {
				'Authorization': primavera.auth,
				'Content-Type': 'application/json'
			},
			method: 'post',
			body: JSON.stringify({
				filial: filial,
				serie: serie,
				encomendas: selection
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
};

module.exports.finishPicking = function(funcionario, wave, linhas) {
    return new Promise(function (fulfill, reject) {
        request({
			url: primavera.url + '/api/wave/terminate/picking',
			headers: {
				'Authorization': primavera.auth,
				'Content-Type': 'application/json'
			},
			method: 'post',
			body: JSON.stringify({
				funcionario: funcionario,
				wave: wave,
				linhas: linhas
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
};

module.exports.finishReplenishment = function(funcionario, wave, linhas) {
    return new Promise(function (fulfill, reject) {
        request({
			url: primavera.url + '/api/wave/terminate/replenishment',
			headers: {
				'Authorization': primavera.auth,
				'Content-Type': 'application/json'
			},
			method: 'post',
			body: JSON.stringify({
				funcionario: funcionario,
				wave: wave,
				linhas: linhas
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
};