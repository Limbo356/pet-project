using DbContextUser;

class CurrentUser
{
    public async Task<IResult> GetRoleUser(User user)
    {
        if (user == null)
        {
            return Results.Ok(new
            {
                isAuth = false,
                role = "Guest",
                name = "Гость",
                email = ""
            });
        }

        return Results.Ok(new
        {
            isAuth = true,
            role = user.Role.ToString(),
            name = user.Profile!.Name,
            email = user.UserLogin?.EmailUser
        });
    }
}