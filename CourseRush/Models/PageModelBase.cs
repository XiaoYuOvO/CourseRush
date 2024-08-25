namespace CourseRush.Models;

public abstract class PageModelBase
{
    protected readonly IMainWindowModel MainModel;

    protected PageModelBase(IMainWindowModel mainModel)
    {
        MainModel = mainModel;
    }
}