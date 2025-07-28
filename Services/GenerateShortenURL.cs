using Microsoft.EntityFrameworkCore;
using ShortenURLService.Data;
using ShortenURLService.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ShortenURLService.Services
{
    public class GenerateShortenURL
    {   //frontend looking for userUrl
        private readonly ShortenURLServiceContext _context;
        private const string Characters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        public GenerateShortenURL(ShortenURLServiceContext context)
        {
            _context = context;
        }

        private async Task<string> GenerateShortCodeAsync()
        {
            Random random = new Random(); // Fixed syntax error (was using - instead of =)
            string shortCode;
            do
            {
                shortCode = new string(Enumerable.Repeat(Characters, 6)
                    .Select(s => s[random.Next(s.Length)]).ToArray());

            } while (await _context.URL.AnyAsync(u => u.ShortenedUrl == shortCode));
            return shortCode;
        }

        public async Task<string> ShortenUrlAsync(string originalUrl)
        {
            // Check if this URL already exists in our database
            var existingUrl = await _context.URL.FirstOrDefaultAsync(u => u.OriginalUrl == originalUrl);
            if (existingUrl != null)
            {
                // Return existing short code if we already have this URL
                return existingUrl.ShortenedUrl;
            }

            // Generate a new short code
            string shortCode = await GenerateShortCodeAsync();

            // Create new URL entity
            var urlEntity = new URL
            {
                OriginalUrl = originalUrl,
                ShortenedUrl = shortCode,
                CreatedAt = DateTime.UtcNow
            };
            // Save to database
            _context.URL.Add(urlEntity);
            await _context.SaveChangesAsync();
            return shortCode;
        }
    }
}