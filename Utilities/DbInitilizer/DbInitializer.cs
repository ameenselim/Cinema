using Cinema_Project.Models;
using CinemaProject.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Threading.Tasks;

namespace Cinema_Project.Utilities.DbInitilizer
{
    public class DbInitializer : IDbInitializer
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public DbInitializer(ApplicationDbContext context ,IConfiguration configuration ,RoleManager<IdentityRole> roleManager ,UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _configuration = configuration;
            _roleManager = roleManager;
            _userManager = userManager;
        }
        public async Task Initialize()
        {
            try
            {


                // Apply any pending migrations
                if (_context.Database.GetPendingMigrations().Any())
                {
                    _context.Database.Migrate();
                }
                // Seed roles if they don't exist
                if (_roleManager.Roles.IsNullOrEmpty())
                {
                    await _roleManager.CreateAsync(new(RolesName.SUPER_ADMIN));
                    await _roleManager.CreateAsync(new(RolesName.ADMIN));
                    await _roleManager.CreateAsync(new(RolesName.EMPLOYEE));
                    await _roleManager.CreateAsync(new(RolesName.CUSTOMER));
                }
                if (await _userManager.FindByEmailAsync(_configuration["SuperAdminAccount:Email"]!) is null)
                {
                    ApplicationUser user = new()
                    {
                        UserName = _configuration["SuperAdminAccount:UserName"],
                        Email = _configuration["SuperAdminAccount:Email"],
                        FirstName = "Super",
                        LastName = "Admin",
                        EmailConfirmed = true
                    };
                    await _userManager.CreateAsync(user, _configuration["SuperAdminAccount:Password"]!);
                    await _userManager.AddToRoleAsync(user, RolesName.SUPER_ADMIN);
                }
                var cinemas = _context.Cinemas.ToList();

                foreach (var cinema in cinemas)
                {
                    if (!_context.Seats.Any(s => s.CinemaId == cinema.Id))
                    {
                        for (int row = 0; row < cinema.RowsCount; row++)
                        {
                            char rowLetter = (char)('A' + row);

                            for (int n = 1; n <= cinema.SeatsPerRow; n++)
                            {
                                _context.Seats.Add(new Seat
                                {
                                    CinemaId = cinema.Id,
                                    SeatNumber = $"{rowLetter}{n}"
                                });
                            }
                        }
                    }
                }

                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                // Log the exception (you can use a logging framework like Serilog, NLog, etc.)
                Console.WriteLine($"An error occurred during database initialization: {ex.Message}");
            }
        }
    }
}
