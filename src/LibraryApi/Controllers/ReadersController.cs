using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using LibraryApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReadersController : ControllerBase
{
    private readonly LibraryDbContext _context;
    private readonly ICacheService _cache;
    private readonly ILogger<ReadersController> _logger;

    private const string AllReadersCacheKey = "readers:all";
    private const string ReaderCacheKeyPrefix = "readers:";

    public ReadersController(
        LibraryDbContext context,
        ICacheService cache,
        ILogger<ReadersController> logger)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Reader>>> GetAll()
    {
        var cached = await _cache.GetAsync<List<Reader>>(AllReadersCacheKey);
        if (cached != null)
        {
            _logger.LogInformation("Readers retrieved from CACHE");
            return Ok(cached);
        }

        var readers = await _context.Readers.AsNoTracking().ToListAsync();
        await _cache.SetAsync(AllReadersCacheKey, readers, TimeSpan.FromMinutes(5));
        _logger.LogInformation("Readers retrieved from DATABASE and cached");

        return Ok(readers);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Reader>> GetById(int id)
    {
        var cacheKey = $"{ReaderCacheKeyPrefix}{id}";

        var cached = await _cache.GetAsync<Reader>(cacheKey);
        if (cached != null)
            return Ok(cached);

        var reader = await _context.Readers.FindAsync(id);
        if (reader == null)
            return NotFound(new { message = $"Reader with id {id} not found" });

        await _cache.SetAsync(cacheKey, reader, TimeSpan.FromMinutes(5));
        return Ok(reader);
    }

    [HttpPost]
    public async Task<ActionResult<Reader>> Create([FromBody] CreateReaderDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var reader = new Reader
        {
            FullName = dto.FullName,
            Email = dto.Email,
            Phone = dto.Phone,
            RegistrationDate = DateTime.UtcNow
        };

        _context.Readers.Add(reader);
        await _context.SaveChangesAsync();
        await _cache.RemoveAsync(AllReadersCacheKey);

        return CreatedAtAction(nameof(GetById), new { id = reader.Id }, reader);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Reader>> Update(int id, [FromBody] UpdateReaderDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var reader = await _context.Readers.FindAsync(id);
        if (reader == null)
            return NotFound(new { message = $"Reader with id {id} not found" });

        reader.FullName = dto.FullName;
        reader.Email = dto.Email;
        reader.Phone = dto.Phone;

        await _context.SaveChangesAsync();

        await _cache.RemoveAsync(AllReadersCacheKey);
        await _cache.RemoveAsync($"{ReaderCacheKeyPrefix}{id}");

        return Ok(reader);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var reader = await _context.Readers.FindAsync(id);
        if (reader == null)
            return NotFound(new { message = $"Reader with id {id} not found" });

        _context.Readers.Remove(reader);
        await _context.SaveChangesAsync();

        await _cache.RemoveAsync(AllReadersCacheKey);
        await _cache.RemoveAsync($"{ReaderCacheKeyPrefix}{id}");

        return NoContent();
    }
}