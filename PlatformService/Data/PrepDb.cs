using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace PlatformService.Data
{
    public static class PrepDb
    {
        public static void PrepPopulation(IApplicationBuilder app, bool isProd)
        {
            using (var serviceScope = app.ApplicationServices.CreateScope())
            {
                SeedData(serviceScope.ServiceProvider.GetService<AppDbContext>(), isProd);
            }
        }

        private static void SeedData(AppDbContext context, bool isProd)
        {
            if (isProd)
            {
                Console.WriteLine("--> Attempting to apply Migrations...");
                try
                {
                    context.Database.Migrate();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"--> Could not run migrations: {ex.Message}");
                }
            }

            try
            {
                context.Database.EnsureCreated();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"--> Could not ensure database created: {ex.Message}");
            }

            if (!context.Platforms.Any())
            {
                Console.WriteLine("Seeding Data...");

                context.Platforms.AddRange(
                    new Models.Platform() { Name = "Dot Net", Publisher = "Microsoft", Cost = "Free" },
                    new Models.Platform() { Name = "SQL Server Express", Publisher = "Microsoft", Cost = "Free" },
                    new Models.Platform() { Name = "Kubernetes", Publisher = "Cloud Native Computing Foundation", Cost = "Free" },
                    new Models.Platform() { Name = "PostgreSQL", Publisher = "PostgreSQL Global Development Group", Cost = "Free" },
                    new Models.Platform() { Name = "Docker", Publisher = "Docker, Inc.", Cost = "Free" }
                );

                context.SaveChanges();
            }
            else
            {
                Console.WriteLine("We already have data");
            }
        }
    }
}