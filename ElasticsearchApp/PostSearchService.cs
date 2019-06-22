using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Nest;

namespace ElasticsearchApp
{
    public class PostSearchService
    {
        private readonly IElasticClient _client;
        private readonly ILogger<PostSearchService> _logger;
        private const string IndexName = "stackoverflow";

        public PostSearchService(IElasticClient client, ILogger<PostSearchService> logger)
        {
            _client = client;
            _logger = logger;
        }

        public async Task<ResponseBase> CreateIndex()
        {
            var createIndexDescriptor = new CreateIndexDescriptor(IndexName)
                .Settings(settings => settings
                    .NumberOfReplicas(1)
                    .NumberOfShards(5)
                    .Analysis(analysis => analysis
                        .Analyzers(analyzers => analyzers
                            .Custom("rebuilt_persian", c => c
                                .CharFilters("zero_width_spaces")
                                .Tokenizer("standard")
                                .Filters("lowercase", "decimal_digit", "arabic_normalization", "persian_normalization", "persian_stop")
                            ))
                        .TokenFilters(filter => filter.Stop("persian_stop", s => s.StopWords("_persian_")))
                        //.TokenFilters(filter => filter.Stop("persian_stop", s => s.StopWords("در","است", "از")))
                        .CharFilters(filter => filter.Mapping("zero_width_spaces", opt => opt.Mappings("\\u200C=>\\u0020"))))
                )
                .Map<Post>(m => m
                    .AutoMap()
                    .Properties(p => p
                        .Keyword(s => s.Name(n => n.Id))
                        .Date(s => s.Name(n => n.CreatedOn))
                        .Number(s => s.Name(n => n.Score).Type(NumberType.Integer))
                        .Number(s => s.Name(n => n.AnswerCount).Type(NumberType.Integer))
                        .Keyword(s => s.Name(n => n.Title))
                        .Text(s => s.Name(n => n.Body).Analyzer("rebuilt_persian").SearchAnalyzer("rebuilt_persian"))
                        .Keyword(s => s.Name(n => n.Tags))
                        .Keyword(s => s.Name(n => n.Suggests))
                    ));
 

            var response = await _client.Indices.CreateAsync(createIndexDescriptor);

            return response;
        }

        public async Task<ResponseBase> Insert(IEnumerable<Post> posts)
        {
            var response = await _client.IndexManyAsync(posts);
            
            return response;
        }
        
        public IEnumerable<Post> Search(string query, int page, int pageSize)
        {
            var result = _client.Search<Post>(x => x.Query(q => q
                    .MultiMatch(mp => mp
                        .Query(query)
                        .Fields(f => f
                            .Fields(f1 => f1.Title, f2 => f2.Body, f3 => f3.Tags))))
                .From(page - 1)
                .Size(pageSize));

            return result.Documents;
        }
    }
}