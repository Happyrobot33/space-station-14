using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Content.Client.Administration.Managers;
using Content.Client.Eui;
using Content.Client.Stylesheets;
using Content.Client.UserInterface.Controls;
using Content.Shared.Administration;
using Content.Shared.Eui;
using JetBrains.Annotations;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;
using Robust.Shared.IoC;
using Robust.Shared.Localization;
using Robust.Shared.Maths;
using Robust.Shared.Utility;
using static Content.Shared.Administration.AutoModEuiState;
using static Content.Shared.Administration.PermissionsEuiMsg;
using static Robust.Client.UserInterface.Controls.BoxContainer;

namespace Content.Client.Administration.UI
{
    [UsedImplicitly]
    public sealed class AutoModEui : BaseEui
    {
        private readonly Menu _menu;
        public AutoModEui()
        {
            IoCManager.InjectDependencies(this);

            _menu = new Menu(this);
        }
        public override void Closed()
        {
            base.Closed();

            SendMessage(new CloseEuiMessage());
            _menu.Close();
        }
        public override void Opened()
        {
            _menu.OpenCentered();
        }

        public override void HandleState(EuiStateBase state)
        {
            var s = (AutoModEuiState)state;

            var data = s.Rules.Select(rule => new AutoModListData(rule)).ToList();
            
            _menu.RulesList.GenerateItem = GenerateItem;
            _menu.RulesList.PopulateList(data);
        }

        private void GenerateItem(ListData data, ListContainerButton button)
        {
            var rule = (AutoModListData)data;
            
            var box = new BoxContainer() { Orientation = LayoutOrientation.Horizontal, HorizontalExpand = true };
            //text entry
            var regex = new LineEdit()
            {
                Text = rule.rule.Message ?? string.Empty,
                HorizontalExpand = true,
                VerticalExpand = true,
            };

            box.AddChild(regex);
            button.AddChild(box);
        }

        private sealed class Menu : DefaultWindow
        {
            private readonly AutoModEui _ui;
            public ListContainer RulesList { get; }
            public Menu(AutoModEui ui)
            {
                _ui = ui;
                Title = Loc.GetString("auto-mod-eui-menu-title");

                var tab = new TabContainer();
                RulesList = new ListContainer
                {
                    HorizontalExpand = true,
                    VerticalExpand = true,
                };
                var rulesVBox = new BoxContainer
                {
                    Orientation = LayoutOrientation.Vertical,
                    Children = {
                        RulesList
                    }
                };
                tab.AddChild(rulesVBox);

                Contents.AddChild(tab);
            }

            protected override Vector2 ContentsMinimumSize => new Vector2(600, 400);
        }

       internal record AutoModListData(AutoModRule rule) : ListData;
    }
}
