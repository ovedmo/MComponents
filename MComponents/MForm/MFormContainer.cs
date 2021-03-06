﻿using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Localization;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MComponents.MForm
{
    public class MFormContainer : ComponentBase
    {
        [Parameter(CaptureUnmatchedValues = true)]
        public IReadOnlyDictionary<string, object> AdditionalAttributes { get; set; }

        [Parameter]
        public RenderFragment<MFormContainerContext> ChildContent { get; set; }

        [Parameter]
        public bool EnableSaveButton { get; set; } = true;

        [Parameter]
        public EventCallback<MFormContainerAfterAllFormsSubmittedArgs> OnAfterAllFormsSubmitted { get; set; }

        [Inject]
        public IStringLocalizer<MComponentsLocalization> L { get; set; }

        protected MFormContainerContext mFormContext;

        protected override void OnInitialized()
        {
            base.OnInitialized();
            mFormContext = new MFormContainerContext(this);
        }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenRegion(mFormContext.GetHashCode());

            builder.OpenElement(0, "div");
            builder.AddMultipleAttributes(1, AdditionalAttributes);

            builder.OpenComponent<CascadingValue<MFormContainerContext>>(3);
            builder.AddAttribute(4, "IsFixed", true);
            builder.AddAttribute(5, "Value", mFormContext);
            builder.AddAttribute(6, "ChildContent", ChildContent.Invoke(mFormContext));
            builder.CloseComponent();

            builder.CloseElement();

            builder.OpenElement(55, "div");
            builder.AddAttribute(56, "class", "m-form-row");

            builder.OpenElement(55, "div");
            builder.AddAttribute(56, "class", "form-group col-12");

            builder.OpenElement(55, "div");
            builder.AddAttribute(56, "class", "col-12");

            if (EnableSaveButton)
            {
                builder.OpenElement(19, "button");
                builder.AddAttribute(20, "type", "button");
                builder.AddAttribute(20, "class", "m-btn m-btn-primary");
                builder.AddAttribute(21, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, Click));
                builder.AddContent(22, L["Save"]);
                builder.CloseElement();
            }

            builder.CloseElement(); //div
            builder.CloseElement(); //div
            builder.CloseElement(); //div

            builder.CloseRegion();
        }

        protected void Click(MouseEventArgs pArgs)
        {
            TrySubmit();
        }

        public Task<bool> TrySubmit()
        {
            return mFormContext.NotifySubmit(L);
        }

        public bool HasUnsavedChanges
        {
            get { return mFormContext.Forms.Any(f => f.HasUnsavedChanges); }
        }
    }
}
