using Dapper;
using Docker.DotNet;
using Docker.DotNet.Models;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace TxCommand.Example.IntegrationTests.Setup
{
    public class DatabaseSetup : IDisposable
    {
        private const string WindowsDockerPath = "npipe://./pipe/docker_engine";
        private const string UnixDockerPath = "unix:///var/run/docker.sock";
        public const string SaPassword = "MySuperSecur3Password!";

        private readonly IDockerClient _docker;
        private string _containerId;

        public readonly IDbConnection Connection;

        public DatabaseSetup()
        {
            var dockerSocketPath = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? WindowsDockerPath
                : UnixDockerPath;

            _docker = new DockerClientConfiguration(new Uri(dockerSocketPath)).CreateClient();

            StartContainer().Wait();
            SetupDatabase().Wait();

            Connection = new SqlConnection($"Server=localhost,12937;Database=Test;User Id=sa;Password={SaPassword};");
            Connection.Open();
        }

        private async Task SetupDatabase()
        {
            using (var connection = new SqlConnection($"Server=localhost,12937;Database=master;User Id=sa;Password={SaPassword};"))
            {
                await connection.OpenAsync();

                await connection.ExecuteAsync("CREATE DATABASE [Test];");
            }

            using (var connection = new SqlConnection($"Server=localhost,12937;Database=Test;User Id=sa;Password={SaPassword};"))
            {
                await connection.OpenAsync();

                await connection.ExecuteAsync(@"CREATE TABLE [dbo].[People] (
                        [Id] INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
                        [Name] VARCHAR(255) NOT NULL
                    )");

                await connection.ExecuteAsync(@"CREATE TABLE [dbo].[Pets] (
                        [Id] INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
                        [PersonId] INT NOT NULL,
                        [Name] VARCHAR(255) NOT NULL UNIQUE,
                        CONSTRAINT FK_People_Pets FOREIGN KEY ([PersonId]) REFERENCES [People] ([Id])
                    )");
            }
        }

        private async Task StartContainer()
        {
            var resp = await _docker.Containers.CreateContainerAsync(new CreateContainerParameters
            {
                Image = "mcr.microsoft.com/mssql/server:2017-CU17-ubuntu",
                Name = "tx-command-database-integration",
                Env = new List<string>
                {
                    $"SA_PASSWORD={SaPassword}",
                    "ACCEPT_EULA=y"
                },
                ExposedPorts = new Dictionary<string, EmptyStruct>
                {
                    {"1433", new EmptyStruct()}
                },
                HostConfig = new HostConfig
                {
                    AutoRemove = true,
                    PortBindings = new Dictionary<string, IList<PortBinding>>
                    {
                        {"1433", new List<PortBinding>{new PortBinding{HostPort = "12937"}}}
                    }
                }
            });

            _containerId = resp.ID;

            await _docker.Containers.StartContainerAsync(_containerId, new ContainerStartParameters());

            Action checkSqlServer = () =>
            {
                using var connection = new SqlConnection($"Server=localhost,12937;Database=master;User Id=sa;Password={SaPassword};");
                connection.Open();
            };

            checkSqlServer.Should().NotThrowAfter(TimeSpan.FromSeconds(60), TimeSpan.FromSeconds(1));
        }

        private async Task StopContainer()
        {
            await _docker.Containers.StopContainerAsync(_containerId, new ContainerStopParameters());
        }

        public void Dispose()
        {
            StopContainer().Wait();
        }
    }
}
