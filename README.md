# Web app to monitor weather data collected through MQTT protocol and stored in InfluxDB3 database

## Dependencies:
- .NET SDK 8
- InfluxDB3 database 

## .env File
Make sure to have .env file inside weatherStationWeb folder with the following content
```env
BROKER_IP="<ip of your mqtt broker>"
USERNAME="<username for your mqtt broker>"
PASS="<password for your mqtt broker>"
INFLUX_AUTH_TOKEN="<token for your database, look into next section>"
```
## Running a local InfluxDB3 instance:

Running a DB server
```
influxdb3 serve --node-id weather-station --object-store file --data-dir <Path to where your DB will be stored>
```
Creating an access token (only do it once)
```
influxdb3 create token --admin 
```
Put this token inside .env file inside weatherStationWeb folder like this:
```
INFLUX_AUTH_TOKEN="<token>"
```
You can also put this token inside your system environment variable for easier use on the commandline
INFLUXDB3_AUTH_TOKEN=<token>

Creating a database
```
influxdb3 create database weather-station
```
Creating a table
```
influxdb3 create table weather --database weather-station
```

Checking if database is working
```
influxdb3 query --database weather-station "SELECT * FROM weather"
```
