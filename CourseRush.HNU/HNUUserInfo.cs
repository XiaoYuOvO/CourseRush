using CourseRush.Core;

namespace CourseRush.HNU;

public class HNUUserInfo : IUserInfo
{
    public HNUUserInfo(string name, AvatarGetter avatarGetter, string className)
    {
        Name = name;
        AvatarGetter = avatarGetter;
        ClassName = className;
    }

    public string Name { get; set; }
    public AvatarGetter AvatarGetter { get; set; }
    public string ClassName { get; set; }
}