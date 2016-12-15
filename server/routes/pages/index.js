const express = require('express');
const router = express.Router();
const SalesOrder = require('../../models/SalesOrder');
const PickingOrder = require('../../models/PickingOrder');

/**
 * PAGES INITIALIZATION
 */
function checkLogin(req, res, next) {
    if(!req.session.userId) {
        res.redirect('/');
        return;
    }
    next();
}

function checkAdmin(req, res, next) {
    if(!req.session.admin) {
        res.redirect('/');
        return;
    }
    next();
}

function checkWorker(req, res, next) {
    if(req.session.admin) {
        res.redirect('/');
        return;
    }
    next();
}

router.get('/', function(req, res, next) {

    if(req.session.userId) {
        if(req.session.admin) {
            res.redirect('/choice');
            return;
        }
        res.redirect('/worker');
        return;
    }

    res.render('index');

});

router.get('/choice', checkLogin, checkAdmin, function(req, res, next) {

    const filiais = [{name: "filial1"},{name: "filial2"},{name: "filial2"}];
    const series = [{name: "serie1"},{name: "serie2"},{name: "serie3"}];
    res.render('choice', {filiais: filiais, series: series});

});

router.get('/creation', checkLogin, checkAdmin, function(req, res, next) {

    const required_params = ['serie', 'filial'];

    console.log("Serie: " + req.query.serie);
    console.log("Filial: " + req.query.filial);
    const salesOrders = [{id: "1", shippingDate: "2016/11/23", client: "Joaquim Almeida"},
        {id: "2", shippingDate: "2015/11/23", client: "Alberto Almeida"},
        {id: "3", shippingDate: "2013/11/23", client: "Joaquim Martins"}];
    res.render('creation', {salesOrders: salesOrders});
});
/*
shippingDate:
    SalesOrder.getAll().then(function(salesOrders) {
        res.render('index', {salesOrders: salesOrders});
 });
        */

router.get('/options', checkLogin, checkAdmin, function(req, res, next) {
 const variables = {worker: "100"};
 res.render('options', {variables: variables});
});

router.get('/status', checkLogin, checkAdmin, function(req, res, next) {

     //NOTA: status em percentagem
     const pickingOrders = [{status: "Em progresso", shippingDate: "2016/11/23", worker: "Joaquim"},
     {status: "Em espera", shippingDate: "2015/11/23", worker: "Fernando"},
     {status: "Concluída", shippingDate: "2013/11/23", worker: "Alice"}];
    res.render('status', {pickingOrders: pickingOrders});
/*
    PickingOrder.getAll().then(function(pickingOrders) {
        res.render('status', {pickingOrders: pickingOrders});
    });
    */
});

router.get('/shipping', checkLogin, checkAdmin, function(req, res, next) {

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

router.get('/warnings', checkLogin, checkAdmin, function(req, res, next) {

    //NOTA: status em percentagem
    const warnings = [{priority: true, message: "O produto CPU, pertencente à sales order #3, não foi expedido a tempo"},
        {priority: true, message: "Apenas foram recolhidas 3 de 5 unidades pedidas do produto Motherboard"},
        {priority: false, message: "Ontem foram expedidas 23 picking orders"}];
    res.render('warnings', {warnings: warnings});
});

router.get('/users', checkLogin, checkAdmin, function(req, res, next) {

    const users = [{code: "001", email: "joaquimalmeida@email.com", name: "Joaquim Almeida", position: "Gerente"},
        {code: "002", email: "marianacorreia@email.com", name: "Mariana Correia", position: "Funcionário"},
        {code: "003", email: "ricardosantos@email.com", name: "Ricardo Santos", position: "Funcionário"},
        {code: "004", email: "joanaferreira@email.com", name: "Joana Ferreira", position: "Funcionário"}];
    res.render('users', {users: users});

});

router.get('/worker', checkLogin, checkWorker, function(req, res, next) {

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

router.get('/partials/newUser', function(req, res, next) {
    res.render('partials/user-modal');
});

router.post('/createPickingWave', function(req, res, next) {
    if (!req.body.selected) {
        res.status(400).send('Erro ao gerar picking wave');
        return;
    }
    const ids = req.body.selected;
    res.send("Sucesso");
});

router.post('/login', function(req, res, next) {
   const email = req.body.email;
   const pass = req.body.password;

   req.session.loggedIn = true;
   req.session.admin = false;
   req.session.userId = email;

   res.redirect('/');
});

router.post('/logout', function(req, res, next) {
    if(req.session) {
        req.session.destroy();
    }

    res.redirect('/');
});

module.exports = router;
