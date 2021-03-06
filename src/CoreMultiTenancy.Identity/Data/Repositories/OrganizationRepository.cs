using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreMultiTenancy.Identity.Entities;
using CoreMultiTenancy.Identity.Interfaces;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using Perustaja.Polyglot.Option;

namespace CoreMultiTenancy.Identity.Data.Repositories
{
    public class OrganizationRepository : IOrganizationRepository
    {
        private readonly string _connectionString;
        private readonly ILogger<OrganizationRepository> _logger;
        private readonly ApplicationDbContext _applicationContext;
        public IUnitOfWork UnitOfWork { get => _applicationContext; }

        public OrganizationRepository(IConfiguration config, ILogger<OrganizationRepository> logger,
            ApplicationDbContext applicationContext)
        {
            _connectionString = config.GetConnectionString("IdentityDb");
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _applicationContext = applicationContext ?? throw new ArgumentNullException(nameof(applicationContext));
        }

        public async Task<List<Organization>> GetUsersOrgsById(Guid userId)
        {
            return await _applicationContext.Set<UserOrganization>()
            .Where(uo => uo.UserId == userId)
            .Include(uo => uo.Organization)
            .Select(uo => uo.Organization)
            .ToListAsync();
        }

        public async Task<bool> ExistsByIdAsync(Guid id)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                var res = await conn.QuerySingleAsync<int>(
                    @"SELECT COUNT(*) FROM Organizations 
                    WHERE Id = @Id AND SuccessfullyCreated = true",
                    new { Id = id }
                );
                return res > 0;
            }
        }

        public async Task<Option<Organization>> GetByIdAsync(Guid id)
        {
            var res = await _applicationContext.Set<Organization>()
            .FirstOrDefaultAsync(o => o.Id == id);
            return res != null
                ? Option<Organization>.Some(res)
                : Option<Organization>.None;
        }

        public Task<List<Organization>> GetUnsuccessfullyCreatedAsync()
            => _applicationContext.Set<Organization>()
                .Where(o => !o.SuccessfullyCreated && (DateTime.UtcNow - o.CreationDate).Hours > 24)
                .ToListAsync();

        public Organization Add(Organization o)
            => _applicationContext.Set<Organization>().Add(o).Entity;

        public Organization Update(Organization o)
            => _applicationContext.Set<Organization>().Update(o).Entity;

        public void Delete(Organization o)
            => _applicationContext.Set<Organization>().Remove(o);


    }
}