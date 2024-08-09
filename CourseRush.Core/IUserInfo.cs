namespace CourseRush.Core;

public interface IUserInfo
{
    public string Name { get; set; }
    public AvatarGetter AvatarGetter { get; set; }
    public string ClassName { get; set; }
}