﻿namespace PolarisGateway.Domain.PolarisPipeline
{
	public enum TrackerStatus
	{
		Initialised,
		NotStarted,
		Running,
		// ReSharper disable once InconsistentNaming
		NoDocumentsFoundInDDEI,
		Completed,
		Failed,
		UnableToEvaluateExistingDocuments
	}
}
