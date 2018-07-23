#https://dba.stackexchange.com/questions/101570/error-42p01-relation-does-not-exist

sudo -i -u postgres

createuser xchange
createdb xchange

psql (enter the password for postgres)

alter user xchange with encrypted password 'some-secure-password';
grant all privileges on database xchange to xchange;

#Import the database schema

sudo -i -u postgres

wget https://raw.githubusercontent.com/coinfoundry/miningcore/master/src/MiningCore/Persistence/Postgres/Scripts/createdb.sql

psql -d xchange -f createdb.sql

#DROP DATABASE "YourDatabase";
#DROP USER [ IF EXISTS ] name [, ...]