# Data Providers #
The data provider subsystem is one of the integral parts of Moving Pictures. These components retrieve meta-data and artwork from online to provide movie details for the user. They should not handle any local parsing information and they should be total agnostic in regards to what they are providing movie information for. The output of this subsystem is populated Movie (DBMovieInfo) objects.

### Data Provider Manager ###
Manages rankings, prefered sources, etc. Yet to be implemented.

### Meta Data ###
Details on providers such as the Movie-XML.com data provider. How and where meta-data is retrieved from.

### Cover Art ###
Where we grab artwork from.

### Backdrops (Fan-Art) ###
Details on the logic of the backdrop importer.

### Scriptable Data Provider ###
This section should describe the upcoming scriptable data provider.