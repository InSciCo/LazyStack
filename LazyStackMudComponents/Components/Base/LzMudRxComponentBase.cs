using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using ReactiveUI.Blazor;


namespace LzMudComponents
{
    public class LzMudRxComponentBase<T>: ReactiveComponentBase<T>
        where T : class, INotifyPropertyChanged
    {
        /// <summary>
        /// User class names, separated by space.
        /// </summary>
        [Parameter]
        [MudBlazor.Category(CategoryTypes.ComponentBase.Common)]
        public string Class { get; set; }

        /// <summary>
        /// User styles, applied on top of the component's own classes and styles.
        /// </summary>
        [Parameter]
        [MudBlazor.Category(CategoryTypes.ComponentBase.Common)]
        public string Style { get; set; }

        /// <summary>
        /// Use Tag to attach any user data object to the component for your convenience.
        /// </summary>
        [Parameter]
        [MudBlazor.Category(CategoryTypes.ComponentBase.Common)]
        public object Tag { get; set; }

        /// <summary>
        /// UserAttributes carries all attributes you add to the component that don't match any of its parameters.
        /// They will be splatted onto the underlying HTML tag.
        /// </summary>
        [Parameter(CaptureUnmatchedValues = true)]
        [MudBlazor.Category(CategoryTypes.ComponentBase.Common)]
        public Dictionary<string, object> UserAttributes { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// If the UserAttributes contain an ID make it accessible for WCAG labelling of input fields
        /// </summary>
        public string FieldId => (UserAttributes?.ContainsKey("id") == true ? UserAttributes["id"].ToString() : $"mudinput-{Guid.NewGuid()}");

    }
}
