namespace UserService.QraphQL
{
    public record UserToken
     (
         string? Token,
         string? Expired,
         string? Message
     );
}
