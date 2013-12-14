using System;
using System.Collections.Generic;
using System.Text;

namespace LD28.Core
{
	/// <summary>
	/// Contains methods for visually creating time spans.
	/// Original code: https://github.com/phatboyg/Magnum/blob/master/src/Magnum/Extensions/ExtensionsToTimeSpan.cs
	/// </summary>
	public static class TimeSpanExtensions
	{
		private static readonly TimeSpan Day = TimeSpan.FromDays(1);
		private static readonly TimeSpan Hour = TimeSpan.FromHours(1);
		private static readonly TimeSpan Month = TimeSpan.FromDays(30);
		private static readonly TimeSpan Year = TimeSpan.FromDays(365);

		/// <summary>
		/// Creates a TimeSpan for the specified number of weeks
		/// </summary>
		/// <param name="value">The number of weeks</param>
		/// <returns></returns>
		public static TimeSpan Weeks(this int value)
		{
			return TimeSpan.FromDays(value * 7);
		}

		/// <summary>
		/// Creates a TimeSpan for the specified number of days
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static TimeSpan Days(this int value)
		{
			return TimeSpan.FromDays(value);
		}

		/// <summary>
		/// Creates a TimeSpan for the specified number of hours
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static TimeSpan Hours(this int value)
		{
			return TimeSpan.FromHours(value);
		}

		/// <summary>
		/// Creates a TimeSpan for the specified number of minutes
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static TimeSpan Minutes(this int value)
		{
			return TimeSpan.FromMinutes(value);
		}

		/// <summary>
		/// Creates a TimeSpan for the specified number of seconds
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static TimeSpan Seconds(this int value)
		{
			return TimeSpan.FromSeconds(value);
		}

		public static TimeSpan Seconds(this double value)
		{
			return TimeSpan.FromSeconds(value);
		}

		/// <summary>
		/// Creates a TimeSpan for the specified number of milliseconds
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static TimeSpan Milliseconds(this int value)
		{
			return TimeSpan.FromMilliseconds(value);
		}

		/// <summary>
		/// Returns an enumeration of the specified TimeSpan with the specified number of elements
		/// </summary>
		/// <param name="value">The TimeSpan to repeat</param>
		/// <param name="times">The number of times to repeat the TimeSpan</param>
		/// <returns>An enumeration of TimeSpan</returns>
		public static IEnumerable<TimeSpan> Repeat(this TimeSpan value, int times)
		{
			for (var i = 0; i < times; i++)
				yield return value;
		}

		public static string ToFriendlyString(this TimeSpan ts)
		{
			if (ts.Equals(Month))
				return "1M";
			if (ts.Equals(Year))
				return "1y";
			if (ts.Equals(Day))
				return "1d";
			if (ts.Equals(Hour))
				return "1h";

			var sb = new StringBuilder();

			var years = ts.Days / 365;
			var months = (ts.Days % 365) / 30;
			var weeks = ((ts.Days % 365) % 30) / 7;
			var days = (((ts.Days % 365) % 30) % 7);

			var parts = new List<string>();

			if (years > 0)
				sb.Append(years).Append("y");

			if (months > 0)
				sb.Append(months).Append("M");

			if (weeks > 0)
				sb.Append(weeks).Append("w");

			if (days > 0)
				sb.Append(days).Append("d");

			if (ts.Hours > 0)
				sb.Append(ts.Hours).Append("h");
			if (ts.Minutes > 0)
				sb.Append(ts.Minutes).Append("m");
			if (ts.Seconds > 0)
				sb.Append(ts.Seconds).Append("s");
			if (ts.Milliseconds > 0)
				sb.Append(ts.Milliseconds).Append("ms");

			return sb.ToString();
		}
	}
}
