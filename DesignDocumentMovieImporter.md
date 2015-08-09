# Movie Importer #
These components handle the process of turning a list of files into useful information in the database. These components read directly from the disk, they use the Data Provider components as a service and the output is a database full of meta-data relating to files on disk. If there is a heart to the plug-in, this is it. This is the component the creates all the magic.

### File Parsing / Matching ###
Covers the file name parsing and matching process. Rules for matching, conditions for where and how titles are parsed from. Also details on tanking of returned matches from the Data Providers.

### File System Watcher ###
Details on the components that bring in new files as they are added to the file-system in real time.