#!/usr/bin/env python3
"""Extract AudioClip samples from a Unity asset directory with UnityPy."""
from __future__ import annotations

import hashlib
import json
import re
import sys
from pathlib import Path

import UnityPy

ROOT = Path("VisualAssets/ExportedProject/Assets")
DEST = Path("VisualAssets/OriginalAudio/UnityPyExtracted")


def safe_name(value: str) -> str:
    value = re.sub(r"[^A-Za-z0-9._ -]+", "_", value).strip(" .")
    return value[:160] or "AudioClip"


def extension(payload: bytes) -> str:
    if payload.startswith(b"RIFF") and payload[8:12] == b"WAVE":
        return ".wav"
    if payload.startswith(b"OggS"):
        return ".ogg"
    if payload.startswith(b"FSB5"):
        return ".fsb"
    if payload.startswith(b"ID3") or payload.startswith(b"\xff\xfb"):
        return ".mp3"
    return ".bin"


def main() -> int:
    if not ROOT.is_dir():
        print(f"ERROR: Unity asset source directory missing: {ROOT}", file=sys.stderr)
        return 2

    DEST.mkdir(parents=True, exist_ok=True)
    env = UnityPy.load(str(ROOT))
    exported = []
    audio_clip_objects = 0
    skipped_no_samples = 0
    seen_hashes: dict[str, str] = {}

    for obj in env.objects:
        if obj.type.name != "AudioClip":
            continue
        audio_clip_objects += 1
        try:
            clip = obj.read()
            samples = clip.samples
        except Exception as exc:
            exported.append({
                "path_id": int(obj.path_id),
                "clip_name": "",
                "status": "read_error",
                "error": str(exc),
            })
            continue

        if not samples:
            skipped_no_samples += 1
            exported.append({
                "path_id": int(obj.path_id),
                "clip_name": str(getattr(clip, "name", "")),
                "status": "no_samples",
            })
            continue

        for sample_name, payload in samples.items():
            if not payload:
                exported.append({
                    "path_id": int(obj.path_id),
                    "clip_name": str(sample_name),
                    "status": "empty",
                })
                continue

            raw = bytes(payload)
            digest = hashlib.sha256(raw).hexdigest()
            ext = extension(raw)
            base = safe_name(str(sample_name))
            filename = f"{base}__pathid_{int(obj.path_id)}{ext}"
            target = DEST / filename

            if digest in seen_hashes:
                exported.append({
                    "path_id": int(obj.path_id),
                    "clip_name": str(sample_name),
                    "status": "duplicate_payload",
                    "sha256": digest,
                    "canonical_file": seen_hashes[digest],
                })
                continue

            # Do not silently overwrite an existing artifact from a prior run.
            if target.exists():
                target = DEST / f"{base}__pathid_{int(obj.path_id)}__{digest[:10]}{ext}"
            target.write_bytes(raw)
            seen_hashes[digest] = target.name
            exported.append({
                "path_id": int(obj.path_id),
                "clip_name": str(sample_name),
                "status": "exported",
                "file": target.name,
                "bytes": len(raw),
                "sha256": digest,
                "format": ext.lstrip("."),
            })

    report = {
        "source": str(ROOT),
        "output": str(DEST),
        "audio_clip_objects": audio_clip_objects,
        "unique_payloads_written": len(seen_hashes),
        "clips_without_samples": skipped_no_samples,
        "records": exported,
    }
    (DEST / "UnityPy_audio_manifest.json").write_text(
        json.dumps(report, indent=2, ensure_ascii=False) + "\n",
        encoding="utf-8",
    )

    print(f"AudioClip objects found: {audio_clip_objects}")
    print(f"Unique audio payloads written: {len(seen_hashes)}")
    print(f"AudioClip objects without samples: {skipped_no_samples}")
    print(f"Manifest: {DEST / 'UnityPy_audio_manifest.json'}")

    if audio_clip_objects == 0:
        print("ERROR: no AudioClip objects found in input.", file=sys.stderr)
        return 3
    if len(seen_hashes) == 0:
        print("ERROR: AudioClip objects were found, but no samples were exported.", file=sys.stderr)
        return 4
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
