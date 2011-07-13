using System;
using MonoMac.AppKit;
using Eto.Forms;
using MonoMac.ObjCRuntime;
using MonoMac.Foundation;
using Eto.Drawing;

namespace Eto.Platform.Mac.Forms
{
	interface IColorDialogHandler
	{
		Color Color { get; set; }
		ColorDialog Widget { get; }
	}
	
	class ColorHandler : NSObject
	{
		public static ColorHandler Instance { get; set; }
		public IColorDialogHandler Handler { get; set; }
		
		[Export("selected:")]
		public void selected(NSColorPanel panel)
		{
			Handler.Color = Generator.Convert(panel.Color);
			Handler.Widget.OnColorChanged(EventArgs.Empty);
		}
	}
	
	public class ColorDialogHandler : WidgetHandler<NSColorPanel, ColorDialog>, IColorDialog, IColorDialogHandler
	{
		
		public ColorDialogHandler()
		{
			Control = NSColorPanel.SharedColorPanel;
		}
		
		public Color Color { get; set; }

		#region IDialog implementation
		
		public DialogResult ShowDialog (Window parent)
		{
			if (ColorHandler.Instance == null) ColorHandler.Instance = new ColorHandler();
			ColorHandler.Instance.Handler = this;
			Control.Color = Generator.ConvertNS (this.Color);
			
			Control.SetTarget (ColorHandler.Instance);
			Control.SetAction (new Selector("selected:"));
			
			NSApplication.SharedApplication.OrderFrontColorPanel (ColorHandler.Instance);
			
			
			return DialogResult.None;
		}
		
		#endregion
	}
}

