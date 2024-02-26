using Microsoft.EntityFrameworkCore;
using DataLibrary;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OpenApi;
namespace API;

public static class SongEndpoints
{
    public static void MapSongEndpoints (this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/Song").WithTags(nameof(Song));

        group.MapGet("/", async (PlaylistContext db) =>
        {
            return await db.Songs.ToListAsync();
        })
        .WithName("GetAllSongs")
        .WithOpenApi();

        group.MapGet("/{id}", async Task<Results<Ok<Song>, NotFound>> (int id, PlaylistContext db) =>
        {
            return await db.Songs.AsNoTracking()
                .FirstOrDefaultAsync(model => model.Id == id)
                is Song model
                    ? TypedResults.Ok(model)
                    : TypedResults.NotFound();
        })
        .WithName("GetSongById")
        .WithOpenApi();

        group.MapPut("/{id}", async Task<Results<Ok, NotFound>> (int id, Song song, PlaylistContext db) =>
        {
            var affected = await db.Songs
                .Where(model => model.Id == id)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(m => m.Id, song.Id)
                    .SetProperty(m => m.PathName, song.PathName)
                    .SetProperty(m => m.Title, song.Title)
                    .SetProperty(m => m.AlphaTitle, song.AlphaTitle)
                    .SetProperty(m => m.Artist, song.Artist)
                    .SetProperty(m => m.Band, song.Band)
                    .SetProperty(m => m.Album, song.Album)
                    .SetProperty(m => m.Track, song.Track)
                    .SetProperty(m => m.Year, song.Year)
                    .SetProperty(m => m.Genre, song.Genre)
                    .SetProperty(m => m.Length, song.Length)
                    );
            return affected == 1 ? TypedResults.Ok() : TypedResults.NotFound();
        })
        .WithName("UpdateSong")
        .WithOpenApi();

        group.MapPost("/", async (Song song, PlaylistContext db) =>
        {
            db.Songs.Add(song);
            await db.SaveChangesAsync();
            return TypedResults.Created($"/api/Song/{song.Id}",song);
        })
        .WithName("CreateSong")
        .WithOpenApi();

        group.MapDelete("/{id}", async Task<Results<Ok, NotFound>> (int id, PlaylistContext db) =>
        {
            var affected = await db.Songs
                .Where(model => model.Id == id)
                .ExecuteDeleteAsync();
            return affected == 1 ? TypedResults.Ok() : TypedResults.NotFound();
        })
        .WithName("DeleteSong")
        .WithOpenApi();
    }
}
