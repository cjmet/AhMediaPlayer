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

* ### Music Player
- [x] My First MAUI App
- [ ] Initial Background Async Task
- [ ] Play a song
- [ ] Play a static song from local storage
- [ ] Create a Playlist class
- [ ] Play as song from a static Playlist
- [ ] Play more than one song from a static Playlist
- [ ] Integrate with Music Library

* ### Music Library
- [ ] Search for MP3 files.
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
* The Desktop just repeatedly crashed with little information.  However, the notebook at home gave meaninful feedback and helped me track down the key issue.
* MAUI: Data Annotations may not always work, use modelBuilder.Entity<Playlist>().HasKey(p => p.Id); instead.
 
<br>
 	
## Dev Blog
*  [Key] Annotations do not seem to work in EF Maui.
	* modelBuilder.Entity<Playlist>().HasKey(p => p.Id);
*  Remember to set your Id properties to Public!
*  Remember to set your Interfaces and DbContext to Public!
*  A start on the Initial Background Task.
*  My First MAUI App