using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Entities.Models;
using Microsoft.EntityFrameworkCore;


namespace Backend.Entities
{
    public class BackendContext : DbContext
    {
        public BackendContext(DbContextOptions options) : base(options)
        {

        }

        public DbSet<Branch> Branches { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<Make> Makes { get; set; }
        public DbSet<Model> Models { get; set; }
        public DbSet<ProfilePicture> ProfilePictures { get; set; }
        public DbSet<Sale> Sales { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<VehicleImage> VehicleImages { get; set; }

        public DbSet<VehicleType> VehicleType { get; set; }

        public DbSet<VehicleColor> VehicleColor { get; set; }

        public DbSet<FuelType> FuelType { get; set; }

        public DbSet<Favorites> Favorites { get; set; }

        public DbSet<Promoted> Promoted { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Firstname = "Andrei",
                    Lastname = "Lupu",
                    Phonenumber = "+40745575094",
                    Email = "andrei_2008118@yahoo.com",
                    isConfirmed = true,
                    Role = "Admin",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("123123"),
                }
                );
            modelBuilder.Entity<Country>().HasData(
                new Country
                {
                    countryID = 1,
                    name = "England"
                },
                new Country
                {
                    countryID = 2,
                    name = "Austria"
                },
                new Country
                {
                    countryID = 3,
                    name = "Belgium"
                },
                new Country
                {
                    countryID = 4,
                    name = "Bulgaria"
                },
                new Country
                {
                    countryID = 5,
                    name = "Canada"
                },
                new Country
                {
                    countryID = 6,
                    name = "Czech Republic"
                },
                new Country
                {
                    countryID = 7,
                    name = "Croatia"
                },
                new Country
                {
                    countryID = 8,
                    name = "Denmark"
                },
                new Country
                {
                    countryID = 9,
                    name = "Estonia"
                },

                new Country
                {
                    countryID = 10,
                    name = "Finland"
                },
                new Country
                {
                    countryID = 11,
                    name = "France"
                },
                new Country
                {
                    countryID = 12,
                    name = "Germany"
                },
                new Country
                {
                    countryID = 13,
                    name = "Greece"
                },
                new Country
                {
                    countryID = 14,
                    name = "Ireland"
                },
                new Country
                {
                    countryID = 15,
                    name = "Italy"
                },
                new Country
                {
                    countryID = 16,
                    name = "Japan"
                },
                new Country
                {
                    countryID = 17,
                    name = "Luxembourg"
                },

                new Country
                {
                    countryID = 18,
                    name = "Norvegia"
                },
                new Country
                {
                    countryID = 19,
                    name = "Holland"
                },
                new Country
                {
                    countryID = 20,
                    name = "Poland"
                },
                new Country
                {
                    countryID = 21,
                    name = "Romania"
                },
                new Country
                {
                    countryID = 22,
                    name = "Russia"
                },
                new Country
                {
                    countryID = 23,
                    name = "Slovacia"
                },
                new Country
                {
                    countryID = 24,
                    name = "Slovenia"
                },
                new Country
                {
                    countryID = 25,
                    name = "Spain"
                },

                new Country
                {
                    countryID = 26,
                    name = "USA"
                },
                new Country
                {
                    countryID = 27,
                    name = "Sweden"
                },
                new Country
                {
                    countryID = 28,
                    name = "Turkey"
                },
                new Country
                {
                    countryID = 29,
                    name = "Ukraine"
                },
                new Country
                {
                    countryID = 30,
                    name = "Hungary"
                },

                new Country
                {
                    countryID = 31,
                    name = "Portugal"
                },

                new Country
                {
                    countryID = 32,
                    name = "China"
                },
                new Country
                {
                    countryID = 33,
                    name = "South Koreea"
                },

                new Country
                {
                    countryID = 34,
                    name = "India"
                },

                new Country
                {
                    countryID = 35,
                    name = "Australia"
                },

                new Country
                {
                    countryID = 36,
                    name = "Mexico"
                },

                new Country
                {
                    countryID = 37,
                    name = "Brasil"
                },
                new Country
                {
                    countryID = 38,
                    name = "Argentina"
                },
                new Country
                {
                    countryID = 39,
                    name = "Iceland"
                },
                new Country
                {
                    countryID = 40,
                    name = "Lithuania"
                },
                new Country
                {
                    countryID = 41,
                    name = "Lativa"
                },
                new Country
                {
                    countryID = 42,
                    name = "Other"
                });

            modelBuilder.Entity<VehicleType>().HasData(
                new VehicleType
                {
                    Id = 1,
                    type = "Saloon"
                },
                new VehicleType
                {
                    Id = 2,
                    type = "Estate"
                },
                new VehicleType
                {
                    Id = 3,
                    type = "Hatchback"
                },
                new VehicleType
                {
                    Id = 4,
                    type = "SUV"
                },
                new VehicleType
                {
                    Id = 5,
                    type = "Van"
                },
                new VehicleType
                {
                    Id = 6,
                    type = "Pick-up Truck"
                },
                new VehicleType
                {
                    Id = 7,
                    type = "Cabrio"
                });

            modelBuilder.Entity<VehicleColor>().HasData(
                new VehicleColor
                {
                    Id = 1,
                    color = "Black"
                },
                new VehicleColor
                {
                    Id = 2,
                    color = "White"
                },
                new VehicleColor
                {
                    Id = 3,
                    color = "Blue"
                },
                new VehicleColor
                {
                    Id = 4,
                    color = "Green"
                },
                new VehicleColor
                {
                    Id = 5,
                    color = "Grey"
                },
                new VehicleColor
                {
                    Id = 6,
                    color = "Red"
                },
                new VehicleColor
                {
                    Id = 7,
                    color = "Brown"
                },
                new VehicleColor
                {
                    Id = 8,
                    color = "Silver"
                },
                new VehicleColor
                {
                    Id = 9,
                    color = "Orange"
                },
                new VehicleColor
                {
                    Id = 10,
                    color = "Yellow"
                },
                new VehicleColor
                {
                    Id = 11,
                    color = "Burgundy"
                },
                new VehicleColor
                {
                    Id = 12,
                    color = "Other"
                });

            modelBuilder.Entity<FuelType>().HasData(
                new FuelType
                {
                    Id = 1,
                    fuel = "Diesel"
                },
                new FuelType
                {
                    Id = 2,
                    fuel = "Petrol"
                },
                new FuelType
                {
                    Id = 3,
                    fuel = "Petrol + LPG"
                },
                new FuelType
                {
                    Id = 4,
                    fuel = "Hybrid"
                },
                new FuelType
                {
                    Id = 5,
                    fuel = "Electric"
                }, 
                new FuelType
                {
                    Id = 6,
                    fuel = "Hydrogen"
                });
        }
    }
}
