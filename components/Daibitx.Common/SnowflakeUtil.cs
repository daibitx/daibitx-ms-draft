using System;

namespace Daibitx.Common
{
    /// <summary>
    /// 雪花id生成
    /// 1位符号 + 41位时间戳 + 10位机器ID + 12位序列号
    /// </summary>
    public class SnowflakeIdGenerator
    {
        private const long Twepoch = 1672531200000L; // 自定义起始时间（毫秒）
        private const int WorkerIdBits = 5;
        private const int DatacenterIdBits = 5;
        private const int SequenceBits = 12;

        private const long MaxWorkerId = -1L ^ (-1L << WorkerIdBits);
        private const long MaxDatacenterId = -1L ^ (-1L << DatacenterIdBits);

        private const int WorkerIdShift = SequenceBits;
        private const int DatacenterIdShift = SequenceBits + WorkerIdBits;
        private const int TimestampLeftShift = SequenceBits + WorkerIdBits + DatacenterIdBits;
        private const long SequenceMask = -1L ^ (-1L << SequenceBits);

        private readonly long _workerId;
        private readonly long _datacenterId;

        private long _lastTimestamp = -1L;
        private long _sequence = 0L;


        public SnowflakeIdGenerator(long workerId, long datacenterId)
        {
            if (workerId > MaxWorkerId || workerId < 0) throw new ArgumentException("workerId invalid");
            if (datacenterId > MaxDatacenterId || datacenterId < 0) throw new ArgumentException("datacenterId invalid");

            _workerId = workerId;
            _datacenterId = datacenterId;
        }

        public long NextId()
        {
            while (true)
            {
                long timestamp = TimeGen();
                long lastTimestamp = Interlocked.Read(ref _lastTimestamp);
                long sequence = Interlocked.Read(ref _sequence);

                if (timestamp < lastTimestamp)
                {
                    timestamp = TilNextMillis(lastTimestamp);
                }

                if (timestamp == lastTimestamp)
                {
                    sequence = (sequence + 1) & SequenceMask;
                    if (sequence == 0)
                    {
                        timestamp = TilNextMillis(lastTimestamp);
                    }
                }
                else
                {
                    sequence = 0L;
                }

                if (Interlocked.CompareExchange(ref _sequence, sequence, Interlocked.Read(ref _sequence)) == sequence - 1 ||
                    Interlocked.CompareExchange(ref _lastTimestamp, timestamp, lastTimestamp) == lastTimestamp)
                {
                    return ((timestamp - Twepoch) << TimestampLeftShift) |
                           (_datacenterId << DatacenterIdShift) |
                           (_workerId << WorkerIdShift) |
                           sequence;
                }
            }
        }

        private long TilNextMillis(long lastTimestamp)
        {
            long timestamp = TimeGen();
            while (timestamp <= lastTimestamp)
            {
                timestamp = TimeGen();
            }
            return timestamp;
        }

        private long TimeGen()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }
    }
}
