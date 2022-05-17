namespace UserService.GraphQL
{
    public record ProfilesInput
    (
        int? Id,
        int UserId,
        string Name,
        string Address,
        string City,
        string Phone
    );
}
