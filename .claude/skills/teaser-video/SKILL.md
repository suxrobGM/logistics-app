---
name: teaser-video
description: Produce a launch-quality product teaser (~45s) with motion graphics, a synthesized score and sound design - real app UI captured with Playwright, composed in Remotion as 3D layers, kinetic type and camera moves cut to a tempo grid. Delivers MP4 + README GIF. Use for "make a teaser", "product video", "demo video for the website", "README gif", "refilm the teaser".
---

# Teaser video

A teaser is motion design that happens to contain the product, not a screen recording with
title cards between clips. The old pipeline (full-screen takes, crossfades, silence) produced
exactly that flat look. This one composes the real UI as objects on a dark stage, moves a camera
through them, cuts on the beat of a score generated for the edit, and lands sound effects on the
same frames.

```
capture (Playwright)          compose (Remotion)              score (Node DSP)
  2x stills of screens   ->     scenes on a 120 BPM grid  <-    music.wav + sfx/*.wav
  1x CDP footage of flows       crops, 3D, camera, type          on the same grid
                                        |
                                 render -> review -> deliver (MP4 + GIF)
```

The working LogisticsX teaser is the reference implementation:

| Path                                     | What it is                                                              |
| ---------------------------------------- | ----------------------------------------------------------------------- |
| `reference/capture/session.js`           | Signed-in page via a persistent profile; in-app navigation              |
| `reference/capture/rig.js`               | CDP screencast, constant-rate assembly, injected cursor                 |
| `reference/capture/stills.js`            | 2x screenshots into `compose/public/stills/`                            |
| `reference/capture/take-ai-dispatch.js`  | The one filmed flow, with an event log for the edit and the SFX         |
| `reference/compose/src/`                 | The Remotion project: timeline, theme, components, one file per scene   |
| `reference/compose/audio/`               | `dsp.mjs` (oscillators, filters, reverb, WAV), `score.mjs`, `sfx.mjs`   |
| `reference/compose/preview.mjs`          | Bundle once, render chosen frames as stills - the fast review loop      |
| `reference/compose/deliver.sh`           | Web MP4, README GIF, 2fps review frames                                 |

Work in the scratchpad, never in the repo. `compose/public/` is not committed; rebuild it:

```bash
cp -r <skill>/reference/{capture,compose} "$SCRATCH/" && cd "$SCRATCH"
(cd capture && npm install) && (cd compose && npm install)
export MSYS_NO_PATHCONV=1 && cd capture
node stills.js home=/home loads=/loads eld=/eld invoices=/invoices payroll=/payroll maintenance=/maintenance \
  loadboard=/loadboard safety=/safety financials=/reports/financials containers=/containers expenses=/expenses messages=/messages
TEASER_USER=customer1@test.com TEASER_BASE=https://customer.logisticsx.app TEASER_ROOT_IS_HOME=1 node stills.js customer=/shipments
node take-ai-dispatch.js && mkdir -p ../compose/public/footage && cp takes/ai-run.mp4 ../compose/public/footage/ai.mp4
node stills.js ai-final=/ai-dispatch                       # the finished session, for the close-ups
cd ../compose
mkdir -p public/phones public/tiles && cp <repo>/docs/store-assets/screenshots/raw/*.png public/phones/
for n in loads loadboard invoices containers expenses home payroll eld messages maintenance safety customer; do
  ffmpeg -y -i public/stills/$n.png -vf scale=1600:900 -q:v 3 public/tiles/$n.jpg; done
node audio/score.mjs && node audio/sfx.mjs
```

A new take changes the timings: update `SEGMENTS`, `CAMERA` and `PILLS` in
`scenes/AIDispatch.tsx` from `takes/ai-run.json`, and re-measure the crop rects on `ai-final.png`.
The finale crop shows whatever state the decision is in when `ai-final` is captured.

## The two hard rules

**1. No real-world side effects without itemized consent.** Filming clicks real buttons. List
every action in the storyboard that writes data or leaves the machine (email, push, payments,
third-party calls), say what each does and how many times, and get agreement. Prefer filming up to
the irreversible step and cutting on the click. If something fires unintentionally, stop it at
the source, verify, and report it with a count.

For LogisticsX specifically: approving an AI dispatch `assign_load_to_truck` decision only writes
`Load.AssignedTruckId`; `create_trip` pushes to the driver and Telegram; `propose_counter_offer`
emails a real broker. Never confirm the last two on camera.

**2. Every frame is public.** Use seeded demo accounts, frame around anything personal, and
review the final cut frame by frame before delivering.

## 1. Storyboard to a tempo

Pick the tempo first and make every duration a multiple of it. 120 BPM at 60fps gives a 30-frame
beat and a 120-frame bar, so scenes start on bars, hits land on beats, and the score can be
written against the same numbers. `src/timeline.ts` and `audio/score.mjs` share those constants.

Arc that works (bars): cold open with kinetic type (2) - the problem as UI fragments piling up (2)
- silence, then the brand reveal on the downbeat (2) - the hero feature, longest (6) - three or
four short proof scenes (1-2 each) - a fast montage, one screen per beat (2) - close and URL (4).

Write down, per scene: what the viewer should feel, which captured asset carries it, and the
one sentence on screen. A scene with no job gets cut before anything is filmed.

## 2. Capture

**Stills (2x)** carry most of the video. `deviceScaleFactor: 2` and `page.screenshot` give
3200x1800 PNGs of a 1600x900 page, sharp up to 2x zoom. The `Crop` component takes a rect in the
page's CSS pixels, so measure regions on a 1600x900 downscale with a 100px grid
(`drawgrid=w=100:h=100`).

**Footage (1x)** only for flows that must visibly happen: typing, an agent working, a click. CDP
`Page.startScreencast` records in CSS pixels whatever the device scale factor, so takes are
1600x900. Keep the camera at or below ~1.3x on footage and use a 2x still of the end state for
close-ups. `zoom: 2` on the document does give 2x frames but breaks `vh` layout and Mapbox; do not
use it. Log event times during the take (`mark()` in the take script) - keystrokes, clicks,
arrivals - they drive the edit and the SFX.

Curate before rolling: dismiss cookie banners, avoid tiles showing zeros or broken numbers, and
clean junk test records in the demo tenant through the UI (with consent).

**Auth gotchas (LogisticsX):** the SPA keeps tokens in `sessionStorage`, so navigate in-app with
`history.pushState` + `popstate` instead of `page.goto`. The identity server rate-limits the login
page to 10 attempts per 15 minutes per IP; use a persistent Playwright profile
(`launchPersistentContext`) so later scripts bounce straight through "Sign in". The customer
portal lands on `/`, so treat the root as signed-in there. Under Git Bash set
`MSYS_NO_PATHCONV=1` or route arguments like `/loads` become Windows paths.

**The AI dispatch take:** the agent only sees Draft loads that are not in a trip, so check the
tenant has some ("0 unassigned loads" makes a dull take). Keep the request to one load: every
HttpClient gets the standard resilience handler (10s per attempt, 30s total), LLM client
included, so a whole-fleet request times out with "didn't complete within the allowed timeout
of '00:00:30'".

## 3. Compose in Remotion

Load the `remotion-best-practices` skill first. The look:

- **A flat ink stage** with a faint drifting dot grid, plus one global lens layer (vignette and
  animated grain). No gradients, glows or coloured icon circles; colour only for state (green
  approved, red violation, amber warning). The brand mark is the one gradient on screen.
- **The UI as objects.** Crops of real screens float in a `perspective` container with
  `preserve-3d`, enter on springs from depth, dim as newer ones arrive, and collapse to a point
  before a cut. Browser chrome (`Window`) frames footage; phones get a bezel component.
- **A camera, not zooms.** Keyframes of `{frame, scale, focus x/y}` in page coordinates,
  interpolated with an ease-in-out, drive a transform on the window. Never animate the footage
  itself with ffmpeg `zoompan` (it rounds to whole pixels and shakes).
- **Kinetic type** in the app's own fonts (`@remotion/google-fonts`): lines rise out of masks,
  words stagger by 3-5 frames, numbers count up with tabular figures, mono labels type themselves.
  Over busy UI, dim the layers behind the text instead of adding a glow.
- **Speed ramps** as `OffthreadVideo` segments, each with its own `trimBefore` and
  `playbackRate`. Map logged take times through the same segment table to place SFX.
- **Custom graphics where the product has none on screen**, e.g. the tilted US route map built
  from `us-atlas` + `d3-geo` with the demo tenant's real lanes.

Iterate with `node preview.mjs <frames...>` and a contact sheet; it is far faster than rendering
video. Many 3200px PNGs in one frame make headless Chrome fail to decode
("source image cannot be decoded"); use downscaled JPEG tiles wherever an image is drawn small.

## 4. Score and sound design

Music cannot be licensed by an agent, so synthesize it. `audio/score.mjs` writes an arrangement
per section (filtered pad and pulse, riser and snare roll into a silence, impact on the reveal,
four-on-the-floor groove with sidechained bass and arp, a hotter montage, a final hit with a long
tail). `audio/sfx.mjs` renders whooshes, swipes, clicks, keystrokes, pops, a chime, impacts and
ticks. Cues live in `Teaser.tsx` as `{at, sfx, volume}` on the frame grid.

You cannot listen, so measure: `showspectrumpic` for balance and arrangement,
`ebur128` for loudness (aim around -12 LUFS integrated, -1 dBFS peak), `showwavespic` for
dynamics. Keep effects 6-10 dB under the music unless they are the hit.

## 5. Render, review, deliver

```bash
npx remotion render Teaser out/render.mp4 --codec=h264 --crf=16 --audio-codec=aac --audio-bitrate=320k
npx remotion render Teaser out/gif-src.mp4 --frames=720-1439 --props='{"gif":true}' --codec=h264 --crf=14 --muted
./deliver.sh out/render.mp4 out/gif-src.mp4 out/final
```

The full render takes about a minute. Read the 2fps review frames for PII, clipped panels,
unreadable text and broken data, then spot-check a few at full resolution. Probe duration and
size.

The README GIF is the AI dispatch scene rendered with `gif: true`, which drops the film grain and
the drifting grid: per-frame noise doubles the GIF size. At 800px, 15fps and 128 colours it is
about 7.5 MB; keep it under ~10 MB. GitHub will not inline a committed MP4, so the website gets
the MP4 and the README gets the GIF.

In LogisticsX the outputs replace `docs/images/teaser.gif` and, in the private submodule,
`src/Client/Logistics.Angular/projects/website/public/demo-video.mp4`.

Report paths, duration and dimensions, and anything that happened while filming that the user
should know (writes to the demo tenant, production errors seen on camera).
