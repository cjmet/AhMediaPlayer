using Microsoft.EntityFrameworkCore;
using DataLibrary;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OpenApi;
using NuGet.Protocol.Core.Types;
namespace API;

public static class SongEndpoints
{
    public static void MapSongEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/Song").WithTags(nameof(Song));


        group.MapGet("/", async (ISongRepository _repository) =>
        {
            return await _repository.GetAllSongsAsync();
        })
        .WithName("GetAllSongs")
        .WithOpenApi();


        group.MapGet("/Search", async (string _search, ISongRepository _repository) =>
        {
            return await _repository.SearchAllSongs(_search);
        })
        .WithName("Search")
        .WithOpenApi();


        group.MapGet("/SearchQuery", async (string _property, string _search, ISongRepository _repository) =>
        {
            return await _repository.SearchQuery(_property, _search);
        })
        .WithName("SearchQuery")
        .WithOpenApi();


        group.MapGet("/SearchAdvanced", async (string _advancedSearch, ISongRepository _repository) =>
        {
            var _list = new List<Song>();

            (_list, _, _ ) = await _repository.AdvancedSearchRepository(_list, _advancedSearch);
            return _list;
        })
        .WithName("SearchAdvanced")
        .WithOpenApi();


        group.MapGet("/{id}", async Task<Results<Ok<Song>, NotFound>> (int id, ISongRepository _repository) =>
        {
            var song = await _repository.GetSongByIdAsync(id);
            return song != null
                ? TypedResults.Ok(song)
                : TypedResults.NotFound();
        })
        .WithName("GetSongById")
        .WithOpenApi();


        group.MapPut("/{id}", async Task<Results<Ok, NotFound>> (int id, Song song, ISongRepository
            _repository) =>
        {
            var result = await _repository.UpdateSongAsync(id, song);
            return result > 0
                ? TypedResults.Ok()
                : TypedResults.NotFound();
        })
        .WithName("UpdateSong")
        .WithOpenApi();


        // the confusing one again.
        group.MapPost("/", async Task<Results<Created<Song>, BadRequest>> (Song song, ISongRepository _repository) =>
        {
            var result = await _repository.AddSongAsync(song);
            return result > 0
                ? TypedResults.Created($"/api/Song/{song.Id}", song)
                : TypedResults.BadRequest();
        })
        .WithName("CreateSong")
        .WithOpenApi();


        group.MapDelete("/{id}", async Task<Results<Ok, NotFound>> (int id, ISongRepository _repository) =>
        {
            var result = await _repository.DeleteSongAsync(id);
            return result >= 1
                ? TypedResults.Ok()
                : TypedResults.NotFound();
        })
        .WithName("DeleteSong")
        .WithOpenApi();
    }
}
