![Angel Hornet Logo](https://github.com/cjmet/CodeKy_SD09/blob/main/Angel%20Hornet%20Logo.png)
# AhMediaPlayer
### Code Kentucky Software Development - Class Project
#### This project is for Educational purposes.
The goal is to create a simple Media Player, with Media Library and WebAPI using .Net MAUI.  

CLI (and later API and Web) application using Entity Framework Core.  The application will have a simple menu system and will allow the user to add, update?, and delete products and orders.  This is a learning experience.

**See Also:** https://github.com/cjmet/AngelHornetLibrary

<br>

### Code Kentucky
Code Kentucky is a software development bootcamp in Louisville, Kentucky.  The course is designed to teach the fundamentals of software development and to prepare students for a career in the field.  The course is taught by experienced professionals in the field.

https://codekentucky.org/

https://codelouisville.org/

https://code-you.org/

<br>

## Project Plan
Create a music library Web API and simple Media Player

## Current Questions
* How to communicate between the background task and main task?
   * Version 1 used unsafe refs
   * Version 2 is using pulling and locking the data from the Background Task
   * Version 3 is using callback with locking, concurrency, and done signal from the background task to the main.
*  How to Async return a value, non-blocking, from a blockable operation

* ### Music Player
- [x] My First MAUI App
- [x] Initial Test Background Async Task
- [x] Improved Background Task
- [x] Initial EF Core Setup and Test
- [x] Add Callback to the Background Task
- [ ] Integrate FileFind Callback with EF Core.
- [ ] Move data files back out into a DataLibrary .Net8 for easier management. Particularly if we have any more migration issues and need to use copied and simplified CLI versions  of the data files for migrations.
- [ ] Add the rest of the data structures back in, and to the Data Library.
- [ ] Play a song
- [ ] Play a static song from local storage
- [ ] Create a Playlist class
- [ ] Play as song from a static Playlist
- [ ] Play more than one song from a static Playlist
- [ ] Integrate with Music Library

* ### Music Library
- [ ] Search for MP3 files, with a Background Task.
- [ ] Read Meta Data from MP3 files
- [ ] Store in sqlite database
- [ ] Integrate with Music Player
- [ ] Add Search and Filter functionality
- [ ] Add Automatic Playlists based on Meta Data

* ### Music Library Web API
- [ ] Chances are most of this work will be done in the Music Library Project
- [ ] Add an API Project to the solution
- [ ] Develop basic API to match basic functionality of Music Library

<br>

## Project Requirements
- [ ] Async Task
- [ ] List or Dictionary
- [ ] API
- [ ] Logging
- [ ] Unit Testing


<br>

## Known Issues
* ...

<br>

## Dev Blog
* Rewrote FindFiles for the third time, to use a callback from the background task to the main task.
*  Improved the Background Task in preparation for using an action task to integrate it with EF Core
*  Made progress on window size and location, as well as learning the initial basics of xaml layout.
*  **MAKE SURE your DbSet data uses PROPERTIES and not fields nor variables!**
  * USE: **public string Name { get; set; }**
  * not: _public string Name;_
*  Remember to set your Id properties to Public!
*  Remember to set your Interfaces and DbContext to Public!
*  A start on the Initial Background Task.
*  My First MAUI App