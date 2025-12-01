using Daibitx.Grpc.Client.Exceptions;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Daibitx.Grpc.Client
{
    public abstract class GrpcClientBase : IAsyncDisposable
    {
        protected readonly CallInvoker CallInvoker;
        protected readonly JsonSerializerOptions JsonOptions;
        protected readonly ILogger Logger;
        protected readonly string ServiceName;

        protected GrpcClientBase(
            CallInvoker callInvoker,
            ILogger logger,
            JsonSerializerOptions jsonOptions,
            string serviceName)
        {
            CallInvoker = callInvoker;
            Logger = logger;
            JsonOptions = jsonOptions;
            ServiceName = serviceName;
        }

        /// <summary>
        /// 创建带元数据的调用上下文
        /// </summary>
        protected Metadata CreateMetadata(Dictionary<string, string>? customHeaders = null)
        {
            var metadata = new Metadata();

            // 添加追踪信息
            var traceId = Activity.Current?.Id ?? Guid.NewGuid().ToString();
            metadata.Add("x-trace-id", traceId);
            metadata.Add("x-request-id", Guid.NewGuid().ToString());

            // 添加自定义头
            if (customHeaders != null)
            {
                foreach (var (key, value) in customHeaders)
                {
                    metadata.Add(key, value);
                }
            }

            return metadata;
        }

        /// <summary>
        /// 执行带重试策略的调用
        /// </summary>
        protected async Task<TResponse> ExecuteWithRetryAsync<TResponse>(
            Func<CallOptions, Task<TResponse>> callFunc,
            CallOptions options,
            int maxRetries = 3)
        {
            var attempt = 0;
            var delays = new[] { 100, 250, 500 }; // 毫秒

            while (true)
            {
                try
                {
                    attempt++;
                    return await callFunc(options);
                }
                catch (RpcException ex) when (IsRetryableStatus(ex.StatusCode) && attempt <= maxRetries)
                {
                    if (attempt > maxRetries) throw;

                    var delay = delays[Math.Min(attempt - 1, delays.Length - 1)];
                    Logger.LogWarning(ex, "gRPC call failed (attempt {Attempt}), retrying in {Delay}ms",
                        attempt, delay);

                    await Task.Delay(delay);
                }
                catch (RpcException ex) when (ex.StatusCode == StatusCode.Unavailable)
                {
                    // 处理连接问题
                    Logger.LogError(ex, "gRPC service unavailable");
                    throw new ServiceUnavailableException(ServiceName, ex);
                }
            }
        }

        /// <summary>
        /// 安全的反序列化
        /// </summary>
        protected T? Deserialize<T>(string data)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(data)) return default;
                return JsonSerializer.Deserialize<T>(data, JsonOptions);
            }
            catch (JsonException ex)
            {
                Logger.LogError(ex, "Failed to deserialize response");
                throw new InvalidDataException("Invalid response format", ex);
            }
        }

        private static bool IsRetryableStatus(StatusCode code) => code switch
        {
            StatusCode.DeadlineExceeded => true,
            StatusCode.Unavailable => true,
            StatusCode.ResourceExhausted => true,
            StatusCode.Aborted => true,
            _ => false
        };

        protected CallOptions CreateCallOptions(
            CancellationToken cancellationToken = default,
            Dictionary<string, string>? headers = null,
            TimeSpan? timeout = null)
        {
            var metadata = CreateMetadata(headers);
            return new CallOptions(
                headers: metadata,
                cancellationToken: cancellationToken,
                deadline: timeout.HasValue ? DateTime.UtcNow.Add(timeout.Value) : default,
                writeOptions: new WriteOptions(WriteFlags.NoCompress)
            );
        }

        public virtual ValueTask DisposeAsync()
        {
            return ValueTask.CompletedTask;
        }
    }

}
