const express = require('express');
const router = express.Router();
const SalesOrder = require('../../models/SalesOrder');
const PickingWave = require('../../models/PickingWave');
const Util = require('../../models/Util');
const User = require('../../models/User');
const Armazem = require('../../models/Armazem');

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

function checkWarnings(req, res, next) {
	Util.getNumAvisos().then(function(numAvisos) {
		req.session.numWarnings = numAvisos.quantidade;
		next();
	});
}

//Done
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

//Done
router.get('/choice', checkLogin, checkAdmin, checkWarnings, function(req, res, next) {
	Util.getSeries().then(function(series) {
		Util.getFiliais().then(function(filiais) {
			res.render('choice', {filiais: filiais, series: series, numWarnings: req.session.numWarnings});
		});
	});
});

//Done
router.get('/creation', checkLogin, checkAdmin, checkWarnings, function(req, res, next) {
    const required_params = ['serie', 'filial'];
	const serie = req.query.serie;
	const filial = req.query.filial;
	
	SalesOrder.getAll(serie, filial).then(function(salesOrders) {
		req.session.serie = serie;
		req.session.filial = filial;
		res.render('creation', {serie: serie, filial: filial, salesOrders: salesOrders, numWarnings: req.session.numWarnings});
	});
});

// Done
router.get('/status', checkLogin, checkAdmin, checkWarnings, function(req, res, next) {
	PickingWave.getAllPicking().then(function(waves) {
		res.render('status', {pickingOrders: waves, numWarnings: req.session.numWarnings});
	});
});

// Done
router.get('/status-rep', checkLogin, checkAdmin, checkWarnings, function(req, res, next) {
	 
	PickingWave.getAllReplenishment().then(function(waves) {
		res.render('status-rep', {pickingOrders: waves, numWarnings: req.session.numWarnings});
	});
});

// Done
router.get('/shipping', checkLogin, checkAdmin, checkWarnings, function(req, res, next) {
	Util.getSeries().then(function(series) {
		Util.getFiliais().then(function(filiais) {
			res.render('choice-shipping', {filiais: filiais, series: series, numWarnings: req.session.numWarnings});
		});
	});
});

router.get('/shipping-guide', checkLogin, checkAdmin, checkWarnings, function(req, res, next) {
	const required_params = ['serie', 'filial'];
	const serie = req.query.serie;
	const filial = req.query.filial;
	
	SalesOrder.getAllToShip(serie, filial).then(function(salesOrders) {
		req.session.serieShipping = serie;
		req.session.filialShipping = filial;
		res.render('shipping', {serie: serie, filial: filial, salesOrders: salesOrders, numWarnings: req.session.numWarnings});
	});
});

//Done
router.get('/warnings', checkLogin, checkAdmin, function(req, res, next) {
	Util.getAvisos().then(function(avisos) {
		res.render('warnings', {warnings: avisos});
	});
});

//Done
router.get('/users', checkLogin, checkAdmin, checkWarnings, function(req, res, next) {
   	User.getAll().then(function(users) {
		res.render('users', {users: users, numWarnings: req.session.numWarnings});
	});
});

//Done
router.get('/options', checkLogin, checkAdmin, checkWarnings, function(req, res, next) {
	Util.getCapacidade().then(function(capacidade) {
		Util.getArmazemPrincipal().then(function(armazemPrincipal) {
			Armazem.getAll().then(function(armazensRaw) {
				console.log(require('util').inspect(armazensRaw));
				armazens = armazensRaw.map(function(armazem) {
					armazem.selected = (armazem.ID == armazemPrincipal['armazem-principal']);
					return armazem;
				});
				res.render('options', {capacidade: capacidade, armazens: armazens, numWarnings: req.session.numWarnings});
			});
		});
	});
});

router.get('/worker', checkLogin, checkWorker, function(req, res, next) {
	 
	 PickingWave.getAssignedToEmployee(req.session.userId).then(function(task) {
		if (task != null) {
			req.session.currentWave = task.id;
			req.session.waveType = task.tipo;
		}
		let variables = {};
		if (task) {
			if (task.tipo == 1) {
				variables = {pickingOrder: task}
			} else {
				variables = {replenishmentOrder: task}
			}
		}
		res.render('worker', variables);
	 });
});

/**
 * REQUESTS
 */

 //Done
router.get('/partials/salesOrder/:id', function(req, res, next) {
   const id = req.params.id;

   SalesOrder.getItems(req.session.serie, req.session.filial, id).then(function (salesOrder) {
       res.render('partials/product-modal', {salesOrder:salesOrder, layout: false});
   })
});

router.get('/partials/salesOrderToShip/:id', function(req, res, next) {
   const id = req.params.id;

   SalesOrder.getItemsToShip(req.session.serieShipping, req.session.filialShipping, id).then(function (salesOrder) {
       res.render('partials/product-modal', {salesOrder:salesOrder, layout: false});
   })
});

//Done
router.get('/partials/newUser', function(req, res, next) {
    res.render('partials/user-modal');
});

//Done
router.post('/createPickingWave', function(req, res, next) {
    if (!req.body.selected) {
        res.status(400).send('Tem de selecionar pelo menos uma sales order');
        return;
    }
    const ids = req.body.selected;
	PickingWave.generate(req.session.serie, req.session.filial, ids).then(function() {
		res.send("Picking wave gerada");
	}).catch(function() {
		res.status(500).send('Erro ao gerar picking wave');
	});
});

//Done
router.post('/finishTask', checkLogin, checkWorker, function(req, res, next) {
	if (!req.body.linhas) {
		res.status(400).send('Nenhuma linha selecionada');
	}
	
	const linhas = req.body.linhas;
	
	let finishPromise = null
	
	if (req.session.waveType == 0) {
		finishPromise = PickingWave.finishReplenishment(req.session.userId, req.session.currentWave, linhas);
	} else {
		finishPromise = PickingWave.finishPicking(req.session.userId, req.session.currentWave, linhas);
	}
	
	finishPromise.then(function() {
		res.end();
	}).catch(function() {
		res.status(500).end();
	});
});

router.post('/createShipping', checkLogin, checkAdmin, function(req, res, next) {
	if (!req.body.nDoc){
		res.status(400).send('Nenhuma sales order selecionada');
		return;
	}
	
	const nDoc = req.body.nDoc;
	SalesOrder.ship(req.session.serieShipping, req.session.filialShipping, nDoc).then(function() {
		res.end();
	}).catch(function(err){
		console.log(err);
		res.status(500).send("Erro ao gerar guia de remessa");
	});
});

// Done
router.post('/login', function(req, res, next) {
   const email = req.body.email;
   const pass = req.body.password;

   User.login(email, pass).then(function(userData) {		   
	   req.session.admin = userData.is_admin;
	   req.session.userId = userData.user;

	   res.redirect('/');
   });
});

// Done
router.post('/logout', function(req, res, next) {
    if(req.session) {
        req.session.destroy();
    }

    res.redirect('/');
});

router.post('/createUser', checkLogin, checkAdmin, function(req, res, next) {
	const required_params = ['', '', '', ''];
	
	const username = req.body.username;
	const password = req.body.password;
	const name = req.body.name;
	const cargo = req.body.cargo;
	
	User.create(cargo, name, username, password).then(function() {
		res.redirect('/users');
	});
})

//Done
router.post('/definitions', checkLogin, checkAdmin, function(req, res, next) {
	const capacidade = req.body.capacidade;
	const armazem = req.body.armazem;
	
	Util.setCapacidade(capacidade).then(function() {
		return Util.setArmazemPrincipal(armazem);
	}).then(function() {
		res.redirect('/options');
	});
});

module.exports = router;
