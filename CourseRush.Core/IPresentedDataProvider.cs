namespace CourseRush.Core;

public interface IPresentedDataProvider<TValue>
{
    static abstract List<PresentedData<TValue>> GetPresentedData();
    static abstract List<PresentedData<TValue>> GetSimplePresentedData();
}