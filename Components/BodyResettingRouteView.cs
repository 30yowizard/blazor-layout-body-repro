using System.Reflection;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace LayoutBodyRepro.Components;

public sealed class BodyResettingRouteView : RouteView
{
    protected override void Render(RenderTreeBuilder builder)
    {
        var layout = RouteData.PageType.GetCustomAttribute<LayoutAttribute>()?.LayoutType ?? DefaultLayout;

        builder.OpenComponent<LayoutView>(0);
        builder.AddComponentParameter(1, nameof(LayoutView.Layout), layout);
        builder.AddComponentParameter(2, nameof(LayoutView.ChildContent), (RenderFragment)RenderPage);
        builder.CloseComponent();
    }

    private void RenderPage(RenderTreeBuilder builder)
    {
        builder.OpenComponent(0, RouteData.PageType);

        foreach (var routeValue in RouteData.RouteValues)
        {
            builder.AddComponentParameter(1, routeValue.Key, routeValue.Value);
        }

        if (typeof(LayoutComponentBase).IsAssignableFrom(RouteData.PageType))
        {
            builder.AddComponentParameter(2, nameof(LayoutComponentBase.Body), (RenderFragment?)null);
        }

        builder.CloseComponent();
    }
}
