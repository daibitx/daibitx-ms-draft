using Daibitx.Grpc.Contracts;
using Daibitx.Grpc.Server.Abstraction;
using Daibitx.Grpc.Server.Exceptions;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Text.Json;
using ValidationException = Daibitx.Grpc.Server.Exceptions.ValidationException;

namespace Daibitx.Grpc.Server
{
    public abstract class GrpcServiceBase : IAsyncDisposable
    {
        protected readonly ILogger Logger;
        protected readonly JsonSerializerOptions JsonOptions;
        protected readonly ActivitySource ActivitySource;

        protected GrpcServiceBase(
            ILogger logger,
            JsonSerializerOptionsProvider optionsProvider)
        {
            Logger = logger;
            JsonOptions = optionsProvider.DefaultOptions;
            ActivitySource = new ActivitySource(GetType().Name);
        }

        /// <summary>
        /// 解析请求数据为强类型
        /// </summary>
        protected TRequest? ParseRequest<TRequest>(string data)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(data)) return default;
                return JsonSerializer.Deserialize<TRequest>(data, JsonOptions);
            }
            catch (JsonException ex)
            {
                Logger.LogWarning(ex, "Failed to deserialize request data");
                return default;
            }
        }

        /// <summary>
        /// 序列化响应数据
        /// </summary>
        protected string SerializeResponse<TResponse>(TResponse data)
        {
            try
            {
                return JsonSerializer.Serialize(data, JsonOptions);
            }
            catch (JsonException ex)
            {
                Logger.LogError(ex, "Failed to serialize response data");
                return "{}";
            }
        }

        /// <summary>
        /// 从元数据中提取跟踪信息
        /// </summary>
        protected Dictionary<string, string> ExtractTracingMetadata(ServerCallContext context)
        {
            var metadata = new Dictionary<string, string>();

            // 提取常见跟踪头
            var traceHeaders = new[] { "x-request-id", "x-correlation-id", "x-trace-id" };
            foreach (var header in traceHeaders)
            {
                var value = context.RequestHeaders.GetValue(header);
                if (!string.IsNullOrEmpty(value))
                {
                    metadata[header] = value;
                }
            }

            return metadata;
        }

        /// <summary>
        /// 构建成功的标准响应
        /// </summary>
        protected StandardResponse SuccessResponse(
            object? data = null,
            string message = "Success",
            Dictionary<string, string>? metadata = null)
        {
            return new StandardResponse
            {
                Success = true,
                Message = message,
                Data = data != null ? SerializeResponse(data) : "{}",
                ErrorCode = 0,
                Metadata = { metadata ?? new Dictionary<string, string>() }
            };
        }

        /// <summary>
        /// 构建失败的标准响应
        /// </summary>
        protected StandardResponse FailureResponse(
            string message,
            int errorCode = 400,
            object? details = null)
        {
            return new StandardResponse
            {
                Success = false,
                Message = message,
                Data = details != null ? SerializeResponse(details) : "{}",
                ErrorCode = errorCode
            };
        }

        /// <summary>
        /// 处理业务异常并转换为标准响应
        /// </summary>
        protected async Task<StandardResponse> ExecuteWithErrorHandlingAsync<T>(
            Func<Task<T>> func,
            string operationName,
            CancellationToken cancellationToken = default)
        {
            using var activity = ActivitySource.StartActivity(operationName);

            try
            {
                var result = await func();
                return SuccessResponse(result, $"{operationName} completed successfully");
            }
            catch (BusinessException ex)
            {
                Logger.LogWarning(ex, "Business error in {Operation}", operationName);
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                return FailureResponse(ex.Message, ex.ErrorCode, ex.Details);
            }
            catch (ValidationException ex)
            {
                Logger.LogWarning(ex, "Validation error in {Operation}", operationName);
                activity?.SetStatus(ActivityStatusCode.Error, "Validation failed");
                return FailureResponse("Validation failed", 422, ex.Errors);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Unexpected error in {Operation}", operationName);
                activity?.SetStatus(ActivityStatusCode.Error, "Internal server error");
                return FailureResponse("Internal server error", 500, new { errorId = Guid.NewGuid() });
            }
        }

        /// <summary>
        /// 验证请求
        /// </summary>
        protected void ValidateRequest<T>(T request)
        {
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(request);

            if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
            {
                throw new ValidationException(validationResults);
            }
        }

        public virtual ValueTask DisposeAsync()
        {
            ActivitySource.Dispose();
            return ValueTask.CompletedTask;
        }
    }

}
