using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CourseRush.Core;
using HandyControl.Controls;
using HandyControl.Data;
using HandyControl.Interactivity;
using TextBox = HandyControl.Controls.TextBox;

namespace CourseRush.Controls;

public abstract class CourseDetailDrawer : Drawer
{
    public abstract TextBlock DrawerHeader { get; }

    public abstract void SetFontSize(double headerSize, double contentSize);
}

public class CourseDetailDrawer<TCourse> : CourseDetailDrawer where TCourse : ICourse, IPresentedDataProvider<TCourse>
{
    public UniformSpacingPanel ContentPanel { get; } = new()
    {
        Spacing = 16,
        Orientation = Orientation.Vertical,
        Margin = new Thickness(0,10,0,10)
    };
    public sealed override TextBlock DrawerHeader { get; }

    public override void SetFontSize(double headerSize, double contentSize)
    {
        DrawerHeader.FontSize = headerSize;
        ContentPanel.SetValue(Control.FontSizeProperty, contentSize);
    }

    public void ShowCourse(TCourse course)
    {
        IsOpen = true;
        DrawerHeader.Text = string.Format(CourseRush.Language.ui_label_course_detail, course.CourseName);
        ContentPanel.DataContext = course;
    }

    public CourseDetailDrawer()
    {
        ShowMask = true;
        Dock = Dock.Right;
        MaskCanClose = true;
        MaskBrush = FindResource("DarkOpacityBrush") as Brush;
        DrawerHeader = new TextBlock
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            Margin = new Thickness(10,0,0,0),
            Text = "Header",
            Style = FindResource("TextBlockTitleBold") as Style
        };
        Grid.SetRow(DrawerHeader, 0);
        var closeButton = new Button
        {
            Command = ControlCommands.Close,
            HorizontalAlignment = HorizontalAlignment.Right,
            Foreground = FindResource("PrimaryTextBrush") as Brush,
            Style = FindResource("ButtonIcon") as Style
        };
        IconElement.SetGeometry(closeButton, FindResource("DeleteFillCircleGeometry") as Geometry);
        Grid.SetRow(closeButton, 0);
        
        Content = new Border
        {
            Background = FindResource("RegionBrush") as Brush,
            Width = double.NaN,
            BorderThickness = new Thickness(0,1,0,0),
            BorderBrush = FindResource("BorderBrush") as Brush,
            Child = new Grid
            {
                Width = double.NaN,
                RowDefinitions =
                {
                    new RowDefinition
                    {
                        Height = new GridLength(1, GridUnitType.Auto)
                    },
                    new RowDefinition
                    {
                        Height = new GridLength(1, GridUnitType.Star)
                    }
                },
                Children =
                {
                    DrawerHeader, closeButton, ContentPanel
                }
            }
        };
        Grid.SetRow(ContentPanel, 1);
        
        foreach (var presentedData in TCourse.GetPresentedData())
        {
            var textBox = new TextBox
            {
                HorizontalAlignment = HorizontalAlignment.Right,
                TextAlignment = TextAlignment.Justify,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(10,0,10,0),
                IsReadOnly = true,
                Style = FindResource("TextBoxExtend") as Style
            };
            ContentPanel.DataContextChanged += (_, args) => textBox.DataContext = args.NewValue;
            textBox.DataContextChanged += (_, args) =>
            {
                if (args.NewValue is TCourse course)
                    textBox.Text = presentedData.GetValue(course);
            };
            InfoElement.SetIsReadOnly(textBox, true);
            TitleElement.SetTitle(textBox, presentedData.DataTip);
            TitleElement.SetTitlePlacement(textBox, TitlePlacementType.Left);
            TitleElement.SetTitleWidth(textBox, new GridLength(1, GridUnitType.Star));
            ContentPanel.Children.Add(textBox);
        }
    }
}