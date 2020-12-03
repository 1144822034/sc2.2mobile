using Engine;
using System;

namespace Game
{
    public class ClickTextWidget:CanvasWidget
    {
        public LabelWidget labelWidget;
        public ClickableWidget clickidget;
        public Action click;
        public RectangleWidget rectangleWidget;
        public Color BackGround = Color.Transparent;
        public Color pressColor = Color.Red;
        public ClickTextWidget(Vector2 vector2,string text,Action click,bool box=false) {
            Size = vector2;
            HorizontalAlignment = WidgetAlignment.Center;
            VerticalAlignment = WidgetAlignment.Center;
            labelWidget = new LabelWidget() { Text = text,FontScale= 0.8f,HorizontalAlignment=WidgetAlignment.Center,VerticalAlignment=WidgetAlignment.Center };
            if (click == null)
            {
                Children.Add(labelWidget);
            }
            else {
                clickidget = new ClickableWidget();
                rectangleWidget = new RectangleWidget() { OutlineThickness = 0 };
                if (box)
                {
                    BackGround = Color.Gray;
                    rectangleWidget.FillColor = BackGround;
                    rectangleWidget.OutlineColor = Color.Transparent;
                    rectangleWidget.OutlineThickness = 1;
                }
                Children.Add(rectangleWidget);
                Children.Add(clickidget);
                Children.Add(labelWidget);
                this.click = click;
            }
        }
        public override void Update()
        {
            if (clickidget == null) return;
            if (clickidget.IsClicked) {
                click?.Invoke();
            }
        }
    }
}