![Angel Hornet Logo](https://github.com/cjmet/CodeKy_SD09/blob/main/Angel%20Hornet%20Logo.png)
# AhMediaPlayer
### Code Kentucky Software Development - Class Project
#### This project is for Educational purposes.
The goal is to create a simple Media Player, with Media Library and WebAPI using .Net MAUI.  

CLI (and later API and Web) application using Entity Framework Core.  The application will have a simple menu system and will allow the user to add, update?, and delete products and orders.  This is a learning experience.


### See Also: 
* One nice example of what Microsoft has done with Maui
  * https://dotnetpodcasts.azurewebsites.net/
  * https://github.com/microsoft/dotnet-podcasts

<br>

### Code Kentucky
Code Kentucky is a software development bootcamp in Louisville, Kentucky.  The course is designed to teach the fundamentals of software development and to prepare students for a career in the field.  The course is taught by experienced professionals in the field.

* https://codekentucky.org/
* https://codelouisville.org/
* https://code-you.org/

<br>

## Current Project Questions
1. &nbsp;How do I? &ensp; - &ensp; Access the log service from my other libraries and classes?
   1. I put logging into AhLog() Library, Using DI, then access it through either AhLog or DI as needed.
   1. I want it usable for CLI, Libraries, APP, and DI.
   1. Is this a reasonable way to do this?
1. &nbsp;How Do I? &ensp; - &ensp; Reference or Use MainPage.mediaElement from another file.cs class?  I could move the PlaySong() method to a library then, and re-use it on other pages later.  Dispatcher Maybe?  Syntax?
1. &nbsp;Maui Debugging?  Most crashes are "generic", and very unhelpful?  Am I missing a tool or plugin or something?
1. &nbsp;Windows.Storage.FileProperties.MusicProperties ? .Net 8? Maui?
   * This is the second time I've needed a Windows.Storage class.  Any help here would be greatly appreciated. I'm not sure what I'm doing wrong. 
1. &nbsp;Review how to Async return a value, non-blocking, from a blockable operation, using a 4 step process.  (See Sandbox)
1. &nbsp;Review of AhGetFiles.GetFilesAsync();   
<br>

## Project Plan
Create a music library Web API and simple Media Player
* ### To-Do (Other Tasks)
- [x] Logging Service.  Move all the Debug.Writeline into a logging service that: 
  - [x] Writes to Logfile.txt
  - [x] Writes to Debug.Writeline
  - [ ] Writes to a Popup Window (if in debug mode)

* ### Music Player
- [x] My First MAUI App
- [x] Initial EF Core Setup and Test
- [ ] /MauiProgramLogic/MauiProgramLogic.cs
- [x] Play a song
- [x] Play a static song from local storage
- [x] Create a Playlist class
- [x] Wire up a Button with Command and Command Parameters
- [x] Play as song from a static Playlist
- [x] Play more than one song from a static Playlist
- [x] Work on the Song UI Layout
- [ ] Work on Playlist UI


* ### Common Library 
- [x] CommonLibrary Project so that program logic can be shared, then wire up individual wrappers in a project as needed.
- [ ] Logging 
- [x] Search for MP3 files, with a Background Task.
  - [x] Initial Test Background Async Task
  - [x] Improved Background Task
  - [x] Add Callback Ability to the Background Task
  - [x] Integrate FileFind, Callback, EF Core, and Maui
  - [x] Read and use Meta Data from MP3 files
- [x] Extract ScanForMedia() into /CommonLibrary/CommonProgramLogic.cs 
- [ ] Work on general logic around the DB and App
  - [x] Id each song by filename to eliminate duplicates.
    - [ ] Uniquely ID each song to eliminate duplicates, including duplicate filenames
- [ ] Add Automatic Playlists based on Meta Data
- [ ] Generic Lockable Class.  Locked<T>.

* ### Data Library
- [x] Store in sqlite database
- [x] Integrate with Music Player
- [x] Add the rest of the data structures back in, and to the Data Library.
- [x] Move data files back out into a DataLibrary for easier management. 
- [ ] DataLibary -> Models, Interfaces, and (Services or Contexts)
- [ ] Implement the Interfaces and Repository Pattern.
- [ ] Add Search and Filter functionality

* ### Music Library Web API
- [ ] Chances are most of this work will be done in the Common and Data Library Projects
- [ ] Implement the Interfaces and Repository Pattern.
- [ ] Add an API Project to the solution
- [ ] Develop basic API to match basic functionality of Music Library
- [ ] Using the Repository Pattern and scaffolding will effetively do this for us.

<br>

## Project Requirements (Pick 3 or more)
- [x] Async Task
  * Created a background task to scan local and remote smb drives for media files, using callbacks to deliver results back to the main task asynchronously.
- [x] List or Dictionary
  * Using multiple lists as well as the ConcurrentBag class in the background task.
- [ ] API
- [ ] CRUD API
- [ ] Multiple Data Tables
- [x] Logging of Errors and Debug Info.
  * AhLog() class and Serilog
- [ ] Unit Testing
- [ ] SOLID Principles
- [ ] SQL Queries
- [ ] Regex
- [ ] Generic Class, Locked\<T>?

<br>

## Instructions
* Requires: Visual Studio 2022, C#12, .Net 8, .Net MAUI
* Use: Solution Configuration: Debug
  * This will put the DB on your desktop so that the CLI, API, and MAUI all use the same DB.  Otherwise, chaos ensues.
* You'll Also need: 
  * https://github.com/cjmet/AngelHornetLibrary
* ...

<br>

## Known Issues
* Use the latest update of Visual Studio 2022, .Net 8, and .Net MAUI
* MAUI Debugging is very generic and unhelpful.  It's hard to know what's wrong.  Any advice here would be greatly appreciated.
* %AppData% is a Different location in Win11/Console, Win11/Web, and Win11/Maui.
  * Maui has a virtual file-system redirect for %AppData%
* Extra Wholesale Bag of Salt:
  * Advice from earlier than Dec 2023 may be outdated or even incorrect.
  * However, a fair amount of earlier advice and info on Xamarin can still be helpful.  Just use it with extreme caution, particularly on any more complicated issues.
* Disable windows app virtualization of the %appdata filesystem.  This options does not appear to be available in MAUI, only older UWP.

<br>

## Dev Blog
* Worked on the second Maui Window to display AhLog() data.
* Resources are required to have Lowercase Filenames:
  * `Get-childItem "." | Rename-Item -NewName {$_.Basename.tostring().tolower() + $_.extension}`
* Added default Logging.  AhLog() class using Serilog.
* Started work on basic logging.
* Rewrote the background task for the 4th time as: async IAsyncEnumerable<string> GetFilesAsync().  This version is streaming, and more closely matches standard conventions.  It should in theory be easier to use and understand.  It was perhaps the easiest iteration to write, but also took the most lines of code.
* Started work on CommonLibrary, cleaning up code and moving common logic into the library in a more organized and readable way.
* Fixed the playlist to continue playing ... but still lots of work to do.
* 😲 I had no idea this was a thing: &nbsp;&nbsp; `var _sourceSongList = TestSonglist.ItemsSource.Cast\<Song>().ToList();` &nbsp;&nbsp; This was exactly what I needed, and I found it almost entirely by accident reading almost completely unrelated code.
* Wired up the Song List, and now can select and play a song from the list.  I am going to need to redo whole sections of this code now that I have a better understanding of everything so far.
* When avialable, there's a very tiny '+' button when writing an event handler.  USE IT!  Each one is just enough different to typo something, and waste time otherwise.
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
  * I'm not at all certain where various peices of code should go.  It's all very messy right now while I'm still learning to crawl around slowly 
* **%AppData% is Different in Win11/Console, Win11/Web, and Win11/Maui**
  * This means the database files were being stored in different locations.
  * Win11 is: "C:\Users\Username\AppData\Local\test_playlists.db"
  * MAUI  is: "C:\Users\Username\AppData\Local\Packages\ ... mauimediaplayer ... \LocalCache\Local\test_playlists.db"
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