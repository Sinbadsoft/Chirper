Chirper

Chirper is a simple twitter clone webapp with an .NET ASP.NET MVC 4 powered front-end and a Cassandra NoSQL back-end

http://www.sinbadsoft.com/blog/chirper-twitter-clone-webapp-with-net-front-end-and-cassandra-nosql-back-end/

* Dependencies:
- Microsoft .NET 4 Framework
- ASP.NET MVC 4
- Aquiles 1.0 library
- DataStax Community Edition
- NuGet
- Twitter Bootstrap 2.1.1

* Tested With:
- DataStax Community Edition 1.1.6 + OpsCenter Community (http://www.datastax.com/products/community)
- Windows 7
- Visual Studio 2010 SP1

* Enhancements and Improvements:
]- Solution updated to Visual Studio 2010
- web app now runs using ASP.NET MVC 4 using Razor View engine instead of ASP.NET MVC 2 and WebFroms view Engine
- now runs using Aquiles 1.0 library

* Cassandra setup

Install DataStax community edition and OpsCenter Community Edition

** Verify cluster name in DataStax OpsCenter Community (top left corner of "Dashboard" view next to Cassandra version number)

NOTE: To open DataStax OpsCenter Community Edition, open a web browser other than Internet Explorer and navigate to http://localhost:8888 

This is the cluster configuration you should have in web.config:

<aquilesConfiguration>
    <clusters>
      <add friendlyName="Test Cluster"> <!-- Cluster name from DataStax OpsCenter Community should match here -->
        <connection poolType="SIZECONTROLLEDPOOL" factoryType="FRAMED"/>
        <endpointManager type="ROUNDROBIN" defaultTimeout="6000">
          <cassandraEndpoints>
            <add address="localhost" port="9160"/>
          </cassandraEndpoints>
        </endpointManager>
      </add>
    </clusters>
  </aquilesConfiguration>

** Chirper Keyspace setup

Open 'Cassandra CLI Utility' (start menu -> DataStax Community Edition)

Type each of the following lines at the prompt:

create keyspace Chirper;
use Chirper;
create column family Users with comparator = UTF8Type;
create column family Tweets with comparator = UTF8Type;
create column family Following with comparator = UTF8Type;
create column family Followers with comparator = UTF8Type;
create column family TimeLine with comparator = UTF8Type;
create column family UserLine with comparator = UTF8Type;

* Roadmap
Moved to Roadmap.md