using System.Collections.Generic;
using NuGet.Web.Indexes;
using Raven.Client.Linq;
using System.Linq;

namespace NuGet.Web.Controllers
{
	public class PackagesController : RavenController
	{
		public IEnumerable<Packages_Search.ReduceResult> Get(int page = 0)
		{
			return DocumentSession.Query<Packages_Search.ReduceResult, Packages_Search>()
				.Where(x=>x.IsPrerelease == false)
				.OrderByDescending(x=>x.DownloadCount)
					.ThenBy(x=>x.Created)
				.Skip(page*30)
				.Take(30)
				.ToList();
		}
	}

	public class SearchController : RavenController
	{
		public IEnumerable<Packages_Search.ReduceResult> Get(string q, int page = 0)
		{
			return DocumentSession.Query<Packages_Search.ReduceResult, Packages_Search>()
				.Search(x => x.Query, q)
				.Where(x => x.IsPrerelease == false)
				.OrderByDescending(x => x.DownloadCount)
					.ThenBy(x => x.Created)
				.Skip(page * 30)
				.Take(30)
				.ToList();
		}
	}
}