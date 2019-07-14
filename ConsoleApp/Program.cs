using Dapper;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace ConsoleApp
{
    class ProgramRunner
    {
        static void Main(string[] args)
        {
            using (var db = new SqlConnection("Data Source=.;Initial Catalog=ConsoleDb;Integrated Security=true;"))
            {
                var userDictionary = new Dictionary<string, User>();

                var sql = "SELECT * FROM Users u JOIN Posts p ON u.UserId = p.AuthorUserId";

                var list = db.Query<User, Post, User>(
                    sql,
                    (user, post) =>
                    {
                        if (!userDictionary.TryGetValue(user.UserId, out User userEntry))
                        {
                            userEntry = user;
                            userEntry.Posts = new List<Post>();
                            userDictionary.Add(user.UserId, userEntry);
                        }

                        userEntry.Posts.Add(post);

                        return userEntry;
                    },
                    splitOn: "PostId")
                .Distinct()
                .ToList();
            }


            Console.WriteLine("Hello World!");
        }
    }


    public class User
    {
        public string UserId { get; set; }

        public string FirstName { get; set; }

        public List<Post> Posts { get; set; }
    }

    public class Post
    {
        public int PostId { get; set; }

        public string Title { get; set; }
    }
}
