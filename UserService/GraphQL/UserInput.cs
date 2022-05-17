namespace UserService.GraphQL
{
    public record UserInput
    (
        int? Id,
        string FullName,
        string Email,
        string Username,
        string Password
    );
}
