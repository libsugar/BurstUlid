using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading;
using AOT;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace LibSugar.Unity
{

    [BurstCompile]
    [Serializable]
    [DebuggerDisplay("{ToString(),nq}")]
    [TypeConverter(typeof(BurstUlidTypeConverter))]
    [StructLayout(LayoutKind.Explicit, Size = 16)]
#if HAS_SYSTEM_TEXT_JSON
    [System.Text.Json.Serialization.JsonConverter(typeof(BurstUlidJsonConverter))]
#endif
    public struct BurstUlid : IEquatable<BurstUlid>, IComparable<BurstUlid>
    {
        // Binary layout in memory (if memory is Little-Endian)
        // 
        // 0                   1                   2                   3
        //  0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
        // +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        // |                    32_bit_uint_time_low_0123                  |
        // +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        // |    16_bit_uint_time_high_45    |      16_bit_uint_random_01   |
        // +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        // |                     32_bit_uint_random_2345                   |
        // +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        // |                     32_bit_uint_random_6789                   |
        // +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        //
        // Binary layout in net (and ulid stand)
        // 
        // 0                   1                   2                   3
        //  0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
        // +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        // |                    32_bit_uint_time_high_5432                 |
        // +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        // |    16_bit_uint_time_low_10     |      16_bit_uint_random_98   |
        // +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        // |                     32_bit_uint_random_7654                   |
        // +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        // |                     32_bit_uint_random_3210                   |
        // +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+

        [FieldOffset(0)]
        internal v128 _v;

        #region ser field
        
        [FieldOffset(0)]
        [SerializeField]
        internal ulong _0;
        [FieldOffset(sizeof(ulong))]
        [SerializeField]
        internal ulong _1;
        
        #endregion

        #region const

        public static readonly BurstUlid Empty;

        #endregion

        #region ctor

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BurstUlid(Guid guid) => this = FromGuid(guid);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BurstUlid(int4 int4) => this = FromInt4(int4);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BurstUlid(uint4 uint4) => this = FromUInt4(uint4);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BurstUlid(v128 v128) => this = FromV128(v128);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BurstUlid(ReadOnlySpan<byte> bytes) => this = FromBytes(bytes);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BurstUlid(string str)
            => this = BurstUlidImpl.TryParse(str, out var ulid) ? ulid : throw new UlidInvalidFormatException();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BurstUlid(ReadOnlySpan<char> str)
            => this = BurstUlidImpl.TryParse(str, out var ulid) ? ulid : throw new UlidInvalidFormatException();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BurstUlid(FixedString32Bytes str)
            => this = BurstUlidImpl.TryParse(str, out var ulid) ? ulid : throw new UlidInvalidFormatException();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BurstUlid(FixedString64Bytes str)
            => this = BurstUlidImpl.TryParse(str, out var ulid) ? ulid : throw new UlidInvalidFormatException();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BurstUlid(FixedString128Bytes str)
            => this = BurstUlidImpl.TryParse(str, out var ulid) ? ulid : throw new UlidInvalidFormatException();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BurstUlid(FixedString512Bytes str)
            => this = BurstUlidImpl.TryParse(str, out var ulid) ? ulid : throw new UlidInvalidFormatException();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BurstUlid(FixedString4096Bytes str)
            => this = BurstUlidImpl.TryParse(str, out var ulid) ? ulid : throw new UlidInvalidFormatException();

        #endregion

        #region create

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BurstUlid NewUlid() => BurstUlidImpl.NewUlid();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BurstUlid NewUlid(long timestamp) => BurstUlidImpl.NewUlid(timestamp);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BurstUlid NewUlid(long timestamp, BurstUlidRandomness rand)
            => BurstUlidImpl.NewUlid(timestamp, rand);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BurstUlid NewUlid(ref Random random)
            => BurstUlidImpl.NewUlid(ref random);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BurstUlid NewUlid(long timestamp, ref Random random)
            => BurstUlidImpl.NewUlid(timestamp, ref random);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BurstUlid NewUlidCryptoRand() => BurstUlidImpl.NewUlidCryptoRand();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BurstUlid NewUlidCryptoRand(long timestamp) => BurstUlidImpl.NewUlidCryptoRand(timestamp);

        #endregion

        #region cast

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Guid ToGuid() => BurstUlidImpl.ToGuid(this);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int4 ToInt4() => BurstUlidImpl.ToInt4(this);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly uint4 ToUInt4() => BurstUlidImpl.ToUInt4(this);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly v128 ToV128() => BurstUlidImpl.ToV128(this);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BurstUlid FromGuid(Guid guid) => BurstUlidImpl.FromGuid(guid);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BurstUlid FromInt4(int4 int4) => BurstUlidImpl.FromInt4(int4);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BurstUlid FromUInt4(uint4 uint4) => BurstUlidImpl.FromUInt4(uint4);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BurstUlid FromV128(v128 v128) => BurstUlidImpl.FromV128(v128);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BurstUlid FromBytes(ReadOnlySpan<byte> bytes) => BurstUlidImpl.FromBytes(bytes);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator BurstUlid(Guid guid) => FromGuid(guid);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Guid(BurstUlid ulid) => ulid.ToGuid();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator BurstUlid(int4 int4) => FromInt4(int4);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator int4(BurstUlid ulid) => ulid.ToInt4();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator BurstUlid(uint4 uint4) => FromUInt4(uint4);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator uint4(BurstUlid ulid) => ulid.ToUInt4();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator BurstUlid(v128 v128) => FromV128(v128);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator v128(BurstUlid ulid) => ulid.ToV128();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void ToBytes(Span<byte> bytes) => BurstUlidImpl.ToBytes(this, bytes);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly byte[] ToByteArray() => BurstUlidImpl.ToByteArray(this);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryWriteBytes(Span<byte> bytes) => BurstUlidImpl.TryWriteBytes(this, bytes);

        #endregion

        #region eq

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(BurstUlid other) => BurstUlidImpl.Equals(this, other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override bool Equals(object obj) => obj is BurstUlid other && Equals(other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override int GetHashCode() => BurstUlidImpl.GetHashCode(this);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(BurstUlid left, BurstUlid right) => left.Equals(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(BurstUlid left, BurstUlid right) => !left.Equals(right);

        #endregion

        #region get

        public readonly long Timestamp
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => BurstUlidImpl.TakeTimestamp(this);
        }

        public readonly DateTimeOffset DateTimeOffset
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => DateTimeOffset.FromUnixTimeMilliseconds(Timestamp);
        }

        public readonly DateTime DateTime
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => DateTimeOffset.DateTime;
        }

        public readonly BurstUlidRandomness Randomness
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => BurstUlidImpl.TakeRandomness(this);
        }

        #endregion

        #region to_string

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override string ToString()
            => BurstUlidImpl.ToString(this);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly FixedString32Bytes ToFixedString()
            => BurstUlidImpl.ToFixedString(this);

        #endregion

        #region try_parse

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryParse(string str, out BurstUlid ulid)
            => BurstUlidImpl.TryParse(str, out ulid);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryParse(ReadOnlySpan<char> str, out BurstUlid ulid)
            => BurstUlidImpl.TryParse(str, out ulid);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryParse(FixedString32Bytes str, out BurstUlid ulid)
            => BurstUlidImpl.TryParse(str, out ulid);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryParse(FixedString64Bytes str, out BurstUlid ulid)
            => BurstUlidImpl.TryParse(str, out ulid);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryParse(FixedString128Bytes str, out BurstUlid ulid)
            => BurstUlidImpl.TryParse(str, out ulid);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryParse(FixedString512Bytes str, out BurstUlid ulid)
            => BurstUlidImpl.TryParse(str, out ulid);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryParse(FixedString4096Bytes str, out BurstUlid ulid)
            => BurstUlidImpl.TryParse(str, out ulid);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryParseUtf8(ReadOnlySpan<byte> str, out BurstUlid ulid)
            => BurstUlidImpl.TryParseUtf8(str, out ulid);

        #endregion

        #region parse

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BurstUlid Parse(string str)
            => BurstUlidImpl.TryParse(str, out var ulid) ? ulid : throw new UlidInvalidFormatException();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BurstUlid Parse(ReadOnlySpan<char> str)
            => BurstUlidImpl.TryParse(str, out var ulid) ? ulid : throw new UlidInvalidFormatException();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BurstUlid Parse(FixedString32Bytes str)
            => BurstUlidImpl.TryParse(str, out var ulid) ? ulid : throw new UlidInvalidFormatException();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BurstUlid Parse(FixedString64Bytes str)
            => BurstUlidImpl.TryParse(str, out var ulid) ? ulid : throw new UlidInvalidFormatException();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BurstUlid Parse(FixedString128Bytes str)
            => BurstUlidImpl.TryParse(str, out var ulid) ? ulid : throw new UlidInvalidFormatException();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BurstUlid Parse(FixedString512Bytes str)
            => BurstUlidImpl.TryParse(str, out var ulid) ? ulid : throw new UlidInvalidFormatException();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BurstUlid Parse(FixedString4096Bytes str)
            => BurstUlidImpl.TryParse(str, out var ulid) ? ulid : throw new UlidInvalidFormatException();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BurstUlid ParseUtf8(ReadOnlySpan<byte> str)
            => BurstUlidImpl.TryParseUtf8(str, out var ulid) ? ulid : throw new UlidInvalidFormatException();

        #endregion

        #region compare

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int CompareTo(BurstUlid other) => BurstUlidImpl.CompareTo(this, other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(BurstUlid left, BurstUlid right) => left.CompareTo(right) < 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(BurstUlid left, BurstUlid right) => left.CompareTo(right) > 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(BurstUlid left, BurstUlid right) => left.CompareTo(right) <= 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(BurstUlid left, BurstUlid right) => left.CompareTo(right) >= 0;

        #endregion

        #region to_net_format

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly v128 ToNetFormat()
            => BurstUlidImpl.ToNetFormat(this);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BurstUlid FromNetFormat(v128 v128)
            => BurstUlidImpl.FromNetFormat(v128);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void WriteNetFormatBytes(Span<byte> bytes)
            => BurstUlidImpl.WriteNetFormatBytes(this, bytes);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BurstUlid FromNetFormatBytes(ReadOnlySpan<byte> bytes)
            => BurstUlidImpl.FromNetFormatBytes(bytes);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryWriteNetFormatBytes(Span<byte> bytes)
            => BurstUlidImpl.TryWriteNetFormatBytes(this, bytes);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryFromNetFormatBytes(ReadOnlySpan<byte> bytes, out BurstUlid ulid)
            => BurstUlidImpl.TryFromNetFormatBytes(bytes, out ulid);

        #endregion

        #region init

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void InitStatic()
        {
            GetTicks.Init();
            GetCryptoRand.Init();
        }

        #endregion
    }

    [StructLayout(LayoutKind.Explicit, Size = 16)]
    public struct BurstUlidRandomness
    {
        [FieldOffset(0)]
        internal ulong _a;
        [FieldOffset(sizeof(ulong))]
        internal ushort _b;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BurstUlidRandomness FromBytes(ReadOnlySpan<byte> bytes)
            => BurstUlidImpl.RandomnessFromBytes(bytes);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly byte[] ToByteArray()
            => BurstUlidImpl.RandomnessToByteArray(this);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryWriteBytes(BurstUlidRandomness ulid, Span<byte> bytes)
            => BurstUlidImpl.RandomnessTryWriteBytes(this, bytes);
    }

    [BurstCompile]
    public static class BurstUlidImpl
    {
        #region create

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BurstUlid NewUlid()
        {
            NewUlid(out var ulid);
            return ulid;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BurstUlid NewUlid(long timestamp)
        {
            NewUlid(timestamp, out var ulid);
            return ulid;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BurstUlid NewUlid(long timestamp, BurstUlidRandomness rand)
        {
            NewUlid(timestamp, rand, out var ulid);
            return ulid;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BurstUlid NewUlid(ref Random random)
        {
            NewUlid(ref random, out var ulid);
            return ulid;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BurstUlid NewUlid(long timestamp, ref Random random)
        {
            NewUlid(timestamp, ref random, out var ulid);
            return ulid;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BurstUlid NewUlidCryptoRand()
        {
            NewUlidCryptoRand(out var ulid);
            return ulid;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BurstUlid NewUlidCryptoRand(long timestamp)
        {
            NewUlidCryptoRand(timestamp, out var ulid);
            return ulid;
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void NewUlid(out BurstUlid ulid)
        {
            var ticks = GetTicks.Invoke();
            var timestamp = UtcTicksToUnixTimeMilliseconds(ticks);
            BurstUlidRandomness randomness = default;
            var random = new Random((uint)ticks.GetHashCode());
            Unsafe.As<BurstUlidRandomness, int3>(ref randomness) = random.NextInt3();
            NewUlid(timestamp, randomness, out ulid);
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void NewUlid(long timestamp, out BurstUlid ulid)
        {
            BurstUlidRandomness randomness = default;
            var random = new Random((uint)GetTicks.Invoke().GetHashCode());
            Unsafe.As<BurstUlidRandomness, int3>(ref randomness) = random.NextInt3();
            NewUlid(timestamp, randomness, out ulid);
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void NewUlid(ref Random random, out BurstUlid ulid)
        {
            var timestamp = UtcTicksToUnixTimeMilliseconds(GetTicks.Invoke());
            NewUlid(timestamp, ref random, out ulid);
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void NewUlid(long timestamp, ref Random random, out BurstUlid ulid)
        {
            BurstUlidRandomness randomness = default;
            Unsafe.As<BurstUlidRandomness, int3>(ref randomness) = random.NextInt3();
            NewUlid(timestamp, randomness, out ulid);
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void NewUlidCryptoRand(out BurstUlid ulid)
        {
            var timestamp = UtcTicksToUnixTimeMilliseconds(GetTicks.Invoke());
            BurstUlidRandomness randomness;
            GetCryptoRand.Invoke(&randomness);
            NewUlid(timestamp, randomness, out ulid);
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void NewUlidCryptoRand(long timestamp, out BurstUlid ulid)
        {
            BurstUlidRandomness randomness;
            GetCryptoRand.Invoke(&randomness);
            NewUlid(timestamp, randomness, out ulid);
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void NewUlid(long timestamp, in BurstUlidRandomness rand, out BurstUlid ulid)
        {
            var time_bits = ((ulong)timestamp & 0x00_00_FF_FF_FF_FF_FF_FFL) << 16;
            ulid = new() { _v = new v128(time_bits | rand._b, rand._a) };
        }

        #endregion

        #region cast

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BurstUlid FromGuid(Guid guid)
        {
            FromGuid(in guid, out var ulid);
            return ulid;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Guid ToGuid(BurstUlid ulid)
        {
            ToGuid(in ulid, out var guid);
            return guid;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BurstUlid ToBurstUlid(this Guid guid) => FromGuid(guid);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BurstUlid FromInt4(int4 int4)
        {
            FromInt4(in int4, out var ulid);
            return ulid;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int4 ToInt4(BurstUlid ulid)
        {
            ToInt4(in ulid, out var int4);
            return int4;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BurstUlid FromUInt4(uint4 uint4)
        {
            FromUInt4(in uint4, out var ulid);
            return ulid;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint4 ToUInt4(BurstUlid ulid)
        {
            ToUInt4(in ulid, out var uint4);
            return uint4;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BurstUlid FromV128(v128 v128)
        {
            FromV128(in v128, out var ulid);
            return ulid;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static v128 ToV128(BurstUlid ulid)
        {
            ToV128(in ulid, out var v128);
            return v128;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe BurstUlid FromBytes(ReadOnlySpan<byte> bytes)
        {
            if (bytes.Length < sizeof(BurstUlid)) throw new ArgumentOutOfRangeException(nameof(bytes));
            fixed (byte* ptr = bytes)
            {
                UnsafeFromBytes(ptr, out var ulid);
                return ulid;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void ToBytes(BurstUlid ulid, Span<byte> bytes)
        {
            if (bytes.Length < sizeof(BurstUlid)) throw new ArgumentOutOfRangeException(nameof(bytes));
            fixed (byte* ptr = bytes)
            {
                UnsafeToBytes(in ulid, ptr);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe byte[] ToByteArray(BurstUlid ulid)
        {
            var arr = new byte[sizeof(BurstUlid)];
            fixed (byte* ptr = arr)
            {
                UnsafeToBytes(in ulid, ptr);
            }
            return arr;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe bool TryWriteBytes(BurstUlid ulid, Span<byte> bytes)
        {
            if (bytes.Length < sizeof(BurstUlid)) return false;
            fixed (byte* ptr = bytes)
            {
                UnsafeToBytes(in ulid, ptr);
            }
            return true;
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void FromGuid(in Guid guid, out BurstUlid ulid)
        {
            ulid = Unsafe.As<Guid, BurstUlid>(ref Unsafe.AsRef(in guid));
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ToGuid(in BurstUlid ulid, out Guid guid)
        {
            guid = Unsafe.As<BurstUlid, Guid>(ref Unsafe.AsRef(in ulid));
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void FromInt4(in int4 int4, out BurstUlid ulid)
        {
            ulid = Unsafe.As<int4, BurstUlid>(ref Unsafe.AsRef(in int4));
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ToInt4(in BurstUlid ulid, out int4 int4)
        {
            int4 = Unsafe.As<BurstUlid, int4>(ref Unsafe.AsRef(in ulid));
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void FromUInt4(in uint4 uint4, out BurstUlid ulid)
        {
            ulid = Unsafe.As<uint4, BurstUlid>(ref Unsafe.AsRef(in uint4));
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ToUInt4(in BurstUlid ulid, out uint4 uint4)
        {
            uint4 = Unsafe.As<BurstUlid, uint4>(ref Unsafe.AsRef(in ulid));
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void FromV128(in v128 v128, out BurstUlid ulid)
        {
            ulid = new() { _v = v128 };
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ToV128(in BurstUlid ulid, out v128 v128)
        {
            v128 = ulid._v;
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void UnsafeFromBytes(byte* bytes, out BurstUlid ulid)
        {
            var source = new ReadOnlySpan<byte>(bytes, sizeof(BurstUlid));
            fixed (BurstUlid* ptr = &ulid)
            {
                var target = new Span<byte>(ptr, sizeof(BurstUlid));
                source.CopyTo(target);
            }
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void UnsafeToBytes(in BurstUlid ulid, byte* bytes)
        {
            fixed (BurstUlid* ptr = &ulid)
            {
                var source = new ReadOnlySpan<byte>(ptr, sizeof(BurstUlid));
                var target = new Span<byte>(bytes, sizeof(BurstUlid));
                source.CopyTo(target);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe BurstUlidRandomness RandomnessFromBytes(ReadOnlySpan<byte> bytes)
        {
            if (bytes.Length < sizeof(ulong) + sizeof(ushort)) throw new ArgumentOutOfRangeException(nameof(bytes));
            fixed (byte* ptr = bytes)
            {
                RandomnessUnsafeFromBytes(ptr, out var randomness);
                return randomness;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe byte[] RandomnessToByteArray(BurstUlidRandomness randomness)
        {
            var arr = new byte[sizeof(ulong) + sizeof(ushort)];
            fixed (byte* ptr = arr)
            {
                RandomnessUnsafeToBytes(in randomness, ptr);
            }
            return arr;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe bool RandomnessTryWriteBytes(BurstUlidRandomness randomness, Span<byte> bytes)
        {
            if (bytes.Length < sizeof(ulong) + sizeof(ushort)) return false;
            fixed (byte* ptr = bytes)
            {
                RandomnessUnsafeToBytes(in randomness, ptr);
            }
            return true;
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void RandomnessUnsafeFromBytes(byte* bytes, out BurstUlidRandomness randomness)
        {
            var source = new ReadOnlySpan<byte>(bytes, sizeof(ulong) + sizeof(ushort));
            fixed (BurstUlidRandomness* ptr = &randomness)
            {
                var target = new Span<byte>(ptr, sizeof(BurstUlidRandomness))[..(sizeof(ulong) + sizeof(ushort))];
                source.CopyTo(target);
            }
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void RandomnessUnsafeToBytes(in BurstUlidRandomness randomness, byte* bytes)
        {
            var target = new Span<byte>(bytes, sizeof(ulong) + sizeof(ushort));
            fixed (BurstUlidRandomness* ptr = &randomness)
            {
                var source =
                    new ReadOnlySpan<byte>(ptr, sizeof(BurstUlidRandomness))[..(sizeof(ulong) + sizeof(ushort))];
                source.CopyTo(target);
            }
        }

        #endregion

        #region eq

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Equals(in BurstUlid self, in BurstUlid other) => self.ToUInt4().Equals(other.ToUInt4());

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetHashCode(in BurstUlid self) => self.ToUInt4().GetHashCode();

        #endregion

        #region get

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BurstUlidRandomness TakeRandomness(BurstUlid ulid)
        {
            TakeRandomness(ulid, out var randomness);
            return randomness;
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long TakeTimestamp(in BurstUlid ulid)
            => (long)(ulid._v.ULong0 >> 16);

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void TakeRandomness(in BurstUlid ulid, out BurstUlidRandomness randomness)
        {
            randomness._a = ulid._v.ULong1;
            randomness._b = (ushort)(ulid._v.ULong0 & 0x00_00_00_00_00_00_FF_FFL);
        }

        #endregion

        #region to_string

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedString32Bytes ToFixedString(BurstUlid ulid)
        {
            ToFixedString(ulid, out var str);
            return str;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe string ToString(BurstUlid ulid)
            => string.Create(26, ulid, static (span, ulid) =>
            {
                fixed (char* ptr = span)
                {
                    UnsafeToString(ulid, (ushort*)ptr);
                }
            });

        public static readonly ushort[] Base32Chars =
        {
            48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 65, 66, 67, 68, 69, 70, 71, 72, 74, 75, 77, 78, 80, 81, 82, 83, 84,
            86, 87, 88, 89, 90
        };
        public static readonly byte[] Base32Bytes =
        {
            48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 65, 66, 67, 68, 69, 70, 71, 72, 74, 75, 77, 78, 80, 81, 82, 83, 84,
            86, 87, 88, 89, 90
        };

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void ToFixedString(in BurstUlid ulid, out FixedString32Bytes str)
        {
            str = default;
            str.Length = 26;
            var buffer = new Span<byte>(str.GetUnsafePtr(), 26);
            var (lo, up) = (ulid._v.ULong1, ulid._v.ULong0);
            for (var i = 0; i < 26; i++)
            {
                buffer[25 - i] = Base32Bytes[(int)((uint)lo & 31u)];
                lo = (lo >> 5) | (up << 59);
                up >>= 5;
            }
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void UnsafeToString(in BurstUlid ulid, ushort* ptr)
        {
            var buffer = new Span<ushort>(ptr, 26);
            var (lo, up) = (ulid._v.ULong1, ulid._v.ULong0);
            for (var i = 0; i < 26; i++)
            {
                buffer[25 - i] = Base32Chars[(int)((uint)lo & 31u)];
                lo = (lo >> 5) | (up << 59);
                up >>= 5;
            }
        }

        #endregion

        #region parse

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryParse(string str, out BurstUlid ulid)
            => TryParse((ReadOnlySpan<char>)str, out ulid);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe bool TryParse(ReadOnlySpan<char> str, out BurstUlid ulid)
        {
            if (str.Length < 26) throw new ArgumentOutOfRangeException(nameof(str.Length));
            fixed (char* ptr = str)
            {
                return UnsafeTryParse((ushort*)ptr, out ulid);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe bool TryParse(FixedString32Bytes str, out BurstUlid ulid)
        {
            if (str.Length < 26) throw new ArgumentOutOfRangeException(nameof(str.Length));
            return UnsafeTryParseUtf8(str.GetUnsafePtr(), out ulid);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe bool TryParse(FixedString64Bytes str, out BurstUlid ulid)
        {
            if (str.Length < 26) throw new ArgumentOutOfRangeException(nameof(str.Length));
            return UnsafeTryParseUtf8(str.GetUnsafePtr(), out ulid);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe bool TryParse(FixedString128Bytes str, out BurstUlid ulid)
        {
            if (str.Length < 26) throw new ArgumentOutOfRangeException(nameof(str.Length));
            return UnsafeTryParseUtf8(str.GetUnsafePtr(), out ulid);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe bool TryParse(FixedString512Bytes str, out BurstUlid ulid)
        {
            if (str.Length < 26) throw new ArgumentOutOfRangeException(nameof(str.Length));
            return UnsafeTryParseUtf8(str.GetUnsafePtr(), out ulid);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe bool TryParse(FixedString4096Bytes str, out BurstUlid ulid)
        {
            if (str.Length < 26) throw new ArgumentOutOfRangeException(nameof(str.Length));
            return UnsafeTryParseUtf8(str.GetUnsafePtr(), out ulid);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe bool TryParseUtf8(ReadOnlySpan<byte> str, out BurstUlid ulid)
        {
            if (str.Length < 26) throw new ArgumentOutOfRangeException(nameof(str.Length));
            fixed (byte* ptr = str)
            {
                return UnsafeTryParseUtf8(ptr, out ulid);
            }
        }

        public static readonly int[] lookup =
        {
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 0, 1, 2, 3, 4, 5, 6, 7,
            8, 9, -1, -1, -1, -1, -1, -1, -1, 10, 11, 12, 13, 14, 15, 16, 17, -1, 18, 19, -1, 20, 21, -1, 22, 23, 24,
            25, 26, -1, 27, 28, 29, 30, 31, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1
        };

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe bool UnsafeTryParse(ushort* ptr, out BurstUlid ulid)
        {
            var str = new Span<ushort>(ptr, 26);
            ulong lo = 0UL, up = 0UL;
            for (var i = 0; i < 26; i++)
            {
                var c = str[i];
                if (c > 'z')
                {
                    ulid = default;
                    return false;
                }
                var n = lookup[c];
                if (n < 0)
                {
                    ulid = default;
                    return false;
                }
                var n2 = (ulong)n;
                up = (up << 5) | (lo >> 59);
                lo = (lo << 5) | n2;
            }
            ulid = new() { _v = new(up, lo) };
            return true;
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe bool UnsafeTryParseUtf8(byte* ptr, out BurstUlid ulid)
        {
            var str = new Span<byte>(ptr, 26);
            ulong lo = 0UL, up = 0UL;
            for (var i = 0; i < 26; i++)
            {
                var c = str[i];
                if (c > 'z')
                {
                    ulid = default;
                    return false;
                }
                var n = lookup[c];
                if (n < 0)
                {
                    ulid = default;
                    return false;
                }
                var n2 = (ulong)n;
                up = (up << 5) | (lo >> 59);
                lo = (lo << 5) | n2;
            }
            ulid = new() { _v = new(up, lo) };
            return true;
        }

        #endregion

        #region compare

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CompareTo(in BurstUlid self, in BurstUlid other)
        {
            if (self.Equals(other)) return 0;
            if (self.Timestamp < other.Timestamp) return -1;
            if (self.Timestamp > other.Timestamp) return 1;
            if ((self.ToUInt4() > other.ToUInt4()).Equals(true)) return 1;
            return -1;
        }

        #endregion

        #region to_net_format

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static v128 ToNetFormat(BurstUlid ulid)
        {
            ToNetFormat(ulid, out var v128);
            return v128;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BurstUlid FromNetFormat(v128 v128)
        {
            FromNetFormat(v128, out var ulid);
            return ulid;
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void ToNetFormat(in BurstUlid ulid, out v128 v128)
        {
            if (BitConverter.IsLittleEndian)
            {
                ref readonly var v = ref ulid._v;
                v128 = new v128(v.Byte5, v.Byte4, v.Byte3, v.Byte2, v.Byte1, v.Byte0,
                    v.Byte15, v.Byte14, v.Byte13, v.Byte12, v.Byte11, v.Byte10, v.Byte9, v.Byte8, v.Byte7, v.Byte6);
            }
            else
            {
                fixed (v128* ptr = &v128)
                {
                    *(BurstUlid*)ptr = ulid;
                }
            }
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void FromNetFormat(in v128 v128, out BurstUlid ulid)
        {
            if (BitConverter.IsLittleEndian)
            {
                ref readonly var v = ref v128;
                ulid = new()
                {
                    _v = new v128(v.Byte5, v.Byte4, v.Byte3, v.Byte2, v.Byte1, v.Byte0,
                        v.Byte15, v.Byte14, v.Byte13, v.Byte12, v.Byte11, v.Byte10, v.Byte9, v.Byte8, v.Byte7, v.Byte6),
                };
            }
            else
            {
                fixed (BurstUlid* ptr = &ulid)
                {
                    *(v128*)ptr = v128;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteNetFormatBytes(BurstUlid ulid, Span<byte> bytes)
        {
            if (!TryWriteNetFormatBytes(ulid, bytes)) throw new ArgumentOutOfRangeException(nameof(bytes.Length));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BurstUlid FromNetFormatBytes(ReadOnlySpan<byte> bytes)
            => TryFromNetFormatBytes(bytes, out var ulid)
                ? ulid
                : throw new ArgumentOutOfRangeException(nameof(bytes.Length));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe bool TryWriteNetFormatBytes(BurstUlid ulid, Span<byte> bytes)
        {
            if (bytes.Length < sizeof(v128)) return false;
            fixed (byte* ptr = bytes)
            {
                UnsafeWriteNetFormatBytes(ulid, ptr);
            }
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe bool TryFromNetFormatBytes(ReadOnlySpan<byte> bytes, out BurstUlid ulid)
        {
            if (bytes.Length < sizeof(v128))
            {
                ulid = default;
                return false;
            }
            fixed (byte* ptr = bytes)
            {
                UnsafeFromNetFormatBytes(ptr, out ulid);
            }
            return true;
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void UnsafeWriteNetFormatBytes(in BurstUlid ulid, byte* bytes)
        {
            ToNetFormat(ulid, out var v128);
            var source = new Span<byte>(&v128, sizeof(v128));
            var target = new Span<byte>(bytes, sizeof(v128));
            source.CopyTo(target);
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void UnsafeFromNetFormatBytes(byte* bytes, out BurstUlid ulid)
        {
            var source = new Span<byte>(bytes, sizeof(v128));
            v128 v128;
            var target = new Span<byte>(&v128, sizeof(v128));
            source.CopyTo(target);
            FromNetFormat(v128, out ulid);
        }

        #endregion

        #region utils

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long UtcTicksToUnixTimeMilliseconds(long ticks)
        {
            var milliseconds = ticks / TimeSpan.TicksPerMillisecond;
            return milliseconds - 62_135_596_800_000;
        }

        #endregion
    }

    #region hybrid

    internal abstract unsafe class GetTicks
    {
        public static long Invoke()
        {
            var f = (delegate* unmanaged[Cdecl]<long>)fn_ptr.Data;
            if (f == null) throw new NullReferenceException();
            return f();
        }

        private static readonly SharedStatic<nuint> fn_ptr = SharedStatic<nuint>.GetOrCreate<GetTicks>();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate long Delegate();

        public static void Init() => fn_ptr.Data = (nuint)(nint)Marshal.GetFunctionPointerForDelegate((Delegate)Impl);

        [MonoPInvokeCallback(typeof(Delegate))]
        private static long Impl() => DateTimeOffset.UtcNow.UtcTicks;
    }

    internal abstract unsafe class GetCryptoRand
    {
        public static void Invoke(BurstUlidRandomness* ptr)
        {
            var f = (delegate* unmanaged[Cdecl]<BurstUlidRandomness*, void>)fn_ptr.Data;
            if (f == null) throw new NullReferenceException();
            f(ptr);
        }

        private static readonly SharedStatic<nuint> fn_ptr = SharedStatic<nuint>.GetOrCreate<GetCryptoRand>();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void Delegate(BurstUlidRandomness* ptr);

        public static void Init() => fn_ptr.Data = (nuint)(nint)Marshal.GetFunctionPointerForDelegate((Delegate)Impl);

        [MonoPInvokeCallback(typeof(Delegate))]
        private static void Impl(BurstUlidRandomness* ptr)
            => CryptoRng.rng.Value.GetBytes(new Span<byte>(ptr, sizeof(BurstUlidRandomness)));
    }

    internal static class CryptoRng
    {
        public static readonly ThreadLocal<RNGCryptoServiceProvider> rng = new(() => new());
    }

    #endregion

    #region exception

    public class UlidInvalidFormatException : Exception
    {
        public UlidInvalidFormatException() { }
        public UlidInvalidFormatException(string message) : base(message) { }
        public UlidInvalidFormatException(string message, Exception inner) : base(message, inner) { }
    }

    #endregion

}
