using pdfforge.DataStorage.Storage;
using pdfforge.DataStorage;
using System.Collections.Generic;
using System.Text;
using System;

// ! This file is generated automatically.
// ! Do not edit it outside the sections for custom code.
// ! These changes will be deleted during the next generation run

namespace pdfforge.PDFCreator.Conversion.Settings
{
	/// <summary>
	/// Appends one or more pages at the end of the converted document
	/// </summary>
	public class AttachmentPage {
		
		/// <summary>
		/// Enables the AttachmentPage action
		/// </summary>
		public bool Enabled { get; set; }
		
		/// <summary>
		/// Filename of the PDF that will be appended
		/// </summary>
		public string File { get; set; }
		
		
		private void Init() {
			Enabled = false;
			File = "";
		}
		
		public AttachmentPage()
		{
			Init();
		}
		
		public void ReadValues(Data data, string path)
		{
			try { Enabled = bool.Parse(data.GetValue(@"" + path + @"Enabled")); } catch { Enabled = false;}
			try { File = Data.UnescapeString(data.GetValue(@"" + path + @"File")); } catch { File = "";}
		}
		
		public void StoreValues(Data data, string path)
		{
			data.SetValue(@"" + path + @"Enabled", Enabled.ToString());
			data.SetValue(@"" + path + @"File", Data.EscapeString(File));
		}
		
		public AttachmentPage Copy()
		{
			AttachmentPage copy = new AttachmentPage();
			
			copy.Enabled = Enabled;
			copy.File = File;
			
			return copy;
		}
		
		public override bool Equals(object o)
		{
			if (!(o is AttachmentPage)) return false;
			AttachmentPage v = o as AttachmentPage;
			
			if (!Enabled.Equals(v.Enabled)) return false;
			if (!File.Equals(v.File)) return false;
			
			return true;
		}
		
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			
			sb.AppendLine("Enabled=" + Enabled.ToString());
			sb.AppendLine("File=" + File.ToString());
			
			return sb.ToString();
		}
		
		public override int GetHashCode()
		{
			// ReSharper disable once BaseObjectGetHashCodeCallInGetHashCode
			return base.GetHashCode();
		}
		
// Custom Code starts here
// START_CUSTOM_SECTION:GENERAL

// END_CUSTOM_SECTION:GENERAL
// Custom Code ends here. Do not edit below
		
	}
}
