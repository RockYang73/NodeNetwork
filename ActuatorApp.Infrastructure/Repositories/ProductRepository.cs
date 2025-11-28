using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using ActuatorApp.Core.Entities;
using ActuatorApp.Core.Interfaces;
using Dapper;
using Microsoft.Data.Sqlite;

namespace ActuatorApp.Infrastructure.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly string _connectionString;

        public ProductRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        private IDbConnection CreateConnection()
        {
            return new SqliteConnection(_connectionString);
        }

        public async Task<IEnumerable<Tcsync>> GetTcsyncsAsync()
        {
            using (var connection = CreateConnection())
            {
                return await connection.QueryAsync<Tcsync>("SELECT * FROM Tcsync");
            }
        }

        public async Task<IEnumerable<Touch>> GetTouchesAsync()
        {
            using (var connection = CreateConnection())
            {
                return await connection.QueryAsync<Touch>("SELECT * FROM Touch");
            }
        }

        public async Task<IEnumerable<TCS>> GetTCSsAsync()
        {
            using (var connection = CreateConnection())
            {
                return await connection.QueryAsync<TCS>("SELECT * FROM TCS");
            }
        }

        public async Task<IEnumerable<Controlbox>> GetControlBoxesAsync()
        {
            using (var connection = CreateConnection())
            {
                return await connection.QueryAsync<Controlbox>("SELECT * FROM Controlbox");
            }
        }

        public async Task<IEnumerable<Control>> GetControlsAsync()
        {
            using (var connection = CreateConnection())
            {
                return await connection.QueryAsync<Control>("SELECT * FROM Control");
            }
        }

        public async Task<IEnumerable<Actuator>> GetActuatorsAsync()
        {
            using (var connection = CreateConnection())
            {
                return await connection.QueryAsync<Actuator>("SELECT * FROM Actuator");
            }
        }

        public async Task<IEnumerable<Columns>> GetColumnsAsync()
        {
            using (var connection = CreateConnection())
            {
                return await connection.QueryAsync<Columns>("SELECT * FROM Columns");
            }
        }

        public async Task<IEnumerable<Actlevel>> GetActlevelsAsync()
        {
            using (var connection = CreateConnection())
            {
                return await connection.QueryAsync<Actlevel>("SELECT * FROM Actlevel");
            }
        }
    }
}
