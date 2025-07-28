using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShortenURLService.Data;
using ShortenURLService.Models;
using ShortenURLService.Services;
namespace ShortenURLService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class URLsController : ControllerBase
    {
        private readonly IRedisCacheService _redis;
        private readonly GenerateShortenURL _shortenService;
        private readonly ShortenURLServiceContext _context;

        public URLsController(ShortenURLServiceContext context, IRedisCacheService redis, GenerateShortenURL shortenService)
        {
            _context = context;
            _redis = redis;
            _shortenService = shortenService;
        }

        // GET: api/URLs
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<URL>>> GetURL()
        {
            return await _context.URL.ToListAsync();
        }

        // POST: api/URLs
        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult<URL>> PostURL(URL url)
        {
            // Generate shortened URL if not provided
            if (string.IsNullOrEmpty(url.ShortenedUrl))
            {
                url.ShortenedUrl = await _shortenService.ShortenUrlAsync(url.OriginalUrl);
            }

            url.CreatedAt = DateTime.UtcNow;
            _context.URL.Add(url);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetURL", new { id = url.Id }, url);
        }

        // Redirection Endpoint
        [AllowAnonymous]
        [HttpGet("redirect/{shortCode}")]
        public async Task<IActionResult> RedirectToOriginalUrl(string shortCode)
        {
            var cachedUrl = await _redis.GetCachedUrlAsync(shortCode);
            if (cachedUrl != null)
            {
                return Redirect(cachedUrl);
            }

            // If not in cache, check the database 
            var url = await _context.URL.FirstOrDefaultAsync(u => u.ShortenedUrl == shortCode);
            if (url == null)
            {
                return NotFound("Short code not found.");
            }

            // Cache the URL for future requests
            await _redis.CacheUrlAsync(shortCode, url.OriginalUrl);

            return Redirect(url.OriginalUrl);
        }

        // Validation if a short code exists
        [AllowAnonymous]
        [HttpGet("validate/{shortCode}")]
        public async Task<IActionResult> ValidateShortCode(string shortCode)
        {
            var url = await _context.URL.FirstOrDefaultAsync(u => u.ShortenedUrl == shortCode);
            if (url == null)
            {
                return NotFound("Short code not found.");
            }
            return Ok(new { OriginalUrl = url.OriginalUrl });
        }

        // DELETE: api/URLs/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteURL(int id)
        {
            var url = await _context.URL.FindAsync(id);
            if (url == null)
            {
                return NotFound();
            }

            _context.URL.Remove(url);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}