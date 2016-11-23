const db = require('../config/db');

module.exports.getAll = function () {
    return new Promise(function (fulfill, reject) {
        const pickingOrders = ['1', '2', '3'];
        fulfill(pickingOrders);
    });
};

module.exports.get = function (id) {
    return new Promise(function (fulfill, reject) {
        const pickingOrder = {
            'sales': [
                {
                    'id': '2',
                    'items': [
                        {
                            'id': '1',
                            'requested': 2,
                            'picked': 1,
                            'status': 'picked'
                        },
                        {
                            'id': '32',
                            'requested': 1,
                            'picked': 0,
                            'status': 'not picked'
                        }
                    ]
                }
            ]
        };

        fulfill(pickingOrder);
    });
};

module.exports.getAssignedToEmployee = function (employeeId) {
    return new Promise(function (fulfill, reject) {
       const assignedOrders = ['1', '3'];

       fulfill(assignedOrders);
    });
};

module.exports.generate = function(selection) {
    return new Promise(function (fulfill, reject) {
        const pickingOrders = ['3', '4', '5'];

        fulfill(pickingOrders);
    });
};

module.exports.pick = function(id, item, quantity) {
    return new Promise(function (fulfill, reject) {
        fulfill();
    })
};

module.exports.finish = function(id) {
    return new Promise(function(fulfill, reject){
        fulfill();
    })
};