using CareNote.Models;


namespace CareNote.Services
{
    public class KnowledgeService : IKnowledgeService
    {
        private readonly List<KnowledgeFile> _userFiles = new();
        private readonly Dictionary<string, List<string>> _fileContentIndex = new();
        private readonly ILogger<KnowledgeService> _logger;
        private readonly HttpClient _httpClient;
        private int _nextId = 1;

        public KnowledgeService(HttpClient httpClient, ILogger<KnowledgeService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            
            
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }


        public Task<List<KnowledgeFile>> GetUserFilesAsync(string userId) 

        {
            var userFiles = _userFiles.Where(f => f.UserId == userId).ToList();
            return Task.FromResult(userFiles);
        }

        public async Task ProcessFileAsync(Stream fileStream, string fileName, string userId)
        {
            using var reader = new StreamReader(fileStream);
            var content = await reader.ReadToEndAsync();

            var file = new KnowledgeFile
            {
                Id = _nextId++,
                UserId = userId,
                FileName = fileName,
                FileType = Path.GetExtension(fileName),
                FileSize = content.Length,
                UploadedAt = DateTime.Now,
                IsProcessed = true
            };

            _userFiles.Add(file);

            // Simple text segmentation
            var sentences = content.Split('.', '!', '?')
                                   .Where(s => !string.IsNullOrWhiteSpace(s))
                                   .Select(s => s.Trim())
                                   .ToList();

            _fileContentIndex[fileName] = sentences;

            _logger.LogInformation("File {FileName} uploaded and indexed for {User}", fileName, userId);
        }

        public Task<List<SearchResult>> SearchAsync(string query, string userId)
        {
            var userFiles = _userFiles.Where(f => f.UserId == userId).ToList();
            var results = new List<SearchResult>();

            foreach (var file in userFiles)
            {
                if (_fileContentIndex.TryGetValue(file.FileName, out var sentences))
                {
                    var relevant = sentences
                        .Where(s => s.Contains(query, StringComparison.OrdinalIgnoreCase))
                        .Select(s => new SearchResult
                        {
                            FileName = file.FileName,
                            Content = s.Length > 200 ? s.Substring(0, 200) + "..." : s,
                            Relevance = CalculateRelevance(s, query)
                        })
                        .Take(3)
                        .ToList();

                    results.AddRange(relevant);
                }
            }

            var ordered = results.OrderByDescending(r => r.Relevance).Take(10).ToList();
            _logger.LogInformation("Local search '{Query}' returned {Count} results for {User}", query, ordered.Count, userId);
            return Task.FromResult(ordered);
        }

        public Task DeleteFileAsync(int fileId, string userId)
        {
            var file = _userFiles.FirstOrDefault(f => f.Id == fileId && f.UserId == userId);
            if (file != null)
            {
                _userFiles.Remove(file);
                _fileContentIndex.Remove(file.FileName);
                _logger.LogInformation("Deleted file {FileId} for user {User}", fileId, userId);
            }
            return Task.CompletedTask;
        }


        public async Task<List<ExternalResource>> SearchExternalSourcesAsync(string query) // Söker även externa källor (Svenska + internationella medicinska resurser)
        {
            var results = new List<ExternalResource>();
            var tasks = new List<Task>();

            try
            {
                tasks.Add(Task.Run(async () => 
                {
                    var swedishResults = await SearchSwedishSources(query);
                    lock (results) results.AddRange(swedishResults);
                }));

               
                tasks.Add(Task.Run(async () => 
                {
                    var medicalResults = await SearchMedicalSources(query);
                    lock (results) results.AddRange(medicalResults);
                }));

               
                tasks.Add(Task.Run(async () => 
                {
                    var guidelineResults = await SearchGuidelineSources(query);
                    lock (results) results.AddRange(guidelineResults);
                }));

              
                await Task.WhenAll(tasks).ContinueWith(t => 
                {
                    if (t.IsFaulted)
                    {
                        _logger.LogWarning("Some external searches failed: {Error}", t.Exception?.Message);
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in external search for query '{Query}'", query);
            }

            if (!results.Any())
            {
                results.AddRange(GetStaticResources());
            }

            
            var finalResults = results
                .GroupBy(r => r.Url)
                .Select(g => g.First())
                .Take(15)
                .ToList();

            _logger.LogInformation("External search '{Query}' returned {Count} results", query, finalResults.Count);
            return finalResults;
        }

        private async Task<List<ExternalResource>> SearchSwedishSources(string query)
        {
            var results = new List<ExternalResource>();
            
            try
            {
                
                results.Add(new ExternalResource
                {
                    Title = $"Vårdhandboken - '{query}'",
                    Description = "Sök i Socialstyrelsens nationella riktlinjer och kunskapsstöd",
                    Url = $"https://vardhandboken.se/sok/?q={Uri.EscapeDataString(query)}",
                    Category = "Riktlinjer",
                    Source = "Socialstyrelsen",
                    RelevanceScore = 95
                });

                
                results.Add(new ExternalResource
                {
                    Title = $"1177 - '{query}'",
                    Description = "Information och råd om hälsa och sjukdomar",
                    Url = $"https://www.1177.se/sok/?q={Uri.EscapeDataString(query)}",
                    Category = "Patientinformation",
                    Source = "1177",
                    RelevanceScore = 90
                });

                
                results.Add(new ExternalResource
                {
                    Title = $"SBU - '{query}'",
                    Description = "Evidensbaserade utvärderingar av metoder i vården",
                    Url = $"https://www.sbu.se/sv/publikationer/sok/?q={Uri.EscapeDataString(query)}",
                    Category = "Forskning",
                    Source = "SBU",
                    RelevanceScore = 85
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Swedish sources search failed for query '{Query}'", query);
            }

            return results;
        }

        private async Task<List<ExternalResource>> SearchMedicalSources(string query)
        {
            var results = new List<ExternalResource>();
            
            try
            {
                
                var pmcUrl = $"https://www.ncbi.nlm.nih.gov/pmc/?term={Uri.EscapeDataString(query)}";
                results.Add(new ExternalResource
                {
                    Title = $"PubMed Central - '{query}'",
                    Description = "Vetenskapliga artiklar med fri tillgång",
                    Url = pmcUrl,
                    Category = "Forskning",
                    Source = "PubMed Central",
                    RelevanceScore = 80
                });

                
                var ctUrl = $"https://clinicaltrials.gov/ct2/results?cond={Uri.EscapeDataString(query)}";
                results.Add(new ExternalResource
                {
                    Title = $"ClinicalTrials.gov - '{query}'",
                    Description = "Pågående och avslutade kliniska studier",
                    Url = ctUrl,
                    Category = "Kliniska studier",
                    Source = "NIH",
                    RelevanceScore = 75
                });

               
                var whoUrl = $"https://www.who.int/search?indexCatalogue=genericsearchindex1&query={Uri.EscapeDataString(query)}";
                results.Add(new ExternalResource
                {
                    Title = $"WHO - '{query}'",
                    Description = "Globala riktlinjer och rapporter",
                    Url = whoUrl,
                    Category = "Riktlinjer",
                    Source = "WHO",
                    RelevanceScore = 85
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Medical sources search failed for query '{Query}'", query);
            }

            return results;
        }

        private async Task<List<ExternalResource>> SearchGuidelineSources(string query)
        {
            var results = new List<ExternalResource>();
            
            try
            {
                // 1. NICE (UK National Institute for Health and Care Excellence)
                results.Add(new ExternalResource
                {
                    Title = $"NICE Guidelines - '{query}'",
                    Description = "Brittiska kliniska riktlinjer",
                    Url = $"https://www.nice.org.uk/search?q={Uri.EscapeDataString(query)}",
                    Category = "Riktlinjer",
                    Source = "NICE",
                    RelevanceScore = 80
                });

                // 2. Cochrane Library
                results.Add(new ExternalResource
                {
                    Title = $"Cochrane - '{query}'",
                    Description = "Systematiska översikter av medicinsk forskning",
                    Url = $"https://www.cochranelibrary.com/search?searchText={Uri.EscapeDataString(query)}",
                    Category = "Forskning",
                    Source = "Cochrane",
                    RelevanceScore = 85
                });

                // 3. ECDC (European Centre for Disease Prevention and Control)
                results.Add(new ExternalResource
                {
                    Title = $"ECDC - '{query}'",
                    Description = "Europeiska smittskyddsriktlinjer",
                    Url = $"https://www.ecdc.europa.eu/en/search?search_api_views_fulltext={Uri.EscapeDataString(query)}",
                    Category = "Smittskydd",
                    Source = "ECDC",
                    RelevanceScore = 75
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Guideline sources search failed for query '{Query}'", query);
            }

            return results;
        }

        private List<ExternalResource> GetStaticResources()
        {
            return new List<ExternalResource>
            {
                new ExternalResource
                {
                    Title = "Vårdhandboken - Startsida",
                    Description = "Nationella riktlinjer och kunskapsstöd för vårdpersonal",
                    Url = "https://vardhandboken.se",
                    Category = "Riktlinjer",
                    Source = "Socialstyrelsen",
                    RelevanceScore = 100
                },
                new ExternalResource
                {
                    Title = "1177 - Omvårdnad",
                    Description = "Praktiska råd och information för vårdpersonal",
                    Url = "https://www.1177.se/omvardnad",
                    Category = "Praktisk",
                    Source = "1177",
                    RelevanceScore = 95
                },
                new ExternalResource
                {
                    Title = "Läkemedelsboken",
                    Description = "Läkemedelsinformation, interaktioner och dosering",
                    Url = "https://lakemedelsboken.se",
                    Category = "Läkemedel",
                    Source = "Läkemedelsverket",
                    RelevanceScore = 90
                },
                new ExternalResource
                {
                    Title = "SBU - Publikationer",
                    Description = "Evidensbaserade utvärderingar av vårdmetoder",
                    Url = "https://www.sbu.se/sv/publikationer/",
                    Category = "Forskning",
                    Source = "SBU",
                    RelevanceScore = 85
                },
                new ExternalResource
                {
                    Title = "Socialstyrelsen - Riktlinjer",
                    Description = "Nationella riktlinjer för hälso- och sjukvård",
                    Url = "https://www.socialstyrelsen.se/utveckla-verksamhet/evidensbaserad-praktik/nationella-riktlinjer/",
                    Category = "Riktlinjer",
                    Source = "Socialstyrelsen",
                    RelevanceScore = 90
                },
                new ExternalResource
                {
                    Title = "Khan Academy - Health & Medicine",
                    Description = "Gratis utbildningsmaterial om hälsa och medicin",
                    Url = "https://www.khanacademy.org/science/health-and-medicine",
                    Category = "Utbildning",
                    Source = "Khan Academy",
                    RelevanceScore = 70
                },
                new ExternalResource
                {
                    Title = "PubMed Central - Free Articles",
                    Description = "Vetenskapliga artiklar med öppen tillgång",
                    Url = "https://www.ncbi.nlm.nih.gov/pmc/",
                    Category = "Forskning",
                    Source = "NIH",
                    RelevanceScore = 80
                },
                new ExternalResource
                {
                    Title = "WHO - Health Topics",
                    Description = "Globala hälsorapporter och riktlinjer",
                    Url = "https://www.who.int/health-topics",
                    Category = "Riktlinjer",
                    Source = "WHO",
                    RelevanceScore = 85
                }
            };
        }


        private double CalculateRelevance(string content, string query)
        {
            var words = query.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var contentLower = content.ToLower();
            if (words.Length == 0) return 0;
            var matchCount = words.Count(w => contentLower.Contains(w));
            return matchCount / (double)words.Length;
        }
    }
}