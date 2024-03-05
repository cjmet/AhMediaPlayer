![Angel Hornet Logo](https://github.com/cjmet/CodeKy_SD09/blob/main/Angel%20Hornet%20Logo.png)
# Angel Hornet Media Player
### Code Kentucky Software Development - Class Project
#### This project is for Educational purposes.
The goal is to create a simple Media Player, with Media Library and WebAPI using .Net MAUI.  

The application will have a simple system to Play MP3s, ... and hopefully Add, Update, and Delete Songs and Playlists.  In addition to the MAui App, I'd like to add API, CLI, and a Web application.  This is a learning experience.


### See Also: 
* One nice example of what Microsoft has done with Maui
  * https://dotnetpodcasts.azurewebsites.net/
  * https://github.com/microsoft/dotnet-podcasts


### Code Kentucky
Code Kentucky is a software development boot camp in Louisville, Kentucky.  The course is designed to teach the fundamentals of software development and to prepare students for a career in the field.  The course is taught by experienced professionals in the field.

* https://codekentucky.org/
* https://codelouisville.org/
* https://code-you.org/

<br>

## Instructions
* Requires: Updated Visual Studio 2022, C#12, .Net 8, .Net MAUI
* Run the App First.  Due to windows app virtualization of the %AppData% file-system, as well as other directories.
* You'll Also need: 
  * https://github.com/cjmet/AngelHornetLibrary
* ...


## Known Issues
* Maui apps have a virtualized and redirected file system.  This can cause issues with file paths and locations.
  * Since I'm not officially publishing, I have to "guess" where this directory will end up.
* The first time you load the Maui App, it will scan your %userprofile%/music, this may take a while.
  * Seconds for my local machine, 25 minutes for a LAN NAS, and Hours for a WAN NAS.
  * This scan will ***NOT*** follow reparse points.  This may cause it to miss some redirected paths, particularly with Onedrive.  If that happens you can use the manual scan button.
* Swagger can only load about 1000 songs, any more and it locks up.  Use Postman if you want to test a larger query.
* * GUI responsiveness suffers to SMB WAN Operations.  
  * This is in some cases lagging the entire OS, not just the application.  This is as much an OS issue as programming issue.  
  * I've further isolated the synchronous operations into a sub-task, which has helped, but not entirely solved the issue.


## Suggestions
* If you want cross-platform compatibility, keep at least an 'android' project target enabled at all times. And probably test it once a day.
* MAUI Debugging is sometimes fairly generic and unhelpful. 
* Buy a Wholesale Truckload of Salt: 
  * Advice from earlier than Dec 2023 may be outdated or even incorrect.
  * However, a fair amount of earlier advice and info on Xamarin can still be helpful.  Just use it with extreme caution, particularly on any more complicated issues.

<br>

## Current Project Questions
1. &nbsp; Class: API Authentication and Validation
1. &nbsp; The Sync / Async Task buried in the middle of the converted Advanced Search inside DataLibrary.  I need to learn how to comprehend and fix this particular pattern.
---
1. &nbsp; There HAS to be a better way to cause a binding value to update on an event triggering?
    ```
    var tmp = _label.BindingContext;
    _label.BindingContext = null;
    _label.BindingContext = tmp;
    ```
1. &nbsp; IQueryable and Union, Intersect, Except?
1. &nbsp; Predicate Builder for search and filter.
1. &nbsp; ExecuteUpdate on linked Songs item in Playlists? 
---
1. &nbsp; What is the correct place to put the second window's initialization code?
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
* ### To-Do List
- [ ] More API work
---
- [ ] Rework to scan filenames and pathnames only first, partially filling in song info. Then go back and scan and decode the id3 headers to fill in the rest of the information.  
- [ ] QueenBee Controller to monitor and direct all the worker tasks.  Add redundancy and restarts as well as monitoring to the background task(s).   We currently can only get about 1000 songs at a time over the wan before it breaks due to a time-out or noise on the lines.
- [ ] Modify GetFilesAsync to use a List<string> of search patterns.  Aka: "mp3", "flac", "wav", "m4a", "mp4", "wma", "aac", "alac" ...  mpeg4, mpeg3, mpeg2, adts, asf, riff, avi, ac-3, amr, 3gp, flac, wav
- [ ] Consolidate a single log-file reader that sends data to both the log-viewer and message queue.
- [ ] Add a drive check first time we touch each drive ... cache the result.   That way we don't keep pinging offline or malfunctioning drives.


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
- [ ] /MauiProgramLogic/MauiProgramLogic.cs
- [ ] Integration of Playlists, Songs, Search and GUI
  - [x] Playlist Selection GUI
  - [x] Initial Integration of Playlists and Songs
  - [x] Random, Loop Buttons, Start, End, Next, Previous, ...
  - [ ] Adding Songs to Playlists GUI (Update)
  - [ ] Removing Songs from Playlists GUI (Update)
  - [ ] Adding Playlists GUI (Create)
  - [ ] Deleting Playlists GUI (Delete)
  - [x] Basic Search and Filter functionality GUI
  - [x] Advanced Search and Filter First Pass
  - [x] Advanced Search and Filter Second Pass
  

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
- [x] Extract ScanForMedia() into /CommonLibrary/CommonProgramLogic.cs 
- [ ] Work on general logic around the DB and App
  - [x] Id each song by filename to eliminate duplicates.
    - [ ] Uniquely ID each song to eliminate duplicates, including duplicates with different filenames
- [x] Create Random Playlists for Testing
- [ ] CRUD  (No this isn't the best way to implement this. Ideally it will go down into a repository.)
    - [x] Create
    - [x] Read
    - [ ] Update
    - [ ] Delete
- [x] More Fluent Advanced Search with Parsing, to replace both Search and Advanced Search.

   
* ### Data Library
- [x] Store in SQLite database
- [x] Integrate with Music Player
- [x] Add the rest of the data structures back in, and to the Data Library.
- [x] Move data files back out into a DataLibrary for easier management. 
- [x] Initial Integration of Playlists and Songs
- [x] Searching and Adding Songs to Db
- [ ] Adding Songs to Playlists
- [ ] Removing Songs from Playlists
- [ ] Adding Playlists
- [ ] Deleting Playlists
- [ ] DataLibary -> Models, Interfaces, and (Services or Contexts)
- [ ] Implement the Interfaces and Repository Pattern.


* ### Music Library Web API
- [x] Implement the Interfaces and Repository Pattern.
- [x] Add an API Project to the solution
- [x] Develop basic API to match basic functionality of Music Library
  - [x] Basic Playlist API
  - [x] Basic Song API
  - [x] Basic CRUD API  (Songlist 1 is partially restricted, for internal use.)
- [ ] Playlist Validation
- [ ] Song Validation
- [ ] Orphaned Songs from Playlist API
- [x] API Locking Playlist 1 as internal "All Songs" Playlist". This is only locked in the API.

<br>

## Project Requirements (Pick 3 or more)
- [x] Async Task
  * Created an async background task to scan local and remote SMB drives for media files, using callbacks to deliver updates asynchronously.
- [x] List or Dictionary
  * Using multiple lists as well as the ConcurrentBag class in the background task.
- [x] API
  * Very Simple Default Endpoint Implementation, converted to Async and Repository Pattern.  I did add Advanced Search to the Song API, but it's hard to know where to go from there without a target for the API.
- [x] CRUD API
  * Very Simple Default Endpoint Implementation, converted to Async and Repository Pattern.
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
* Worked on API.  Added searches to the Song API.  Converted Advanced Search to DataLibrary and Repository Pattern.  Turns out that's where it should have been in the first place, instead of common.
* Advanced Search, Version 2, with simple search parsing and filtering enabled.
* Resized Event is _"working"_
* There HAS to be a better way to cause a binding value to update on an event triggering?
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
* GUI responsiveness to SMB WAN Operations.  These operations are running on separate async tasks, but the 'opening file' phase still locks the GUI.*  
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