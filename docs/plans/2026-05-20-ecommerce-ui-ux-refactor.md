# ECommerceMVC UI/UX Refactor Plan

> For Hermes: implement directly in the existing ASP.NET MVC/Razor views and shared theme files.

Goal: Refactor the storefront into a modern furniture ecommerce experience that feels premium, friendly, and conversion-focused while reusing strong layout ideas from amado-master and visual inspiration from Dribbble furniture ecommerce shots.

Architecture:
- Keep the existing Razor/Bootstrap architecture.
- Centralize the visual redesign in the shared design system first, then retouch the highest-value surfaces: navbar, homepage hero, catalog, product cards, detail page, cart, newsletter, footer.
- Prefer reusable classes in modern-theme.css over large one-off inline styles.

Tech stack:
- ASP.NET MVC + Razor (.cshtml)
- Shared CSS theme in ECommerceMVC/wwwroot/css/modern-theme.css
- Existing vanilla JS in ECommerceMVC/wwwroot/js/modern-app.js
- Bootstrap 5.3 utilities where helpful

Reference direction:
- amado-master: furniture-first navigation, editorial category presentation, clean shop/product layout
- Dribbble furniture ecommerce patterns: neutral palette, large product imagery, warmer luxury tones, softer shadows, stronger whitespace, card hover polish, editorial headings

---

## Brainstorm summary

What should improve:
1. Current UI still carries too much cyber/web3 posture in the base theme.
2. Furniture ecommerce should feel warmer, calmer, and more tactile.
3. Stronger hierarchy is needed on shop, product detail, cart, and footer.
4. Product cards should communicate category, quality, and action faster.
5. The homepage hero should feel like a premium showroom, not a generic landing block.

Target experience:
- Warm editorial luxury
- Friendly and easy to scan
- Big product imagery
- Soft shadows, rounded corners, premium neutral palette
- Clear conversion actions
- Delightful but restrained motion

Visual system direction:
- Light default theme: ivory, sand, warm taupe, charcoal text, bronze accent
- Dark mode: graphite, espresso, muted bronze, soft ivory text
- Typography: serif-flavored heading + clean sans body
- Motion: subtle lift, fade, hover, image zoom

---

## TODO roadmap

1. Replace the global visual tokens and fonts with a furniture ecommerce design system.
2. Refactor navbar styling and wording to feel more premium and commerce-focused.
3. Refine homepage hero copy and supporting UI so it matches furniture ecommerce instead of generic web3 language.
4. Upgrade catalog page structure with a stronger intro and better filter/shop chrome.
5. Improve product cards with real category data and better action hierarchy.
6. Polish product detail page layout and cart experience.
7. Restyle newsletter and footer for a softer premium finish.
8. Build and verify.

---

## Implementation tasks

### Task 1: Replace the global design tokens
Files:
- Modify: ECommerceMVC/Views/Shared/_CustomerHead.cshtml
- Modify: ECommerceMVC/wwwroot/css/modern-theme.css

Steps:
1. Replace the Google font stack with a serif heading + modern sans body pairing.
2. Rewrite the root color tokens from cyber neon into warm furniture ecommerce neutrals.
3. Rewrite dark mode tokens to a premium charcoal/bronze palette.
4. Keep existing utility class names stable so view files do not break.

Verification:
- Site should still load existing pages without missing class names.
- Theme toggle should still work.

### Task 2: Improve navbar tone and actions
Files:
- Modify: ECommerceMVC/Views/Shared/_ModernNavbar.cshtml
- Modify: ECommerceMVC/wwwroot/css/modern-theme.css

Steps:
1. Tighten nav labels to match furniture shopping intent.
2. Add a prominent CTA link for shopping or showroom exploration.
3. Upgrade navbar surfaces, hover states, and mobile drawer polish.

Verification:
- Desktop nav remains readable.
- Mobile drawer still opens/closes.

### Task 3: Refine homepage hero for furniture ecommerce
Files:
- Modify: ECommerceMVC/Views/Home/Index.cshtml

Steps:
1. Keep the 3D showroom hero, but rewrite copy and metadata to fit furniture ecommerce.
2. Remove explicit web3 positioning from visible copy.
3. Strengthen action labels around shopping and showroom exploration.

Verification:
- Hero still renders the canvas.
- Buttons and category chips still function.

### Task 4: Upgrade the catalog experience
Files:
- Modify: ECommerceMVC/Views/HangHoa/Index.cshtml
- Modify: ECommerceMVC/wwwroot/css/modern-theme.css

Steps:
1. Add a catalog intro section inspired by furniture ecommerce landing patterns.
2. Improve the top results/sorting bar.
3. Make filter panel feel more like a premium merchandising sidebar.

Verification:
- Filters still submit.
- Sorting and view count controls still submit.

### Task 5: Upgrade product cards
Files:
- Modify: ECommerceMVC/Views/HangHoa/ProductItem.cshtml
- Modify: ECommerceMVC/wwwroot/css/modern-theme.css

Steps:
1. Replace fake static category text with real product category data.
2. Improve price badge, quick actions, card metadata, and CTA balance.
3. Add small trust/descriptor copy without clutter.

Verification:
- Product links still navigate.
- Add to cart and favourite actions still post.

### Task 6: Polish product detail and cart
Files:
- Modify: ECommerceMVC/Views/HangHoa/Detail.cshtml
- Modify: ECommerceMVC/Views/Cart/Index.cshtml
- Modify: ECommerceMVC/wwwroot/css/modern-theme.css

Steps:
1. Make product detail feel more editorial and premium.
2. Improve spacing, badges, purchase CTA emphasis, and review layout.
3. Improve cart clarity and order summary emphasis.

Verification:
- Quantity controls still work.
- Add-to-cart form and review form markup remains intact.

### Task 7: Restyle newsletter and footer
Files:
- Modify: ECommerceMVC/Views/Shared/_CustomerNewsletter.cshtml
- Modify: ECommerceMVC/Views/Shared/_CustomerFooter.cshtml
- Modify: ECommerceMVC/wwwroot/css/modern-theme.css

Steps:
1. Convert newsletter into a premium brand moment.
2. Improve footer hierarchy and trust language.
3. Keep all existing links working.

Verification:
- Newsletter form still posts.
- Footer remains responsive.

### Task 8: Build and verify
Files:
- No code target; run validation

Steps:
1. Run a build command that avoids apphost locking issues if the dev exe is running.
2. Report any warnings that pre-existed and any new errors if found.

Verification command:
- dotnet build ECommerceMVC.sln /p:UseAppHost=false

Expected:
- Build succeeds unless there are unrelated project issues.

---

## Expected deliverables

Changed files should include at minimum:
- F:\ECommerceMVC\ECommerceMVC\Views\Shared\_CustomerHead.cshtml
- F:\ECommerceMVC\ECommerceMVC\Views\Shared\_ModernNavbar.cshtml
- F:\ECommerceMVC\ECommerceMVC\Views\Home\Index.cshtml
- F:\ECommerceMVC\ECommerceMVC\Views\HangHoa\Index.cshtml
- F:\ECommerceMVC\ECommerceMVC\Views\HangHoa\ProductItem.cshtml
- F:\ECommerceMVC\ECommerceMVC\Views\HangHoa\Detail.cshtml
- F:\ECommerceMVC\ECommerceMVC\Views\Cart\Index.cshtml
- F:\ECommerceMVC\ECommerceMVC\Views\Shared\_CustomerNewsletter.cshtml
- F:\ECommerceMVC\ECommerceMVC\Views\Shared\_CustomerFooter.cshtml
- F:\ECommerceMVC\ECommerceMVC\wwwroot\css\modern-theme.css

---

## Success criteria

- UI feels clearly like furniture ecommerce rather than cyber/web3 demo UI.
- amado-master influence is visible in structure and merchandising posture, not copied literally.
- Dribbble-inspired polish is visible in spacing, cards, imagery framing, and premium warmth.
- High-traffic pages are visually consistent.
- Existing backend actions still work.
- Build completes with /p:UseAppHost=false.