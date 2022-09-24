using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Circuits.Services.Services.Interfaces;

namespace Circuits.Components.Common.Select;

public partial class BCHSelect<TItem> : ComponentBase, IDisposable where TItem : class
{
    [Inject] private IJSUtilsService _jsUtilsService { get; set; } = null!;

    private class Element
    {
        public string Name { get; set; } = string.Empty;
        public TItem Item { get; set; } = default!;
    }

    private class Group
    {
        public bool Expanded { get; set; } = true;
        public string Name { get; set; } = string.Empty;
        public List<Element> Elements { get; set; } = new();
    }

    [Inject] private IJSRuntime _jsRuntime { get; set; } = null!;

    [Parameter] public EventCallback OnFocusOut { get; set; }
    [Parameter] public bool Grouping { get; set; } = false;
    [Parameter] public bool Filtering { get; set; } = false;
    [Parameter] public bool MultipleSelect { get; set; } = false;
    [Parameter] public bool ScrollToSelected { get; set; } = false;
    [Parameter] public string CssClass { get; set; } = string.Empty;

    [Parameter] public int ItemHeight { get; set; } = 40;
    [Parameter] public int Height { get; set; } = 200;
    [Parameter] public int Width { get; set; } = 200;
    [Parameter] public TItem DefaultValue { get; set; } = null!;
    [Parameter] public string DefaultText { get; set; } = "Please Select";
    [Parameter] public IEnumerable<TItem> Options { get; set; } = new List<TItem>();
    [Parameter] public Func<TItem, string> ElementNamePredicate { get; set; } = null!;
    [Parameter] public Func<TItem, string> FilterByPredicate { get; set; } = null!;
    [Parameter] public Func<TItem, string> GroupNamePredicate { get; set; } = x => string.Empty;
    [Parameter] public Func<TItem, object> GroupPredicate { get; set; } = x => string.Empty;
    [Parameter] public Func<TItem, string> CssItemPredicate { get; set; } = x => string.Empty;
    [Parameter] public EventCallback<KeyboardEventArgs> OnFilterKeyDown { get; set; }

    [Parameter] public EventCallback<TItem> SelectedChanged { get; set; }
    [Parameter]
    public TItem Selected
    {
        get => _selectedValue;
        set
        {
            if (_selectedValue == value) return;
            _selectedValue = value;

            SelectedChanged.InvokeAsync(value);
        }
    }

    [Parameter] public EventCallback<string> FilterChanged { get; set; }
    [Parameter]
    public string Filter
    {
        get => _typedFilterValue;
        set
        {
            if (_typedFilterValue == value) return;
            _typedFilterValue = value;
            OnFilterType();
            FilterChanged.InvokeAsync(value);
        }
    }

    [Parameter] public EventCallback<bool> IsOpenedChanged { get; set; }
    [Parameter]
    public bool IsOpened
    {
        get => _isOpened;
        set
        {
            if (_isOpened == value) return;
            _isOpened = value;
            IsOpenedChanged.InvokeAsync(value);
        }
    }

    [Parameter] public EventCallback<List<TItem>> SelectedItemsChanged { get; set; }
    [Parameter]
    public List<TItem> SelectedItems
    {
        get => _selectedItems;
        set
        {
            if (CheckListEquality(_selectedItems, value))
            {
                return;
            }

            _selectedItems.Clear();
            _selectedItems.AddRange(value);
            SelectedItemsChanged.InvokeAsync(value);
        }
    }

    [Parameter] public RenderFragment<TItem> RowTemplate { get; set; } = null!;

    private string _conteinerId = $"_id{Guid.NewGuid()}";
    private string _contentId = $"_id{Guid.NewGuid()}";
    private string _inputId = $"_id{Guid.NewGuid()}";
    private string _scrollerId = $"_id{Guid.NewGuid()}";

    private TItem _selectedValue = null!;
    private bool _isOpened = false;
    private ElementReference _inputRef;
    private string _typedFilterValue = string.Empty;
    private string _placeholder = "";

    private List<Group> _groups = new();

    private int _prevCount = -1;
    private DotNetObjectReference<BCHSelect<TItem>> _dotNetObjectReference = null!;
    private bool _scrolled = false;

    private List<TItem> _selectedItems = new();

    protected override async Task OnInitializedAsync()
    {
        if (FilterByPredicate is null)
        {
            FilterByPredicate = ElementNamePredicate;
        }

        if (!MultipleSelect)
        {
            Selected = DefaultValue == null ? null! : DefaultValue;
        }

        _placeholder = Selected == null ? DefaultText : ElementNamePredicate.Invoke(Selected);

        await SelectedItemsChanged.InvokeAsync(_selectedItems);

        StateHasChanged();

        await base.OnInitializedAsync();
    }

    public void Dispose()
    {
        _dotNetObjectReference?.Dispose();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _dotNetObjectReference = DotNetObjectReference.Create(this);
            await _jsRuntime.InvokeVoidAsync("bchSelectAddOnOuterFocusOut", _inputId, _conteinerId, _dotNetObjectReference, "OnContainerFocusOutAsync");
        }

        if (IsOpened)
        {
            await _inputRef.FocusAsync();

            if (ScrollToSelected && Selected != null && !MultipleSelect && !_scrolled)
            {
                _scrolled = true;

                int index = Grouping ? -1 : -2;

                for (int i = 0; i < _groups.Count; i++)
                {
                    foreach (var element in _groups[i].Elements)
                    {
                        if (element.Item == Selected)
                        {
                            i = _groups.Count;
                            break;
                        }

                        index++;
                    }

                    index++;
                }
                int offset = index * ItemHeight;

                await _jsUtilsService.ScrollToAsync(_scrollerId, "0", $"{offset}", "auto");
            }
        }

        if (_prevCount != Options.Count())
        {
            FilterData();
            StateHasChanged();
        }
    }

    private async Task OnOptionClickedAsync(TItem option)
    {
        if (!MultipleSelect)
        {
            IsOpened = false;
            Selected = option;
            Filter = string.Empty;

            if (Selected != null)
            {
                _placeholder = ElementNamePredicate.Invoke(Selected);
            }
        }
        else
        {
            if (_selectedItems.Contains(option))
            {
                _selectedItems.Remove(option);
            }
            else
            {
                _selectedItems.Add(option);
            }

            await SelectedItemsChanged.InvokeAsync(_selectedItems);
        }

        _scrolled = false;

        StateHasChanged();
    }

    private bool IsSelected(TItem option)
    {
        if (MultipleSelect)
        {
            return _selectedItems.Contains(option);
        }

        return _selectedValue == option;
    }

    private void OnSelectClicked()
    {
        IsOpened = !IsOpened;
        _scrolled = false;
        Filter = string.Empty;

        FilterData();
        StateHasChanged();
    }

    private void OnFilterType()
    {
        FilterData(_typedFilterValue);

        StateHasChanged();
    }

    private void FilterData(string filter = "")
    {
        _groups = new();

        var groups = Options.GroupBy(GroupPredicate);
        _prevCount = Options.Count();

        foreach (var group in groups)
        {
            if (group.Count() == 0) continue;

            var gr = new Group
            {
                Name = GroupNamePredicate.Invoke(group.First())
            };

            foreach (var item in group)
            {
                var name = ElementNamePredicate == null ? string.Empty : ElementNamePredicate.Invoke(item);
                var filterElementValue = FilterByPredicate.Invoke(item);

                if (!string.IsNullOrEmpty(filterElementValue) && filterElementValue.Contains(filter, StringComparison.OrdinalIgnoreCase))
                {
                    gr.Elements.Add(new Element
                    {
                        Item = item,
                        Name = name
                    });
                }
            }

            if (gr.Elements.Count == 0) continue;

            _groups.Add(gr);
        }
    }

    private void OnGroupClicked(Group group)
    {
        group.Expanded = !group.Expanded;
        StateHasChanged();
    }

    private async Task OnInputKeyDownAsync(KeyboardEventArgs e)
    {
        if (e.Code == "Enter" || e.Code == "NumpadEnter")
        {
            _typedFilterValue = string.Empty;
            StateHasChanged();
        }
        else
        {
            if (!IsOpened)
            {
                IsOpened = true;
                _scrolled = false;

                FilterData();
                StateHasChanged();
            }
        }

        await OnFilterKeyDown.InvokeAsync(e);
    }

    [JSInvokable]
    public async Task OnContainerFocusOutAsync()
    {
        IsOpened = false;
        _scrolled = false;
        Filter = string.Empty;
        await OnFocusOut.InvokeAsync();
        StateHasChanged();
    }

    private bool CheckListEquality(List<TItem> list1, List<TItem> list2)
    {
        if (list1.Count != list2.Count)
        {
            return false;
        }

        for (int i = 0; i < list1.Count; i++)
        {
            if (list1[i] != list2[i])
            {
                return false;
            }
        }

        return true;
    }
}