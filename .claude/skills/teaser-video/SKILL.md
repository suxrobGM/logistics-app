---
name: teaser-video
description: Film, cut, and deliver a short teaser/demo video of any app by driving it with Playwright and editing with ffmpeg - storyboard, authenticated capture, PII review, animated title cards, MP4 + GIF. Use for "make a teaser", "record a demo video", "product launch video", "screen recording for the README".
user-invocable: true
metadata:
  version: 1.0.0
  updated: 2026-08-01
  portable: true
  requires:
    - node >=24
    - npm packages (installed into a scratch dir, not the project): playwright, ffmpeg-static, pngjs
    - a Chromium/Chrome channel available to Playwright
  files:
    - SKILL.md
    - reference/package.json
    - reference/login.js
    - reference/scout.js
    - reference/rig.js
    - reference/build.js
    - reference/cards.js
    - reference/jitter.js
---

# Teaser video

Produce a launch-quality teaser (~30-45s) of a running app: storyboard it, film
it by driving the real UI, cut it with ffmpeg, and hand back an MP4 plus a GIF
for the README.

The output must look like a product video, not a screen recording. The
difference is direction - staging, framing, and rhythm - not resolution.

Reference implementations live in `reference/`; they are working ESM modules,
not pseudocode. Set up in a scratch directory - never inside the project:

```bash
cp -r <skill>/reference/* "$SCRATCH/" && cd "$SCRATCH" && npm install
export TEASER_BASE_URL="https://the-app.example.com"
```

`reference/package.json` declares `"type": "module"` and the three
dependencies. Every script reads `TEASER_BASE_URL`, so nothing is hardcoded to
one app.

## Before anything else: the two hard rules

**1. Never trigger real-world side effects without explicit, itemized consent.**

Filming a real app means clicking real buttons. Some of those send email, post
publicly, charge money, or submit forms to third parties. Before you touch the
app:

- Enumerate every action in your storyboard that leaves the machine.
- Tell the user exactly what each one does and how many times, then get
  agreement on the count.
- Prefer a mode that stops short of the irreversible step (a preview, a
  dry-run, a confirmation gate you can film and then decline).

There is no such thing as a read-only probe against a live system. A
"diagnostic" click that lands in an already-running session fires for real. If
you are debugging why an action did not work, assume your next click _will_
work, and be ready for it.

If something fires that you did not intend: stop the process at its source
immediately (kill the session/worker, not just the UI button, which may not
take effect), verify the stopped state by reading it back, and tell the user
plainly in your final message - count included.

**2. Assume every frame is public and full of PII.**

Real accounts carry names, emails, addresses, and documents. Decide the policy
with the user up front:

- **Frame around it** (default) - compose shots that exclude identity. Crop
  persistent chrome (sidebars, avatars, account menus) at the ffmpeg stage.
- **Hide before rolling** - `page.evaluate` a `visibility: hidden` on offending
  cards _before_ the camera rolls. Never film it and hope to cut around it.
- **Use a seeded demo account** - cleanest when one exists.

Review every frame of the final cut at full resolution before delivering. Not
the contact sheet - the frames.

## Workflow

### 1. Scout and storyboard

Take authenticated screenshots of every candidate screen first (`reference/scout.js`).
Look at them. Then write a storyboard table: beat, shot, what the viewer should
_feel_, and length. A shot with no assigned job gets cut before it is filmed.

Arc that works: tension (the problem) → reveal (the product) → proof (it really
does the thing) → scale (the results) → trust (where it runs) → close.

Note in the storyboard which screens hold PII and which are safe.

### 2. Stage the set

Curate before rolling, never after:

- Pick views where the data looks its best - full charts, active lists,
  recognizable names, non-zero counts. Never film a spinner, an empty state, or
  a half-rendered chart.
- Set UI state deliberately (panel widths, collapsed/expanded, filters, scroll
  position) via `localStorage` in an init script or a pre-roll `page.evaluate`.
- Dismiss toasts, banners, and cookie bars off-camera.
- Let data settle: `networkidle` plus a few seconds.

### 3. Film

Use **CDP screencast** (`Page.startScreencast`), not screen recording. It
captures the page off-screen at ~100fps, so nothing the user is doing appears in
frame and the machine stays usable. Playwright drives; CDP films.
`reference/rig.js` implements it.

- One subject per shot. Frame the thing, not "the whole app".
- Camera moves come from **scripted eased scrolling inside the page**
  (`cameraScroll`), never from a filter. See the zoompan warning below.
- The cursor is an actor: use the injected SVG cursor (`cursorClick`) that
  glides with easing and dips on press, or hide it. No idle drift.
- Rehearse each take, extract frames, look at them, adjust, then film for real.

**Filming an external window** (a browser the app itself opens, a desktop app)
needs OS capture - `gdigrab`/`x11grab`/`avfoundation` (`recordDesktop` in the
rig). This captures _everything on screen_, including the user's own work. Warn
the user, ask them to step away, and review the result frame by frame; delete it
if it caught anything private.

### 4. Title cards

Animated HTML/CSS filmed in-browser, not static images. Inject the card markup
into a blank page **on the app's own origin** so its web fonts and palette
resolve, then screencast it. Pull the real display font off the live page
(`getComputedStyle`) and pin a known-good mono stack rather than sniffing one.

Match the app's motion language (its own easing curves and durations). Load the
`frontend-design` skill before designing them if the project has no established
card style.

### 5. Cut

`reference/build.js` normalizes each segment to a common size/fps, then
crossfades the chain. Keep cuts every ~2.5-4s. Give the payoff moment room at
normal speed while compressing the routine around it.

### 6. Review, then deliver

- Generate a contact sheet **and** per-second full-resolution frames.
- Read the frames for PII, clipped panels, and unreadable text.
- Verify duration and dimensions with a probe.
- Deliver an MP4 plus, for READMEs, a palette-optimized GIF (GitHub will not
  inline-play a committed MP4). Keep the GIF under ~10 MB.
- Leave it silent unless asked; you cannot license music. Say so.

## Hard-won specifics

**Never use `zoompan` for push-ins.** It recomputes zoom per frame and rounds
the crop window to whole pixels, so the image snaps back and forth - it reads as
an earthquake. Use locked-off shots plus real in-page scrolling. If you must
have a push-in, pre-scale the source 4x first, and verify with a frame-delta
measurement (`reference/jitter.js`): a locked-off static shot should measure
well under 1.0 mean delta; visible shake reads 3+.

**Screencast stops emitting frames when animation settles.** A card whose
animation ends after 1.2s yields a 1.2s clip. Pad with
`tpad=stop_mode=clone:stop_duration=N` and cut to the length you want with `-t`.

**Assemble timestamped frames, not a fixed rate.** Screencast frames arrive with
`metadata.timestamp`; write an ffmpeg concat list with real per-frame durations,
then resample to constant 60fps. Assuming a fixed interval produces judder.

**`ffmpeg-static` as a dev dependency**, resolved by module path. Never depend on
ffmpeg being on PATH. (`ffprobe` is _not_ in that package - parse `ffmpeg -i`
stderr instead.)

**Authenticated capture:** log in once headed, save `storageState`, reuse it
across every take (`reference/login.js`). Sessions expire - finish filming the
same day or re-run it. Also seed any `localStorage` UI preferences there.

**Localhost from a deployed origin is blocked** by Chrome's local-network access
policy. If the page must reach a local service, launch with
`--disable-features=LocalNetworkAccessChecks,PrivateNetworkAccessChecks,BlockInsecurePrivateNetworkRequests`.

**Crop persistent chrome at a fixed offset.** Film at viewport width
`1920 + railWidth`, then `crop=1920:1080:railWidth:0` to drop the sidebar and
land exactly on 1080p.

## Deliverables

Report the path, duration, and dimensions; state plainly what is still needed
from the user (music, a GitHub-native video upload); and disclose anything that
happened during filming that they would want to know.
