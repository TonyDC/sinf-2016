const express = require('express');
const router = express.Router();
const SalesOrder = require('../../models/SalesOrder');


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

router.get('/partials/salesOrder/:id', function(req, res, next) {
   const id = req.params.id;

   SalesOrder.getItems(id).then(function (salesOrder) {
       res.render('partials/product-modal', {salesOrder:salesOrder, layout: false});
   })
});

module.exports = router;
