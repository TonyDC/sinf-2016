const express = require('express');
const router = express.Router();
const PickingOrder = require('../../models/PickingOrder');


router.get('/', function(req, res, next) {
    /*
    const salesOrders = [{date: "2016/11/23", client: "Joaquim Almeida"},
        {date: "2015/11/23", client: "Alberto Almeida"},
        {date: "2013/11/23", client: "Joaquim Martins"}];
        */

    PickingOrder.getAll().then(function(pickingOrders) {
        res.render('status', {pickingOrders: pickingOrders});
    });
});

module.exports = router;
