using System;
using System.Windows.Controls;
using CourseRush.Core;

namespace CourseRush;

public class CourseTabItem : TabItem
{
    public CourseDataGrid Grid { get; }
    public ICourseCategory Category { get; }

    public CourseTabItem(CourseDataGrid grid, ICourseCategory category)
    {
        Grid = grid;
        Category = category;
        Content = Grid;
        Header = category.CategoryName;
    }

    public void SubscribeAutoFontResize(Action<AutoFontSizeChanged> subscription)
    {
        Grid.SubscribeAutoFontResize(subscription);
        subscription(factor =>
        {
            FontSize = factor * 14;
        });
    }
}