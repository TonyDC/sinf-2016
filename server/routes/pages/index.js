const express = require('express');
const router = express.Router();
const SalesOrder = require('../../models/SalesOrder');
const PickingOrder = require('../../models/PickingOrder');

/**
 * PAGES INITIALIZATION
 */
router.get('/', function(req, res, next) {

    res.render('index');

});

router.get('/choice', function(req, res, next) {

    const filiais = [{name: "filial1"},{name: "filial2"},{name: "filial2"}];
    const series = [{name: "serie1"},{name: "serie2"},{name: "serie3"}];
    res.render('choice', {filiais: filiais, series: series});

});

router.get('/options', function(req, res, next) {

    const variables = {warehouse: "300", worker: "100"};
    res.render('options', {variables: variables});

});

router.get('/creation', function(req, res, next) {

    const salesOrders = [{id: "1", shippingDate: "2016/11/23", client: "Joaquim Almeida"},
        {id: "2", shippingDate: "2015/11/23", client: "Alberto Almeida"},
        {id: "3", shippingDate: "2013/11/23", client: "Joaquim Martins"}];
    res.render('creation', {salesOrders: salesOrders});

/*shippingDate:
    SalesOrder.getAll().then(function(salesOrders) {
        res.render('index', {salesOrders: salesOrders});
    });
    */
});

router.get('/status', function(req, res, next) {

     //NOTA: status em percentagem
     const pickingOrders = [{status: "100", shippingDate: "2016/11/23", worker: "Joaquim"},
     {status: "60", shippingDate: "2015/11/23", worker: "Fernando"},
     {status: "50", shippingDate: "2013/11/23", worker: "Alice"}];
    res.render('status', {pickingOrders: pickingOrders});
/*
    PickingOrder.getAll().then(function(pickingOrders) {
        res.render('status', {pickingOrders: pickingOrders});
    });
    */
});

router.get('/shipping', function(req, res, next) {

     const salesOrders = [{status: "Expedida", shippingDate: "2016/11/23", shippingguide: ""},
     {status: "Parcial", shippingDate: "2015/11/23", shippingGuide: ""},
     {status: "Expedida", shippingDate: "2013/11/23", shippingGuide: ""}];
    res.render('shipping', {salesOrders: salesOrders});
	/*
    SalesOrder.getAllToShip().then(function(salesOrders) {
		res.render('shipping', {salesOrders: salesOrders});
	});
	*/
});

router.get('/worker', function(req, res, next) {

     const pickingOrder = {steps:
         [
             {location: "ABCDEF", done: "false", items:
                 [
                 {name: "CPU", quantity: "2", client: "Joaquim", finalAmount:"1"},
                    {name: "Motherboard", quantity: "3", client: "Google", finalAmount:"3"}
                 ]
             },
             {location: "GHIJKL", done: "false", items:
                 [
                     {name: "dfgsgsd", quantity: "1", client: "Apple", finalAmount:"1"},
                     {name: "gjhgks", quantity: "5", client: "Microsoft", finalAmount:"5"}
                 ]
             }
         ]
     };

    res.render('worker', {pickingOrder: pickingOrder});

    //FALTA PÔR ID
    /*
    PickingOrder.get(1).then(function(pickingOrder) {
        res.render('worker', {pickingOrder: pickingOrder});
    });
    */
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
