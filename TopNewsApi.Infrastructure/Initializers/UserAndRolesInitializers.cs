﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TopNewsApi.Core.Entities.User;
using TopNewsApi.Infrastructure.Context;

namespace TopNewsApi.Infrastructure.Initializers
{
    public class UserAndRolesInitializers
    {
        public static async Task SeedUserAndRole(IApplicationBuilder applicationBuilder)
        {
            using (var serviceScope = applicationBuilder.ApplicationServices.CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<AppDbContext>();
                UserManager<AppUser> userManager = serviceScope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
                if (userManager.FindByEmailAsync("admin@email.com").Result == null)
                {
                    AppUser admin = new AppUser()
                    {
                        FirstName = "Bill",
                        LastName = "Gates",
                        UserName = "admin@email.com",
                        Email = "admin@email.com",
                        EmailConfirmed = true,
                        PhoneNumber = "+380981786512",
                        PhoneNumberConfirmed = true,
                    };

                    context.Roles.AddRangeAsync(
                        new IdentityRole()
                        {
                            Name = "Administrator",
                            NormalizedName = "ADMINISTRATOR"
                        },
                        new IdentityRole()
                        {
                            Name = "User",
                            NormalizedName = "USER"
                        }
                   );
                    await context.SaveChangesAsync();

                    IdentityResult adminResult = userManager.CreateAsync(admin, "Qwerty-1").Result;
                    if (adminResult.Succeeded)
                    {
                        userManager.AddToRoleAsync(admin, "Administrator").Wait();
                    }
                }
            }
        }
    }
}
