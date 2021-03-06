//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Colorizer;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;


	internal class ColorizeCommand : Command
	{
		public ColorizeCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			using (var one = new OneNote(out var page, out var ns))
			{
				var dark = page.GetPageColor(out _, out _).GetBrightness() < 0.5;

				var colorizer = new Colorizer(args[0] as string, dark ? "dark" : "light");

				var runs = page.Root.Descendants(ns + "T")
					.Where(e => e.Attributes().Any(a => a.Name == "selected" && a.Value == "all"));

				if (runs == null)
				{
					return;
				}

				var updated = false;

				foreach (var run in runs.ToList())
				{
					run.SetAttributeValue("lang", "yo");

					var cdata = run.GetCData();

					if (cdata.Value.Contains("<br>"))
					{
						// special handling to expand soft line breaks (Shift + Enter) into
						// hard breaks, splitting the line into multiple ines...

						var text = cdata.Value;
						var lines = text.Split(new string[] { "<br>" }, StringSplitOptions.None);

						// update current cdata with first line
						cdata.Value = colorizer.ColorizeOne(
							new XCData(lines[0]).GetWrapper().Value);

						// collect subsequent lines from soft-breaks
						var elements = new List<XElement>();
						for (int i = 1; i < lines.Length; i++)
						{
							var colorized = colorizer.ColorizeOne(
								new XCData(lines[i]).GetWrapper().Value);

							elements.Add(new XElement(ns + "OE",
								run.Parent.Attributes(),
								new XElement(ns + "T", new XCData(colorized)
								)));
						}

						run.Parent.AddAfterSelf(elements);

						updated = true;
					}
					else
					{
						cdata.Value = colorizer.ColorizeOne(cdata.GetWrapper().Value);
						updated = true;
					}
				}

				if (updated)
				{
					await one.Update(page);
				}
			}
		}
	}
}
/*
<one:T><![CDATA[&lt;?xml version=&quot;1.0&quot;?&gt; <br>
&lt;hello xmlns=&quot;urn:ietf:params:xml:ns:netconf:base:1.0&quot;&gt; <br>
  &lt;capabilities&gt;   <br>
    &lt;capability&gt;urn:ietf:params:xml:ns:netconf:base:1.0&lt;/capability&gt;   <br>
  &lt;/capabilities&gt;   <br>
&lt;/hello&gt;]]&gt;]]&gt;]]></one:T>
*/