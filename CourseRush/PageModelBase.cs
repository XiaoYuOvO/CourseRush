namespace CourseRush;

public abstract class PageModelBase
{
    protected readonly IMainWindowModel MainModel;

    protected PageModelBase(IMainWindowModel mainModel)
    {
        MainModel = mainModel;
    }
}