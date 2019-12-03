using Dapper;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConsoleApp
{
    class ProgramRunner
    {
        static void Main(string[] args)
        {
            //using (var db = new ApplicationDbContext())
            //{
            //    db.Users.Add(new User()
            //    {
            //        UserId = Guid.NewGuid().ToString(),
            //        FirstName = "M"
            //    }); 

            //    db.SaveChanges();
            //}


            //using (var db = new ApplicationDbContext())
            //{
            //    var list = db.Users.ToList();
            //}





            //using (var db = new SqlConnection("Data Source=.;Initial Catalog=ConsoleDb;Integrated Security=true;"))
            //{
            //    var userDictionary = new Dictionary<string, User>();

            //    var sql = "SELECT * FROM Users u JOIN Posts p ON u.UserId = p.AuthorUserId";

            //    var list = db.Query<User, Post, User>(
            //        sql,
            //        (user, post) =>
            //        {
            //            if (!userDictionary.TryGetValue(user.UserId, out User userEntry))
            //            {
            //                userEntry = user;
            //                userEntry.Posts = new List<Post>();
            //                userDictionary.Add(user.UserId, userEntry);
            //            }

            //            userEntry.Posts.Add(post);

            //            return userEntry;
            //        },
            //        splitOn: "PostId")
            //    .Distinct()
            //    .ToList();
            //}


            Console.WriteLine("Hello World!");
        }
    }


    public class ApplicationDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Sheep> Sheeps { get; set; }
        public DbSet<Rabbit> Rabbits { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Data Source=.;Initial Catalog=App;Integrated Security=true;");
            }
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfiguration(new UserConfiguration());
            builder.ApplyConfiguration(new SheepConfiguration());

            base.OnModelCreating(builder);
        }
    }

    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasDiscriminator<string>("Discriminator")
                .HasValue<User>(nameof(User))
                .HasValue<Publisher>(nameof(Publisher))
                .HasValue<Manager>(nameof(Manager));
        }
    }

    public class SheepConfiguration : IEntityTypeConfiguration<Rabbit>
    {
        public void Configure(EntityTypeBuilder<Rabbit> builder)
        {
            builder.HasMany(e => e.Sheeps1).WithOne(e => e.Rabbit1).OnDelete(DeleteBehavior.Restrict);
            builder.HasMany(e => e.Sheeps2).WithOne(e => e.Rabbit2).OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class Sheep
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public Rabbit Rabbit1 { get; set; }
        public int Rabbit1Id { get; set; }

        public Rabbit Rabbit2 { get; set; }
        public int Rabbit2Id { get; set; }
    }

    public class Rabbit
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public ICollection<Sheep> Sheeps1 { get; set; }

        public ICollection<Sheep> Sheeps2 { get; set; }
    }


    public class User
    {
        public string UserId { get; set; }

        public string FirstName { get; set; }

        public List<Post> Posts { get; set; }
    }

    public class Manager : User
    {
        public string LastName { get; set; }
    }

    public class Publisher : User
    {
        public int Credit { get; set; }
    }

    public class Post
    {
        public int PostId { get; set; }

        public string Title { get; set; }
    }



}
