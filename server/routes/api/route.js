const express = require('express');
const router = express.Router();
const Client = require('../../models/Client')
const SalesOrder = require('../../models/SalesOrder');
const Product = require('../../models/Product');
const PickingOrder = require('../../models/PickingOrder');


router.use(function (req, res, next) {
    res.setHeader('Content-Type', 'application/json');
    next();
});


router.get('/product/:id', function (req, res, next) {
    const id = req.params.id;

    Product.get(id).then(function (product) {
        res.json(product);
    });
});

router.get('client/:id', function (req, res, next) {
    const id = req.params.id;

    Client.get(id).then(function(client) {
        res.json(client);
    });
});

router.get('/sales-orders', function (req, res, next) {
    SalesOrder.getAll().then(function (salesOrders) {
        res.json({'sales': salesOrders});
    });
});

router.get('/sales-orders/:id', function (req, res, next) {
    const id = req.params.id;

    SalesOrder.getItems(id).then(function (items){
        res.json({'items': items});
    });
});


router.get('/picking', function (req, res, next) {
    PickingOrder.getAll().then(function (pickingOrders) {
        res.json({'pickings': pickingOrders});
    });
});

router.get('/picking/:id', function (req, res, next) {
    const id = req.params.id;

    PickingOrder.get(id).then(function(pickingOrders){
        res.json({'pickings': pickingOrders});
    });
});

router.post('/picking', function (req, res, next) {
    if (!req.body.selection) {
        res.json({err: 'Missing selection'});
        return;
    }
    const selection = req.body.selection;

    PickingOrder.generate(selection).then(function (pickingOrders) {
        res.json({'pickings': pickingOrders});
    });
});

router.get('/picking/employee/:id', function(req, res, next) {
    const id = req.params.id;

    PickingOrder.getAssignedToEmployee(id).then(function (pickingOrders) {
        req.json({orders: pickingOrders});
    })
});

router.post('/picking/:id', function(req, res, next) {
    if (!req.body.item) {

    }
    if (!req.body.quantity) {

    }
    const id = req.params.id;
    const item = req.body.item;
    const quantity = req.body.quantity;

    PickingOrder.pick(id, item, quantity).then(function () {
       res.end();
    });
});

router.post('/picking/:id/finish', function(req, res, next) {
   const id = req.params.id;

   PickingOrder.finish(id).then(function () {
       res.end();
   })
});

router.post('/sales-orders/:id/ship', function(req, res, next) {
    const id = req.params.id;

    SalesOrder.ship(id).then(function (document) {
        res.json({document: document});
    })
})

module.exports = router;