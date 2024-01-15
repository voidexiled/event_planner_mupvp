public class User
{
    // TODO: public string CharacterName { get; set; } = "";
    public string Key { get; internal set; } = "";
    public UserTypes UserType { get; internal set; }

    public string GetRankAsString()
    {
        return UserType switch
        {
            UserTypes.USUARIO => "Usuario",
            UserTypes.VIP0 => "Premium Bronze",
            UserTypes.VIP1 => "Premium Silver",
            UserTypes.VIP2 => "Premium Oro",
            UserTypes.TUTOR => "Tutor",
            UserTypes.GAMEMANAGER => "Game Manager",
            UserTypes.COMMUNITYMANAGER => "Community Manager",
            UserTypes.ADMINISTRADOR => "Administrador",
            UserTypes.CREADOR => "Creador",
            _ => "Usuario"
        };
    }

}