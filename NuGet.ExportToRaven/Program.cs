using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Raven.Abstractions.Commands;
using Raven.Client;
using Raven.Client.Document;
using Raven.Imports.Newtonsoft.Json;
using Raven.Json.Linq;

namespace NuGet.ExportToRaven
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
				string url = "https://nuget.org/api/v2/Packages";
				while (true)
				{
					Console.WriteLine("GET {0}", url);
					if (url == null)
						break;
					var webRequest = (HttpWebRequest)WebRequest.Create(url);
					webRequest.Accept = "application/json";
					using (var resp = webRequest.GetResponse())
					using (var strema = resp.GetResponseStream())
					{
						url = WritePackagesToRaven(strema, store);
					}
				}
			}


		}

		private static string WritePackagesToRaven(Stream strema, IDocumentStore store)
		{
			var json = RavenJToken.ReadFrom(new JsonTextReader(new StreamReader(strema)))
				.Value<RavenJObject>("d");


			using (var session = store.OpenSession())
			{
				foreach (RavenJObject result in json.Value<RavenJArray>("results"))
				{
					ModifyResult(result);
					session.Advanced.Defer(new PutCommandData
						{
							Document = result,
							Metadata = new RavenJObject
								{
									{"Raven-Entity-Name", "Packages"}
								},
							Key = "packages/" + result.Value<string>("PackageId") + "/" + result.Value<string>("Version")
						});
				}
				session.SaveChanges();
			}
			return json.Value<string>("__next");
		}

		private static void ModifyResult(RavenJObject result)
		{
			var tags = result.Value<string>("Tags");
			if (tags != null)
			{
				result["Tags"] =
					new RavenJArray(tags.Split(new[] {' ', ',', ';'}, StringSplitOptions.RemoveEmptyEntries));
			}
			else
			{
				result["Tags"] = new RavenJArray();
			}
			var deps = result.Value<string>("Dependencies");
			if (deps != null)
			{
				result["Dependencies"] =
					new RavenJArray(deps.Split(new[] {'|'}, StringSplitOptions.RemoveEmptyEntries)
						                .Select(s =>
							                {
								                var strings = s.Split(':');
								                return RavenJObject.FromObject(new {Package = strings[0], Version = strings[1]});
							                }));
			}
			result["PackageId"] = result["Id"];
			result.Remove("Id");
			result.Remove("__metadata");
			result.Remove("DownloadCount");
		}
	}
}
