#if __CONSTRAINED__ || UNITY_STANDALONE_LINUX || UNITY
using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace Lidgren.Network
{
    internal static class ConstrainedClock
    {
        private const double TicksPerSecond = TimeSpan.TicksPerSecond;

        public static long Ticks
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => DateTime.UtcNow.Ticks;
		}

        public static long Milliseconds
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond;
		}

        public static double Seconds
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => DateTime.UtcNow.Ticks / TicksPerSecond;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static long Since(long ticks) => Ticks - ticks;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static long SinceMilliseconds(long ticks) => (Ticks - ticks) / TimeSpan.TicksPerMillisecond;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static double SinceSeconds(long ticks) => (Ticks - ticks) / TicksPerSecond;
    }

	public static partial class NetUtility
	{
		private static byte[] s_randomMacBytes;
		
		static NetUtility()
		{
		}

		[CLSCompliant(false)]
		public static ulong GetPlatformSeed(int seedInc)
		{
			ulong seed = (ulong)ConstrainedClock.Ticks + (ulong)seedInc;
			return seed ^ ((ulong)(new object().GetHashCode()) << 32);
		}
		
		/// <summary>
		/// Gets my local IPv4 address (not necessarily external) and subnet mask
		/// </summary>
		public static IPAddress GetMyAddress(out IPAddress mask)
		{
			mask = null;
#if UNITY_ANDROID || UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_IOS || UNITY
			try
			{
				if (!(UnityEngine.Application.internetReachability == UnityEngine.NetworkReachability.NotReachable))
				{
					return null;
				}
				return IPAddress.Parse(UnityEngine.Network.player.externalIP);
			}
			catch // Catch Access Denied errors
			{
				return null;
			}
#endif
			return null;
		}

		public static byte[] GetMacAddressBytes()
		{
			if (s_randomMacBytes == null)
			{
				s_randomMacBytes = new byte[8];
				MWCRandom.Instance.NextBytes(s_randomMacBytes);
			}
			return s_randomMacBytes;
		}

		public static IPAddress GetBroadcastAddress()
		{
			return IPAddress.Broadcast;
		}

		public static void Sleep(int milliseconds)
		{
			System.Threading.Thread.Sleep(milliseconds);
		}

		public static IPAddress CreateAddressFromBytes(byte[] bytes)
		{
			return new IPAddress(bytes);
		}

		private static readonly SHA1 s_sha = SHA1.Create();
		public static byte[] ComputeSHAHash(byte[] bytes, int offset, int count)
		{
			return s_sha.ComputeHash(bytes, offset, count);
		}
	}

	public static partial class NetTime
	{
		private static readonly long s_timeInitialized = ConstrainedClock.Ticks;

        /// <summary>
        /// Get number of seconds since the application started
        /// </summary>
        public static double Now
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => ConstrainedClock.SinceSeconds(s_timeInitialized);
		}
	}
}
#endif
