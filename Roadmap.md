* Roadmap
- chirp "it&#39;s me bart" should show as "it's me bart"
- do infinite scrolling on chirps (I think it's trying to retrieve and show all at once, just get a bunch at a time)
- when looking at the user's profile:
-- show pie chart of % of followers who are {Male, Female, Unspecified}
-- show pie chart of % of following who are {Male, Female, Unspecified}
- show post activity by day using GitHub profile grid by day visualization
- bootstrap the user action navigation menu
- mimic twitter's layout for showing Tweets
- stay up to date on Twitter Bootstrap
- if not already, deploy somewhere publicly
- add unit tests
- use an IoC container
- use Autofac to inject Repository into controllers (use something like Autofac for Dependency Injection)
- create a fake Chirp Repository to test different scenarios
- show assigned permalink to each Chirp
- allow one user to directly send a private Chirp to another user
- enable ReChirp
- in the code and Cassandra, change Tweet -> Chirp
- lost password (currently you can look this up in Cassandra directly since it's not getting encoded in any way)
-- email me my password if I forgot it
-- allow me to reset my password
- mimic behavior of Python Cassandra clone (named ...) and/or Sinatra clone applications mentioned in Cloning applications book ("Cloning Internet Applications with Ruby")
- clean up behavior of "In Reply To"
- integrate Twitter bootstrap presentation framework into this application
- keep working to clone Twitter using Cassandra and other NoSQL technologies
- show number of users online (using Redis)
- recommend some people to me (send me emails)
- user profile storage
-- store when the user was created (so you can do memeber since..)
-- store user's age/birthday (prompt them for day, month, year) - month and year are required
-- store user's location (geotag)
- find people
-- near me (geotag), near a location
-- find people meeting certain criteria (sex, age group, location)
- do some analytics (perhaps using Hadoop)
-- find users who share tags in common
-- most referenced tags
-- users posting the most chirps (of all time, today, last hour, etc...)
-- count chirps per user
-- count total chirps
-- count total chirps per time period (last day, last hour,...)
-- count chirps with a given tags
-- who chirps more, men or women - per time period (last day, last hour,...)
- internationalization / localization
- API

* DONE
- convert views from Web Forms view engine to Razor view engine (IN PROGRESS)
- clean up page titles on each Razor view
- hook up Twitter Bootstrap to this
- user profile storage
-- store user's gender (male or female, or unspecified)
- update more buttons to use bootstrap look and feel
- show # of followers the user has on their main user page
- show # of people following them on the user has on their main user page
- show # of chirps user has posted on their main user page
- link followers count to followers page on main user page
- link following count to following page on main user page
- show chirps using relative time
	x seconds, 
	x mins, 
	x hours,
	x days,
	x months,
	x years ago)
- on the timeline, show tweets from latest to earliest (i.e. descending order)
- clicking Chirper in top nav menu brings you back to the main page

* Open Questions
- when to show absolute time of a chirp?
- test for time zone offset and chirp posting times
