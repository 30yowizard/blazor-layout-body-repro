using System.Reflection;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace LayoutBodyRepro.Components;

public sealed class BodyResettingRouteView : RouteView
{
    protected override void Render(RenderTreeBuilder builder)
    {
        var pageLayoutType = RouteData.PageType.GetCustomAttribute<LayoutAttribute>()?.LayoutType
            ?? DefaultLayout;

        builder.OpenComponent<LayoutView>(0);
        builder.AddComponentParameter(1, nameof(LayoutView.Layout), pageLayoutType);
        builder.AddComponentParameter(2, nameof(LayoutView.ChildContent), (RenderFragment)RenderPageWithParameters);
        builder.CloseComponent();
    }

    private void RenderPageWithParameters(RenderTreeBuilder builder)
    {
        builder.OpenComponent(0, RouteData.PageType);

        foreach (var kvp in RouteData.RouteValues)
        {
            builder.AddComponentParameter(1, kvp.Key, kvp.Value);
        }

        if (typeof(LayoutComponentBase).IsAssignableFrom(RouteData.PageType))
        {
            builder.AddComponentParameter(2, nameof(LayoutComponentBase.Body), (RenderFragment?)null);
        }

        builder.CloseComponent();
    }
}
