using System;
using CoreMultiTenancy.Identity.Data.Configuration;
using CoreMultiTenancy.Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CoreMultiTenancy.Identity.Data
{
    public class ApplicationDbContext : IdentityDbContext<User, Role, Guid>
    {
        public DbSet<Organization> Organizations { get; set; }
        public DbSet<OrganizationAccessToken> OrganizationAccessTokens { get; set; }
        public DbSet<UserOrganization> UserOrganizations { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            // UserOrganizations join table
            builder.ApplyConfiguration(new UserOrganizationEntityTypeConfiguration());

            base.OnModelCreating(builder);
        }
    }
}