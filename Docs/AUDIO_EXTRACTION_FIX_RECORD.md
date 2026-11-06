# Original Unity Audio Extraction — Fix Record

## Reviewed commit
- Commit: `7b9c5ea6543e8c05d411ce0815b374c15342604e`
- Branch: `work/stranded-deep-original`
- Date (UTC): 2026-11-06
- Commit message: `Fix audio extraction verification for duplicate quarantine`
- Workflow file changed: `.github/workflows/copy-original-audio.yml`

## Change verified
The source-vs-destination clip verification now uses:

```bash
diff -qr --exclude=Duplicates "$CLIPS" "$DEST/UnityAudioClips"
```

This excludes the workflow-created `Duplicates` quarantine directory from the initial source copy comparison. The mixer comparison line is unchanged.

## Validation state
- Commit metadata and diff were inspected directly.
- Combined commit status response currently contains no status checks (`statuses: []`).
- Therefore this commit's workflow execution and actual extracted audio contents are **not yet verified as successful**.
- Next verification should inspect the Actions run associated with this commit, confirm the job passes, and then inspect the resulting `VisualAssets/OriginalAudio` tree, duplicate quarantine, metadata, and SHA-256 manifest before marking extraction complete.
