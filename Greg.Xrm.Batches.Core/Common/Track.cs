using Common.Logging;
using System;
using System.Diagnostics;

namespace Avanade.Rina.Batches.Core.Common
{
	public class Track : IDisposable
	{
		private readonly ILog log;
		private readonly string message;
		private readonly Stopwatch stopwatch;
		private readonly bool isEnabled;
		private bool disposed;

		public static Track This(ILog log, string message)
		{
			return new Track(log, message);
		}


		protected Track(ILog log, string message)
		{
			if (log == null)
				throw new ArgumentNullException(nameof(log));
			if (string.IsNullOrWhiteSpace(message))
				throw new ArgumentNullException(nameof(message));

			this.isEnabled = log.IsDebugEnabled;
			if (!this.isEnabled) return;

			this.log = log;
			this.message = message;
			this.stopwatch = Stopwatch.StartNew();
			this.log.Debug(this.message + "...");
		}


		public void Dispose()
		{
			if (this.disposed)
				throw new ObjectDisposedException("Track", "Tracker already disposed!");
			if (!this.isEnabled) 
				return;

			lock (this)
			{
				this.stopwatch.Stop();
				this.log.Debug(message + "...COMPLETED in: " + this.stopwatch.Elapsed);
				this.disposed = true;
			}
		}
	}
}
