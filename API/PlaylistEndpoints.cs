using Microsoft.EntityFrameworkCore;
using DataLibrary;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OpenApi;
namespace API;

public static class PlaylistEndpoints
{
    public static void MapPlaylistEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/Playlist").WithTags(nameof(Playlist));

        group.MapGet("/", async (IPlaylistRepository repository) =>
        {
            return await repository.GetAllPlaylistsAsync();
        })
        .WithName("GetAllPlaylists")
        .WithOpenApi();

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

        group.MapPut("/{id}", async Task<Results<Ok,NotFound>> (int id, Playlist playlist, IPlaylistRepository repository) =>
        {
            var result = await repository.UpdatePlaylistAsync(id, playlist);
            return result > 0
                ? TypedResults.Ok()
                : TypedResults.NotFound();
        })
        .WithName("UpdatePlaylist")
        .WithOpenApi();

        // This one was a little confusing.  I was expecting: Task<Results<Created<Playlist>,BadRequest>>,
        // but it is actually:  Task<Results<Created<Playlist>,BadRequest>>
        group.MapPost("/", async Task<Results<Created<Playlist>,BadRequest>> (Playlist playlist, IPlaylistRepository repository) =>
        {
            var result = await repository.AddPlaylistAsync(playlist);
            return result > 0
                ? TypedResults.Created($"/api/Playlist/{playlist.Id}", playlist)
                : TypedResults.BadRequest();
        })
        .WithName("CreatePlaylist")
        .WithOpenApi();

        group.MapDelete("/{id}", async Task<Results<Ok,NotFound>> (int id, IPlaylistRepository repository) =>
        {
            var result = await repository.DeletePlaylistAsync(id);
            return result > 0                 
                ? TypedResults.Ok()
                : TypedResults.NotFound();
        })
        .WithName("DeletePlaylist")
        .WithOpenApi();
    }
}
