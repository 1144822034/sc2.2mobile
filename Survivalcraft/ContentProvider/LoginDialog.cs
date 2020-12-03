using System;
using System.Collections.Generic;
using System.IO;
using Engine;

namespace Game
{
    public class LoginDialog:Dialog
    {
        public Action<byte[]> succ;
        public Action<Exception> fail;
        public StackPanelWidget MainView;
        public BevelledButtonWidget btna,btnb,btnc;
        public TextBoxWidget txa, txb;
        public LabelWidget tip = new LabelWidget() { HorizontalAlignment = WidgetAlignment.Near, VerticalAlignment = WidgetAlignment.Near, Margin = new Vector2(1f, 1f) };
        public LoginDialog() {
            CanvasWidget canvasWidget = new CanvasWidget() {Size=new Vector2(600f,240f),HorizontalAlignment=WidgetAlignment.Center,VerticalAlignment=WidgetAlignment.Center };
            RectangleWidget rectangleWidget = new RectangleWidget() { FillColor=new Color(0,0,0,255),OutlineColor=new Color(128,128,128,128),OutlineThickness=2};
            StackPanelWidget stackPanelWidget = new StackPanelWidget() { Direction=LayoutDirection.Vertical,HorizontalAlignment=WidgetAlignment.Center,VerticalAlignment=WidgetAlignment.Near,Margin=new Vector2(10f,10f)};
            Children.Add(canvasWidget);
            canvasWidget.Children.Add(rectangleWidget);
            canvasWidget.Children.Add(stackPanelWidget);
            MainView = stackPanelWidget;
            MainView.Children.Add(tip);
            MainView.Children.Add(makeTextBox("用户名:"));
            MainView.Children.Add(makeTextBox("密  码:"));
            MainView.Children.Add(makeButton());

        }
        public Widget makeTextBox(string title) {
            CanvasWidget canvasWidget = new CanvasWidget() { Margin=new Vector2(10,0)};
            RectangleWidget rectangleWidget = new RectangleWidget() { FillColor=Color.Black,OutlineColor=Color.White,Size=new Vector2(float.PositiveInfinity,80)};
            StackPanelWidget stack = new StackPanelWidget() { Direction=LayoutDirection.Horizontal};
            LabelWidget label = new LabelWidget() { HorizontalAlignment=WidgetAlignment.Near,VerticalAlignment=WidgetAlignment.Near,Text=title,Margin=new Vector2(1f,1f)};
            TextBoxWidget textBox = new TextBoxWidget() {VerticalAlignment=WidgetAlignment.Center,HorizontalAlignment=WidgetAlignment.Stretch,Color=new Color(255,255,255),Margin=new Vector2(4f,0f), Size = new Vector2(float.PositiveInfinity, 80) };
            if (title == "用户名:") txa = textBox;
            if (title == "密  码:") txb = textBox;
            stack.Children.Add(label);
            stack.Children.Add(textBox);
            canvasWidget.Children.Add(rectangleWidget);
            canvasWidget.Children.Add(stack);
            return canvasWidget;        
        }
        public Widget makeButton() {
            StackPanelWidget stack = new StackPanelWidget() { Direction=LayoutDirection.Horizontal};
            BevelledButtonWidget bevelledButtonWidget1 = new BevelledButtonWidget() { Size=new Vector2(160,60),Margin=new Vector2(4f,0),Text="登陆"};
            BevelledButtonWidget bevelledButtonWidget2 = new BevelledButtonWidget() { Size = new Vector2(160, 60), Margin = new Vector2(4f, 0), Text = "注册" };
            BevelledButtonWidget bevelledButtonWidget3 = new BevelledButtonWidget() { Size = new Vector2(160, 60), Margin = new Vector2(4f, 0), Text = "取消" };
            stack.Children.Add(bevelledButtonWidget1);
            stack.Children.Add(bevelledButtonWidget2);
            stack.Children.Add(bevelledButtonWidget3);
            btna=bevelledButtonWidget1;
            btnb=bevelledButtonWidget2;
            btnc=bevelledButtonWidget3;
            return stack;
        }
        public override void Update()
        {
            if (btna.IsClicked) {
                Dictionary<string,string> par= new Dictionary<string, string>();
                par.Add("user",txa.Text);
                par.Add("pass",txb.Text);
                WebManager.Post(SPMBoxExternalContentProvider.m_redirectUri+"/com/api/login",par,null,new MemoryStream(),new CancellableProgress(),succ,fail);
            }
            if (btnb.IsClicked) {
                WebBrowserManager.LaunchBrowser(SPMBoxExternalContentProvider.m_redirectUri + "/com/reg");
            }
            if (btnc.IsClicked) {
                DialogsManager.HideDialog(this);
            }

        }
    }
}