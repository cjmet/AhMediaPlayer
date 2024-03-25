![Angel Hornet Logo](https://github.com/cjmet/CodeKy_SD09/blob/main/Angel%20Hornet%20Logo.png)
# Angel Hornet Media Player
### Code Kentucky Software Development - Class Project
#### This project is for Educational purposes.
The goal is to create a simple Media Player, with Media Library and WebAPI using .Net MAUI.  

The application will have a simple system to Play MP3s, ... and Add, Update, and Delete Songs and Playlists.  In addition to the MAui App, I'd like to eventually add API, CLI, and a Web application.  This is a learning experience.

### See Also: 
* One nice example of what Microsoft has done with Maui
  * https://dotnetpodcasts.azurewebsites.net/
  * https://github.com/microsoft/dotnet-podcasts


### Code Kentucky
Code Kentucky is a software development program in Louisville, Kentucky.  The course is designed to teach the fundamentals of software development and to prepare students for a career in the field.  The course is taught by experienced professionals in the field.

* https://codekentucky.org/
* https://codelouisville.org/
* https://code-you.org/

<br>

## Instructions
* Requires: Updated Visual Studio 2022, C#12, .Net 8, .Net MAUI.  Anything older than Nov 2023 will definitely not work.
* You'll Also need: 
  * https://github.com/cjmet/AngelHornetLibrary
* Compile and Run the App First.  Due to windows App virtualization of the %AppData% file-system, as well as other directories.  
* The App now has an installable .Zip package, if you have any trouble compiling it, you can use the .Zip package instead.
  * [AhMediaPlayer/Packages](https://github.com/cjmet/AhMediaPlayer/tree/main/Packages "AhMediaPlayer/Packages")



## Known Issues
* Only Windows is currently supported.
* You may need to compile the Maui App separately from the rest of the project.  Compiling the solution as a whole almost always fails to run properly.
* If you do not see the logo image, you need to clean and recompile the Maui App separately. 
* Make sure to select Windows in the upper left corner of Visual Studio Editor, as well as Maui in the upper middle, and Windows in the upper right.  Anything else can lead to unexpected behavior.
* Maui Apps have a virtualized and redirected file system.  This can cause issues with file paths and locations. Since I'm not officially publishing, I have to "guess" where this directory will end up.
* If you use the pre-compiled zip, you may get a warning that it could not find the MSIX Package directory.  
* When you load the Maui App, it will scan your %userprofile%/music, this may take a while.
  * Seconds for my local machine, 25 minutes for a large LAN NAS, and Hours for a WAN NAS.
  * This scan will ***NOT*** follow re-parse points.  This may cause it to miss some redirected paths, particularly with OneDrive.  If that happens you can use the manual scan.
  * This startup sequence was not an issue after the first scan, even with the large NAS, but it is possible that it could be given the right circumstances.
* Swagger can only load about 1000 songs, any more and it locks up.  Use Postman and enable larger queries in AhConfig.Constants if you want to test a larger query.  Or just use the App for the larger queries.
* GUI responsiveness suffers to SMB WAN Operations.  
  * This is in some cases lagging the entire OS, not just the application.  This is as much an OS issue as programming issue.  
  * I've further isolated the synchronous SMB/OS operations into a sub-task, which has helped, but not entirely alleviated the issue.
* MP3s required.  
  * A short private demo for class can be arranged.  But publicly publishing even extremely short samples of audio gets into issues I'd rather avoid.


## Suggestions
* I highly recommend that, if possible, if you are not under hard time constraints, that you: 
  * Spend the extra time to use at least a generic repository pattern as discussed in class.
  * Spend the extra time to use MvvM at least to a minimal degree. Some Maui controls are just not designed to work well without it.
  * I did neither of these at first, and I am already regretting it.  On the other side of that, TIME is an issue and I would likely be a week or two behind if I had.
  * Save plenty of time for publishing and debugging the publishing process if you are going to attempt to publish.  I think I would recommend saving a week just for learning publishing.  It can be, and still is, a nightmare.
* If you want cross-platform compatibility, keep at least an 'android' project target enabled at all times. And probably test it once a day.  My pain is your gain.
* MAUI Debugging is sometimes horribly generic and unhelpful, . . . or worse.
* Buy a Wholesale ~~Truckload~~ Super Tanker of Salt: 
  * Advice from earlier than Dec 2023 may be outdated or even incorrect.
  * Many older examples and tutorials may not even compile.
  * However, a fair amount of earlier advice and info on Xamarin can still be helpful.  Just use it with caution, particularly on more complicated issues.
* Fragility of the layouts has been a major issue for me.  These are my recommendations:
  * Use defaults as much as possible.
  * Keep the padding around the edges as much as possible.
  * Beware anything that auto-sizes.  
  * If you have ANY flicker or wobble or stutter at all in the layout, FIX IT!  It will only lead to a crash later.
  * The worst case I have fixed so far was an 8 pixel font in a label 12 pixels high had to be increased to 13 pixels high to stop the layout from crashing.
  * My entire Layout #2 was unstable, so I had to roll it back.  Then I had to spend two days to debug it one element at a time till I found and fixed all the issues. 
  * Test vigorously.
  * Use a 4k monitor for testing if possible.  Most of the fragility never showed up on a 1080p, but happens primarily on resizing on a 4k.  
  * For me Resize bottom right, top left on the 4k, is fairly deadly to the layout if it's unstable.
 

<br>

## Current Project Questions
1. What is the best way to accomplish saving sort order to the Db?  
1. &nbsp; General Program Design Principles.
   1. Now that I've gotten this far, I can absolutely see the advantages of, and wish in hind-sight I'd had both more experience and considerably more time to better implement things like repository and MvvM.  And I would like to discuss and look at general design principles more closely.
        ``` 
        [ Windows Android iOS MacOS MauiBlazorWeb ]  CLI   Web
              --------- Maui Gui --~~ ? ~~            |     |
                -----------MvvM ----------------------'     |  
                     --- Interface ---                      |
             General Business Logic and Services ~~~ ? ~~~ API
                     --- Interface -------------------------'    
                  Repository / DAO Layer
                 DbContext / Data Storage
        ```
   1. Unfortunately, right now the majority of my logic is in the Maui layer.  It would be very beneficial to move as much as possible into MvvM layers.
   1. Also, right now, most of the data access is directly in the dbContext, with only one or two Maui Methods and the API using the IRepository.  It would be very beneficial to move Data Access to the IRepository.
   1. But that still leaves things pretty tightly coupled.
   1. I already had to move some logic into the repository, because I wanted it accessible to the API, but it really probably doesn't logically go there.
   1. To decouple the Front from the Back, it seems most of the logic really needs to go into the middle Logic/Services Layer, and in addition, the API likely needs to couple to the middle Logic/Services Layer, and not directly to the repository layer.  Then have the API feed an MvvM layer that works with the (Web) UI layer.  Otherwise you have to move or duplicate logic either in the front or back.  In the back couples and binds you while the front duplicates efforts.
   1. So a more robust middle services and logic layer is where I'm heading with this line of thought, as well as coupling the API to the middle layer instead of the lower layers. But that's also what I'd like to discuss.
---
1. &nbsp; In the process of eliminating one question, I get three more:
   1. &nbsp; Class: MvvM, Data Transformation Objects (or Data Transfer Objects or Data Transport Objects), Authentication, and Authorization.
   1. &nbsp; Question about MvvM and planning and layout.  View as a whole, whole page, partial page(s), or organized by data structures and sections?
   1. &nbsp; MvvM vs Events and Commands.  More Command Info, as I'm still lacking entirely where commands are concerned.
1. &nbsp; The Sync / Async Task buried in the middle of the converted Advanced Search inside DataLibrary.  I need to learn how to comprehend and fix this particular pattern.
---
1. &nbsp; Class: API Authentication and Validation
1. &nbsp; IQueryable and Union, Intersect, Except?
1. &nbsp; Predicate Builder for search and filter.
1. &nbsp; ExecuteUpdate on linked Songs item in Playlists? 
---
1. &nbsp; What is the correct place to put the second window's initialization code?  Probably NOT where it is now.
1. &ensp; What is the correct way to Check that a window element is open and finished rendering before trying to access it?  
   1. I'm currently checking it's Height as an easy check
`while (await this.Dispatcher.DispatchAsync(() => WebView.Height) < 1) await Task.Delay(1000);`
   1. Then I'm checking is* when a more difficult check is needed.
        `var isAlive = await this.Dispatcher.DispatchAsync(() => WebView.IsEnabled && WebView.IsVisible && WebView.IsLoaded);
        if (!isAlive) { Debug.WriteLine($"WebView is Dead!"); return; throw new Exception(); }`
1. &ensp; What is the correct way to Access the log service from my other libraries and classes?
   1. I put logging into AhLog() Library, Using DI, then access it through either AhLog or DI as needed.
   1. I want it usable for CLI, Libraries, APP, and DI.
   1. Is this a reasonable way to do this?
1. &nbsp; How Do I? &ensp; - &ensp; Reference or Use MainPage.mediaElement from another file.cs class?  I could move the PlaySong() method to a library then, and re-use it on other pages later.  Dispatcher Maybe?  Syntax?
   1. Pass the target element to the method as a parameter?
---
1. &nbsp; Code Review how to async return a value, non-blocking, from a block-able operation, using a 4 step process.  (See Sandbox)
1. &nbsp; Code Review of AhGetFiles.GetFilesAsync() and/or AhFileIO.PollLinesAsync().

<br>

## Project Plan
Create a music library Web API and simple Media Player
* ### General To-Do List
- [ ] Sort Order, Save Method, Playlist Order, and other standard playlist controls.
  - [ ] Search, Sort, and Filter " ", "", ' ', '', and NULL ... when sorting nulls take up a large part of the top of the list.
  - [ ] Dictionary<Song>?  I'm probably going to regret this.
- [ ] More API work?  Where do we go from here with the API?
- [ ] Repository Pattern and Interfaces
- [ ] MauiProgramLogic, General SOLID Principles, Refactoring, Model views, General Logic, more organized engineering and design.
- [ ] Uniquely ID each song to eliminate duplicates, including duplicates with different filenames
---


* ### Music Player
- [x] My First MAUI App
- [x] Initial EF Core Setup and Test
- [x] Play a song
- [x] Play a static song from local storage
- [x] Create a Playlist class
- [x] Wire up a Button with Command and Command Parameters
- [x] Play as song from a static Playlist (Read)
- [x] Play more than one song from a static Playlist
- [x] Song Selection and Playing UI Layout
- [x] On Error Skip
- [x] Logging Service.  Move all the Debug.Writeline into a logging service that: 
  - [x] Writes to a Pop-Up Window (if in debug mode)
  - [x] Improved Logging Window
  - [x] `public async IAsyncEnumerable<string> PollLinesAsync(path, pollInterval, CancellationToken?)`
- [x] Integration of Playlists, Songs, Search and GUI
  - [x] Playlist Selection GUI
  - [x] Initial Integration of Playlists and Songs
  - [x] Random, Loop Buttons, Start, End, Next, Previous, ...
  - [x] Basic Search and Filter functionality GUI
  - [x] Advanced Search and Filter First Pass
  - [x] Advanced Search and Filter Second Pass
  - [x] Adding Playlists GUI (Create)
  - [x] Deleting Playlists GUI (Delete)
  - [x] Adding Songs to Playlists GUI (Update)
  - [x] Removing Songs from Playlists GUI (Update)


* ### Common Library 
- [x] CommonLibrary Project so that program logic can be shared, then wire up individual wrappers in a project as needed.
- [x] Logging Service.  Move all the Debug.Writeline into a logging service that: 
  - [x] Writes to Logfile.txt
  - [x] Writes to Debug.Writeline
- [x] Search for MP3 files, with a Background Task. (Create)
  - [x] Initial Test Background Async Task
  - [x] Improved Background Task
  - [x] Add Callback Ability to the Background Task
  - [x] Integrate FileFind, Callback, EF Core, and Maui
  - [x] Read and use Meta Data from MP3 files
  - [x] Id each song by filename to eliminate duplicates.
- [x] Extract ScanForMedia() into /CommonLibrary/CommonProgramLogic.cs 
- [x] Create Random Playlists for Testing
- [x] More Fluent Advanced Search with Parsing, to replace both Search and Advanced Search.

   
* ### Data Library
- [x] Store in SQLite database
- [x] Integrate with Music Player
- [x] Add the rest of the data structures back in, and to the Data Library.
- [x] Move data files back out into a DataLibrary for easier management. 
- [x] Initial Integration of Playlists and Songs
- [x] Searching and Adding Songs to Db
- [x] Implement the Interfaces and Repository Pattern.
- [x] CRUD Interfaces


* ### Music Library Web API
- [x] Implement the Interfaces and Repository Pattern.
- [x] Add an API Project to the solution
- [x] Develop basic API to match basic functionality of Music Library
  - [x] Basic Playlist API
  - [x] Basic Song API
  - [x] Basic CRUD API 
  - [x] API Locking of Playlist 1 and Songs Db as internal "All Songs" Playlist". (This is only locked in the API.)
- [x] Basic API Authentication Demo
- [ ] Actual API Authentication Implementation
- [ ] API Authorization
- [ ] API Playlist Validation
- [ ] API Song Validation
- [ ] API Orphaned Songs?


* ### Wish List and Refactoring
- [ ] Unit Testing on the API.  It's the perfect candidate for unit testing.
    - [ ] Create A Playlist, Modify It, Add Songs, Remove Songs, Delete it.
    - [ ] Test Authentication, Register, Login, Test Authentication, Logout, Test Authentication, Delete? the Test Account.
    - [ ] Automate an Advanced Search Test
- [ ] Rework to scan filenames and pathnames only first, partially filling in song info. Then go back and scan and decode the id3 headers to fill in the rest of the information.  
- [ ] QueenBee Controller to monitor and direct all the worker tasks.  Add redundancy and restarts as well as monitoring to the background task(s).   We currently can only get about 1000 songs at a time over the wan before it breaks due to a time-out or noise on the lines.
- [ ] Refactoring all the various tasks to better task design patterns.  Particularly better use of TaskCreationOptionsLongRunning.  Better combining of tasks were possible, and overall optimization of the tasks.
- [ ] Modify GetFilesAsync to use a List<string> of search patterns.  Aka: "mp3", "flac", "wav", "m4a", "mp4", "wma", "aac", "alac" ...  mpeg4, mpeg3, mpeg2, adts, asf, riff, avi, ac-3, amr, 3gp, flac, wav.  Just be mindful in the implementation that we don't want to cause async db issues as a tertiary effect.
- [ ] Consolidate a single log-file reader that sends data to both the log-viewer and message queue.
- [ ] Add a drive check first time we touch each drive ... cache the result.   That way we don't keep pinging off-line or malfunctioning drives.
- [ ] Maybe only cache drives that take more than 3-5 seconds to load a file.
- [ ] Maybe Refactor to Stream First, then Cache, then Read from Cache.  Right now everything caches.

<br>

## Project Requirements (Pick 3 or more)
- [x] Async Task
  * Created an async background task to scan local and remote SMB drives for media files, using callbacks to deliver updates asynchronously.
  * Many other async tasks as well, throughout the project.
- [x] List or Dictionary
  * Using multiple lists as well as the ConcurrentBag class in the background task.
- [x] API
  * Simple Default Endpoint Implementation, converted to Async and Repository Pattern, Added Default Basic Authentication.  Only one demo endpoint is locked by Authentication.
  * I did add Search to the Song API, as well as a few other misc endpoints, However, it's hard to know where to go from here without a target for the API.
- [x] CRUD API
  * Simple Default Endpoint Implementation, converted to Async and Repository Pattern.
  * You can add, delete, and update new playlists, and add and delete songs_by_id to and from created playlists.
  * By default Playlist[1] is locked in the API.  
  * By default Songs are locked in the API.
  * You can unlock the API by modifying the Constants.cs file.  It took days to scan the remote Db, so I didn't want to make any casual mistakes.  And I backed up the Db too, just in case.
- [x] Multiple Data Tables
  * modelBuilder.Entity<Song>().HasMany(s => s.Playlists).WithMany(p => p.Songs);
- [x] Logging of Errors and Debug Info.
  * AhLog() class and Serilog
- [x] Regex
  * Regex to clean up and convert Title to AlphaTitle to sort by
  * Regex in the Advanced Search
- [ ] Unit Testing
  * Wish List Item
- [ ] ~~SOLID Principles~~
  * I'll do a better job of this in the future, and as I have time to refactor and clean things up.  I was too entirely clueless and stumbling around in the dark to properly implement a good SOLID plan.
- [ ] ~~SQL Queries~~
  * Unlikely to be used in this project.
- [ ] ~~Generic Class~~
  * Unlikely to be used in this project.

<br>

## Dev Blog
* Third Attempt still has many of the same errors and bugs.  But is semi-stable.
* Second Attempt to move the project into a new clean framework.  First attempt ended up with the same errors as before, and just turned into a complete disaster again.
* Publishing and compiling is a disaster no matter what I do.  It's either wanting android stuff, or IOS stuff, or it's ignoring the windows app manifest.  Every way I try it, it's another different error.
* Added NULL Keyword and Logic, and minor layout updates.
* Publish 0.1.19.  Fixed Shuffle, and several additional publishing issues.  Eventually I'm going to have to completely rebuild the project with a fresh install.
* Oops. I broke shuffle
* Since the entire project and machine got fried by the dynamic install option, I went ahead and fixed various small package related things.
* ***DO NOT*** Try to publish a .zip that is not self-contained.   
  * It IS smaller, by half, but requires installing a command-line elevated package.  That package is very unintuitive and undiagnostic.  When I did get it installed, it fried my computer, and corrupted the project.  I had to disable android as a target completely to be able to compile or run anything.  Once I did finally have it all straightened out 4 or 5 hours later ... 
* Working on Sort Order, in preparation for Playlist Order.  The database is not saving list by order, but instead is in song.id order.
* ***THIS SHOULD NOT HAPPEN !!!*** &nbsp; 🤬 The exact same layout, same dimensions, everything.  5 labels.
  * **Stable:** 5 labels inside a 2x3 grid, Each in it's very own cell, with very precise buffers around the edges, it's stable.
  * **Crashes** on resize: the very same 5 labels in one very large cell, and place them Upper Center, Middle Left, Middle Right, Bottom Left, Bottom Right 
* Updated the Layout again.  The new layout is stable, but not as compact as I would like due to the stability bugs.
* Updated Layout was not stable. Reverted it. I'll have to re-enable it one item at a time until I can find the unstable item.  The fragility of MAUI layouts is frustrating.
* Updated the layout.  Moved the kittens to the corner, and gained room for one extra Playlist and Song each.
* We have a .Zip Package! Via command line publishing.  
  * .Zip: 	dotnet publish -f net8.0-windows10.0.19041.0 -c Release -p:RuntimeIdentifierOverride=win10-x64  -p:WindowsPackageType=None -p:WindowsAppSDKSelfContained=true --output C:\Users\cjmetcalfe\Local\Packages\tmp\ 
  * .MSIX:  dotnet publish -f net8.0-windows10.0.19041.0 -c Release -p:RuntimeIdentifierOverride=win10-x64 -p:PackageCertificateThumbprint=...... --sc
  * Tested it on a remote VM and a local desktop.  Both worked, with the following exceptions:
    * The Resource song would not play on the remote
    * I'm guessing Resources do not register nor play in the .Zip version.  Probably because it's NOT a "package" and resources are more accurately "'package' resources" 
* WARNING: Do ***NOT*** try to publish using Visual Studio 2022 as of March 2024.  There are multiple issues.  Use the command line instead.  There are still issues, but they are more manageable.
  * I suspect it's breaking or missing the equivalent of the -f and -p:Override above. It may also be trying to rebuild and recompile the entire solution with Maui options, which would definitely lead to unexpected results.
  * There was a warning about limiting publish scope, but no explanation of what that means or how to limit the scope such that Maui doesn't try to compile non-Maui
* These workarounds are unsuccessful for me.  I'll keep trying, and keep looking for other answers, but I'm not very hopeful.  
    * https://developercommunity.visualstudio.com/t/Assets-file-projectassetsjson-doesnt-/10461984
* Publish for Maui in Visual Studio is known broken.  Awesome.  Edited one file to fix one error related to target ID, but there are more edits required.  I'll either work more on that tomorrow or try publishing on the command line instead.

* Fixed a minor bug.  The code cleanup automation deleted a couple of using statements that were still needed.
* Bottom line on an actual demo video (unless it's muted) is complicated.  This makes demoing a music player unnecessarily complicated, but that's where we are.
* Worked on cleanup and demos.  I've also asked for input on some additional free mp3s I could include just in case for demos.  
* General cleanup and organization.  Updated double-tap gesture loading.  It should be a little safer now.
* I think I'm gonna call this Version 0.1.  Then mostly work on fixing bugs, cleaning things up, refactoring, and organizing code.
* **Warning** Negative Padding can be dangerous.
  * Cause a crash if you also set IsVisble=False.  
  * Some elements won't accept it at all or always crash.
  * Others it works with so long as you don't zero them out, as in negative padding has be less than half the smallest dimension / 2 - 1.
* Fixed another Bug
  * ***WARNING*** Minimum Text Height needs to be approximately RoundUp( 1 + FontSize * 1.5 ) ... The +1 is to keep it from crashing
  * ***HEIGHT*** is effecting width, near edges.  Maybe because of the corner rounding?  But that's then causing a stutter? and layout looping crash
* Fixed a bug I introduced in the delete method.  I KNEW better, but forgot.
  * You can't OrdinalIgnoreCase in EF on the Database side, only the client side.  This is one way to do it, the second line evaluates OrdinalIgnoreCase on the client side.
    ```
    var _playlists = await _dbContext.Playlists.ToListAsync();
    _playlists = _playlists.OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase).ToList();
    ```  
* Holy Swiss Cheese!  30 songs per second to 10k songs per second.
  * At first the new version was hard locking.  I fixed that by making the list 'smarter', and I strongly suspect that's related to 'setting already set' AND using SaveChangesAsync() in place of SaveChages().  I can't explain the WHY, but the results are dramatic.
* GitHub continues downloading Pascal Case image names to windows, but we can't compile them that way.  So I renamed everything _lc in lower case to hopefully force it to download lowercase.
* `AliceBlue #FFF0F8FF`
`Platform/Windows/App.Xaml`
    ```
    <maui:MauiWinUIApplication ... 
        <maui:MauiWinUIApplication.Resources>
            <StaticResource x:Key="ListViewItemBackgroundSelectedPointerOver" ResourceKey="SystemControlForegroundAccentBrush" />
            <StaticResource x:Key="ListViewItemBackgroundSelected" ResourceKey="SystemControlForegroundAccentBrush" />
            <SolidColorBrush x:Key="SystemControlForegroundAccentBrush"  Color="AliceBlue" />
        </maui:MauiWinUIApplication.Resources>
    </maui:MauiWinUIApplication>
    ```
    Another Alternative I did not use:
    ```
	    <maui:MauiWinUIApplication ......>
		    <maui:MauiWinUIApplication.Resources>
			    <ResourceDictionary>
				    <SolidColorBrush x:Key="ListViewItemBackgroundSelected" Color="#Ff0000" />
				    <SolidColorBrush x:Key="ListViewItemBackgroundSelectedPointerOver" Color="#Ff0000" />
				    <SolidColorBrush x:Key="ListViewItemBackgroundSelectedPressed" Color="#Ff0000" />
				    <SolidColorBrush x:Key="ListViewItemSelectedBackgroundThemeBrush" Color="#Ff0000" />
			    </ResourceDictionary>
		    </maui:MauiWinUIApplication.Resources>
	    </maui:MauiWinUIApplication>
    ```
* Add/Remove, Add All, Remove All, Edit, ... 
* Also dove into starting to learn some of the custom gestures.  Mostly just played with select, click, double-click so far, and how to get them to play nice with each other.  It was a productive day of experimentation.
* First alpha of the Gui layout for adding/removing songs from the playlist.  It's not going to look anything at all like what I originally imagined, but hopefully it works well enough.
* After the detour to learn more things, I'm once again making slow, but sometimes tedious forward progress.
* 🤬 "And we just paste this 180 character long mystical incantation written in ancient Sumerian here, and voila!  It works!"  What is possibly the most incomprehensible yet also most important line of code, and there's no attempt made to even try to explain it.  Then the actual source code and files are missing so you can't even try to decipher it without watching the video 47 times.
* Make that a super tanker boatload of salt. Spent several days watching videos and doing tutorials on various aspects of Maui.  Even some of the official sources are of dubious quality, others ... 
* Worked on the GUI, Database, and Some EF Core Issues.
* Started GUI Layout work for Editing Playlists
* API Authentication, Various API Endpoints, Editing of misc Interface methods
* Experimental Half-Baked Alpha Authentication added to the API.  This is a very basic and incomplete implementation. 
* Worked on API.  Added searches to the Song API.  Converted Advanced Search to DataLibrary and Repository Pattern.  Turns out that's where it should have been in the first place, instead of common.
* Advanced Search, Version 2, with simple search parsing and filtering enabled.
* Resized Event is _"working"_
* There HAS to be a better way to cause a binding value to update on an event triggering?  There is, we're getting there.  I'm currently learning one way with `INotifyPropertyChanged`, but would like to learn the other as well, but can't find any info on it beyond one mention.
    ```
    var tmp = _label.BindingContext;
    _label.BindingContext = null;
    _label.BindingContext = tmp;
    ```
* First pass on Advanced Search is complete and working.
* 😕 __WHY!__ do these not all use the same syntax?!?
    ```
    _currentSet.UnionBy(_selectionSet, s => s.Id).ToList();
    _currentSet.IntersectBy(_selectionSet.Select(s => s.Id), c => c.Id).ToList(); 
    _currentSet.ExceptBy(_selectionSet.Select(s => s.Id), c => c.Id).ToList(); 
    ```
* Use Concrete Lists with Union, Intersect, and Except.  IQuerable should be worked around and converted ToList() as necessary ... when using Union, Intersect, or Except, or only used with caution by someone much more experienced than I currently am.
* Worked on Advanced Search.  Ugh.  Linq, and Dbs, and Types, and several other things to straighten out.
* Worked on Advanced search and GUI Layout
* Added Manual Search Button, for use with Libraries, Servers, NASes, etc.
* Added additional controls, Gui Work, and a few other things.
* More debugging and adjustments on the file caching.
* GUI responsiveness to SMB WAN Operations.  These operations are running on separate async tasks, but the 'opening file' phase still locks the GUI.
  * Even optimized with 'await FileStream' opening a file over the WAN can take a few seconds locking the GUI until the async transfer starts.  The transfer itself is async, the 'opening' is not.
  * tried a couple different methods, and outside of native code this seems to be as optimized as it is going to get.
  * Reading the MP3 Tags over the WAN can also lock GUI for several seconds.  I'd have to rewrite this entire library, and even then it would not be entirely fixed without native code.
* This seems to be about as optimal as I can do without getting into platform specifics.
    * Opening a WAN FileStream can still lock the application for a few seconds, even with the FileStream being awaited and async.
* Save FileSize into the database so we don't have to call it from the WAN more than once.
* Looks like this will require native code, and not Maui compatible, and I'm wanting to keep things as compatible as possible
* FileInfo is insidious! ... going to see if I can find any other way to do this.
    ```
    // this should already be happening in a task, but it's lagging the GUI anyway
    // so lets wrap it in another 'async Task<long>' and see if that helps.
    // That did help, but it still lags the GUI.
    long _sourceSize = await Task.Run<long>(async () => { return new FileInfo(_source).Length; });
    ```
* Implemented Cache Cleaning
* Win11 does not automatically update last_access dates on media read. ... So we'll have to do it ourselves, so that cache cleaning can work use those dates.
* Next song on failure.  That was definitely low hanging fruit.
* Worked on File Caching.  After three tries I'm just glad it's working at all.   However, It could definitely use additional improvements.  I could also use additional guidance on how to do this kind of thing, and the other related tasks as well.
  * solved the primary problem by limiting the workers to a single central worker and using async safe types.  But this brings up other issues of keeping the thread running, and monitoring, and so on.
* THEY AREN'T KIDDING ABOUT ASYNC ISSUES.  
  * Even when it's only a simple type, and limited to an unchanging read-only, somehow, when multiple threads are involved they can mis-read a read-only!
  * Use async safe types, use locking, use dispatching, use task sync methods, or use some other mechanism to ensure that even simple reads of simple types do not happen at the same time by multiple threads.
* Trying to work on a Cache and Queue for slow connections.  It's only partially working.  The primary problem right now is synchronizing and controlling tasks.  Probably need to implement some locking.
* Added pathname to the Playlist when in debug mode.
  * Heinous Hackery with Bind Converters ensued to get around the headtruncate bug in Maui Windows Nested Labels.
  * I'd love some help on solving this problem more appropriately.
* Implemented the First Pass Playlist API, need to work on the Song List API next.
* Holy Mackerel 🐟, the following syntax was a complicated, cross-eyed, triple-nested puzzle.
```
        group.MapGet("/{id}", async Task<Results<Ok<Playlist>, NotFound>> (int id, IPlaylistRepository repository) =>
        {
            var playlist = await repository.GetPlaylistByIdAsync(id);
            return playlist != null
                ? TypedResults.Ok(playlist)
                : TypedResults.NotFound();
        })
        .WithName("GetPlaylistById")
        .WithOpenApi();
```
* First Pass at Interfaces and Repository Pattern Adoption
* DbContext.ExecuteUpdateAsync() and DbContext.ExecuteDeleteAsync()
* Another half-day debugging using the big MP3 library.  At this point I'm (half-heartedly) convinced that 9 out of 10 MP3s have at least one field of malformed or corrupt data.
* Added Song.AlphaTitle to sort by.  Regex to remove non-alphanumeric characters, and then to remove leading "The " and "A " from the title, etc, ... 
* Cleaned up the message loop some.  More bug fixes.
* Scanned 17,000 MP3s over 10/100Mb SMB into the Db in 25 Minutes.  Cached Updates after the first scan took a bit over 1 minute. 
* Lots and Lots and Tons and Tons of debugging while reading the big music library. A great many files have corrupted tags or properties that caused various parts of the program to crash
* Alpha Search Bar. We'll improve this more later, and integrate it with the playlists.
* LineItem is a Lambda Expression, and it isn't supported in EF Core Either.
* >"The LINQ expression 'DbSet<Song>()
    .Where(s => s.LineItem.ToLower().Contains(___searchText_0))' could not be translated. Additional information: Translation of member 'LineItem' on entity type 'Song' failed. This commonly occurs when the specified member is unmapped. Either rewrite the query in a form that can be translated, or switch to client evaluation explicitly by inserting a call to 'AsEnumerable', 'AsAsyncEnumerable', 'ToList', or 'ToListAsync'. See https://go.microsoft.com/fwlink/?linkid=210103"*
* >"In Entity Framework Core, the Contains method used in LINQ queries does not support the StringComparison enumeration. However, you can use the ToUpper or ToLower methods to perform a case-insensitive comparison. "
* Updated the messageBox and started sending log messages to it.  Then some general code cleanup
* Change the default log level to debug.  Use the info level more carefully.  
* Worked on Playlists, Playlist switching, and many various small things.
* There are many inconsistencies in the various methods.  Even within the same method various behaviors can vary.  One version of WebView auto-loads while the other does not, etc.
* 🤬 Rewrote the Log Viewer for the third time.  
  * The original idea was in theory very simple:
    ```
    WebView WebView = new WebView();
    WebView.Source = AhLog._logFilePath;
    this.Content = WebView;
    _ = Task.Run(async () => { while (true) { 
        await Task.Delay(1000);
        await this.Dispatcher.DispatchAsync(() => WebView.Reload());
    } } );
    ```
  * The Reality ended up far different. 
    * The first version took a day or two, a hundred lines and tons of debugging.  The second version took about a day with more debugging.  This third version only took an hour or two, and it's by far the best iteration.  On the positive side of things, I did learn many things along the way.
* Added FileLoop and (simple) file change detection.  This would perhaps be better with a FileSystemWatcher, but it's already 100 lines long and complicated enough.  And I'm probably going to throw it away and completely rewrite it anyway.  I just wanted to finish this path to learn everything I could from it.
* Don't write to the log-file inside the loop that reads your log file. 😉
* Using ListView and StreamReader inside the Second Window
  * `while (DataWindow.Height < 1) await Task.Delay(25);` To make sure the window is open before trying to access it.
  * MUST use `ObserverableCollection`, and MUST use `await Dispatcher.DispatchAsync`, (or at least use {Dispatch(); await Task.Delay(1)}, when running on a different thread.
  * `ObservableCollection` is a collection that provides notifications when items get added, removed, or when the whole list is refreshed.  This makes updates automatic.
  * **`await Application.Current.Dispatcher.DispatchAsync()`**   Is definitely the method you want, particularly when running async on a different thread.
    * Alternatively you 'can' {Dispatch(); await Task.Delay(1);} in order to allow the system to process the Dispatch before continuing.  However, this is slower, a little buggy, and is not recommended.
  * MUST use `FileShare.ReadWrite` to allow the file to be read while it is open by the other process. ReadWrite is a bit counterintuitive, as I'm only asking for read access, but it is necessary.  
  * Permissions by level, in order?: `Read, Write, ReadWrite, Delete, Append, AllAccess`.  Increase permission level until you get the access you need.
* Opening the Second Window
  * I still don't know the 'correct' place to put this code. But It needs to be somewhere that you can reference `Application.Current`.
  * https://devblogs.microsoft.com/dotnet/announcing-dotnet-maui-preview-11/
    ``` 
    var secondWindow = new Window
    {
        Page = new MySecondPage
        {
            // ...
        }
    };
    Application.Current.OpenWindow(secondWindow);
    ```    
  * This seems to work to make sure the windows are rendered before trying to access them.
    ```
    // Let the main window open first.
    while (list.Height < 1) await Task.Delay(25);
    // and then let it populate, there should be at least one song?
    while (list.ItemsSource == null || list.ItemsSource.Cast<Song>().ToList().Count < 1) await Task.Delay(25);
    ```
* Change: `Application.Current.MainPage.Dispatcher.Dispatch` to `this.Dispatcher.Dispatch`
* Spent all day fixing code I'm probably never going to use, just because it was an eventful learning experience.
* That was a total Disaster. Need to fix the data types. Need to figure out where to move the 2nd window code. Need to be able to verify that data is on-screen. More things than I can even think to write down
* Worked on the second Maui Window to display AhLog() data.
* Resources are required to have Lowercase Filenames:
  * `Get-childItem "." | Rename-Item -NewName {$_.Basename.tostring().tolower() + $_.extension}`
* Added default Logging.  AhLog() class using Serilog.
* Started work on basic logging.
* Rewrote the background task for the 4th time as: async IAsyncEnumerable<string> GetFilesAsync().  This version is streaming, and more closely matches standard conventions.  It should in theory be easier to use and understand.  It was perhaps the easiest iteration to write, but also took the most lines of code.
* Started work on CommonLibrary, cleaning up code and moving common logic into the library in a more organized and readable way.
* Fixed the Playlist to continue playing ... but still lots of work to do.
* 😲 I had no idea this was a thing: &nbsp;&nbsp; `var _sourceSongList = TestSonglist.ItemsSource.Cast<Song>().ToList();` &nbsp;&nbsp; This was exactly what I needed, and I found it almost entirely by accident reading almost completely unrelated code.
* Wired up the Song List, and now can select and play a song from the list.  I am going to need to redo whole sections of this code now that I have a better understanding of everything so far.
* When available, there's a very tiny '+' button when writing an event handler.  USE IT!  Each one is just enough different to typo something, and waste time otherwise.
* There were additional Issues with nested buttons
* Thank you Ernesto Ramos for help with fixing the button
  * Line 3 was missing from the examples I was using.
  * Line 5-7 are also required
  * I suspect there may be a better way to write this, one that doesn't have to effect the entire page.
    ```
    <ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
    xmlns:local="clr-namespace:ButtonCommandTest"
    x:Class="ButtonCommandTest.MainPage">
    <ContentPage.BindingContext>
    <local:CommandDemoViewModel />
    </ContentPage.BindingContext>
    ```
* Re-did the entire layout, added Id3 Tags for Meta Data, and added a genre dictionary.
  * Each grid with no dimension constrains and auto-sizes equally to all others grids with no dimensions.  "Auto" expands without constraint if asked.  I ended up creating 4 "Default" Rows, 1 Row for the logo, 3 Rows for the Song List, and everything else fixed rows.
  * Added two side columns for spacing so I could force overlay for Labels for Title, Artist, and Album.  Probably could have done this with a combination of padding and margins, but this worked.
  * Added Meta Data Tags with the iD3 Library.  I didn't have any lucky trying to add and use Windows.Storage.FileProperties.MusicProperties.  Same as before, I'm not sure if the Storage class just isn't available for Maui, has a different name, or I'm missing something I need to install?
  * Added a Genre Dictionary Class to translate Genre Numbers to human readable strings.
* Use   <StackLayout Orientation="Horizontal" or "Vertical">
  * VerticalStackLayout and HorizontalStackLayout do not always respect all placement options. These are apparently depreciated?
* General House Keeping and cleaning up various experiment messes made while learning.
  * Constants.cs
* Changed the Title in Appshell.xaml
* Did more integration with FileFind, Callback, EF Core, and Maui.
  * I'm not at all certain where various pieces of code should go.  It's all very messy right now while I'm still learning to crawl around slowly 
* **%AppData% is Different in Win11/Console, Win11/Web, and Win11/Maui**
  * This means the database files were being stored in different locations.
  * Win11 is: "C:\Users\Username\AppData\Local\test_Playlists.db"
  * MAUI  is: "C:\Users\Username\AppData\Local\Packages\ ... MauiMediaPlayer ... \LocalCache\Local\test_Playlists.db"
* Rewrote FindFiles for the third time, to use a callback from the background task to the main task.
* Improved the Background Task in preparation for using an action task to integrate it with EF Core
* Made progress on window size and location, as well as learning the initial basics of xaml layout.
* **MAKE SURE your DbSet data uses PROPERTIES and not fields nor variables!**
  * USE: **public string Name { get; set; }**
  * not: _public string Name;_
* Remember to set your Id properties to Public!
* Remember to set your Interfaces and DbContext to Public!
* Move DataLibrary to a .Net Library for compatibility with API and CLI projects
  * A MauiLibrary is not the same as a .Net Library.  The MauiLibrary may? only compatible with Maui?  While the .Net Library works with API and CLI as well.  This might be an issue later if I try to develop an Android app.
* A start on the Initial Background Task.
* My First MAUI App