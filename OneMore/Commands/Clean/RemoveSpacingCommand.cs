﻿//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Globalization;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using System.Xml.Linq;


	internal class RemoveSpacingCommand : Command
	{
		private bool spaceBefore;
		private bool spaceAfter;
		private bool spaceBetween;
		private bool includeHeadings;


		public RemoveSpacingCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			using (var dialog = new RemoveSpacingDialog())
			{
				if (dialog.ShowDialog(owner) == DialogResult.OK)
				{
					spaceBefore = dialog.SpaceBefore;
					spaceAfter = dialog.SpaceAfter;
					spaceBetween = dialog.SpaceBetween;
					includeHeadings = dialog.IncludeHeadings;

					await RemoveSpacing();
				}
			}
		}

		private async Task RemoveSpacing()
		{
			using (var one = new OneNote(out var page, out var ns))
			{
				logger.StartClock();

				var elements =
					(from e in page.Root.Descendants(page.Namespace + "OE")
					 where e.Elements().Count() == 1
					 let t = e.Elements().First()
					 where (t != null) && (t.Name.LocalName == "T") &&
						 ((e.Attribute("spaceBefore") != null) ||
						 (e.Attribute("spaceAfter") != null) ||
						 (e.Attribute("spaceBetween") != null))
					 select e)
					.ToList();

				if (elements != null)
				{
					var quickStyles = page.GetQuickStyles()
						.Where(s => s.StyleType == StyleType.Heading);

					var customStyles = new StyleProvider().GetStyles()
						.Where(e => e.StyleType == StyleType.Heading)
						.ToList();

					var modified = false;

					foreach (var element in elements)
					{
						// is this a known Heading style?
						var attr = element.Attribute("quickStyleIndex");
						if (attr != null)
						{
							var index = int.Parse(attr.Value, CultureInfo.InvariantCulture);
							if (quickStyles.Any(s => s.Index == index))
							{
								if (includeHeadings)
								{
									modified |= CleanElement(element);
								}

								continue;
							}
						}

						// is this a custom Heading style?
						var style = new Style(element.CollectStyleProperties(true));
						if (customStyles.Any(s => s.Equals(style)))
						{
							if (includeHeadings)
							{
								modified |= CleanElement(element);
							}

							continue;
						}

						// normal paragraph
						modified |= CleanElement(element);
					}

					logger.WriteTime("removed spacing, now saving...");

					if (modified)
					{
						await one.Update(page);
					}
				}
			}
		}

		private bool CleanElement(XElement element)
		{
			XAttribute attribute;
			bool modified = false;

			if (spaceBefore)
			{
				attribute = element.Attribute("spaceBefore");
				if (attribute != null)
				{
					attribute.Remove();
					modified = true;
				}
			}

			if (spaceAfter)
			{
				attribute = element.Attribute("spaceAfter");
				if (attribute != null)
				{
					attribute.Remove();
					modified = true;
				}
			}

			if (spaceBetween)
			{
				attribute = element.Attribute("spaceBetween");
				if (attribute != null)
				{
					attribute.Remove();
					modified = true;
				}
			}

			return modified;
		}
	}
}
