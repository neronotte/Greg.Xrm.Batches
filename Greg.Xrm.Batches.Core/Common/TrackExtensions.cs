using System;
using Common.Logging;

namespace Avanade.Rina.Batches.Core.Common
{
	public static class TrackExtensions
	{
		public static IDisposable Track(this ILog log, string message)
		{
			return Avanade.Rina.Batches.Core.Common.Track.This(log, message);
		}
	}
}