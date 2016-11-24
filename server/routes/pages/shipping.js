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
        res.render('shipping', {salesOrders: salesOrders});
    });
});

router.get('/status', function(req, res, next) {
    res.render('status');
});

router.get('/', function(req, res, next) {
    res.render('index');
});

module.exports = router;
