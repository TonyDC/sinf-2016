const db = require('../config/db');

module.exports.get = function (id) {
    return new Promise(function(fulfill, reject) {
		request('http://localhost:52313/api/artigo/' + id, function (error, response, body) {
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