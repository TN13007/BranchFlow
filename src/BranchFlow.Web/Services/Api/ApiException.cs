using System.Net;
using System.Text.Json;

namespace BranchFlow.Web.Services.Api;

public sealed class ApiException(HttpStatusCode statusCode, string message) : Exception(message)
{
    public HttpStatusCode StatusCode { get; } = statusCode;

    public static async Task<ApiException> FromResponseAsync(HttpResponseMessage response, CancellationToken ct)
    {
        string message = response.StatusCode switch
        {
            HttpStatusCode.BadRequest => "Dữ liệu chưa hợp lệ. Vui lòng kiểm tra lại.",
            HttpStatusCode.Unauthorized => "Phiên đăng nhập không còn hợp lệ. Vui lòng đăng nhập lại.",
            HttpStatusCode.Forbidden => "Bạn không có quyền thực hiện thao tác tại chi nhánh này.",
            HttpStatusCode.NotFound => "Không tìm thấy dữ liệu trong phạm vi truy cập của bạn.",
            HttpStatusCode.Conflict => "Dữ liệu đã thay đổi. Vui lòng tải lại và kiểm tra trước khi tiếp tục.",
            HttpStatusCode.ServiceUnavailable => "Dịch vụ tạm thời chưa sẵn sàng. Vui lòng thử kết nối lại sau.",
            HttpStatusCode.BadGateway or HttpStatusCode.GatewayTimeout => "Chưa nhận được phản hồi từ dịch vụ. Vui lòng thử lại sau.",
            HttpStatusCode.TooManyRequests => "Bạn đã thử quá nhiều lần. Vui lòng chờ một phút rồi thử lại.",
            _ => "Dịch vụ đang gặp sự cố. Vui lòng thử lại sau."
        };
        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            try
            {
                using var json = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(ct), cancellationToken: ct);
                if (json.RootElement.TryGetProperty("errors", out var errors) && errors.ValueKind == JsonValueKind.Object)
                {
                    var details = errors.EnumerateObject().SelectMany(p => p.Value.ValueKind == JsonValueKind.Array
                        ? p.Value.EnumerateArray().Where(v => v.ValueKind == JsonValueKind.String).Select(v => v.GetString()!) : [])
                        .Take(5).ToArray();
                    if (details.Length > 0) message = string.Join(" ", details)[..Math.Min(1000, string.Join(" ", details).Length)];
                }
            }
            catch (JsonException) { /* API không trả ProblemDetails hợp lệ: dùng thông báo an toàn. */ }
        }
        return new ApiException(response.StatusCode, message);
    }
}
