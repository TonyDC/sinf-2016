-- DDL
IF db_id('PICKING') IS NULL
	CREATE DATABASE PICKING
GO

USE PICKING;

IF OBJECT_ID('dbo.Definicoes', 'U') IS NOT NULL
	DROP TABLE Definicoes;

IF OBJECT_ID('dbo.Avisos', 'U') IS NOT NULL
	DROP TABLE Avisos;

IF OBJECT_ID('dbo.LinhaPicking', 'U') IS NOT NULL
	DROP TABLE LinhaPicking;

IF OBJECT_ID('dbo.LinhaReplenishment', 'U') IS NOT NULL
	DROP TABLE LinhaReplenishment;

IF OBJECT_ID('dbo.ReplenishmentWave', 'U') IS NOT NULL
	DROP TABLE ReplenishmentWave;

IF OBJECT_ID('dbo.PickingWave', 'U') IS NOT NULL
	DROP TABLE PickingWave;

IF OBJECT_ID('dbo.Gerente', 'U') IS NOT NULL
	DROP TABLE Gerente;
	
IF OBJECT_ID('dbo.Funcionario', 'U') IS NOT NULL
	DROP TABLE Funcionario;

IF OBJECT_ID('dbo.Utilizador', 'U') IS NOT NULL
	DROP TABLE Utilizador;

IF OBJECT_ID('dbo.LinhaEncomenda', 'U') IS NOT NULL
	DROP TABLE LinhaEncomenda;

IF OBJECT_ID('dbo.QuantidadeReserva', 'U') IS NOT NULL
	DROP TABLE QuantidadeReserva;


CREATE TABLE QuantidadeReserva (
	id INT PRIMARY KEY IDENTITY,
	artigo NVARCHAR(48) NOT NULL UNIQUE,					-- external database reference
	armazem NVARCHAR(5) NOT NULL,							-- external database reference
	--localizacao VARCHAR(30) NOT NULL,					-- external database reference
	quant_reservada REAL NOT NULL DEFAULT 0,
	CONSTRAINT CHK_QR_quantidade_nao_negativa CHECK(quant_reservada >= 0)
	--CONSTRAINT UN_Artigo_Localizacao UNIQUE (artigo, localizacao)
)

CREATE TABLE LinhaEncomenda (
	id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
	id_linha UNIQUEIDENTIFIER NOT NULL,				-- external database reference
	versao_ult_act CHAR(30) NOT NULL,				
	artigo NVARCHAR(48) NOT NULL,					-- external database reference
	quant_pedida REAL NOT NULL,						
	quant_satisfeita REAL NOT NULL DEFAULT 0,
	unidade NVARCHAR(5) NOT NULL,
	data_entrega DATETIME,							-- pode admitir valores NULL!
	serie NVARCHAR(5) NOT NULL, 
	CONSTRAINT UN_idLinha_versao UNIQUE (id_linha, versao_ult_act),
	CONSTRAINT CHK_LE_quantidade_nao_negativa CHECK(quant_pedida > 0 AND quant_satisfeita >= 0),
	CONSTRAINT CHK_pedida_menorQue_satisfeita CHECK(quant_pedida >= quant_satisfeita)
)

CREATE TABLE Utilizador (
	id INT PRIMARY KEY IDENTITY,
	username NVARCHAR(100) COLLATE Latin1_General_CS_AS NOT NULL,
	name NVARCHAR(100) NOT NULL,      
	pass NVARCHAR(60) NOT NULL,
	CONSTRAINT UN_Username UNIQUE (username) 
)

CREATE TABLE Funcionario (
	id INT PRIMARY KEY REFERENCES Utilizador(id)
	--cap_max INT NOT NULL DEFAULT 100
)

CREATE TABLE Gerente (
	id INT PRIMARY KEY REFERENCES Utilizador(id)
)

CREATE TABLE PickingWave (
	id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
	em_progresso BIT NOT NULL DEFAULT 1,
	id_funcionario INT NOT NULL REFERENCES Funcionario(id),
	data_inicio DATETIME NOT NULL DEFAULT GETDATE(),
	data_conclusao DATETIME,
	CONSTRAINT CNT_picking_wave_date CHECK(data_conclusao IS NULL OR data_conclusao >= data_inicio),
	CONSTRAINT CNT_PW_progresso_data_conclusao CHECK((em_progresso = 1 AND data_conclusao IS NULL) OR (em_progresso = 0 AND data_conclusao IS NOT NULL))
)

CREATE TABLE ReplenishmentWave (
	id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
	em_progresso BIT NOT NULL DEFAULT 1,
	id_funcionario INT NOT NULL REFERENCES Funcionario(id),
	data_inicio DATETIME NOT NULL DEFAULT GETDATE(),
	data_conclusao DATETIME,
	CONSTRAINT CNT_replenishment_wave_date CHECK(data_conclusao IS NULL OR data_conclusao >= data_inicio),
	CONSTRAINT CNT_RW_progresso_data_conclusao CHECK((em_progresso = 1 AND data_conclusao IS NULL) OR (em_progresso = 0 AND data_conclusao IS NOT NULL))
)

CREATE TABLE LinhaReplenishment (
	id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
	id_replenishment UNIQUEIDENTIFIER REFERENCES ReplenishmentWave(id),
	quant_a_satisfazer REAL NOT NULL,
	quant_recolhida REAL NOT NULL DEFAULT 0,
	localizacao_origem VARCHAR(30),				        -- a localização vai ser preenchida aquando do pedido de criação de picking order
	localizacao_destino VARCHAR(30) NOT NULL,
	artigo NVARCHAR(48) NOT NULL,
	unidade NVARCHAR(5) NOT NULL,	
	serie NVARCHAR(5) NOT NULL, 	
	notificado_aviso BIT NOT NULL DEFAULT 0,			-- para evitar nova geração de avisos
	CONSTRAINT CTR_recolhida_satisfeita_replenish CHECK(quant_recolhida <= quant_a_satisfazer),
	CONSTRAINT CHK_LR_recolhida_aSatisfazer CHECK(quant_recolhida >= 0 AND quant_a_satisfazer > 0)
	--CONSTRAINT CNT_LR_wave_loc_destino CHECK((localizacao_origem IS NULL AND id_replenishment IS NULL) OR (localizacao_origem IS NOT NULL AND id_replenishment IS NOT NULL))
)

CREATE TABLE LinhaPicking (
	id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
	id_linha_encomenda UNIQUEIDENTIFIER NOT NULL REFERENCES LinhaEncomenda(id),
	id_picking UNIQUEIDENTIFIER REFERENCES PickingWave(id),
	quant_a_satisfazer REAL NOT NULL,
	quant_recolhida REAL NOT NULL DEFAULT 0,
	localizacao VARCHAR(30),				        -- a localização vai ser preenchida aquando do pedido de criação de picking order
	artigo NVARCHAR(48) NOT NULL,
	notificado_aviso BIT NOT NULL DEFAULT 0,		-- para evitar nova geração de avisos
	CONSTRAINT CTR_recolhida_satisfeita_picking CHECK(quant_recolhida <= quant_a_satisfazer),
	CONSTRAINT CHK_LP_recolhida_aSatisfazer CHECK(quant_recolhida >= 0 AND quant_a_satisfazer > 0)
	--CONSTRAINT CNT_LP_wave_loc_destino CHECK((localizacao IS NULL AND id_picking IS NULL) OR (localizacao IS NOT NULL AND id_picking IS NOT NULL))
)

CREATE TABLE Avisos (
	id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
	mensagem VARCHAR(500) NOT NULL,
	visto BIT NOT NULL DEFAULT 0,
	data_registo DATETIME NOT NULL DEFAULT GETDATE()
	-- quem_viu INT REFERENCES Gerente(id)
)

CREATE TABLE Definicoes (
	chave NVARCHAR(300) PRIMARY KEY,
	valor NVARCHAR(300)
)

-- ---------------------------------------------------------------------------------
-- DML

-- Username: admin, password: admin
-- Username: worker, passowrd: worker
INSERT INTO Utilizador(username, name, pass) VALUES('admin@org.com', 'Administrador', '$2a$11$s0GMyuOSBXOUHLD353oHA..cmtYznNePmsIlWMvnIj06qlVkBaPFG'), ('worker@org.com', 'Worker', '$2a$11$mAmHgZiA.qyvHpHE0FK45.hWVW9oxRNn2ZXlt6wozDmF.Wq1k4jlC')  
INSERT INTO Gerente VALUES(1)
INSERT INTO Funcionario VALUES(2)

INSERT INTO Definicoes VALUES ('cap_max_funcionario', '100'), ('armazem_principal', 'A1')
GO

-- ---------------------------------------------------------------------------------
-- TRIGGERS (DDL)
/*
CREATE TRIGGER TR_CapMaxFuncionario ON Definicoes
INSTEAD OF UPDATE
AS
	DECLARE @new_cap INT;

	SELECT @new_cap = CAST(u.valor AS INT) FROM INSERTED u;
	PRINT @new_cap;
	BEGIN
		IF(@new_cap < 1)
		BEGIN
			RAISERROR('Bad worker capacity', 16, 1);
			ROLLBACK;
		END
		ELSE
			BEGIN
			BEGIN TRANSACTION;
			UPDATE Definicoes SET valor = CAST(@new_cap AS varchar(300)) WHERE chave = 'cap_max_funcionario';
			COMMIT TRANSACTION;			-- Antes de um commit, há que iniciar a transacção
			END
	END
GO
*/