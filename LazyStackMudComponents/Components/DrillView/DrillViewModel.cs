using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LzMudComponents
{
    public class DrillViewModel
    {
        // Menu Item Key
        string? Key { get; set; }
        // Menu Item Label
        string? MenuEntry { get; set; }
        // Is this Menu item the default item?
        bool? IsMenuEntryDefault { get; set; }
        // Extended title shown when menu item selected (optional)
        string? Title { get; set; }
        // Extended description shown when menu item selected (optional)
        bool? HasItemsMenu { get; set; }
        // Menu component template to use - optional (default will be provided)
        string? MenuTemplate { get; set; }
        // Default Items template to use (can be overridden by individual item)
        // Component template used to display item(s) - optional
        string? ItemsTemplate { get; set; }
        // Items - null if no sub items
        List<DrillView>? Items { get; set; }
        string? Intro { get; set; }
        string? ItemsKeyName { get; set; }
        // Component template to use - ovrrides ItemsTemplate. Optional if ItemsTemplate is specified.
        string? Template { get; set; }
        // Component display elements
        // The idea here is that various component templates may provide 
        // different layouts of one or more of these elements. Generally, there 
        // is display component associated with each display element type but
        // that is a component template implementation detail.This prescriptive 
        // approach to content elements is pursued for security reasons. 
        string? P1 { get; set; }
        string? P2 { get; set; }
        string? P3 { get; set; }
        string? P4 { get; set; }
        string? P5 { get; set; }
        string? WebsiteLabel { get; set; }
        string? Website { get; set; }
        string? Tags { get; set; }
        string? Website2Label { get; set; }
        string? WebSite2 { get; set; }
        string? Map { get; set; }
        string? Contact { get; set; }
        string? Email { get; set; }
        string? Subject { get; set; }
        string? Body { get; set; }
        string? Distance { get; set; }
        string? Nav { get; set; }

        // Currently Unused 
        string? To { get; set; }
        string? ItemsFile { get; set; }


    }

}
