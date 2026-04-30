#!/usr/bin/env python3
"""
Generate starter SVG templates for Get Bricked gameplay art.
"""

from __future__ import annotations

import argparse
from pathlib import Path


PALETTE = {
    "void": "#120914",
    "indigo": "#241B2F",
    "violet": "#2A2139",
    "cyan": "#03EDF9",
    "magenta": "#FF7EDB",
    "laser": "#FC28A8",
    "coral": "#F97E72",
    "mint": "#72F1B8",
    "gold": "#FEDE5D",
    "white": "#FDFDFD",
}


def build_ball() -> str:
    return f"""<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 128 128">
  <defs>
    <radialGradient id="core" cx="50%" cy="42%" r="58%">
      <stop offset="0%" stop-color="{PALETTE['white']}"/>
      <stop offset="48%" stop-color="{PALETTE['gold']}"/>
      <stop offset="100%" stop-color="{PALETTE['coral']}"/>
    </radialGradient>
  </defs>
  <circle cx="64" cy="64" r="54" fill="url(#core)"/>
  <circle cx="64" cy="64" r="60" fill="none" stroke="{PALETTE['white']}" stroke-opacity="0.45" stroke-width="4"/>
  <path d="M42 40c7-9 19-15 31-15" fill="none" stroke="{PALETTE['white']}" stroke-width="6" stroke-linecap="round" opacity="0.9"/>
</svg>
"""


def build_brick() -> str:
    return f"""<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 256 96">
  <defs>
    <linearGradient id="face" x1="0%" y1="0%" x2="100%" y2="100%">
      <stop offset="0%" stop-color="{PALETTE['magenta']}"/>
      <stop offset="100%" stop-color="{PALETTE['laser']}"/>
    </linearGradient>
  </defs>
  <rect x="8" y="8" width="240" height="80" rx="12" fill="{PALETTE['violet']}"/>
  <rect x="14" y="14" width="228" height="68" rx="10" fill="url(#face)"/>
  <path d="M26 30h204" stroke="{PALETTE['white']}" stroke-opacity="0.28" stroke-width="4" stroke-linecap="round"/>
  <path d="M34 58h68M104 58h50M162 58h60" stroke="{PALETTE['white']}" stroke-opacity="0.2" stroke-width="6" stroke-linecap="round"/>
</svg>
"""


def build_paddle() -> str:
    return f"""<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 320 72">
  <defs>
    <linearGradient id="bar" x1="0%" y1="50%" x2="100%" y2="50%">
      <stop offset="0%" stop-color="{PALETTE['cyan']}"/>
      <stop offset="50%" stop-color="{PALETTE['white']}"/>
      <stop offset="100%" stop-color="{PALETTE['cyan']}"/>
    </linearGradient>
  </defs>
  <rect x="8" y="14" width="304" height="44" rx="18" fill="{PALETTE['indigo']}"/>
  <rect x="18" y="22" width="284" height="28" rx="14" fill="url(#bar)"/>
  <path d="M40 36h240" stroke="{PALETTE['white']}" stroke-opacity="0.4" stroke-width="4" stroke-linecap="round"/>
</svg>
"""


def build_powerup() -> str:
    return f"""<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 128 128">
  <defs>
    <linearGradient id="diamond" x1="0%" y1="0%" x2="100%" y2="100%">
      <stop offset="0%" stop-color="{PALETTE['mint']}"/>
      <stop offset="100%" stop-color="{PALETTE['cyan']}"/>
    </linearGradient>
  </defs>
  <path d="M64 10 114 64 64 118 14 64Z" fill="{PALETTE['violet']}"/>
  <path d="M64 20 104 64 64 108 24 64Z" fill="url(#diamond)"/>
  <path d="M64 34 84 64 64 94 44 64Z" fill="{PALETTE['white']}" opacity="0.85"/>
</svg>
"""


BUILDERS = {
    "ball": build_ball,
    "brick": build_brick,
    "paddle": build_paddle,
    "powerup": build_powerup,
}


def main() -> int:
    parser = argparse.ArgumentParser(description="Generate starter SVG templates for Get Bricked.")
    parser.add_argument("--kind", choices=sorted(BUILDERS.keys()), required=True)
    parser.add_argument("--out", required=True, help="Output SVG path")
    args = parser.parse_args()

    output_path = Path(args.out).resolve()
    output_path.parent.mkdir(parents=True, exist_ok=True)
    output_path.write_text(BUILDERS[args.kind](), encoding="utf-8")
    print(f"Wrote {args.kind} template to {output_path}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
