﻿using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components.Web;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Components.Rendering;
using System.Linq.Expressions;
using Microsoft.Extensions.Localization;

namespace MComponents.MSelect
{
    public class MSelect<T> : InputSelect<T>, IMSelect
    {
        [Inject]
        private IJSRuntime JSRuntime { get; set; }

        [Inject]
        public IStringLocalizer<MComponentsLocalization> L { get; set; }

        [CascadingParameter]
        protected EditContext CascadedEditContext2 { get; set; }

        [Parameter]
        public IEnumerable<T> Options { get; set; }

        [Parameter]
        public string Property { get; set; }

        [Parameter]
        public Type PropertyType { get; set; }

        [Parameter]
        public bool EnableSearch { get; set; }

        [Parameter]
        public bool IsDisabled { get; set; }

        [Parameter]
        public EventCallback<SelectionChangedArgs<T>> OnSelectionChanged { get; set; }

        protected string mNullValueOverride;

        private string mNullValueDescription;

        [Parameter]
        public string NullValueDescription
        {
            get => mNullValueOverride ?? mNullValueDescription;
            set => mNullValueDescription = value;
        }

        [Parameter]
        public ICollection<T> Values { get; set; }

        [Parameter]
        public EventCallback<ICollection<T>> ValuesChanged { get; set; }

        [Parameter]
        public Expression<Func<ICollection<T>>> ValuesExpression { get; set; }

        protected ElementReference OptionsDiv { get; set; }
        protected ElementReference SelectSpan { get; set; }

        protected bool mOptionsVisible;
        protected bool mBlockNextFocus;

        protected T[] DisplayValues = new T[0];

        protected T SelectedValue;

        protected ElementReference SearchInput { get; set; }

        protected string InputValue = string.Empty;

        protected IMPropertyInfo mPropertyInfo;

        protected List<MSelectOption> mAdditionalOptions = new List<MSelectOption>();

        protected Type mtType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);

        protected bool MultipleSelectMode => ValuesExpression != null;

        public override async Task SetParametersAsync(ParameterView parameters)
        {
            //Workaround for bypass check is ValueExpression is set

            parameters.SetParameterProperties(this);

            if (MultipleSelectMode)
            {
                EditContext = CascadedEditContext2;
            }

            await base.SetParametersAsync(parameters);

            if (MultipleSelectMode)
            {
                FieldIdentifier = FieldIdentifier.Create(ValuesExpression);
            }

            mNullValueDescription = mNullValueDescription ?? L["Please select..."];
        }

        protected override void OnInitialized()
        {
            if (Values != null && Value != null)
                throw new ArgumentException($"use {nameof(Values)} or {nameof(Value)} through bind-value, but not both");

            if (MultipleSelectMode && Values == null)
                throw new ArgumentException($"{nameof(Values)} must be != null");

            if (Options == null && mtType.IsEnum)
            {
                Options = Enum.GetValues(mtType).Cast<T>();
            }

            if (Property != null)
            {
                mPropertyInfo = ReflectionHelper.GetIMPropertyInfo(typeof(T), Property, PropertyType);
            }

            if (Options != null)
            {
                DisplayValues = Options.ToArray();
            }

            if (MultipleSelectMode)
            {
                UpdateDescription();
            }

            base.OnInitialized();
        }

        protected override void BuildRenderTree(RenderTreeBuilder pBuilder)
        {
            if (this.ChildContent != null)
            {
                RenderFragment child() =>
                        (builder2) =>
                        {
                            builder2.AddContent(56, this.ChildContent);
                        };

                pBuilder.OpenComponent<CascadingValue<MSelect<T>>>(4);
                pBuilder.AddAttribute(5, "Value", this);
                pBuilder.AddAttribute(6, "ChildContent", child());
                pBuilder.CloseComponent();
            }

            pBuilder.OpenElement(0, "span");

            if (!IsDisabled)
            {
                pBuilder.AddAttribute(157, "tabindex", "0");
                pBuilder.AddAttribute(158, "onkeydown", EventCallback.Factory.Create<KeyboardEventArgs>(this, InputKeyDown));
                pBuilder.AddEventStopPropagationAttribute(159, "onkeydown", true);
                pBuilder.AddEventStopPropagationAttribute(4345, "onkeyup", true);

                pBuilder.AddAttribute(160, "onfocus", EventCallback.Factory.Create<FocusEventArgs>(this, OnFocusIn));
                pBuilder.AddAttribute(161, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, OnComboboxClicked));
            }

            pBuilder.AddAttribute(12, "class", "m-select m-form-control m-clickable " + CssClass + (IsDisabled ? " m-select--disabled" : string.Empty) + (mOptionsVisible ? " m-select--open" : string.Empty));

            if (AdditionalAttributes != null)
                pBuilder.AddMultipleAttributes(4, AdditionalAttributes.Where(a => a.Key.ToLower() != "class" && a.Key.ToLower() != "onkeydown" && a.Key.ToLower() != "onkeyup"));

            pBuilder.AddElementReferenceCapture(163, (__value) =>
            {
                SelectSpan = __value;
            });

            pBuilder.OpenElement(19, "span");
            pBuilder.AddAttribute(20, "class", "m-select-content");
            pBuilder.AddAttribute(21, "role", "textbox");

            pBuilder.AddContent(24,
                  CurrentValueAsString == null || CurrentValueAsString == string.Empty ? NullValueDescription : CurrentValueAsString
            );

            pBuilder.CloseElement();

            pBuilder.AddMarkupContent(27, "<span class=\"m-select-dropdown-icon fa fa-angle-down\" role =\"presentation\"></span>");

            pBuilder.CloseElement();

            if (mOptionsVisible)
            {
                pBuilder.OpenElement(32, "div");
                pBuilder.AddAttribute(33, "tabindex", "0");

                pBuilder.AddAttribute(11, "onkeydown", EventCallback.Factory.Create<KeyboardEventArgs>(this, InputKeyDown));
                pBuilder.AddEventStopPropagationAttribute(12, "onkeydown", true);
                pBuilder.AddEventStopPropagationAttribute(12, "onkeyup", true);
                pBuilder.AddEventPreventDefaultAttribute(495, "onkeydown", true);
                pBuilder.AddEventPreventDefaultAttribute(496, "onkeyup", true);

                //This causes sometimes an error in console
                //see https://github.com/dotnet/aspnetcore/issues/21241
                pBuilder.AddAttribute(34, "onfocusout", EventCallback.Factory.Create<FocusEventArgs>(this, OnFocusLost));

                pBuilder.AddAttribute(35, "class", "m-select-options-container");
                pBuilder.AddElementReferenceCapture(36, (__value) =>
                {
                    OptionsDiv = __value;
                });

                pBuilder.OpenElement(38, "div");
                pBuilder.AddAttribute(39, "class", "m-select-options-list-container");

                if (EnableSearch)
                {
                    pBuilder.OpenElement(43, "span");
                    pBuilder.AddAttribute(44, "class", "m-select-search-container");

                    pBuilder.OpenElement(46, "input");
                    pBuilder.AddAttribute(47, "oninput", EventCallback.Factory.Create<ChangeEventArgs>(this, OnSearchInputChanged));
                    pBuilder.AddAttribute(48, "onkeydown", EventCallback.Factory.Create<KeyboardEventArgs>(this, InputKeyDown));

                    pBuilder.AddEventStopPropagationAttribute(483, "onkeydown", true);
                    pBuilder.AddEventStopPropagationAttribute(484, "onkeyup", true);
                    pBuilder.AddEventStopPropagationAttribute(485, "oninput", true);

                    // pBuilder.AddEventPreventDefaultAttribute(485, "onkeydown", true);
                    // pBuilder.AddEventPreventDefaultAttribute(486, "onkeyup", true);

                    pBuilder.AddAttribute(49, "value", "");
                    pBuilder.AddAttribute(50, "class", "m-form-control m-select-search-input");
                    pBuilder.AddAttribute(51, "type", "search");
                    pBuilder.AddAttribute(52, "tabindex", "0");
                    pBuilder.AddAttribute(53, "autocomplete", "off");
                    pBuilder.AddAttribute(54, "autocorrect", "off");
                    pBuilder.AddAttribute(55, "autocapitalize", "none");
                    pBuilder.AddAttribute(56, "spellcheck", "false");
                    pBuilder.AddAttribute(57, "role", "search");

                    pBuilder.AddElementReferenceCapture(61, (__value) =>
                    {
                        SearchInput = __value;
                    });
                    pBuilder.CloseElement(); //input

                    pBuilder.CloseElement(); //span

                    Task.Delay(50).ContinueWith((a) =>
                    {
                        if (SearchInput.Id != null)
                            _ = JSRuntime.InvokeVoidAsync("mcomponents.focusElement", SearchInput);
                    });
                }
                else
                {
                    Task.Delay(50).ContinueWith((a) =>
                    {
                        if (OptionsDiv.Id != null)
                            _ = JSRuntime.InvokeVoidAsync("mcomponents.focusElement", OptionsDiv);
                    });
                }

                pBuilder.OpenElement(68, "ul");
                pBuilder.AddAttribute(69, "class", "m-select-options-list m-scrollbar");
                pBuilder.AddAttribute(70, "role", "listbox");

                int i = 0;

                foreach (var entry in DisplayValues)
                {
                    int index = i;

                    bool isSelected = entry != null && entry.Equals(SelectedValue);

                    pBuilder.OpenElement(76, "li");
                    pBuilder.AddAttribute(77, "class", "m-select-options-entry m-clickable" + (isSelected ? " m-select-options-entry--highlighted" : string.Empty));
                    pBuilder.AddAttribute(80, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, () => OnOptionSelect(index)));
                    pBuilder.AddAttribute(78, "role", "option");

                    if (MultipleSelectMode)
                    {
                        pBuilder.OpenElement(76, "label");
                        pBuilder.AddAttribute(78, "class", "m-checkbox m-select-checkbox m-clickable");
                        pBuilder.AddEventStopPropagationClicksAttribute(81);

                        pBuilder.OpenElement(76, "input");
                        pBuilder.AddAttribute(78, "type", "checkbox");

                        pBuilder.AddAttribute(80, "onchange", EventCallback.Factory.Create<ChangeEventArgs>(this, () => OnOptionSelect(index)));
                        pBuilder.AddEventStopPropagationClicksAttribute(81);

                        if (Values.Contains(entry))
                        {
                            pBuilder.AddAttribute(78, "checked", "checked");
                        }

                        pBuilder.CloseElement(); //input

                        pBuilder.OpenElement(81, "span"); //this is required for design magic
                        pBuilder.CloseElement();

                        pBuilder.AddContent(81, FormatValueAsString(entry));

                        pBuilder.CloseElement(); //label
                    }
                    else
                    {
                        pBuilder.AddAttribute(80, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, () => OnOptionSelect(index)));
                        pBuilder.AddContent(81, FormatValueAsString(entry));
                    }

                    pBuilder.CloseElement(); //li
                    i++;
                }

                foreach (var entry in mAdditionalOptions.Where(a => !a.ShowOnlyIfNoSearchResults || (i == 0 && a.ShowOnlyIfNoSearchResults)))
                {
                    bool isSelected = entry != null && entry.Equals(SelectedValue);

                    pBuilder.OpenElement(76, "li");
                    pBuilder.AddAttribute(77, "class", "m-select-options-entry m-clickable" + (isSelected ? " m-select-options-entry--highlighted" : string.Empty));
                    pBuilder.AddAttribute(78, "role", "option");
                    //        pBuilder.AddAttribute(79, "aria-selected", "false");

                    pBuilder.AddMultipleAttributes(80, entry.AdditionalAttributes);

                    pBuilder.AddContent(81, entry.Value.ToString());
                    pBuilder.CloseElement();
                }

                pBuilder.CloseElement();
                pBuilder.CloseElement();
                pBuilder.CloseElement();

                JSRuntime.InvokeVoidAsync("mcomponents.scrollToSelectedEntry");
            }
        }

        public void RegisterOption(MSelectOption pOption)
        {
            if (mAdditionalOptions.Any(o => o.Identifier == pOption.Identifier))
                return;

            mAdditionalOptions.Add(pOption);
            StateHasChanged();
        }

        protected void OnComboboxClicked(MouseEventArgs args)
        {
            ToggleOptions(true);
        }

        protected void OnFocusIn(FocusEventArgs args)
        {
            if (mBlockNextFocus)
            {
                mBlockNextFocus = false;
                return;
            }

            if (!mOptionsVisible)
                Task.Delay(150).ContinueWith((a) =>
                {
                    if (!mOptionsVisible)
                        InvokeAsync(() => ToggleOptions(false));
                });
        }

        protected void OnOptionSelect(int pIndex)
        {
            UpdateSelection(DisplayValues.ElementAt(pIndex));

            if (!MultipleSelectMode)
                ToggleOptions(true);
        }

        protected void OnFocusLost(FocusEventArgs pArgs)
        {
            Task.Delay(150).ContinueWith((a) =>
            {
                HideOptions(false);
            });
        }

        protected void ToggleOptions(bool pUserInteracted)
        {
            if (mOptionsVisible)
            {
                HideOptions(pUserInteracted);
            }
            else
            {
                ShowOptions();
            }
        }

        public void ShowOptions()
        {
            if (IsDisabled)
                return;

            if (Options != null)
                DisplayValues = Options.ToArray();
            SelectedValue = CurrentValue;

            mOptionsVisible = true;
            InvokeAsync(() => StateHasChanged());
        }

        public void HideOptions(bool pUserInteracted)
        {
            mOptionsVisible = false;
            InvokeAsync(() => StateHasChanged());

            if (pUserInteracted)
                Task.Delay(50).ContinueWith((a) =>
                {
                    if (SelectSpan.Id != null)
                    {
                        mBlockNextFocus = true;
                        _ = JSRuntime.InvokeVoidAsync("mcomponents.focusElement", SelectSpan);
                    }
                });
        }

        protected void OnSearchInputChanged(ChangeEventArgs args)
        {
            InputValue = (string)args.Value;

            if (InputValue == string.Empty)
            {
                DisplayValues = Options.ToArray();
            }
            else
            {
                InputValue = InputValue.ToLower();
                DisplayValues = Options.Where(v => FormatValueAsString(v).ToLower().Contains(InputValue)).ToArray();
            }

            StateHasChanged();
            JSRuntime.InvokeVoidAsync("mcomponents.scrollToSelectedEntry");
        }

        protected void InputKeyDown(KeyboardEventArgs args)
        {
            if (!mOptionsVisible)
            {
                if (args.Key == "Enter" || args.Key == " ")
                    ShowOptions();

                return;
            }

            if (args.Key == "Escape" || (args.Key == "Backspace" && InputValue.Length == 0))
            {
                HideOptions(true);
                return;
            }

            if (args.Key == "Enter")
            {
                if (SelectedValue == null || DisplayValues.Count() == 1 || !DisplayValues.Contains(SelectedValue))
                {
                    UpdateSelection(DisplayValues.FirstOrDefault());
                    HideOptions(true);
                    return;
                }

                if (!MultipleSelectMode)
                    UpdateSelection(SelectedValue);

                HideOptions(true);
                return;
            }

            if (args.Key == "ArrowDown")
            {
                int index = Array.IndexOf(DisplayValues, SelectedValue);

                if (index < 0)
                    index = -1;

                index++;

                if (index >= DisplayValues.Count())
                    return;

                SelectValue(DisplayValues.ElementAt(index));
                return;
            }

            if (args.Key == "ArrowUp")
            {
                int index = Array.IndexOf(DisplayValues, SelectedValue);

                if (index < 0)
                    index = 1;

                index--;

                if (index < 0)
                    return;

                SelectValue(DisplayValues.ElementAt(index));
                return;
            }

            if (args.Key == " ")
            {
                if (MultipleSelectMode)
                {
                    int index = Array.IndexOf(DisplayValues, SelectedValue);
                    if (index >= 0)
                        OnOptionSelect(index);
                    return;
                }

                return;
            }

            if (args.Key == "End")
            {
                SelectValue(DisplayValues.LastOrDefault());
                return;
            }

            if (args.Key == "Home")
            {
                SelectValue(DisplayValues.FirstOrDefault());
                return;
            }

            if (args.Key == "PageDown")
            {
                int index = Array.IndexOf(DisplayValues, SelectedValue);
                if (index < 0)
                    return;

                index += 8;

                if (index > DisplayValues.Length - 1)
                    return;

                SelectValue(DisplayValues[index]);
            }

            if (args.Key == "PageUp")
            {
                int index = Array.IndexOf(DisplayValues, SelectedValue);
                if (index < 0)
                    return;

                index -= 8;

                if (index < 0)
                    return;

                SelectValue(DisplayValues[index]);
            }
        }

        override protected string FormatValueAsString(T value)
        {
            if (value == null)
                return NullValueDescription;

            if (Property != null)
            {
                return mPropertyInfo.GetValue(value)?.ToString();
            }

            if (mtType.IsEnum)
            {
                var evalue = value as Enum;
                return evalue.ToName();
            }

            if (mtType == typeof(bool))
            {
                return (bool)(object)value ? L["True"] : L["False"];
            }

            return value.ToString();
        }

        private void UpdateSelection(T pSelectedValue)
        {
            if (MultipleSelectMode)
            {
                if (Values.Contains(pSelectedValue))
                {
                    Values.Remove(pSelectedValue);
                }
                else
                {
                    Values.Add(pSelectedValue);
                }

                UpdateDescription();

                ValuesChanged.InvokeAsync(Values);
                EditContext.NotifyFieldChanged(FieldIdentifier);
                StateHasChanged();
            }
            else
            {
                if ((pSelectedValue == null && CurrentValue != null) || (CurrentValue == null && pSelectedValue != null) || (pSelectedValue != null && !pSelectedValue.Equals(CurrentValue)))
                {
                    var oldValue = CurrentValue;
                    CurrentValue = pSelectedValue;

                    SelectionChangedArgs<T> args = new SelectionChangedArgs<T>()
                    {
                        NewValue = pSelectedValue,
                        OldValue = oldValue
                    };

                    OnSelectionChanged.InvokeAsync(args);
                }
            }
        }

        protected void UpdateDescription()
        {
            if (Values == null)
                mNullValueOverride = null;
            else
            {
                if (Values.Count >= 3)
                {
                    mNullValueOverride = string.Format(L["{0} items selected"], Values.Count);
                }
                else
                {
                    mNullValueOverride = string.Join(", ", Values.Select(v => FormatValueAsString(v)));
                }
            }

            if (mNullValueOverride == string.Empty)
                mNullValueOverride = null;
        }

        public void SelectValue(T pValue)
        {
            SelectedValue = pValue;
            JSRuntime.InvokeVoidAsync("mcomponents.scrollToSelectedEntry");
            StateHasChanged();
        }

        public void Refresh()
        {
            UpdateDescription();
            StateHasChanged();
        }
    }
}
