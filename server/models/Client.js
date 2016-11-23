const db = require('../config/db');

module.exports.get = function (id) {
    return new Promise(function (fulfill, reject) {
        const client = {
            id: 4,
            name: 'José Manuel',
            address: 'Rua do Porto',
            code: '4456-222',
            location: 'Porto',
            country: 'Portugal'
        };

        fulfill(client);
    });
};