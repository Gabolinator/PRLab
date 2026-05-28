namespace PRLab.API.Dtos.PostDto.Request;

public sealed record DescriptorCreateRequest(
    DescriptionPostDTO? DescriptorPostDto,
    CreateRequest CreateRequest,
    string RequestedBy);


public static class DescriptorCreateRequestExtensions
{
    public static string Print(this DescriptorCreateRequest request)
    {
        if (request == null)
        {
            return "DescriptorCreateRequest <null>";
        }

        var descriptor = request.DescriptorPostDto is null
            ? "null"
            : $"{{DescriptionContent: \"{request.DescriptorPostDto.Content}\", Authority: {request.DescriptorPostDto.Authority}, CreatedBy: {request.DescriptorPostDto.CreatedBy ?? "null"} }}";

        return
            $"DescriptorCreateRequest {{ CreateRequest: {request.CreateRequest}, RequestedBy: \"{request.RequestedBy}\", Descriptor: {descriptor} }}";
    }
    
}
