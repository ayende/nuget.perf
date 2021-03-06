﻿namespace NuGet.Web.Models
{
	public class Package
	{
		public string Id { get; set; }
		public string Version { get; set; }
		public string Authors { get; set; }
		public string Copyright { get; set; }
		public string Created { get; set; }
		public string[] Dependencies { get; set; }
		public string Description { get; set; }
		public string GalleryDetailsUrl { get; set; }
		public string IconUrl { get; set; }
		public bool IsLatestVersion { get; set; }
		public bool IsAbsoluteLatestVersion { get; set; }
		public bool IsPrerelease { get; set; }
		public string Language { get; set; }
		public string LastUpdated { get; set; }
		public string Published { get; set; }
		public string LicenseUrl { get; set; }
		public string PackageHash { get; set; }
		public string PackageHashAlgorithm { get; set; }
		public string PackageSize { get; set; }
		public string ProjectUrl { get; set; }
		public string ReportAbuseUrl { get; set; }
		public string ReleaseNotes { get; set; }
		public bool RequireLicenseAcceptance { get; set; }
		public string Summary { get; set; }
		public string[] Tags { get; set; }
		public string Title { get; set; }
		public int VersionDownloadCount { get; set; }
		public string PackageId { get; set; }
	}
}