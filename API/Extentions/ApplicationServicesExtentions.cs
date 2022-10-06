using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Data;
using API.Interfaces;
using API.Services;
using Microsoft.EntityFrameworkCore;

namespace API.Extentions
{
    public static class ApplicationServicesExtentions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services,IConfiguration config)
        {
            services.AddScoped<ITokenService, TokenServices>();
            services.AddDbContext<DataContext>(options =>
            {

               options.UseSqlite(config.GetConnectionString("DefaultConnection"));
            });
            return services;
        }
            
    }
}