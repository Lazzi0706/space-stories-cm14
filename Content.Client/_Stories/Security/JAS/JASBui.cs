using System.Linq;
using Content.Client._Stories.Security.JAS.Controls;
using Content.Client._Stories.Security.JAS.Layouts;
using Content.Client._Stories.Security.JAS.Views;
using Content.Shared._Stories.Security.JAS;
using Robust.Client.UserInterface;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Client._Stories.Security.JAS;

public sealed class JASBui : BoundUserInterface
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    private JASWindow? _window;

    private JASMainLayout? _mainLayout;
    private JASHomeLayout? _homeLayout;


    private JASDetailsView? _detailsView;
    private JASChargesView? _chargesView;

    public JASBui (EntityUid owner, Enum uiKey)  : base (owner, uiKey) {}


    protected override void Open()
    {
        if (!EntMan.TryGetComponent(Owner, out JASComponent? jasComp))
            return;

        base.Open();

        _window = this.CreateWindow<JASWindow>();
        _homeLayout = new JASHomeLayout();

        _window.LoginButton.OnPressed += _ => Login();

    }

    protected override void UpdateState(BoundUserInterfaceState rawState)
    {
        base.UpdateState(rawState);

        if (rawState is not JASBuiState state)
            return;

        UpdateWindow(state);
    }

    private void UpdateWindow(JASBuiState state)
    {
        if (_window == null)
            return;
        if (!EntMan.TryGetComponent(Owner, out JASComponent? jasComp))
            return;

        if (state.Authorized && _mainLayout == null)
        {
            _window.JASHomeLayoutContainer.Visible = false;
            _mainLayout = new JASMainLayout();
            _detailsView = new JASDetailsView();
            _chargesView = new JASChargesView();

            _mainLayout.LogoutButton.OnPressed += _ => Logout();

            _mainLayout.DetailsOptionButton.OnMouseEntered += _ => _mainLayout.Cursor.Text = "jas -chl details";
            _mainLayout.ChargesOptionButton.OnMouseEntered += _ => _mainLayout.Cursor.Text = "jas -chl charges";
            _mainLayout.WitnessOptionButton.OnMouseEntered += _ => _mainLayout.Cursor.Text = "jas -chl witness";
            _mainLayout.ExportOptionButton.OnMouseEntered += _ => _mainLayout.Cursor.Text = "jas -chl export";

            _mainLayout.DetailsOptionButton.OnPressed += _ => ShowTab(JASTabs.Details);
            _mainLayout.ChargesOptionButton.OnPressed += _ => ShowTab(JASTabs.Charges);

            if (state.Tab != JASTabs.None)
                ShowTab(state.Tab);

            if (!string.IsNullOrEmpty(state.Details))
                _detailsView.Input.TextRope = new Rope.Leaf(state.Details);


            var chargesCategories = jasComp.LawCategories.ToList();

            foreach (var chargeCategory in chargesCategories)
            {
                if (!_prototype.TryIndex(chargeCategory, out var chargeCategoryPrototype))
                    continue;

                var chargeCategoryButton = new JASChargeCategoryButton(chargeCategoryPrototype.Name);

                switch (chargeCategoryPrototype.Name)
                {
                    case "Variable":
                        chargeCategoryButton.Modulate = Color.Lime;
                        break;
                    case "Minor":
                        chargeCategoryButton.Modulate = Color.Yellow;
                        break;
                    case "Major":
                        chargeCategoryButton.Modulate = Color.Orange;
                        break;
                    case "Capital":
                        chargeCategoryButton.Modulate = Color.Crimson;
                        break;
                }

                _chargesView.Categories.AddChild(chargeCategoryButton);

                foreach (var charge in chargeCategoryPrototype.Tags.ToList())
                {
                    if (!_prototype.TryIndex(charge, out var chargePrototype))
                        continue;

                    var chargeCard = new JASChargeCard(chargePrototype.Name, chargePrototype.Description);

                    _chargesView.ChargesList.AddChild(chargeCard);
                }
            }

            _window.JASMainLayoutContainer.AddChild(_mainLayout);
        }
        else if (!state.Authorized)
        {
            _mainLayout = null;
            _window.JASHomeLayoutContainer.Visible = true;
            _window.JASMainLayoutContainer.RemoveAllChildren();
        }

    }

    private void Login()
    {
        if (_window == null)
            return;
        if (!EntMan.TryGetComponent<JASComponent>(Owner, out var jasComp))
            return;

        SendPredictedMessage(new JASPrivilegedIdInsertedMsg());

    }
    private void Logout()
    {
        if (_window == null)
            return;
        if (!EntMan.TryGetComponent<JASComponent>(Owner, out var jasComp))
            return;

        SendPredictedMessage(new JASPrivilegedIdEjectedMsg());

    }

    private void ShowTab(JASTabs tab)
    {
        if (_window == null || _mainLayout == null)
            return;
        if (_detailsView == null || _chargesView == null)
            return;

        _mainLayout.MainMenuOptionsLayout.RemoveAllChildren();

        switch (tab)
        {
            case JASTabs.Details:
                _mainLayout.Cursor.Text = "jas -chl details";
                _detailsView.SaveButton.OnPressed += _ =>
                {
                    SendPredictedMessage(new JASDetailsSaveMsg(Rope.Collapse(_detailsView.Input.TextRope)));
                };
                _mainLayout.MainMenuOptionsLayout.AddChild(_detailsView);
                break;
            case JASTabs.Charges:
                _mainLayout.Cursor.Text = "jas -chl charges";
                _mainLayout.MainMenuOptionsLayout.AddChild(_chargesView);
                break;
        }

        SendPredictedMessage(new JASTabSelectedMsg(tab));
    }

}
