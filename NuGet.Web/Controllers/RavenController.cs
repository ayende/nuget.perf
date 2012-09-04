using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using NuGet.Web.Indexes;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Indexes;

namespace NuGet.Web.Controllers
{
	public class RavenController : ApiController
	{
		private static IDocumentStore documentStore;

		public static IDocumentStore DocumentStore
		{
			get
			{
				if (documentStore == null)
				{
					lock (typeof (RavenController))
					{
						if (documentStore != null)
							return documentStore;
						documentStore = new DocumentStore
							{
								Url = "http://localhost:8080",
								DefaultDatabase = "Nuget"
							}.Initialize();
						IndexCreation.CreateIndexes(typeof (Packages_Search).Assembly, documentStore);
					}
				}
				return documentStore;
			}
		}

		public IDocumentSession DocumentSession { get; set; }

		public override async Task<HttpResponseMessage> ExecuteAsync(HttpControllerContext controllerContext, CancellationToken cancellationToken)
		{
			using (DocumentSession = DocumentStore.OpenSession())
			{
				HttpResponseMessage result = await base.ExecuteAsync(controllerContext, cancellationToken);
				DocumentSession.SaveChanges();
				return result;
			}
		}
	}
}