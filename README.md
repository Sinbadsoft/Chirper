Chirper is a simple twitter clone webapp with a .NET front-end and a nosql back-end based on Cassandra.

http://www.sinbadsoft.com/blog/chirper-twitter-clone-webapp-with-net-front-end-and-cassandra-nosql-back-end/

* Dependencies
- Microsoft .NET 4 Framework
- ASP.NET MVC 4
- Aquiles 1.0 library
- DataStax Community Edition
- NuGet

* Tested With
- DataStax Community Edition 1.1.6 + OpsCenter Community (http://www.datastax.com/products/community)
- Windows 7
- Visual Studio 2010 SP1

* Enhancements and Improvements
]- Solution updated to Visual Studio 2010
- web app now runs using ASP.NET MVC 4 instead of ASP.NET MVC 2
- now runs using Aquiles 1.0 library

* Cassandra setup

Install DataStax community edition and OpsCenter Community Edition

** Verify cluster name in DataStax OpsCenter Community (top left corner of "Dashboard" view next to Cassandra version number)

This is the cluster name you need to have in web.config

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

- convert views from Web Forms view engine to Razor view engine (IN PROGRESS)
- clean up behavior of "In Reply To"
- integrate Twitter bootstrap presentation framework into this application
- use something like Autofac for Dependency Injection
- keep working to clone Twitter using Cassandra and other NoSQL technologies
- do some analytics using Hadoop

* Original License: 
Copyright � Chaker Nakhli 2010
Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the
License. You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0 Unless required by
applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language
governing permissions and limitations under the License. 
