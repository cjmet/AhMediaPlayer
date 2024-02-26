![Angel Hornet Logo](https://github.com/cjmet/CodeKy_SD09/blob/main/Angel%20Hornet%20Logo.png)
# Angel Hornet Media Player
### Code Kentucky Software Development - Class Project
#### This project is for Educational purposes.
The goal is to create a simple Media Player, with Media Library and WebAPI using .Net MAUI.  

CLI (and later API and Web) application using Entity Framework Core.  The application will have a simple system to Play MP3s, ... and maybe Add, Update, and Delete Songs and Playlists.  This is a learning experience.


### See Also: 
* One nice example of what Microsoft has done with Maui
  * https://dotnetpodcasts.azurewebsites.net/
  * https://github.com/microsoft/dotnet-podcasts

<br>

### Code Kentucky
Code Kentucky is a software development boot camp in Louisville, Kentucky.  The course is designed to teach the fundamentals of software development and to prepare students for a career in the field.  The course is taught by experienced professionals in the field.

* https://codekentucky.org/
* https://codelouisville.org/
* https://code-you.org/

<br>

## Instructions
* Requires: Visual Studio 2022, C#12, .Net 8, .Net MAUI
* Use: Solution Configuration: Debug
  * This will put the DB on your desktop so that the CLI, API, and MAUI all use the same DB.  Otherwise, chaos ensues ... due to windows app virtualization of the %AppData file-system.
* You'll Also need: 
  * https://github.com/cjmet/AngelHornetLibrary
* ...

<br>

## Current Project Questions
1. &nbsp; ExecuteUpdate on linked Songs item in Playlists?  See code.
1. &nbsp; Program Logic? of Locking Playlist 1 as internal "All Songs" Playlist"?  Does this go in the API, Interface, or Repository? ... I Feel like this should be in the repository so that incoming API calls can't Fubar a database?  Deleting from Allsongs would also be different, in that it will just be added back next time it scans.... shouldn't we mark it deleted and hide it instead?  Or is this getting too much into the program logic arena?  And should go into a shared program logic?  But then how do you add that to an API, such that the raw API doesn't fubar working Db Systems and other shared applications?
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
1. &nbsp; Maui Debugging?  Most crashes are "generic", and very unhelpful?  Am I missing a tool or plug-in or something?
1. &nbsp; Windows.Storage.FileProperties.MusicProperties ? .Net 8? Maui?
   * This is the second time I've needed a Windows.Storage class.  Any help here would be greatly appreciated. I'm not sure what I'm doing wrong. 
   * On a related Note.  Always keep at least one 'other' target enabled, IE: 'Android'.  I disabled the other targets to make it simpler while learning and building ... and as a side-effect it enabled 'Windows.\*', including Windows.Storage.  But now I can't re-enable android targets without a major refactor.  Oops.
1. &nbsp; Review how to Async return a value, non-blocking, from a block-able operation, using a 4 step process.  (See Sandbox)
1. &nbsp; Review of AhGetFiles.GetFilesAsync() and/or AhFileIO.PollLinesAsync().
<br>

## Project Plan
Create a music library Web API and simple Media Player
* ### To-Do List
- [ ] More API work

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
- [x] Logging Service.  Move all the Debug.Writeline into a logging service that: 
  - [x] Writes to a Pop-Up Window (if in debug mode)
  - [x] Improved Logging Window
  - [x] `public async IAsyncEnumerable<string> PollLinesAsync(path, pollInterval, CancellationToken?)`
- [ ] /MauiProgramLogic/MauiProgramLogic.cs
- [ ] Integration of Playlists, Songs, Search and GUI
  - [x] Playlist Selection GUI
  - [x] Initial Integration of Playlists and Songs
  - [ ] Adding Songs to Playlists GUI (Update)
  - [ ] Removing Songs from Playlists GUI (Update)
  - [ ] Adding Playlists GUI (Create)
  - [ ] Deleting Playlists GUI (Delete)
  - [ ] Search and Filter functionality GUI
  - [ ] Add Automatic Playlists based on Meta Data GUI
- [ ] Consolidate a single log-file reader that sends data to both the log-viewer and message queue.
- [ ] Handling of completely unplayable fubar mp3 files that aren't even mp3 files?  Right now they just don't play, and stop the playlist until you continue to the next song.


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
- [ ] Generic Lockable Class: `Locked <T>`


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
- [ ] Develop basic API to match basic functionality of Music Library
  - [x] Basic Playlist API
  - [ ] Basic Song API
  - [ ] Playlist Validation
  - [ ] Song Validation
  - [ ] Orphaned Songs from Playlist API
  - [ ] Program Logic of Locking Playlist 1 as internal "All Songs" Playlist"?  Does this go in the API, Interface, or Repository?

<br>

## Project Requirements (Pick 3 or more)
- [x] Async Task
  * Created an async background task to scan local and remote SMB drives for media files, using callbacks to deliver updates asynchronously.
- [x] List or Dictionary
  * Using multiple lists as well as the ConcurrentBag class in the background task.
- [ ] API
  * Not yet implemented
- [ ] CRUD API
  * Not yet implemented
- [x] Multiple Data Tables
  * modelBuilder.Entity<Song>().HasMany(s => s.Playlists).WithMany(p => p.Songs);
- [x] Logging of Errors and Debug Info.
  * AhLog() class and Serilog
- [x] Regex
  * Regex to clean up and convert Title to AlphaTitle to sort by
- [ ] Unit Testing
  * Not yet implemented
- [ ] ~~SOLID Principles~~
  * I'll do a better job of this in the future, and as I have time to refactor and clean things up.  I was too entirly clueless and stumbling around in the dark to properly implement a good SOLID plan.
- [ ] ~~SQL Queries~~
  * Unlikely to be used in this project.
- [ ] ~~Generic Class~~
  * Unlikely to be used in this project.

<br>

## Known Issues
* If you want cross-platform compatibility, keep at least an 'android' project target enabled at all times. And probably test it once a day.
  * I disabled all the other targets for simplicity while learning, but that also allowed the project to introduce and use incompatible libraries.
  * At some point in the future I'll need to do a major refactor to fix this and enable android.
* Use the latest update of Visual Studio 2022, .Net 8, and .Net MAUI
* MAUI Debugging is very generic and unhelpful.  It's hard to know what's wrong.  Any advice here would be greatly appreciated.
* %AppData% is a Different location in Win11/Console, Win11/Web, and Win11/Maui.
  * Maui has a virtual file-system redirect for %AppData%
* Extra Wholesale Bag of Salt:
  * Advice from earlier than Dec 2023 may be outdated or even incorrect.
  * However, a fair amount of earlier advice and info on Xamarin can still be helpful.  Just use it with extreme caution, particularly on any more complicated issues.
* Disable windows app virtualization of the %AppData file-system.  This options does not appear to be available in MAUI, only older UWP.

<br>

## Dev Blog
* Implemented the First Pass Playlist API, need to work on the Songlist API next.
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