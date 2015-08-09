# Testing Your Setup #
### Building and Running ###
Well believe it or not you are about done, your environment should be ready to go for development purposes. The key test here is to make sure everything compiles properly, so click "Build" on the menu bar at the top and select "Build Solution". This should take a bit of time, depending on how powerful of a computer you have. Something like one to two minutes is not uncommon because of all the MediaPortal code that needs to be compiled.

If everything compiled correctly, then next you will want to test that the Configuration Screen launches correctly. Right click the MovingPicturesConfigTester project and click "Set as Startup Project". Then from the menu bar at the top, click Debug -> Start Debugging. And there you go the Configuration Screen should pop up.

### Testing in MediaPortal ###
If you want to test changes you have made in an actual MediaPortal install, you need to make sure that you include the NLog dependency. Briefly, NLog is the logging library the plug-in uses and the nlog.dll file is required for the plug-in to run correctly. We don't want to have to deploy two separate DLLs though, so we have a tool to merge the two files. All you need to do is go to your Releases folder and run the merge.bat file, then you should be good to go.

### Problems? ###
If you get any errors with the _build process_, this most likely has to do with your References in one of your projects. In other words a project is most likely missing a dependency. In this case you will want to go through each project checking for any References flagged with a warning icon. If you find anything flagged, odds are it's a MediaPortal project. Track it down and add it to your solution. At the time of this writing though, you should not have any issues, if you loaded in the two MovingPictures projects and the five listed MediaPortal projects you should be good to go. If all of your references are correct, try closing Visual Studio, opening it, and then rebuild the solution.

If you do have an issue you can't resolve, make a post over in the [Moving Pictures General Discussion Group](http://groups.google.com/group/moving-pictures). It is regularly monitored by the core developer(s). If we get enough discussion relating to development, in the future I might create a Development Google Group though. So if you see a link on the main Google Code page to a Development group, odds are I just haven't updated the document yet, so take a peek there.

# Submitting Changes #
### Submitting a Patch ###
Most new developers on the project will be using this method first. Subversion gives you the ability to take all the changes you have made on your local system to the Moving Pictures source code and group it into one file called a patch. It's not difficult to do, in Windows Explorer, right click the Moving Pictures folder (the root folder, the one that links to the trunk), then click TortoiseSVN->Create Patch. On the following pop-up dialog you will have the ability to review all the changes that will be included in the patch. Once satisfied, click OK and your patch will be created. Give the patch a meaningful name and upload it to the Developers Google Group (and post a message about it). One of the regular developers will review it and respond. Thanks for helping out!

### Commiting Changes to Subversion ###
If you start making regular contributions to the project then you'll probably be added as a Project Member. This means you can commit changes directly to Subversion via TortoiseSVN. I am not going to cover this process here but it is pretty simple. Just be sure that you fill out a meaningful log message. The ability to skim through the past log messages in Subversion is very important, and if you make a commit without logging a message _you will get yelled at_. Also try to commit groups of files all at once. If you have made changes to five files all for one bug fix or enhancement, then these five files should all be commited together. Don't check them in one at a time individually, this just makes it more difficult to track changes in the version history.

# Comments #
So that's all I have. This document took a while to write so I hope you  have found it useful. If you have any suggestions, feel free to leave a comment below. And if you have had or are still having any trouble, feel free to post in the Google Group, we are friendly people and would be glad to help you get started.

-- John