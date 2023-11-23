using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Hello World!");

        var loggerFactory = LoggerFactory.Create(builder => { builder.AddConsole(); });

        var context = new MyDbContext(loggerFactory);
        context.Database.EnsureCreated();
        InitializeData(context);

        Console.WriteLine("All posts:");
        var data = context.BlogPosts.Select(x => x.Title).ToList();
        Console.WriteLine(JsonSerializer.Serialize(data));


        Console.WriteLine("How many comments each user left:");
        //ToDo: write a query and dump the data to console
        // Expected result (format could be different, e.g. object serialized to JSON is ok):
        // Ivan: 4
        // Petr: 2
        // Elena: 3

        var NumberOfCommentsPerUser = context.BlogComments
            .GroupBy(bc => bc.UserName)
            .Select(x => new
            {
                UserName = x.Key,
                Count = x.Count()
            })
            .ToList();
        NumberOfCommentsPerUser.ForEach(x => Console.WriteLine($"{x.UserName}: {x.Count}"));

        Console.WriteLine("Posts ordered by date of last comment. Result should include text of last comment:");
        //ToDo: write a query and dump the data to console
        // Expected result (format could be different, e.g. object serialized to JSON is ok):
        // Post2: '2020-03-06', '4'
        // Post1: '2020-03-05', '8'
        // Post3: '2020-02-14', '9'

        var PostsOrderedByLastCommentDate = context.BlogPosts
            .Select(bp => new
            {
                bp.Title,
                LastComment = bp.Comments.OrderBy(c => c.CreatedDate)
                    .Select(c => new
                    {
                        c.CreatedDate,
                        c.Text
                    })
                    .LastOrDefault()
            })
            .OrderByDescending(pObject => pObject.LastComment.CreatedDate)
            .ToList();
        PostsOrderedByLastCommentDate.ForEach(x => Console.WriteLine($"{x.Title}: {x.LastComment.CreatedDate.ToShortDateString()}, {x.LastComment.Text}"));


        Console.WriteLine("How many last comments each user left:");
        // 'last comment' is the latest Comment in each Post
        //ToDo: write a query and dump the data to console
        // Expected result (format could be different, e.g. object serialized to JSON is ok):
        // Ivan: 2
        // Petr: 1

        var NumberOfLastCommentsLeftByUser = context.BlogPosts
            .Select(bp => bp.Comments.OrderBy(c => c.CreatedDate)
                .LastOrDefault().UserName)
            .GroupBy(user => user)
            .Select(user => new
            {
                UserName = user.Key,
                Count = user.Count()
            })
            .ToList();
        NumberOfLastCommentsLeftByUser.ForEach(x => Console.WriteLine($"{x.UserName}: {x.Count}"));

        // Console.WriteLine(
        //     JsonSerializer.Serialize(BlogService.NumberOfCommentsPerUser(context)));
        // Console.WriteLine(
        //     JsonSerializer.Serialize(BlogService.PostsOrderedByLastCommentDate(context)));
        // Console.WriteLine(
        //     JsonSerializer.Serialize(BlogService.NumberOfLastCommentsLeftByUser(context)));

    }

    private static void InitializeData(MyDbContext context)
    {
        context.BlogPosts.Add(new BlogPost("Post1")
        {
            Comments = new List<BlogComment>()
            {
                new BlogComment("1", new DateTime(2020, 3, 2), "Petr"),
                new BlogComment("2", new DateTime(2020, 3, 4), "Elena"),
                new BlogComment("8", new DateTime(2020, 3, 5), "Ivan"),
            }
        });
        context.BlogPosts.Add(new BlogPost("Post2")
        {
            Comments = new List<BlogComment>()
            {
                new BlogComment("3", new DateTime(2020, 3, 5), "Elena"),
                new BlogComment("4", new DateTime(2020, 3, 6), "Ivan"),
            }
        });
        context.BlogPosts.Add(new BlogPost("Post3")
        {
            Comments = new List<BlogComment>()
            {
                new BlogComment("5", new DateTime(2020, 2, 7), "Ivan"),
                new BlogComment("6", new DateTime(2020, 2, 9), "Elena"),
                new BlogComment("7", new DateTime(2020, 2, 10), "Ivan"),
                new BlogComment("9", new DateTime(2020, 2, 14), "Petr"),
            }
        });
        context.SaveChanges();
    }
}