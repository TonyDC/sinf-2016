const request = require('request');
const primavera = require('../config/primavera');

module.exports.login = function(user, pass) {
	return new Promise(function (fulfill, reject) {
		request({
			url: primavera.url + '/api/auth/login',
			headers: {
				'Authorization': primavera.auth,
				'Content-Type': 'application/json'
			},
			method: 'post',
			body: JSON.stringify({
				username: user,
				password: pass
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
			userData = JSON.parse(body);
			fulfill(userData);
		});
	});
}

module.exports.getAll = function() {
	return new Promise(function (fulfill, reject) {
		request({
			url: primavera.url + '/api/auth/users',
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
			usersRaw = JSON.parse(body);
			users = usersRaw.map(function(user) {
				user.type = (user.type == 1 ? 'Gerente' : 'Funcionário');
				return user;
			});
			fulfill(users);
		});
	});
}

module.exports.createAdmin = function() {
	return new Promise(function (fulfill, reject) {
		request({
			url: primavera.url + '/api/auth/users',
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
			usersRaw = JSON.parse(body);
			users = usersRaw.map(function(user) {
				user.type = (user.type == 1 ? 'Gerente' : 'Funcionário');
				return user;
			});
			fulfill(users);
		});
	});
}

module.exports.createWorker = function() {
	return new Promise(function (fulfill, reject) {
		request({
			url: primavera.url + '/api/auth/users',
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
			usersRaw = JSON.parse(body);
			users = usersRaw.map(function(user) {
				user.type = (user.type == 1 ? 'Gerente' : 'Funcionário');
				return user;
			});
			fulfill(users);
		});
	});
}