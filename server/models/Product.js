const db = require('../config/db');

module.exports.get = function (id) {
    return new Promise(function (fulfill, reject) {
        const product = {
            id: 1,
            name: 'hammer',
            stock: '200',
            location: 'A.01.12'
        };

        fulfill(product);
    });
};