using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nest;

namespace ElasticsearchApp
{
    public class ApplicationHostedService : IHostedService
    {
        private readonly IElasticClient _elasticClient;
        private readonly ILogger<ApplicationHostedService> _logger;
        private readonly PostSearchService _postSearchService;

        public ApplicationHostedService(IElasticClient elasticClient, ILogger<ApplicationHostedService> logger, PostSearchService postSearchService)
        {
            _elasticClient = elasticClient;
            _logger = logger;
            _postSearchService = postSearchService;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
//            _logger.LogInformation($"start service at {DateTime.Now}");
//            
//            var response1 = await _postSearchService.CreateIndex();
//            _logger.LogInformation("Index created!");
//
//            var response2 = await InsertPosts(cancellationToken);
//            _logger.LogInformation("Post inserted");

            Search("آمریکا");
            Search("امریکا");

            _logger.LogInformation("End Service");
            
            await Task.CompletedTask;
        }
        
        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private void Search(string query)
        {
            _logger.LogInformation($"search {query}");
            
            var posts = _postSearchService.Search(query, 1, 10).ToList();

            foreach (var post in posts)
            {
                _logger.LogInformation(post.Body);
            }
            
            _logger.LogInformation($"end search, posts count: {posts.Count}");
        } 

        private async Task<ResponseBase> InsertPosts(CancellationToken cancellationToken)
        {
            var posts = new List<Post>
            {
                new Post
                {
                    Id = "56689116",
                    Title = "واکنش فرمانده کل سپاه به انهدام پهپاد آمریکا",
                    CreatedOn = DateTime.Now.AddDays(-10).AddMinutes(80),
                    Score = 4,
                    Body =
                        "سردار سلامی در کنگره ملی ۵۴۰۰ شهید استان کردستان گفت: سحرگاه امروز پدافند هوافضای سپاه پاسداران، یک فروند هواپیمای جاسوسی آمریکا را که به مرزهای آبی ما تعرض کرده بود، شجاعانه سرنگون کرد.",
                    Tags = new List<string> {"پدافند", "پهپاد"},
                    AnswerCount = 3
                },
                new Post
                {
                    Id = "56685016",
                    Title = "دومین جلسه رسیدگی به پرونده کیمیاخودرو آغاز شد",
                    CreatedOn = DateTime.Now.AddDays(-5).AddMinutes(80),
                    Score = 4,
                    Body =
                        "ه گزارش خبرنگار حقوقی و قضایی خبرگزاری فارس، ۱۵ متهم این پرونده، به اتهام مشارکت یا معاونت در انجام عملیات واسپاری (لیزینگ) بدون اخذ مجوز از بانک مرکزی و مشارکت در کلاهبرداری شبکه‌ای محاکمه می‌شوند",
                    Tags = new List<string> {"کیمیاخودرو", "لیزینگ"},
                    AnswerCount = 3
                }
            };

            foreach (var post in posts)
            {
                post.Suggests = post.Tags;
            }

            var response = await _postSearchService.Insert(posts);
            
            return response;
        }
    }
}