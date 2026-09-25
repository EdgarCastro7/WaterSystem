using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using System;
using WaterSystem.Data.Entities;
using WaterSystem.Helpers;

namespace WaterSystem.Data
{
    public class SeedDb
    {
        private readonly DataContext _context;
        private readonly IUserHelper _userHelper;

        public SeedDb(DataContext context, IUserHelper userHelper)
        {
            _context = context;
            _userHelper = userHelper;
        }

        public async Task SeedAsync()
        {
            await _context.Database.MigrateAsync();

            await _userHelper.CheckRoleAsync("Admin");

            await _userHelper.CheckRoleAsync("Employee");

            await _userHelper.CheckRoleAsync("Client");

            var user = await _userHelper.GetUserByEmailAsync("edgarcastro@gmail.com");
            if (user == null)
            {
                user = new User
                {
                    FirstName = "Edgar",
                    LastName = "Castro",
                    Email = "edgarcastro@gmail.com",
                    UserName = "edgarcastro@gmail.com",
                    PhoneNumber = "123123123",
                    Address = "Rua da Serra 30",
                };

                var result = await _userHelper.AddUserAsync(user, "Admin123!");

                if (result != IdentityResult.Success)
                {
                    throw new InvalidOperationException("Could not create the user in seeder");
                }

                await _userHelper.AddUserToRoleAsync(user, "Admin");
            }

            var isInRole = await _userHelper.IsUserInRoleAsync(user, "Admin");
            if(!isInRole)
            {
                await _userHelper.AddUserToRoleAsync(user, "Admin");
            }
        }
    }
}
