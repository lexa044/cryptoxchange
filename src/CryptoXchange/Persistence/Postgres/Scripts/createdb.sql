SET ROLE xchange;

DROP TABLE IF EXISTS transferrequests;

CREATE TABLE transferrequests (
  id SERIAL PRIMARY KEY,
  fromCoin VARCHAR(8) NOT NULL,
  fromAddress VARCHAR(64) NOT NULL,
  toCoin VARCHAR(8) NOT NULL,
  toAddress VARCHAR(64) NOT NULL,
  status SMALLINT NOT NULL,
  amount decimal(28,12) NULL,
  confirmationRequired SMALLINT NULL,
  confirmationProgress SMALLINT NULL,
  created TIMESTAMP NOT NULL,
  updated TIMESTAMP NOT NULL,

  CONSTRAINT TRANSFER_REQUESTS_SYMBOL UNIQUE (fromCoin, fromAddress, toCoin,toAddress) DEFERRABLE INITIALLY DEFERRED
);

DROP TABLE IF EXISTS transfers;

CREATE TABLE transfers (
  id SERIAL PRIMARY KEY,
  fromCoin VARCHAR(8) NOT NULL,
  fromAddress VARCHAR(64) NOT NULL,
  toCoin VARCHAR(8) NOT NULL,
  toAddress VARCHAR(64) NOT NULL,
  status SMALLINT NOT NULL,
  bidAmount decimal(28,12) NULL,
  tradeAmount decimal(28,12) NULL,
  exchangeRate decimal(28,12) NULL,
  reference VARCHAR(128) NOT NULL,
  created TIMESTAMP NOT NULL,
  updated TIMESTAMP NOT NULL,

  CONSTRAINT TRANSFERS_SYMBOL UNIQUE (fromCoin, fromAddress, toCoin,toAddress) DEFERRABLE INITIALLY DEFERRED
);