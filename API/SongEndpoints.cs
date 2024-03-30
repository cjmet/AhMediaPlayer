//nuget Microsoft.AspNetCore.Identity.EntityFrameworkCore
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

using Microsoft.EntityFrameworkCore;
using DataLibrary;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OpenApi;
using NuGet.Protocol.Core.Types;
using AhConfig;

namespace API;

public static class SongEndpoints
{
    public static void MapSongEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/Song").WithTags(nameof(Song));


        group.MapGet("/", async Task<Results<Ok<List<Song>>, NotFound>> (ISongRepository _repository) =>
        {
            var _results = await _repository.GetAllSongsAsync();
            if (Const.ApiDemoMode && _results.Count > Const.ApiDemoMax) _results = _results.GetRange(0, Const.ApiDemoMax);
            return _results != null
                ? TypedResults.Ok(_results)
                : TypedResults.NotFound();
        })
        .WithName("GetAllSongs")
        .WithOpenApi();


        group.MapGet("/Search", async Task<Results<Ok<List<Song>>, NotFound>> (string _search, ISongRepository _repository) =>
        {
            var _results = await _repository.SearchAllSongs(_search);
            if (Const.ApiDemoMode && _results.Count > Const.ApiDemoMax) _results = _results.GetRange(0, Const.ApiDemoMax);
            return _results != null
                ? TypedResults.Ok(_results)
                : TypedResults.NotFound();
        })
        .WithName("Search")
        .WithOpenApi();


        group.MapGet("/SearchQuery", async Task<Results<Ok<List<Song>>, NotFound>> (string _property, string _search, ISongRepository _repository) =>
        {
            var _results = await _repository.SearchQuery(_property, _search);
            if (Const.ApiDemoMode && _results.Count > Const.ApiDemoMax) _results = _results.GetRange(0, Const.ApiDemoMax);
            return _results != null
                ? TypedResults.Ok(_results)
                : TypedResults.NotFound();
        })
        .WithName("SearchQuery")
        .WithOpenApi();


        group.MapGet("/SearchAdvanced", async Task<Results<Ok<List<Song>>, NotFound>> (string _advancedSearch, ISongRepository _repository) =>
        {
            var _results = new List<Song>();

            (_results, _, _ ) = await _repository.AdvancedSearchRepository(_results, _advancedSearch);
            if (Const.ApiDemoMode && _results.Count > Const.ApiDemoMax) _results = _results.GetRange(0, Const.ApiDemoMax);
            return _results != null
                  ? TypedResults.Ok(_results)
                  : TypedResults.NotFound();
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


        group.MapPut("/{id}", async Task<Results<Ok, NotFound, UnauthorizedHttpResult>> (int id, Song song, ISongRepository
            _repository) =>
        {
            if (Const.ApiDenySongAdmin) return TypedResults.Unauthorized();
            var result = await _repository.UpdateSongAsync(id, song);
            return result > 0
                ? TypedResults.Ok()
                : TypedResults.NotFound();
        })
        .WithName("UpdateSong")
        .WithOpenApi();


        // the confusing one again.
        group.MapPost("/", async Task<Results<Created<Song>, BadRequest, UnauthorizedHttpResult>> (Song song, ISongRepository _repository) =>
        {
            if (Const.ApiDenySongAdmin) return TypedResults.Unauthorized();
            var result = await _repository.AddSongAsync(song);
            return result > 0
                ? TypedResults.Created($"/api/Song/{song.Id}", song)
                : TypedResults.BadRequest();
        })
        .WithName("CreateSong")
        .WithOpenApi();


        group.MapDelete("/{id}", async Task<Results<Ok, NotFound, UnauthorizedHttpResult>> (int id, ISongRepository _repository) =>
        {
            if (Const.ApiDenySongAdmin) return TypedResults.Unauthorized();
            var result = await _repository.DeleteSongAsync(id);
            return result >= 1
                ? TypedResults.Ok()
                : TypedResults.NotFound();
        })
        .WithName("DeleteSong")
        .WithOpenApi();
    }
}
