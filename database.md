# SINF

## Table of contents

1. [Database creation script](#create)
2. [Triggers](#triggers)
    1. [Default Date](#ddate)
    2. [Picking order assigned to employees](#picking_order_trigger)
3. [Database population script](#populate)
4. [Queries](#queries)
    1. [Auth](#auth)
        1. [Get worker name from ID](#worker_id)
    2. [Picking](#picking)
        1. [Picking](#picking)
        2. [Get all picking orders](#picking_orders)
        3. [Get picking order info](#porder_info)
        4. [Get sum of delivered items from completed picking orders](#sum_ditems)
    

## <a name="create"></a>Database creation script

```SQL
CREATE DATABASE IF NOT EXISTS warehouse; 

USE warehouse;

SET FOREIGN_KEY_CHECKS=0;

DROP TABLE IF EXISTS utilizador;
CREATE TABLE utilizador 
  ( 
     id       INT NOT NULL PRIMARY KEY auto_increment, 
     nome     VARCHAR(30) BINARY NOT NULL,
     email    VARCHAR(255) BINARY NOT NULL UNIQUE, 
     password VARCHAR(255) BINARY NOT NULL,
     tipo_conta ENUM('gestor', 'funcionario') NOT NULL
  ); 

DROP TABLE IF EXISTS picking_wave;
CREATE TABLE picking_wave 
  ( 
     id           INT NOT NULL PRIMARY KEY auto_increment, 
     data_criacao DATE NOT NULL 
  ); 

DROP TABLE IF EXISTS picking_order;
CREATE TABLE picking_order 
  ( 
     id                      INT NOT NULL PRIMARY KEY auto_increment, 
     id_funcionario          INT NOT NULL,
     id_picking_wave         INT NOT NULL, 
     id_localizacao_destino  VARCHAR(36) BINARY NOT NULL,
     id_armazem              VARCHAR(36) BINARY NOT NULL,
     data_expedicao          DATE NOT NULL,
     terminado               BIT DEFAULT FALSE, 
     FOREIGN KEY(id_funcionario) REFERENCES utilizador(id), 
     FOREIGN KEY(id_picking_wave) REFERENCES picking_wave(id) 
  ); 

DROP TABLE IF EXISTS picking_order_step;
CREATE TABLE picking_order_step
  (
      id                INT NOT NULL PRIMARY KEY auto_increment,
      id_picking_order  INT NOT NULL,
      id_localizacao    VARCHAR(36) BINARY NOT NULL,
      id_armazem        VARCHAR(36) BINARY NOT NULL,
      terminado         BIT DEFAULT FALSE,
      FOREIGN KEY(id_picking_order) REFERENCES picking_order(id) 
  );

DROP TABLE IF EXISTS picking_order_item;
CREATE TABLE picking_order_item 
  ( 
     id                     INT NOT NULL PRIMARY KEY auto_increment, 
     id_picking_order_step  INT NOT NULL,  
     id_linha               VARCHAR(36) BINARY NOT NULL,
     id_artigo              VARCHAR(36) BINARY NOT NULL,
     quantidade_pedida      INT NOT NULL, 
     quantidade_recebida    INT DEFAULT 0, 
     FOREIGN KEY(id_picking_order_step) REFERENCES picking_order_step(id) 
  );
  
SET FOREIGN_KEY_CHECKS=1;
```

## <a name="triggers"></a>Triggers

### <a name="ddate"></a>Default Date

Since mySQL only supports constants as default values, we need to use triggers to set a default date. This trigger does that.

```SQL
DELIMITER $$
CREATE TRIGGER default_date BEFORE INSERT ON picking_wave FOR EACH ROW
IF ( ISNULL(NEW.data_criacao) ) THEN
 SET NEW.data_criacao=CURDATE();
END IF;
$$
DELIMITER ;
```

### <a name="picking_order_trigger"></a>Picking order assigned to employees

These triggers guarantee that a the user responsible for a picking order is a worker. 

```SQL
DELIMITER $$
CREATE TRIGGER assigned_to_employees_insert BEFORE INSERT ON picking_order FOR EACH ROW
IF NOT EXISTS (SELECT COUNT(*) FROM utilizador WHERE utilizador.id = NEW.id AND utilizador.tipo_conta = 'funcionario') THEN
  SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Uma picking order deve ser atribuida a um funcionario.';
END IF;
$$
DELIMITER ;
```

```SQL
DELIMITER $$
CREATE TRIGGER assigned_to_employees_update BEFORE INSERT ON picking_order FOR EACH ROW
IF NOT EXISTS (SELECT COUNT(*) FROM utilizador WHERE utilizador.id = NEW.id AND utilizador.tipo_conta = 'funcionario') THEN
  SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Uma picking order deve ser atribuida a um funcionario.';
END IF;
$$
DELIMITER ;
```

## <a name="populate"></a>Database population script
``` SQL
# Auth

INSERT INTO utilizador(nome, email, password, tipo_conta) VALUES ('Afonso', 'afonso@gmail.com', 'Afonso', 'funcionario');
INSERT INTO utilizador(nome, email, password, tipo_conta) VALUES ('Flavio', 'flavio@gmail.com', 'Flavio', 'funcionario');
INSERT INTO utilizador(nome, email, password, tipo_conta) VALUES ('Antonio' ,'antonio@gmail.com', 'Antonio', 'funcionario');
INSERT INTO utilizador(nome, email, password, tipo_conta) VALUES ('Pedro', 'pedro@gmail.com', 'Pedro', 'funcionario');
INSERT INTO utilizador(nome, email, password, tipo_conta) VALUES ('Joao', 'joao@gmail.com', 'Joao', 'funcionario');
INSERT INTO utilizador(nome, email, password, tipo_conta) VALUES ('Castro', 'castro@gmail.com', 'Castro', 'gestor');
INSERT INTO utilizador(nome, email, password, tipo_conta) VALUES ('Couto', 'couto@gmail.com', 'Couto', 'gestor');
INSERT INTO utilizador(nome, email, password, tipo_conta) VALUES ('Casimiro', 'casimiro@gmail.com', 'Casimiro', 'gestor');
INSERT INTO utilizador(nome, email, password, tipo_conta) VALUES ('Silva', 'silva@gmail.com', 'Silva', 'gestor');
INSERT INTO utilizador(nome, email, password, tipo_conta) VALUES ('Monteiro', 'monteiro@gmail.com', 'Monteiro', 'gestor');

# Picking

INSERT INTO picking_wave(data_criacao) VALUES ('2016-10-02');
SET @picking_wave_id = LAST_INSERT_ID();
	INSERT INTO picking_order(id_funcionario, id_picking_wave, id_localizacao_destino, id_armazem, data_expedicao) VALUES (1, @picking_wave_id, 'EXPED', 'EXPED', '2016-08-05');
	SET @picking_order = LAST_INSERT_ID();
		INSERT INTO picking_order_step(id_picking_order, id_armazem, id_localizacao) VALUES (1, 'A2.B.002', 'A2');
		SET @picking_order_step = LAST_INSERT_ID();
			INSERT INTO picking_order_item(id_picking_order_step, id_linha, id_artigo, quantidade_pedida) VALUES (@picking_order_step, "a623d927-7a5a-11e6-a55f-080027184ecd"
, "B0002", 4);
		INSERT INTO picking_order_step(id_picking_order, id_localizacao, id_armazem) VALUES (1, 'A4', 'A4');
		SET @picking_order_step = LAST_INSERT_ID();	
			INSERT INTO picking_order_item(id_picking_order_step, id_linha, id_artigo, quantidade_pedida) VALUES (@picking_order_step, "a623d928-7a5a-11e6-a55f-080027184ecd"
, "B0007", 4);

	INSERT INTO picking_order(id_funcionario, id_picking_wave, id_localizacao_destino, id_armazem, data_expedicao) VALUES (2, @picking_wave_id, 'EXPED', 'EXPED', '2016-01-07');
	SET @picking_order = LAST_INSERT_ID();
		INSERT INTO picking_order_step(id_picking_order, id_localizacao, id_armazem) VALUES (1, 'A3.A.1.002', 'A3');
		SET @picking_order_step = LAST_INSERT_ID();
			INSERT INTO picking_order_item(id_picking_order_step, id_linha, id_artigo, quantidade_pedida) VALUES (@picking_order_step, "a623d929-7a5a-11e6-a55f-080027184ecd"
, "B0006", 4);
```

## <a name="queries"></a>Queries

### <a name="auth"></a>Auth

#### <a name="worker_id"></a>Get worker name from ID

Returns a worker's name from their ID.

Arguments:

- :id - The worker's id.

```SQL
SELECT nome
FROM utilizador
WHERE id = :id;
```

### <a name="picking"></a>Picking

#### <a name="picking_orders"></a>Get all picking orders

Returns all picking orders.

```SQL
SELECT *
FROM picking_order;
```

#### <a name="porder_info"></a>Get picking order info

Returns the status, shipping date and worker responsible of a picking order.

Arguments:

- :id - The picking order's ID.

``` SQL
SET @completed = (
    SELECT COUNT(*)
    FROM picking_order_step
    WHERE picking_order_step.id_picking_order = :id
        AND picking_order_step.terminado = TRUE
);

SET @total = (
    SELECT COUNT(*) 
    FROM picking_order_step
    WHERE picking_order_step.id_picking_order = :id
);

SELECT utilizador.nome, @completed/@total AS status FROM picking_order, utilizador
WHERE picking_order.id = :id
    AND utilizador.id = picking_order.id_funcionario;
```

#### <a name="sum_ditems"></a>Get sum of delivered items from completed picking orders

Returns the sum of delivered items from picking orders that are already complete.

Arguments:

- :id_linha - The Primavera id of the item

```SQL
SELECT COALESCE(SUM(picking_order_item.quantidade_recebida), 0) AS total
FROM picking_order_item
    JOIN picking_order_step
        ON picking_order_item.id_picking_order_step = picking_order_step.id
    JOIN picking_order
	ON picking_order_step.id_picking_order = picking_order.id
WHERE picking_order_item.id_linha = :id_linha
    AND picking_order.terminado = TRUE;
```