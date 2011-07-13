using System;
using Eto.Forms;
using MonoMac.AppKit;
using MonoMac.Foundation;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Eto.Platform.Mac.Forms
{
    interface IMacFileDialog
    {
    	List<string> MacFilters { get; }
        string[] Filters { get; }
		string GetDefaultExtension();
		int CurrentFilterIndex { get; }
    }
 
    class SavePanelDelegate : NSOpenSavePanelDelegate
    {
        public IMacFileDialog Handler { get; set; }
     
        public override bool ShouldEnableUrl (NSSavePanel panel, NSUrl url)
        {
            if (Handler.Filters == null)
                return true;
            if (Directory.Exists (url.Path))
                return true;

			var extension = Path.GetExtension (url.Path).ToLowerInvariant ().TrimStart ('.');
            if (Handler.MacFilters == null || Handler.MacFilters.Contains (extension, StringComparer.InvariantCultureIgnoreCase))
                return true;
            else
                return false;
        }
		
    }
	
    public abstract class MacFileDialog<T, W> : WidgetHandler<T, W>, IFileDialog, IMacFileDialog
     where T: NSSavePanel
     where W: FileDialog
    {
        string[] filters;
		List<string> macfilters;
		NSPopUpButton fileTypes;
		List<string> titles;

        public MacFileDialog ()
        {
			fileTypes = new NSPopUpButton();
        }
		
		protected void CreateControl()
		{
			if (Control.AccessoryView != null) return;

			var fileTypeView = new NSView();
			fileTypeView.AutoresizingMask = NSViewResizingMask.HeightSizable | NSViewResizingMask.WidthSizable;
			
			int padding = 15;
			
			var label = new NSTextField();
			label.StringValue = "Format";
			label.DrawsBackground = false;
			label.Bordered = false;
			label.Bezeled = false;
			label.Editable = false;
			label.Selectable = false;
			label.SizeToFit();
			fileTypeView.AddSubview(label);
			
			fileTypes.SizeToFit();
			fileTypes.Activated += (sender, e) => {
				SetFilters ();
				Control.ValidateVisibleColumns ();// SetFilters ();
				Control.Update ();
			};
			fileTypeView.AddSubview(fileTypes);
			fileTypes.SetFrameOrigin(new System.Drawing.PointF(label.Frame.Width + 10, padding));

			label.SetFrameOrigin(new System.Drawing.PointF(0, padding + (fileTypes.Frame.Height - label.Frame.Height) / 2));
			
			fileTypeView.Frame = new System.Drawing.RectangleF(0, 0, fileTypes.Frame.Width + label.Frame.Width + 10, fileTypes.Frame.Height + padding*2);
			
			Control.AccessoryView = fileTypeView;
		}
     
        public virtual string FileName {
            get { 
                return Control.Url.Path;
            }
            set {  }
        }
		
		public Uri Directory {
			get {
				return new Uri(Control.DirectoryUrl.AbsoluteString);
			}
			set {
				Control.DirectoryUrl = new NSUrl(value.AbsoluteUri);
			}
		}
		
		public string GetDefaultExtension ()
		{
			if (CurrentFilterIndex >= 0)
			{
				var filter = filters[CurrentFilterIndex];
	            string[] filtervals = filter.Split ('|');
	            string[] filterexts = filtervals [1].Split (';');
				string ext = filterexts.FirstOrDefault();
				if (!string.IsNullOrEmpty(ext))
				{
					return ext.TrimStart('*', '.');
				}
			}
			return null;
		}
		
		void SetFilters()
		{
            macfilters = new List<string> ();
			var filter = filters[this.CurrentFilterIndex];
            //foreach (var filter in filters) {
                string[] filtervals = filter.Split ('|');
                string[] filterexts = filtervals [1].Split (';');
                foreach (var filterext in filterexts) {
                    macfilters.Add (filterext.TrimStart ('*', '.'));
                }
            //}
            Control.AllowedFileTypes = macfilters.Distinct ().ToArray ();
		}
		
		public List<string> MacFilters
		{
			get { return macfilters; }
		}

        public string[] Filters {
            get { return filters; }
            set { 
                filters = value;
             	titles = new List<string>();
				fileTypes.RemoveAllItems();
                foreach (var filter in filters) {
                string[] filtervals = filter.Split ('|');
					titles.Add(filtervals[0]);
                }
				fileTypes.AddItems(titles.ToArray());
				
				SetFilters ();
            }
        }

        public int CurrentFilterIndex {
            get { 
				var title = fileTypes.TitleOfSelectedItem;
                return titles.IndexOf(title);
			}
            set { 
				fileTypes.SelectItem(filters[value]);
			}
        }

        public bool CheckFileExists {
            get { return false; }
            set {  }
        }

        public string Title {
            get { return Control.Title; }
            set { Control.Title = value; }
        }
     
        public Eto.Forms.DialogResult ShowDialog (Window parent)
        {
            //Control.AllowsOtherFileTypes = false;
            Control.Delegate = new SavePanelDelegate{ Handler = this };
			CreateControl();

			
            int ret = MacModal.Run(Control, parent);
            
            return ret == 1 ? DialogResult.Ok : DialogResult.Cancel;
        }
		
		protected override void Dispose (bool disposing)
		{
			//base.Dispose (disposing);
		}

    }
}
