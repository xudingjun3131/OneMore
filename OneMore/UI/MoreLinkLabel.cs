//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.UI
{
	using System;
	using System.Windows.Forms;


	/// <summary>
	/// Extension of LinkLabel to force system Hand cursor instead of default Forms Hand cursor.
	/// </summary>
	internal class MoreLinkLabel : LinkLabel
	{
		private readonly IntPtr hcursor;


		public MoreLinkLabel()
		{
			Cursor = Cursors.Hand;
			hcursor = Native.LoadCursor(IntPtr.Zero, Native.IDC_HAND);
		}


		protected override void WndProc(ref Message msg)
		{
			if (msg.Msg == Native.WM_SETCURSOR && hcursor != IntPtr.Zero)
			{
				Native.SetCursor(hcursor);
				msg.Result = IntPtr.Zero; // indicate handled
				return;
			}

			base.WndProc(ref msg);
		}
	}
}
