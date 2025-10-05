# Search Engine Indexing

We deliberately prevent this site from being indexed by search engines. We do this in **three layers**:

1) an **HTTP response header** on every request  
2) a **meta robots** tag in our HTML layouts  
3) a **robots.txt** that disallows all crawling

This belt-and-braces approach covers HTML pages, static assets, and edge cases.

---

## 1 Global `X-Robots-Tag` header (Program.cs)

Add this middleware **before** `UseStaticFiles()` so it also applies to CSS/JS/images:

```csharp
// Block indexing for all responses (HTML + static assets)
app.Use(async (ctx, next) =>
{
    ctx.Response.Headers["X-Robots-Tag"] = "noindex, nofollow, noarchive";
    await next();
});
```

## 2 Meta robots in our HTML layouts

Add the tag to both layouts that render HTML pages:

- **App shell layout** (e.g., the `<head>` in your App.razor HTML shell):

    ```html
    <meta name="robots" content="noindex, nofollow, noarchive" />
    ```

- **Identity layout** (`/Pages/Shared/_IdentityLayout.cshtml`, inside `<head>`):

    ```html
    <meta name="robots" content="noindex, nofollow, noarchive" />
    ```

---

## 3 robots.txt

`wwwroot/robots.txt`:

```
User-agent: *
Disallow: /
```

This politely tells crawlers not to fetch any paths. (Search engines may still fetch for discovery, which is why we also send the header and meta tags.)

---

## Verification

- **Header**
  - Windows:
    ```powershell
    curl -I https://your-host/ | findstr /I "X-Robots-Tag"
    ```
  - macOS/Linux:
    ```bash
    curl -I https://your-host/ | grep -i x-robots-tag
    ```
  Should show: `X-Robots-Tag: noindex, nofollow, noarchive`.

- **Meta tag**
  - Open a page → DevTools → Elements → `<head>` → verify the `<meta name="robots" ...>` tag.

- **robots.txt**
  - Visit `https://your-host/robots.txt` and confirm:
    ```
    User-agent: *
    Disallow: /
    ```

---

## Notes

- Pages behind authentication aren’t indexable anyway, but these measures prevent the **login page** and **static assets** from being indexed or cached by search engines.
- If you use a CDN or reverse proxy, ensure it doesn’t strip the `X-Robots-Tag` header.
- To remove anything already indexed, use Google Search Console’s **Removals** tool and keep `noindex` in place until it drops.