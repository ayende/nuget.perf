using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Raven.Abstractions.Data;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Linq;

namespace NuGet.Search
{
	class Program
	{
		static void Main(string[] args)
		{
			using (var store = new DocumentStore
				{
					Url = "http://localhost:8080",
					DefaultDatabase = "Nuget"
				}.Initialize())
			{

				using (var session = store.OpenSession())
				{
					session.Store(new FacetSetup
						{
							Id = "facets/PackagesTags",
							Facets =
							{
								new Facet
									{
										Name = "Tags",
										MaxResults = 4,
										Mode = FacetMode.Default,
										TermSortMode = FacetTermSortMode.HitsDesc
									}
							},
						});
					session.SaveChanges();
				}
				while (true)
				{
					Console.Write("Search: ");
					var search = Console.ReadLine();
					if (string.IsNullOrEmpty(search))
					{
						Console.Clear();
						continue;
					}
					using (var session = store.OpenSession())
					{
						var q= PeformQuery(session, search);
						if(q == null)
							continue;
						var facetResults = q.ToFacets("facets/PackagesTags");
						foreach (var result in facetResults.Results)
						{
							Console.WriteLine();
							Console.Write("{0}:\t", result.Key);
							foreach (var val in result.Value.Values)
							{
								Console.Write("{0} [{1:#,#}] | ", val.Range, val.Hits);
							}
							Console.WriteLine();
						}
					}
				}
			}
		}

		private static IQueryable<Package> PeformQuery(IDocumentSession session, string search, bool guessIfNoResultsFound = true)
		{
			var q = session.Query<PackageSearch>("Packages/Search")
				.Search(x => x.Query, search)
				.Where(x => x.IsLatestVersion && x.IsAbsoluteLatestVersion && x.IsPrerelease == false)
				.As<Package>()
				.OrderByDescending(x => x.VersionDownloadCount).ThenBy(x => x.Created)
				.Take(3);
			var packages = q.ToList();

			if (packages.Count > 0)
			{
				foreach (var package in packages)
				{
					Console.WriteLine("\t{0}", package.Id);
				}
				return q;
			}
			
			if (guessIfNoResultsFound)
			{
				DidYouMean(session, search);
			}
			else
			{
				Console.WriteLine("\tNo search results were found");
			}

			return null;
		}

		private static void DidYouMean(IDocumentSession session, string search)
		{
			var suggestionQueryResult = session.Query<PackageSearch>("Packages/Search")
				.Search(x => x.Query, search)
				.Suggest();
			switch (suggestionQueryResult.Suggestions.Length)
			{
				case 0:
					Console.WriteLine("\tNo search results were found");
					break;
				case 1:
					// we may have it filtered because of the other conditions, don't recurse again
					Console.WriteLine("\tSearch corrected to: {0}", suggestionQueryResult.Suggestions[0]);
					Console.WriteLine();
					PeformQuery(session, suggestionQueryResult.Suggestions[0], guessIfNoResultsFound: false);
					break;
				default:
					Console.WriteLine("\tDid you mean?");
					foreach (var suggestion in suggestionQueryResult.Suggestions)
					{
						Console.WriteLine("\t - {0} ?", suggestion);
					}
					break;
			}
		}
	}

	public class Package
	{
		public string DocId { get; set; }
		public string Id { get; set; }
		public string Title { get; set; }
		public string Authors { get; set; }
		public string Version { get; set; }
		public string[] Tags { get; set; }

		public int VersionDownloadCount { get; set; }
		public DateTime Created { get; set; }
	}

	public class PackageSearch
	{
		public string Query;
		public bool IsLatestVersion, IsAbsoluteLatestVersion, IsPrerelease;
	}
}
