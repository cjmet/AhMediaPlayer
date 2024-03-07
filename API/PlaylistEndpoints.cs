using Microsoft.EntityFrameworkCore;
using DataLibrary;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ApiEndpoints;
namespace API;

public static class PlaylistEndpoints
{
    public static void MapPlaylistEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/Playlist").WithTags(nameof(Playlist));

       
        group.MapGet("/", async Task<Results<Ok<List<Playlist>>, NotFound>> (IPlaylistRepository repository) =>
        {
            var playlists = await repository.GetAllPlaylistsAsync();
            return playlists != null
                ? TypedResults.Ok(playlists)
                : TypedResults.NotFound();
        })
        .WithName("GetAllPlaylists")
        .WithOpenApi();


        group.MapGet("/RequiresAuthentication", async Task<Results<Ok<List<Playlist>>, NotFound>> (IPlaylistRepository repository) =>
        {
            var playlists =  await repository.GetAllPlaylistsAsync();
            return playlists != null
                ? TypedResults.Ok(playlists)
                : TypedResults.NotFound();
        })
        .WithName("RequiresAuthentication")
        .WithOpenApi()
        .RequireAuthorization();


        // Holy Hand Grenades, the following syntax was complicated, and triple-nested!
        group.MapGet("/{id}", async Task<Results<Ok<Playlist>, NotFound>> (int id, IPlaylistRepository repository) =>
        {
            var playlist = await repository.GetPlaylistByIdAsync(id);
            return playlist != null
                ? TypedResults.Ok(playlist)
                : TypedResults.NotFound();
        })
        .WithName("GetPlaylistById")
        .WithOpenApi();


        group.MapGet("/GetPlaylistSongs/{id}", async Task<Results<Ok<Playlist>, NotFound>> (int id, IPlaylistRepository repository) =>
        {
            var playlist = await repository.GetPlaylistWithSongsAsync(id);
            return playlist != null
                ? TypedResults.Ok(playlist)
                : TypedResults.NotFound();
        })
        .WithName("GetPlaylistsongs")
        .WithOpenApi();


        group.MapPut("/{id}", async Task<Results<Ok,NotFound,UnauthorizedHttpResult>> (int id, Playlist playlist, IPlaylistRepository repository) =>
        {
            if (id == 1) return TypedResults.Unauthorized();
            var result = await repository.UpdatePlaylistAsync(id, playlist);
            return result > 0
                ? TypedResults.Ok()
                : TypedResults.NotFound();
        })
        .WithName("UpdatePlaylist")
        .WithOpenApi();

        
        // This one was a little confusing.
        group.MapPost("/", async Task<Results<Created<Playlist>,BadRequest>> (Playlist playlist, IPlaylistRepository repository) =>
        {
            var result = await repository.AddPlaylistAsync(playlist);
            return result > 0
                ? TypedResults.Created($"/api/Playlist/{playlist.Id}", playlist)
                : TypedResults.BadRequest();
        })
        .WithName("CreatePlaylist")
        .WithOpenApi();

        
        group.MapDelete("/{id}", async Task<Results<Ok,NotFound,UnauthorizedHttpResult>> (int id, IPlaylistRepository repository) =>
        {
            if (id == 1) return TypedResults.Unauthorized();
            var result = await repository.DeletePlaylistAsync(id);
            return result > 0                 
                ? TypedResults.Ok()
                : TypedResults.NotFound();
        })
        .WithName("DeletePlaylist")
        .WithOpenApi();


        group.MapPost("/PlaylistAddSong", async Task<Results<Created<Playlist>,BadRequest,UnauthorizedHttpResult>> (int playlistId, int songId, IPlaylistRepository repository) =>
        {
            if (playlistId == 1) return TypedResults.Unauthorized();
            var playlist = await repository.GetPlaylistByIdAsync(playlistId);
            if (playlist == null) return TypedResults.BadRequest();

            var result = await repository.AddSongToPlaylistAsync(playlistId, songId);
            return result > 0
                ? TypedResults.Created($"/api/Playlist/{playlist.Id}", playlist)
                : TypedResults.BadRequest();
        })
        .WithName("PlaylistAddSong")
        .WithOpenApi();


        group.MapDelete("/PlaylistRemoveSong", async Task<Results<Created<Playlist>, BadRequest, UnauthorizedHttpResult>> (int playlistId, int songId, IPlaylistRepository repository) =>
        {
            if (playlistId == 1) return TypedResults.Unauthorized();
            var playlist = await repository.GetPlaylistByIdAsync(playlistId);
            if (playlist == null) return TypedResults.BadRequest();

            var result = await repository.RemoveSongFromPlaylist(playlistId, songId);
            return result > 0
                ? TypedResults.Created($"/api/Playlist/{playlist.Id}", playlist)
                : TypedResults.BadRequest();
        })
        .WithName("PlaylistRemoveSong")
        .WithOpenApi();


    }
}
