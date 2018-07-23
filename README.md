## CryptoXchange

CryptoXchange simple Cryto Exchange .

Even though the engine can be used to run a production-exchange.

### Features

- Supports clusters of pools each running individual currencies
- Payment processing
- Runs on Linux and Windows

### Coins

Coin | Implemented | Tested | Planned | Notes
:--- | :---: | :---: | :---: | :---:
Bitcoin | Yes | Yes | |
Litecoin | Yes | Yes | |
Zcash | Yes | Yes | |
Monero | Yes | Yes | |
Ethereum | Yes | Yes | | Requires [Parity](https://github.com/paritytech/parity/releases)
Ethereum Classic | Yes | Yes | | Requires [Parity](https://github.com/paritytech/parity/releases)
Expanse | Yes | Yes | | - **Not working for Byzantinium update**<br>- Requires [Parity](https://github.com/paritytech/parity/releases)
DASH | Yes | Yes | |
Bitcoin Gold | Yes | Yes | |
Bitcoin Cash | Yes | Yes | |
Vertcoin | Yes | Yes | |
Monacoin | Yes | Yes | |
Globaltoken | Yes | Yes | | Requires [GLT Daemon](https://globaltoken.org/#downloads)
Ellaism | Yes | Yes | | Requires [Parity](https://github.com/paritytech/parity/releases)
Groestlcoin | Yes | Yes | |
Dogecoin | Yes | No | |
DigiByte | Yes | Yes | |
Namecoin | Yes | No | |
Viacoin | Yes | No | |
Peercoin | Yes | No | |
Straks | Yes | Yes | |
Electroneum | Yes | Yes | |
MoonCoin | Yes | Yes | |

### Runtime Requirements

- [.Net Core 2.0 Runtime](https://www.microsoft.com/net/download/core#/runtime)
- [PostgreSQL Database](https://www.postgresql.org/)
- Coin Daemon (per coin)

### PostgreSQL Database setup

Create the database:

```console
$ createuser xchange
$ createdb xchange
$ psql (enter the password for postgres)
```

Run the query after login:

```sql
alter user xchange with encrypted password 'some-secure-password';
grant all privileges on database xchange to xchange;
```

Import the database schema:

```console
$ wget https://raw.githubusercontent.com/lexa044/cryptoxchange/master/src/CryptoXchange/Persistence/Postgres/Scripts/createdb.sql
$ psql -d xchange -U xchange -f createdb.sql
```

### Building from Source (Shell)

Install the [.Net Core 2.0 SDK](https://www.microsoft.com/net/download/core) for your platform

#### Linux (Ubuntu example)

```console
$ git clone https://github.com/lexa044/cryptoxchange
$ cd cryptoxchange/src/CryptoXchange
$ ./linux-build.sh
```

#### Windows

```dosbatch
> git clone https://github.com/lexa044/cryptoxchange
> cd cryptoxchange/src/CryptoXchange
> windows-build.bat
```

#### After successful build

Now copy `config.json` to `../../build`, edit it to your liking and run:

```
cd ../../build
dotnet CryptoXchange.dll
```

### Building from Source (Visual Studio)

- Install [Visual Studio 2017](https://www.visualstudio.com/vs/) (Community Edition is sufficient) for your platform
- Install [.Net Core 2.0 SDK](https://www.microsoft.com/net/download/core) for your platform
- Open `CryptoXchange.sln` in VS 2017
