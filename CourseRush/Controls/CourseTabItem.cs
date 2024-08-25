using System;
using System.Windows.Controls;
using CourseRush.Core;
using CourseRush.Models;

namespace CourseRush.Controls;

public class CourseTabItem : TabItem
{
    public CourseDataPanel Panel { get; }
    public ICourseCategory Category { get; }

    public CourseTabItem(CourseDataPanel panel, ICourseCategory category)
    {
        Panel = panel;
        Category = category;
        Content = Panel;
        Header = category.CategoryName;
    }

    public void SubscribeAutoFontResize(Action<AutoFontSizeChanged> subscription)
    {
        Panel.SubscribeAutoFontResize(subscription);
        subscription(factor =>
        {
            FontSize = factor * 14;
        });
    }
}