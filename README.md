# Routed layout renders itself twice

A component that is both a routable page and the layout of another page renders itself twice after navigating from the child page to it.

## Repro

1. `dotnet run`
2. Open http://localhost:5059/parent/child
3. Click `parent`

Expected: one "Parent" heading.
Actual: two "Parent" headings. Loading `/parent` directly renders one.

Reproduces on .NET 10.0.13 and 11.0.0-rc.1 with interactive server rendering.

## Pages

- `Components/Pages/Parent.razor`: `@page "/parent"`, `@layout MainLayout`, inherits `LayoutComponentBase`.
- `Components/Pages/Child.razor`: `@page "/parent/child"`, `@layout Parent`.

## Cause

On `/parent/child` the tree is `MainLayout > Parent (Body = Child) > Child`.
On `/parent` it is `MainLayout > Parent`, so the renderer retains the same `Parent` instance.
`RouteView` supplies no `Body` when `Parent` is the routed page, so `Parent` keeps its previous `Body`.
That fragment ends in `RouteView`'s page render delegate, which renders the current `RouteData` page: `Parent` again, inside itself.

Without `@layout MainLayout` on `Parent` the root type differs between the two routes, `Parent` is recreated, and the bug does not show.

## Possible fix

One way is to pass `Body = null` to a routed page that is a `LayoutComponentBase`, in `RouteView.RenderPageWithParameters`:

```diff
     foreach (var kvp in RouteData.RouteValues)
     {
         builder.AddComponentParameter(1, kvp.Key, kvp.Value);
     }
 
+    if (typeof(LayoutComponentBase).IsAssignableFrom(RouteData.PageType))
+    {
+        builder.AddComponentParameter(2, nameof(LayoutComponentBase.Body), (RenderFragment)null);
+    }
+
     builder.CloseComponent();
```

`Components/BodyResettingRouteView.cs` is a copy of `RouteView.Render` and `RenderPageWithParameters` with that change.
The only other difference is that it skips the private layout attribute cache.
To try it, replace `RouteView` with `BodyResettingRouteView` in `Components/Routes.razor`.

This only covers `Body`, and it adds a type check per render that the real fix would probably cache.
A more general fix is to reset parameters that a parent stops supplying, as proposed in #31272.
