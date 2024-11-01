using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace BouncingBall.Views;

public partial class Accordion : ContentView
{
    public static readonly BindableProperty IndicatorViewProperty = BindableProperty.Create(nameof(IndicatorView), typeof(View), typeof(Accordion), default(View));
    public View IndicatorView
    {
        get => (View)GetValue(IndicatorViewProperty);
        set => SetValue(IndicatorViewProperty, value);
    }

    public static readonly BindableProperty ContentViewProperty = BindableProperty.Create(nameof(AccordionContentView), typeof(View), typeof(Accordion), default(View));
    public View AccordionContentView
    {
        get => (View)GetValue(ContentViewProperty);
        set => SetValue(ContentViewProperty, value);
    }

    public static readonly BindableProperty TitleProperty = BindableProperty.Create(nameof(Title), typeof(string), typeof(Accordion), default(string));
    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly BindableProperty IsOpenBindablePropertyProperty = BindableProperty.Create(nameof(IsOpen), typeof(bool), typeof(Accordion), false, propertyChanged: IsOpenChanged);
    public bool IsOpen
    {
        get { return (bool)GetValue(IsOpenBindablePropertyProperty); }
        set { SetValue(IsOpenBindablePropertyProperty, value); }
    }

    public static readonly BindableProperty HeaderBackgroundColorProperty = BindableProperty.Create(nameof(HeaderBackgroundColor), typeof(Color), typeof(Accordion), Colors.Transparent);
    public Color HeaderBackgroundColor
    {
        get { return (Color)GetValue(HeaderBackgroundColorProperty); }
        set { SetValue(HeaderBackgroundColorProperty, value); }
    }

    private static void IsOpenChanged(BindableObject bindable, object oldValue, object newValue)
    {
        bool isOpen;

        if (bindable != null && newValue != null)
        {
            var control = (Accordion)bindable;
            isOpen = (bool)newValue;

            if (control.IsOpen == false)
            {
                VisualStateManager.GoToState(control, "Open");
                control.Close();
            }
            else
            {
                VisualStateManager.GoToState(control, "Closed");
                control.Open();
            }
        }
    }

    public uint AnimationDuration { get; set; }

    public Accordion()
    {
        InitializeComponent();
        AnimationDuration = 250;
        IsOpen = false;
        Loaded += (sender, args) => Close();
    }

    async void Close()
    {
        AccordionContent.IsVisible = false;
        await Task.WhenAll(
            AccordionContent.TranslateTo(0, -10, AnimationDuration),
            IndicatorContainer.RotateTo(-180, AnimationDuration),
            AccordionContent.FadeTo(0, 50)
        );
    }

    async void Open()
    {
        AccordionContent.IsVisible = true;
        await Task.WhenAll(
            AccordionContent.TranslateTo(0, 10, AnimationDuration),
            IndicatorContainer.RotateTo(0, AnimationDuration),
            AccordionContent.FadeTo(30, 50, Easing.SinIn)
        );
    }

    private void TitleTapped(object sender, EventArgs e)
    {
        IsOpen = !IsOpen;
    }
}