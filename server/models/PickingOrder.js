const db = require('../config/db');
const Product = require('./Product');

module.exports.getAll = function () {
    return new Promise(function (fulfill, reject) {
		db.pool.query('SELECT picking_order.id, nome, terminado, data_expedicao FROM picking_order JOIN utilizador ON picking_order.id_funcionario = utilizador.id;', function(err, rows) {
			const pickingOrders = (rows.map(function(row) { return {id: row.id, status: 10, shippingDate: row.data_expedicao, worker: row.nome  };}));
			fulfill(pickingOrders);
		});
    });
};

module.exports.get = function (id) {
    return new Promise(function (fulfill, reject) {
		db.pool.query('SELECT id, id_localizacao FROM picking_order_step WHERE id_picking_order = ?', [id], function(err, rows) {	
			stepPromises = rows.map(function(row) {
				return new Promise(function(fulfill, reject) {
					db.pool.query('SELECT id_artigo, quantidade_pedida FROM picking_order_item WHERE id_picking_order_step = ?', [row.id], function(err, rows) {
						console.log(JSON.stringify(rows));
						items = rows.map(function(row) { return {quantity: row.quantidade_pedida, name: row.id_artigo}; });
						fulfill({id: row.id, location: row.id_localizacao, items: items});
					});
				});
			});
			
			Promise.all(stepPromises).then(function(steps) {
				fulfill({id: id, steps: steps});
			});
		});
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