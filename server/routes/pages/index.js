const express = require('express');
const router = express.Router();
const SalesOrder = require('../../models/SalesOrder');
const PickingOrder = require('../../models/PickingOrder');

/**
 * PAGES INITIALIZATION
 */
router.get('/', function(req, res, next) {
    /*
    const salesOrders = [{date: "2016/11/23", client: "Joaquim Almeida"},
        {date: "2015/11/23", client: "Alberto Almeida"},
        {date: "2013/11/23", client: "Joaquim Martins"}];
        */

    SalesOrder.getAll().then(function(salesOrders) {
        res.render('index', {salesOrders: salesOrders});
    });
});

router.get('/status', function(req, res, next) {
    /*
     NOTA: status em percentagem

     const pickingOrders = [{status: "100", shipping-date: "2016/11/23", worker: "Joaquim"},
     {status: "60", shipping-date: "2015/11/23", worker: "Fernando"},
     {status: "50", shipping-date: "2013/11/23", worker: "Alice"}];
     */

    PickingOrder.getAll().then(function(pickingOrders) {
        res.render('status', {pickingOrders: pickingOrders});
    });
});

router.get('/shipping', function(req, res, next) {
    /*
     const salesOrders = [{status: "Expedida", shipping-date: "2016/11/23", shipping-guide: ""},
     {status: "Parcial", shipping-date: "2015/11/23", shipping-guide: ""},
     {status: "Expedida", shipping-date: "2013/11/23", shipping-guide: ""}];
     */

    SalesOrder.getAll().then(function(salesOrders) {
        res.render('shipping', {salesOrders: salesOrders});
    });
});

router.get('/worker', function(req, res, next) {
    PickingOrder.get(1).then(function(pickingOrder) {
        res.render('worker', {pickingOrder: pickingOrder});
    /*
     const pickingOrder = [{location: "ABCDEF", done: "false",
     items = [{name: "CPU", amount: "2", client: "Joaquim", final-amount:"1"},
     {name: "Motherboard", amount: "3", client: "Google", final-amount:"3"}]
     }];
     */

    //não sei o que pôr no id
    });
});

/**
 * REQUESTS
 */

router.get('/partials/salesOrder/:id', function(req, res, next) {
   const id = req.params.id;

   SalesOrder.getItems(id).then(function (salesOrder) {
       res.render('partials/product-modal', {salesOrder:salesOrder, layout: false});
   })
});

router.post('/createPickingWave', function(req, res, next) {
    if (!req.body.selected) {
        res.status(400).send('Erro ao gerar picking wave');
        return;
    }
    const ids = req.body.selected;
    res.send("Sucesso");
});

module.exports = router;
