using System;
using System.Linq;
using NuGet.Web.Models;
using Raven.Abstractions.Indexing;
using Raven.Client.Indexes;

namespace NuGet.Web.Indexes
{
	public class Packages_Search : AbstractIndexCreationTask<Package, Packages_Search.ReduceResult>
	{
		public class ReduceResult
		{
			public DateTime Created { get; set; }
			public int DownloadCount { get; set; }
			public string PackageId { get; set; }
			public bool IsPrerelease { get; set; }
			public object[] Query { get; set; }
		}

		public Packages_Search()
		{
			Map = packages => from p in packages
			                  select new
				                  {
					                  p.Created, 
									  DownloadCount = p.VersionDownloadCount, 
									  p.PackageId, 
									  p.IsPrerelease,
									  Query = new object[] { p.Tags, p.Title, p.PackageId}
				                  };
			Reduce = results =>
			         from result in results
			         group result by new {result.PackageId, result.IsPrerelease}
			         into g
			         select new
					         {
						         g.Key.PackageId,
						         g.Key.IsPrerelease,
						         DownloadCount = g.Sum(x => x.DownloadCount),
						         Created = g.Select(x => x.Created).OrderBy(x => x).First(),
								 Query = g.SelectMany(x=>x.Query).Distinct()
					         };

			Store(x=>x.Query, FieldStorage.No);
		}
	}
}