﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Entities.Enums;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data.Repositories;

[Flags]
public enum LibraryIncludes
{
    None = 1,
    Series = 2,
    AppUser = 4,
    Folders = 8,
    // Ratings = 16
}

public interface ILibraryRepository
{
    void Add(Library library);
    void Update(Library library);
    void Delete(Library library);
    Task<IEnumerable<LibraryDto>> GetLibraryDtosAsync();
    Task<bool> LibraryExists(string libraryName);
    Task<Library> GetLibraryForIdAsync(int libraryId, LibraryIncludes includes);
    Task<Library> GetFullLibraryForIdAsync(int libraryId);
    Task<Library> GetFullLibraryForIdAsync(int libraryId, int seriesId);
    Task<IEnumerable<LibraryDto>> GetLibraryDtosForUsernameAsync(string userName);
    Task<IEnumerable<Library>> GetLibrariesAsync();
    Task<bool> DeleteLibrary(int libraryId);
    Task<IEnumerable<Library>> GetLibrariesForUserIdAsync(int userId);
    Task<LibraryType> GetLibraryTypeAsync(int libraryId);
    Task<IEnumerable<Library>> GetLibraryForIdsAsync(IList<int> libraryIds);
    Task<int> GetTotalFiles();
}

public class LibraryRepository : ILibraryRepository
{
    private readonly DataContext _context;
    private readonly IMapper _mapper;

    public LibraryRepository(DataContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public void Add(Library library)
    {
        _context.Library.Add(library);
    }

    public void Update(Library library)
    {
        _context.Entry(library).State = EntityState.Modified;
    }

    public void Delete(Library library)
    {
        _context.Library.Remove(library);
    }

    public async Task<IEnumerable<LibraryDto>> GetLibraryDtosForUsernameAsync(string userName)
    {
        return await _context.Library
            .Include(l => l.AppUsers)
            .Where(library => library.AppUsers.Any(x => x.UserName == userName))
            .OrderBy(l => l.Name)
            .ProjectTo<LibraryDto>(_mapper.ConfigurationProvider)
            .AsNoTracking()
            .AsSingleQuery()
            .ToListAsync();
    }

    public async Task<IEnumerable<Library>> GetLibrariesAsync()
    {
        return await _context.Library
            .Include(l => l.AppUsers)
            .ToListAsync();
    }

    public async Task<bool> DeleteLibrary(int libraryId)
    {
        var library = await GetLibraryForIdAsync(libraryId, LibraryIncludes.Folders | LibraryIncludes.Series);
        _context.Library.Remove(library);

        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<IEnumerable<Library>> GetLibrariesForUserIdAsync(int userId)
    {
        return await _context.Library
            .Include(l => l.AppUsers)
            .Where(l => l.AppUsers.Select(ap => ap.Id).Contains(userId))
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<LibraryType> GetLibraryTypeAsync(int libraryId)
    {
        return await _context.Library
            .Where(l => l.Id == libraryId)
            .AsNoTracking()
            .Select(l => l.Type)
            .SingleAsync();
    }

    public async Task<IEnumerable<Library>> GetLibraryForIdsAsync(IList<int> libraryIds)
    {
        return await _context.Library
            .Where(x => libraryIds.Contains(x.Id))
            .ToListAsync();
    }

    public async Task<int> GetTotalFiles()
    {
        return await _context.MangaFile.CountAsync();
    }

    public async Task<IEnumerable<LibraryDto>> GetLibraryDtosAsync()
    {
        return await _context.Library
            .Include(f => f.Folders)
            .OrderBy(l => l.Name)
            .ProjectTo<LibraryDto>(_mapper.ConfigurationProvider)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Library> GetLibraryForIdAsync(int libraryId, LibraryIncludes includes)
    {

        var query = _context.Library
            .Where(x => x.Id == libraryId);

        query = AddIncludesToQuery(query, includes);
        return await query.SingleAsync();
    }

    private static IQueryable<Library> AddIncludesToQuery(IQueryable<Library> query, LibraryIncludes includeFlags)
    {
        if (includeFlags.HasFlag(LibraryIncludes.Folders))
        {
            query = query.Include(l => l.Folders);
        }

        if (includeFlags.HasFlag(LibraryIncludes.Series))
        {
            query = query.Include(l => l.Series);
        }

        if (includeFlags.HasFlag(LibraryIncludes.AppUser))
        {
            query = query.Include(l => l.AppUsers);
        }

        return query;
    }


    /// <summary>
    /// This returns a Library with all it's Series -> Volumes -> Chapters. This is expensive. Should only be called when needed.
    /// </summary>
    /// <param name="libraryId"></param>
    /// <returns></returns>
    public async Task<Library> GetFullLibraryForIdAsync(int libraryId)
    {
        return await _context.Library
            .Where(x => x.Id == libraryId)
            .Include(f => f.Folders)
            .Include(l => l.Series)
            .ThenInclude(s => s.Metadata)
            .Include(l => l.Series)
            .ThenInclude(s => s.Volumes)
            .ThenInclude(v => v.Chapters)
            .ThenInclude(c => c.Files)
            .AsSplitQuery()
            .SingleAsync();
    }

    /// <summary>
    /// This is a heavy call, pulls all entities for a Library, except this version only grabs for one series id
    /// </summary>
    /// <param name="libraryId"></param>
    /// <param name="seriesId"></param>
    /// <returns></returns>
    public async Task<Library> GetFullLibraryForIdAsync(int libraryId, int seriesId)
    {

        return await _context.Library
            .Where(x => x.Id == libraryId)
            .Include(f => f.Folders)
            .Include(l => l.Series.Where(s => s.Id == seriesId))
            .ThenInclude(s => s.Metadata)
            .Include(l => l.Series.Where(s => s.Id == seriesId))
            .ThenInclude(s => s.Volumes)
            .ThenInclude(v => v.Chapters)
            .ThenInclude(c => c.Files)
            .AsSplitQuery()
            .SingleAsync();
    }

    public async Task<bool> LibraryExists(string libraryName)
    {
        return await _context.Library
            .AsNoTracking()
            .AnyAsync(x => x.Name == libraryName);
    }

    public async Task<IEnumerable<LibraryDto>> GetLibrariesForUserAsync(AppUser user)
    {
        return await _context.Library
            .Where(library => library.AppUsers.Contains(user))
            .Include(l => l.Folders)
            .AsNoTracking()
            .ProjectTo<LibraryDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }


}
